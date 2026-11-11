# FOC REBORN MOBILE — KNOWN ISSUES & BLOCKERS
## Active Engineering Blocker Register & Risk Mitigations

**Project:** FOC REBORN MOBILE  
**Document ID:** SEC-RISK-001  
**Lead Architect:** JIN  
**Last Updated:** 2026-11-11  

---

## 1. ACTIVE ISSUE REGISTRY

```
[CRITICAL]  ISSUE-001: Host Drive C: Storage Exhaustion (< 0.30 GB Free)
[HIGH]      ISSUE-002: Unity Editor Executable Unavailable on Host PC
[MEDIUM]    ISSUE-003: Android SDK / NDK Environment Variables Unconfigured on Host
[MEDIUM]    ISSUE-004: W3AndroidEngine JASS Semantic Init Blocked on Missing common.j
[LOW]       ISSUE-005: Dual-Directory Scaffolding Artifacts at Root
```

---

### ISSUE-001: Critical Storage Exhaustion on Host Drive C:
* **Severity:** `CRITICAL`
* **Status:** `ACTIVE / MITIGATED VIA WORKSPACE REDIRECTION`
* **Description:** Drive C: currently has only ~0.29 GB of free storage. Any unmonitored write operation will immediately trigger disk exhaustion and system instability.
* **Root Cause:** Host OS disk partition allocation.
* **Architectural Mitigation:**
  1. Strict prohibition of any write operations to Drive C:.
  2. All Unity assets, C# scripts, W3AndroidEngine builds, and test scripts reside strictly on Drive D: (`D:\FIGHT OF THE CHARACTERS` and `D:\W3AndroidEngine`).
  3. All temporary files, Gradle caches, and Python scripts run exclusively from Drive D:.
* **Action Required:** Maintain constant monitoring of Drive C: space. Never invoke commands that extract to `%USERPROFILE%` or `%TEMP%` on Drive C:.

---

### ISSUE-002: Unity Editor Executable Unavailable on Host PC
* **Severity:** `HIGH`
* **Status:** `BLOCKED ON CURRENT HOST WORKSTATION`
* **Description:** `Unity.exe` (target version: 6000.0.0f1 LTS) is not installed on this specific workstation. While all Unity assets, scenes, meta files, DAE models, textures, prefabs, and C# combat scripts exist and are validly serialized, live in-editor Play Mode verification cannot be run directly here.
* **Root Cause:** Workstation provisioning lacks the desktop Unity 6 editor package.
* **Architectural Mitigation:**
  1. Strict zero-fabrication rule: Do not claim Unity Editor runtime tests have executed when Unity Editor is absent.
  2. Mathematical verification: Verified vertex fidelity, skinning matrices, and bone counts using Python AST and Collada XML parsers ($\text{MAE} \le 0.000034$, 0 failures).
  3. Git remote sync: Complete Unity project is mirrored to GitHub (`https://github.com/Yazerukun/fight-of-the-characters-mobile.git`) for checkout on any Unity-enabled development machine.
* **Action Required:** Keep Unity repository completely synchronized and preserved as the authoritative primary development project.

---

### ISSUE-003: Android SDK / NDK Environment Variables Unconfigured on Host
* **Severity:** `MEDIUM`
* **Status:** `MITIGATED / DEFERRED TO PACKAGING STAGE`
* **Description:** Environment variables `ANDROID_HOME`, `ANDROID_NDK_ROOT`, and `JAVA_HOME` are not set globally in the Windows system environment, though `adb.exe` and `javac.exe` (Java 17 Temurin) exist on disk.
* **Root Cause:** Host developer environment configuration.
* **Architectural Mitigation:** Desktop and headless testing do not require Android SDK. When Android packaging is required, environment variables can be pointed to SDK directories on Drive D:.

---

### ISSUE-004: W3AndroidEngine JASS Semantic Init Blocked on Missing common.j
* **Severity:** `MEDIUM`
* **Status:** `DOCUMENTED REFERENCE LIMITATION`
* **Description:** W3AndroidEngine's ANTLR4 parser successfully parses `war3map.j` (32,042 lines in 481 ms), but halts during `program.initialize()` on `Unknown type: gamecache` because Blizzard base types (`common.j`) are not bundled with the custom map.
* **Architectural Mitigation:** Confirms the permanent architectural decision to adopt Unity 6 LTS URP as the primary self-contained game engine. W3AndroidEngine is retained as an analytical reference tool.
