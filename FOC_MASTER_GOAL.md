# FOC REBORN MOBILE — PERMANENT MASTER GOAL
## Long-Term Project Directive & Engineering Architecture

**PROJECT CODENAME:** FOC REBORN MOBILE  
**PRIMARY OBJECTIVE:** Create a faithful, high-performance Android mobile adaptation of the legendary Warcraft III custom map **Fight of Characters 9.1a (Asia)** (`Fight_of_Characters9.1a(Asia).w3x`).  
**PRIMARY DEVELOPMENT ENGINE:** Unity 6 LTS URP Mobile Project (`D:\FIGHT OF THE CHARACTERS\FOC-Reborn-Mobile`)  
**REFERENCE TOOLS & ANCILLARY HARNESSES:** W3AndroidEngine (`D:\W3AndroidEngine\repo`), SmashJassParser, MPQ Diagnostics  
**SOURCE REPOSITORY PRESERVATION:**
- Both repositories are strictly preserved as separate entities.
- Never delete, merge, or overwrite the `W3AndroidEngine` repository.
- The original source map `Fight_of_Characters9.1a(Asia).w3x` (8,378,373 bytes) remains immutable.

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

## 2. PERMANENT ENGINE ARCHITECTURE DECISION
1. **Primary Development Engine (Unity 6 LTS URP):**
   - Designated as the permanent primary production engine for commercial Android delivery.
   - 100% self-contained: packages original models, converted textures, audio, and gameplay C# code without depending on proprietary Blizzard base data.
   - Standalone modular combat foundation (`HeroStats.cs`, `CombatController.cs`, `AbilitySystem.cs`, `InventorySystem.cs`, `DamageCalculation.cs`).
2. **Reference & Extraction Tools (W3AndroidEngine & MPQ Diagnostics):**
   - Preserved at `D:\W3AndroidEngine\repo` as an immutable analytical tool for JASS decompilation, SLK parsing, binary extraction, and headless simulation validation.
3. **Rigid Dual-Repo Isolation:** Neither repository may be merged into or contaminated by the other.

---

## 3. ABSOLUTE ENGINEERING LAWS
1. Never fabricate test results, execution logs, or build artifacts.
2. Never substitute static Python validation for runtime execution.
3. If Unity Editor is unavailable on host PC, runtime-dependent tasks must be reported as `BLOCKED`.
4. Never delete or overwrite the Unity backup repository or W3AndroidEngine repository.
5. All downloaded dependencies, build caches, and repositories must reside on Drive D: due to Drive C: critical storage constraints (< 0.50 GB free).
