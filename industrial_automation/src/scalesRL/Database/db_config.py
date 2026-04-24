import sqlite3

# Base de datos SQLite
def init_database():
    conn = sqlite3.connect('scale_config.db')
    cursor = conn.cursor()
    #Tabla para configuración de basculas registradas
    cursor.execute('''
        CREATE TABLE IF NOT EXISTS scales (
            scale_id TEXT PRIMARY KEY,
            model TEXT NOT NULL,
            connection_type TEXT NOT NULL,
            address TEXT NOT NULL,
            port INTEGER,
            baudrate INTEGER,
            created_at DATETIME DEFAULT CURRENT_TIMESTAMP
        )
    ''')
    conn.commit()
    conn.close()
