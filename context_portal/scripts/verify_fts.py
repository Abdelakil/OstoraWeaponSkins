import sqlite3
conn = sqlite3.connect('context_portal/context.db')
cur = conn.cursor()

# Raw FTS test
cur.execute("SELECT category, key FROM custom_data_fts WHERE custom_data_fts MATCH 'supplier risk' LIMIT 5")
print("FTS 'supplier risk':")
for r in cur.fetchall():
    print(" ", r)

cur.execute("SELECT category, key FROM custom_data_fts WHERE custom_data_fts MATCH 'approval workflow' LIMIT 5")
print("\nFTS 'approval workflow':")
for r in cur.fetchall():
    print(" ", r)

# Stats
cur.execute("SELECT COUNT(*) FROM custom_data_fts")
print(f"\ncustom_data_fts row count: {cur.fetchone()[0]}")
