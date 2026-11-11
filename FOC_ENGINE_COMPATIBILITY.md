# FOC REBORN MOBILE — ENGINE COMPATIBILITY MATRIX
## Architectural Comparison: W3AndroidEngine vs Unity 6 LTS URP

**Project:** FOC REBORN MOBILE  
**Document ID:** ARCH-ENG-001  
**Author:** JIN (Principal Systems Architect & Lead Game Developer)  
**Status:** ACTIVE EVALUATION  
**Date:** 2026-11-11  

---

## 1. EXECUTIVE ARCHITECTURAL SUMMARY

To deliver an authentic, high-performance mobile version of **Fight of Characters 9.1a (Asia)**, two distinct engine architectures are under active evaluation:

1. **W3AndroidEngine (Warsmash Reforged Derivative):**
   - **Stack:** Java 17, LibGDX, LWJGL (Desktop) / Android Native OpenGL ES 2.0/3.0.
   - **Philosophy:** Direct native execution of original Warcraft III files (`.w3x`, `.mdx`, `.blp`, `.j`, `.slk`).
   - **Advantage:** If fully compatible, it eliminates manual porting of 32,042 lines of JASS triggers and hundreds of custom ability SLK tables.
   - **Critical Question:** Can it run high-speed complex custom maps like FOC 9.1a without missing native functions or CASC/MPQ dependencies?

2. **Unity 6 LTS URP Mobile Project:**
   - **Stack:** C# / .NET 9, Universal Render Pipeline (URP), IL2CPP ARM64.
   - **Philosophy:** Modern rebuild using converted original assets (MDX -> DAE, BLP -> PNG) and clean modular gameplay architecture.
   - **Advantage:** Unmatched mobile tooling, modern post-processing, robust UI systems, built-in physics, and commercial mobile optimization.
   - **Disadvantage:** Requires writing or generating C# gameplay logic to replicate 32,000+ lines of custom JASS abilities and item mechanics.

---

## 2. DETAILED TECHNICAL COMPARISON MATRIX

| Dimension | W3AndroidEngine (Primary Candidate) | Unity 6 LTS URP (Preserved Backup) | Architectural Assessment |
| :--- | :--- | :--- | :--- |
| **MDX Model Support** | Native MDX v800/v900 binary parser. Parses bones, geosets, sequences, ribbons, and particle emitters directly. | Requires conversion (MDX -> DAE/FBX). Preserves geometry, bones, and keyframes via our validated pipeline. | **W3AndroidEngine wins** on asset turnaround; **Unity wins** on modern shader/lighting capabilities. |
| **BLP Texture Support** | Native BLP1 / BLP2 direct decompression in memory. | Converted to 32-bit PNG with standard Unity Texture Importer. | **Tie.** PNG conversion for Unity is already automated and lossless. |
| **JASS2 Gameplay Scripting** | Native JASS2 virtual machine with bytecode interpreter. Runs `war3map.j` natively. | Requires transpilation or manual C# reimplementation of FOC abilities and game triggers. | **W3AndroidEngine wins heavily** IF all Blizzard native functions (`common.j`, `blizzard.j`) are implemented. |
| **Object Data (Units/Items/Spells)** | Reads `war3map.w3u`, `w3a`, `w3t`, `w3b` directly from map archive. | Requires custom binary/text parser to generate Unity ScriptableObjects. | **W3AndroidEngine wins** on data pipeline automation. |
| **Terrain & Water** | Native `war3map.w3e` mesh generator with authentic WC3 cliff levels and water shading. | Reconstructed via custom mesh generator or Unity Terrain. | **W3AndroidEngine wins** on 1:1 Warcraft III aesthetic replication. |
| **Pathing & Collision** | Native `war3map.wpm` (32x32 pathing grid cells: unwalkable, unbuildable, unamphibious). | Re-baked via Unity NavMesh or custom Spatial Hash Grid. | **W3AndroidEngine wins** on authentic pathing and collision rules. |
| **Touch Controls & Mobile HUD** | LibGDX Scene2D overlay. Basic on-screen controls implemented in current repository. | Advanced uGUI / UI Toolkit with responsive layouts, anchors, canvas scalers, and touch event systems. | **Unity wins** on UI flexibility, touch ergonomics, and mobile responsiveness. |
| **Audio Engine** | LibGDX Audio / OpenAL. Supports WC3 sound definitions and 3D positional audio. | Unity FMOD-based audio system with audio mixers, DSP effects, ducking, and spatialization. | **Unity wins** on audio mixing and low-latency DSP effects. |
| **Android Build Pipeline** | Pure Gradle (`./gradlew assembleRelease`). Small APK (~25–40 MB without map). Fast build times. | Unity IL2CPP build system. Larger APK (~80–120 MB). Requires Unity Editor installation. | **W3AndroidEngine wins** on lightweight build pipeline and CLI automation. |
| **Debugging & Tooling** | Standard Java/Android Studio debugging, JDB, logcat. | Unity Profiler, Frame Debugger, Memory Profiler, Visual Studio. | **Unity wins** on visual profiling tools (when Editor is available). |
| **Host Workstation Compatibility** | Requires only Java 17 (installed: Temurin 17.0.20.1) and Android SDK. Operational on current PC. | Requires Unity Editor 6000.0.0f1 (NOT currently installed on host PC). | **W3AndroidEngine is currently actionable**; Unity Editor is currently blocked. |

---

## 3. ENGINE ADOPTION CRITERIA & GATING PROTOCOL

W3AndroidEngine is currently positioned as the **Primary Candidate**, but its adoption must satisfy strict empirical milestones:

```
                  ┌──────────────────────────────────────────────┐
                  │ W3AndroidEngine Evaluation Pipeline          │
                  └──────────────────────┬───────────────────────┘
                                         │
                   [GATE 1: Compile Desktop Classes]
                                         │
                    [GATE 2: Load Standard WC3 Map]
                                         │
                    [GATE 3: Load FOC 9.1a (Asia)]
                                         │
                  [GATE 4: Verify 4 Hero MDX/BLP Rigs]
                                         │
                 [GATE 5: Execute FOC JASS Triggers]
                                         │
                 [GATE 6: Playable 4-Hero Combat Loop]
                                         │
                   [GATE 7: Assemble Android ARM64]
                                         │
                   [GATE 8: Play on Physical Device]
                                         │
                                         ▼
                   ===============================================
                   DECISION: ADOPT OR FALL BACK TO UNITY 6
                   ===============================================
```

### Risk Mitigation Plan:
If W3AndroidEngine fails at Gate 3 (e.g. unhandled JASS constructs, crash on custom FOC MPQ archives, or missing engine natives), the project **immediately pivots** to the Unity 6 LTS pipeline using our already verified DAE models, textures, and modular C# gameplay architecture.

The Unity backup at `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile` remains 100% untouched and synchronized with GitHub.
