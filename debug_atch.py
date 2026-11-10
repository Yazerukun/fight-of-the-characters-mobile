import struct

with open(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx', 'rb') as f:
    d = f.read()

pos = 4
while pos + 8 <= len(d):
    tag, sz = struct.unpack('<4sI', d[pos:pos+8])
    pos += 8
    cdata = d[pos:pos+sz]
    pos += sz
    if tag == b'ATCH':
        print(f"ATCH chunk size: {sz}")
        p = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0]
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            print(f"ATCH entry at {p}: inc_sz={inc_sz}, name={name}, obj_id={obj_id}, parent={parent_id}")
            # check remaining bytes after inc_sz
            remaining = len(cdata) - (p + inc_sz)
            print(f"  Remaining after inc_sz: {remaining}")
            # check what follows
            extra_slice = cdata[p+inc_sz:p+inc_sz+268]
            print(f"  Extra slice len: {len(extra_slice)}")
            p += inc_sz + 268
        break
