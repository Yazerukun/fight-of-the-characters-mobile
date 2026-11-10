/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOC.HeroSystems
{
    public static class HeroDatabase
    {
        private static List<HeroData> _cachedHeroes;

        public static List<HeroData> GetAllHeroes()
        {
            if (_cachedHeroes == null || _cachedHeroes.Count == 0)
            {
                InitializeDatabase();
            }
            return _cachedHeroes;
        }

        public static HeroData GetHeroById(string heroId)
        {
            var heroes = GetAllHeroes();
            for (int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i].heroId.Equals(heroId, StringComparison.OrdinalIgnoreCase))
                {
                    return heroes[i];
                }
            }
            return heroes.Count > 0 ? heroes[0] : null;
        }

        private static void InitializeDatabase()
        {
            _cachedHeroes = new List<HeroData>();

            // 1. MONKEY D. LUFFY (One Piece - STR)
            var luffy = ScriptableObject.CreateInstance<HeroData>();
            luffy.heroId = "hero_luffy";
            luffy.heroName = "Monkey D. Luffy";
            luffy.title = "Straw Hat Captain";
            luffy.franchise = FranchiseType.OnePiece;
            luffy.primaryAttribute = AttributeType.Strength;
            luffy.attackType = AttackType.Melee;
            luffy.loreDescription = "Captain of the Straw Hat Pirates. Possesses a rubber body granted by the Gomu Gomu no Mi, combining extreme physical resilience with crushing blunt force attacks.";
            luffy.portraitAssetPath = "Art/Textures/unknown_0028.png";
            luffy.modelPrefabPath = "Art/Heroes/Luffy.obj";
            luffy.baseStrength = 28;
            luffy.strengthGainPerLevel = 3.2f;
            luffy.baseAgility = 20;
            luffy.agilityGainPerLevel = 2.0f;
            luffy.baseIntelligence = 14;
            luffy.intelligenceGainPerLevel = 1.1f;
            luffy.baseHealth = 720;
            luffy.baseMana = 210;
            luffy.baseArmor = 5;
            luffy.baseMinDamage = 52;
            luffy.baseMaxDamage = 58;
            luffy.movementSpeed = 315f;
            luffy.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "luffy_q",
                    skillName = "Gomu Gomu Pistol",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0045.png",
                    description = "Stretches his arm backward and unleashes a high-speed straight punch dealing 90/140/190/240/290 + (STR × 2.2) physical damage in a linear path.",
                    manaCost = 70,
                    cooldownSeconds = 8f,
                    scalingFormula = "90 + (STR × 2.2)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "luffy_w",
                    skillName = "Gomu Gomu Bazooka",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0046.png",
                    description = "Stretches both arms backward and slams open palms into target, launching enemies 600 units backward and stunning them for 1.5 seconds.",
                    manaCost = 85,
                    cooldownSeconds = 12f,
                    scalingFormula = "120 + (STR × 1.8)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "luffy_e",
                    skillName = "Gear Second",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0047.png",
                    description = "Pumps blood throughout his body at extreme velocity. Grants +50% Attack Speed and +35% Movement Speed for 15 seconds at the cost of 5% current HP.",
                    manaCost = 60,
                    cooldownSeconds = 18f,
                    scalingFormula = "+50% AS, +35% MS",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "luffy_r",
                    skillName = "Gear Third: Giant Pistol",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0048.png",
                    description = "Bites into his thumb and inflates his bone structure into a giant fist, flattening an area for 350/500/650 + (STR × 3.0) crushing damage.",
                    manaCost = 150,
                    cooldownSeconds = 75f,
                    scalingFormula = "350 + (STR × 3.0)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(luffy);

            // 2. ICHIGO KUROSAKI (Bleach - AGI)
            var ichigo = ScriptableObject.CreateInstance<HeroData>();
            ichigo.heroId = "hero_ichigo";
            ichigo.heroName = "Ichigo Kurosaki";
            ichigo.title = "Substitute Soul Reaper";
            ichigo.franchise = FranchiseType.Bleach;
            ichigo.primaryAttribute = AttributeType.Agility;
            ichigo.attackType = AttackType.Melee;
            ichigo.loreDescription = "Human Soul Reaper wielding the Zanpakuto Zangetsu. Master of high-speed flash-step shunpo, spiritual compression waves, and hollowfication.";
            ichigo.portraitAssetPath = "Art/Textures/ichigo2.png";
            ichigo.modelPrefabPath = "Art/Heroes/Ichigo.obj";
            ichigo.baseStrength = 22;
            ichigo.strengthGainPerLevel = 2.2f;
            ichigo.baseAgility = 27;
            ichigo.agilityGainPerLevel = 3.0f;
            ichigo.baseIntelligence = 16;
            ichigo.intelligenceGainPerLevel = 1.4f;
            ichigo.baseHealth = 650;
            ichigo.baseMana = 280;
            ichigo.baseArmor = 4;
            ichigo.baseMinDamage = 48;
            ichigo.baseMaxDamage = 54;
            ichigo.movementSpeed = 320f;
            ichigo.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "ichigo_q",
                    skillName = "Getsuga Tenshou",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0049.png",
                    description = "Releases a concentrated crescent wave of condensed spiritual pressure from Zangetsu, piercing enemies for 85/135/185/235/285 + (AGI × 2.0) magical damage.",
                    manaCost = 65,
                    cooldownSeconds = 7f,
                    scalingFormula = "85 + (AGI × 2.0)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "ichigo_w",
                    skillName = "Flash Step (Shunpo)",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0050.png",
                    description = "Instantaneous spiritual teleportation up to 700 units distance, rendering Ichigo invulnerable for 0.25s during spatial transit.",
                    manaCost = 50,
                    cooldownSeconds = 9f,
                    scalingFormula = "700 Range Blink",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "ichigo_e",
                    skillName = "Hollow Mask",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0051.png",
                    description = "Summons his inner Hollow mask, amplifying all physical damage output by 30% and converting 15% of damage dealt into HP restoration for 12 seconds.",
                    manaCost = 90,
                    cooldownSeconds = 24f,
                    scalingFormula = "+30% DMG, +15% Lifesteal",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "ichigo_r",
                    skillName = "Bankai: Tensa Zangetsu",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0052.png",
                    description = "Compresses immense spiritual pressure into a pitch-black blade. Grants maximum movement speed (522), +100 Attack Speed, and empowers Getsuga Tenshou into Kuroi Getsuga (+50% bonus damage).",
                    manaCost = 140,
                    cooldownSeconds = 80f,
                    scalingFormula = "Max MS, +100 AS, Kuroi Getsuga",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(ichigo);

            // 3. SABER / ARTORIA PENDRAGON (Fate - STR)
            var saber = ScriptableObject.CreateInstance<HeroData>();
            saber.heroId = "hero_saber";
            saber.heroName = "Artoria Pendragon";
            saber.title = "King of Knights (Saber)";
            saber.franchise = FranchiseType.Fate;
            saber.primaryAttribute = AttributeType.Strength;
            saber.attackType = AttackType.Melee;
            saber.loreDescription = "Legendary King of Britain summoned under the Saber class. Wielder of the holy sword Excalibur and protected by the sheath Avalon.";
            saber.portraitAssetPath = "Art/Textures/unknown_0020.png";
            saber.modelPrefabPath = "Art/Heroes/Saber.obj";
            saber.baseStrength = 26;
            saber.strengthGainPerLevel = 2.8f;
            saber.baseAgility = 22;
            saber.agilityGainPerLevel = 2.2f;
            saber.baseIntelligence = 18;
            saber.intelligenceGainPerLevel = 1.6f;
            saber.baseHealth = 710;
            saber.baseMana = 300;
            saber.baseArmor = 5;
            saber.baseMinDamage = 50;
            saber.baseMaxDamage = 56;
            saber.movementSpeed = 315f;
            saber.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "saber_q",
                    skillName = "Invisible Air",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0053.png",
                    description = "Releases high-pressure wind compressed around Excalibur, blowing enemies backward and dealing 100/150/200/250/300 + (STR × 1.9) damage.",
                    manaCost = 75,
                    cooldownSeconds = 8f,
                    scalingFormula = "100 + (STR × 1.9)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "saber_w",
                    skillName = "Mana Burst",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0054.png",
                    description = "Infuses weapons and limbs with magical energy, boosting next basic attack by 140/200/260/320 bonus physical damage with a 1.2s stun.",
                    manaCost = 80,
                    cooldownSeconds = 10f,
                    scalingFormula = "140 + (STR × 2.4)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "saber_e",
                    skillName = "Instinct",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0055.png",
                    description = "Sixth sense honed through battlefield combat. Passively grants 18% evasion and 25% critical strike chance (2.2× damage).",
                    manaCost = 0,
                    cooldownSeconds = 0f,
                    scalingFormula = "+18% Evasion, +25% Crit",
                    isPassive = true
                },
                new HeroSkillDefinition {
                    skillId = "saber_r",
                    skillName = "Excalibur: Sword of Promised Victory",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0056.png",
                    description = "Raises the holy sword and fires a colossal pillar of divine light across 1200 range, obliterating everything in its path for 400/600/800 + (STR × 3.5) holy damage.",
                    manaCost = 180,
                    cooldownSeconds = 90f,
                    scalingFormula = "400 + (STR × 3.5)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(saber);

            // 4. ROCK LEE (Naruto - AGI)
            var lee = ScriptableObject.CreateInstance<HeroData>();
            lee.heroId = "hero_lee";
            lee.heroName = "Rock Lee";
            lee.title = "The Green Beast of Hidden Leaf";
            lee.franchise = FranchiseType.Naruto;
            lee.primaryAttribute = AttributeType.Agility;
            lee.attackType = AttackType.Melee;
            lee.loreDescription = "Specialist in pure Taijutsu through sheer determination and unrelenting discipline. Capable of unleashing the Eight Inner Gates.";
            lee.portraitAssetPath = "Art/Textures/lock_lee.png";
            lee.modelPrefabPath = "Art/Heroes/RockLee.obj";
            lee.baseStrength = 24;
            lee.strengthGainPerLevel = 2.4f;
            lee.baseAgility = 28;
            lee.agilityGainPerLevel = 3.3f;
            lee.baseIntelligence = 12;
            lee.intelligenceGainPerLevel = 0.9f;
            lee.baseHealth = 640;
            lee.baseMana = 180;
            lee.baseArmor = 4;
            lee.baseMinDamage = 49;
            lee.baseMaxDamage = 55;
            lee.movementSpeed = 330f;
            lee.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "lee_q",
                    skillName = "Leaf Hurricane (Konoha Senpu)",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0057.png",
                    description = "Delivers a lightning-fast spinning high-kick followed by a sweeping low-kick, dealing 95/145/195/245/295 + (AGI × 2.1) damage and slowing target by 40%.",
                    manaCost = 60,
                    cooldownSeconds = 7f,
                    scalingFormula = "95 + (AGI × 2.1)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "lee_w",
                    skillName = "Dynamic Entry",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0058.png",
                    description = "Leaps forward through the air with an explosive flying kick, slamming the target for 110/160/210/260 + (AGI × 1.8) damage and stunning for 1.25s.",
                    manaCost = 70,
                    cooldownSeconds = 10f,
                    scalingFormula = "110 + (AGI × 1.8)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "lee_e",
                    skillName = "Eight Gates Released: Gate of Life",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0059.png",
                    description = "Removes the body's natural limits. Turns skin crimson and increases Attack Speed by 80% and Movement Speed by 40% for 14 seconds.",
                    manaCost = 80,
                    cooldownSeconds = 22f,
                    scalingFormula = "+80% AS, +40% MS",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "lee_r",
                    skillName = "Hidden Lotus (Ura Renge)",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0060.png",
                    description = "Kicks the target into the stratosphere, wraps bandages around them, and executes a supersonic downward pile driver for 380/540/700 + (AGI × 3.6) physical devastation.",
                    manaCost = 130,
                    cooldownSeconds = 75f,
                    scalingFormula = "380 + (AGI × 3.6)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(lee);

            // 5. SON GOKU (Dragon Ball - STR)
            var goku = ScriptableObject.CreateInstance<HeroData>();
            goku.heroId = "hero_goku";
            goku.heroName = "Son Goku";
            goku.title = "Earth's Greatest Saiyan";
            goku.franchise = FranchiseType.DragonBall;
            goku.primaryAttribute = AttributeType.Strength;
            goku.attackType = AttackType.Melee;
            goku.loreDescription = "Legendary Saiyan warrior raised on Earth. Constantly breaks through mortal limits to protect the universe from annihilation.";
            goku.portraitAssetPath = "Art/Textures/unknown_0022.png";
            goku.modelPrefabPath = "Art/Heroes/Luffy.obj";
            goku.baseStrength = 30;
            goku.strengthGainPerLevel = 3.5f;
            goku.baseAgility = 22;
            goku.agilityGainPerLevel = 2.1f;
            goku.baseIntelligence = 15;
            goku.intelligenceGainPerLevel = 1.2f;
            goku.baseHealth = 760;
            goku.baseMana = 240;
            goku.baseArmor = 6;
            goku.baseMinDamage = 55;
            goku.baseMaxDamage = 62;
            goku.movementSpeed = 315f;
            goku.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "goku_q",
                    skillName = "Kamehameha",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0061.png",
                    description = "Gathers Ki in both cupped hands and releases a concentrated energy beam across 900 range, dealing 110/170/230/290 + (STR × 2.3) energy damage.",
                    manaCost = 80,
                    cooldownSeconds = 8f,
                    scalingFormula = "110 + (STR × 2.3)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "goku_w",
                    skillName = "Instant Transmission",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0062.png",
                    description = "Locks onto an enemy's Ki signature and instantly teleports directly behind them, granting +40% attack speed on the next 3 strikes.",
                    manaCost = 65,
                    cooldownSeconds = 11f,
                    scalingFormula = "Target Blink + Attack Speed",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "goku_e",
                    skillName = "Kaio-Ken ×20",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0063.png",
                    description = "Multiplies Ki output violently, surrounding Goku in a crimson aura. Increases attack damage by 45% and movement speed by 25% at a cost of 3% HP per second.",
                    manaCost = 50,
                    cooldownSeconds = 15f,
                    scalingFormula = "+45% DMG, +25% MS",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "goku_r",
                    skillName = "Spirit Bomb (Genki Dama)",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0064.png",
                    description = "Draws energy from all living beings into a gargantuan sphere of pure Ki. After a 1.5s channel, hurls it to obliterate a target area for 450/650/850 + (STR × 4.0) pure damage.",
                    manaCost = 200,
                    cooldownSeconds = 100f,
                    scalingFormula = "450 + (STR × 4.0) Pure",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(goku);

            // 6. NARUTO UZUMAKI (Naruto - STR)
            var naruto = ScriptableObject.CreateInstance<HeroData>();
            naruto.heroId = "hero_naruto";
            naruto.heroName = "Naruto Uzumaki";
            naruto.title = "Hero of the Hidden Leaf";
            naruto.franchise = FranchiseType.Naruto;
            naruto.primaryAttribute = AttributeType.Strength;
            naruto.attackType = AttackType.Melee;
            naruto.loreDescription = "Jinchuriki of the Nine-Tails Fox (Kurama). Master of shadow clones, spiraling chakra spheres, and Sage Mode transformation.";
            naruto.portraitAssetPath = "Art/Textures/unknown_0025.png";
            naruto.modelPrefabPath = "Art/Heroes/Luffy.obj";
            naruto.baseStrength = 27;
            naruto.strengthGainPerLevel = 3.0f;
            naruto.baseAgility = 21;
            naruto.agilityGainPerLevel = 2.0f;
            naruto.baseIntelligence = 16;
            naruto.intelligenceGainPerLevel = 1.3f;
            naruto.baseHealth = 730;
            naruto.baseMana = 260;
            naruto.baseArmor = 5;
            naruto.baseMinDamage = 51;
            naruto.baseMaxDamage = 57;
            naruto.movementSpeed = 310f;
            naruto.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "naruto_q",
                    skillName = "Rasengan",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0065.png",
                    description = "Compresses rotating chakra into the palm and slams it into the target, dealing 100/155/210/265/320 + (STR × 2.0) damage and stunning for 1.0s.",
                    manaCost = 75,
                    cooldownSeconds = 8f,
                    scalingFormula = "100 + (STR × 2.0)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "naruto_w",
                    skillName = "Shadow Clone Jutsu (Kage Bunshin)",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0066.png",
                    description = "Creates 3 tangible shadow clones that deal 30% of Naruto's damage and absorb 200% damage for 15 seconds.",
                    manaCost = 90,
                    cooldownSeconds = 16f,
                    scalingFormula = "3 Clones (30% DMG)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "naruto_e",
                    skillName = "Sage Mode",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0067.png",
                    description = "Harmonizes with natural energy. Increases base armor by +8, attack damage by +35, and extends attack reach with invisible frog kumite for 16 seconds.",
                    manaCost = 80,
                    cooldownSeconds = 20f,
                    scalingFormula = "+8 Armor, +35 DMG",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "naruto_r",
                    skillName = "Wind Style: Rasenshuriken",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0068.png",
                    description = "Infuses wind chakra into a giant vortex shuriken and hurls it across 1000 range. Expands into a microscopic cellular storm dealing 420/600/780 + (STR × 3.5) damage.",
                    manaCost = 175,
                    cooldownSeconds = 85f,
                    scalingFormula = "420 + (STR × 3.5)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(naruto);

            // 7. SASUKE UCHIHA (Naruto - AGI)
            var sasuke = ScriptableObject.CreateInstance<HeroData>();
            sasuke.heroId = "hero_sasuke";
            sasuke.heroName = "Sasuke Uchiha";
            sasuke.title = "Avenger of the Uchiha";
            sasuke.franchise = FranchiseType.Naruto;
            sasuke.primaryAttribute = AttributeType.Agility;
            sasuke.attackType = AttackType.Melee;
            sasuke.loreDescription = "Sole surviving heir of the Uchiha clan. Wields the Sharingan and lightning release, cutting through all barriers with the Chidori blade.";
            sasuke.portraitAssetPath = "Art/Textures/unknown_0027.png";
            sasuke.modelPrefabPath = "Art/Heroes/Ichigo.obj";
            sasuke.baseStrength = 22;
            sasuke.strengthGainPerLevel = 2.1f;
            sasuke.baseAgility = 28;
            sasuke.agilityGainPerLevel = 3.2f;
            sasuke.baseIntelligence = 18;
            sasuke.intelligenceGainPerLevel = 1.7f;
            sasuke.baseHealth = 630;
            sasuke.baseMana = 310;
            sasuke.baseArmor = 3;
            sasuke.baseMinDamage = 47;
            sasuke.baseMaxDamage = 53;
            sasuke.movementSpeed = 320f;
            sasuke.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "sasuke_q",
                    skillName = "Chidori",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0069.png",
                    description = "Concentrates high-frequency lightning chakra in his hand and charges forward at high speed, piercing through the target for 95/150/205/260 + (AGI × 2.2) damage.",
                    manaCost = 70,
                    cooldownSeconds = 8f,
                    scalingFormula = "95 + (AGI × 2.2)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "sasuke_w",
                    skillName = "Chidori Nagashi (Current)",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0070.png",
                    description = "Discharges an omnidirectional field of electricity from his body, paralyzing all nearby enemies for 1.5 seconds and dealing 80/120/160/200 damage.",
                    manaCost = 85,
                    cooldownSeconds = 12f,
                    scalingFormula = "AoE Stun + Lightning",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "sasuke_e",
                    skillName = "Sharingan Precision",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0071.png",
                    description = "Passively perceives enemy movement trajectories, granting 20% evasion and 20% chance to critically strike for 2.0× damage.",
                    manaCost = 0,
                    cooldownSeconds = 0f,
                    scalingFormula = "+20% Evasion, +20% Crit",
                    isPassive = true
                },
                new HeroSkillDefinition {
                    skillId = "sasuke_r",
                    skillName = "Kirin (Thunder Dragon)",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0072.png",
                    description = "Channels natural atmospheric thunderclouds into a gargantuan lightning mythical beast. Strikes the target zone at 1/1000th of a second for 440/640/840 + (AGI × 3.8) devastation.",
                    manaCost = 190,
                    cooldownSeconds = 90f,
                    scalingFormula = "440 + (AGI × 3.8)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(sasuke);

            // 8. RORONOA ZORO (One Piece - AGI)
            var zoro = ScriptableObject.CreateInstance<HeroData>();
            zoro.heroId = "hero_zoro";
            zoro.heroName = "Roronoa Zoro";
            zoro.title = "Pirate Hunter (Santoryu)";
            zoro.franchise = FranchiseType.OnePiece;
            zoro.primaryAttribute = AttributeType.Agility;
            zoro.attackType = AttackType.Melee;
            zoro.loreDescription = "First mate of the Straw Hat Pirates. Master of the Three-Sword Style (Santoryu) who aspires to become the world's greatest swordsman.";
            zoro.portraitAssetPath = "Art/Textures/unknown_0033.png";
            zoro.modelPrefabPath = "Art/Heroes/Ichigo.obj";
            zoro.baseStrength = 25;
            zoro.strengthGainPerLevel = 2.6f;
            zoro.baseAgility = 27;
            zoro.agilityGainPerLevel = 3.0f;
            zoro.baseIntelligence = 13;
            zoro.intelligenceGainPerLevel = 1.0f;
            zoro.baseHealth = 680;
            zoro.baseMana = 200;
            zoro.baseArmor = 5;
            zoro.baseMinDamage = 51;
            zoro.baseMaxDamage = 57;
            zoro.movementSpeed = 315f;
            zoro.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "zoro_q",
                    skillName = "Oni Giri (Demon Slash)",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0073.png",
                    description = "Crosses swords and dashes through target at blinding speed, dealing 100/150/200/250 + (AGI × 2.0) physical slash damage.",
                    manaCost = 65,
                    cooldownSeconds = 8f,
                    scalingFormula = "100 + (AGI × 2.0)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "zoro_w",
                    skillName = "Tora Gari (Tiger Hunt)",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0074.png",
                    description = "Leaps forward swinging two overhead blades downward, striking targets in a frontal cone and bleeding them for 3 seconds.",
                    manaCost = 70,
                    cooldownSeconds = 10f,
                    scalingFormula = "Cone Bleed + Physical",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "zoro_e",
                    skillName = "Busoshoku Haki (Armament)",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0075.png",
                    description = "Hardens swords with black armament haki. Attacks bypass 30% of target armor and gain +25% bonus critical strike chance.",
                    manaCost = 0,
                    cooldownSeconds = 0f,
                    scalingFormula = "30% Armor Piercing",
                    isPassive = true
                },
                new HeroSkillDefinition {
                    skillId = "zoro_r",
                    skillName = "Sanzen Sekai (Three Thousand Worlds)",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0076.png",
                    description = "Spins two blades like propellers while charging forward, carving through all enemies in a 600-unit line for 390/570/750 + (AGI × 3.5) physical annihilation.",
                    manaCost = 150,
                    cooldownSeconds = 80f,
                    scalingFormula = "390 + (AGI × 3.5)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(zoro);

            // 9. PORTGAS D. ACE (One Piece - INT)
            var ace = ScriptableObject.CreateInstance<HeroData>();
            ace.heroId = "hero_ace";
            ace.heroName = "Portgas D. Ace";
            ace.title = "Fire Fist Ace";
            ace.franchise = FranchiseType.OnePiece;
            ace.primaryAttribute = AttributeType.Intelligence;
            ace.attackType = AttackType.Ranged;
            ace.loreDescription = "Commander of the Whitebeard Pirates' 2nd Division. User of the Mera Mera no Mi, commanding searing pyrotechnic devastation.";
            ace.portraitAssetPath = "Art/Textures/unknown_0029.png";
            ace.modelPrefabPath = "Art/Heroes/Luffy.obj";
            ace.baseStrength = 20;
            ace.strengthGainPerLevel = 1.9f;
            ace.baseAgility = 21;
            ace.agilityGainPerLevel = 2.0f;
            ace.baseIntelligence = 28;
            ace.intelligenceGainPerLevel = 3.3f;
            ace.baseHealth = 600;
            ace.baseMana = 380;
            ace.baseArmor = 3;
            ace.baseMinDamage = 45;
            ace.baseMaxDamage = 51;
            ace.movementSpeed = 310f;
            ace.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "ace_q",
                    skillName = "Hiken (Fire Fist)",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0077.png",
                    description = "Transforms his fist into a roaring column of flame, incinerating a wide corridor for 115/175/235/295 + (INT × 2.4) magic fire damage.",
                    manaCost = 85,
                    cooldownSeconds = 8f,
                    scalingFormula = "115 + (INT × 2.4)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "ace_w",
                    skillName = "Higan (Fire Gun)",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0078.png",
                    description = "Points index fingers like pistols and fires rapid bullets of condensed fire, burning the target for 120/180/240 damage and applying 3s scorch.",
                    manaCost = 70,
                    cooldownSeconds = 6f,
                    scalingFormula = "120 + (INT × 1.6)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "ace_e",
                    skillName = "Logia Intangibility",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0079.png",
                    description = "Passively disperses incoming physical attacks into embers. Grants 22% chance to completely negate incoming physical damage instances.",
                    manaCost = 0,
                    cooldownSeconds = 0f,
                    scalingFormula = "22% Damage Negation",
                    isPassive = true
                },
                new HeroSkillDefinition {
                    skillId = "ace_r",
                    skillName = "Dai Enkai: Entei (Great Flame Emperor)",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0080.png",
                    description = "Manifests a colossal miniature sun of churning flames above his head and drops it upon the battlefield, obliterating a 700-radius zone for 450/650/850 + (INT × 3.8) fire devastation.",
                    manaCost = 210,
                    cooldownSeconds = 95f,
                    scalingFormula = "450 + (INT × 3.8)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(ace);

            // 10. PRINCE VEGETA (Dragon Ball - STR)
            var vegeta = ScriptableObject.CreateInstance<HeroData>();
            vegeta.heroId = "hero_vegeta";
            vegeta.heroName = "Prince Vegeta";
            vegeta.title = "Proud Prince of Saiyans";
            vegeta.franchise = FranchiseType.DragonBall;
            vegeta.primaryAttribute = AttributeType.Strength;
            vegeta.attackType = AttackType.Melee;
            vegeta.loreDescription = "Fierce Saiyan royalty possessing unmatched pride and relentless combat ferocity. Obliterates foes with explosive Galick Gun and Big Bang blasts.";
            vegeta.portraitAssetPath = "Art/Textures/unknown_0023.png";
            vegeta.modelPrefabPath = "Art/Heroes/Luffy.obj";
            vegeta.baseStrength = 29;
            vegeta.strengthGainPerLevel = 3.3f;
            vegeta.baseAgility = 23;
            vegeta.agilityGainPerLevel = 2.2f;
            vegeta.baseIntelligence = 15;
            vegeta.intelligenceGainPerLevel = 1.1f;
            vegeta.baseHealth = 740;
            vegeta.baseMana = 230;
            vegeta.baseArmor = 6;
            vegeta.baseMinDamage = 54;
            vegeta.baseMaxDamage = 60;
            vegeta.movementSpeed = 315f;
            vegeta.skills = new List<HeroSkillDefinition>
            {
                new HeroSkillDefinition {
                    skillId = "vegeta_q",
                    skillName = "Galick Gun",
                    keyBinding = "Q",
                    iconPath = "Art/Textures/unknown_0081.png",
                    description = "Gathers violet Ki and discharges a furious beam across 900 range, dealing 110/165/220/275 + (STR × 2.2) Ki damage.",
                    manaCost = 75,
                    cooldownSeconds = 8f,
                    scalingFormula = "110 + (STR × 2.2)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "vegeta_w",
                    skillName = "Big Bang Attack",
                    keyBinding = "W",
                    iconPath = "Art/Textures/unknown_0082.png",
                    description = "Extends open palm forward and fires a concentrated sphere of destructive Ki, causing a devastating 350-radius explosion for 130/190/250 + (STR × 1.8) damage.",
                    manaCost = 85,
                    cooldownSeconds = 11f,
                    scalingFormula = "130 + (STR × 1.8)",
                    isPassive = false
                },
                new HeroSkillDefinition {
                    skillId = "vegeta_e",
                    skillName = "Saiyan Pride",
                    keyBinding = "E",
                    iconPath = "Art/Textures/unknown_0083.png",
                    description = "As Vegeta's health depletes, his combat power escalates. Passively gains up to +60% bonus attack damage and +40% attack speed at low HP.",
                    manaCost = 0,
                    cooldownSeconds = 0f,
                    scalingFormula = "Up to +60% DMG at Low HP",
                    isPassive = true
                },
                new HeroSkillDefinition {
                    skillId = "vegeta_r",
                    skillName = "Final Flash",
                    keyBinding = "R",
                    iconPath = "Art/Textures/unknown_0084.png",
                    description = "Charges maximum Ki in both hands extended outward before slamming them together to unleash a universe-shattering golden beam for 460/660/860 + (STR × 3.8) devastation.",
                    manaCost = 195,
                    cooldownSeconds = 90f,
                    scalingFormula = "460 + (STR × 3.8)",
                    isUltimate = true
                }
            };
            _cachedHeroes.Add(vegeta);
        }
    }
}
