import struct

with open(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx', 'rb') as f:
    d = f.read()

pos = 4
while pos + 8 <= len(d):
    tag, sz = struct.unpack('<4sI', d[pos:pos+8])
    pos += 8
    cdata = d[pos:pos+sz]
    pos += sz
    if tag == b'GEOS':
        print(f"GEOS chunk total size: {sz}")
        p = 0
        geoset_idx = 0
        while p < len(cdata):
            inclusive_sz = struct.unpack('<I', cdata[p:p+4])[0]
            # Geoset header:
            # tag 'VRTX', vertexCount
            p_inner = p + 4
            vtag, vcount = struct.unpack('<4sI', cdata[p_inner:p_inner+8])
            p_inner += 8 + vcount * 12
            ntag, ncount = struct.unpack('<4sI', cdata[p_inner:p_inner+8])
            p_inner += 8 + ncount * 12
            ptag, pcount = struct.unpack('<4sI', cdata[p_inner:p_inner+8]) # primitives / faces
            p_inner += 8 + pcount * 4 # usually 1 primitive type (4)
            ftag, fcount = struct.unpack('<4sI', cdata[p_inner:p_inner+8]) # face indices
            p_inner += 8 + fcount * 2
            gtag, gcount = struct.unpack('<4sI', cdata[p_inner:p_inner+8]) # vertex groups
            vgroups = list(cdata[p_inner+8:p_inner+8+gcount])
            p_inner += 8 + gcount
            # Matrix groups
            mtag, mcount = struct.unpack('<4sI', cdata[p_inner:p_inner+8])
            mgroups = struct.unpack(f'<{mcount}I', cdata[p_inner+8:p_inner+8+mcount*4])
            p_inner += 8 + mcount * 4
            # Matrix indexes
            itag, icount = struct.unpack('<4sI', cdata[p_inner:p_inner+8])
            mindexes = struct.unpack(f'<{icount}I', cdata[p_inner+8:p_inner+8+icount*4])
            p_inner += 8 + icount * 4

            print(f"Geoset {geoset_idx}: {vcount} verts, {fcount//3} tris, {gcount} vgroups, {mcount} mgroups, {icount} mindexes")
            print(f"  Matrix indexes sample: {mindexes[:10]}")
            geoset_idx += 1
            p += inclusive_sz
        break
