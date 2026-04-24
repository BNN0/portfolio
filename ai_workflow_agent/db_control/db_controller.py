import os
from psycopg_pool import ConnectionPool
from psycopg import Connection
from langgraph.checkpoint.postgres import PostgresSaver

#CREDENCIALES DE DB


def get_URI():
    DB_URI = '''postgresql://{DB_USER}:{DB_PASSWORD}@{DB_HOST}:{DB_PORT}/{DB_DATABASE}?sslmode=require'''.format(
        DB_USER=os.getenv('DB_USER'),
        DB_PASSWORD=os.getenv('DB_PASSWORD'),
        DB_HOST=os.getenv('DB_HOST'),
        DB_PORT=os.getenv('DB_PORT'),
        DB_DATABASE=os.getenv('DB_DATABASE')
    )
    return DB_URI

connection_kwargs = {
        "autocommit": True,
        "prepare_threshold": 0,
    }

async def setup_checkpoint_db():
    try:
        with Connection.connect(get_URI(), **connection_kwargs) as conn:
            memory = PostgresSaver(conn)
            memory.setup()
    except Exception as e:
        print("Error in setup_checkpoint_db", str(e)) 

def insert_messages_to_db(thread_id, message_user, message_bot):
    """Función que inserta mensajes en la base de datos.
    Inserta los mensajes si existe el usuario y el chat, de no ser asi, genera el usuario, el chat y el historial de mensajes"""
    try:
        with ConnectionPool(conninfo=get_URI(), max_size=20, kwargs=connection_kwargs, open=False) as pool:
            with pool.connection() as conn:

                conn.execute('SET TIME ZONE "America/Mexico_City";')

                conn.execute(f"""
                    DO $$
                        BEGIN
                            IF EXISTS (
                                SELECT 1 
                                FROM users 
                                WHERE thread_id = '{thread_id}'
                                AND EXISTS (
                                    SELECT 1 
                                    FROM chats 
                                    WHERE user_id = (SELECT id FROM users WHERE thread_id = '{thread_id}')
                                )
                            )
                            THEN
                                INSERT INTO messages(chat_id, created_at, send_by, content) 
                                VALUES (
                                    (SELECT id FROM chats WHERE user_id = (SELECT id FROM users WHERE thread_id = '{thread_id}')), 
                                    CURRENT_TIMESTAMP, 
                                    'user', 
                                    '{message_user}'
                                );
                                INSERT INTO messages(chat_id, created_at, send_by, content) 
                                VALUES (
                                    (SELECT id FROM chats WHERE user_id = (SELECT id FROM users WHERE thread_id = '{thread_id}')), 
                                    CURRENT_TIMESTAMP, 
                                    'bot', 
                                    '{message_bot}'
                                );
                            ELSE
                                INSERT INTO users(thread_id, name, created_at) 
                                VALUES ('{thread_id}', NULL, CURRENT_TIMESTAMP);
                                INSERT INTO chats(user_id) 
                                VALUES ((SELECT id FROM users WHERE thread_id = '{thread_id}'));
                                INSERT INTO messages(chat_id, created_at, send_by, content) 
                                VALUES (
                                    (SELECT id FROM chats WHERE user_id = (SELECT id FROM users WHERE thread_id = '{thread_id}')), 
                                    CURRENT_TIMESTAMP, 
                                    'user', 
                                    '{message_user}'
                                );
                                INSERT INTO messages(chat_id, created_at, send_by, content) 
                                VALUES (
                                    (SELECT id FROM chats WHERE user_id = (SELECT id FROM users WHERE thread_id = '{thread_id}')), 
                                    CURRENT_TIMESTAMP, 
                                    'bot', 
                                    '{message_bot}'
                                );
                            END IF;
                        END;
                        $$

                """)
    except Exception as e:
        print("Error in insert_messages_to_db: ", str(e)) 
