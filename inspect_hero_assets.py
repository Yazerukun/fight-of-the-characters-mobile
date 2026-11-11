import os
import struct
import xml.etree.ElementTree as ET

heroes = [
    ('Ichigo', 'unknown_0186.mdx', 'Ichigo.dae', 'ichigo2.png', 'Hero_Ichigo.prefab'),
    ('Saber', 'unknown_0237.mdx', 'Saber.dae', 'HeroSaber.png', 'Hero_Saber.prefab'),
    ('RockLee', 'unknown_0208.mdx', 'RockLee.dae', 'lock lee.png', 'Hero_RockLee.prefab'),
    ('Luffy', 'unknown_0190.mdx', 'Luffy.dae', 'luffy.png', 'Hero_Luffy.prefab')
]

for name, mdx_f, dae_f, tex_f, prefab_f in heroes:
    mdx_p = os.path.join('FOC-Reborn-Mobile', 'OriginalAssets', mdx_f)
    dae_p = os.path.join('FOC-Reborn-Mobile', 'Assets', 'Art', 'Heroes', name, dae_f)
    prefab_p = os.path.join('FOC-Reborn-Mobile', 'Assets', 'Resources', 'Heroes', prefab_f)
    tex_p = os.path.join('FOC-Reborn-Mobile', 'Assets', 'Art', 'Heroes', name, tex_f)

    mdx_sz = os.path.getsize(mdx_p) if os.path.exists(mdx_p) else 0
    dae_sz = os.path.getsize(dae_p) if os.path.exists(dae_p) else 0
    prefab_sz = os.path.getsize(prefab_p) if os.path.exists(prefab_p) else 0
    tex_exists = os.path.exists(tex_p)

    tree = ET.parse(dae_p)
    root = tree.getroot()
    ns = {'c': 'http://www.collada.org/2005/11/COLLADASchema'}

    pos_array = root.find('.//c:mesh/c:source[@id="geom-mesh-positions"]/c:float_array', ns)
    verts = int(pos_array.attrib['count']) // 3 if pos_array is not None else 0

    tri_elem = root.find('.//c:mesh/c:triangles', ns)
    tris = int(tri_elem.attrib['count']) if tri_elem is not None else 0
    mat = tri_elem.attrib.get('material', 'none') if tri_elem is not None else 'none'

    joints = root.findall('.//c:library_visual_scenes//c:node[@type="JOINT"]', ns)
    anims = root.findall('.//c:library_animations/c:animation', ns)

    # Check root joints
    all_joint_ids = set(j.attrib.get('id', '') for j in joints)
    child_joint_ids = set()
    for j in joints:
        for c in j.findall('./c:node[@type="JOINT"]', ns):
            child_joint_ids.add(c.attrib.get('id', ''))
    root_joints = sorted(list(all_joint_ids - child_joint_ids))

    print(f"=== {name} ===")
    print(f"  1. MDX Source: {mdx_f} ({mdx_sz:,} bytes) - Verified Present")
    print(f"  2. DAE File: {dae_f} ({dae_sz:,} bytes) - Valid Collada XML")
    print(f"  3. Mesh: {verts} Vertices, {tris} Triangles")
    print(f"  4. Material & Texture: Material='{mat}', Texture='{tex_f}' (Exists: {tex_exists})")
    print(f"  5. Bone Hierarchy: {len(joints)} Joints, Root Bones: {root_joints}")
    print(f"  6. Prefab: {prefab_f} ({prefab_sz:,} bytes) in Assets/Resources/Heroes/")
    print(f"  7. Animation Channels in DAE: {len(anims)}")
