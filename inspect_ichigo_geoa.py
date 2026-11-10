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

if 'GEOA' in chunks:
    gadata = chunks['GEOA']
    p = 0
    geoa_idx = 0
    print(f"=== GEOA CHUNK (len={len(gadata)}) ===")
    while p < len(gadata):
        inc_sz = struct.unpack('<I', gadata[p:p+4])[0]
        alpha = struct.unpack('<f', gadata[p+4:p+8])[0]
        flags, color_r, color_g, color_b = struct.unpack('<Ifff', gadata[p+8:p+24])
        geoset_id = struct.unpack('<I', gadata[p+24:p+28])[0]
        print(f"GEOA {geoa_idx}: geosetId={geoset_id}, baseAlpha={alpha}, flags={flags}, inc_sz={inc_sz}")
        
        # Check for tracks inside this GEOA
        p_track = p + 28
        while p_track + 16 <= p + inc_sz:
            tag = gadata[p_track:p_track+4]
            if tag in (b'KGAL', b'KGAC'):
                nr_keys, interp, glob_seq = struct.unpack('<III', gadata[p_track+4:p_track+16])
                print(f"   Track {tag.decode('latin1')}: keys={nr_keys}, interp={interp}")
                # KGAL keys are (time, alpha:float)
                key_sz = 8 if interp <= 1 else 16
                p_k = p_track + 16
                for k in range(min(nr_keys, 10)):
                    t, val = struct.unpack('<If', gadata[p_k:p_k+8])
                    print(f"     key {k}: time={t}, alpha={val}")
                    p_k += key_sz
                p_track += 16 + nr_keys * key_sz
            else:
                p_track += 4
        p += inc_sz
        geoa_idx += 1
