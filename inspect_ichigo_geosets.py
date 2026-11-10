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

gdata = chunks['GEOS']
sp = 0
geoset_idx = 0
while sp < len(gdata):
    sz = struct.unpack('<I', gdata[sp:sp+4])[0]
    next_geoset = sp + sz
    
    # Header: sz (4), VRTX (4), v_cnt (4)
    v_cnt = struct.unpack('<I', gdata[sp+8:sp+12])[0]
    curr = sp + 12 + v_cnt * 12
    # NRMS (4), n_cnt (4)
    n_cnt = struct.unpack('<I', gdata[curr+4:curr+8])[0]
    curr += 8 + n_cnt * 12
    # PTYP (4), pt_cnt (4)
    pt_cnt = struct.unpack('<I', gdata[curr+4:curr+8])[0]
    curr += 8 + pt_cnt * 4
    # PCNT (4), pc_cnt (4)
    pc_cnt = struct.unpack('<I', gdata[curr+4:curr+8])[0]
    curr += 8 + pc_cnt * 4
    # PVTX (4), pv_cnt (4)
    pv_cnt = struct.unpack('<I', gdata[curr+4:curr+8])[0]
    curr += 8 + pv_cnt * 2
    
    print(f"Geoset {geoset_idx}: size={sz}, vertices={v_cnt}, triangles={pv_cnt//3}")
    sp = next_geoset
    geoset_idx += 1

print(f"Total geosets: {geoset_idx}")
