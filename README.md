# ⚔️ Fight of the Characters: 3D Mobile Arena (FOTC)

[![Platform: Android](https://img.shields.io/badge/Platform-Android%2016%20%7C%208%2B-brightgreen.svg)](https://developer.android.com/)
[![Engine: WebGL + Godot 4.3](https://img.shields.io/badge/Engine-WebGL%20MDX%20%2B%20Godot%204.3-blue.svg)](https://godotengine.org/)
[![GitHub Release](https://img.shields.io/github/v/release/Yazerukun/fight-of-the-characters-mobile?color=blue)](https://github.com/Yazerukun/fight-of-the-characters-mobile/releases/latest)
[![CI Build](https://github.com/Yazerukun/fight-of-the-characters-mobile/actions/workflows/build-apk.yml/badge.svg)](https://github.com/Yazerukun/fight-of-the-characters-mobile/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-purple.svg)](LICENSE)

<p align="center">
  <a href="https://github.com/Yazerukun/fight-of-the-characters-mobile/releases/download/v1.0.0/FightOfCharacters.apk">
    <img src="https://img.shields.io/badge/DOWNLOAD_APK-v1.0.0_(5.7_MB)-2ea44f?style=for-the-badge&logo=android&logoColor=white" alt="Download APK" />
  </a>
</p>

An authentic 3D mobile fighting game inspired by the legendary Warcraft III custom map **"Fight of Characters" (FOTC / FOCS3 Another)**. Built with high-performance 3D skeletal animation pipelines, authentic Lordaeron cobblestone terrain, periodic 1v1 duel arenas, and touchscreen controls designed specifically for Android smartphones and tablets.

---

## 🎮 Features

- **Authentic 3D Warcraft III Arena:**
  - Full 3D perspective projection of the Lordaeron / Dalaran stone paver battleground.
  - Central **Ring of Honor (Duel Arena)** with ancient glowing gold/amber runic inscriptions and 4 stone pillars with animated flickering torch braziers.
  - Creep spawning lanes, Radiant Blue Fountain, and Dire Red Fountain.
  - Dynamic 3D ground drop shadows and Warcraft III selection brackets (Green for player, Red pulsing circle for targeted enemies).
  - Classic green 4-arrow RTS click reticles when navigating.

- **Mobile MOBA / Action Controller Ergonomics:**
  - **Left Side:** Virtual analog joystick for smooth 360° omnidirectional movement.
  - **Right Side:** Large, prominent circular **ATTACK** button (`⚔️ ATTACK [A]`) with auto-acquire, nearest target selection, and walk-to-attack logic.
  - **Radial Skill Arc:** 5-skill ability cluster ergonomically positioned around the Attack button:
    - `[Q]` Lapse Blue (Suction vortex AoE pull + damage)
    - `[W]` Reversal Red (Repulsive knockback shockwave)
    - `[E]` Infinity (100% invulnerability barrier)
    - `[R]` Hollow Purple (Cataclysmic particle laser beam)
    - `[D]` Unlimited Void (Domain Expansion cosmic stun + DoT)
  - Full native Android multi-touch support and haptic feedback.

- **Classic Warcraft III In-Game HUD:**
  - Real-time Hero Portrait with Level badge.
  - Thick Green HP bar (`1400 / 1400`) and Blue MP bar (`600 / 600`).
  - Stat readouts (Attack Damage, Physical Armor Mitigation, Movement Speed).
  - Dynamic Target HUD displaying enemy portrait, name, and current health.
  - Goblin Merchant Shop (`[B]`) and Hero Roster Selection (`[H]`).

- **Original Maps Included:**
  - `maps/FOCS3 Another 9.7 C03.w3x` (27.5 MB)
  - `maps/fight_of_characters 7.7b E.w3x` (4.09 MB)

---

## 📱 Android APK Installation

The pre-compiled debug APK is available directly in this repository:

**Path:** `build/android/debug/FightOfCharacters.apk`

### Quick Install via ADB:
```powershell
adb install -r build/android/debug/FightOfCharacters.apk
```

### Launch Game via ADB:
```powershell
adb shell am start -n com.jin.fotcmobile/.MainActivity
```

---

## 🛠️ Project Structure

```
├── android/                   # Capacitor 7 Android Native Project & Gradle Wrapper
├── build/android/debug/       # Pre-built installable Android APK (FightOfCharacters.apk)
├── godot_project/             # Godot 4.3 3D Engine Scenes & GDScript Roster
├── maps/                      # Original Warcraft III Custom Maps (.w3x)
│   ├── FOCS3 Another 9.7 C03.w3x
│   └── fight_of_characters 7.7b E.w3x
├── public/models/             # Extracted 3D MDX Character Models & BLP Textures
│   ├── ace.mdx, byakuya.mdx, gilgamesh.mdx, ichigo.mdx
│   ├── inuyasha.mdx, luffy.mdx, rocklee.mdx, saber.mdx, sasuke.mdx, toshiro.mdx
├── scripts/                   # MPQ Asset Extraction & Conversion Tooling
├── src/                       # WebGL 3D Warcraft III Engine Core
│   ├── engine3d/FOTC3DArena.js # 60Hz Physics Loop, Combat AI, & Terrain Pipeline
│   ├── ui/styles.css          # Responsive Mobile HUD & Controller Styling
│   └── main.js                # Game Engine Bootstrapper
├── www/                       # Web asset bundle for Capacitor container
└── index.html                 # Game Entry Point & Canvas Overlay
```

---

## 💻 Desktop Local Play

To run the game locally on PC/Desktop:

1. Double-click **`PLAY_FOTC_3D.bat`** in the repository root.
2. Or run a local HTTP server:
   ```powershell
   python -m http.server 3000
   ```
3. Open your browser at `http://localhost:3000/`.

---

## 📜 Credits & License

Inspired by the classic Warcraft III custom map "Fight of Characters". Created for educational, preservation, and non-commercial reverse-engineering purposes. Distributed under the MIT License.
