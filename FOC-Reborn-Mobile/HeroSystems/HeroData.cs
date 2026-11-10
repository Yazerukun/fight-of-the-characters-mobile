/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOC.HeroSystems
{
    public enum AttributeType
    {
        Strength,
        Agility,
        Intelligence
    }

    public enum FranchiseType
    {
        Bleach,
        Naruto,
        OnePiece,
        Fate,
        DragonBall,
        Mixed
    }

    public enum AttackType
    {
        Melee,
        Ranged
    }

    [System.Serializable]
    public class HeroSkillDefinition
    {
        public string skillId;
        public string skillName;
        public string keyBinding; // Q, W, E, R, T / Passive
        public string iconPath;
        public string description;
        public int manaCost;
        public float cooldownSeconds;
        public string scalingFormula;
        public bool isPassive;
        public bool isUltimate;
    }

    [CreateAssetMenu(fileName = "NewHeroData", menuName = "FOC/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroId;
        public string heroName;
        public string title;
        public FranchiseType franchise;
        public AttributeType primaryAttribute;
        public AttackType attackType;
        [TextArea(3, 6)]
        public string loreDescription;

        [Header("Visual & Audio Assets")]
        public string portraitAssetPath;
        public string fullArtworkPath;
        public string modelPrefabPath;
        public string selectVoiceClipPath;
        public string lockInVoiceClipPath;

        [Header("Base Attributes (WC3 9.1a Exact)")]
        public int baseStrength = 22;
        public float strengthGainPerLevel = 2.4f;

        public int baseAgility = 24;
        public float agilityGainPerLevel = 2.8f;

        public int baseIntelligence = 16;
        public float intelligenceGainPerLevel = 1.5f;

        [Header("Vitals & Movement")]
        public int baseHealth = 650;
        public float baseHealthRegen = 2.5f;
        public int baseMana = 260;
        public float baseManaRegen = 1.2f;
        public int baseArmor = 4;
        public int baseMinDamage = 48;
        public int baseMaxDamage = 54;
        public float attackRange = 140f;
        public float attackSpeed = 1.7f;
        public float movementSpeed = 320f;

        [Header("Skills (5 Standard Slots)")]
        public List<HeroSkillDefinition> skills = new List<HeroSkillDefinition>();

        [Header("Forms & Transformations")]
        public bool hasTransformations;
        public string defaultFormName = "Normal";
        public string unlockedFormName = "Bankai / Gear / Mode";

        public int CalculateMaxHealth(int level)
        {
            int str = baseStrength + Mathf.FloorToInt(strengthGainPerLevel * (level - 1));
            return baseHealth + (str * 25);
        }

        public int CalculateMaxMana(int level)
        {
            int intel = baseIntelligence + Mathf.FloorToInt(intelligenceGainPerLevel * (level - 1));
            return baseMana + (intel * 15);
        }

        public int CalculateAverageDamage(int level)
        {
            int primaryStat = 0;
            switch (primaryAttribute)
            {
                case AttributeType.Strength:
                    primaryStat = baseStrength + Mathf.FloorToInt(strengthGainPerLevel * (level - 1));
                    break;
                case AttributeType.Agility:
                    primaryStat = baseAgility + Mathf.FloorToInt(agilityGainPerLevel * (level - 1));
                    break;
                case AttributeType.Intelligence:
                    primaryStat = baseIntelligence + Mathf.FloorToInt(intelligenceGainPerLevel * (level - 1));
                    break;
            }
            int baseAvg = (baseMinDamage + baseMaxDamage) / 2;
            return baseAvg + primaryStat;
        }
    }
}
