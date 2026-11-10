import os
import hashlib

def get_guid(name):
    return hashlib.md5(name.encode('utf-8')).hexdigest()

heroes_seqs = {
    'Ichigo': [
        ('Attack_1', 1667, 2667, False),
        ('Attack_2', 3000, 4000, False),
        ('Walk', 5667, 6333, True),
        ('Death', 6667, 7667, False),
        ('Dissipate', 8000, 9667, False),
        ('Stand', 10000, 11000, True),
        ('Morph', 15000, 16333, False),
        ('Attack_Alternate_1', 18333, 19333, False),
        ('Attack_Alternate_2', 19667, 20667, False),
        ('Walk_Alternate', 22333, 23000, True),
        ('Death_Alternate', 23333, 24333, False),
        ('Stand_Alternate', 26667, 27667, True)
    ],
    'Saber': [
        ('Stand_2', 2333, 5867, True),
        ('Attack', 14333, 15500, False),
        ('Attack_Slam', 17000, 18133, False),
        ('Death', 22000, 23900, False),
        ('Walk', 28767, 29500, True),
        ('Stand', 30500, 31667, True),
        ('Attack_2', 31833, 32967, False),
        ('Stand_Ready', 33133, 34533, True),
        ('Spell_Excalibur', 66500, 67900, False)
    ],
    'RockLee': [
        ('Stand_Ready', 333, 1333, True),
        ('Stand', 1667, 2667, True),
        ('Attack', 3000, 4000, False),
        ('Attack_3', 4333, 5333, False),
        ('Walk', 5667, 6367, True),
        ('Spell_DynamicEntry', 7000, 8000, False),
        ('Death', 9667, 10667, False),
        ('Spell_Throw', 13333, 14333, False),
        ('Morph_EightGates', 14400, 15900, False),
        ('Attack_Spin', 16000, 16533, False),
        ('Attack_2', 17333, 18333, False),
        ('Spell_HiddenLotus', 18667, 20500, False)
    ],
    'Luffy': [
        ('Stand', 333, 1333, True),
        ('Stand_Ready', 1667, 2667, True),
        ('Attack', 3000, 4000, False),
        ('Attack_2', 4333, 5333, False),
        ('Walk', 7000, 7833, True),
        ('Death', 8000, 9000, False),
        ('Spell_Pistol', 14667, 14838, False),
        ('Spell_Channel', 12333, 13667, False),
        ('Morph_GearSecond', 23505, 24505, False),
        ('Stand_Alternate', 33172, 34172, True),
        ('Attack_Alternate', 35838, 36838, False),
        ('Walk_Alternate', 39838, 40672, True)
    ]
}

heroes_dir = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes"

for hero, seqs in heroes_seqs.items():
    h_dir = os.path.join(heroes_dir, hero)
    dae_path = os.path.join(h_dir, f"{hero}.dae")
    meta_path = f"{dae_path}.meta"

    guid = get_guid(f"{hero}_dae_asset")
    clips_yaml = []

    for sname, start_ms, end_ms, is_loop in seqs:
        start_frame = round(start_ms / 1000.0 * 30.0)
        end_frame = round(end_ms / 1000.0 * 30.0)
        loop_val = 1 if is_loop else 0
        clip_entry = f"""    - serializedVersion: 2
      name: {sname}
      takeName: Scene
      firstFrame: {start_frame}
      lastFrame: {end_frame}
      wrapMode: 0
      orientationOffsetY: 0
      level: 0
      cycleOffset: 0
      loop: {loop_val}
      hasAdditiveReferencePose: 0
      loopTime: {loop_val}
      loopBlend: 0
      loopBlendOrientation: 0
      loopBlendPositionY: 0
      loopBlendPositionXZ: 0
      keepOriginalOrientation: 0
      keepOriginalPositionY: 1
      keepOriginalPositionXZ: 0
      heightFromFeet: 0
      mirror: 0"""
        clips_yaml.append(clip_entry)

    clips_block = "\n".join(clips_yaml)

    meta_content = f"""fileFormatVersion: 2
guid: {guid}
ModelImporter:
  serializedVersion: 22200
  internalIDToNameTable: []
  externalObjects: {{}}
  materials:
    materialImportMode: 1
    materialName: 0
    materialSearch: 1
    materialLocation: 1
  animations:
    legacyGenerateAnimations: 4
    bakeSimulation: 0
    resampleCurves: 1
    optimizeGameObjects: 0
    motionNodeName: 
    rigImportErrors: 
    rigImportWarnings: 
    animationImportErrors: 
    animationImportWarnings: 
    animationRetargetingWarnings: 
    animationDoRetargetingNotes: 
    animatedQuaternions: 0
    animationType: 2
    humanoidOversampling: 1
    avatarSetup: 1
    addHumanoidExtraRootOnlyWhenTheAvatarIsPrepared: 1
    additionalBone: 0
    userData: 
    assetBundleName: 
    assetBundleVariant: 
    clipAnimations:
{clips_block}
  meshes:
    skinWeightsMode: 0
    bakeAxisConversion: 0
  importAnimation: 1
"""
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)
    print(f"Generated ModelImporter meta for {hero}.dae with {len(seqs)} animation clips!")
