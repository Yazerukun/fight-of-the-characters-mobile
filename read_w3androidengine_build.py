import urllib.request
import json
import base64

url = "https://api.github.com/repos/nikoltu/W3AndroidEngine/readme"
req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
with urllib.request.urlopen(req) as resp:
    data = json.loads(resp.read().decode())
    content = base64.b64decode(data['content']).decode('utf-8', errors='ignore')

# Print Building and Requirements sections
capture = False
for line in content.splitlines():
    if "## Requirements" in line or "## Building" in line or "## Feature status" in line:
        capture = True
    elif line.startswith("## ") and capture and not ("Requirements" in line or "Building" in line or "Feature" in line):
        capture = False
    if capture:
        print(line)
