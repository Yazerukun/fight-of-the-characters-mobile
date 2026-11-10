# FIGHT OF CHARACTERS 9.1a ASIA — SKILL DATABASE

## 1. CORE MECHANICS MATRIX

All skills in the Fight of Characters mobile port maintain strict parity with Warcraft III patch 1.24b/1.26a custom mechanics:

1. **Damage Classification:**
   - **Physical Damage:** Mitigated by Armor formula: $\text{Reduction} = \frac{\text{Armor} \times 0.06}{1 + (0.06 \times |\text{Armor}|)}$.
   - **Magic Damage:** Mitigated by Base Hero Magic Resistance (default 25%) and modified by Intelligence Magic Amp ($+0.05\% \text{ per INT point}$).
   - **Pure / Absolute Damage:** Pierces armor, magic resistance, and invulnerability shields.

2. **Cooldown & Channeling:**
   - Skills enter cooldown immediately upon cast point execution.
   - Channeling skills can be interrupted by Stun, Silence, or Knockback.

3. **Hit-Stop & Screen Trauma Shake:**
   - Impacts trigger 30ms to 60ms micro-pause for tactile weight.
   - Screen trauma uses quadratic decay: $\text{Trauma} \in [0.0, 1.0]$, $\text{Shake} = \text{Trauma}^2$.

---

## 2. SKILL DEFINITIONS (SPECIFICATION SAMPLES)

### Ichigo Kurosaki
1. **Hollow Instinct (Passive):**
   - *Rank 1-4:* Converts 10% / 15% / 20% / 25% of all physical damage dealt into temporary life shield.
   - *Bloodlust trigger:* When targeting enemies below 35% health, moves 15% / 20% / 25% / 30% faster.

2. **Getsuga Tenshō (Active Skill 1):**
   - *Targeting:* Line vector (length: 10m / 700 units, width: 1.5m / 140 units).
   - *Cooldown:* 11 / 10 / 9 / 8 / 7 seconds.
   - *Mana Cost:* 75 / 90 / 105 / 120 / 135.
   - *Damage:* $85 / 130 / 175 / 220 / 265 + (\text{AGI} \times 2.0)$ Magic Damage.
   - *Evolution:* At rank 3+, upgrades to *Getsuga Jūjishō* (cross-slash), expanding width to 2.5m and adding a 0.75s 40% slow.

3. **Hollowfication (Active Skill 2 / Form Buff):**
   - *Duration:* 12 seconds.
   - *Cooldown:* 24 / 22 / 20 / 18 / 16 seconds.
   - *Mana Cost:* 90 / 100 / 110 / 120 / 130.
   - *Effects:* +40% / 60% / 80% / 100% Attack Speed, +25% MoveSpeed, and regular attacks splash 30% damage to nearby enemies.

4. **Shunpo (Active Skill 3 / Mobility):**
   - *Range:* 500 / 575 / 650 / 725 / 800 units.
   - *Cooldown:* 14 / 12 / 10 / 8 / 6 seconds.
   - *Mana Cost:* 60 / 65 / 70 / 75 / 80.
   - *Effect:* Instantly teleports to target location. Next basic attack within 2.5 seconds delivers a guaranteed 150% / 175% / 200% / 225% / 250% critical strike.

5. **Bankai: Blood Chains / Tensa Zangetsu (Ultimate):**
   - *Duration:* 25 / 30 / 35 seconds.
   - *Cooldown:* 90 / 80 / 70 seconds.
   - *Mana Cost:* 150 / 200 / 250.
   - *Transformation:* Swaps model to Bankai coat, reduces all active skill cooldowns by 30%, and every 3rd basic attack releases a mini-Getsuga shockwave dealing $150 + (\text{AGI} \times 1.5)$ magic damage.
