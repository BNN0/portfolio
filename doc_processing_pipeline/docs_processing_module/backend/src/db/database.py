import sqlite3
from datetime import datetime

DATABASE_PATH = "src\db\pdf_analyzer.db"

# ============================================
# DATABASE INITIALIZATION
# ============================================

async def create_db():
    """Create database and all required tables"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        
        # File status table
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS file_status (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                filename TEXT NOT NULL,
                object_name TEXT UNIQUE NOT NULL,
                bucket_name TEXT NOT NULL,
                size_bytes INTEGER,
                status TEXT DEFAULT 'pending',
                has_modified INTEGER DEFAULT 0,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
        # Split files table - CRITICAL FOR SPLIT FUNCTIONALITY
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS split_files (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                original_object TEXT NOT NULL,
                part1_name TEXT NOT NULL,
                part2_name TEXT NOT NULL,
                bucket_name TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                UNIQUE(original_object, bucket_name)
            )
        ''')
        
        # Need modify table
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS need_modify (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                original_file TEXT NOT NULL,
                filename TEXT NOT NULL,
                object_name TEXT UNIQUE NOT NULL,
                bucket_name TEXT NOT NULL,
                status TEXT DEFAULT 'pending',
                size_bytes INTEGER,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
        # Prompts table
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS prompts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                prompt TEXT NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
        conn.commit()
        conn.close()
        print("✅ Database initialized successfully")
        return True
        
    except Exception as e:
        print(f"❌ Error initializing database: {e}")
        import traceback
        traceback.print_exc()
        return False

# ============================================
# FILE STATUS OPERATIONS
# ============================================

async def insert_file_status(filename: str, object_name: str, bucket_name: str, size_bytes: int = 0):
    """Insert file status into database"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        
        cursor.execute(
            '''INSERT INTO file_status (filename, object_name, bucket_name, size_bytes, status) 
               VALUES (?, ?, ?, ?, 'pending')''',
            (filename, object_name, bucket_name, size_bytes)
        )
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error inserting file status: {e}")
        return False

async def get_file_status(objectname: str):
    """Get file status from database"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM file_status WHERE object_name = ?', (objectname,))
        result = cursor.fetchone()
        conn.close()
        return result
    except Exception as e:
        print(f"❌ Error getting file status: {e}")
        return None

async def update_file_status(filename: str, status: str, object: str, bucket: str, modified: int = 0):
    """Update file status"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute(
            '''UPDATE file_status 
               SET status = ?, has_modified = ?, updated_at = CURRENT_TIMESTAMP 
               WHERE filename = ? AND object_name = ? AND bucket_name = ?''',
            (status, modified, filename, object, bucket)
        )
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error updating file status: {e}")
        return False

# ============================================
# SPLIT FILES OPERATIONS - NEW
# ============================================

async def insert_split_info(original_object: str, part1_name: str, part2_name: str, bucket_name: str):
    """Register that a file was split"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        
        cursor.execute(
            '''INSERT OR REPLACE INTO split_files 
               (original_object, part1_name, part2_name, bucket_name, created_at) 
               VALUES (?, ?, ?, ?, CURRENT_TIMESTAMP)''',
            (original_object, part1_name, part2_name, bucket_name)
        )
        conn.commit()
        conn.close()
        print(f"✅ Split info saved: {original_object}")
        return True
    except Exception as e:
        print(f"❌ Error inserting split info: {e}")
        return False

async def get_split_info(original_object: str, bucket_name: str):
    """Get split file info"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute(
            '''SELECT part1_name, part2_name FROM split_files 
               WHERE original_object = ? AND bucket_name = ?''',
            (original_object, bucket_name)
        )
        result = cursor.fetchone()
        conn.close()
        return result
    except Exception as e:
        print(f"❌ Error getting split info: {e}")
        return None

# ============================================
# NEED MODIFY OPERATIONS
# ============================================

async def insert_need_modify(original_file: str, filename: str, object_name: str, bucket_name: str, status: str, size_bytes: int):
    """Insert into need_modify table"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute(
            '''INSERT INTO need_modify (original_file, filename, object_name, bucket_name, status, size_bytes) 
               VALUES (?, ?, ?, ?, ?, ?)''',
            (original_file, filename, object_name, bucket_name, status, size_bytes)
        )
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error inserting need_modify: {e}")
        return False

async def get_need_modify(objectname: str):
    """Get need_modify record"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM need_modify WHERE object_name = ?', (objectname,))
        result = cursor.fetchone()
        conn.close()
        return result
    except Exception as e:
        print(f"❌ Error getting need_modify: {e}")
        return None

async def update_need_modify(filename: str, status: str, object: str, bucket: str):
    """Update need_modify record"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute(
            '''UPDATE need_modify 
               SET status = ?, updated_at = CURRENT_TIMESTAMP 
               WHERE filename = ? AND object_name = ? AND bucket_name = ?''',
            (status, filename, object, bucket)
        )
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error updating need_modify: {e}")
        return False

async def delete_need_modify(object: str, bucket: str):
    """Delete need_modify record"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute('DELETE FROM need_modify WHERE object_name = ? AND bucket_name = ?', (object, bucket))
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error deleting need_modify: {e}")
        return False

# ============================================
# PROMPT OPERATIONS
# ============================================

async def insert_prompt(prompt: str):
    """Insert prompt"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute('INSERT INTO prompts (prompt) VALUES (?)', (prompt,))
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error inserting prompt: {e}")
        return False

async def select_prompt():
    """Get latest prompt"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute('SELECT * FROM prompts ORDER BY id DESC LIMIT 1')
        result = cursor.fetchone()
        conn.close()
        return result
    except Exception as e:
        print(f"❌ Error selecting prompt: {e}")
        return None

async def update_prompt(prompt: str):
    """Update latest prompt"""
    try:
        conn = sqlite3.connect(DATABASE_PATH)
        cursor = conn.cursor()
        cursor.execute(
            'UPDATE prompts SET prompt = ?, updated_at = CURRENT_TIMESTAMP WHERE id = (SELECT MAX(id) FROM prompts)',
            (prompt,)
        )
        conn.commit()
        conn.close()
        return True
    except Exception as e:
        print(f"❌ Error updating prompt: {e}")
        return False