# FOC REBORN MOBILE — ASSET INVENTORY & PIPELINE AUDIT
## Master Catalog of Extracted Warcraft III FOC 9.1a Assets & Unity Scripts

**Project:** FOC REBORN MOBILE  
**Source Map Archive:** `Fight_of_Characters9.1a(Asia).w3x` (8.37 MB)  
**Total Cataloged Assets:** 623 Extracted Files + Unity Core Scripts  
**Audit Verification Status:** `VERIFIED BY BUILD/TEST` & `VERIFIED BY SOURCE`  
**Date:** 2026-11-11  

---

## 1. CORE MAP DATA & SCRIPTS

| Archive Filename | Size | Format / Content | System Function | Status |
| :--- | :--- | :--- | :--- | :--- |
| `war3map.j` | 1,479,479 bytes | JASS2 Script (32,042 lines) | Complete game logic, triggers, hero spells, damage calculations, and game loop. | `STATIC FILE PRESENT` |
| `war3map.wts` | 6,247 bytes | Warcraft String Table | Localized text, hero names, skill tooltips, and dialogues. | `STATIC FILE PRESENT` |
| `war3map.w3e` | 131,180 bytes | Binary Terrain Elevation | Tile types, cliff heights, and water elevation. | `STATIC FILE PRESENT` |
| `war3map.wpm` | 65,580 bytes | Binary Pathing Map | 32x32 pixel pathing grid (147,456 cells). | `STATIC FILE PRESENT` |
| `war3map.doo` | 42,120 bytes | Binary Doodads | 1,168 static destructibles, trees, lanterns, and arena obstacles. | `STATIC FILE PRESENT` |
| `war3map.w3u` | 248,912 bytes | Custom Unit Data | Hero base stats, attack speed, movement speed, collision size. | `STATIC FILE PRESENT` |
| `war3map.w3a` | 412,680 bytes | Custom Ability Data | Skill cooldowns, mana costs, cast ranges, effect references. | `STATIC FILE PRESENT` |
| `war3map.w3t` | 89,450 bytes | Custom Item Data | Shop inventories, item stats, recipe requirements. | `STATIC FILE PRESENT` |

---

## 2. THE 4 VALIDATION HEROES: 11-POINT ASSET VALIDATION MATRIX

| Validation Point | 1. Ichigo Kurosaki | 2. Saber (Artoria) | 3. Rock Lee | 4. Monkey D. Luffy | Status Label |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1. Original MDX Source** | `unknown_0186.mdx` (143,266 B, 'ichigo 2') | `unknown_0237.mdx` (78,204 B, 'HeroSaber') | `unknown_0208.mdx` (93,261 B, 'lee done') | `unknown_0190.mdx` (241,287 B, 'jmlcqxp18') | `VERIFIED BY SOURCE` |
| **2. Converted DAE Import** | `Ichigo.dae` (399,720 B) | `Saber.dae` (324,624 B) | `RockLee.dae` (217,750 B) | `Luffy.dae` (654,873 B) | `VERIFIED BY BUILD/TEST` |
| **3. Mesh Verts & Tris** | 1,848 Verts / 2,067 Tris | 778 Verts / 1,698 Tris | 669 Verts / 756 Tris | 3,353 Verts / 3,649 Tris | `VERIFIED BY BUILD/TEST` |
| **4. Material & Texture** | `HeroMaterial` -> `ichigo2.png` | `HeroMaterial` -> `saber.png` | `HeroMaterial` -> `lock_lee.png` | `HeroMaterial` -> `luffy.png` | `VERIFIED BY BUILD/TEST` |
| **5. Bone Hierarchy & Bind** | 51 Joints (Error: 0.000100) | 25 Joints (Error: 0.000100) | 17 Joints (Error: 0.000100) | 54 Joints (Error: 0.000100) | `VERIFIED BY BUILD/TEST` |
| **6. SkinnedMeshRenderer** | Target: `geom-mesh` | Target: `geom-mesh` | Target: `geom-mesh` | Target: `geom-mesh` | `STATIC FILE PRESENT` |
| **7. Bones Array & Root** | Root: `Bone_0_hero2`, etc. | Root: `Bone_15_Bone_Root` | Root: `Bone_0_hero2`, etc. | Root: `Bone_0_hero2`, etc. | `VERIFIED BY BUILD/TEST` |
| **8. Animator & Avatar** | Generic Rig / ModelImporter | Generic Rig / ModelImporter | Generic Rig / ModelImporter | Generic Rig / ModelImporter | `STATIC FILE PRESENT` |
| **9. Imported Anim Clips** | 28 Anim Channels | 23 Anim Channels | 13 Anim Channels | 27 Anim Channels | `VERIFIED BY BUILD/TEST` |
| **10. Runtime Playback** | `HeroAnimationVerificationScene` | `HeroAnimationVerificationScene` | `HeroAnimationVerificationScene` | `HeroAnimationVerificationScene` | `BLOCKED` (Unity Editor absent) |
| **11. Weapon/Aura Attach** | 9 Attachments (`ATCH` chunk) | 9 Attachments + 1 Particle | 8 Attachments (`ATCH` chunk) | 8 Attachments (`ATCH` chunk) | `VERIFIED BY SOURCE` |

---

## 3. MODULAR COMBAT FOUNDATION SCRIPTS (UNITY C#)

All scripts reside in `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\Assets\Scripts\Combat\`:

| Script File | Lines | Bytes | Namespace | Role & Verification Status |
| :--- | :--- | :--- | :--- | :--- |
| `DamageCalculation.cs` | 176 | 6,269 | `FOC.CombatSystems` | WC3 attack/armor matrix, armor mitigation formula, spell resistance. [VERIFIED BY SOURCE] |
| `HeroStats.cs` | 305 | 10,653 | `FOC.CombatSystems` | Attributes (STR/AGI/INT), stat gain curves, WC3 move speed clamping (100–522). [VERIFIED BY SOURCE] |
| `CombatController.cs` | 387 | 13,231 | `FOC.CombatSystems` | Combat state machine, attack animation timing, auto-target acquisition. [VERIFIED BY SOURCE] |
| `AbilitySystem.cs` | 188 | 6,003 | `FOC.CombatSystems` | Q/W/E/R/D slot management, mana checks, cooldown timers, cast range validation. [VERIFIED BY SOURCE] |
| `InventorySystem.cs` | 250 | 8,493 | `FOC.CombatSystems` | 6-slot inventory, item actives/passives, stat bonus aggregation. [VERIFIED BY SOURCE] |
| `Abilities/GetsugaTenshoAbility.cs` | 204 | 7,800 | `FOC.CombatSystems.Abilities` | Ported Getsuga Tenshō ability with agility scaling and Bankai Kuroi Getsuga. [VERIFIED BY SOURCE] |
| `ActionCameraController.cs` | 108 | 4,349 | `FOC.CombatSystems` | 3D third-person action camera with trauma screen shake (juice). [VERIFIED BY SOURCE] |
