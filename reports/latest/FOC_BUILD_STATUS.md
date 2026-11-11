# FOC REBORN MOBILE — BUILD STATUS REPORT
## Comprehensive Compilation & Toolchain Matrix

**Project:** FOC REBORN MOBILE  
**Lead Architect:** JIN  
**Last Updated:** 2026-11-11T12:50:00+08:00  

---

## 1. BUILD TARGET STATUS

| Target Platform | Engine / Pipeline | Build Command | Status | Verification / Log Details |
| :--- | :--- | :--- | :--- | :--- |
| **Desktop JVM (Classes)** | W3AndroidEngine | `./gradlew desktop:classes` | `PASS` [VERIFIED BY BUILD/TEST] | Build successful in 6m 24s. 8/8 tasks executed without errors. |
| **Core Unit Tests** | W3AndroidEngine | `./gradlew core:test` | `PASS` [VERIFIED BY BUILD/TEST] | 191/191 tests passed (0 failures, 0 errors) in 1m 41s. |
| **Desktop Executable (Fat JAR)** | W3AndroidEngine | `./gradlew desktop:dist` | `PASS` [VERIFIED BY BUILD/TEST] | Built `desktop-1.0.jar` (55.26 MB) in 2m 3s. Zero-dependency JVM runnable. |
| **FOC Map Diagnostic Harness** | Java 17 Temurin | `java -cp desktop-1.0.jar FOCMapDiagnostic.java` | `PASS` [VERIFIED BY BUILD/TEST] | Executed in 1.42s. 97x97 terrain, 384x384 pathing, 1,168 doodads parsed. |
| **Unity C# Combat Classes** | Unity C# / .NET | Script Parsing & AST Audit | `PASS` [VERIFIED BY SOURCE] | 7 combat scripts parsed; 100% syntactically valid, braces match. |
| **Unity 6 URP Android APK** | Unity 6000.0.0f1 | `BuildPlayer(Android)` | `BLOCKED` | Host machine lacks Unity Editor installation (`Unity.exe`). |
| **Unity In-Editor Play Mode** | Unity 6000.0.0f1 | `ExecuteMethod:RunTests` | `BLOCKED` | Host machine lacks Unity Editor installation (`Unity.exe`). |
| **Android ARM64 Debug APK** | W3AndroidEngine | `./gradlew android:assembleDebug` | `NOT TESTED` | Requires Android SDK API 34/36 configuration on Drive D:. |
| **Android ARM64 Release APK** | W3AndroidEngine | `./gradlew android:assembleRelease`| `NOT TESTED` | Scheduled for Stage 7. |

---

## 2. HARDWARE & STORAGE ENVIRONMENT STATUS
* **Host Operating System:** Windows 10 Pro 64-bit (10.0.19045)
* **Processor:** AMD Ryzen 3 PRO 2100GE with Radeon Vega Graphics (4 CPUs, ~3.2 GHz)
* **System Memory:** 16,384 MB (16 GB RAM)
* **Graphics Hardware:** NVIDIA GeForce GTX 1650 (4 GB VRAM)
* **Disk C: Available Storage:** `0.29 GB Free` (CRITICAL CONSTRAINT - FULLY PROTECTED)
* **Disk D: Available Storage:** `68.64 GB Free` (HEALTHY - ALL ARTIFACTS CONFINED HERE)
