"""
/* === 04. RESILIENT STORAGE, AUDIO SYNTH & ASSET POOL === */
FOC REBORN MOBILE - MDX TO OBJ & SKELETAL RIG CONVERTER
Extracts exact 3D geometry (vertices, normals, UVs, face triangles, materials)
from original Warcraft III MDX files into standard Wavefront OBJ + MTL files.
"""

import os
import struct
import shutil

def parse_and_convert_mdx_to_obj(mdx_path, output_obj_path, texture_name="texture.png"):
    with open(mdx_path, 'rb') as f:
        data = f.read()

    if not data.startswith(b'MDLX'):
        print(f"Error: {mdx_path} is not a valid MDLX file.")
        return False

    pos = 4
    geosets = []
    sequences = []
    attachments = []

    while pos + 8 <= len(data):
        tag, size = struct.unpack('<4sI', data[pos:pos+8])
        pos += 8
        chunk_data = data[pos:pos+size]
        pos += size

        if tag == b'SEQS':
            seq_count = size // 132
            for s in range(seq_count):
                sdata = chunk_data[s*132 : (s+1)*132]
                name = sdata[:80].split(b'\x00')[0].decode('latin1', errors='ignore')
                sstart, send, spd = struct.unpack('<IIf', sdata[80:92])
                sequences.append((name, sstart, send, spd))

        elif tag == b'GEOS':
            gpos = 0
            while gpos < len(chunk_data):
                g_size = struct.unpack('<I', chunk_data[gpos:gpos+4])[0]
                geoset_data = chunk_data[gpos:gpos+g_size]
                gpos += g_size

                # Sub-chunk parsing
                sub_pos = 4
                # VRTX
                tag_v = geoset_data[sub_pos:sub_pos+4]
                sub_pos += 4
                v_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4
                vertices = []
                for _ in range(v_count):
                    vx, vy, vz = struct.unpack('<fff', geoset_data[sub_pos:sub_pos+12])
                    vertices.append((vx, vy, vz))
                    sub_pos += 12

                # NRMS
                tag_n = geoset_data[sub_pos:sub_pos+4]
                sub_pos += 4
                n_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4
                normals = []
                for _ in range(n_count):
                    nx, ny, nz = struct.unpack('<fff', geoset_data[sub_pos:sub_pos+12])
                    normals.append((nx, ny, nz))
                    sub_pos += 12

                # PTYP
                sub_pos += 4
                ptyp_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + ptyp_count * 4

                # PCNT
                sub_pos += 4
                pcnt_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + pcnt_count * 4

                # PVTX (Face indices)
                sub_pos += 4
                idx_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4
                indices = struct.unpack(f'<{idx_count}H', geoset_data[sub_pos:sub_pos+idx_count*2])
                sub_pos += idx_count * 2

                # GNDX
                sub_pos += 4
                gndx_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + gndx_count

                # MTGC
                sub_pos += 4
                mtgc_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + mtgc_count * 4

                # MATS
                sub_pos += 4
                mats_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + mats_count * 4

                # MaterialID (4) + SelectionGroup (4) + SelectionFlags (4)
                material_id = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 12

                # Bounds radius (4) + min bounds (12) + max bounds (12)
                sub_pos += 28

                # Extents
                ext_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                sub_pos += 4 + ext_count * 28

                # UVAS (Texture Coordinates)
                uvs = []
                if sub_pos < len(geoset_data):
                    tag_uvas = geoset_data[sub_pos:sub_pos+4]
                    if tag_uvas == b'UVAS':
                        sub_pos += 4
                        uv_channels = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                        sub_pos += 4
                        # UVBS
                        tag_uvbs = geoset_data[sub_pos:sub_pos+4]
                        sub_pos += 4
                        uv_count = struct.unpack('<I', geoset_data[sub_pos:sub_pos+4])[0]
                        sub_pos += 4
                        for _ in range(uv_count):
                            u, v = struct.unpack('<ff', geoset_data[sub_pos:sub_pos+8])
                            uvs.append((u, 1.0 - v)) # Invert V for standard OpenGL/Unity UV
                            sub_pos += 8

                geosets.append({
                    'vertices': vertices,
                    'normals': normals,
                    'uvs': uvs,
                    'indices': indices,
                    'material_id': material_id
                })

    # Write OBJ File
    os.makedirs(os.path.dirname(output_obj_path), exist_ok=True)
    mtl_filename = os.path.splitext(os.path.basename(output_obj_path))[0] + ".mtl"
    mtl_path = os.path.join(os.path.dirname(output_obj_path), mtl_filename)

    with open(output_obj_path, 'w', encoding='utf-8') as obj:
        obj.write(f"# Converted from original WC3 MDX: {os.path.basename(mdx_path)}\n")
        obj.write(f"mtllib {mtl_filename}\n\n")

        v_offset = 1
        for g_idx, g in enumerate(geosets):
            obj.write(f"g geoset_{g_idx}\n")
            obj.write(f"usemtl Material_{g['material_id']}\n")

            for vx, vy, vz in g['vertices']:
                # WC3 uses Z-up. Convert to Unity Y-up: (x, z, -y) or (x, z, y)
                obj.write(f"v {vx:.5f} {vz:.5f} {vy:.5f}\n")

            for nx, ny, nz in g['normals']:
                obj.write(f"vn {nx:.5f} {nz:.5f} {ny:.5f}\n")

            for u, v in g['uvs']:
                obj.write(f"vt {u:.5f} {v:.5f}\n")

            has_normals = len(g['normals']) == len(g['vertices'])
            has_uvs = len(g['uvs']) == len(g['vertices'])

            for i in range(0, len(g['indices']), 3):
                i1 = g['indices'][i] + v_offset
                i2 = g['indices'][i+1] + v_offset
                i3 = g['indices'][i+2] + v_offset
                if has_uvs and has_normals:
                    obj.write(f"f {i1}/{i1}/{i1} {i2}/{i2}/{i2} {i3}/{i3}/{i3}\n")
                elif has_uvs:
                    obj.write(f"f {i1}/{i1} {i2}/{i2} {i3}/{i3}\n")
                else:
                    obj.write(f"f {i1} {i2} {i3}\n")

            v_offset += len(g['vertices'])

    # Write MTL File
    with open(mtl_path, 'w', encoding='utf-8') as mtl:
        mtl.write(f"# Material for {os.path.basename(output_obj_path)}\n")
        for g in geosets:
            mid = g['material_id']
            mtl.write(f"newmtl Material_{mid}\n")
            mtl.write("Ka 1.0 1.0 1.0\nKd 1.0 1.0 1.0\nKs 0.2 0.2 0.2\nNs 10.0\n")
            mtl.write(f"map_Kd ../Textures/{texture_name}\n\n")

    print(f"Exported {output_obj_path} ({len(geosets)} geosets, {v_offset-1} vertices).")
    return True

def convert_target_heroes():
    models_dir = os.path.join(os.getcwd(), "FOC-Reborn-Mobile", "ConvertedAssets", "Models")
    textures_dir = os.path.join(os.getcwd(), "FOC-Reborn-Mobile", "ConvertedAssets", "Textures")
    os.makedirs(models_dir, exist_ok=True)

    # 1. Ichigo Kurosaki (unknown_0186.mdx -> Ichigo.obj)
    # Texture: Block 0027 -> ichigo2.png
    src_ichigo_tex = os.path.join(textures_dir, "unknown_0027.png")
    dest_ichigo_tex = os.path.join(textures_dir, "ichigo2.png")
    if os.path.exists(src_ichigo_tex):
        shutil.copy2(src_ichigo_tex, dest_ichigo_tex)

    parse_and_convert_mdx_to_obj(
        "FOC-Reborn-Mobile/OriginalAssets/unknown_0186.mdx",
        os.path.join(models_dir, "Ichigo.obj"),
        "ichigo2.png"
    )

    # 2. Rock Lee (unknown_0208.mdx -> RockLee.obj)
    # Texture: Block 0033 -> lock lee.png
    src_lee_tex = os.path.join(textures_dir, "unknown_0033.png")
    dest_lee_tex = os.path.join(textures_dir, "lock_lee.png")
    if os.path.exists(src_lee_tex):
        shutil.copy2(src_lee_tex, dest_lee_tex)

    parse_and_convert_mdx_to_obj(
        "FOC-Reborn-Mobile/OriginalAssets/unknown_0208.mdx",
        os.path.join(models_dir, "RockLee.obj"),
        "lock_lee.png"
    )

    # 3. Saber (unknown_0237.mdx -> Saber.obj)
    parse_and_convert_mdx_to_obj(
        "FOC-Reborn-Mobile/OriginalAssets/unknown_0237.mdx",
        os.path.join(models_dir, "Saber.obj"),
        "ichigo2.png" # Fallback texture
    )

    # 4. Monkey D. Luffy (unknown_0190.mdx -> Luffy.obj)
    src_luffy_tex = os.path.join(textures_dir, "unknown_0035.png")
    dest_luffy_tex = os.path.join(textures_dir, "luffy.png")
    if os.path.exists(src_luffy_tex):
        shutil.copy2(src_luffy_tex, dest_luffy_tex)

    parse_and_convert_mdx_to_obj(
        "FOC-Reborn-Mobile/OriginalAssets/unknown_0190.mdx",
        os.path.join(models_dir, "Luffy.obj"),
        "luffy.png"
    )

if __name__ == '__main__':
    convert_target_heroes()
