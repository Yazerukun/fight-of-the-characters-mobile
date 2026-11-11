# FOC REBORN MOBILE — BASE DATA DEPENDENCY MANIFEST
## Comprehensive Audit of W3AndroidEngine External File Requirements

**Project:** FOC REBORN MOBILE  
**Engine Candidate:** W3AndroidEngine (Warsmash Reforged Java 17 / LibGDX)  
**Document ID:** DEP-MAN-001  
**Author:** JIN (Principal Systems Architect & Lead Game Developer)  
**Date:** 2026-11-11  
**Status:** `VERIFIED BY SOURCE` & `VERIFIED BY BUILD/TEST`  

---

## 1. EXECUTIVE DEPENDENCY SUMMARY

W3AndroidEngine is an open-source reverse-engineered Warcraft III client runtime. Unlike a standalone game engine (e.g. Unity, Godot, Unreal), W3AndroidEngine **does not embed Blizzard's proprietary base game data**. 

When running custom maps (including `Fight_of_Characters9.1a(Asia).w3x`), the engine requires an underlying base game data source (`Folder`, `MPQ`, or `CASC`) to provide core string tables, font metrics, base ability templates, and JASS native prototypes. Custom maps only supply custom models, icons, sound overrides, and `war3map.j` triggers—they do **not** contain Blizzard's base interface definitions.

---

## 2. EXACT CODE PATHS & REQUIRED ASSET MANIFEST

Every base game file requested by the engine was extracted and verified directly from the `D:\W3AndroidEngine\repo\core` source code:

### Category 1: JASS2 Base Language Runtime (MANDATORY FOR HEADLESS & GUI)
Without these files, W3AndroidEngine's ANTLR4 parser (`SmashJassParser`) can tokenize `war3map.j`, but the interpreter **cannot resolve basic types** (`gamecache`, `unit`, `player`, `location`, `trigger`, `timer`), causing an immediate fatal runtime exception during `program.initialize()`.

| Relative Path | Code Location & Invocation Point | Mandatory For Headless? | Mandatory For GUI? | Failure Consequence |
| :--- | :--- | :--- | :--- | :--- |
| `Scripts\common.j` | `WarsmashConstants.java:77`, `Jass2.java:62-108`, `GameReplays.java:56` | **YES** | **YES** | `RuntimeException: Unknown type: gamecache` during `program.initialize()`. |
| `Scripts\Blizzard.j` | `WarsmashConstants.java:77`, `Jass2.java:62-108`, `GameReplays.java:56` | **YES** | **YES** | Missing standard Blizzard helper functions (e.g. `bj_lastCreatedUnit`, camera setups). |
| `Scripts\common.ai` | `GameReplays.java:56` | NO (Campaign/Replay only) | NO | AI script fallback. |

---

### Category 2: Core Object Data & Standard SLK Tables (MANDATORY FOR SIMULATION)
Loaded sequentially by `War3MapViewer.loadSLKs()` and `StandardObjectData.java` during map initialization.

| Relative Path | Code Location & Invocation Point | Mandatory For Headless? | Mandatory For GUI? | Failure Consequence |
| :--- | :--- | :--- | :--- | :--- |
| `TerrainArt\Terrain.slk` | `War3MapViewer.java:431` | **YES** | **YES** | Null pointer / crash when parsing `war3map.w3e` ground tile textures. |
| `TerrainArt\CliffTypes.slk` | `War3MapViewer.java:433` | **YES** | **YES** | Crash when generating cliff elevation geometry. |
| `TerrainArt\Water.slk` | `War3MapViewer.java:435` | **YES** | **YES** | Crash when calculating water height and shore boundaries. |
| `Doodads\Doodads.slk` | `StandardObjectData.java:168`, `War3MapViewer.java:444` | **YES** | **YES** | Cannot resolve doodad types in `war3map.doo`. |
| `Doodads\DoodadMetaData.slk` | `StandardObjectData.java:211`, `War3MapViewer.java:446` | **YES** | **YES** | Cannot parse custom doodad field overrides. |
| `Units\DestructableData.slk` | `StandardObjectData.java:145`, `War3MapViewer.java:448` | **YES** | **YES** | Cannot resolve arena gates, trees, or breakable rocks. |
| `Units\DestructableMetaData.slk`| `StandardObjectData.java:344`, `War3MapViewer.java:450` | **YES** | **YES** | Destructable field parsing error. |
| `Units\UnitData.slk` | `DataFolderCheck.java:67`, `War3MapViewer.java:461` | **YES** | **YES** | **Engine detection gate fails**; base unit classifications unresolvable. |
| `Units\unitUI.slk` | `War3MapViewer.java:463` | **YES** | **YES** | Missing unit model filepaths, scale, and soundsets. |
| `Units\ItemData.slk` | `StandardObjectData.java:122`, `War3MapViewer.java:464` | **YES** | **YES** | Custom items in `war3map.w3t` fail base table inheritance. |
| `Units\UnitMetaData.slk` | `StandardObjectData.java:333`, `War3MapViewer.java:466` | **YES** | **YES** | Cannot parse `war3map.w3u` custom hero attributes. |
| `Units\AbilityData.slk` | `StandardObjectData.java:248` | **YES** | **YES** | Custom abilities in `war3map.w3a` fail base ability prototype lookup. |
| `Units\AbilityMetaData.slk` | `StandardObjectData.java:355` | **YES** | **YES** | Cannot parse `war3map.w3a` custom spell field modifiers. |
| `Units\AbilityBuffData.slk` | `StandardObjectData.java:284` | **YES** | **YES** | Hero buffs and stun/silence status effects fail to instantiate. |
| `Units\AbilityBuffMetaData.slk`| `StandardObjectData.java:366` | **YES** | **YES** | Buff field parsing error. |
| `Units\UpgradeData.slk` | `StandardObjectData.java:310` | **YES** | **YES** | Upgrades in `war3map.w3q` fail. |
| `Units\UpgradeMetaData.slk` | `StandardObjectData.java:377` | **YES** | **YES** | Upgrade field parsing error. |
| `Splats\LightningData.slk` | `War3MapViewer.java:604` | **YES** | **YES** | Beam effects (Chain Lightning, Mana Burn) crash on lookup. |
| `Splats\SplatData.slk` | `EventObjectEmitterObject.java:160` | NO | **YES** | Ground splats and footprints fail. |
| `Splats\UberSplatData.slk` | `EventObjectEmitterObject.java:172` | NO | **YES** | Building/spawn ground scorch marks fail. |
| `Splats\SpawnData.slk` | `EventObjectEmitterObject.java:158` | NO | **YES** | Blood/water particle impact spawns fail. |

---

### Category 3: UI Strings, Skins & Frame Definitions (MANDATORY FOR BOTH HEADLESS & GUI)
*Crucial Architectural Finding:* `HeadlessLauncher` creates `WarsmashGdxMenuScreen`, which initializes the full `MenuUI` and `GameUI` component tree. Therefore, **headless execution currently fails if UI definitions are missing**.

| Relative Path | Code Location & Invocation Point | Mandatory For Headless? | Mandatory For GUI? | Failure Consequence |
| :--- | :--- | :--- | :--- | :--- |
| `UI\WorldEditGameStrings.txt` | `WorldEditStrings.java:26` | **YES** | **YES** | `IllegalStateException: Game data not found: UI\WorldEditGameStrings.txt is missing`. |
| `UI\WorldEditStrings.txt` | `WorldEditStrings.java:17` | Optional | Optional | Falls back to internal defaults if absent. |
| `UI\war3skins.txt` | `GameUI.java:216` | **YES** | **YES** | `FileNotFoundException: UI\war3skins.txt`. |
| `UI\FrameDef\FrameDef.toc` | `MenuUI.java:253`, `MeleeUI.java:437` | **YES** | **YES** | `IllegalStateException: Unable to load FrameDef.toc`. |
| `UI\MiscData.txt` | `War3MapViewer.java:497`, `GlueBackdropRenderer.java:182` | **YES** | **YES** | Missing camera presets, light directions, and selection colors. |
| `Units\MiscData.txt` | `War3MapViewer.java:500` | **YES** | **YES** | Missing gameplay constants (combat math, attack table). |
| `Units\MiscGame.txt` | `War3MapViewer.java:503` | **YES** | **YES** | Missing hero XP curves and leveling factors. |
| `UI\MiscUI.txt` | `War3MapViewer.java:506` | **YES** | **YES** | Missing HUD anchor offsets. |
| `UI\SoundInfo\UISounds.slk` | `War3MapViewer.java:565`, `MenuUI.java:1050` | **YES** | **YES** | Missing interface button click and alert sounds. |
| `UI\SoundInfo\UnitAckSounds.slk` | `War3MapViewer.java:481` | **YES** | **YES** | Missing unit order confirmation voice lines. |
| `UI\SoundInfo\UnitCombatSounds.slk`| `War3MapViewer.java:487` | **YES** | **YES** | Missing sword clash, impact, and flesh hit sound IDs. |
| `UI\SoundInfo\AnimSounds.slk` | `War3MapViewer.java:491` | Optional | Optional | Animation frame sound triggers fail silently if missing. |

---

### Category 4: Typography & TrueType Fonts
| Relative Path | Code Location & Invocation Point | Mandatory For Headless? | Mandatory For GUI? | Failure Consequence |
| :--- | :--- | :--- | :--- | :--- |
| `Fonts\frizqt__.ttf` | `DynamicFontGeneratorHolder.java:30`, `SkinArt.java:285` | **YES** | **YES** | `IllegalStateException: No such font file: ... (for "MasterFont")`. |
| `Fonts\blizzardglobal-v5_4.ttf`| `SkinArt.java:286` | Optional | **YES** | Localized Asian glyph font fallback. |

---

## 3. LOADING ORDER & ENGINE INITIALIZATION SEQUENCE

When `HeadlessLauncher` or `DesktopLauncher` loads `Fight_of_Characters9.1a(Asia).w3x`, execution follows this exact deterministic pipeline:

```
[STEP 0: Data Sources Resolution]
   ├── 1. Read warsmash.ini [DataSources]
   ├── 2. Open CompoundDataSource (Base Folder/CASC + Map MPQ Archive)
   └── 3. Check for UI\WorldEditGameStrings.txt & Units\UnitData.slk

[STEP 1: Engine & Viewer Initialization]
   ├── 1. Instantiate War3MapViewer
   ├── 2. Load WorldEditStrings (UI\WorldEditGameStrings.txt)
   └── 3. Initialize Audio & Render Pipelines (Headless or LWJGL GL30)

[STEP 2: GameUI & Skins Resolution]
   ├── 1. Read UI\war3skins.txt ([Main], [Default] sections)
   ├── 2. Load MasterFont (Fonts\frizqt__.ttf) via FreeTypeFontGenerator
   ├── 3. Load UI\FrameDef\FrameDef.toc & SmashFrameDef.toc
   └── 4. Read Menu Background Model (GlueSpriteLayerBackground)

[STEP 3: SLK Table Parsing (War3MapViewer.loadSLKs)]
   ├── 1. Terrain & Environment: Terrain.slk, CliffTypes.slk, Water.slk
   ├── 2. Doodads & Destructibles: Doodads.slk, DestructableData.slk
   ├── 3. Units, Items & Abilities: UnitData.slk, unitUI.slk, ItemData.slk, AbilityData.slk
   └── 4. Game Balance Constants: UI\MiscData.txt, Units\MiscData.txt, Units\MiscGame.txt

[STEP 4: Map Archive Parsing (War3Map)]
   ├── 1. war3map.w3i (Player slots, teams, map flags)
   ├── 2. war3map.w3e (Tile elevation grid)
   ├── 3. war3map.wpm (384x384 pathing mesh)
   ├── 4. war3map.doo (1,168 static doodads)
   └── 5. war3map.w3u / w3a / w3t (Custom FOC Hero & Ability modifications)

[STEP 5: JASS Compilation & Execution (Jass2.loadCommon)]
   ├── 1. Parse Scripts\common.j (Blizzard native prototypes & type declarations)
   ├── 2. Parse Scripts\Blizzard.j (Standard Blizzard helper functions)
   ├── 3. Parse Scripts\war3map.j (32,042 lines of FOC custom triggers)
   ├── 4. Execute program.initialize() (Global variable initialization)
   ├── 5. Check Unregistered Natives (Verify all native function calls exist)
   └── 6. Execute main() and config() entry points
```

---

## 4. ARCHITECTURAL EVALUATION: HEADLESS VS GUI

* **Can Headless Mode Bypass UI Dependencies?**
  In the current W3AndroidEngine codebase (`v0.3.6`), **NO**. 
  `HeadlessLauncher.java` explicitly delegates to `WarsmashGdxMenuScreen`, which invokes `MenuUI.mainSkins()`. Therefore, `HeadlessLauncher` enforces the exact same UI dependencies (`war3skins.txt`, `FrameDef.toc`, `MasterFont`) as the graphical game client.
* **Can Headless Mode Be Refactored?**
  Yes. A specialized headless harness (such as our `FOCMapDiagnostic.java`) can directly invoke `War3Map` and `SmashJassParser` without touching `MenuUI`. However, running the full game simulation loop still requires `common.j`, `Blizzard.j`, and the SLK data tables.
