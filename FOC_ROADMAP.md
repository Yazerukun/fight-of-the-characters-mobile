# FOC REBORN MOBILE — PROJECT ROADMAP
## Long-Term Milestone Execution Plan

**Project Codename:** FOC REBORN MOBILE  
**Target Delivery:** Faithful Android Mobile Adaptation of Fight of Characters 9.1a (Asia)  
**Primary Engine:** Unity 6 LTS URP Mobile Project (`D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile`)  
**Reference Analytical Harness:** W3AndroidEngine (`D:\W3AndroidEngine\repo`)  
**Last Updated:** 2026-11-11  

---

## MILESTONE OVERVIEW & PROGRESS TRACKER

```
[STAGE 1: W3X EXTRACTION & RIGGING] ──> [STAGE 2: W3ANDROIDENGINE ANALYSIS] ──> [STAGE 3: UNITY COMBAT FOUNDATION]
             (COMPLETED)                                 (COMPLETED)                             (COMPLETED)
                   │
                   ▼
[STAGE 4: JASS TRANSLATION & 4 HEROES] ──> [STAGE 5: ARENA MAP & CREEP WAVES] ──> [STAGE 6: MOBILE HUD & CONTROLS]
             (IN PROGRESS)                                (QUEUED)                               (QUEUED)
                   │
                   ▼
[STAGE 7: ANDROID BUILD & IL2CPP OPTIMIZATION] ──> [STAGE 8: MULTIPLAYER & EXPANSION]
             (QUEUED)                                          (QUEUED)
```

---

### STAGE 1: W3X EXTRACTION & ASSET RIGGING (COMPLETED)
- [x] Extract 623 original game files from `Fight_of_Characters9.1a(Asia).w3x` [VERIFIED BY SOURCE]
- [x] Audit original MDX binary files for 4 heroes (Ichigo, Saber, Rock Lee, Luffy) [VERIFIED BY STATIC TEST]
- [x] Convert MDX models to genuine rigged DAE skeletons [VERIFIED BY BUILD/TEST]
- [x] Verify mathematical deformation and 0 IBM errors via `validate_dae_and_deformation.py` [VERIFIED BY BUILD/TEST]
- [x] Convert BLP textures to 32-bit PNG with preserved alpha channels [VERIFIED BY BUILD/TEST]
- [x] Package initial Unity 6 prefabs in `Assets/Resources/Heroes/` [STATIC FILE PRESENT]

---

### STAGE 2: W3ANDROIDENGINE BASE DATA ANALYSIS (COMPLETED)
- [x] Isolate Gradle build cache to `D:\W3AndroidEngine\gradle-cache` [VERIFIED BY SOURCE]
- [x] Compile W3AndroidEngine desktop classes (8/8 tasks) and pass 191/191 unit tests [VERIFIED BY BUILD/TEST]
- [x] Package standalone fat JAR `desktop-1.0.jar` (55.26 MB) [VERIFIED BY BUILD/TEST]
- [x] Trace base-game dependencies (`war3skins.txt`, `FrameDef.toc`, SLK tables, `common.j`) [VERIFIED BY SOURCE]
- [x] Parse authentic FOC map binary structures (`war3map.w3e`, `wpm`, `doo`, `j`) [VERIFIED BY BUILD/TEST]
- [x] Document legal base-data dependency blocker and confirm standalone Unity 6 superiority [VERIFIED BY SOURCE]

---

### STAGE 3: UNITY RUNTIME VALIDATION & COMBAT FOUNDATION (COMPLETED)
- [x] Phase 1: Environment Preflight Audit on host workstation [VERIFIED BY BUILD/TEST]
- [x] Phase 1: Formally record Unity Editor executable absence as `BLOCKED` for live editor tests [BLOCKED]
- [x] Phase 2: Document 11-point asset validation matrix for 4 validation heroes [VERIFIED BY BUILD/TEST]
- [x] Phase 3: Inspect `HeroAnimationVerificationScene.unity` and record live playback as `BLOCKED` [STATIC FILE PRESENT / BLOCKED]
- [x] Phase 4: Implement modular combat foundation in C#:
  - [x] `DamageCalculation.cs`: WC3 attack/armor matrix, armor mitigation, spell resistance [VERIFIED BY SOURCE]
  - [x] `HeroStats.cs`: Attributes (STR/AGI/INT), stat scaling, vitals, WC3 speed clamps [VERIFIED BY SOURCE]
  - [x] `CombatController.cs`: State machine, attack interval timing, auto-acquisition [VERIFIED BY SOURCE]
  - [x] `AbilitySystem.cs`: Q/W/E/R/D slots, mana deduction, cooldown radial timers [VERIFIED BY SOURCE]
  - [x] `InventorySystem.cs`: 6-slot inventory, item actives/passives, stat bonus aggregation [VERIFIED BY SOURCE]
- [x] Phase 5: JASS Ability Porting Strategy & First Selected Ability:
  - [x] Trace JASS trigger functions `bOe` and `Bbe` from `war3map.j` [VERIFIED BY SOURCE]
  - [x] Implement `GetsugaTenshoAbility.cs` with agility scaling and Bankai Kuroi Getsuga [VERIFIED BY SOURCE]
  - [x] Author JASS native-to-C# transpilation matrix and unsupported native catalog [VERIFIED BY SOURCE]

---

### STAGE 4: JASS TRANSLATION & 4 HERO COMPLETE SKILLSETS (UPCOMING)
- [ ] Port remaining Ichigo skills (Flash Step, Hollow Mask, Bankai: Tensa Zangetsu) [NOT TESTED]
- [ ] Port Saber skills (Invisible Air, Mana Burst, Instinct, Excalibur) [NOT TESTED]
- [ ] Port Rock Lee skills (Leaf Hurricane, Dynamic Entry, Eight Gates, Primary Lotus) [NOT TESTED]
- [ ] Port Luffy skills (Gomu Gomu Pistol, Bazooka, Gear Second, Gear Third) [NOT TESTED]
- [ ] Validate combat balance against original FOC 9.1a damage metrics [NOT TESTED]
