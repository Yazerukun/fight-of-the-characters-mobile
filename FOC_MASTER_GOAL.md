# FOC REBORN MOBILE — PERMANENT MASTER GOAL
## Long-Term Project Directive & Engineering Architecture

**PROJECT CODENAME:** FOC REBORN MOBILE  
**PRIMARY OBJECTIVE:** Create a faithful, high-performance Android mobile adaptation of the legendary Warcraft III custom map **Fight of Characters 9.1a (Asia)** (`Fight_of_Characters9.1a(Asia).w3x`).  
**PRIMARY ENGINE CANDIDATE:** W3AndroidEngine (Warsmash Reforged: Java 17 / LibGDX / Android Native)  
- Repository: `https://github.com/nikoltu/W3AndroidEngine`  
- Local Clone: `D:\W3AndroidEngine\repo` (Commit: `a865a389`, Version: `0.3.6`)  
**EXISTING UNITY BACKUP & REFERENCE:** Unity 6 LTS URP Mobile Project  
- Repository: `https://github.com/Yazerukun/fight-of-the-characters-mobile`  
- Local Location: `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile` (Commit: `4ad4d7e`)  

---

## 1. CORE PILLARS & GAMEPLAY FAITHFULNESS
The primary directive is uncompromising fidelity to the original Warcraft III game content:
* **Original Map & Terrain:** Reconstruct or load the authentic arena geometry, tilesets, pathing map (`war3map.wpm`), and doodad layout (`war3map.doo`).
* **Original 4 Validation Heroes:**
  - **Ichigo Kurosaki** (`unknown_0186.mdx`, `'ichigo 2'`, 1,848 vertices, 51 joints, 6 geosets, Shikai & Bankai).
  - **Saber / Artoria** (`unknown_0237.mdx`, `'HeroSaber'`, 778 vertices, 25 joints, 14 sequences).
  - **Rock Lee** (`unknown_0208.mdx`, `'lee done'`, 669 vertices, 17 joints, 13 sequences).
  - **Monkey D. Luffy** (`unknown_0190.mdx`, `'jmlcqxp18'`, 3,353 vertices, 54 joints, 21 sequences).
* **Original Assets:** Direct utilization of original MDX binary models, BLP textures, sound effects, and UI icons. No primitive shapes, generic humanoids, or invented replacement models.
* **Original Mechanics:** Preserved JASS triggers (`war3map.j`), exact ability damages, agility scaling curves, cooldowns, item shops, and creep wave spawn intervals.
* **Mobile Enhancements:** Touch virtual joystick, mobile HUD, screen layout optimization, low latency audio, and Android ARM64 hardware acceleration.

---

## 2. DUAL-ENGINE STRATEGY & ENGINE ADOPTION GATE
The project utilizes a disciplined dual-engine evaluation framework:
1. **Primary Candidate (W3AndroidEngine):** Evaluated for native Warcraft III engine compatibility. Because it natively parses `.w3x` files, MDX/BLP assets, and JASS bytecode, it offers the highest potential fidelity if compatibility is proven.
2. **Preserved Backup (Unity 6 URP):** The complete Unity 6 mobile project is permanently preserved at `D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile` and synced on GitHub. It serves as an active fallback and reference implementation.
3. **Engine Adoption Criteria:** W3AndroidEngine will only be adopted as the sole production engine once all 8 adoption gates pass:
   - Gate 1: Engine builds successfully on desktop and Android (`NOT TESTED`).
   - Gate 2: Known working WC3 custom map loads (`NOT TESTED`).
   - Gate 3: Original FOC map (`Fight_of_Characters9.1a(Asia).w3x`) loads (`NOT TESTED`).
   - Gate 4: Original MDX and BLP assets render with authentic shading (`NOT TESTED`).
   - Gate 5: Original skeletal rigs and animation sequences play (`NOT TESTED`).
   - Gate 6: Original JASS trigger bytecode executes correctly (`NOT TESTED`).
   - Gate 7: Abilities, items, and creep waves function properly (`NOT TESTED`).
   - Gate 8: Release Android APK builds and runs smoothly on an actual mobile device (`NOT TESTED`).

---

## 3. ABSOLUTE ENGINEERING LAWS
1. Never fabricate test results, execution logs, or build artifacts.
2. Never substitute static Python validation for runtime execution.
3. Never claim Unity Editor or Android runtime passes without authentic command logs.
4. Never delete or overwrite the Unity backup repository.
5. All downloaded dependencies, build caches, and repositories must reside on Drive D: (`D:\W3AndroidEngine`) due to Drive C: storage constraints.
