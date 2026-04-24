from fastapi import FastAPI, HTTPException
from fastapi.responses import FileResponse
from typing import List, Optional
from pydantic import BaseModel
import os
import tempfile
from reportlab.pdfgen import canvas
from reportlab.lib.pagesizes import letter, A4
from reportlab.platypus import Table, TableStyle
from reportlab.lib import colors
from reportlab.lib.units import inch
from reportlab.lib.styles import ParagraphStyle
from reportlab.platypus import Paragraph
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.utils import ImageReader
import asyncio
from datetime import datetime

app = FastAPI(title="Generador de PDF de Certificados", version="1.0.0")

class Cabezera1(BaseModel):
    boleta_no: str
    fecha: str

class Cabezera2(BaseModel):
    productor: str
    producto: str
    procedencia: str
    vehiculo: str
    placas: str
    chofer: str

class AnalisisItem(BaseModel):
    tipo: str
    porcentaje: Optional[float] = None
    castigo: Optional[float] = None

class PesosInfo1(BaseModel):
    peso_bruto: float
    peso_tara: float
    peso_neto: float
    fecha_completa: str

class PesosInfo2(BaseModel):
    deduccion: float
    peso_neto_analizado: float

class CertificadoRequest(BaseModel):
    boleta_no: str
    fecha: str
    productor: str
    producto: str
    procedencia: str
    vehiculo: str
    placas: str
    chofer: str
    analisis: Optional[List[AnalisisItem]] = []
    pesos_info1: PesosInfo1
    pesos_info2: PesosInfo2

# Colores principales del diseño
principal_color = (0.8, 0, 0.3)
secondary_color = (0.5, 0.5, 0.5)

# Funciones del código original (copiadas tal como están)
async def draw_logo(c: canvas.Canvas, logo_color: int):
    logo_path = "./images/logo.png" if logo_color == 1 else "./images/logo_gray.png"
    logo_path_second = "./images/logo_gray.png"
    logo_width = 80
    logo_height = 65

    # Dibujar logo en la parte superior e inferior
    c.drawImage(logo_path, 25, A4[1] - 80, width=logo_width, height=logo_height)
    c.drawImage(logo_path_second, 25, (A4[1] / 2) - 80, width=logo_width, height=logo_height)

async def draw_background(c: canvas.Canvas, logo_color: int):
    background_path = "./images/logo.png" if logo_color == 1 else "./images/logo_gray.png"
    background_path_second = "./images/logo_gray.png" 
    background_width = 240
    background_height = 200

    # Dibujar fondo en la parte superior e inferior
    c.setFillAlpha(0.1)
    c.drawImage(background_path, 340, 450, width=background_width, height=background_height)
    c.setFillColorRGB(0.7, 0.7, 0.7)
    c.drawImage(background_path_second, 340, (A4[1] / 2) - 390, width=background_width, height=background_height, mask='auto')
    c.setFillColorRGB(0, 0, 0)
    c.setFillAlpha(1)

async def draw_infoCompany(c: canvas.Canvas, data: Cabezera1, page_color: int):
    w, h = A4
    # Parte superior
    c.setFont("Helvetica-Bold", 18)
    c.drawString(w / 5, h - 35, "ACEITES Y PROTEINAS, S.A. DE C.V.")

    c.setFont("Helvetica-Bold", 10)
    c.drawString(w - (w / 5.5), h - 30, "Boleta No: ")

    color = principal_color if page_color == 1 else secondary_color
    # Número de boleta
    c.setFillColor(color)
    c.setFont("Helvetica", 10)
    c.drawString(w - ((w / 10.5)), h - 30, data.boleta_no)

    # Información de la empresa
    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w / 5, h - 50, "CAMINO A BACHOCO S/N LOCALIDAD BACHIGUALATO. C.P. 80130 APDO, POST. 2033")
    c.drawString(w / 5, h - 62.5, "TEL: 667-600-050 Y 667-600-018 FAX, CULIACAN, SINALOA.")

    c.setFont("Helvetica-Bold", 11)
    c.drawString(w / 5, h - 85, "CERTIFICADO DE PESO Y CALIDAD")

    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 40, h - 85, "Fecha: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 75, h - 85, data.fecha)

    # Parte inferior
    c.setFillColor((0,0,0))
    c.setFont("Helvetica-Bold", 18)
    c.drawString(w / 5, (h / 2) - 35, "ACEITES Y PROTEINAS, S.A. DE C.V.")

    c.setFont("Helvetica-Bold", 10)
    c.drawString(w - (w / 5.5), (h / 2) - 30, "Boleta No: ")

    # Número de boleta
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 10)
    c.drawString(w - ((w / 10.5)), (h / 2) - 30, data.boleta_no)

    # Información de la empresa
    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w / 5, (h / 2) - 50, "CAMINO A BACHOCO S/N LOCALIDAD BACHIGUALATO. C.P. 80130 APDO, POST. 2033")
    c.drawString(w / 5, (h / 2) - 62.5, "TEL: 667-600-050 Y 667-600-018 FAX, CULIACAN, SINALOA.")

    c.setFont("Helvetica-Bold", 11)
    c.drawString(w / 5, (h / 2) - 85, "CERTIFICADO DE PESO Y CALIDAD")

    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 40, (h / 2) - 85, "Fecha: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 75, (h / 2) - 85, data.fecha)

async def draw_infoShipment(c: canvas.Canvas, data: Cabezera2, page_color: int):
    w, h = A4
    color = principal_color if page_color == 1 else secondary_color
    # Parte superior
    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(30, h - 115, "Productor: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString(80, h - 115, data.productor)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 80, h - 115, "Producto: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) - 30, h - 115, data.producto)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w - (w / 3), h - 115, "Procedencia: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString(w - (w / 4.25), h - 115, data.procedencia)

    c.setFillColor((0,0,0))
    c.setFont("Helvetica-Bold", 9)
    c.drawString(30, h - 135, "Vehiculo: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString(75, h - 135, data.vehiculo)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 80, h - 135, "Placas: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) - 45, h - 135, data.placas)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w - (w / 3), h - 135, "Chofer: ")
    c.setFillColor(color)
    c.setFont("Helvetica", 9)
    c.drawString(w - (w / 3.6), h - 135, data.chofer)

    # Parte inferior
    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(30, (h / 2) - 115, "Productor: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString(80, (h / 2) - 115, data.productor)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 80, (h / 2) - 115, "Producto: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) - 30, (h / 2) - 115, data.producto)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w - (w / 3), (h / 2) - 115, "Procedencia: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString(w - (w / 4.25), (h / 2) - 115, data.procedencia)

    c.setFillColor((0,0,0))
    c.setFont("Helvetica-Bold", 9)
    c.drawString(30, (h / 2) - 135, "Vehiculo: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString(75, (h / 2) - 135, data.vehiculo)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 80, (h / 2) - 135, "Placas: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) - 45, (h / 2) - 135, data.placas)

    c.setFont("Helvetica-Bold", 9)
    c.setFillColor((0,0,0))
    c.drawString(w - (w / 3), (h / 2) - 135, "Chofer: ")
    c.setFillColor(secondary_color)
    c.setFont("Helvetica", 9)
    c.drawString(w - (w / 3.6), (h / 2) - 135, data.chofer)

async def draw_analisisTable(c: canvas.Canvas, data: CertificadoRequest, page_color: int):
    w, h = A4
    color = principal_color if page_color == 1 else secondary_color

    tipos_analisis = [
            "HUMEDAD", "IMPUREZA", "DAÑO", "QUEBRADO", "PESO ESPECIFICO",
            "GRANO VERDE", "YODO", "CROMATOGRAFIA", "MANCHADO", "VIEJO", "OTROS GRANOS"
        ]

    analisis_data = [['ANALISIS', '%', 'CASTIGOS']]
    analisis_dict = {item.tipo.upper(): item for item in (data.analisis or [])}

    for tipo in tipos_analisis:
        analisis_item = analisis_dict.get(tipo)
        tiene_info = analisis_item and (analisis_item.porcentaje is not None or analisis_item.castigo is not None)

        porcentaje = str(analisis_item.porcentaje) if analisis_item and analisis_item.porcentaje is not None else "-"
        castigo = str(analisis_item.castigo) if analisis_item and analisis_item.castigo is not None else "-"

        analisis_data.append([tipo, porcentaje, castigo])

    analisis_table = Table(analisis_data, colWidths=[90, 50, 70], rowHeights=15)

    table_style = [
            # Header
            ('BACKGROUND', (0, 0), (-1, 0), color),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, -1), 7),
            ('GRID', (0, 0), (-1, -1), 1, color),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ]

    for i, tipo in enumerate(tipos_analisis, 1):
            analisis_item = analisis_dict.get(tipo)
            if not (analisis_item and (analisis_item.porcentaje is not None or analisis_item.castigo is not None)):
                table_style.append(('BACKGROUND', (0, i), (-1, i), colors.lightgrey))

    analisis_table.setStyle(TableStyle(table_style))
    analisis_table.wrapOn(c, w, h)
    analisis_table.drawOn(c, 30, h - 340)
    
    # Parte inferior
    table_style_second = [
            # Header
            ('BACKGROUND', (0, 0), (-1, 0), secondary_color),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, -1), 7),
            ('GRID', (0, 0), (-1, -1), 1, secondary_color),
            ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ]
    analisis_table.setStyle(TableStyle(table_style_second))
    analisis_table.drawOn(c, 30, (h / 2) - 340)

async def draw_pesosTable(c: canvas.Canvas, data: PesosInfo1, page_color: int):
    w, h = A4
    color = principal_color if page_color == 1 else secondary_color
    header_height = 17
    value_height = 27

    main_value_style = ParagraphStyle(
        name='MainValue',
        alignment=TA_CENTER,
        fontSize=9,
        fontName='Helvetica',
        leading=6,
        spaceAfter=1
    )

    subtext_style = ParagraphStyle(
        name='Subtext',
        alignment=TA_CENTER,
        fontSize=6,
        fontName='Helvetica',
        leading=10
    )

    def create_value_cell(main_text, sub_text):
        main_para = Paragraph(f"{main_text}", main_value_style)
        sub_para = Paragraph(sub_text, subtext_style)
        inner_table = Table([[main_para], [sub_para]], colWidths=[120])
        return inner_table

    pesos_data = [
        ["PESO BRUTO (KG)"],
        [create_value_cell(f"{data.peso_bruto:.3f}", data.fecha_completa)],
        ["PESO TARA (KG)"],
        [create_value_cell(f"{data.peso_tara:.3f}", data.fecha_completa)],
        ["PESO NETO (KG)"],
        [create_value_cell(f"{data.peso_neto:.3f}", data.fecha_completa)]
    ]

    row_heights = [
        header_height, value_height,
        header_height, value_height,
        header_height, value_height,
    ]

    pesos_table = Table(pesos_data, colWidths=[120], rowHeights=row_heights)

    table_style = [
        ('BACKGROUND', (0, 0), (0, 0), color),
        ('BACKGROUND', (0, 2), (0, 2), color),
        ('BACKGROUND', (0, 4), (0, 4), color),
        ('TEXTCOLOR', (0, 0), (0, 0), colors.whitesmoke),
        ('TEXTCOLOR', (0, 2), (0, 2), colors.whitesmoke),
        ('TEXTCOLOR', (0, 4), (0, 4), colors.whitesmoke),
        ('BACKGROUND', (0, 1), (0, 1), colors.white),
        ('BACKGROUND', (0, 3), (0, 3), colors.white),
        ('BACKGROUND', (0, 5), (0, 5), colors.white),
        ('ALIGN', (0, 1), (0, 1), 'CENTER'),
        ('ALIGN', (0, 3), (0, 3), 'CENTER'),
        ('ALIGN', (0, 5), (0, 5), 'CENTER'),
        ('VALIGN', (0, 1), (0, 1), 'MIDDLE'),
        ('VALIGN', (0, 3), (0, 3), 'MIDDLE'),
        ('VALIGN', (0, 5), (0, 5), 'MIDDLE'),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, -1), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, -1), 8),
        ('GRID', (0, 0), (-1, -1), 1, color),
    ]

    pesos_table.setStyle(TableStyle(table_style))
    pesos_table.wrapOn(c, w, h)
    pesos_table.drawOn(c, (w / 2) - 20, h - 300)

    # Parte inferior
    table_style_second = [
        ('BACKGROUND', (0, 0), (0, 0), secondary_color),
        ('BACKGROUND', (0, 2), (0, 2), secondary_color),
        ('BACKGROUND', (0, 4), (0, 4), secondary_color),
        ('TEXTCOLOR', (0, 0), (0, 0), colors.whitesmoke),
        ('TEXTCOLOR', (0, 2), (0, 2), colors.whitesmoke),
        ('TEXTCOLOR', (0, 4), (0, 4), colors.whitesmoke),
        ('BACKGROUND', (0, 1), (0, 1), colors.white),
        ('BACKGROUND', (0, 3), (0, 3), colors.white),
        ('BACKGROUND', (0, 5), (0, 5), colors.white),
        ('ALIGN', (0, 1), (0, 1), 'CENTER'),
        ('ALIGN', (0, 3), (0, 3), 'CENTER'),
        ('ALIGN', (0, 5), (0, 5), 'CENTER'),
        ('VALIGN', (0, 1), (0, 1), 'MIDDLE'),
        ('VALIGN', (0, 3), (0, 3), 'MIDDLE'),
        ('VALIGN', (0, 5), (0, 5), 'MIDDLE'),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, -1), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, -1), 8),
        ('GRID', (0, 0), (-1, -1), 1, secondary_color),
    ]
    pesos_table.setStyle(TableStyle(table_style_second))
    pesos_table.drawOn(c, (w / 2) - 20, (h / 2) - 300)

async def draw_deductionTable(c: canvas.Canvas, data: PesosInfo2, page_color: int):
    w, h = A4
    color = principal_color if page_color == 1 else secondary_color
    header_height = 17
    value_height = 27

    main_value_style = ParagraphStyle(
        name='MainValue',
        alignment=TA_CENTER,
        fontSize=9,
        fontName='Helvetica',
        leading=6,
        spaceAfter=1
    )

    def create_value_cell(main_text):
        main_para = Paragraph(f"{main_text}", main_value_style)
        inner_table = Table([[main_para]], colWidths=[130])
        return inner_table

    pesos_data = [
        ["DEDUCCIÓN (KG)"],
        [create_value_cell(f"{data.deduccion:.3f}")],
        ["PESO NETO ANALIZADO (KG)"],
        [create_value_cell(f"{data.peso_neto_analizado:.3f}")]
    ]

    row_heights = [
        header_height, value_height,
        header_height, value_height
    ]

    pesos_table = Table(pesos_data, colWidths=[130], rowHeights=row_heights)

    table_style = [
        ('BACKGROUND', (0, 0), (0, 0), color),
        ('BACKGROUND', (0, 2), (0, 2), color),
        ('TEXTCOLOR', (0, 0), (0, 0), colors.whitesmoke),
        ('TEXTCOLOR', (0, 2), (0, 2), colors.whitesmoke),
        ('BACKGROUND', (0, 1), (0, 1), colors.white),
        ('BACKGROUND', (0, 3), (0, 3), colors.white),
        ('ALIGN', (0, 1), (0, 1), 'CENTER'),
        ('ALIGN', (0, 3), (0, 3), 'CENTER'),
        ('VALIGN', (0, 1), (0, 1), 'MIDDLE'),
        ('VALIGN', (0, 3), (0, 3), 'MIDDLE'),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, -1), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, -1), 8),
        ('GRID', (0, 0), (-1, -1), 1, color),
    ]

    pesos_table.setStyle(TableStyle(table_style))
    pesos_table.wrapOn(c, w, h)
    pesos_table.drawOn(c, (w - 180), h - 280)

    # Parte inferior
    table_style_second = [
        ('BACKGROUND', (0, 0), (0, 0), secondary_color),
        ('BACKGROUND', (0, 2), (0, 2), secondary_color),
        ('TEXTCOLOR', (0, 0), (0, 0), colors.whitesmoke),
        ('TEXTCOLOR', (0, 2), (0, 2), colors.whitesmoke),
        ('BACKGROUND', (0, 1), (0, 1), colors.white),
        ('BACKGROUND', (0, 3), (0, 3), colors.white),
        ('ALIGN', (0, 1), (0, 1), 'CENTER'),
        ('ALIGN', (0, 3), (0, 3), 'CENTER'),
        ('VALIGN', (0, 1), (0, 1), 'MIDDLE'),
        ('VALIGN', (0, 3), (0, 3), 'MIDDLE'),
        ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
        ('FONTNAME', (0, 0), (-1, -1), 'Helvetica-Bold'),
        ('FONTSIZE', (0, 0), (-1, -1), 8),
        ('GRID', (0, 0), (-1, -1), 1, secondary_color),
    ]
    pesos_table.setStyle(TableStyle(table_style_second))
    pesos_table.drawOn(c, (w - 180), (h / 2) - 280)

async def draw_signs(c: canvas.Canvas):
    w, h = A4
    c.setFont("Helvetica", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 20, h - 340, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 10, h - 353, "PESADOR")
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 140, h - 340, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 165, h - 353, "ANALIZADOR")
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 60, h - 370, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 95, h - 385, "CHOFER")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) - 270, h - 353, "Observaciones:")

    # Parte inferior
    c.setFont("Helvetica", 9)
    c.setFillColor((0,0,0))
    c.drawString((w / 2) - 20, (h / 2) - 340, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 10, (h / 2) - 353, "PESADOR")
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 140, (h / 2) - 340, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 165, (h / 2) - 353, "ANALIZADOR")
    c.setFont("Helvetica", 9)
    c.drawString((w / 2) + 60, (h / 2)- 370, "_____________________")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) + 95, (h / 2) - 385, "CHOFER")
    c.setFont("Helvetica-Bold", 9)
    c.drawString((w / 2) - 270, (h / 2) - 353, "Observaciones:")

    # Texto giratorio
    c.saveState()
    c.translate(200, 50)
    c.setFillColor((0.5,0.5,0.5))
    c.setFont("Helvetica-Bold", 8)
    c.rotate(90)
    c.drawString(400, -370, "SR. PRODUCTOR: PARA SU COMODIDAD CONTAMOS CON 3 RAMPAS PARA DESCARGA")
    c.drawString((h/2) -440, - 370, "SR. PRODUCTOR: PARA SU COMODIDAD CONTAMOS CON 3 RAMPAS PARA DESCARGA")
    c.setFillColor((0,0,0))
    c.restoreState()

async def create_pdf_page(c: canvas.Canvas, data: CertificadoRequest, page_color: int):
    w, h = A4
    await draw_logo(c, page_color)
    await draw_background(c, page_color)
    await draw_infoCompany(c, Cabezera1(boleta_no=data.boleta_no, fecha=data.fecha), page_color)
    await draw_infoShipment(c, Cabezera2(
        productor=data.productor,
        producto=data.producto,
        procedencia=data.procedencia,
        vehiculo=data.vehiculo,
        placas=data.placas,
        chofer=data.chofer
    ), page_color)
    await draw_analisisTable(c, data, page_color)
    await draw_pesosTable(c, data.pesos_info1, page_color)
    await draw_deductionTable(c, data.pesos_info2, page_color)
    await draw_signs(c)

    c.line(-w, (h / 2), w, (h / 2))  # Draw a horizontal line across the page
    c.showPage()

async def second_page(data: CertificadoRequest, filename: str):
    """Función equivalente a second_page del código original"""
    c = canvas.Canvas(filename, pagesize=A4)
    await create_pdf_page(c, data, page_color=1)
    await create_pdf_page(c, data, page_color=2)
    c.save()

@app.post("/generate-certificate")
async def generate_certificate(certificado: CertificadoRequest):
    """
    Endpoint para generar el certificado PDF
    Recibe todos los datos necesarios y devuelve el archivo PDF
    """
    try:
        # Crear un archivo temporal
        with tempfile.NamedTemporaryFile(delete=False, suffix=".pdf") as tmp_file:
            temp_filename = tmp_file.name
        
        # Generar el PDF usando la función second_page (equivalente a second_table)
        await second_page(certificado, temp_filename)
        
        # Nombre del archivo final
        fecha_actual = datetime.now().strftime("%Y%m%d_%H%M%S")
        final_filename = f"Boleta_{certificado.boleta_no}-{fecha_actual}.pdf"
        
        # Retornar el archivo PDF
        return FileResponse(
            path=temp_filename,
            filename=final_filename,
            media_type='application/pdf',
            headers={"Content-Disposition": f"attachment; filename={final_filename}"}
        )
    
    except Exception as e:
        # Limpiar el archivo temporal si existe
        if 'temp_filename' in locals() and os.path.exists(temp_filename):
            os.unlink(temp_filename)
        raise HTTPException(status_code=500, detail=f"Error generando el PDF: {str(e)}")

@app.get("/")
async def root():
    """Endpoint de salud para verificar que la API está funcionando"""
    return {"message": "API para Generación de Certificados PDF está funcionando"}

@app.get("/health")
async def health_check():
    """Endpoint de verificación de salud"""
    return {"status": "healthy", "service": "PDF Certificate Generator"}

# Ejemplo de uso del endpoint
@app.get("/example-request")
async def example_request():
    """
    Devuelve un ejemplo de la estructura de datos que debe enviarse al endpoint
    """
    return {
            "boleta_no": "123459",
            "fecha": "01/01/2024",
            "productor": "Productor de prueba",
            "producto": "Producto de prueba",
            "procedencia": "Procedencia de prueba",
            "vehiculo": "Vehiculo de prueba",
            "placas": "ABC-1234",
            "chofer": "Chofer de prueba",
            "analisis": [
                {"tipo": "HUMEDAD", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "IMPUREZA", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "DAÑO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "QUEBRADO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "PESO ESPECIFICO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "GRANO VERDE", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "YODO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "CROMATOGRAFIA", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "MANCHADO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "VIEJO", "porcentaje": 10.0, "castigo": 12.0},
                {"tipo": "OTROS GRANOS", "porcentaje": 10.0, "castigo": 12.0}
            ],
            "pesos_info1": {
                "peso_bruto": 1000.0,
                "peso_tara": 200.0,
                "peso_neto": 800.0,
                "fecha_completa": "01/01/2024 12:00:00"
            },
            "pesos_info2": {
                "deduccion": 50.0,
                "peso_neto_analizado": 750.0
            }
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)