import sqlite3
conn = sqlite3.connect('context_portal/context.db')
cur = conn.cursor()
cur.execute("SELECT name, sql FROM sqlite_master WHERE type='table'")
for r in cur.fetchall():
    print('TABLE:', r[0])
    print(r[1])
    print('---')
