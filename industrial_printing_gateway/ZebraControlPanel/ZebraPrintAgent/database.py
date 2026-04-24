import sqlite3
import json
import os
from datetime import datetime
from typing import Optional

DB_NAME = "printers.db"

def init_db():
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    
    try:
        cursor.execute("SELECT vendor_id FROM printers LIMIT 1")
    except sqlite3.OperationalError:
        # Table might not exist or schema mismatch.
        # Dropping table for clean slate as per major refactor request
        cursor.execute("DROP TABLE IF EXISTS printers")
        
        cursor.execute('''
            CREATE TABLE printers (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT UNIQUE NOT NULL,
                status BOOLEAN DEFAULT 1,
                port TEXT,
                vendor_id TEXT,
                product_id TEXT,
                alias TEXT,
                debug_mode BOOLEAN DEFAULT 0,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
    conn.commit()
    conn.close()

def save_printer_config(
        name: str, 
        status: bool = True,
        port: str = None, 
        vendor_id: str = None, 
        product_id: str = None, 
        alias: str = None, 
        debug_mode: bool = False
    ):
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    now = datetime.now()
    
    try:
        # Check if exists to determine if we insert or update
        cursor.execute("SELECT id FROM printers WHERE name = ?", (name,))
        row = cursor.fetchone()
        
        if row:
            cursor.execute('''
                UPDATE printers 
                SET status = ?, port = ?, vendor_id = ?, product_id = ?, alias = ?, debug_mode = ?, updated_at = ?
                WHERE name = ?
            ''', (status, port, vendor_id, product_id, alias, debug_mode, now, name))
        else:
            cursor.execute('''
                INSERT INTO printers (name, status, port, vendor_id, product_id, alias, debug_mode, created_at, updated_at)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
            ''', (name, status, port, vendor_id, product_id, alias, debug_mode, now, now))
            
        conn.commit()
    finally:
        conn.close()

def get_all_printers():
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    try:
        cursor.execute('''
            SELECT id, name, status, port, vendor_id, product_id, alias, debug_mode, created_at, updated_at 
            FROM printers
        ''')
        rows = cursor.fetchall()
        return [{
            "id": row[0],
            "name": row[1],
            "status": bool(row[2]),
            "port": row[3],
            "vendor_id": row[4],
            "product_id": row[5],
            "alias": row[6],
            "debug_mode": bool(row[7]),
            "created_at": row[8],
            "updated_at": row[9]
        } for row in rows]
    finally:
        conn.close()

def get_printer_by_id(id: int):
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    try:
        cursor.execute('''
            SELECT id, name, status, port, vendor_id, product_id, alias, debug_mode, created_at, updated_at 
            FROM printers WHERE id = ?
        ''', (id,))
        row = cursor.fetchone()

        return {
            "id": row[0],
            "name": row[1],
            "status": bool(row[2]),
            "port": row[3],
            "vendor_id": row[4],
            "product_id": row[5],
            "alias": row[6],
            "debug_mode": bool(row[7]),
            "created_at": row[8],
            "updated_at": row[9]
        }
    finally:
        conn.close()

def verify_printer_by_filter(name: str, vid: str, pid: str):
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    try:
        cursor.execute('''
            SELECT id, name, status, port, vendor_id, product_id, alias, debug_mode, created_at, updated_at 
            FROM printers WHERE name = ? AND vendor_id = ? AND product_id = ?
        ''', (name, vid, pid,))
        row = cursor.fetchone()

        if(row != None):
            return  True

        return False
    except Exception as e:
        print("Error al obtener datos filtrados de impresora: ", e)
    finally:
        conn.close()

def delete_printer(printer_id: int):
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    try:
        cursor.execute("DELETE FROM printers WHERE id = ?", (printer_id,))
        conn.commit()
        return cursor.rowcount > 0
    finally:
        conn.close()

def update_printer_config(
        id: int, 
        status: bool = True,
        port: str = None, 
        vendor_id: str = None, 
        product_id: str = None, 
        alias: str = None, 
        debug_mode: bool = False
        ):
    conn = sqlite3.connect(DB_NAME)
    cursor = conn.cursor()
    now = datetime.now()

    try:

        cursor.execute("SELECT name FROM printers WHERE id = ?", (id,))
        row = cursor.fetchone()



        if row: 
            cursor.execute('''
                UPDATE printers 
                SET status = ?, port = ?, vendor_id = ?, product_id = ?, alias = ?, debug_mode = ?, updated_at = ?
                WHERE id = ?
            ''', (status, port, vendor_id, product_id, alias, debug_mode, now, id,))

        conn.commit()
    except Exception as e:
        print("Error updating config: ", e)
    finally:
        conn.close()

