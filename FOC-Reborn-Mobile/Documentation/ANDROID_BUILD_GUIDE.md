# FIGHT OF CHARACTERS 9.1a ASIA — ANDROID MOBILE BUILD & PACKAGING GUIDE

This guide details the complete process for configuring, building, and signing the Android APK/AAB for Fight of Characters 9.1a Asia Mobile Port.

---

## 1. PREREQUISITES & ENVIRONMENT SETUP

1. **Unity Engine Version:** Unity 2022.3 LTS (recommended 2022.3.20f1 or newer).
2. **Android SDK / NDK:**
   - Android SDK Tools 34.0.0
   - Android NDK r23b (23.1.7779620)
   - OpenJDK 17
3. **Target Platforms:**
   - Architecture: ARM64-v8a (64-bit mandatory for Google Play), ARMv7 fallback.
   - Minimum API Level: Android 8.0 'Oreo' (API Level 26).
   - Target API Level: Android 14 (API Level 34).

---

## 2. PLAYER SETTINGS CONFIGURATION

In Unity Editor (`Project Settings -> Player -> Android`):

### Identification & Graphics
- **Package Name:** `com.foc.reborn.asia`
- **Product Name:** `Fight of Characters 9.1a Asia`
- **Default Orientation:** `Landscape Left` (with Landscape Right allowed; Portrait disabled).
- **Color Space:** `Linear`
- **Graphics APIs:**
  1. Vulkan (Primary)
  2. OpenGLES3 (Fallback)
- **Multithreaded Rendering:** Enabled
- **Static & Dynamic Batching:** Enabled
- **Texture Compression:** ASTC (optimal for modern mobile GPU Mali/Adreno/PowerVR)

### Configuration
- **Scripting Backend:** `IL2CPP`
- **Target Architectures:**
  - [x] ARMv7
  - [x] ARM64
- **C++ Compiler Configuration:** `Master` / `Release`
- **Optimized Frame Pacing:** Enabled (for rock-solid 60 FPS / 120 FPS display sync)

---

## 3. BUILDING VIA COMMAND LINE (HEADLESS / CI/CD)

Execute the build script located in `AndroidBuild/UnityAndroidBuildScript.cs`:

```powershell
Unity.exe -quit -batchmode -nographics `
  -projectPath "D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile" `
  -executeMethod UnityAndroidBuildScript.PerformAndroidBuild `
  -logFile "D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\AndroidBuild\build.log"
```

The resulting binary will be output to:
`D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile\AndroidBuild\FOC_9.1a_Asia_Release.apk`
