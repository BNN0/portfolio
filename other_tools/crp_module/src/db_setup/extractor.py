import pandas as pd
import os
from .database import drop_tables, insert_provider, insert_part, init_db, insert_status_quantity_for_all_parts, initialize_supply_chain_data

def run_data_import(file_input, initial_date, status_callback=None):
    def update_status(msg):
        print(msg, flush=True)
        if status_callback:
            status_callback(msg)

    update_status(f"Loading Excel data from {file_input if isinstance(file_input, str) else 'uploaded file'}...")
    xl = pd.ExcelFile(file_input)
    sheet_name = next((s for s in xl.sheet_names if "CPM" in s.upper()), xl.sheet_names[0])
    update_status(f"Using sheet: {sheet_name}")
    cpm_data = pd.read_excel(xl, sheet_name=sheet_name, header=None, skiprows=0)

    # Validate that the file has the expected CPM shape to prevent IndexErrors
    if cpm_data.shape[0] < 89 or cpm_data.shape[1] < 84:
        raise ValueError(f"El archivo cargado no tiene el formato CPM esperado (dimensiones insuficientes: {cpm_data.shape}). Verifique que sea el archivo correcto.")

    # Slice from original row 88 to include the date header row
    cpm_data = cpm_data.iloc[88:, 3:]

    records = []
    makers = []
    part_ids = []

    # Start loop at 1 to skip the date header row (index 0)
    for i in range(1, len(cpm_data), 4):
        if i + 2 < len(cpm_data):
            # ... (parsing logic stays same)
            index = cpm_data.iloc[i, 0]
            part_no = cpm_data.iloc[i, 1].strip() if isinstance(cpm_data.iloc[i, 1], str) else ""
            description = cpm_data.iloc[i, 2].strip() if isinstance(cpm_data.iloc[i, 2], str) else ""
            total_starter = cpm_data.iloc[i, 81] if pd.notna(cpm_data.iloc[i, 81]) else 0
            maker = cpm_data.iloc[i, 3].strip() if isinstance(cpm_data.iloc[i, 3], str) else ""
            verify = cpm_data.iloc[i, 82]
            supply_dates = []
            
            for j in range(83, cpm_data.shape[1]):
                if pd.notna(cpm_data.iloc[0, j]):
                    try:
                        date = pd.to_datetime(cpm_data.iloc[0, j]).date().isoformat()
                    except (ValueError, TypeError):
                        continue
                    
                    required_qty = cpm_data.iloc[i, j] if pd.notna(cpm_data.iloc[i, j]) else 0
                    if required_qty != 'NaN' and required_qty != 0:
                        supply_dates.append({"date": date, "process": "REQUIRED QUANTITY", "qty": round(required_qty)})
                    
                    incoming_delivery = cpm_data.iloc[i+1, j] if pd.notna(cpm_data.iloc[i+1, j]) else 0
                    if incoming_delivery != 'NaN' and incoming_delivery != 0:
                        supply_dates.append({"date": date, "process": "INCOMING DELIVERY", "qty": round(incoming_delivery)})
                    
                    on_the_way = cpm_data.iloc[i+2, j] if pd.notna(cpm_data.iloc[i+2, j]) else 0
                    if on_the_way != 'NaN' and on_the_way != 0:
                        supply_dates.append({"date": date, "process": "ON THE WAY", "qty": round(on_the_way)})
            
            record = {"id": index, "PartNo": part_no, "Description": description, "Total": total_starter, "Maker": maker, "Verify": verify, "SupplyDates": supply_dates}
            if record not in records:
                records.append(record)
                if maker and maker not in makers:
                    makers.append(maker)

    update_status(f"Limpiando tablas...")
    drop_tables()
    
    update_status("Inicializando base de datos...")
    init_db()

    update_status(f"Insertando {len(makers)} proveedores...")
    for maker in makers:
        insert_provider(maker)

    total_parts = len(records)
    update_status(f"Insertando {total_parts} partes y su historial...")
    for idx, record in enumerate(records):
        if idx % 20 == 0:
            update_status(f"Progreso: Insertando parte {idx}/{total_parts}...")
        try:
            part_id = insert_part(record["PartNo"], record["Description"], record["Total"], record["Maker"])
            if part_id: 
                part_ids.append(part_id)
                initialize_supply_chain_data(part_id, record["SupplyDates"])
        except Exception as e:
            print(f"Error al insertar parte {record['PartNo']}: {e}", flush=True)

    update_status(f"Insertando status quantity inicial con fecha {initial_date}...")
    insert_status_quantity_for_all_parts(initial_date)
    
    update_status(f"Recalculando estados para {len(part_ids)} partes (esto puede tardar)...")
    from src.db_manager import recalculate_inventory_status
    for idx, pid in enumerate(part_ids):
        if idx % 20 == 0:
            update_status(f"Recalculando: {idx}/{len(part_ids)}...")
        recalculate_inventory_status(pid)
        
    update_status("Importación y recálculo finalizados con éxito.")
    return True



if __name__ == "__main__":
    pass

