DROP TABLE IF EXISTS "supply_chain_data";
DROP TABLE IF EXISTS "invoices";
DROP TABLE IF EXISTS "process_types";
DROP TABLE IF EXISTS "parts";
DROP TABLE IF EXISTS "providers";

CREATE TABLE IF NOT EXISTS "parts" (
  "id" serial PRIMARY KEY,
  "part_no" varchar,
  "description" varchar,
  "total" bigint,
  "stock_limit" bigint DEFAULT 0,
  "provider_id" integer,
  "acknowledged_until" date,
  "created_at" timestamp,
  "updated_at" timestamp
);

CREATE TABLE  IF NOT EXISTS "providers" (
  "id" serial PRIMARY KEY,
  "name" varchar
);

INSERT INTO "providers" ("name") VALUES ('NON PROVIDER');

CREATE TABLE  IF NOT EXISTS"process_types" (
  "id" serial PRIMARY KEY,
  "name" varchar
);

INSERT INTO "process_types" ("name") VALUES ('REQUIRED QUANTITY');
INSERT INTO "process_types" ("name") VALUES ('INCOMING DELIVERY');
INSERT INTO "process_types" ("name") VALUES ('ON THE WAY');
INSERT INTO "process_types" ("name") VALUES ('STATUS QUANTITY');

CREATE TABLE IF NOT EXISTS "invoices" (
  "id" serial PRIMARY KEY,
  "invoice_number" varchar UNIQUE,
  "provider_id" integer,
  "total_value" decimal,
  "created_at" timestamp,
  "updated_at" timestamp
);

CREATE TABLE IF NOT EXISTS "supply_chain_data" (
  "id" serial PRIMARY KEY,
  "part_no" integer,
  "process_type_id" integer,
  "entry_date" date,
  "quantity" integer,
  "invoice_id" integer,
  "created_at" timestamp,
  "updated_at" timestamp
);

ALTER TABLE "parts" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "supply_chain_data" ADD FOREIGN KEY ("part_no") REFERENCES "parts" ("id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "supply_chain_data" ADD FOREIGN KEY ("process_type_id") REFERENCES "process_types" ("id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "supply_chain_data" ADD FOREIGN KEY ("invoice_id") REFERENCES "invoices" ("id") DEFERRABLE INITIALLY IMMEDIATE;

ALTER TABLE "invoices" ADD FOREIGN KEY ("provider_id") REFERENCES "providers" ("id") DEFERRABLE INITIALLY IMMEDIATE;
