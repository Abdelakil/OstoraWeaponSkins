"""Rewrite custom_data triggers to work with standalone FTS5 table.

The original delete/update triggers used FTS5 external-content syntax
INSERT INTO custom_data_fts (custom_data_fts, rowid, ...) VALUES ('delete', ...),
which is not valid for our rebuilt standalone FTS. We replace them with
plain DELETE FROM custom_data_fts WHERE rowid = ...
"""
import sqlite3

conn = sqlite3.connect('context_portal/context.db')
cur = conn.cursor()

cur.executescript("""
DROP TRIGGER IF EXISTS custom_data_after_insert;
DROP TRIGGER IF EXISTS custom_data_after_delete;
DROP TRIGGER IF EXISTS custom_data_after_update;

CREATE TRIGGER custom_data_after_insert AFTER INSERT ON custom_data
BEGIN
    INSERT INTO custom_data_fts (rowid, category, key, value_text)
    VALUES (new.id, new.category, new.key, new.value);
END;

CREATE TRIGGER custom_data_after_delete AFTER DELETE ON custom_data
BEGIN
    DELETE FROM custom_data_fts WHERE rowid = old.id;
END;

CREATE TRIGGER custom_data_after_update AFTER UPDATE ON custom_data
BEGIN
    DELETE FROM custom_data_fts WHERE rowid = old.id;
    INSERT INTO custom_data_fts (rowid, category, key, value_text)
    VALUES (new.id, new.category, new.key, new.value);
END;
""")
conn.commit()
print("Triggers rewritten for standalone FTS.")
conn.close()
