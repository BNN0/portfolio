BEGIN TRANSACTION;
CREATE TABLE api_config (
                    id SERIAL PRIMARY KEY,
                    api_url TEXT NOT NULL,
                    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );
INSERT INTO "api_config" VALUES(1,'http://localhost:8090/print-zpl','2026-02-20 18:18:54');
CREATE TABLE label_presets (
                    id SERIAL PRIMARY KEY,
                    template_name TEXT NOT NULL,
                    preset_name TEXT NOT NULL,
                    data TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(template_name, preset_name)
                );

CREATE TABLE print_jobs (
                    id SERIAL PRIMARY KEY,
                    template_name TEXT NOT NULL,
                    printer_name TEXT NOT NULL,
                    quantity INTEGER,
                    data TEXT,
                    status TEXT DEFAULT 'pending',
                    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                , serials TEXT, label_date TEXT, job_type TEXT, printed_by TEXT);

CREATE TABLE printers (
                    id SERIAL PRIMARY KEY,
                    name TEXT UNIQUE NOT NULL,
                    ip_address TEXT,
                    port INTEGER DEFAULT 9100,
                    description TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

CREATE TABLE scanned_serials (
                id SERIAL PRIMARY KEY,
                job_id INTEGER,
                master_serial TEXT,
                scanned_code TEXT,
                FOREIGN KEY(job_id) REFERENCES print_jobs(id)
            );

CREATE TABLE settings (
                key TEXT PRIMARY KEY,
                value TEXT
            );
INSERT INTO "settings" VALUES('api_url','http://localhost:8090/print-zpl');
CREATE TABLE templates (
                    id SERIAL PRIMARY KEY,
                    name TEXT UNIQUE NOT NULL,
                    zpl_format TEXT NOT NULL,
                    fields TEXT NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );

CREATE TABLE users (
                id SERIAL PRIMARY KEY,
                username TEXT UNIQUE,
                password TEXT,
                role TEXT
            );
INSERT INTO "users" VALUES(1,'Admin','admin1234','admin');

COMMIT;
