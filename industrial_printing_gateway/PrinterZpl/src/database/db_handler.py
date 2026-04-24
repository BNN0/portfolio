import sqlite3
import json
import os
import sys
import re

class Database:
    def __init__(self, db_path=None):
        if db_path is None:
            # Si se ejecuta como ejecutable empaquetado (PyInstaller)
            if getattr(sys, 'frozen', False):
                app_data = os.environ.get('APPDATA')
                if not app_data:
                    # Alternativa para otros SO si fuera necesario
                    app_data = os.path.expanduser('~')
                
                base_dir = os.path.join(app_data, "PrinterZpl")
                if not os.path.exists(base_dir):
                    os.makedirs(base_dir)
                self.db_path = os.path.join(base_dir, "zpl_manager.db")
            else:
                # Por defecto en desarrollo, en la misma carpeta que este archivo
                self.db_path = os.path.join(os.path.dirname(__file__), "zpl_manager.db")
        else:
            self.db_path = db_path
        self.init_db()

    def init_db(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Tabla de plantillas
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS templates (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT UNIQUE,
                zpl_format TEXT,
                fields TEXT
            )
        ''')
        
        # Tabla de impresoras
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS printers (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT UNIQUE,
                ip_address TEXT,
                port INTEGER DEFAULT 9100,
                description TEXT
            )
        ''')
        
        # Tabla de historial de impresión
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS print_jobs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                template_name TEXT,
                printer_name TEXT,
                quantity INTEGER,
                data TEXT,
                status TEXT,
                serials TEXT,
                label_date TEXT,
                job_type TEXT,
                printed_by TEXT
            )
        ''')

        # Migración: Verificar si las nuevas columnas existen y agregarlas si no
        cursor.execute("PRAGMA table_info(print_jobs)")
        columns = [col[1] for col in cursor.fetchall()]
        if "serials" not in columns:
            cursor.execute("ALTER TABLE print_jobs ADD COLUMN serials TEXT")
        if "label_date" not in columns:
            cursor.execute("ALTER TABLE print_jobs ADD COLUMN label_date TEXT")
        if "job_type" not in columns:
            cursor.execute("ALTER TABLE print_jobs ADD COLUMN job_type TEXT")
        if "printed_by" not in columns:
            cursor.execute("ALTER TABLE print_jobs ADD COLUMN printed_by TEXT")
        if "created_at" in columns and "timestamp" not in columns:
            try:
                cursor.execute("ALTER TABLE print_jobs RENAME COLUMN created_at TO timestamp")
            except sqlite3.OperationalError:
                # If RENAME COLUMN is not supported, we'll handle it in the query
                pass

        # Tabla de preconfiguraciones (Presets)
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS label_presets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                template_name TEXT,
                preset_name TEXT,
                data TEXT,
                UNIQUE(template_name, preset_name)
            )
        ''')

        # Tabla de seriales escaneados
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS scanned_serials (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                job_id INTEGER,
                master_serial TEXT,
                scanned_code TEXT,
                FOREIGN KEY(job_id) REFERENCES print_jobs(id)
            )
        ''')

        # Tabla de configuraciones globales (como URL de API)
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS settings (
                key TEXT PRIMARY KEY,
                value TEXT
            )
        ''')

        # Tabla de usuarios
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE,
                password TEXT,
                role TEXT
            )
        ''')

        # Insertar administrador por defecto si no hay usuarios
        cursor.execute("SELECT COUNT(*) FROM users")
        if cursor.fetchone()[0] == 0:
            cursor.execute("INSERT INTO users (username, password, role) VALUES (?, ?, ?)",
                         ("Admin", "admin1234", "admin"))
        
        conn.commit()
        conn.close()

    def add_template(self, name, zpl_format, fields):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            cursor.execute("INSERT INTO templates (name, zpl_format, fields) VALUES (?, ?, ?)",
                         (name, zpl_format, json.dumps(fields)))
            conn.commit()
        finally:
            conn.close()

    def update_template(self, name, zpl_format, fields):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            cursor.execute("UPDATE templates SET zpl_format = ?, fields = ? WHERE name = ?",
                         (zpl_format, json.dumps(fields), name))
            conn.commit()
        finally:
            conn.close()

    def get_templates(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT name, zpl_format, fields FROM templates")
        rows = cursor.fetchall()
        conn.close()
        
        templates = []
        for row in rows:
            templates.append({
                "name": row[0],
                "zpl_format": row[1],
                "fields": json.loads(row[2])
            })
        return templates

    def delete_template(self, name):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("DELETE FROM templates WHERE name = ?", (name,))
        conn.commit()
        conn.close()

    def add_printer(self, name, ip_address, port=9100, description=""):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            cursor.execute("INSERT INTO printers (name, ip_address, port, description) VALUES (?, ?, ?, ?)",
                         (name, ip_address, port, description))
            conn.commit()
        finally:
            conn.close()

    def get_printers(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT name, ip_address, port, description FROM printers")
        rows = cursor.fetchall()
        conn.close()
        
        printers = []
        for row in rows:
            printers.append({
                "name": row[0],
                "ip_address": row[1],
                "port": row[2],
                "description": row[3]
            })
        return printers

    def delete_printer(self, name):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("DELETE FROM printers WHERE name = ?", (name,))
        conn.commit()
        conn.close()

    def update_printer(self, name, ip_address, port, description):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("UPDATE printers SET ip_address = ?, port = ?, description = ? WHERE name = ?",
                     (ip_address, port, description, name))
        conn.commit()
        conn.close()

    def log_print_job(self, template_name, printer_name, quantity, data, status='pending', serials=None, label_date=None, job_type='normal', printed_by=None):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute('''
            INSERT INTO print_jobs (
                template_name, printer_name, quantity, data, status, serials, label_date, job_type, printed_by
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)
        ''', (template_name, printer_name, quantity, json.dumps(data), status, serials, label_date, job_type, printed_by))
        job_id = cursor.lastrowid
        conn.commit()
        conn.close()
        return job_id

    def update_print_job_status(self, job_id, status):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("UPDATE print_jobs SET status = ? WHERE id = ?", (status, job_id))
        conn.commit()
        conn.close()
        
    def get_next_master_serial(self, base_prefix):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        all_serials = []
        
        cursor.execute("SELECT serials FROM print_jobs WHERE serials LIKE ?", ('%' + base_prefix + '%',))
        for row in cursor.fetchall():
            if row[0]:
                all_serials.append(row[0])
                
        # Buscar en scanned_serials
        cursor.execute("SELECT master_serial FROM scanned_serials WHERE master_serial LIKE ?", ('%' + base_prefix + '%',))
        for row in cursor.fetchall():
            if row[0]:
                all_serials.append(row[0])
        
        conn.close()
        
        max_count = 0
        for raw_s in all_serials:
            # Limpiar posibles formatos (braces, espacios, listas separadas por coma)
            s_clean = raw_s.replace('{', '').replace('}', '').strip()
            # Si es una lista, procesar cada elemento
            parts = [p.strip() for p in s_clean.split(',')]
            for part in parts:
                if part.startswith(base_prefix):
                    try:
                        # Extraer los últimos 4 caracteres y convertir a int
                        count_val = int(part[-4:])
                        if count_val > max_count:
                            max_count = count_val
                    except (ValueError, IndexError):
                        continue
        
        return str(max_count + 1).zfill(4)

    def log_scanned_serials(self, job_id, master_serial, scanned_list):
        if not scanned_list:
            return
            
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            for code in scanned_list:
                cursor.execute(
                    "INSERT INTO scanned_serials (job_id, master_serial, scanned_code) VALUES (?, ?, ?)",
                    (job_id, master_serial, code)
                )
            conn.commit()
        finally:
            conn.close()

    def check_fuzzy_serial_duplicate(self, part_no_prefix, last_5_digits):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            # Query fuzzy usando SUBSTR para precisión quirúrgica
            cursor.execute("""
                SELECT scanned_code 
                FROM scanned_serials 
                WHERE SUBSTR(scanned_code, 1, ?) = ? 
                AND SUBSTR(scanned_code, -5) = ?
                LIMIT 1
            """, (len(part_no_prefix), part_no_prefix, last_5_digits))
            
            result = cursor.fetchone()
            return result[0] if result else None
        finally:
            conn.close()

    def add_label_preset(self, template_name, preset_name, data):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            cursor.execute("INSERT OR REPLACE INTO label_presets (template_name, preset_name, data) VALUES (?, ?, ?)",
                         (template_name, preset_name, json.dumps(data)))
            conn.commit()
        finally:
            conn.close()

    def get_label_presets(self, template_name):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT preset_name, data FROM label_presets WHERE template_name = ?", (template_name,))
        rows = cursor.fetchall()
        conn.close()
        
        presets = []
        for row in rows:
            presets.append({
                "preset_name": row[0],
                "data": json.loads(row[1])
            })
        return presets

    def delete_label_preset(self, template_name, preset_name):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("DELETE FROM label_presets WHERE template_name = ? AND preset_name = ?", (template_name, preset_name))
        conn.commit()
        conn.close()

    def save_api_url(self, api_url):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("INSERT OR REPLACE INTO settings (key, value) VALUES ('api_url', ?)", (api_url,))
        conn.commit()
        conn.close()

    def get_api_url(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT value FROM settings WHERE key = 'api_url'")
        result = cursor.fetchone()
        conn.close()
        return result[0] if result else "http://localhost:5000/print"

    # ==================== GESTIÓN DE USUARIOS ====================
    def add_user(self, username, password, role='user'):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            cursor.execute("INSERT INTO users (username, password, role) VALUES (?, ?, ?)",
                         (username, password, role))
            conn.commit()
            return True
        except sqlite3.IntegrityError:
            return False
        finally:
            conn.close()

    def get_users(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT id, username, role FROM users")
        rows = cursor.fetchall()
        conn.close()
        return [{"id": r[0], "username": r[1], "role": r[2]} for r in rows]

    def delete_user(self, user_id):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("DELETE FROM users WHERE id = ?", (user_id,))
        conn.commit()
        conn.close()

    def validate_login(self, username, password):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT role FROM users WHERE username = ? AND password = ?", (username, password))
        result = cursor.fetchone()
        conn.close()
        return result[0] if result else None

    # ==================== HISTORIAL DE IMPRESIÓN ====================
    def get_print_history(self, template_name=None, search_type=None, search_value=None):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        # Verificar qué columna de fecha existe
        cursor.execute("PRAGMA table_info(print_jobs)")
        columns = [col[1] for col in cursor.fetchall()]
        date_col = "timestamp" if "timestamp" in columns else ("created_at" if "created_at" in columns else "id")
        
        # Función para normalizar texto (solo alfanuméricos)
        def normalize_text(text):
            if text is None: return ""
            return re.sub(r'[^A-Za-z0-9]', '', str(text)).upper()
        
        # Registrar la función en la conexión actual
        conn.create_function("NORMALIZE", 1, normalize_text)
        
        query = f"SELECT id, {date_col}, template_name, serials, printed_by, status, data FROM print_jobs"
        params = []
        conditions = []
        
        if template_name:
            conditions.append("template_name = ?")
            params.append(template_name)
            
        if search_type and search_value:
            s_val = normalize_text(search_value)
            if search_type == "Serial":
                # Buscar usando la función personalizada NORMALIZE
                conditions.append("NORMALIZE(serials) LIKE ?")
                params.append(f"%{s_val}%")
            elif search_type == "Material":
                # Buscar en la subconsulta usando NORMALIZE
                id_query = "SELECT job_id FROM scanned_serials WHERE NORMALIZE(scanned_code) LIKE ?"
                conditions.append(f"id IN ({id_query})")
                params.append(f"%{s_val}%")
                
        if conditions:
            query += " WHERE " + " AND ".join(conditions)
        
        query += " ORDER BY timestamp DESC"
        
        cursor.execute(query, params)
        rows = cursor.fetchall()
        conn.close()
        
        history = []
        for row in rows:
            history.append({
                "id": row[0],
                "timestamp": row[1],
                "template_name": row[2],
                "serials": row[3],
                "printed_by": row[4],
                "status": row[5],
                "data": json.loads(row[6]) if row[6] else {}
            })
        return history

    def get_scanned_serials_by_master_serial(self, master_serial):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT scanned_code FROM scanned_serials WHERE master_serial = ?", (master_serial,))
        rows = cursor.fetchall()
        conn.close()
        return [row[0] for row in rows]

    def get_scanned_serials_by_job_id(self, job_id):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        cursor.execute("SELECT scanned_code FROM scanned_serials WHERE job_id = ?", (job_id,))
        rows = cursor.fetchall()
        conn.close()
        return [row[0] for row in rows]

    def clear_history(self, template_name=None):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        try:
            if template_name:
                # Obtener IDs de trabajos a eliminar
                cursor.execute("SELECT id FROM print_jobs WHERE template_name = ?", (template_name,))
                job_ids = [row[0] for row in cursor.fetchall()]
                
                if job_ids:
                    placeholders = ', '.join(['?'] * len(job_ids))
                    cursor.execute(f"DELETE FROM scanned_serials WHERE job_id IN ({placeholders})", job_ids)
                    cursor.execute(f"DELETE FROM print_jobs WHERE id IN ({placeholders})", job_ids)
            else:
                cursor.execute("DELETE FROM scanned_serials")
                cursor.execute("DELETE FROM print_jobs")
            conn.commit()
        finally:
            conn.close()

