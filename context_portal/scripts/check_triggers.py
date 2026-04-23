import sqlite3
conn = sqlite3.connect('context_portal/context.db')
cur = conn.cursor()
cur.execute("SELECT name, sql FROM sqlite_master WHERE type='trigger'")
for r in cur.fetchall():
    print('TRIGGER:', r[0])
    print(r[1])
    print('---')
cur.execute("SELECT COUNT(*) FROM custom_data")
print('custom_data count:', cur.fetchone()[0])
