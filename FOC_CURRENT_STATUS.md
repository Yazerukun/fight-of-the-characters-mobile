# FOC REBORN MOBILE — CURRENT STATUS
**Last Updated:** 2026-11-11T11:00:00+08:00  
**Active Milestone:** STAGE 1 — Engine & Map Compatibility Preflight  
**Lead Architect & Studio Manager:** JIN  

---

## 1. COMPONENT STATUS MATRIX

| Component | Target / Path | Status Label | Verification Details |
| :--- | :--- | :--- | :--- |
| **Primary Engine Candidate** | `D:\W3AndroidEngine\repo` | `STATIC FILE PRESENT` | Cloned from `nikoltu/W3AndroidEngine`. Commit `a865a389` (v0.3.6). |
| **Unity Backup Repository** | `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile` | `VERIFIED BY SOURCE` | Preserved on branch `main`, commit `4ad4d7e`. Live on GitHub. |
| **Original W3X Map** | `Fight_of_Characters9.1a(Asia).w3x` | `STATIC FILE PRESENT` | 4.10 MB authentic map archive present in workspace root. |
| **Extracted WC3 Assets** | `OriginalAssets/` (623 files) | `VERIFIED BY SOURCE` | All models, textures, strings, triggers extracted from W3X. |
| **Hero Rig Models (DAE)** | `Assets/Art/Heroes/` (4 Heroes) | `VERIFIED BY STATIC TEST` | 100% geometry parity verified ($\text{MAE} \le 0.000034$), 0 IBM errors. |
| **Hero Baked Prefabs** | `Assets/Resources/Heroes/` | `STATIC FILE PRESENT` | Clean Unity 6 YAML serialized prefabs with strict assertions. |
| **Desktop Java 17 Runtime** | Temurin OpenJDK 17.0.20.1 | `VERIFIED BY BUILD` | Available on system PATH (`javac.exe` / `java.exe`). |
| **Android ADB Runtime** | `C:\adb\adb.exe` (v1.0.41) | `VERIFIED BY BUILD` | Operational command line tool. |
| **Unity Editor Executable** | Host Workstation (`Unity.exe`) | `BLOCKED` | Not installed on host PC. Required version: 6000.0.0f1. |
| **Android SDK / NDK** | Standalone SDK Manager | `BLOCKED` | `ANDROID_HOME` and `ANDROID_NDK_ROOT` unset. |
| **W3AndroidEngine Build** | `./gradlew desktop:classes` | `NOT TESTED` | Scheduled for execution using Drive D: cache. |

---

## 2. DISK & STORAGE CONSTRAINTS
* **Drive C: Status:** `0.43 GB Free` (CRITICAL CONSTRAINT).
  - All Gradle caches, Android builds, and external dependencies must be redirected to Drive D:.
* **Drive D: Status:** `69.21 GB Free` (HEALTHY).
  - Dedicated engine workspace established at `D:\W3AndroidEngine`.
  - Gradle user home configured at `D:\W3AndroidEngine\gradle-cache`.

---

## 3. ACTIVE BLOCKERS & UNVERIFIED CLAIMS
* **Blocker 1:** Host PC does not have Unity Editor installed; live Editor Play Mode verification is blocked until a machine with Unity 6 is available.
* **Blocker 2:** Android SDK platform 36 is not yet configured for command-line APK assembly.
* **Unverified Claim:** Compatibility of `Fight_of_Characters9.1a(Asia).w3x` with W3AndroidEngine JASS interpreter has NOT yet been tested.
