/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using UnityEngine;

namespace FOC.CombatSystems
{
    public enum AttackType
    {
        Hero,       // 100% vs Hero, 100% vs Normal, 100% vs Light/Medium/Heavy
        Normal,     // 100% vs Medium, 150% vs Light, 100% vs Unarmored, 70% vs Hero
        Pierce,     // 150% vs Light, 75% vs Medium, 200% vs Unarmored, 50% vs Hero
        Siege,      // 150% vs Fortified, 100% vs Unarmored, 50% vs Hero
        Chaos,      // 100% vs All defense types
        Magic       // 125% vs Light, 75% vs Heavy, 100% vs Medium, 70% vs Hero
    }

    public enum ArmorType
    {
        Hero,
        Light,
        Medium,
        Heavy,
        Fortified,
        Unarmored
    }

    public enum DamageClassification
    {
        Physical,
        Magical,
        Pure,
        Chaos
    }

    public struct DamageResult
    {
        public int rawDamage;
        public int mitigatedDamage;
        public bool isCritical;
        public bool isEvaded;
        public DamageClassification classification;
        public AttackType attackType;
        public ArmorType armorType;
    }

    /* === 03. PRECISION MATH, VECTORS & COLLISION HELPERS === */
    public static class DamageCalculation
    {
        public const float WC3_ARMOR_FACTOR = 0.06f;
        public const float HERO_NATURAL_MAGIC_RESISTANCE = 0.25f; // Standard WC3 25% spell resistance
        public const float MAX_ARMOR_REDUCTION = 0.85f;          // 85% cap on armor mitigation
        public const float MAX_NEGATIVE_ARMOR_VULNERABILITY = 0.71f; // 71% max bonus damage from negative armor

        // WC3 Attack vs Armor Type Matrix
        private static readonly float[,] AttackArmorTable = new float[6, 6]
        {
            // Hero, Light, Medium, Heavy, Fortified, Unarmored
            /* Hero */    { 1.00f, 1.00f, 1.00f, 1.00f, 0.50f, 1.00f },
            /* Normal */  { 0.70f, 1.50f, 1.00f, 1.00f, 0.70f, 1.00f },
            /* Pierce */  { 0.50f, 2.00f, 0.75f, 1.00f, 0.35f, 1.50f },
            /* Siege */   { 0.50f, 1.00f, 0.50f, 1.00f, 1.50f, 1.50f },
            /* Chaos */   { 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f },
            /* Magic */   { 0.70f, 1.25f, 0.75f, 2.00f, 0.35f, 1.00f }
        };

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public static float GetAttackTypeMultiplier(AttackType attack, ArmorType armor)
        {
            int a = (int)attack;
            int d = (int)armor;
            if (a < 0 || a > 5 || d < 0 || d > 5) return 1.0f;
            return AttackArmorTable[a, d];
        }

        public static float CalculateArmorMitigation(int armor)
        {
            if (armor >= 0)
            {
                float reduction = (armor * WC3_ARMOR_FACTOR) / (1.0f + (armor * WC3_ARMOR_FACTOR));
                return Mathf.Clamp(reduction, 0.0f, MAX_ARMOR_REDUCTION);
            }
            else
            {
                // Negative armor amplification: 1 - (1 - factor)^|armor|
                float absArmor = Mathf.Abs(armor);
                float amplification = 1.0f - Mathf.Pow(1.0f - WC3_ARMOR_FACTOR, absArmor);
                return -Mathf.Clamp(amplification, 0.0f, MAX_NEGATIVE_ARMOR_VULNERABILITY);
            }
        }

        public static DamageResult ComputePhysicalDamage(
            int rawDamage,
            AttackType attackType,
            ArmorType armorType,
            int targetArmor,
            float critMultiplier = 1.0f,
            float evasionChance = 0.0f)
        {
            DamageResult result = new DamageResult
            {
                rawDamage = rawDamage,
                classification = DamageClassification.Physical,
                attackType = attackType,
                armorType = armorType,
                isCritical = false,
                isEvaded = false
            };

            // Evasion Check
            if (evasionChance > 0f)
            {
                float roll = UnityEngine.Random.value;
                if (roll < evasionChance)
                {
                    result.isEvaded = true;
                    result.mitigatedDamage = 0;
                    return result;
                }
            }

            // Critical Strike Check
            float damageAfterCrit = rawDamage;
            if (critMultiplier > 1.0f)
            {
                damageAfterCrit = rawDamage * critMultiplier;
                result.isCritical = true;
            }

            // Attack vs Armor Multiplier
            float typeMult = GetAttackTypeMultiplier(attackType, armorType);
            float damageAfterType = damageAfterCrit * typeMult;

            // Armor Reduction
            float armorMitigation = CalculateArmorMitigation(targetArmor);
            float finalDamage = damageAfterType * (1.0f - armorMitigation);

            result.mitigatedDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage));
            return result;
        }

        public static DamageResult ComputeMagicalDamage(
            int rawDamage,
            float targetSpellResistance = HERO_NATURAL_MAGIC_RESISTANCE)
        {
            DamageResult result = new DamageResult
            {
                rawDamage = rawDamage,
                classification = DamageClassification.Magical,
                attackType = AttackType.Magic,
                armorType = ArmorType.Hero,
                isCritical = false,
                isEvaded = false
            };

            float resistanceClamped = Mathf.Clamp(targetSpellResistance, 0.0f, 0.95f);
            float finalDamage = rawDamage * (1.0f - resistanceClamped);

            result.mitigatedDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage));
            return result;
        }

        public static DamageResult ComputePureDamage(int rawDamage)
        {
            return new DamageResult
            {
                rawDamage = rawDamage,
                mitigatedDamage = Mathf.Max(1, rawDamage),
                classification = DamageClassification.Pure,
                attackType = AttackType.Chaos,
                armorType = ArmorType.Hero,
                isCritical = false,
                isEvaded = false
            };
        }
    }
}
