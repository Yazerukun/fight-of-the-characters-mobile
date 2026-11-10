import os
import shutil
import uuid

resources_dir = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Resources"
heroes_res_dir = os.path.join(resources_dir, "Heroes")
os.makedirs(heroes_res_dir, exist_ok=True)

# Generate folder meta files if not present
def create_folder_meta(folder_path, guid):
    meta_path = folder_path + ".meta"
    if not os.path.exists(meta_path):
        with open(meta_path, 'w', encoding='utf-8') as f:
            f.write(f"fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
        print(f"Created folder meta: {meta_path}")

create_folder_meta(resources_dir, "91a82047192837461928374619283741")
create_folder_meta(heroes_res_dir, "91a82047192837461928374619283742")

heroes = [
    ("Ichigo", "Hero_Ichigo", "81920384719283746192837461928301"),
    ("Saber", "Hero_Saber", "81920384719283746192837461928302"),
    ("RockLee", "Hero_RockLee", "81920384719283746192837461928303"),
    ("Luffy", "Hero_Luffy", "81920384719283746192837461928304")
]

source_base = r"D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Art\Heroes"

for hero_name, target_name, guid in heroes:
    src_prefab = os.path.join(source_base, hero_name, f"{hero_name}.prefab")
    dst_prefab = os.path.join(heroes_res_dir, f"{target_name}.prefab")
    dst_meta = dst_prefab + ".meta"
    
    if os.path.exists(src_prefab):
        with open(src_prefab, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Adjust GameObject name inside prefab YAML if needed
        content = content.replace(f"m_Name: {hero_name}", f"m_Name: {target_name}")
        
        with open(dst_prefab, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Created Resource Prefab: {dst_prefab}")
        
        with open(dst_meta, 'w', encoding='utf-8') as f:
            f.write(f"fileFormatVersion: 2\nguid: {guid}\nPrefabImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
        print(f"Created Resource Meta: {dst_meta}")

print("Resource Prefabs baking completed successfully.")
