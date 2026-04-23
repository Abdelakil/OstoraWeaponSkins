"""Fix ConPort's pre-existing FTS schema bug.

The custom_data_fts virtual table was created with content="custom_data",
which makes FTS5 look for a 'value_text' column in custom_data on reads.
The base table has 'value', not 'value_text', so FTS MATCH queries fail.

We rebuild custom_data_fts as a contentless-style FTS (standalone storage)
and repopulate it from custom_data. Existing INSERT/UPDATE/DELETE triggers
already write the correct rows, so they continue to work.
"""
import sqlite3

conn = sqlite3.connect('context_portal/context.db')
cur = conn.cursor()

# Same fix for decisions_fts which has an analogous issue (no failure observed,
# but it has content="decisions"; decisions table HAS all columns so it's fine).

print("Dropping and recreating custom_data_fts (standalone) ...")
cur.executescript("""
DROP TABLE IF EXISTS custom_data_fts;
CREATE VIRTUAL TABLE custom_data_fts USING fts5(
    category,
    key,
    value_text
);
""")

print("Repopulating from custom_data ...")
cur.execute("SELECT id, category, key, value FROM custom_data")
rows = [(r[0], r[1], r[2], r[3]) for r in cur.fetchall()]
cur.executemany(
    "INSERT INTO custom_data_fts (rowid, category, key, value_text) VALUES (?, ?, ?, ?)",
    rows,
)
conn.commit()
print(f"  Inserted {len(rows)} rows.")

# Test
cur.execute(
    "SELECT category, key FROM custom_data_fts WHERE custom_data_fts MATCH ? LIMIT 5",
    ("supplier risk",),
)
print("\nFTS 'supplier risk' sample:")
for r in cur.fetchall():
    print(" ", r)

cur.execute(
    "SELECT category, key FROM custom_data_fts WHERE custom_data_fts MATCH ? LIMIT 5",
    ("approval workflow",),
)
print("\nFTS 'approval workflow' sample:")
for r in cur.fetchall():
    print(" ", r)

conn.close()
print("\nDone.")
