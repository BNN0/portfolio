import os
from pathlib import Path
import psycopg2
from psycopg2.extras import RealDictCursor

BASE_DIR = Path(__file__).resolve().parent
SQL_FILE = BASE_DIR / "crp.sql"


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


def init_db():
    if not SQL_FILE.exists():
        raise FileNotFoundError(f"SQL structure file not found: {SQL_FILE}")

    sql = SQL_FILE.read_text(encoding="utf-8")

    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute(sql)
        conn.commit()

def drop_tables():
    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute("DROP TABLE IF EXISTS supply_chain_data CASCADE;")
            cursor.execute("DROP TABLE IF EXISTS parts CASCADE;")
            cursor.execute("DROP TABLE IF EXISTS providers CASCADE;")
            cursor.execute("DROP TABLE IF EXISTS process_types CASCADE;")
        conn.commit()

def insert_provider(name):
    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute(
                "INSERT INTO providers (name) VALUES (%s)",
                (name,))
            conn.commit()

def insert_part(part_no, description, total, provider_name, stock_limit=0):
    provider = get_provider(provider_name)
    if not provider:
        provider = get_provider('NON PROVIDER')
        if not provider:
            raise ValueError(f"Provider '{provider_name}' not found in database.")
    provider_id = provider["id"]
    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute(
                "INSERT INTO parts (part_no, description, total, provider_id, stock_limit, created_at, updated_at) VALUES (%s, %s, %s, %s, %s, NOW(), NOW()) RETURNING id",
                (part_no, description, total, provider_id, stock_limit),
                )
            result = cursor.fetchone()
            conn.commit()
            return result["id"] if result else None

def get_provider(name):
    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute("SELECT id FROM providers WHERE name = %s", (name,))
            return cursor.fetchone()

def insert_status_quantity_for_all_parts(entry_date):
    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute("""INSERT INTO supply_chain_data (part_no, process_type_id, entry_date, quantity, created_at, updated_at)
                            SELECT id, 4, %s, total, NOW(), NOW()
                            FROM parts;""", (entry_date,))
        conn.commit()


def initialize_supply_chain_data(part_no_id, registers):
    if not registers:
        return

    sql_values = ""

    for register in registers:
        date = register["date"]
        process = register["process"]
        qty = register["qty"]
        sql_values += f"({part_no_id}, (SELECT id from process_types WHERE name = '{process}' LIMIT 1), '{date}', {qty}, NOW(), NOW()),"

    with get_connection() as conn:
        with conn.cursor() as cursor:
            cursor.execute(f"""INSERT INTO supply_chain_data (part_no, process_type_id, entry_date, quantity, created_at, updated_at)
                            VALUES {sql_values[:-1]}""")
        conn.commit()