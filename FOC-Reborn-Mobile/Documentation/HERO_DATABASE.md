# FIGHT OF CHARACTERS 9.1a ASIA — HERO DATABASE (40 HEROES)

This database documents the canonical 40 heroes featured in Fight of Characters 9.1a Asia, organized by franchise, base attributes, stat scaling, and core form transformations as implemented in the mobile port.

---

## 1. FRANCHISE DISTRIBUTION

| Franchise | Count | Heroes |
|---|---|---|
| **BLEACH** | 9 | Ichigo Kurosaki, Byakuya Kuchiki, Tōshirō Hitsugaya, Kenpachi Zaraki, Uryū Ishida, Renji Abarai, Ulquiorra Cifer, Grimmjow Jaegerjaquez, Sōsuke Aizen |
| **NARUTO** | 9 | Sasuke Uchiha, Madara Uchiha, Obito Uchiha, Gaara, Rock Lee, Neji Hyūga, Hiruzen Sarutobi, Orochimaru, Itachi Uchiha |
| **ONE PIECE** | 7 | Monkey D. Luffy, Portgas D. Ace, Whitebeard (Edward Newgate), Gol D. Roger, Rob Lucci, Enel, Roronoa Zoro |
| **FATE** | 4 | Archer (EMIYA), Saber (Artoria Pendragon), Gilgamesh, Lancer (Cu Chulainn) |
| **MIXED** | 11 | Inuyasha (Inuyasha), Son Goku (Dragon Ball), Iori Yagami (KOF), Kyo Kusanagi (KOF), Alucard (Hellsing), Metal Bat (One Punch Man), Kakashi Hatake (Naruto/Mixed), Sanji (One Piece/Mixed), Riku (Kingdom Hearts), Cloud Strife, Sephiroth |

---

## 2. HERO ATTRIBUTE PROFILES & STAT FORMULAS

Warcraft III Attribute Mechanics Preserved:
- **Primary Attribute:** Each point grants +1 Attack Damage.
- **Strength (STR):** 1 STR = +25 Max HP, +0.05 HP Regen/sec.
- **Agility (AGI):** 1 AGI = +0.04 Attack Speed, +0.3 Armor.
- **Intelligence (INT):** 1 INT = +15 Max MP, +0.05 MP Regen/sec, +0.5% Magic Amplification.

### Detailed Roster Sample (Canonical 12 Illustrated in UI)

#### 1. Ichigo Kurosaki
- **Franchise:** Bleach
- **Primary Attribute:** Agility
- **Base Stats:** HP: 650 | MP: 280 | Base DMG: 48-54 | Armor: 4 | Range: Melee (128) | MoveSpeed: 320
- **Stat Growth:** STR +2.2 | AGI +3.0 | INT +1.4
- **Form / Stance:** Bankai: Blood Chains (Form 2: Hollow Mask, Form 3: Mugetsu)
- **Skills:**
  1. *Hollow Instinct* (Passive): Converts 15% physical damage dealt into temporary shield; grants bonus MS when enemy is below 35% HP.
  2. *Getsuga Tenshō* (Active Skill 1): Crescent shockwave dealing 85/130/175/220/265 + (AGI × 2.0) magic damage in a 10m line. Evolves to Getsuga Jūjishō at rank 3.
  3. *Hollowfication* (Active Skill 2): Summons the Hollow Mask, boosting Attack Speed by 40/60/80/100% and granting +25% bonus movement speed for 12 seconds.
  4. *Shunpo* (Active Skill 3): Instant directional blink (range: 650 units). Next attack within 2.5s deals 1.5x critical damage.
  5. *Bankai: Blood Chains / Tensa Zangetsu* (Ultimate): Enters full Bankai stance. Cooldowns reduced by 30%, normal attacks launch mini-Getsuga waves for 30s.

#### 2. Sasuke Uchiha
- **Franchise:** Naruto
- **Primary Attribute:** Agility
- **Base Stats:** HP: 620 | MP: 310 | Base DMG: 46-52 | Armor: 3 | Range: Melee (128) | MoveSpeed: 315
- **Stat Growth:** STR +2.0 | AGI +3.1 | INT +1.8
- **Form:** Curse Mark Stage 2 / Rinnegan
- **Skills:** Chidori, Chidori Nagashi, Fire Style: Dragon Fire, Sharingan Dodge, Kirin (Ultimate).

#### 3. Monkey D. Luffy
- **Franchise:** One Piece
- **Primary Attribute:** Strength
- **Base Stats:** HP: 720 | MP: 220 | Base DMG: 52-58 | Armor: 5 | Range: Melee (140) | MoveSpeed: 310
- **Stat Growth:** STR +3.2 | AGI +2.1 | INT +1.0
- **Form:** Gear Second / Gear Fourth: Boundman
- **Skills:** Gomu Gomu no Pistol, Gomu Gomu no Bazooka, Gomu Gomu no Gatling, Gear Second (Buff), Gomu Gomu no Giant Axe / King Kong Gun.

#### 4. Portgas D. Ace
- **Franchise:** One Piece
- **Primary Attribute:** Intelligence
- **Base Stats:** HP: 600 | MP: 350 | Base DMG: 44-50 | Armor: 3 | Range: Ranged (550) | MoveSpeed: 305
- **Stat Growth:** STR +1.9 | AGI +2.0 | INT +3.2
- **Skills:** Kagero (Heat Haze), Hiken (Fire Fist), Higan (Fire Gun), Enjomo (Flame Commandment), Dai Enkai: Entei (Great Flame Commandment: Flame Emperor).

#### 5. Byakuya Kuchiki
- **Franchise:** Bleach
- **Primary Attribute:** Agility
- **Base Stats:** HP: 590 | MP: 340 | Base DMG: 45-51 | Armor: 3 | Range: Melee (135) | MoveSpeed: 315
- **Skills:** Senka, Hadō #33 Sōkatsui, Bakudō #61 Rikujōkōrō, Senbonzakura Whirl, Bankai: Senbonzakura Kageyoshi (Ultimate).

#### 6. Whitebeard (Edward Newgate)
- **Franchise:** One Piece
- **Primary Attribute:** Strength
- **Base Stats:** HP: 850 | MP: 240 | Base DMG: 62-70 | Armor: 6 | Range: Melee (160) | MoveSpeed: 295
- **Stat Growth:** STR +3.8 | AGI +1.5 | INT +1.2
- **Skills:** Quake Punch, Air Crack Shockwave, Halberd Cleave, Unyielding Giant (Passive), Island Shaker (Ultimate).

#### 7. Archer (EMIYA)
- **Franchise:** Fate
- **Primary Attribute:** Agility
- **Base Stats:** HP: 610 | MP: 330 | Base DMG: 47-53 | Armor: 4 | Range: Ranged (600) | MoveSpeed: 310
- **Skills:** Kanshou & Bakuya Overedge, Caladbolg II (Fake Spiral Sword), Rho Aias (Seven Rings of Seven Petals), Eye of the Mind (True), Unlimited Blade Works (Ultimate Reality Marble).

#### 8. Saber (Artoria Pendragon)
- **Franchise:** Fate
- **Primary Attribute:** Strength
- **Base Stats:** HP: 710 | MP: 300 | Base DMG: 50-56 | Armor: 5 | Range: Melee (130) | MoveSpeed: 315
- **Skills:** Invisible Air, Mana Burst, Strike Air, Instinct (Passive Dodge), Excalibur (Ultimate Noble Phantasm).

#### 9. Son Goku
- **Franchise:** Mixed (Dragon Ball)
- **Primary Attribute:** Strength
- **Base Stats:** HP: 750 | MP: 260 | Base DMG: 54-60 | Armor: 5 | Range: Melee (128) | MoveSpeed: 320
- **Form:** Super Saiyan 1 / Super Saiyan 3
- **Skills:** Kaioken Rush, Instant Transmission, Kamehameha, Solar Flare, Spirit Bomb (Genki Dama).

#### 10. Inuyasha
- **Franchise:** Mixed (Inuyasha)
- **Primary Attribute:** Strength
- **Base Stats:** HP: 690 | MP: 270 | Base DMG: 51-57 | Armor: 4 | Range: Melee (130) | MoveSpeed: 315
- **Skills:** Iron Reaver Soul Stealer, Blades of Blood, Wind Scar (Kaze no Kizu), Backlash Wave (Bakuryūha), Meidō Zangetsuha.

#### 11. Iori Yagami
- **Franchise:** Mixed (King of Fighters)
- **Primary Attribute:** Agility
- **Base Stats:** HP: 640 | MP: 290 | Base DMG: 49-55 | Armor: 4 | Range: Melee (128) | MoveSpeed: 320
- **Skills:** 108 Shiki: Yamibarai (Purple Flame Wave), 100 Shiki: Oniyaki, 127 Shiki: Aoibana, Kuzukaze (Scum Gale), Kin 1211 Shiki: Yaotome (Maiden Masher).

#### 12. Gaara
- **Franchise:** Naruto
- **Primary Attribute:** Intelligence
- **Base Stats:** HP: 630 | MP: 360 | Base DMG: 43-49 | Armor: 7 (Sand Armor) | Range: Ranged (500) | MoveSpeed: 295
- **Skills:** Sand Binding Coffin (Sabaku Kyū), Sand Waterfall Funeral (Sabaku Sōsō), Shield of Shukaku, Sand Tsunami, Desert Imperial Funeral (Sabaku Taisō).
