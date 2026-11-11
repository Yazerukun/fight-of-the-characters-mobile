# FOC REBORN MOBILE — BUILD STATUS REPORT
## Comprehensive Compilation & Toolchain Matrix

**Project:** FOC REBORN MOBILE  
**Lead Architect:** JIN  
**Last Updated:** 2026-11-11T11:16:00+08:00  

---

## 1. BUILD TARGET STATUS

| Target Platform | Engine / Pipeline | Build Command | Status | Verification / Log Details |
| :--- | :--- | :--- | :--- | :--- |
| **Desktop JVM (Classes)** | W3AndroidEngine | `./gradlew desktop:classes` | `PASS` [VERIFIED BY BUILD/TEST] | Build successful in 6m 24s. 8/8 tasks executed without errors. |
| **Core Unit Tests** | W3AndroidEngine | `./gradlew core:test` | `PASS` [VERIFIED BY BUILD/TEST] | 191/191 tests passed (0 failures, 0 errors) in 1m 41s. |
| **Desktop Executable (Fat JAR)** | W3AndroidEngine | `./gradlew desktop:dist` | `PASS` [VERIFIED BY BUILD/TEST] | Built `desktop-1.0.jar` (55.26 MB) in 2m 3s. Zero-dependency JVM runnable. |
| **FOC Map Diagnostic Harness** | Java 17 Temurin | `java -cp desktop-1.0.jar FOCMapDiagnostic.java` | `PASS` [VERIFIED BY BUILD/TEST] | Executed in 1.42s. 97x97 terrain, 384x384 pathing, 1,168 doodads parsed. |
| **Android ARM64 Debug APK** | W3AndroidEngine | `./gradlew android:assembleDebug` | `NOT TESTED` | Requires Android SDK API 34/36 configuration on Drive D:. |
| **Android ARM64 Release APK** | W3AndroidEngine | `./gradlew android:assembleRelease`| `NOT TESTED` | Scheduled for Stage 7. |
| **Unity 6 URP Android APK** | Unity 6000.0.0f1 | `BuildPlayer(Android)` | `BLOCKED` | Host machine lacks Unity Editor installation (`Unity.exe`). |
| **Unity Test Runner** | Unity 6000.0.0f1 | `ExecuteMethod:RunTests` | `BLOCKED` | Host machine lacks Unity Editor installation (`Unity.exe`). |

---

## 2. GATE 1 COMPILATION EXECUTION LOG

* **Execution Command:**
  ```cmd
  cmd.exe /c "set GRADLE_USER_HOME=D:\W3AndroidEngine\gradle-cache& set JAVA_OPTS=-Dgradle.user.home=D:\W3AndroidEngine\gradle-cache& cd /d D:\W3AndroidEngine\repo& gradlew.bat --gradle-user-home D:\W3AndroidEngine\gradle-cache desktop:classes --console=plain"
  ```
* **Exit Code:** `0` (BUILD SUCCESSFUL)
* **Execution Duration:** `6 minutes 24 seconds`
* **Tasks Executed (8 actionable):**
  1. `> Task :fdfparser:generateGrammarSource` (ANTLR4 frame definition grammar generation)
  2. `> Task :fdfparser:compileJava`
  3. `> Task :jassparser:generateGrammarSource` (ANTLR4 JASS parser grammar generation)
  4. `> Task :shared:compileJava`
  5. `> Task :jassparser:compileJava`
  6. `> Task :core:compileJava`
  7. `> Task :desktop:compileJava`
  8. `> Task :desktop:processResources`
  9. `> Task :desktop:classes`
* **Artifact Locations:**
  - `D:\W3AndroidEngine\repo\desktop\build\classes\java\main`
  - `D:\W3AndroidEngine\repo\core\build\classes\java\main`
  - `D:\W3AndroidEngine\repo\jassparser\build\classes\java\main`
  - `D:\W3AndroidEngine\repo\fdfparser\build\classes\java\main`
  - `D:\W3AndroidEngine\repo\shared\build\classes\java\main`

---

## 3. HOST SYSTEM & TOOLCHAIN SPECIFICATIONS

* **Operating System:** Windows 10 Pro 64-bit (Build 19045)
* **Processor (CPU):** AMD Ryzen 3 PRO 2100GE with Radeon Vega Graphics (4 Cores, 3.20 GHz)
* **System Memory (RAM):** 16.0 GB (13.9 GB Usable)
* **Graphics Processor (GPU):** NVIDIA GeForce GTX 1650 (4 GB GDDR5, Driver 576.88, WDDM 2.7)
* **Storage Allocation Post-Build:**
  - `Drive C:` 118.66 GB Total | **0.44 GB Free** (`SAFE / PROTECTED`)
  - `Drive D:` 931.51 GB Total | **68.76 GB Free** (`ACTIVE DEPLOYMENT TARGET`)
* **Java Development Kit (JDK):**
  - Vendor: Eclipse Adoptium (Temurin)
  - Version: `17.0.20.1` (build 17.0.20.1+1)
  - Location: `C:\Users\Technical\AppData\Local\Programs\Eclipse Adoptium\jdk-17.0.20.101-hotspot\`
* **Android Debug Bridge (ADB):**
  - Version: Android Debug Bridge version 1.0.41 (Version 35.0.2-12147458)
  - Location: `C:\adb\adb.exe`
* **Git Version:** 2.45.1.windows.1
