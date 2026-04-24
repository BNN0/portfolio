from datetime import datetime, timedelta
import psycopg2
from psycopg2.extras import RealDictCursor
import os

def get_connection():
    try:
        return psycopg2.connect(
            host=os.getenv("DB_HOST", "127.0.0.1"),
            port=os.getenv("DB_PORT", "5432"),
            dbname=os.getenv("DB_NAME", "crp_nissan"),
            user=os.getenv("DB_USER", "postgres"),
            password=os.getenv("DB_PASSWORD", "admin"),
            cursor_factory=RealDictCursor,
            client_encoding="utf8",
        )
    except UnicodeDecodeError as e:
        raw_bytes = e.object
        real_error = raw_bytes.decode('windows-1252', errors='replace')
        raise RuntimeError(f"Error de conexión a PostgreSQL detectado: {real_error}") from None


def recalculate_inventory_status(part_id: int):
    conn = None
    try:
        conn = get_connection()
        with conn:
            with conn.cursor() as cursor:
                # 1. Get the earliest STATUS QUANTITY date
                cursor.execute("""
                    SELECT MIN(entry_date) as start_date 
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s AND pt.name = 'STATUS QUANTITY'
                """, (part_id,))
                res = cursor.fetchone()
                if not res or not res['start_date']:
                    print(f"No status history found for part {part_id}")
                    return False
                
                start_date = res['start_date']
                today = datetime.now().date()
                
                # 2. Fetch all relevant records in the range [start_date, today]
                cursor.execute("""
                    SELECT pt.name, scd.quantity, scd.entry_date
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s AND scd.entry_date >= %s AND scd.entry_date <= %s
                """, (part_id, start_date, today))
                rows = cursor.fetchall()
                
                # Organize data: { (date, name): quantity }
                data_map = {}
                for row in rows:
                    date_key = row['entry_date']
                    if isinstance(date_key, datetime): date_key = date_key.date()
                    data_map[(date_key, row['name'])] = row['quantity']
                
                # 3. Cumulative Calculation
                # Initial balance comes from the first day's EXISTING status record
                current_balance = data_map.get((start_date, 'STATUS QUANTITY'), 0)
                
                # Delete existing STATUS QUANTITY records in the range (excluding first day seed)
                # to avoid primary key/unique constraint conflicts during recalculation
                cursor.execute("""
                    DELETE FROM supply_chain_data 
                    WHERE part_no = %s AND process_type_id = 4 
                    AND entry_date > %s AND entry_date <= %s
                """, (part_id, start_date, today))
                
                # Process subsequent days
                curr = start_date + timedelta(days=1)
                while curr <= today:
                    inc = data_map.get((curr, 'INCOMING DELIVERY'), 0)
                    req = data_map.get((curr, 'REQUIRED QUANTITY'), 0)
                    
                    current_balance = current_balance + inc - req
                    
                    # Insert the new calculated status
                    cursor.execute("""
                        INSERT INTO supply_chain_data (part_no, process_type_id, entry_date, quantity, created_at, updated_at)
                        VALUES (%s, 4, %s, %s, NOW(), NOW())
                    """, (part_id, curr, current_balance))
                    
                    curr += timedelta(days=1)
                
                # 4. Update parts table total with today's final balance
                cursor.execute("UPDATE parts SET total = %s WHERE id = %s", (current_balance, part_id))
        return True
    except Exception as e:
        print(f"Error in recalculate_inventory_status for part {part_id}: {e}")
        return False
    finally:
        if conn:
            conn.close()


def get_parts_list(query: str = ""):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                sql = "SELECT id, part_no, description FROM parts"
                params = []
                if query:
                    sql += " WHERE part_no ILIKE %s OR description ILIKE %s"
                    params = [f"%{query}%", f"%{query}%"]
                sql += " LIMIT 10"
                cursor.execute(sql, params)
                return cursor.fetchall()
    except Exception as e:
        print(f"Error in get_parts_list: {e}")
        return []

def get_part_details(part_id: int):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                sql = """
                    SELECT p.*, pr.name as provider_name 
                    FROM parts p
                    LEFT JOIN providers pr ON p.provider_id = pr.id
                    WHERE p.id = %s
                """
                cursor.execute(sql, (part_id,))
                part = cursor.fetchone()
                
                if part:
                    # Check for today's status quantity override
                    from datetime import datetime
                    today = datetime.now().strftime("%Y-%m-%d")
                    status_sql = """
                        SELECT scd.quantity 
                        FROM supply_chain_data scd
                        JOIN process_types pt ON scd.process_type_id = pt.id
                        WHERE scd.part_no = %s AND scd.entry_date = %s AND pt.name = 'STATUS QUANTITY'
                    """
                    cursor.execute(status_sql, (part_id, today))
                    override = cursor.fetchone()
                    if override:
                        part['total'] = override['quantity']
                
                return part
    except Exception as e:
        print(f"Error in get_part_details: {e}")
        return None

def upsert_inventory_data(part_id: int, type_name: str, quantity: float, entry_date: str):
    process_type_map = {
        "REQUIRED QUANTITY": 1,
        "INCOMING DELIVERY": 2,
        "ON THE WAY": 3,
        "STATUS QUANTITY": 4
    }
    
    type_id = process_type_map.get(type_name)
    if not type_id:
        raise ValueError(f"Invalid process type: {type_name}")
    
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                # Check if record exists for this day/part/type
                check_sql = """
                    SELECT id FROM supply_chain_data 
                    WHERE part_no = %s AND process_type_id = %s AND entry_date = %s
                """
                cursor.execute(check_sql, (part_id, type_id, entry_date))
                existing = cursor.fetchone()
                
                if existing:
                    # Update
                    update_sql = """
                        UPDATE supply_chain_data 
                        SET quantity = %s, updated_at = NOW() 
                        WHERE id = %s
                    """
                    cursor.execute(update_sql, (quantity, existing['id']))
                else:
                    # Insert
                    insert_sql = """
                        INSERT INTO supply_chain_data 
                        (part_no, process_type_id, entry_date, quantity, created_at, updated_at)
                        VALUES (%s, %s, %s, %s, NOW(), NOW())
                    """
                    cursor.execute(insert_sql, (part_id, type_id, entry_date, quantity))
            conn.commit()
            return True
    except Exception as e:
        print(f"Error in upsert_inventory_data: {e}")
        return False

def get_daily_status(part_id: int, entry_date: str):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                sql = """
                    SELECT pt.name, scd.quantity 
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s AND scd.entry_date = %s
                """
                cursor.execute(sql, (part_id, entry_date))
                rows = cursor.fetchall()
                # Convert to dict: {'STATUS QUANTITY': 14250, ...}
                return {row['name']: row['quantity'] for row in rows}
    except Exception as e:
        print(f"Error in get_daily_status: {e}")
        return {}
def get_part_status_history(part_id: int):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                sql = """
                    SELECT pt.name, scd.quantity, scd.entry_date
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s
                    ORDER BY scd.entry_date ASC
                """
                cursor.execute(sql, (part_id,))
                return cursor.fetchall()
    except Exception as e:
        print(f"Error in get_part_status_history: {e}")
        return []

def get_projected_inventory(part_id: int, target_date_str: str):
    try:
        today = datetime.now().date()
        target_date = datetime.strptime(target_date_str, "%Y-%m-%d").date()
        
        if target_date <= today:
            return None # Use current logic for today/past
            
        with get_connection() as conn:
            with conn.cursor() as cursor:
                # 1. Get current total from parts table
                cursor.execute("SELECT total FROM parts WHERE id = %s", (part_id,))
                res = cursor.fetchone()
                current_total = res['total'] if res else 0
                
                # 2. Sum movements from today+1 to target_date
                # pt.id 1 = REQUIRED QUANTITY, 2 = INCOMING DELIVERY
                sql = """
                    SELECT pt.name, SUM(scd.quantity) as total_qty
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s 
                      AND scd.entry_date > %s 
                      AND scd.entry_date <= %s
                      AND pt.id IN (1, 2)
                    GROUP BY pt.name
                """
                cursor.execute(sql, (part_id, today, target_date))
                movements = cursor.fetchall()
                
                delta = 0
                for m in movements:
                    if m['name'] == 'INCOMING DELIVERY':
                        delta += m['total_qty']
                    elif m['name'] == 'REQUIRED QUANTITY':
                        delta -= m['total_qty']
                
                return current_total + delta
    except Exception as e:
        print(f"Error in get_projected_inventory: {e}")
        return None

def find_out_of_stock_risk(part_id: int):
    try:
        from datetime import datetime, timedelta
        today = datetime.now().date()
        target_date = today + timedelta(days=60)
        
        with get_connection() as conn:
            with conn.cursor() as cursor:
                # Get current total and stock limit
                cursor.execute("SELECT total, stock_limit FROM parts WHERE id = %s", (part_id,))
                res = cursor.fetchone()
                current_total = res['total'] if res else 0
                stock_limit = res.get('stock_limit', 0) if res else 0
                
                # Get all relevant movements in the next 60 days
                sql = """
                    SELECT pt.name, scd.quantity, scd.entry_date
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.part_no = %s 
                      AND scd.entry_date > %s 
                      AND scd.entry_date <= %s
                      AND pt.id IN (1, 2)
                    ORDER BY scd.entry_date ASC
                """
                cursor.execute(sql, (part_id, today, target_date))
                movements = cursor.fetchall()

                # Calculate effectively day by day
                running_total = current_total
                
                # We need to process by day to find the EXACT first day it drops below 0
                # Group movements by date
                daily_movements = {}
                for m in movements:
                    dt = m['entry_date']
                    if isinstance(dt, datetime): dt = dt.date()
                    if dt not in daily_movements:
                        daily_movements[dt] = {'inc': 0, 'req': 0}
                    if m['name'] == 'INCOMING DELIVERY':
                        daily_movements[dt]['inc'] += m['quantity']
                    elif m['name'] == 'REQUIRED QUANTITY':
                        daily_movements[dt]['req'] += m['quantity']

                # Go day by day
                for day_offset in range(1, 61):
                    current_day = today + timedelta(days=day_offset)
                    if current_day in daily_movements:
                        running_total += daily_movements[current_day]['inc']
                        running_total -= daily_movements[current_day]['req']
                    
                    if running_total < stock_limit:
                        # Found the first negative day
                        color = "red" if day_offset < 30 else "yellow"
                        return {"date": current_day.strftime("%Y-%m-%d"), "color": color}
                        
        return None
    except Exception as e:
        print(f"Error in find_out_of_stock_risk: {e}")
        return None

def update_stock_limit(part_id: int, new_limit: int):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute("UPDATE parts SET stock_limit = %s WHERE id = %s", (new_limit, part_id))
                conn.commit()
                return True
    except Exception as e:
        print(f"Error in update_stock_limit: {e}")
        return False

def get_dashboard_summary():
    try:
        from datetime import datetime, timedelta
        today = datetime.now().date()
        horizon = today + timedelta(days=60)
        
        with get_connection() as conn:
            with conn.cursor() as cursor:
                # 1. Get all parts
                cursor.execute("""
                    SELECT p.id, p.part_no, p.description, p.total, p.stock_limit, p.acknowledged_until, pr.name as provider_name
                    FROM parts p
                    LEFT JOIN providers pr ON p.provider_id = pr.id
                """)
                parts = cursor.fetchall()
                
                # 2. Get all future movements in bulk
                cursor.execute("""
                    SELECT scd.part_no as part_id, pt.name, scd.quantity, scd.entry_date
                    FROM supply_chain_data scd
                    JOIN process_types pt ON scd.process_type_id = pt.id
                    WHERE scd.entry_date > %s AND scd.entry_date <= %s
                      AND pt.id IN (1, 2)
                    ORDER BY scd.entry_date ASC
                """, (today, horizon))
                movements = cursor.fetchall()
                
                # Group by part_id
                part_movements = {}
                for m in movements:
                    pid = m['part_id']
                    if pid not in part_movements:
                        part_movements[pid] = []
                    part_movements[pid].append(m)
                
                summary = {
                    "kpis": {"total_parts": len(parts), "critical_stockouts": 0, "risk_alerts": 0},
                    "critical_alerts": [],
                    "watchlist": [],
                    "vendor_ranking": []
                }
                
                vendor_stats = {} # vendor_name -> {critical: X, risk: Y, total: Z}
                
                # 3. Analyze each part
                for p in parts:
                    pid = p['id']
                    running_total = p['total'] or 0
                    stock_limit = p['stock_limit'] or 0
                    ack_until = p['acknowledged_until']
                    vname = p['provider_name'] or 'Unknown'
                    
                    if vname not in vendor_stats:
                        vendor_stats[vname] = {"critical": 0, "risk": 0, "total_parts": 0}
                    vendor_stats[vname]["total_parts"] += 1
                    
                    risk_date = None
                    risk_color = None
                    
                    # Process movements day by day for this part
                    p_movs = part_movements.get(pid, [])
                    
                    # Group by date
                    daily = {}
                    for m in p_movs:
                        dt = m['entry_date']
                        if dt not in daily: daily[dt] = {'inc': 0, 'req': 0}
                        if m['name'] == 'INCOMING DELIVERY': daily[dt]['inc'] += m['quantity']
                        else: daily[dt]['req'] += m['quantity']
                    
                    # Check next 60 days
                    for offset in range(1, 61):
                        curr = today + timedelta(days=offset)
                        if curr in daily:
                            running_total += daily[curr]['inc']
                            running_total -= daily[curr]['req']
                        
                        if running_total < stock_limit:
                            risk_date = curr
                            risk_color = "red" if offset < 30 else "yellow"
                            break
                    
                    # Check acknowledgment status
                    is_acknowledged = False
                    if risk_date and ack_until:
                        if risk_date >= ack_until:
                            is_acknowledged = True
                    
                    if risk_color == "red": vendor_stats[vname]["critical"] += 1
                    elif risk_color == "yellow": vendor_stats[vname]["risk"] += 1
                    
                    p_info = {
                        "id": pid,
                        "part_no": p['part_no'],
                        "description": p['description'],
                        "total": p['total'],
                        "provider_name": vname,
                        "risk_date": risk_date.strftime("%Y-%m-%d") if risk_date else None,
                        "risk_color": risk_color,
                        "is_acknowledged": is_acknowledged,
                        "acknowledged_until": ack_until.strftime("%Y-%m-%d") if ack_until else None
                    }
                    
                    if risk_color == "red":
                        summary["kpis"]["critical_stockouts"] += 1
                        summary["critical_alerts"].append(p_info)
                    elif risk_color == "yellow":
                        summary["kpis"]["risk_alerts"] += 1
                        summary["critical_alerts"].append(p_info)
                    
                    summary["watchlist"].append(p_info)
                
                # 4. Generate Vendor Ranking
                summary["vendor_ranking"] = sorted(
                    [{"name": k, **v} for k, v in vendor_stats.items()],
                    key=lambda x: (x['critical'], x['risk']),
                    reverse=True
                )[:10]

                # Sort watchlist: Critical first, then Risk, then by date
                summary["watchlist"].sort(key=lambda x: (
                    x['risk_color'] != 'red', 
                    x['risk_color'] != 'yellow', 
                    x['risk_date'] or '9999-99-99'
                ))
                
                # Limit watchlist to first 15 for UI performance
                # But keep the full list in another key if we want to filter locally?
                # For now let's just limit the main return for speed
                summary["watchlist_preview"] = summary["watchlist"][:15]
                
                return summary
    except Exception as e:
        print(f"Error in get_dashboard_summary: {e}")
        import traceback
        traceback.print_exc()
        return None

def acknowledge_part_risk(part_id: int, until_date: str):
    try:
        with get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute("UPDATE parts SET acknowledged_until = %s WHERE id = %s", (until_date, part_id))
                conn.commit()
                return True
    except Exception as e:
        print(f"Error in acknowledge_part_risk: {e}")
        return False

