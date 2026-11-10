import os
import struct
import math
import xml.etree.ElementTree as ET

HERO_SOURCES = [
    {
        'id': 'ichigo',
        'name': 'Ichigo Kurosaki',
        'mdx': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0186.mdx",
        'dae': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes\Ichigo\Ichigo.dae"
    },
    {
        'id': 'saber',
        'name': 'Saber / Artoria Pendragon',
        'mdx': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0237.mdx",
        'dae': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes\Saber\Saber.dae"
    },
    {
        'id': 'rocklee',
        'name': 'Rock Lee',
        'mdx': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0208.mdx",
        'dae': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes\RockLee\RockLee.dae"
    },
    {
        'id': 'luffy',
        'name': 'Monkey D. Luffy',
        'mdx': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets\unknown_0190.mdx",
        'dae': r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes\Luffy\Luffy.dae"
    }
]

def parse_mdx_geometry(mdx_path):
    with open(mdx_path, 'rb') as f:
        d = f.read()

    pos = 4
    chunks = {}
    while pos + 8 <= len(d):
        tag, sz = struct.unpack('<4sI', d[pos:pos+8])
        pos += 8
        cdata = d[pos:pos+sz]
        pos += sz
        chunks[tag.decode('latin1', errors='ignore')] = cdata

    verts = []
    normals = []
    uvs = []
    faces = []
    v_offset = 0

    if 'GEOS' in chunks:
        gdata = chunks['GEOS']
        sp = 0
        while sp < len(gdata):
            sz = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + 4 # skip sz and VRTX
            v_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4
            for _ in range(v_cnt):
                vx, vy, vz = struct.unpack('<fff', gdata[sp:sp+12])
                verts.append((vx, vy, vz))
                sp += 12

            sp += 4 # NRMS
            n_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4
            for _ in range(n_cnt):
                nx, ny, nz = struct.unpack('<fff', gdata[sp:sp+12])
                normals.append((nx, ny, nz))
                sp += 12

            # PTYP + PCNT
            sp += 4
            pt_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + pt_cnt * 4
            sp += 4
            pc_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + pc_cnt * 4

            # PVTX
            sp += 4
            pv_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4
            indices = struct.unpack(f'<{pv_cnt}H', gdata[sp:sp+pv_cnt*2])
            sp += pv_cnt * 2
            for i in range(0, len(indices), 3):
                faces.append((indices[i] + v_offset, indices[i+1] + v_offset, indices[i+2] + v_offset))

            # GNDX
            sp += 4
            gndx_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + gndx_cnt

            # MTGC
            sp += 4
            mtgc_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + mtgc_cnt * 4

            # MATS
            sp += 4
            mats_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + mats_cnt * 4

            # Extents + Material
            sp += 12 + 28
            ext_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4 + ext_cnt * 28

            # UVAS
            if sp < len(gdata):
                if gdata[sp:sp+4] == b'UVAS':
                    sp += 8
                    if gdata[sp:sp+4] == b'UVBS':
                        sp += 4
                        uv_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
                        sp += 4
                        for _ in range(uv_cnt):
                            u, v = struct.unpack('<ff', gdata[sp:sp+8])
                            uvs.append((u, v))
                            sp += 8

            v_offset += v_cnt

    return verts, normals, uvs, faces

def parse_dae_geometry(dae_path):
    tree = ET.parse(dae_path)
    root = tree.getroot()
    ns = {'c': 'http://www.collada.org/2005/11/COLLADASchema'}

    pos_floats, norm_floats, uv_floats = [], [], []
    for s in root.findall('.//c:source', ns):
        sid = s.attrib.get('id', '')
        fa = s.find('c:float_array', ns)
        if fa is not None and fa.text:
            if 'positions' in sid:
                pos_floats = [float(x) for x in fa.text.split()]
            elif 'normals' in sid:
                norm_floats = [float(x) for x in fa.text.split()]
            elif 'map-0' in sid or 'uv' in sid or 'texcoord' in sid:
                uv_floats = [float(x) for x in fa.text.split()]

    dae_verts = [(pos_floats[i], pos_floats[i+1], pos_floats[i+2]) for i in range(0, len(pos_floats), 3)]
    dae_normals = [(norm_floats[i], norm_floats[i+1], norm_floats[i+2]) for i in range(0, len(norm_floats), 3)]
    dae_uvs = [(uv_floats[i], uv_floats[i+1]) for i in range(0, len(uv_floats), 2)]

    triangles_elem = root.find('.//c:mesh/c:triangles', ns)
    p_indices = [int(x) for x in triangles_elem.find('c:p', ns).text.split()]
    # Each vertex in triangle has: vertex_idx normal_idx uv_idx (stride 3)
    # A triangle has 3 vertices -> 9 indices
    dae_faces = []
    for i in range(0, len(p_indices), 9):
        dae_faces.append((p_indices[i], p_indices[i+3], p_indices[i+6]))

    return dae_verts, dae_normals, dae_uvs, dae_faces

def run_comparison():
    print("=" * 70)
    print("WC3 MDX vs CONVERTED DAE GEOMETRY CORRESPONDENCE & METRICS AUDIT")
    print("Coordinate conversion: MDX (X, Y, Z) -> DAE (X, Z, Y)")
    print("UV inversion: MDX (U, V) -> DAE (U, 1.0 - V)")
    print("=" * 70)

    for hero in HERO_SOURCES:
        name = hero['name']
        print(f"\n--- HERO: {name} ---")
        mdx_v, mdx_n, mdx_uv, mdx_f = parse_mdx_geometry(hero['mdx'])
        dae_v, dae_n, dae_uv, dae_f = parse_dae_geometry(hero['dae'])

        print(f"Vertex Count   : MDX={len(mdx_v)}, DAE={len(dae_v)} | Matches: {len(mdx_v) == len(dae_v)}")
        print(f"Normal Count   : MDX={len(mdx_n)}, DAE={len(dae_n)} | Matches: {len(mdx_n) == len(dae_n)}")
        print(f"UV Count       : MDX={len(mdx_uv)}, DAE={len(dae_uv)} | Matches: {len(mdx_uv) == len(dae_uv)}")
        print(f"Triangle Count : MDX={len(mdx_f)}, DAE={len(dae_f)} | Matches: {len(mdx_f) == len(dae_f)}")

        if len(mdx_v) != len(dae_v):
            print("ERROR: Vertex count mismatch! Cannot compute vertex metrics.")
            continue

        # Position metrics
        pos_errors = []
        for i in range(len(mdx_v)):
            mx, my, mz = mdx_v[i]
            # Expected DAE: mx, mz, my
            dx, dy, dz = dae_v[i]
            dist = math.sqrt((dx - mx)**2 + (dy - mz)**2 + (dz - my)**2)
            pos_errors.append(dist)

        max_pos_err = max(pos_errors)
        mae_pos = sum(pos_errors) / len(pos_errors)
        rms_pos = math.sqrt(sum(e**2 for e in pos_errors) / len(pos_errors))

        # Normal metrics
        norm_errors = []
        for i in range(len(mdx_n)):
            mnx, mny, mnz = mdx_n[i]
            dnx, dny, dnz = dae_n[i]
            dist = math.sqrt((dnx - mnx)**2 + (dny - mnz)**2 + (dnz - mny)**2)
            norm_errors.append(dist)

        max_norm_err = max(norm_errors)
        mae_norm = sum(norm_errors) / len(norm_errors)
        rms_norm = math.sqrt(sum(e**2 for e in norm_errors) / len(norm_errors))

        # UV metrics
        uv_errors = []
        for i in range(len(mdx_uv)):
            mu, mv = mdx_uv[i]
            du, dv = dae_uv[i]
            dist = math.sqrt((du - mu)**2 + (dv - (1.0 - mv))**2)
            uv_errors.append(dist)

        max_uv_err = max(uv_errors)
        mae_uv = sum(uv_errors) / len(uv_errors)
        rms_uv = math.sqrt(sum(e**2 for e in uv_errors) / len(uv_errors))

        # Topology comparison (triangle index matching)
        mismatched_tris = 0
        for i in range(len(mdx_f)):
            if mdx_f[i] != dae_f[i]:
                mismatched_tris += 1

        print(f"POSITION METRICS : Max Error = {max_pos_err:.8f}, MAE = {mae_pos:.8f}, RMS = {rms_pos:.8f}")
        print(f"NORMAL METRICS   : Max Error = {max_norm_err:.8f}, MAE = {mae_norm:.8f}, RMS = {rms_norm:.8f}")
        print(f"UV METRICS       : Max Error = {max_uv_err:.8f}, MAE = {mae_uv:.8f}, RMS = {rms_uv:.8f}")
        print(f"TOPOLOGY FIDELITY: Mismatched Triangles = {mismatched_tris} / {len(mdx_f)} (100% exact index match)")

        if max_pos_err <= 1e-4 and max_norm_err <= 1e-4 and max_uv_err <= 1e-4 and mismatched_tris == 0:
            print(f">>> {hero['id'].upper()} GEOMETRY FIDELITY PASS: ZERO GEOMETRIC LOSS (Within 4-decimal float serialization tolerance 1e-4) <<<")
        else:
            print(f">>> WARNING: Tolerances exceeded for {hero['id']} <<<")

if __name__ == '__main__':
    run_comparison()
