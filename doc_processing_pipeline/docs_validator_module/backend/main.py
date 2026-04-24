from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr
from typing import List, Optional
from datetime import datetime
from email_service import EmailService
import httpx
import json
import uuid
import sqlite3
import os

app = FastAPI(title="Invoice Validator API", version="2.0.0")

# Configurar CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==================== Config N8N====================
EXTRACT_API_URL = "http://192.168.10.207:8082/extract-markdown"
N8N_WEBHOOK_URL = "http://192.168.10.207:5678/webhook/invoice-validator"
DB_PATH = "validations.db"

# ==================== PROMPTS ====================

SYSTEM_PROMPT = """
You are an expert tax auditor specializing in the validation of logistics documents against official SAT (Tax Administration System) documents from Mexico.

**TYPES OF DOCUMENTS YOU MAY RECEIVE:**
1.  **LOGISTICS SIDE**: PDF Invoices, Bills of Materials (Excel/CSV), Purchase Orders, Packing Lists.
2.  **SAT SIDE**: CFDIs (XML), SAT PDF Invoices, Foreign Trade Complements (Complementos de Comercio Exterior).

**FUNDAMENTAL RULE:**
The SAT document is ALWAYS the SOURCE OF TRUTH. Your job is to verify if the logistics data matches the SAT data.

═══════════════════════════════════════════════════════════
**MODE 1: TRADITIONAL INVOICE VALIDATION**
═══════════════════════════════════════════════════════════
When both documents are invoices with standard fields:

**MANDATORY FIELDS:**
✓ Invoice Number / Folio
✓ Issue Date
✓ Issuer RFC / Provider RFC
✓ Corporate Name (Razón Social)
✓ Subtotal, VAT (IVA), Total
✓ Currency

**CRITERIA:** EXACT match in text fields. Minimal differences = "danger".

═══════════════════════════════════════════════════════════
**MODE 2: BILL OF MATERIALS (BOM) VALIDATION**
═══════════════════════════════════════════════════════════
When the logistics document contains material lists (Excel/CSV/Table) with multiple line items:

**COMPARISON STRATEGY:**
1.  **Identify structure**: Detect columns such as Parts No., Description, Quantity, Unit Price, Amount, Order No.
2.  **Extract SAT line items**: From the CFDI, extract `<cfdi:Concepto>` with Quantity, UnitValue, and Amount.
3.  **DO NOT GROUP**: Treat every row/line as a separate record, even if they share the same part number.
4.  **Mathematical Comparison**:
    * Sum of total quantities (Logistics) vs. Sum of quantities (SAT) → goes in "fields".
    * Sum of total amounts (Logistics) vs. SAT Total → goes in "fields".
    * Number of lines (Logistics) vs. Number of concepts (SAT) → goes in "fields".
    * BUT in "details", every line must be individual, without grouping.
5.  **Match Tolerance**:
    * Differences ≤ 0.50 USD/MXN in totals → "success".
    * Differences > 0.50 USD/MXN → "danger".
    * Quantities: must match EXACTLY.

**KEY FIELDS IN BOMs:**
- Parts No. / Part Number
- Description
- Quantity / QTY
- Unit
- Unit Price
- Amount / Total
- Order No.
- C/No. (Container Number)

**IF NO DESCRIPTION IN SAT:**
- Do not try to force description matches.
- FOCUS on validating: total quantities, total amounts, number of line items.
- Report numerical discrepancies clearly.

═══════════════════════════════════════════════════════════
**MODE 3: HYBRID VALIDATION**
═══════════════════════════════════════════════════════════
When there is mixed data (standard invoice fields + material table):
1.  Validate header fields as per MODE 1.
2.  Validate line items/materials as per MODE 2.
3.  Ensure the sum of line items matches the invoice total.

═══════════════════════════════════════════════════════════
**OCR CONSIDERATIONS:**
═══════════════════════════════════════════════════════════
- OCR may have reading errors (e.g., "O" instead of "0", "l" instead of "1").
- Ignore obvious OCR errors if the context is clear.
- Be flexible with line breaks and spacing, but NOT with numerical values.
- If a field is unreadable in a document, mark it as "N/A (OCR unreadable)".

═══════════════════════════════════════════════════════════
**MANDATORY FIELDS TO VALIDATE (TRADITIONAL INVOICES):**
═══════════════════════════════════════════════════════════
- Invoice Number / Folio / Series
- Issue Date
- Issuer RFC
- Issuer Name / Corporate Name
- Receiver RFC (if applicable)
- Receiver Name (if applicable)
- Subtotal
- VAT / Taxes
- Total
- Currency
- Payment Form (Forma de Pago)
- Payment Method (Método de Pago)
- CFDI Usage (Uso CFDI)

**OPTIONAL FIELDS:**
- Place of Issue
- Tax Address
- Tax Regime
- Payment Terms
- Purchase Order Number
- UUID (Fiscal Folio)

═══════════════════════════════════════════════════════════
**RESPONSE FORMAT (MANDATORY - DO NOT MODIFY)**
═══════════════════════════════════════════════════════════
ALWAYS respond with a valid JSON in EXACTLY this format:

{
  "invoiceNumber": "string - SAT Folio/document number",
  "fields": [
    {
      "campo": "Field name",
      "logistica": "Value in logistics document",
      "sat": "Value in SAT document",
      "estado": "success | danger"
    }
  ],
  "details": [
    {
      "campo": "Description of the line item/material",
      "logistica": "Logistics value",
      "sat": "SAT value",
      "estado": "success | danger"
    }
  ]
}

**INSTRUCTIONS FOR "fields":**
- **For TRADITIONAL INVOICES**: Include Invoice Number, Date, RFC, Name, Subtotal, VAT, Total.
- **For BILL OF MATERIALS**: Include summary fields such as:
    * "Total Line Items" → logistics: "23", sat: "23"
    * "Total Quantity" → logistics: "1,412 PCS", sat: "1,412 units"
    * "Total Amount" → logistics: "$ 34,067.32 USD", sat: "$ 612,134.18 MXN"
    * "Order Number" (if exists)
    * "Currency" → logistics: "USD", sat: "MXN"

- **For MULTIPLE DOCUMENTS (any M:N combination):**
    * STEP 1 - DOCUMENT INVENTORY:
        * "═══ DOCUMENTS RECEIVED ═══"
    * STEP 2 - INDIVIDUAL LOGISTICS VALIDATIONS:
        * "═══ INVOICE A-001 ═══"
    * STEP 3 - INDIVIDUAL SAT VALIDATIONS (if multiple):
        * "═══ CFDI-1 ═══"
    * STEP 4 - FINAL CONSOLIDATED SUMMARY:
        * "═══ FINAL CONSOLIDATED SUMMARY ═══"

**INSTRUCTIONS FOR "details":**
- **For TRADITIONAL INVOICES**: Itemize every concept from the CFDI.
- **For BILL OF MATERIALS**: Include every material line compared:
    * Field: "Item 1: UCM-P--J6166"
    * Logistics: "1,386 PCS @ $24.13 USD = $33,440.02"
    * SAT: "1,386 units, Amount: $600,856.04 MXN"
    * Status: "success" (if quantities match, even if descriptions differ).

- **For MULTIPLE DOCUMENTS (M:N):** Organize hierarchically and clearly.

 **CRITICAL - DO NOT GROUP LINE ITEMS:**
- NEVER group items with the same part number or description.
- Even if there are several lines with "UCM-P--J6166" in different invoices, each must be a separate record.
- DO NOT sum quantities of items with the same part number.
- Each row of each document is ONE independent line item in "details".
- Clearly identify which invoice AND which CFDI each line item belongs to.

**CRITICAL VALIDATION CRITERIA:**
✓ If a field does not exist, use "N/A".
✓ For BOMs: IF NUMBERS MATCH (quantities, totals), mark "success" EVEN IF descriptions differ.
✓ Tolerance of ±0.50 in amounts due to rounding.
✓ If there is a currency difference (USD vs MXN), mark it in the "Currency" field with "danger" but allow "success" in totals if the conversion is correct.
✓ Ignore format differences in numbers: "15,950.00" = "15950.00".
✓ DO NOT ignore decimal differences: "15,950.00" ≠ "15,950.50".
✓ DO NOT ignore text differences: "S.A." ≠ "S.A. de C.V.".
✓ Dates: accept different formats if they represent the same day.
✓ DO NOT invent data.

Respond ONLY with the JSON, without additional text before or after."""


def create_user_prompt(logistic_section: str, sat_section: str) -> str:
    return f"""
Analyze and compare these documents:

═══════════════════════════════════════════════════════════
LOGISTICS DOCUMENTS (To be validated)
═══════════════════════════════════════════════════════════
{logistic_section}

═══════════════════════════════════════════════════════════
SAT DOCUMENTS (Source of Truth)
═══════════════════════════════════════════════════════════
{sat_section}

═══════════════════════════════════════════════════════════
ANALYSIS INSTRUCTIONS
═══════════════════════════════════════════════════════════

1. **DETECT DOCUMENT TYPE AND PAIRING**:
   - How many documents are on each side?
   - Is it a traditional invoice or a Bill of Materials (BOM)?
   - Is it a Consolidation case? (Multiple logistics docs → 1 SAT doc)
   - Is it a Split case? (1 logistics doc → Multiple SAT docs)
   - Do documents match 1:1?

   **PAIRING EXAMPLES:**

   **Case A: Consolidation (5 Logistics → 1 SAT)**
   - Identify each individual logistics invoice (A, B, C, D, E).
   - For EACH individual invoice:
     * Extract order number, total quantity, total amount.
     * Compare against the corresponding data in the SAT document.
     * Add to "fields" with prefix "Invoice A:", "Invoice B:", etc.
   - AFTER individual analysis:
     * Sum the totals of the 5 logistics invoices.
     * Compare the sum against the total of the SAT document.
     * Add to "fields" with prefix "CONSOLIDATED SUMMARY:".
   - In "details", list ALL line items identified by invoice.
   - In "invoiceNumber", use the folio of the main SAT document.

   **Case B: Split (1 Logistics → 3 SATs)**
   - Sum the totals of the 3 SAT documents.
   - Compare against the total of the logistics invoice.
   - In "fields", report: "Total Logistics: $50,000 USD" vs "Total SAT (3 CFDIs): $900,000 MXN".

   **Case C: One-to-One (1 Logistics → 1 SAT)**
   - Direct field-by-field comparison.

2. **IF IT IS A BILL OF MATERIALS (BOM)**:
   - Identify EACH individual logistics file/invoice.
   - For EACH logistics file:
     * Extract ALL rows as INDIVIDUAL records.
     * NEVER group items with the same part number.
     * Each row = 1 record in "details" with an invoice identifier.
     * Calculate individual totals for that file (quantity, amount).
     * Add these totals in "fields" with the filename prefix.
   
   - AFTER processing each file:
     * Sum total quantities of ALL files → "CONSOLIDATED Total Quantity".
     * Sum total amounts of ALL files → "CONSOLIDATED Total Amount".
     * Count the total number of line items from ALL files.
     * Add these consolidated totals in "fields" with the prefix "CONSOLIDATED:".

   - Compare:
     * Individual totals of each invoice against parts of the SAT document.
     * Consolidated totals against the SAT document total.
   
   - Do not worry if descriptions do not match word-for-word.
   - FOCUS on numbers: quantities, prices, totals.

3. **IF IT IS A TRADITIONAL INVOICE**:
   - Compare field by field as usual.
   - Validate RFC, Corporate Name (Razón Social), date, and totals.

4. **EXTRACT FROM SAT DOCUMENT**:
   - If XML: Look for `<cfdi:Comprobante>`, `<cfdi:Conceptos>`, `<cfdi:Impuestos>`.
   - If PDF: Extract folio, date, RFC, totals, and line items.
   - Count how many concepts/line items it has.

5. **GENERATE THE RESPONSE JSON**:
   - JSON STRUCTURE FOR ANY SCENARIO (1:1, M:1, 1:N, M:N):
   
   **In "invoiceNumber":**
     - If 1 SAT exists: Use its folio.
     - If multiple SATs exist: Use "MULTIPLE: CFDI-1, CFDI-2, CFDI-3".

   **In "fields":**
     1. INVENTORY: List all documents received.
     2. INDIVIDUAL VALIDATIONS: Each logistics invoice with its totals.
     3. SAT VALIDATIONS (if multiple): Each CFDI with its totals.
     4. FINAL CONSOLIDATED SUMMARY: Sum of all vs. sum of all.

   **In "details":**
     1. Group by source document (Invoice A, Invoice B, etc.).
     2. Use visual separators "═══ DETAILS INVOICE X ═══".
     3. List ALL line items of each document individually.
     4. Indicate in the "sat" value which CFDI it corresponds to (if multiple).
     5. NO grouping of items with the same part number.
   
   - If there are 50 lines with the same part number across different invoices, "details" must have 50 records.
   - If descriptions do not match but numbers DO, mark "success".
   - Mark "danger" only when there are significant numerical differences (> 0.50).
   - Maintain the order: individual documents first, consolidated summary at the end.
   - Use visual separators "═══" so the frontend can identify sections.
   - In M:N scenarios, clearly indicate correspondences: "FAC-A → CFDI-1", "FAC-B + FAC-C → CFDI-2".

═══════════════════════════════════════════════════════════

RESPOND ONLY WITH THE JSON IN THIS FORMAT:

{
  "invoiceNumber": "string - SAT document folio",
  "fields": [
    {
      "campo": "string",
      "logistica": "string",
      "sat": "string",
      "estado": "success or danger"
    }
  ],
  "details": [
    {
      "campo": "string",
      "logistica": "string",
      "sat": "string",
      "estado": "success or danger"
    }
  ]
}

DO NOT add any text before or after the JSON."""

# ==================== Base de datos SQLite ====================

def init_database():
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS validations (
            id TEXT PRIMARY KEY,
            invoice_number TEXT NOT NULL,
            files TEXT NOT NULL,
            fields TEXT NOT NULL,
            details TEXT,
            timestamp TEXT NOT NULL,
            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    
    conn.commit()
    conn.close()

def save_validation_to_db(validation_id: str, invoice_number: str, files: dict, fields: list, details: Optional[list] = None):
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    try:
        cursor.execute('''
            INSERT INTO validations (id, invoice_number, files, fields, details, timestamp)
            VALUES (?, ?, ?, ?, ?, ?)
        ''', (
            validation_id,
            invoice_number,
            json.dumps(files),
            json.dumps([field.model_dump() for field in fields]),
            json.dumps([d.model_dump() for d in details]) if details else None,
            datetime.now().isoformat()
        ))
        
        conn.commit()
        print(f"Validación guardada en BD: {validation_id}")
        return True
    except Exception as e:
        print(f"Error guardando validación en BD: {e}")
        return False
    finally:
        conn.close()

def get_all_validations():
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    try:
        cursor.execute('SELECT id, invoice_number, files, fields, details, timestamp FROM validations ORDER BY created_at DESC')
        rows = cursor.fetchall()
        
        validations = []
        for row in rows:
            validation = {
                "validationId": row[0],
                "invoiceNumber": row[1],
                "files": json.loads(row[2]),
                "fields": json.loads(row[3]),
                "details": json.loads(row[4]) if row[4] else None,
                "timestamp": row[5]
            }
            validations.append(validation)
        
        return validations
    except Exception as e:
        print(f"Error obteniendo validaciones: {e}")
        return []
    finally:
        conn.close()

def delete_validation_from_db(validation_id: str) -> bool:
    conn = sqlite3.connect(DB_PATH)
    cursor = conn.cursor()
    
    try:
        cursor.execute('DELETE FROM validations WHERE id = ?', (validation_id,))
        conn.commit()
        
        if cursor.rowcount > 0:
            print(f"Validación eliminada de BD: {validation_id}")
            return True
        else:
            print(f"Validación no encontrada: {validation_id}")
            return False
    except Exception as e:
        print(f"Error eliminando validación: {e}")
        return False
    finally:
        conn.close()

# ==================== Modelos de datos ====================

class FieldComparison(BaseModel):
    campo: str
    logistica: str
    sat: str
    estado: str

class DetailComparison(BaseModel):
    campo: str
    logistica: str
    sat: str
    estado: str

class ValidationResult(BaseModel):
    validationId: str
    invoiceNumber: str
    files: dict
    fields: List[FieldComparison]
    details: Optional[List[DetailComparison]] = None
    timestamp: str

class SendObservationsRequest(BaseModel):
    email: EmailStr
    validationId: str
    invoiceNumber: str
    fields: List[FieldComparison]

class SendObservationsResponse(BaseModel):
    message: str
    emailSent: bool
    timestamp: str

# ==================== Funciones auxiliares ====================

async def extract_file_content(file: UploadFile, dpi: int = 150) -> str:
    try:
        async with httpx.AsyncClient(timeout=500.0) as client:
            # Leer contenido del archivo
            content = await file.read()
            
            # Crear form data
            files = {"file": (file.filename, content, file.content_type)}
            
            # Hacer petición POST a la API de extracción
            response = await client.post(
                EXTRACT_API_URL,
                files=files,
                params={"dpi": dpi}
            )
            
            if response.status_code != 200:
                raise HTTPException(
                    status_code=response.status_code,
                    detail=f"Error al extraer contenido del archivo {file.filename}: {response.text}"
                )
            
            return response.text
            
    except httpx.RequestError as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error de conexión con API de extracción: {str(e)}"
        )
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error al procesar archivo {file.filename}: {str(e)}"
        )

async def compare_documents_with_llm(
    logistic_contents: List[str],
    sat_contents: List[str],
    logistic_filenames: List[str],
    sat_filenames: List[str]
) -> dict:
    # Construir secciones del prompt
    logistic_section = "\n\n".join([
        f"ARCHIVO: {name}\n{'='*50}\n{content}"
        for name, content in zip(logistic_filenames, logistic_contents)
    ])
    
    sat_section = "\n\n".join([
        f"ARCHIVO: {name}\n{'='*50}\n{content}"
        for name, content in zip(sat_filenames, sat_contents)
    ])
    
    # Crear prompt de usuario usando la función mejorada
    user_prompt = create_user_prompt(logistic_section, sat_section)
    
    try:
        async with httpx.AsyncClient(timeout=300.0) as client:
            payload = {
                "system_prompt": SYSTEM_PROMPT,  # Usar el nuevo SYSTEM_PROMPT mejorado
                "user_message": user_prompt
            }
            
            response = await client.post(N8N_WEBHOOK_URL, json=payload)
            
            if response.status_code not in (200, 201):
                raise HTTPException(
                    status_code=response.status_code,
                    detail=f"Error en webhook n8n: {response.text}"
                )
            
            result = response.json()
            
            # Si la respuesta está envuelta en un objeto, extraer el resultado
            if isinstance(result, dict) and "result" in result:
                result_data = result["result"]
            else:
                result_data = result
            
            # Si es string, parsear como JSON
            if isinstance(result_data, str):
                result_data = json.loads(result_data)
            
            return result_data
            
    except httpx.RequestError as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error de conexión con webhook n8n: {str(e)}"
        )
    except json.JSONDecodeError as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error al parsear respuesta del LLM: {str(e)}"
        )
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error procesando comparación con LLM: {str(e)}"
        )

# ==================== Endpoints ====================

@app.on_event("startup")
async def startup_event():
    init_database()
    print("Base de datos inicializada")

@app.get("/")
async def root():
    return {
        "message": "Invoice Validator API",
        "version": "2.0.0",
        "status": "active",
        "endpoints": {
            "validate": "/validate-invoices",
            "send_observations": "/send-observations",
            "get_validations": "/validations",
            "delete_validation": "/validations/{validation_id}"
        }
    }

@app.post("/validate-invoices", response_model=ValidationResult)
async def validate_invoices(
    logisticFiles: List[UploadFile] = File(...),
    satFiles: List[UploadFile] = File(...)
):
    try:
        # Validar que se hayan enviado archivos
        if not logisticFiles or not satFiles:
            raise HTTPException(
                status_code=400,
                detail="Debe proporcionar archivos para ambas secciones (logística y SAT)"
            )
        
        # Extraer nombres de archivos
        logistic_filenames = [file.filename for file in logisticFiles]
        sat_filenames = [file.filename for file in satFiles]
        
        print(f"Archivos recibidos:")
        print(f"   Logística: {logistic_filenames}")
        print(f"   SAT: {sat_filenames}")
        
        # ==================== FASE 1: Extraer contenido de archivos ====================
        print(f"Extrayendo contenido de archivos...")
        
        logistic_contents = []
        for file in logisticFiles:
            print(f"   Procesando: {file.filename}")
            content = await extract_file_content(file)
            logistic_contents.append(content)
        
        sat_contents = []
        for file in satFiles:
            print(f"   Procesando: {file.filename}")
            content = await extract_file_content(file, 100)
            sat_contents.append(content)
        
        print(f"Contenido extraído de {len(logistic_contents)} archivos logísticos")
        print(f"Contenido extraído de {len(sat_contents)} archivos SAT")
    
        # ==================== FASE 2: Comparar con LLM ====================
        print(f"Enviando a LLM para comparación (con soporte para listas de materiales)...")
        
        llm_result = await compare_documents_with_llm(
            logistic_contents,
            sat_contents,
            logistic_filenames,
            sat_filenames
        )
        
        print(f"Comparación completada por LLM")
        
        # ==================== FASE 3: Construir response ====================
        
        # Validar estructura mínima de la respuesta del LLM
        if not isinstance(llm_result, dict):
            raise HTTPException(
                status_code=500,
                detail="Respuesta inválida del LLM: no es un diccionario"
            )
        
        invoice_number = llm_result.get("invoiceNumber", "UNKNOWN")
        fields_data = llm_result.get("fields", [])
        details_data = llm_result.get("details", [])
        
        # Convertir a modelos Pydantic
        fields = [
            FieldComparison(**field) for field in fields_data
        ] if fields_data else []
        
        details = [
            DetailComparison(**detail) for detail in details_data
        ] if details_data else []
        
        validation_id = str(uuid.uuid4())
        
        result = ValidationResult(
            validationId=validation_id,
            invoiceNumber=invoice_number,
            files={
                "logistic": logistic_filenames,
                "sat": sat_filenames
            },
            fields=fields,
            details=details if details else None,
            timestamp=datetime.now().isoformat()
        )
        
        # ==================== FASE 4: Guardar en base de datos ====================
        print(f"Guardando resultado en base de datos...")
        save_validation_to_db(
            validation_id,
            invoice_number,
            result.files,
            fields,
            details
        )
        
        print(f"Validación completada: {result.invoiceNumber} (ID: {validation_id})")
        
        return result
        
    except Exception as e:
        print(f"Error en validación: {str(e)}")
        raise HTTPException(
            status_code=500,
            detail=f"Error al procesar respuesta: {str(e)}"
        )

@app.get("/validations")
async def get_validations():
    print("Obteniendo historial de validaciones...")
    validations = get_all_validations()
    return {
        "success": True,
        "data": validations
    }

@app.delete("/validations/{validation_id}")
async def delete_validation(validation_id: str):
    if not validation_id:
        raise HTTPException(status_code=400, detail="ID de validación requerido")
    
    success = delete_validation_from_db(validation_id)
    
    if success:
        return {
            "success": True,
            "message": "Registro eliminado exitosamente"
        }
    else:
        raise HTTPException(
            status_code=404, 
            detail="Registro no encontrado"
        )

@app.post("/send-observations", response_model=SendObservationsResponse)
async def send_observations(request: SendObservationsRequest):

    print(f"Enviando observaciones:")
    print(f"   Email: {request.email}")
    print(f"   Validación ID: {request.validationId}")
    print(f"   Factura: {request.invoiceNumber}")
    print(f"   Campos: {len(request.fields)} campos")
    
    # Validar que haya campos
    if not request.fields:
        raise HTTPException(
            status_code=400,
            detail="Debe proporcionar al menos un campo para enviar observaciones"
        )
    
    if request.email.endswith("@mitsumi.mx") == False:
        raise HTTPException(
            status_code=400,
            detail="Debe pertenecer a un dominio '@mitsumi.mx'"
        )
    
    # Aquí iría la lógica real de envío de correo (SendGrid, AWS SES, SMTP, etc.)
    email_service = EmailService()
    email_sent = await email_service.send_email_smtp(
        to_email=request.email, 
        invoice_number=request.invoiceNumber, 
        fields=request.fields, 
        validation_id=request.validationId
    )
    
    # Contar campos con errores
    errors_count = sum(1 for field in request.fields if field.estado == "danger")
    success_count = sum(1 for field in request.fields if field.estado == "success")
    
    message = (
        f"Observaciones enviadas exitosamente a {request.email}. "
        f"Resumen: {success_count} campos correctos, {errors_count} discrepancias encontradas."
    )
    
    print(f"{message}")
    
    return SendObservationsResponse(
        message=message,
        emailSent=email_sent,
        timestamp=datetime.now().isoformat()
    )

@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "timestamp": datetime.now().isoformat(),
        "database": "connected"
    }

# ==================== Manejo de errores ====================

from fastapi.responses import JSONResponse

@app.exception_handler(HTTPException)
async def http_exception_handler(request, exc):
    return JSONResponse(
        status_code=exc.status_code,
        content={
            "success": False,
            "error": exc.detail,
            "status_code": exc.status_code
        }
    )

@app.exception_handler(Exception)
async def general_exception_handler(request, exc):
    print(f"❌ Error no manejado: {str(exc)}")
    import traceback
    traceback.print_exc()
    return JSONResponse(
        status_code=500,
        content={
            "success": False,
            "error": "Error interno del servidor",
            "detail": str(exc)
        }
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8080)