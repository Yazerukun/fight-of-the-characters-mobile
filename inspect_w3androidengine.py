import urllib.request
import json

def fetch_json(endpoint):
    url = f"https://api.github.com/repos/nikoltu/W3AndroidEngine/{endpoint}"
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode())

try:
    contents = fetch_json("contents")
    print("=== ROOT CONTENTS ===")
    for item in contents:
        print(f"  {item['type']:5} {item['name']}")

    readme = fetch_json("readme")
    import base64
    content = base64.b64decode(readme['content']).decode('utf-8', errors='ignore')
    print("\n=== README (FIRST 40 LINES) ===")
    for line in content.splitlines()[:40]:
        print(line)

    commits = fetch_json("commits?per_page=5")
    print("\n=== RECENT COMMITS ===")
    for c in commits:
        print(f"  {c['sha'][:8]} - {c['commit']['message'].splitlines()[0]} ({c['commit']['author']['date']})")

except Exception as e:
    print("Error:", e)
