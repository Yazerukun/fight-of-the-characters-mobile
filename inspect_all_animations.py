import struct

HEROES = [
    ('Ichigo', r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx'),
    ('Saber', r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0237.mdx'),
    ('RockLee', r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0208.mdx'),
    ('Luffy', r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0190.mdx'),
]

for hero_name, mdx_path in HEROES:
    with open(mdx_path, 'rb') as f:
        d = f.read()

    pos = 4
    chunks = {}
    while pos + 8 <= len(d):
        tag, sz = struct.unpack('<4sI', d[pos:pos+8])
        pos += 8
        chunks[tag.decode('latin1')] = d[pos:pos+sz]
        pos += sz

    total_t_keys = 0
    total_r_keys = 0
    animated_nodes = 0

    for chunk_name in ['BONE', 'HELP']:
        if chunk_name not in chunks: continue
        cdata = chunks[chunk_name]
        p = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            
            p_t = p + 96
            has_anim = False
            while p_t + 16 <= p + inc_sz:
                tag = cdata[p_t:p_t+4]
                if tag in (b'KGTR', b'KGRT', b'KGSC'):
                    nr_keys, interp, glob_seq = struct.unpack('<III', cdata[p_t+4:p_t+16])
                    key_sz = 16 if tag == b'KGTR' else (20 if tag == b'KGRT' else 16)
                    if interp > 1:
                        key_sz = 40 if tag in (b'KGTR', b'KGSC') else 52
                    if nr_keys > 0:
                        has_anim = True
                        if tag == b'KGTR': total_t_keys += nr_keys
                        if tag == b'KGRT': total_r_keys += nr_keys
                    p_t += 16 + nr_keys * key_sz
                else:
                    break
            if has_anim:
                animated_nodes += 1
            p += inc_sz + (8 if chunk_name == 'BONE' else 0)

    print(f"[{hero_name}] Animated Nodes: {animated_nodes}, Trans Keys: {total_t_keys}, Rot Keys: {total_r_keys}")
