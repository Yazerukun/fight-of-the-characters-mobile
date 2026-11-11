# FOC REBORN MOBILE — PROJECT ROADMAP
## Long-Term Milestone Execution Plan

**Project Codename:** FOC REBORN MOBILE  
**Target Delivery:** Faithful Android Mobile Adaptation of Fight of Characters 9.1a (Asia)  
**Primary Engine Candidate:** W3AndroidEngine (Warsmash Reforged Native)  
**Preserved Fallback Engine:** Unity 6 LTS URP Mobile Project  
**Last Updated:** 2026-11-11  

---

## MILESTONE OVERVIEW & PROGRESS TRACKER

```
[STAGE 1: ENGINE & MAP PREFLIGHT] ──> [STAGE 2: ASSET & RIG PIPELINE] ──> [STAGE 3: MAP & TERRAIN LOADING]
             (IN PROGRESS)                             (COMPLETED)                         (QUEUED)
                   │
                   ▼
[STAGE 4: JASS VM & CORE MECHANICS] ──> [STAGE 5: COMBAT & 4 HEROES] ──> [STAGE 6: MOBILE HUD & CONTROLS]
             (QUEUED)                                  (QUEUED)                            (QUEUED)
                   │
                   ▼
[STAGE 7: ANDROID BUILD & OPTIMIZATION] ──> [STAGE 8: MULTIPLAYER & EXPANSION]
             (QUEUED)                                      (QUEUED)
```

---

### STAGE 1: ENGINE & MAP PREFLIGHT (CURRENT ACTIVE MILESTONE)
*Goal: Validate engine candidates, establish zero-risk workspace boundaries, and test desktop build capabilities.*
- [x] Establish dedicated workspace on Drive D: (`D:\W3AndroidEngine`) to protect constrained Drive C: [VERIFIED BY SOURCE]
- [x] Clone W3AndroidEngine repository (`commit a865a389`, v0.3.6) [VERIFIED BY SOURCE]
- [x] Verify Java 17 OpenJDK preflight on host machine (`17.0.20.1`) [VERIFIED BY BUILD]
- [x] Isolate Gradle build cache to `D:\W3AndroidEngine\gradle-cache` [VERIFIED BY SOURCE]
- [ ] Build W3AndroidEngine desktop classes via Gradle (`gradlew desktop:classes`) [NOT TESTED]
- [ ] Investigate W3AndroidEngine base asset dependencies (CASC / MPQ vs standalone map loading) [NOT TESTED]
- [ ] Test loading a sample WC3 map in desktop W3AndroidEngine [NOT TESTED]
- [ ] Test loading `Fight_of_Characters9.1a(Asia).w3x` in desktop W3AndroidEngine [NOT TESTED]
- **Gate 1 Exit Criteria:** Desktop engine executable compiles and successfully parses an authentic Warcraft III `.w3x` archive without crashing.

---

### STAGE 2: ASSET & RIG PIPELINE (COMPLETED)
*Goal: Extract all original assets from `Fight_of_Characters9.1a(Asia).w3x`, parse binary structures, and generate verified rigs.*
- [x] Extract 623 original assets from authentic W3X map archive [VERIFIED BY SOURCE]
- [x] Audit original MDX binary files for 4 validation heroes (Ichigo, Saber, Rock Lee, Luffy) [VERIFIED BY STATIC TEST]
- [x] Decode bone hierarchies, geosets, sequences, and attachment sockets [VERIFIED BY STATIC TEST]
- [x] Build reproducible MDX-to-Rigged-DAE pipeline preserving 100% vertex geometry and IBM matrices [VERIFIED BY STATIC TEST]
- [x] Verify zero floating-point geometry drift ($\text{MAE} \le 0.000034$) [VERIFIED BY STATIC TEST]
- [x] Convert textures from BLP to 32-bit uncompressed PNG [VERIFIED BY SOURCE]
- [x] Bake clean Unity 6 YAML prefabs with generic rigs and SkinnedMeshRenderer hierarchies [STATIC FILE PRESENT]
- [x] Push complete Unity 6 URP backup to GitHub repository (`4ad4d7e`) [VERIFIED BY SOURCE]
- **Gate 2 Exit Criteria:** Complete asset inventory cataloged, 4 hero rigs mathematically verified against original MDX data.

---

### STAGE 3: MAP & TERRAIN LOADING (QUEUED)
*Goal: Load and render authentic FOC terrain, tilesets, cliffs, water, and static doodads.*
- [ ] Parse `war3map.w3e` (terrain elevation, tileset textures, and cliff levels) [NOT TESTED]
- [ ] Parse `war3map.wpm` (pathing map: walkable, flyable, buildable cells) [NOT TESTED]
- [ ] Parse `war3map.doo` (static doodad and destructible positions) [NOT TESTED]
- [ ] Render authentic FOC arena layout with correct camera boundaries and fog of war [NOT TESTED]
- **Gate 3 Exit Criteria:** Map renders visually identical to Warcraft III FOC arena with authentic pathing mesh active.

---

### STAGE 4: JASS VM & CORE SYSTEMS (QUEUED)
*Goal: Execute game script logic, unit definitions, item tables, and ability data.*
- [ ] Parse `war3map.j` (32,042 lines of authentic JASS2 triggers) [STATIC FILE PRESENT]
- [ ] Parse `war3map.w3u` (custom unit data definitions) [STATIC FILE PRESENT]
- [ ] Parse `war3map.w3a` (custom ability data definitions) [STATIC FILE PRESENT]
- [ ] Parse `war3map.w3t` (custom item data definitions) [STATIC FILE PRESENT]
- [ ] Execute native Warcraft III API functions inside JASS virtual machine [NOT TESTED]
- [ ] Initialize player slots, hero selection pedestals, and tavern mechanisms [NOT TESTED]
- **Gate 4 Exit Criteria:** Game script initializes without fatal runtime exceptions, enters hero selection phase.

---

### STAGE 5: COMBAT & 4 VALIDATION HEROES (QUEUED)
*Goal: Full playable combat loops for Ichigo, Saber, Rock Lee, and Monkey D. Luffy.*
- [ ] **Ichigo Kurosaki:**
  - Base attack animations and projectile attachments [NOT TESTED]
  - Getsuga Tenshō (Q) skill execution with authentic damage scaling [NOT TESTED]
  - Flash Step (W) instant repositioning [NOT TESTED]
  - Bankai transformation (R) swap to Tensa Zangetsu model & moveset [NOT TESTED]
- [ ] **Saber:**
  - Invisible Air (Q), Strike Air (W), Excalibur (R) [NOT TESTED]
- [ ] **Rock Lee:**
  - Leaf Whirlwind (Q), Primary Lotus (W), Eight Gates Release (R) [NOT TESTED]
- [ ] **Monkey D. Luffy:**
  - Gomu Gomu no Pistol (Q), Bazooka (W), Gear Second (R) [NOT TESTED]
- **Gate 5 Exit Criteria:** All 4 heroes can spawn, attack, cast all 4 authentic skills with cooldowns and mana consumption, and damage creeps.

---

### STAGE 6: MOBILE HUD & ERGONOMIC TOUCH CONTROLS (QUEUED)
*Goal: Production-ready mobile interface optimized for thumb-reach and low-latency interaction.*
- [ ] Virtual dual-analog thumbstick (movement + skill aiming direction) [NOT TESTED]
- [ ] Responsive ability buttons with cooldown radial sweep overlays [NOT TESTED]
- [ ] Item inventory slots (6 active item slots with drag-to-use / tap-to-activate) [NOT TESTED]
- [ ] Mini-map HUD with touch-to-ping and hero camera focus buttons [NOT TESTED]
- [ ] Kill/Death/Creep score header and scoreboard overlay [NOT TESTED]
- **Gate 6 Exit Criteria:** 60 FPS touch response, zero input lockup, natural mobile ergonomics.

---

### STAGE 7: ANDROID BUILD & PERFORMANCE OPTIMIZATION (QUEUED)
*Goal: Stable ARM64 release package running on physical Android hardware.*
- [ ] Configure Android SDK Platform 34/35 and NDK toolchains on Drive D: [NOT TESTED]
- [ ] Compile release APK / AAB for `arm64-v8a` [NOT TESTED]
- [ ] Measure frame times, thermal throttling, and memory footprint on device [NOT TESTED]
- [ ] Eliminate garbage collection spikes and optimize draw calls [NOT TESTED]
- **Gate 7 Exit Criteria:** Sustained 60 FPS on mid-range Android test device without memory leaks or overheating.

---

### STAGE 8: MULTIPLAYER & ROSTER EXPANSION (QUEUED)
*Goal: Local network / online multiplayer and expansion of remaining FOC heroes.*
- [ ] Deterministic network synchronization (lockstep or client-side prediction) [NOT TESTED]
- [ ] Convert and rig remaining 20+ FOC heroes (Naruto, Sasuke, Zoro, Sanji, Archer, etc.) [NOT TESTED]
- [ ] Victory condition checks, end-game statistics screen, and match rematching [NOT TESTED]
- **Gate 8 Exit Criteria:** 6-player low-latency multiplayer match completed from hero pick to throne destruction.
