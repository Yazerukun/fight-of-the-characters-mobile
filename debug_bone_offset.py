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
        print("At offset 188..212:")
        print(cdata[184:220])
        # Check if next bone starts at 192 or 200
        for off in [192, 200]:
            inc = struct.unpack('<I', cdata[off:off+4])[0]
            name = cdata[off+4:off+84].split(b'\x00')[0]
            print(f"Testing offset {off}: inc_sz={inc}, name={name}")
        break
