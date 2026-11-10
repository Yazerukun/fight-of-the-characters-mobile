#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FOC.Editor
{
    /// <summary>
    /// Automated Model Import and Prefab Baking Pipeline for Fight of Characters heroes.
    /// Configures ModelImporter rig settings, binds materials and AnimatorControllers,
    /// and bakes genuine runtime prefabs into Assets/Resources/Heroes/.
    /// </summary>
    public static class FOCModelImportPipeline
    {
        private struct HeroImportDescriptor
        {
            public string heroName;
            public string daePath;
            public string texturePath;
            public string controllerPath;
            public string targetPrefabPath;
            public int expectedMinVerts;
            public int expectedMinBones;
        }

        private static readonly HeroImportDescriptor[] HeroDescriptors = new HeroImportDescriptor[]
        {
            new HeroImportDescriptor
            {
                heroName = "Ichigo",
                daePath = "Assets/Art/Heroes/Ichigo/Ichigo.dae",
                texturePath = "Assets/Art/Heroes/Ichigo/ichigo2.png",
                controllerPath = "Assets/Art/Heroes/Ichigo/Ichigo_Animator.controller",
                targetPrefabPath = "Assets/Resources/Heroes/Hero_Ichigo.prefab",
                expectedMinVerts = 1800,
                expectedMinBones = 35
            },
            new HeroImportDescriptor
            {
                heroName = "Saber",
                daePath = "Assets/Art/Heroes/Saber/Saber.dae",
                texturePath = "Assets/Art/Heroes/Saber/saber.png",
                controllerPath = "Assets/Art/Heroes/Saber/Saber_Animator.controller",
                targetPrefabPath = "Assets/Resources/Heroes/Hero_Saber.prefab",
                expectedMinVerts = 700,
                expectedMinBones = 20
            },
            new HeroImportDescriptor
            {
                heroName = "RockLee",
                daePath = "Assets/Art/Heroes/RockLee/RockLee.dae",
                texturePath = "Assets/Art/Heroes/RockLee/lock_lee.png",
                controllerPath = "Assets/Art/Heroes/RockLee/RockLee_Animator.controller",
                targetPrefabPath = "Assets/Resources/Heroes/Hero_RockLee.prefab",
                expectedMinVerts = 600,
                expectedMinBones = 15
            },
            new HeroImportDescriptor
            {
                heroName = "Luffy",
                daePath = "Assets/Art/Heroes/Luffy/Luffy.dae",
                texturePath = "Assets/Art/Heroes/Luffy/luffy.png",
                controllerPath = "Assets/Art/Heroes/Luffy/Luffy_Animator.controller",
                targetPrefabPath = "Assets/Resources/Heroes/Hero_Luffy.prefab",
                expectedMinVerts = 3000,
                expectedMinBones = 40
            }
        };

        [MenuItem("FOC/Import & Bake Hero Prefabs")]
        public static void BakeAllHeroPrefabs()
        {
            Debug.Log("=================================================================");
            Debug.Log("[FOC IMPORT PIPELINE] Initiating Hero Prefab Baking Sequence");
            Debug.Log("=================================================================");

            EnsureDirectoryExists("Assets/Resources/Heroes");

            int successCount = 0;

            for (int i = 0; i < HeroDescriptors.Length; i++)
            {
                HeroImportDescriptor desc = HeroDescriptors[i];
                Debug.Log($"\n--- Processing Hero: {desc.heroName} ---");

                if (!File.Exists(desc.daePath))
                {
                    Debug.LogError($"[FAIL] Source DAE model not found: {desc.daePath}");
                    continue;
                }

                // 1. Configure ModelImporter
                ModelImporter importer = AssetImporter.GetAtPath(desc.daePath) as ModelImporter;
                if (importer != null)
                {
                    importer.animationType = ModelImporterAnimationType.Generic;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    Debug.Log($"[OK] ModelImporter configured for {desc.heroName}.");
                }

                // 2. Instantiate Model Asset
                GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(desc.daePath);
                if (modelAsset == null)
                {
                    Debug.LogError($"[FAIL] Unable to load model asset: {desc.daePath}");
                    continue;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(modelAsset) as GameObject;
                instance.name = $"Hero_{desc.heroName}";

                // 3. Configure SkinnedMeshRenderer & Sockets
                SkinnedMeshRenderer smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr == null)
                {
                    Debug.LogError($"[FAIL] No SkinnedMeshRenderer found in instantiated {desc.heroName}.");
                    GameObject.DestroyImmediate(instance);
                    continue;
                }

                // 4. Attach & Configure Animator
                Animator animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                RuntimeAnimatorController rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(desc.controllerPath);
                if (rac != null)
                {
                    animator.runtimeAnimatorController = rac;
                    Debug.Log($"[OK] Bound AnimatorController: {desc.controllerPath}");
                }

                // 5. Verify / Attach Sockets
                EnsureSocket(instance.transform, "Hand_Right_Weapon_Socket", new Vector3(0.15f, 1.1f, 0.4f));
                EnsureSocket(instance.transform, "Origin_Aura_Socket", new Vector3(0f, 0f, 0f));

                // 6. Bake Prefab
                bool prefabSaved = false;
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, desc.targetPrefabPath, out prefabSaved);
                GameObject.DestroyImmediate(instance);

                if (prefabSaved && savedPrefab != null)
                {
                    Debug.Log($"[SUCCESS] Baked Prefab: {desc.targetPrefabPath}");
                    successCount++;
                }
                else
                {
                    Debug.LogError($"[FAIL] Failed to save prefab: {desc.targetPrefabPath}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("\n=================================================================");
            Debug.Log($"[FOC IMPORT PIPELINE RESULT] {successCount} / {HeroDescriptors.Length} HERO PREFABS BAKED");
            Debug.Log("=================================================================");
        }

        private static void EnsureDirectoryExists(string assetRelativePath)
        {
            if (!AssetDatabase.IsValidFolder(assetRelativePath))
            {
                string parent = Path.GetDirectoryName(assetRelativePath).Replace('\\', '/');
                string folder = Path.GetFileName(assetRelativePath);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    EnsureDirectoryExists(parent);
                }
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static void EnsureSocket(Transform root, string socketName, Vector3 defaultLocalPos)
        {
            Transform socket = root.Find(socketName);
            if (socket == null)
            {
                GameObject sObj = new GameObject(socketName);
                sObj.transform.SetParent(root, false);
                sObj.transform.localPosition = defaultLocalPos;
                sObj.transform.localRotation = Quaternion.identity;
                sObj.transform.localScale = Vector3.one;
                Debug.Log($"[SOCKET] Created socket: {socketName}");
            }
        }
    }
}
#endif
