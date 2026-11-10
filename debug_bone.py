import struct

with open(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx', 'rb') as f:
    d = f.read()

pos = 4
while pos + 8 <= len(d):
    tag, sz = struct.unpack('<4sI', d[pos:pos+8])
    pos += 8
    cdata = d[pos:pos+sz]
    pos += sz
    if tag == b'BONE':
        print(f"BONE chunk size: {sz}")
        print("First 160 bytes hex:")
        print(cdata[:160].hex())
        inc_sz = struct.unpack('<I', cdata[:4])[0]
        name = cdata[4:84].split(b'\x00')[0]
        obj_id, parent_id, flags = struct.unpack('<III', cdata[84:96])
        print(f"inc_sz: {inc_sz}, name: {name}, obj_id: {obj_id}, parent: {parent_id}, flags: {hex(flags)}")
        # Check what follows at offset 96
        print("At offset 96:", cdata[96:120])
        break
