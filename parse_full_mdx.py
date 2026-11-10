import struct
import os

def parse_full_mdx(path):
    filename = os.path.basename(path)
    print(f"==================================================")
    print(f"PARSING FULL MDX HIERARCHY: {filename}")
    print(f"==================================================")

    with open(path, 'rb') as f:
        d = f.read()

    pos = 4
    chunks = {}
    while pos + 8 <= len(d):
        tag, sz = struct.unpack('<4sI', d[pos:pos+8])
        pos += 8
        cdata = d[pos:pos+sz]
        pos += sz
        chunks[tag.decode('latin1', errors='ignore')] = cdata

    # 1. PIVOTS
    pivots = []
    if 'PIVT' in chunks:
        pdata = chunks['PIVT']
        count = len(pdata) // 12
        for i in range(count):
            pivots.append(struct.unpack('<3f', pdata[i*12:(i+1)*12]))
    print(f"Total Pivot Points: {len(pivots)}")

    # 2. SEQUENCES
    sequences = []
    if 'SEQS' in chunks:
        sdata = chunks['SEQS']
        count = len(sdata) // 132
        for i in range(count):
            entry = sdata[i*132:(i+1)*132]
            name = entry[:80].split(b'\x00')[0].decode('latin1', errors='ignore')
            start, end, speed = struct.unpack('<IIf', entry[80:92])
            non_looping, rarity = struct.unpack('<If', entry[92:100])
            sequences.append({
                'name': name,
                'start': start,
                'end': end,
                'speed': speed,
                'non_looping': non_looping,
                'rarity': rarity
            })
    print(f"Total Animation Sequences: {len(sequences)}")
    for s in sequences:
        print(f"  Seq: '{s['name']}' ({s['start']}-{s['end']} ms, Speed: {s['speed']:.1f})")

    # 3. NODES (BONES, HELPERS, ATTACHMENTS)
    nodes = {}

    def parse_tracks(data, p_track, end_track):
        tracks = {}
        while p_track + 16 <= end_track:
            tag = data[p_track:p_track+4]
            if tag in (b'KGTR', b'KGRT', b'KGSC'):
                nr_keys, interp, glob_seq = struct.unpack('<III', data[p_track+4:p_track+16])
                tag_name = tag.decode('latin1')
                key_size = 0
                if tag in (b'KGTR', b'KGSC'):
                    key_size = 16 if interp <= 1 else 40
                elif tag == b'KGRT':
                    key_size = 20 if interp <= 1 else 52
                total_track_sz = 16 + nr_keys * key_size
                tracks[tag_name] = {'count': nr_keys, 'interp': interp, 'glob_seq': glob_seq}
                p_track += total_track_sz
            else:
                break
        return tracks

    # BONES
    if 'BONE' in chunks:
        cdata = chunks['BONE']
        p = 0
        bone_count = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            tracks = parse_tracks(cdata, p+96, p+inc_sz)
            geoset_id, geoset_anim_id = struct.unpack('<II', cdata[p+inc_sz:p+inc_sz+8])
            
            piv = pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            nodes[obj_id] = {
                'type': 'BONE',
                'name': name if name else f"Bone_{obj_id}",
                'obj_id': obj_id,
                'parent_id': parent_id if parent_id != 0xFFFFFFFF else -1,
                'flags': flags,
                'pivot': piv,
                'geoset_id': geoset_id,
                'tracks': tracks
            }
            bone_count += 1
            p += inc_sz + 8
        print(f"Total BONE nodes: {bone_count}")

    # HELPERS
    if 'HELP' in chunks:
        cdata = chunks['HELP']
        p = 0
        help_count = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            tracks = parse_tracks(cdata, p+96, p+inc_sz)
            
            piv = pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            nodes[obj_id] = {
                'type': 'HELP',
                'name': name if name else f"Helper_{obj_id}",
                'obj_id': obj_id,
                'parent_id': parent_id if parent_id != 0xFFFFFFFF else -1,
                'flags': flags,
                'pivot': piv,
                'tracks': tracks
            }
            help_count += 1
            p += inc_sz
        print(f"Total HELP nodes: {help_count}")

    # ATTACHMENTS
    if 'ATCH' in chunks:
        cdata = chunks['ATCH']
        p = 0
        atch_count = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            
            # extra path and atch_id are inside or after node header
            piv = pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            nodes[obj_id] = {
                'type': 'ATCH',
                'name': name if name else f"Attachment_{obj_id}",
                'obj_id': obj_id,
                'parent_id': parent_id if parent_id != 0xFFFFFFFF else -1,
                'flags': flags,
                'pivot': piv,
                'tracks': {}
            }
            atch_count += 1
            p += inc_sz
        print(f"Total ATCH nodes: {atch_count}")

    print(f"Combined Rig Nodes parsed: {len(nodes)}")
    
    # Print root bones and hierarchy
    print("\nRoot Bones & Skeletal Hierarchy:")
    roots = [n for n in nodes.values() if n['parent_id'] == -1]
    for r in roots:
        print(f"  [ROOT] Node {r['obj_id']:2d} ({r['type']}): '{r['name']}' @ Pivot {r['pivot']}")
        children = [n for n in nodes.values() if n['parent_id'] == r['obj_id']]
        for c in children:
            print(f"    -> Child {c['obj_id']:2d} ({c['type']}): '{c['name']}' @ Pivot {c['pivot']}")
            sub_children = [n for n in nodes.values() if n['parent_id'] == c['obj_id']]
            for sc in sub_children:
                print(f"       -> Sub-Child {sc['obj_id']:2d} ({sc['type']}): '{sc['name']}' @ Pivot {sc['pivot']}")

    print("\n")
    return {
        'filename': filename,
        'sequences': sequences,
        'nodes': nodes,
        'pivots': pivots
    }

root = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets"
for f in ['unknown_0186.mdx', 'unknown_0237.mdx', 'unknown_0208.mdx', 'unknown_0190.mdx']:
    parse_full_mdx(os.path.join(root, f))
