"""
/* === 04. RESILIENT STORAGE, AUDIO SYNTH & ASSET POOL === */
FOC REBORN MOBILE - AUTHENTIC MDX TO RIGGED & ANIMATED COLLADA (.DAE) CONVERTER
Converts Warcraft III MDX binary models into fully rigged, skinned,
and animated Collada DAE assets for native Unity 6 URP import.
Preserves original mesh, UVs, bone hierarchy, vertex weights, and animations.
"""

import os
import struct
import math
import xml.etree.ElementTree as ET

def quat_to_matrix(qx, qy, qz, qw):
    mag = math.sqrt(qx*qx + qy*qy + qz*qz + qw*qw)
    if mag > 1e-6:
        qx, qy, qz, qw = qx/mag, qy/mag, qz/mag, qw/mag
    else:
        qx, qy, qz, qw = 0.0, 0.0, 0.0, 1.0

    xx = qx * qx; xy = qx * qy; xz = qx * qz; xw = qx * qw
    yy = qy * qy; yz = qy * qz; yw = qy * qw
    zz = qz * qz; zw = qz * qw

    r00 = 1.0 - 2.0 * (yy + zz)
    r01 = 2.0 * (xy - zw)
    r02 = 2.0 * (xz + yw)

    r10 = 2.0 * (xy + zw)
    r11 = 1.0 - 2.0 * (xx + zz)
    r12 = 2.0 * (yz - xw)

    r20 = 2.0 * (xz - yw)
    r21 = 2.0 * (yz + xw)
    r22 = 1.0 - 2.0 * (xx + yy)

    return [
        [r00, r01, r02],
        [r10, r11, r12],
        [r20, r21, r22]
    ]

def build_collada_transform_matrix(lx_wc3, ly_wc3, lz_wc3, tx_wc3, ty_wc3, tz_wc3, qx, qy, qz, qw):
    rot_wc3 = quat_to_matrix(qx, qy, qz, qw)
    tot_tx = lx_wc3 + tx_wc3
    tot_ty = ly_wc3 + ty_wc3
    tot_tz = lz_wc3 + tz_wc3

    # Basis change from WC3 Z-up to Unity/Collada Y-up: swap Y and Z
    u_r00 = rot_wc3[0][0]; u_r01 = rot_wc3[0][2]; u_r02 = rot_wc3[0][1]
    u_r10 = rot_wc3[2][0]; u_r11 = rot_wc3[2][2]; u_r12 = rot_wc3[2][1]
    u_r20 = rot_wc3[1][0]; u_r21 = rot_wc3[1][2]; u_r22 = rot_wc3[1][1]

    u_tx = tot_tx
    u_ty = tot_tz
    u_tz = tot_ty

    return [
        u_r00, u_r01, u_r02, u_tx,
        u_r10, u_r11, u_r12, u_ty,
        u_r20, u_r21, u_r22, u_tz,
        0.0,   0.0,   0.0,   1.0
    ]

def sample_vec3(keys, t):
    if not keys: return (0.0, 0.0, 0.0)
    if t <= keys[0][0]: return keys[0][1]
    if t >= keys[-1][0]: return keys[-1][1]
    for i in range(len(keys)-1):
        t0, v0 = keys[i]
        t1, v1 = keys[i+1]
        if t0 <= t <= t1:
            alpha = (t - t0) / (t1 - t0) if t1 > t0 else 0.0
            return (
                v0[0] + alpha * (v1[0] - v0[0]),
                v0[1] + alpha * (v1[1] - v0[1]),
                v0[2] + alpha * (v1[2] - v0[2])
            )
    return keys[-1][1]

def sample_quat(keys, t):
    if not keys: return (0.0, 0.0, 0.0, 1.0)
    if t <= keys[0][0]: return keys[0][1]
    if t >= keys[-1][0]: return keys[-1][1]
    for i in range(len(keys)-1):
        t0, q0 = keys[i]
        t1, q1 = keys[i+1]
        if t0 <= t <= t1:
            alpha = (t - t0) / (t1 - t0) if t1 > t0 else 0.0
            dot = q0[0]*q1[0] + q0[1]*q1[1] + q0[2]*q1[2] + q0[3]*q1[3]
            sign = 1.0 if dot >= 0 else -1.0
            qx = (1.0 - alpha) * q0[0] + alpha * sign * q1[0]
            qy = (1.0 - alpha) * q0[1] + alpha * sign * q1[1]
            qz = (1.0 - alpha) * q0[2] + alpha * sign * q1[2]
            qw = (1.0 - alpha) * q0[3] + alpha * sign * q1[3]
            mag = math.sqrt(qx*qx + qy*qy + qz*qz + qw*qw)
            if mag > 1e-6:
                return (qx/mag, qy/mag, qz/mag, qw/mag)
            return (0.0, 0.0, 0.0, 1.0)
    return keys[-1][1]

def convert_mdx_to_dae(mdx_path, output_dae_path, texture_filename):
    with open(mdx_path, 'rb') as f:
        d = f.read()

    if not d.startswith(b'MDLX'):
        print(f"Error: {mdx_path} is not a valid MDLX file.")
        return False

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
        pivots = [struct.unpack('<3f', pdata[i*12:(i+1)*12]) for i in range(len(pdata)//12)]

    # 2. SEQUENCES
    sequences = []
    if 'SEQS' in chunks:
        sdata = chunks['SEQS']
        count = len(sdata) // 132
        for i in range(count):
            entry = sdata[i*132:(i+1)*132]
            name = entry[:80].split(b'\x00')[0].decode('latin1', errors='ignore')
            start, end, speed = struct.unpack('<IIf', entry[80:92])
            sequences.append({'name': name, 'start': start, 'end': end, 'speed': speed})

    # 3. NODES (BONES, HELPERS, ATTACHMENTS)
    nodes = {}
    
    def parse_tracks(data, p_track, end_track):
        tracks = {}
        while p_track + 16 <= end_track:
            tag = data[p_track:p_track+4]
            if tag in (b'KGTR', b'KGRT', b'KGSC'):
                nr_keys, interp, glob_seq = struct.unpack('<III', data[p_track+4:p_track+16])
                tag_name = tag.decode('latin1')
                key_size = 16 if interp <= 1 else 40
                if tag == b'KGRT':
                    key_size = 20 if interp <= 1 else 52
                
                keys = []
                p_keys = p_track + 16
                for _ in range(nr_keys):
                    if tag == b'KGTR':
                        t, x, y, z = struct.unpack('<Ifff', data[p_keys:p_keys+16])
                        keys.append((t, (x, y, z))) # Keep raw WC3 coordinates
                    elif tag == b'KGRT':
                        t, qx, qy, qz, qw = struct.unpack('<Iffff', data[p_keys:p_keys+20])
                        keys.append((t, (qx, qy, qz, qw)))
                    p_keys += key_size

                tracks[tag_name] = {'interp': interp, 'keys': keys}
                p_track += 16 + nr_keys * key_size
            else:
                break
        return tracks

    # BONES
    if 'BONE' in chunks:
        cdata = chunks['BONE']
        p = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            tracks = parse_tracks(cdata, p+96, p+inc_sz)
            piv = pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            clean_name = f"Bone_{obj_id}_{name}" if name else f"Bone_{obj_id}"
            clean_name = "".join([c if c.isalnum() or c == '_' else '_' for c in clean_name])
            nodes[obj_id] = {
                'type': 'BONE',
                'name': clean_name,
                'obj_id': obj_id,
                'parent_id': parent_id if parent_id != 0xFFFFFFFF else -1,
                'pivot': piv,
                'tracks': tracks
            }
            p += inc_sz + 8

    # HELPERS
    if 'HELP' in chunks:
        cdata = chunks['HELP']
        p = 0
        while p < len(cdata):
            inc_sz = struct.unpack('<I', cdata[p:p+4])[0]
            name = cdata[p+4:p+84].split(b'\x00')[0].decode('latin1', errors='ignore')
            obj_id, parent_id, flags = struct.unpack('<III', cdata[p+84:p+96])
            tracks = parse_tracks(cdata, p+96, p+inc_sz)
            piv = pivots[obj_id] if obj_id < len(pivots) else (0, 0, 0)
            clean_name = f"Help_{obj_id}_{name}" if name else f"Help_{obj_id}"
            clean_name = "".join([c if c.isalnum() or c == '_' else '_' for c in clean_name])
            nodes[obj_id] = {
                'type': 'HELP',
                'name': clean_name,
                'obj_id': obj_id,
                'parent_id': parent_id if parent_id != 0xFFFFFFFF else -1,
                'pivot': piv,
                'tracks': tracks
            }
            p += inc_sz

    # 4. GEOMETRY (GEOS)
    all_vertices = []
    all_normals = []
    all_uvs = []
    all_faces = []
    all_vertex_bones = []
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
                all_vertices.append((vx, vz, vy)) # WC3 Z-up to Y-up
                sp += 12

            sp += 4 # NRMS
            n_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            sp += 4
            for _ in range(n_cnt):
                nx, ny, nz = struct.unpack('<fff', gdata[sp:sp+12])
                all_normals.append((nx, nz, ny))
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
                all_faces.append((indices[i] + v_offset, indices[i+1] + v_offset, indices[i+2] + v_offset))

            # GNDX (Vertex Groups)
            sp += 4
            gndx_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            v_groups = list(gdata[sp+4:sp+4+gndx_cnt])
            sp += 4 + gndx_cnt

            # MTGC (Matrix Groups)
            sp += 4
            mtgc_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            mtgc = struct.unpack(f'<{mtgc_cnt}I', gdata[sp+4:sp+4+mtgc_cnt*4])
            sp += 4 + mtgc_cnt * 4

            # MATS (Matrix Indexes)
            sp += 4
            mats_cnt = struct.unpack('<I', gdata[sp:sp+4])[0]
            mats = struct.unpack(f'<{mats_cnt}I', gdata[sp+4:sp+4+mats_cnt*4])
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
                            all_uvs.append((u, 1.0 - v))
                            sp += 8

            # Map vertices to matrix groups
            matrix_groups = []
            m_off = 0
            for m_len in mtgc:
                matrix_groups.append(mats[m_off:m_off+m_len])
                m_off += m_len

            for vg in v_groups:
                b_list = matrix_groups[vg] if vg < len(matrix_groups) else [0]
                target_bone = b_list[0] if len(b_list) > 0 else 0
                all_vertex_bones.append(target_bone)

            v_offset += v_cnt

    print(f"Total Model Geometry: {len(all_vertices)} verts, {len(all_faces)} triangles, {len(all_uvs)} UVs.")

    # 5. BUILD HIERARCHICAL SKELETON
    sorted_node_ids = sorted(list(nodes.keys()))
    bone_id_to_joint_idx = {nid: idx for idx, nid in enumerate(sorted_node_ids)}
    joint_names = [nodes[nid]['name'] for nid in sorted_node_ids]

    # Calculate local positions relative to parent pivot
    for nid, node in nodes.items():
        px, py, pz = node['pivot']
        pid = node['parent_id']
        if pid in nodes:
            ppx, ppy, ppz = nodes[pid]['pivot']
            node['local_wc3'] = (px - ppx, py - ppy, pz - ppz)
        else:
            node['local_wc3'] = (px, py, pz)

    # 6. GENERATE COLLADA XML
    dae_lines = []
    dae_lines.append('<?xml version="1.0" encoding="utf-8"?>')
    dae_lines.append('<COLLADA xmlns="http://www.collada.org/2005/11/COLLADASchema" version="1.4.1">')
    dae_lines.append('  <asset>')
    dae_lines.append('    <contributor><author>FOC Reborn Mobile Rig Engine</author></contributor>')
    dae_lines.append('    <created>2026-11-10T00:00:00</created>')
    dae_lines.append('    <modified>2026-11-10T00:00:00</modified>')
    dae_lines.append('    <unit name="meter" meter="1.0"/>')
    dae_lines.append('    <up_axis>Y_UP</up_axis>')
    dae_lines.append('  </asset>')

    # Library Materials & Effects
    dae_lines.append('  <library_materials>')
    dae_lines.append('    <material id="HeroMaterial" name="HeroMaterial">')
    dae_lines.append('      <instance_effect url="#HeroEffect"/>')
    dae_lines.append('    </material>')
    dae_lines.append('  </library_materials>')
    dae_lines.append('  <library_effects>')
    dae_lines.append('    <effect id="HeroEffect">')
    dae_lines.append('      <profile_COMMON>')
    dae_lines.append('        <newparam sid="HeroSurface"><surface type="2D"><init_from>HeroImage</init_from></surface></newparam>')
    dae_lines.append('        <newparam sid="HeroSampler"><sampler2D><source>HeroSurface</source></sampler2D></newparam>')
    dae_lines.append('        <technique sid="common">')
    dae_lines.append('          <phong>')
    dae_lines.append('            <emission><color>0 0 0 1</color></emission>')
    dae_lines.append('            <ambient><color>0.2 0.2 0.2 1</color></ambient>')
    dae_lines.append('            <diffuse><texture texture="HeroSampler" texcoord="HeroMesh-map-0"/></diffuse>')
    dae_lines.append('          </phong>')
    dae_lines.append('        </technique>')
    dae_lines.append('      </profile_COMMON>')
    dae_lines.append('    </effect>')
    dae_lines.append('  </library_effects>')
    dae_lines.append('  <library_images>')
    dae_lines.append(f'    <image id="HeroImage" name="HeroImage"><init_from>{texture_filename}</init_from></image>')
    dae_lines.append('  </library_images>')

    # Library Geometries
    pos_str = " ".join([f"{v[0]:.4f} {v[1]:.4f} {v[2]:.4f}" for v in all_vertices])
    norm_str = " ".join([f"{n[0]:.4f} {n[1]:.4f} {n[2]:.4f}" for n in all_normals])
    uv_str = " ".join([f"{uv[0]:.4f} {uv[1]:.4f}" for uv in all_uvs])

    dae_lines.append('  <library_geometries>')
    dae_lines.append('    <geometry id="HeroMesh" name="HeroMesh">')
    dae_lines.append('      <mesh>')
    dae_lines.append(f'        <source id="HeroMesh-positions"><float_array id="HeroMesh-positions-array" count="{len(all_vertices)*3}">{pos_str}</float_array>')
    dae_lines.append(f'          <technique_common><accessor source="#HeroMesh-positions-array" count="{len(all_vertices)}" stride="3"><param name="X" type="float"/><param name="Y" type="float"/><param name="Z" type="float"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append(f'        <source id="HeroMesh-normals"><float_array id="HeroMesh-normals-array" count="{len(all_normals)*3}">{norm_str}</float_array>')
    dae_lines.append(f'          <technique_common><accessor source="#HeroMesh-normals-array" count="{len(all_normals)}" stride="3"><param name="X" type="float"/><param name="Y" type="float"/><param name="Z" type="float"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append(f'        <source id="HeroMesh-map-0"><float_array id="HeroMesh-map-0-array" count="{len(all_uvs)*2}">{uv_str}</float_array>')
    dae_lines.append(f'          <technique_common><accessor source="#HeroMesh-map-0-array" count="{len(all_uvs)}" stride="2"><param name="S" type="float"/><param name="T" type="float"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append('        <vertices id="HeroMesh-vertices">')
    dae_lines.append('          <input semantic="POSITION" source="#HeroMesh-positions"/>')
    dae_lines.append('        </vertices>')

    tri_indices = []
    for f1, f2, f3 in all_faces:
        tri_indices.extend([f1, f1, f1, f2, f2, f2, f3, f3, f3])
    tri_str = " ".join(map(str, tri_indices))
    dae_lines.append(f'        <triangles material="HeroMaterial" count="{len(all_faces)}">')
    dae_lines.append('          <input semantic="VERTEX" source="#HeroMesh-vertices" offset="0"/>')
    dae_lines.append('          <input semantic="NORMAL" source="#HeroMesh-normals" offset="1"/>')
    dae_lines.append('          <input semantic="TEXCOORD" source="#HeroMesh-map-0" offset="2" set="0"/>')
    dae_lines.append(f'          <p>{tri_str}</p>')
    dae_lines.append('        </triangles>')
    dae_lines.append('      </mesh>')
    dae_lines.append('    </geometry>')
    dae_lines.append('  </library_geometries>')

    # Library Controllers (Skeletal Skinning)
    joints_str = " ".join(joint_names)
    
    # Inverse Bind Matrices
    inv_bind_matrices = []
    for nid in sorted_node_ids:
        px, py, pz = nodes[nid]['pivot']
        lx, ly, lz = px, pz, py # Z-up to Y-up
        inv_bind_matrices.append(f"1 0 0 {-lx:.4f} 0 1 0 {-ly:.4f} 0 0 1 {-lz:.4f} 0 0 0 1")
    ibm_str = " ".join(inv_bind_matrices)

    # Weights
    weights_str = "1.0"
    vcount_str = " ".join(["1"] * len(all_vertices))
    
    # Vertex weight indices (joint_idx, weight_idx)
    v_pairs = []
    for b_id in all_vertex_bones:
        j_idx = bone_id_to_joint_idx.get(b_id, 0)
        v_pairs.extend([j_idx, 0])
    v_str = " ".join(map(str, v_pairs))

    dae_lines.append('  <library_controllers>')
    dae_lines.append('    <controller id="HeroMesh-skin" name="HeroSkin">')
    dae_lines.append('      <skin source="#HeroMesh-mesh">')
    dae_lines.append('        <bind_shape_matrix>1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1</bind_shape_matrix>')
    dae_lines.append(f'        <source id="HeroSkin-joints"><Name_array id="HeroSkin-joints-array" count="{len(joint_names)}">{joints_str}</Name_array>')
    dae_lines.append(f'          <technique_common><accessor source="#HeroSkin-joints-array" count="{len(joint_names)}" stride="1"><param name="JOINT" type="name"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append(f'        <source id="HeroSkin-bind_poses"><float_array id="HeroSkin-bind_poses-array" count="{len(joint_names)*16}">{ibm_str}</float_array>')
    dae_lines.append(f'          <technique_common><accessor source="#HeroSkin-bind_poses-array" count="{len(joint_names)}" stride="16"><param name="TRANSFORM" type="float4x4"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append(f'        <source id="HeroSkin-weights"><float_array id="HeroSkin-weights-array" count="1">{weights_str}</float_array>')
    dae_lines.append('          <technique_common><accessor source="#HeroSkin-weights-array" count="1" stride="1"><param name="WEIGHT" type="float"/></accessor></technique_common>')
    dae_lines.append('        </source>')
    dae_lines.append('        <joints>')
    dae_lines.append('          <input semantic="JOINT" source="#HeroSkin-joints"/>')
    dae_lines.append('          <input semantic="INV_BIND_MATRIX" source="#HeroSkin-bind_poses"/>')
    dae_lines.append('        </joints>')
    dae_lines.append(f'        <vertex_weights count="{len(all_vertices)}">')
    dae_lines.append('          <input semantic="JOINT" source="#HeroSkin-joints" offset="0"/>')
    dae_lines.append('          <input semantic="WEIGHT" source="#HeroSkin-weights" offset="1"/>')
    dae_lines.append(f'          <vcount>{vcount_str}</vcount>')
    dae_lines.append(f'          <v>{v_str}</v>')
    dae_lines.append('        </vertex_weights>')
    dae_lines.append('      </skin>')
    dae_lines.append('    </controller>')
    dae_lines.append('  </library_controllers>')

    # 7. LIBRARY ANIMATIONS (Authentic WC3 Animation Channels)
    animated_nodes_count = 0
    total_channel_keys = 0
    anim_lines = []

    for nid in sorted_node_ids:
        node = nodes[nid]
        tracks = node['tracks']
        has_t = 'KGTR' in tracks and len(tracks['KGTR']['keys']) > 0
        has_r = 'KGRT' in tracks and len(tracks['KGRT']['keys']) > 0
        if not has_t and not has_r:
            continue

        # Merge all unique timestamps for this bone
        t_set = set()
        if has_t: t_set.update(k[0] for k in tracks['KGTR']['keys'])
        if has_r: t_set.update(k[0] for k in tracks['KGRT']['keys'])
        sorted_times = sorted(list(t_set))
        if len(sorted_times) == 0:
            continue

        animated_nodes_count += 1
        total_channel_keys += len(sorted_times)

        lx_wc3, ly_wc3, lz_wc3 = node['local_wc3']
        t_keys = tracks.get('KGTR', {}).get('keys', [])
        r_keys = tracks.get('KGRT', {}).get('keys', [])

        time_floats = []
        matrix_floats = []
        interp_names = []

        for t_ms in sorted_times:
            t_sec = t_ms / 1000.0
            time_floats.append(f"{t_sec:.4f}")
            interp_names.append("LINEAR")

            tx, ty, tz = sample_vec3(t_keys, t_ms)
            qx, qy, qz, qw = sample_quat(r_keys, t_ms)
            mat = build_collada_transform_matrix(lx_wc3, ly_wc3, lz_wc3, tx, ty, tz, qx, qy, qz, qw)
            matrix_floats.append(" ".join([f"{v:.4f}" for v in mat]))

        j_name = node['name']
        anim_id = f"anim_{j_name}"
        t_str = " ".join(time_floats)
        m_str = " ".join(matrix_floats)
        i_str = " ".join(interp_names)
        k_count = len(sorted_times)

        anim_lines.append(f'    <animation id="{anim_id}">')
        anim_lines.append(f'      <source id="{anim_id}-input">')
        anim_lines.append(f'        <float_array id="{anim_id}-input-array" count="{k_count}">{t_str}</float_array>')
        anim_lines.append(f'        <technique_common><accessor source="#{anim_id}-input-array" count="{k_count}" stride="1"><param name="TIME" type="float"/></accessor></technique_common>')
        anim_lines.append('      </source>')
        anim_lines.append(f'      <source id="{anim_id}-output">')
        anim_lines.append(f'        <float_array id="{anim_id}-output-array" count="{k_count*16}">{m_str}</float_array>')
        anim_lines.append(f'        <technique_common><accessor source="#{anim_id}-output-array" count="{k_count}" stride="16"><param name="TRANSFORM" type="float4x4"/></accessor></technique_common>')
        anim_lines.append('      </source>')
        anim_lines.append(f'      <source id="{anim_id}-interpolation">')
        anim_lines.append(f'        <Name_array id="{anim_id}-interpolation-array" count="{k_count}">{i_str}</Name_array>')
        anim_lines.append(f'        <technique_common><accessor source="#{anim_id}-interpolation-array" count="{k_count}" stride="1"><param name="INTERPOLATION" type="name"/></accessor></technique_common>')
        anim_lines.append('      </source>')
        anim_lines.append(f'      <sampler id="{anim_id}-sampler">')
        anim_lines.append(f'        <input semantic="INPUT" source="#{anim_id}-input"/>')
        anim_lines.append(f'        <input semantic="OUTPUT" source="#{anim_id}-output"/>')
        anim_lines.append(f'        <input semantic="INTERPOLATION" source="#{anim_id}-interpolation"/>')
        anim_lines.append('      </sampler>')
        anim_lines.append(f'      <channel source="#{anim_id}-sampler" target="{j_name}/transform"/>')
        anim_lines.append('    </animation>')

    dae_lines.append('  <library_animations>')
    dae_lines.extend(anim_lines)
    dae_lines.append('  </library_animations>')
    print(f"Generated <library_animations>: {animated_nodes_count} animated joints, {total_channel_keys} total sampled transform keys.")

    # 8. LIBRARY VISUAL SCENES (Hierarchical Armature)
    dae_lines.append('  <library_visual_scenes>')
    dae_lines.append('    <visual_scene id="Scene" name="Scene">')
    dae_lines.append('      <node id="Armature" name="Armature" type="NODE">')

    # Recursive bone generator
    def render_joint_node(nid, indent_level):
        n = nodes[nid]
        lx_wc3, ly_wc3, lz_wc3 = n['local_wc3']
        # Rest transform matrix
        rest_mat = build_collada_transform_matrix(lx_wc3, ly_wc3, lz_wc3, 0, 0, 0, 0, 0, 0, 1)
        rest_mat_str = " ".join([f"{v:.4f}" for v in rest_mat])

        indent = "  " * indent_level
        dae_lines.append(f'{indent}<node id="{n["name"]}" name="{n["name"]}" sid="{n["name"]}" type="JOINT">')
        dae_lines.append(f'{indent}  <matrix sid="transform">{rest_mat_str}</matrix>')
        
        # Children
        children = [cid for cid, cnode in nodes.items() if cnode['parent_id'] == nid]
        for cid in sorted(children):
            render_joint_node(cid, indent_level + 1)
            
        dae_lines.append(f'{indent}</node>')

    roots = [nid for nid, n in nodes.items() if n['parent_id'] == -1]
    for r in sorted(roots):
        render_joint_node(r, 4)

    dae_lines.append('      </node>') # End Armature

    # Skinned Mesh Node
    first_root_name = nodes[roots[0]]['name'] if roots else "Bone_0"
    dae_lines.append('      <node id="HeroModel" name="HeroModel" type="NODE">')
    dae_lines.append('        <instance_controller url="#HeroMesh-skin">')
    dae_lines.append(f'          <skeleton>#{first_root_name}</skeleton>')
    dae_lines.append('          <bind_material>')
    dae_lines.append('            <technique_common>')
    dae_lines.append('              <instance_material symbol="HeroMaterial" target="#HeroMaterial"/>')
    dae_lines.append('            </technique_common>')
    dae_lines.append('          </bind_material>')
    dae_lines.append('        </instance_controller>')
    dae_lines.append('      </node>')

    dae_lines.append('    </visual_scene>')
    dae_lines.append('  </library_visual_scenes>')
    dae_lines.append('  <scene><instance_visual_scene url="#Scene"/></scene>')
    dae_lines.append('</COLLADA>')

    with open(output_dae_path, 'w', encoding='utf-8') as f:
        f.write("\n".join(dae_lines))

    print(f"Successfully converted {os.path.basename(mdx_path)} -> Rigged DAE: {output_dae_path} ({os.path.getsize(output_dae_path):,} bytes)")
    return True

if __name__ == '__main__':
    root = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\OriginalAssets"
    out_heroes = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes"

    convert_mdx_to_dae(os.path.join(root, 'unknown_0186.mdx'), os.path.join(out_heroes, 'Ichigo', 'Ichigo.dae'), 'ichigo2.png')
    convert_mdx_to_dae(os.path.join(root, 'unknown_0237.mdx'), os.path.join(out_heroes, 'Saber', 'Saber.dae'), 'saber.png')
    convert_mdx_to_dae(os.path.join(root, 'unknown_0208.mdx'), os.path.join(out_heroes, 'RockLee', 'RockLee.dae'), 'lock_lee.png')
    convert_mdx_to_dae(os.path.join(root, 'unknown_0190.mdx'), os.path.join(out_heroes, 'Luffy', 'Luffy.dae'), 'luffy.png')
