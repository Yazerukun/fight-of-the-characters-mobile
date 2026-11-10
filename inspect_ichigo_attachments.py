import struct

with open(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx', 'rb') as f:
    d = f.read()

pos = 4
chunks = {}
while pos + 8 <= len(d):
    tag, sz = struct.unpack('<4sI', d[pos:pos+8])
    pos += 8
    chunks[tag.decode('latin1')] = d[pos:pos+sz]
    pos += sz

print("=== ICHIGO MDX CHUNKS ===")
for k in chunks:
    print(f"Chunk: {k}, size={len(chunks[k])}")

if 'ATCH' in chunks:
    adata = chunks['ATCH']
    p = 0
    print("\n--- ATTACHMENT NODES IN ICHIGO ---")
    while p < len(adata):
        inc_sz = struct.unpack('<I', adata[p:p+4])[0]
        name = adata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
        obj_id, parent_id = struct.unpack('<II', adata[p+84:p+92])
        path_data = adata[p+96:p+96+260].split(b'\x00')[0].decode('latin1', errors='ignore')
        print(f"Node obj_id={obj_id}, parent={parent_id}, name='{name}', path='{path_data}', inc_sz={inc_sz}")
        p += inc_sz

if 'RIBB' in chunks:
    rdata = chunks['RIBB']
    print(f"\n--- RIBBON EMITTERS (WEAPON TRAILS) IN ICHIGO (len={len(rdata)}) ---")
