/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using UnityEngine;

namespace FOC.SkillSystems
{
    public enum DamageClassification
    {
        Physical,
        Magical,
        Pure
    }

    public enum SkillTargetType
    {
        Passive,
        NoTarget,
        PointTarget,
        UnitTarget,
        LinearVector,
        AreaOfEffect
    }

    [CreateAssetMenu(fileName = "NewSkillData", menuName = "FOC/Skill Data")]
    public class SkillData : ScriptableObject
    {
        [Header("Identifiers")]
        public string skillId;
        public string skillName;
        public string internalCode; // e.g. A001 from WC3
        public Sprite icon;
        public DamageClassification damageType;
        public SkillTargetType targetType;

        [Header("Values per Rank (1 to 5)")]
        public int maxRank = 5;
        public int[] manaCosts = new int[] { 75, 90, 105, 120, 135 };
        public float[] cooldowns = new float[] { 11f, 10f, 9f, 8f, 7f };
        public float[] baseDamages = new float[] { 85f, 130f, 175f, 220f, 265f };
        public float[] castRanges = new float[] { 700f, 700f, 700f, 700f, 700f };
        public float[] areaOfEffectRadius = new float[] { 140f, 140f, 140f, 140f, 140f };

        [Header("Scaling Coefficients")]
        public float strengthMultiplier = 0f;
        public float agilityMultiplier = 2.0f;
        public float intelligenceMultiplier = 0f;

        [Header("Visual & Audio FX")]
        public GameObject projectilePrefab;
        public GameObject impactVfxPrefab;
        public AudioClip castAudioClip;
        public AudioClip impactAudioClip;

        [Header("Descriptions & Lore")]
        [TextArea(2, 4)]
        public string tooltipSummary;
        [TextArea(3, 6)]
        public string detailedDescription;

        public float ComputeDamage(int rank, int heroStr, int heroAgi, int heroInt)
        {
            int r = Mathf.Clamp(rank - 1, 0, maxRank - 1);
            float baseDmg = (r < baseDamages.Length) ? baseDamages[r] : 0f;
            float statBonus = (heroStr * strengthMultiplier) + 
                              (heroAgi * agilityMultiplier) + 
                              (heroInt * intelligenceMultiplier);
            return baseDmg + statBonus;
        }

        public float GetCooldown(int rank)
        {
            int r = Mathf.Clamp(rank - 1, 0, maxRank - 1);
            return (r < cooldowns.Length) ? cooldowns[r] : 10f;
        }

        public int GetManaCost(int rank)
        {
            int r = Mathf.Clamp(rank - 1, 0, maxRank - 1);
            return (r < manaCosts.Length) ? manaCosts[r] : 50;
        }
    }
}
