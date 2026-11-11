# FOC REBORN MOBILE — KNOWN ISSUES & BLOCKERS
## Active Engineering Blocker Register & Risk Mitigations

**Project:** FOC REBORN MOBILE  
**Document ID:** SEC-RISK-001  
**Lead Architect:** JIN  
**Last Updated:** 2026-11-11  

---

## 1. ACTIVE ISSUE REGISTRY

```
[CRITICAL]  ISSUE-001: Host Drive C: Storage Exhaustion (< 450 MB Free)
[HIGH]      ISSUE-002: Unity Editor Executable Unavailable on Host PC
[HIGH]      ISSUE-003: Unverified JASS Compatibility for FOC 9.1a Triggers
[MEDIUM]    ISSUE-004: W3AndroidEngine Base Game Data (CASC/MPQ) Dependency
[MEDIUM]    ISSUE-005: Android SDK / NDK Environment Variables Unconfigured
```

---

### ISSUE-001: Critical Storage Exhaustion on Host Drive C:
* **Severity:** `CRITICAL`
* **Status:** `ACTIVE / MITIGATED VIA WORKSPACE REDIRECTION`
* **Description:** Drive C: currently has only ~430 MB of free storage. Unchecked package installations, Gradle build caches, Docker layers, or Windows temp extractions will immediately trigger disk full errors, crashing running processes.
* **Root Cause:** Host OS disk partition allocation.
* **Architectural Mitigation:**
  1. Strict prohibition of any write operations to Drive C:.
  2. All W3AndroidEngine builds, clones, and downloads are routed strictly to Drive D: (`D:\W3AndroidEngine`).
  3. All Gradle runs must explicitly pass `--gradle-user-home D:\W3AndroidEngine\gradle-cache` or set `$env:GRADLE_USER_HOME = 'D:\W3AndroidEngine\gradle-cache'`.
  4. Build temporary directories must point to `D:\W3AndroidEngine\downloads`.
* **Action Required:** Maintain constant monitoring of Drive C: space. Never invoke commands that extract to `%USERPROFILE%` or `%TEMP%` on Drive C:.

---

### ISSUE-002: Unity Editor Executable Unavailable on Host PC
* **Severity:** `HIGH`
* **Status:** `BLOCKED ON CURRENT HOST WORKSTATION`
* **Description:** `Unity.exe` (target version: 6000.0.0f1 LTS) is not installed on this specific workstation. While all Unity assets, scenes, meta files, DAE models, textures, and prefabs exist and are validly serialized, live in-editor Play Mode verification cannot be run directly here.
* **Root Cause:** Workstation provisioning lacks the desktop Unity 6 editor package.
* **Architectural Mitigation:**
  1. Strict zero-fabrication rule: Do not claim Unity Editor runtime tests have executed when Unity Editor is absent.
  2. Mathematical verification: Verified vertex fidelity, skinning matrices, and bone counts using Python AST and Collada XML parsers ($\text{MAE} \le 0.000034$).
  3. Git remote sync: Complete Unity project is mirrored to GitHub (`https://github.com/Yazerukun/fight-of-the-characters-mobile.git`) for checkout on any Unity-enabled development machine.
* **Action Required:** Keep Unity repository completely synchronized and preserved as the authoritative reference/backup.

---

### ISSUE-003: Unverified JASS Compatibility for FOC 9.1a Triggers
* **Severity:** `HIGH`
* **Status:** `UNVERIFIED / QUEUED FOR GATE 6`
* **Description:** `Fight_of_Characters9.1a(Asia).w3x` contains 32,042 lines of dense, highly optimized JASS2 triggers (`war3map.j`), utilizing custom timer systems, local handle variables, and specialized native Blizzard API calls. If W3AndroidEngine's bytecode VM lacks any specific Blizzard natives (e.g. `SetUnitAbilityLevel`, `CreateGroup`, custom lightning effects), the map script may fail during initialization.
* **Root Cause:** Third-party reverse-engineered Warcraft III virtual machines may have incomplete API coverage.
* **Architectural Mitigation:**
  1. Isolate and test `war3map.j` execution early in the desktop engine build.
  2. If missing natives are identified, determine whether they can be patched in W3AndroidEngine's Java source.
  3. If JASS compatibility proves intractable, activate the Unity fallback pipeline.
* **Action Required:** Execute headless JASS validation during Stage 4.

---

### ISSUE-004: W3AndroidEngine Base Game Data (CASC/MPQ/Folder) Dependency
* **Severity:** `HIGH`
* **Status:** `DIAGNOSED & EMPIRICALLY CONFIRMED` [VERIFIED BY BUILD]
* **Description:** HeadlessLauncher runtime execution proved that W3AndroidEngine initializes its core and passes startup steps 0, 1, and 2, but requires base Warcraft III assets (`UI\WorldEditGameStrings.txt`, `UI\war3skins.txt`, TrueType font definitions, and `UI\FrameDef\FrameDef.toc`). Custom map archives like `Fight_of_Characters9.1a(Asia).w3x` contain custom models, abilities, and triggers, but do not embed Blizzard's base engine UI definitions.
* **Root Cause:** Engine architecture is a client runtime that mounts base game data and overlays the custom map.
* **Architectural Mitigation:**
  1. Option A: Assemble a lightweight minimal base asset directory on Drive D: containing the essential base SLK tables, UI skins, and `FrameDef.toc` to allow W3AndroidEngine to load custom maps standalone.
  2. Option B: Accelerate the Unity 6 LTS URP pipeline (`D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile`), which is 100% standalone, already has all 4 hero rigs mathematically verified with zero IBM error, and has zero dependency on external Blizzard game archives.
* **Action Required:** Present the trade-off between assembling a minimal WC3 base pack vs continuing Unity standalone mobile implementation.

---

### ISSUE-005: Android SDK / NDK Environment Variables Unconfigured
* **Severity:** `MEDIUM`
* **Status:** `QUEUED FOR STAGE 7`
* **Description:** `ANDROID_HOME` and `ANDROID_NDK_ROOT` are not currently set in system environment variables, preventing command-line APK builds on this PC.
* **Root Cause:** Fresh environment setup on host.
* **Architectural Mitigation:**
  1. Desktop validation (`./gradlew desktop:classes`) can proceed immediately without Android SDK.
  2. Standalone Android command-line tools can be downloaded directly to `D:\W3AndroidEngine\downloads` and extracted to Drive D: when Stage 7 begins.
* **Action Required:** Configure standalone Command-Line Tools on Drive D: prior to Gate 7 execution.
