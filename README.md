# FIGHT OF THE CHARACTERS (FOC) REBORN MOBILE ⚔️📱
### Faithful Android Port of Warcraft III *Fight of Characters 9.1a (Asia)*
**Engine:** Unity 6 LTS | **Render Pipeline:** Universal Render Pipeline (URP) | **Target:** Android ARM64 (Landscape)

---

## 🌟 OVERVIEW
**Fight of the Characters Reborn Mobile** is a faithful, high-performance mobile port of the legendary Warcraft III custom map **Fight of Characters 9.1a (Asia).w3x**. 

This is an authentic **Unity 3D / C# mobile game** built directly from the extracted original Warcraft III assets—preserving original character meshes, bone rigs, textures, animations, audio, and gameplay mechanics.

---

## ⚔️ HERO ASSET FIDELITY & SKELETAL RIGS (STAGE 2B VERIFIED)

All hero models were extracted directly from the original map archive and converted into fully-rigged, animated Collada (`.dae`) assets with embedded animation channels and normalized vertex skinning.

| Hero | WC3 Source File | Model Name | Vertices | Triangles | Bones / Joints | Original Sequences | Converted Asset |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **Ichigo Kurosaki** | `unknown_0186.mdx` | `'ichigo 2'` | 1,848 | 2,067 | 51 Joints | 13 Sequences | `Assets/Art/Heroes/Ichigo/Ichigo.dae` |
| **Saber / Artoria** | `unknown_0237.mdx` | `'HeroSaber'` | 778 | 1,698 | 25 Joints | 14 Sequences | `Assets/Art/Heroes/Saber/Saber.dae` |
| **Rock Lee** | `unknown_0208.mdx` | `'lee done'` | 669 | 756 | 17 Joints | 13 Sequences | `Assets/Art/Heroes/RockLee/RockLee.dae` |
| **Monkey D. Luffy** | `unknown_0190.mdx` | `'jmlcqxp18'` | 3,353 | 3,649 | 54 Joints | 21 Sequences | `Assets/Art/Heroes/Luffy/Luffy.dae` |

### 🔬 Mathematical Parity Verification
Verified via automated reproducible scripts (`validate_dae_and_deformation.py` and `compare_geometry_metrics.py`):
* **Geometric Distortion:** $\text{MAE} \le 0.000034$ (Max Error $\le 0.000075$, matching theoretical 4-decimal float serialization limit).
* **Inverse Bind Matrix (IBM) Deviation:** $\le 0.000100$ deviation from Identity across all joints.
* **Vertex Weights:** 100% normalized ($w = 1.0$) single-influence rigid skinning optimized for mobile GPU performance.
* **Animation Monotonicity:** Zero unordered timestamps, 100% valid channel targets.

---

## 🎮 SCENES & ARCHITECTURE

The Unity project is located in `FOC-Reborn-Mobile/`:

1. **`LoadingScene.unity` (Build Index 0):** Asynchronous asset loading featuring authentic 1024x768 stitched loading screen artwork.
2. **`HeroSelectionScene.unity` (Build Index 1):** Real-time 3D hero selection with inertia turntable, lock-in phase timer, duplicate pick prevention, bot pick automation, and audio feedback.
3. **`OfflineGameScene.unity` (Build Index 2):** Combat arena featuring mobile dual-analog joystick controls and authentic hero stats.
4. **`HeroAnimationVerificationScene.unity` (Build Index 3):** Diagnostic scene featuring playback scrubber, timecode monitor, animation selector, and bone gizmos.

---

## 🛠️ EXTRACTION & CONVERSION PIPELINE

The repository includes custom, mathematically proven Python pipeline scripts:
* **`MDX_To_Rigged_DAE.py`:** Reads binary MDLX format, extracts geosets, bones, helpers, and animation tracks, and outputs Collada 1.4.1 rigged models with `<library_animations>`.
* **`validate_dae_and_deformation.py`:** Audits DAE XML structure, skin controllers, joint hierarchies, and animation channels.
* **`compare_geometry_metrics.py`:** Calculates Max Positional Error, MAE, and RMS error between raw MDX and converted DAE.
* **`extract_assets.py`:** Unpacks MPQ archive and decodes BLP textures into lossless PNG.

---

## 📦 BUILD INSTRUCTIONS

1. Clone repository:
   ```bash
   git clone https://github.com/Yazerukun/fight-of-the-characters-mobile.git
   ```
2. Open `FOC-Reborn-Mobile` in **Unity 6 (6000.x) LTS**.
3. Target Platform: **Android** (Scripting Backend: IL2CPP, Target Architecture: ARM64).
4. Build settings include all 4 registered scenes in `EditorBuildSettings.asset`.

---
*Maintained with military-grade precision by JIN & Yazerukun.*
