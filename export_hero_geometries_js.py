import os
import json
import base64

def parse_obj(obj_path):
    vertices = []
    normals = []
    uvs = []
    faces = []

    with open(obj_path, 'r', encoding='utf-8') as f:
        for line in f:
            if line.startswith('v '):
                parts = line.strip().split()
                vertices.extend([float(parts[1]), float(parts[2]), float(parts[3])])
            elif line.startswith('vn '):
                parts = line.strip().split()
                normals.extend([float(parts[1]), float(parts[2]), float(parts[3])])
            elif line.startswith('vt '):
                parts = line.strip().split()
                uvs.extend([float(parts[1]), float(parts[2])])
            elif line.startswith('f '):
                parts = line.strip().split()[1:]
                for p in parts:
                    sub = p.split('/')
                    v_idx = int(sub[0]) - 1
                    faces.append(v_idx)

    return {
        'vertexCount': len(vertices) // 3,
        'triangleCount': len(faces) // 3,
        'positions': [round(x, 4) for x in vertices],
        'normals': [round(x, 4) for x in normals],
        'uvs': [round(x, 4) for x in uvs],
        'indices': faces
    }

def main():
    models_dir = os.path.join(os.getcwd(), 'FOC-Reborn-Mobile', 'ConvertedAssets', 'Models')
    textures_dir = os.path.join(os.getcwd(), 'FOC-Reborn-Mobile', 'ConvertedAssets', 'Textures')
    output_js = os.path.join(os.getcwd(), 'FOC-Reborn-Mobile', 'UI_UX', 'OriginalHeroMeshAssets.js')

    hero_configs = {
        'ichigo': {
            'obj': os.path.join(models_dir, 'Ichigo.obj'),
            'tex': os.path.join(textures_dir, 'ichigo2.png')
        },
        'rock_lee': {
            'obj': os.path.join(models_dir, 'RockLee.obj'),
            'tex': os.path.join(textures_dir, 'lock_lee.png')
        },
        'saber': {
            'obj': os.path.join(models_dir, 'Saber.obj'),
            'tex': os.path.join(textures_dir, 'ichigo2.png')
        },
        'luffy': {
            'obj': os.path.join(models_dir, 'Luffy.obj'),
            'tex': os.path.join(textures_dir, 'luffy.png')
        }
    }

    hero_mesh_data = {}
    for key, cfg in hero_configs.items():
        if os.path.exists(cfg['obj']):
            data = parse_obj(cfg['obj'])
            tex_b64 = ""
            if os.path.exists(cfg['tex']):
                with open(cfg['tex'], 'rb') as tf:
                    tex_b64 = "data:image/png;base64," + base64.b64encode(tf.read()).decode('ascii')
            data['textureBase64'] = tex_b64
            hero_mesh_data[key] = data
            print(f"Exported {key}: {data['vertexCount']} verts, {data['triangleCount']} tris, texLen={len(tex_b64)}")

    with open(output_js, 'w', encoding='utf-8') as out:
        out.write("/* AUTO-GENERATED FROM ORIGINAL WC3 MDX MODELS & BLP TEXTURES */\n")
        out.write("const ORIGINAL_HERO_MESH_DATA = ")
        json.dump(hero_mesh_data, out)
        out.write(";\n")

    print(f"Geometry database updated: {output_js} ({os.path.getsize(output_js):,} bytes)")

if __name__ == '__main__':
    main()
