import struct
import os

def parse_node_header(data, pos):
    inc_sz, name, obj_id, parent_id, flags = struct.unpack('<I80sIII', data[pos:pos+96])
    name_str = name.split(b'\x00')[0].decode('latin1', errors='ignore')
    return {
        'inclusive_size': inc_sz,
        'name': name_str,
        'object_id': obj_id,
        'parent_id': parent_id,
        'flags': flags,
        'raw_header_size': 96
    }

def inspect_bones(path):
    print(f"=== BONES IN {os.path.basename(path)} ===")
    with open(path, 'rb') as f:
        d = f.read()
    pos = 4
    chunks = {}
    while pos + 8 <= len(d):
        tag, sz = struct.unpack('<4sI', d[pos:pos+8])
        pos += 8
        cdata = d[pos:pos+sz]
        pos += sz
        chunks[tag.decode('latin1')] = cdata

    if 'PIVT' in chunks:
        piv_data = chunks['PIVT']
        piv_count = len(piv_data) // 12
        pivots = [struct.unpack('<3f', piv_data[i*12:(i+1)*12]) for i in range(piv_count)]
        print(f"Total Pivot Points: {len(pivots)}")

    all_nodes = {}

    if 'BONE' in chunks:
        cdata = chunks['BONE']
        p = 0
        bones = []
        while p < len(cdata):
            nh = parse_node_header(cdata, p)
            inc_sz = nh['inclusive_size']
            bones.append(nh)
            all_nodes[nh['object_id']] = nh
            if inc_sz == 0: break
            p += inc_sz
        print(f"Total BONE nodes: {len(bones)}")
        for b in bones[:10]:
            parent_str = str(b['parent_id']) if b['parent_id'] != 0xFFFFFFFF else "ROOT"
            print(f"  BONE ID: {b['object_id']:2d} | Parent: {parent_str:6s} | Name: '{b['name']}'")

    if 'HELP' in chunks:
        cdata = chunks['HELP']
        p = 0
        helpers = []
        while p < len(cdata):
            nh = parse_node_header(cdata, p)
            helpers.append(nh)
            all_nodes[nh['object_id']] = nh
            if nh['inclusive_size'] == 0: break
            p += nh['inclusive_size']
        print(f"Total HELP nodes: {len(helpers)}")
        for h in helpers[:5]:
            parent_str = str(h['parent_id']) if h['parent_id'] != 0xFFFFFFFF else "ROOT"
            print(f"  HELP ID: {h['object_id']:2d} | Parent: {parent_str:6s} | Name: '{h['name']}'")

    if 'ATCH' in chunks:
        cdata = chunks['ATCH']
        p = 0
        attachments = []
        while p < len(cdata):
            nh = parse_node_header(cdata, p)
            attachments.append(nh)
            all_nodes[nh['object_id']] = nh
            if nh['inclusive_size'] == 0: break
            p += nh['inclusive_size']
        print(f"Total ATCH nodes: {len(attachments)}")
        for a in attachments[:5]:
            parent_str = str(a['parent_id']) if a['parent_id'] != 0xFFFFFFFF else "ROOT"
            print(f"  ATCH ID: {a['object_id']:2d} | Parent: {parent_str:6s} | Name: '{a['name']}'")

    print(f"Total Combined Nodes: {len(all_nodes)}\n")

root = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets"
for f in ['unknown_0186.mdx', 'unknown_0237.mdx', 'unknown_0208.mdx', 'unknown_0190.mdx']:
    inspect_bones(os.path.join(root, f))
