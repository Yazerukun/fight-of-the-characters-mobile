import os
import re

repo_dir = 'D:/W3AndroidEngine/repo'
file_requests = {}
pattern = re.compile(r'"([^"\r\n\t]+?\.[a-zA-Z0-9_]{1,6})"')

for root, dirs, files in os.walk(repo_dir):
    if any(x in root for x in ['build', '.git', 'gradle']):
        continue
    for f in files:
        if f.endswith('.java'):
            filepath = os.path.join(root, f)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as jf:
                lines = jf.readlines()
                for line_no, line in enumerate(lines, 1):
                    for m in pattern.findall(line):
                        if any(m.lower().endswith(ext) for ext in [
                            '.txt', '.slk', '.toc', '.fdf', '.j', '.mpq', '.blp',
                            '.mdx', '.ttf', '.w3e', '.wpm', '.doo', '.w3u', '.w3a',
                            '.w3t', '.w3q', '.w3b', '.w3i', '.wts', '.shd'
                        ]):
                            clean_path = m.replace('/', '\\')
                            rel_java = os.path.relpath(filepath, repo_dir)
                            if clean_path not in file_requests:
                                file_requests[clean_path] = []
                            file_requests[clean_path].append((rel_java, line_no))

print(f"Total unique file paths referenced: {len(file_requests)}")
print("\n=== SLK TABLES ===")
for k in sorted(file_requests.keys()):
    if k.lower().endswith('.slk'):
        refs = ", ".join([f"{r}:{ln}" for r, ln in file_requests[k][:3]])
        print(f"  {k} -> {refs}")

print("\n=== UI / TOC / FDF / SKINS ===")
for k in sorted(file_requests.keys()):
    if any(k.lower().endswith(x) for x in ['.toc', '.fdf', '.txt']) and ('ui' in k.lower() or 'skin' in k.lower() or 'frame' in k.lower()):
        refs = ", ".join([f"{r}:{ln}" for r, ln in file_requests[k][:3]])
        print(f"  {k} -> {refs}")

print("\n=== SCRIPTS / JASS ===")
for k in sorted(file_requests.keys()):
    if k.lower().endswith('.j') or 'script' in k.lower():
        refs = ", ".join([f"{r}:{ln}" for r, ln in file_requests[k][:3]])
        print(f"  {k} -> {refs}")

print("\n=== FONTS ===")
for k in sorted(file_requests.keys()):
    if k.lower().endswith('.ttf') or 'font' in k.lower():
        refs = ", ".join([f"{r}:{ln}" for r, ln in file_requests[k][:3]])
        print(f"  {k} -> {refs}")
