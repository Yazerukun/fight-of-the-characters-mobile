import struct
import os

def test_skinning(path):
    print(f"=== TESTING SKINNING IN {os.path.basename(path)} ===")
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

    # Parse PIVT
    pivots = []
    if 'PIVT' in chunks:
        pdata = chunks['PIVT']
        pivots = [struct.unpack('<3f', pdata[i*12:(i+1)*12]) for i in range(len(pdata)//12)]

    # Parse BONE
    bones = []
    if 'BONE' in chunks:
        cdata = chunks['BONE']
        p = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            geoset_id, geoset_anim_id = struct.unpack('<II', cdata[p+inc_sz:p+inc_sz+8])
            bones.append({
                'name': name if name else f"Bone_{obj_id}",
                'obj_id': obj_id,
                'parent_id': parent_id,
                'pivot': pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            })
            p += inc_sz + 8

    print(f"Total Bones: {len(bones)}")

    # Parse GEOS skinning
    if 'GEOS' in chunks:
        cdata = chunks['GEOS']
        p = 0
        g_idx = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            gdata = cdata[p:p+inc_sz]
            p += inc_sz

            sp = 4
            # VRTX
            sp += 4
            v_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + v_cnt * 12
            # NRMS
            sp += 4
            n_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + n_cnt * 12
            # PTYP
            sp += 4
            pt_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + pt_cnt * 4
            # PCNT
            sp += 4
            pc_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + pc_cnt * 4
            # PVTX
            sp += 4
            pv_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + pv_cnt * 2
            # GNDX (Vertex Groups)
            sp += 4
            gndx_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            v_groups = list(gdata[sp+4:sp+4+gndx_cnt])
            sp += 4 + gndx_cnt
            # MTGC (Matrix Group sizes)
            sp += 4
            mtgc_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            mtgc = struct.unpack(f'<{mtgc_cnt}I', gdata[sp+4:sp+4+mtgc_cnt*4])
            sp += 4 + mtgc_cnt * 4
            # MATS (Matrix Indexes)
            sp += 4
            mats_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            mats = struct.unpack(f'<{mats_cnt}I', gdata[sp+4:sp+4+mats_cnt*4])
            sp += 4 + mats_cnt * 4

            # Build matrix groups:
            matrix_groups = []
            m_offset = 0
            for sz in mtgc:
                matrix_groups.append(mats[m_offset : m_offset + sz])
                m_offset += sz

            # Map vertices to bones
            vertex_bones = []
            for vg in v_groups:
                if vg < len(matrix_groups):
                    vertex_bones.append(matrix_groups[vg])
                else:
                    vertex_bones.append([0])

            print(f"Geoset {g_idx}: {v_cnt} verts, {len(matrix_groups)} matrix groups.")
            unique_bone_ids = set()
            for b_list in vertex_bones:
                unique_bone_ids.update(b_list)
            print(f"  Referenced Bone IDs ({len(unique_bone_ids)}): {sorted(list(unique_bone_ids))[:15]}")
            g_idx += 1

test_skinning(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx')
test_skinning(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0237.mdx')
test_skinning(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0208.mdx')
test_skinning(r'D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0190.mdx')
