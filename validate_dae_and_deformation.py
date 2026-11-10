import os
import xml.etree.ElementTree as ET
import numpy as np

def validate_dae(dae_path):
    print(f"==================================================")
    print(f"VALIDATING DAE: {os.path.basename(dae_path)}")
    print(f"File path: {dae_path}")
    print(f"File size: {os.path.getsize(dae_path):,} bytes")
    print(f"==================================================")

    tree = ET.parse(dae_path)
    root = tree.getroot()

    # Namespace handling
    ns = ""
    if root.tag.startswith("{"):
        ns = root.tag.split("}")[0] + "}"

    failures = []
    warnings = []

    # 1. GEOMETRY
    geometries = root.findall(f".//{ns}geometry")
    print(f"[1. GEOMETRY]")
    print(f"  Geometries count: {len(geometries)}")
    if len(geometries) == 0:
        failures.append("No <geometry> found in DAE")
        return failures

    geom = geometries[0]
    mesh = geom.find(f"{ns}mesh")
    pos_arr = mesh.find(f".//{ns}float_array[@id='HeroMesh-positions-array']")
    norm_arr = mesh.find(f".//{ns}float_array[@id='HeroMesh-normals-array']")
    uv_arr = mesh.find(f".//{ns}float_array[@id='HeroMesh-map-0-array']")
    triangles = mesh.find(f"{ns}triangles")

    pos_vals = [float(x) for x in pos_arr.text.split()] if pos_arr is not None and pos_arr.text else []
    norm_vals = [float(x) for x in norm_arr.text.split()] if norm_arr is not None and norm_arr.text else []
    uv_vals = [float(x) for x in uv_arr.text.split()] if uv_arr is not None and uv_arr.text else []

    num_verts = len(pos_vals) // 3
    num_normals = len(norm_vals) // 3
    num_uvs = len(uv_vals) // 2

    print(f"  Vertex count: {num_verts}")
    print(f"  Normal count: {num_normals}")
    print(f"  UV count: {num_uvs}")

    tri_count = int(triangles.attrib.get('count', 0)) if triangles is not None else 0
    mat_symbol = triangles.attrib.get('material', 'None') if triangles is not None else 'None'
    p_elem = triangles.find(f"{ns}p") if triangles is not None else None
    indices = [int(x) for x in p_elem.text.split()] if p_elem is not None and p_elem.text else []

    print(f"  Triangles count: {tri_count}")
    print(f"  Material assignment: '{mat_symbol}'")
    print(f"  Total index elements in <p>: {len(indices)} (Expected: {tri_count * 9})")

    if len(indices) != tri_count * 9:
        failures.append(f"Triangle index count mismatch: got {len(indices)}, expected {tri_count * 9}")

    # Check index bounds
    for i in range(0, len(indices), 3):
        v_idx = indices[i]
        n_idx = indices[i+1]
        uv_idx = indices[i+2]
        if v_idx >= num_verts:
            failures.append(f"Vertex index out of bounds: {v_idx} >= {num_verts}")
            break
        if n_idx >= num_normals:
            failures.append(f"Normal index out of bounds: {n_idx} >= {num_normals}")
            break
        if uv_idx >= num_uvs:
            failures.append(f"UV index out of bounds: {uv_idx} >= {num_uvs}")
            break

    # 2. CONTROLLERS & SKIN
    print(f"\n[2. CONTROLLER & SKINNING]")
    controllers = root.findall(f".//{ns}controller")
    print(f"  Controllers count: {len(controllers)}")
    if len(controllers) == 0:
        failures.append("No <controller> found in DAE")
        return failures

    skin = controllers[0].find(f"{ns}skin")
    joints_elem = skin.find(f".//{ns}Name_array[@id='HeroSkin-joints-array']")
    joint_names = joints_elem.text.split() if joints_elem is not None and joints_elem.text else []
    print(f"  Joints in skin controller: {len(joint_names)}")

    # Inverse bind matrices
    ibm_elem = skin.find(f".//{ns}float_array[@id='HeroSkin-bind_poses-array']")
    ibm_vals = [float(x) for x in ibm_elem.text.split()] if ibm_elem is not None and ibm_elem.text else []
    print(f"  Inverse bind matrix floats: {len(ibm_vals)} (Expected: {len(joint_names) * 16})")

    if len(ibm_vals) != len(joint_names) * 16:
        failures.append(f"IBM count mismatch: {len(ibm_vals)} vs {len(joint_names)*16}")

    # Check IBM validity (determinant != 0)
    ibm_matrices = []
    for j in range(len(joint_names)):
        m = np.array(ibm_vals[j*16 : (j+1)*16]).reshape(4, 4)
        det = np.linalg.det(m)
        if abs(det) < 1e-6 or np.isnan(det) or np.isinf(det):
            failures.append(f"Joint {joint_names[j]} has degenerate or invalid inverse bind matrix (det={det})")
        ibm_matrices.append(m)

    # Skin weights
    weight_elem = skin.find(f".//{ns}float_array[@id='HeroSkin-weights-array']")
    weights = [float(x) for x in weight_elem.text.split()] if weight_elem is not None and weight_elem.text else []
    print(f"  Weights array elements: {len(weights)} (values: {weights[:5]})")

    # Vertex weights
    vw_elem = skin.find(f"{ns}vertex_weights")
    vw_count = int(vw_elem.attrib.get('count', 0)) if vw_elem is not None else 0
    vcount_elem = vw_elem.find(f"{ns}vcount") if vw_elem is not None else None
    v_elem = vw_elem.find(f"{ns}v") if vw_elem is not None else None

    vcounts = [int(x) for x in vcount_elem.text.split()] if vcount_elem is not None and vcount_elem.text else []
    v_pairs = [int(x) for x in v_elem.text.split()] if v_elem is not None and v_elem.text else []

    print(f"  Vertex weights vertex count: {vw_count} (Matches vertex count: {vw_count == num_verts})")
    if vw_count != num_verts:
        failures.append(f"Skin vertex count mismatch: {vw_count} vs {num_verts}")

    # Check normalization and valid joint references
    p_v = 0
    non_normalized = 0
    invalid_joints = 0
    influences_per_vertex = []

    for v_i in range(vw_count):
        cnt = vcounts[v_i]
        influences_per_vertex.append(cnt)
        w_sum = 0.0
        for _ in range(cnt):
            j_idx = v_pairs[p_v]
            w_idx = v_pairs[p_v + 1]
            p_v += 2
            if j_idx < 0 or j_idx >= len(joint_names):
                invalid_joints += 1
            if w_idx < 0 or w_idx >= len(weights):
                failures.append(f"Vertex {v_i} weight index out of bounds: {w_idx}")
            else:
                w_val = weights[w_idx]
                if not np.isfinite(w_val) or w_val < 0.0:
                    failures.append(f"Vertex {v_i} non-finite or negative weight: {w_val}")
                w_sum += w_val
        if abs(w_sum - 1.0) > 1e-4:
            non_normalized += 1

    print(f"  Vertex influence count distribution: min={min(influences_per_vertex)}, max={max(influences_per_vertex)}, avg={np.mean(influences_per_vertex):.1f}")
    print(f"  Non-normalized vertex weights: {non_normalized}")
    print(f"  Invalid joint index references: {invalid_joints}")

    if non_normalized > 0:
        failures.append(f"{non_normalized} vertices have non-normalized weights")
    if invalid_joints > 0:
        failures.append(f"{invalid_joints} vertex weight references point to invalid joint indices")

    # 3. VISUAL SCENE & JOINT HIERARCHY
    print(f"\n[3. SKELETAL JOINT HIERARCHY]")
    visual_scene = root.find(f".//{ns}visual_scene")
    scene_nodes = visual_scene.findall(f".//{ns}node[@type='JOINT']") if visual_scene is not None else []
    print(f"  Total <node type='JOINT'> in visual scene: {len(scene_nodes)}")

    scene_joint_names = set(n.attrib.get('id', n.attrib.get('name')) for n in scene_nodes)
    missing_joints = [j for j in joint_names if j not in scene_joint_names]
    print(f"  Missing joints in visual scene: {len(missing_joints)}")
    if len(missing_joints) > 0:
        failures.append(f"{len(missing_joints)} joints referenced in skin are missing from visual scene: {missing_joints[:5]}")

    # Build hierarchy
    def get_joint_hierarchy(elem, parent=None):
        hierarchy = {}
        for child in elem:
            if child.tag.endswith('node') and child.attrib.get('type') == 'JOINT':
                jid = child.attrib.get('id', child.attrib.get('name'))
                trans_elem = child.find(f"{ns}translate")
                mat_elem = child.find(f"{ns}matrix")
                
                local_m = np.identity(4)
                if mat_elem is not None and mat_elem.text:
                    m_floats = [float(x) for x in mat_elem.text.split()]
                    local_m = np.array(m_floats).reshape((4, 4))
                elif trans_elem is not None and trans_elem.text:
                    trans = [float(x) for x in trans_elem.text.split()]
                    local_m[0, 3] = trans[0]
                    local_m[1, 3] = trans[1]
                    local_m[2, 3] = trans[2]

                sub = get_joint_hierarchy(child, jid)
                hierarchy[jid] = {'parent': parent, 'matrix': local_m, 'children': sub}
        return hierarchy

    armature = visual_scene.find(f".//{ns}node[@id='Armature']") if visual_scene is not None else None
    if armature is not None:
        tree_map = get_joint_hierarchy(armature)
        roots = list(tree_map.keys())
        print(f"  Root joints in Armature: {roots}")
    else:
        failures.append("No Armature node found in visual scene")

    # 4. BIND POSE RECONSTRUCTION TEST
    print(f"\n[4. DEFORMATION & BIND POSE AUDIT]")
    # Calculate global rest transform for joints
    global_transforms = {}

    def compute_global(joint_dict, parent_global=np.identity(4)):
        for jid, data in joint_dict.items():
            local_m = data['matrix']
            glob = parent_global @ local_m
            global_transforms[jid] = glob
            compute_global(data['children'], glob)

    if armature is not None:
        compute_global(tree_map)
        print(f"  Computed global rest matrices for {len(global_transforms)} joints.")

        # Check: global_transform @ inverse_bind_matrix should be approximately Identity
        max_ibm_error = 0.0
        for j_idx, j_name in enumerate(joint_names):
            if j_name in global_transforms:
                g_m = global_transforms[j_name]
                inv_m = ibm_matrices[j_idx]
                product = g_m @ inv_m
                diff = np.max(np.abs(product - np.identity(4)))
                if diff > max_ibm_error:
                    max_ibm_error = diff
        print(f"  Max (Global_Rest @ InverseBindMatrix - Identity) deviation: {max_ibm_error:.6f}")
        if max_ibm_error > 1e-2:
            warnings.append(f"Max IBM deviation is {max_ibm_error:.4f} (rest pose vs IBM)")

    # 5. ANIMATIONS CHUNK AUDIT
    print(f"\n[5. ANIMATION CHUNKS AUDIT]")
    lib_anim = root.find(f".//{ns}library_animations")
    if lib_anim is not None:
        anims = lib_anim.findall(f"{ns}animation")
        print(f"  Embedded DAE <animation> elements: {len(anims)}")
        total_channels = 0
        invalid_channel_targets = 0
        unordered_timestamps = 0
        total_sampled_keys = 0

        for a in anims:
            channel = a.find(f"{ns}channel")
            if channel is not None:
                total_channels += 1
                target = channel.attrib.get('target', '')
                j_target = target.split('/')[0] if '/' in target else target
                if j_target not in scene_joint_names:
                    invalid_channel_targets += 1
                    failures.append(f"Animation channel targets non-existent joint: {target}")

            # Check timestamps ordering and validity
            t_source = None
            for s in a.findall(f"{ns}source"):
                if '-input' in s.attrib.get('id', ''):
                    t_source = s
                    break
            if t_source is not None:
                fa = t_source.find(f"{ns}float_array")
                if fa is not None and fa.text:
                    t_vals = [float(x) for x in fa.text.split()]
                    total_sampled_keys += len(t_vals)
                    for i in range(len(t_vals) - 1):
                        if t_vals[i+1] < t_vals[i]:
                            unordered_timestamps += 1
                            failures.append(f"Unordered animation timestamp in {a.attrib.get('id')}: {t_vals[i]} > {t_vals[i+1]}")
                            break

        print(f"  Animation channels validated: {total_channels}")
        print(f"  Total keyframe matrices sampled: {total_sampled_keys}")
        print(f"  Invalid channel targets: {invalid_channel_targets}")
        print(f"  Unordered timestamps: {unordered_timestamps}")
    else:
        print(f"  Embedded DAE <library_animations>: None (Animations sliced via ModelImporter in .meta)")

    print(f"\n--- VALIDATION SUMMARY FOR {os.path.basename(dae_path)} ---")
    print(f"Failures: {len(failures)}")
    for f in failures:
        print(f"  [FAIL] {f}")
    print(f"Warnings: {len(warnings)}")
    for w in warnings:
        print(f"  [WARN] {w}")
    if len(failures) == 0:
        print(f"  >>> DAE INTEGRITY PASS: 100% VALID RIG & SKINNING STRUCTURE <<<")
    return failures

heroes_dir = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes"
all_fails = {}
for h in ['Ichigo', 'Saber', 'RockLee', 'Luffy']:
    p = os.path.join(heroes_dir, h, f"{h}.dae")
    if os.path.exists(p):
        fails = validate_dae(p)
        all_fails[h] = fails
    else:
        print(f"File not found: {p}")

print("\n==================================================")
print(f"TOTAL VALIDATION FAILURES ACROSS 4 HEROES: {sum(len(f) for f in all_fails.values())}")
print("==================================================")
