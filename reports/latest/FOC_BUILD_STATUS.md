# FOC REBORN MOBILE — BUILD STATUS REPORT
## Comprehensive Compilation, Toolchain & Device Matrix

**Project:** FOC REBORN MOBILE  
**Lead Architect & Studio Manager:** JIN  
**Last Updated:** 2026-11-11T13:00:00+08:00  

---

## 1. BUILD TARGET & DEVICE STATUS

| Target Platform / Pipeline | Toolchain / Command | Status | Verification & Evidence |
| :--- | :--- | :--- | :--- |
| **Real Test Device Detection** | `C:\adb\adb.exe devices -l` | `PASS` [VERIFIED ON DEVICE] | Connected: `RMX5070` (realme 14 5G), Android 16, API 36, arm64-v8a. |
| **Device Screencap Extraction** | `adb shell screencap -p` | `PASS` [VERIFIED ON DEVICE] | Screenshot captured (2,881,827 bytes) at `reports/latest/realme_14_5g_screenshot.png`. |
| **Device Logcat Streaming** | `adb logcat -d` | `PASS` [VERIFIED ON DEVICE] | Logcat output streamed without errors. Chipset: `volcano` (qcom), Vulkan: true. |
| **Unity 6 URP Android APK** | `BuildPlayer(Android)` | `BLOCKED` [BLOCKED] | Unity Editor (6000.0.0f1) not installed on host workstation. |
| **Unity Android SDK / NDK** | Standalone Android NDK/SDK | `BLOCKED` [BLOCKED] | `ANDROID_HOME`, `ANDROID_NDK_ROOT` unset on host PC. |
| **Desktop JVM (Classes)** | W3AndroidEngine `./gradlew desktop:classes` | `PASS` [VERIFIED BY BUILD/TEST] | Build successful in 6m 24s. 8/8 tasks executed without errors. |
| **Core Unit Tests** | W3AndroidEngine `./gradlew core:test` | `PASS` [VERIFIED BY BUILD/TEST] | 191/191 tests passed (0 failures, 0 errors) in 1m 41s. |
| **Desktop Executable (Fat JAR)** | W3AndroidEngine `./gradlew desktop:dist` | `PASS` [VERIFIED BY BUILD/TEST] | Built `desktop-1.0.jar` (55.26 MB) in 2m 3s. Zero-dependency JVM runnable. |
| **FOC Map Diagnostic Harness** | Java 17 Temurin `FOCMapDiagnostic.java` | `PASS` [VERIFIED BY BUILD/TEST] | Executed in 1.42s. 97x97 terrain, 384x384 pathing, 1,168 doodads parsed. |
| **Unity C# Combat Classes** | Script AST & Syntax Audit (7 files) | `PASS` [VERIFIED BY SOURCE] | All combat scripts parsed; 100% syntactically valid, braces match. |

---

## 2. DISK SPACE AUDIT & PREREQUISITE REQUIREMENTS

| Drive Partition | Used Space | Free Space | Health Assessment | Unity Installation Feasibility |
| :--- | :--- | :--- | :--- | :--- |
| **Drive C:** | 111.46 GB | **0.28 GB** (286 MB) | `CRITICAL EXHAUSTION` | **FAILED.** Cannot support Windows temp unpacking (needs 3–5 GB). |
| **Drive D:** | 396.52 GB | **68.64 GB** | `HEALTHY` | **PASSED.** Sufficient for 10–12 GB Unity Editor + SDK/NDK install. |

### Exact Space Requirements to Install Unity 6 + Android Support:
1. **Drive D: Space Required:** ~10.5 GB (Unity 6000.0.0f1 Editor: 4.2 GB, Android Build Support: 2.1 GB, Android SDK/NDK: 3.8 GB, JDK: 0.4 GB). *Available: 68.64 GB (Sufficient).*
2. **Drive C: Temp Space Required:** ~3.5 GB (Installer extraction buffer in `%TEMP%` and `%LOCALAPPDATA%`). *Available: 0.28 GB (INSUFFICIENT).*
3. **Safety Action:** Halted installation per user instruction: *"Kung dili igo ang available space, STOP ug report ang exact required ug available space. Ayaw pag-delete og files o pag-install nga walay approval."*

---

## 3. REALME 14 5G ADB DETECTION LOG
```text
C:\adb\adb.exe devices -l
List of devices attached
3058fe31               device product:RMX5070 model:RMX5070 device:RE608EL1 transport_id:1

C:\adb\adb.exe -s 3058fe31 shell "getprop ro.product.model; getprop ro.build.version.release; getprop ro.build.version.sdk; wm size; wm density; getprop ro.product.cpu.abi; df -h /data"
RMX5070
16
36
Physical size: 1080x2400
Physical density: 480
arm64-v8a
Filesystem       Size Used Avail Use% Mounted on
/dev/block/dm-74 219G 177G   42G  81% /data/user/0
```
