/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using UnityEngine;
using FOC.HeroSystems;

namespace FOC.CombatSystems
{
    [System.Serializable]
    public class HeroStats : MonoBehaviour
    {
        [Header("Attribute Allocation")]
        [SerializeField] private AttributeType primaryAttribute = AttributeType.Agility;
        [SerializeField] private int baseStrength = 22;
        [SerializeField] private float strengthGainPerLevel = 2.2f;
        [SerializeField] private int baseAgility = 27;
        [SerializeField] private float agilityGainPerLevel = 3.0f;
        [SerializeField] private int baseIntelligence = 16;
        [SerializeField] private float intelligenceGainPerLevel = 1.4f;

        [Header("Item & Buff Stat Modifiers")]
        public int bonusStrength = 0;
        public int bonusAgility = 0;
        public int bonusIntelligence = 0;
        public int bonusDamage = 0;
        public int bonusArmor = 0;
        public float bonusAttackSpeedPercent = 0f;
        public float bonusMovementSpeed = 0f;
        public int bonusMaxHealth = 0;
        public int bonusMaxMana = 0;

        [Header("Base Combat Constants")]
        [SerializeField] private int baseHealth = 650;
        [SerializeField] private int baseMana = 280;
        [SerializeField] private int baseArmor = 4;
        [SerializeField] private int baseMinDamage = 48;
        [SerializeField] private int baseMaxDamage = 54;
        [SerializeField] private float baseAttackTime = 1.7f;
        [SerializeField] private float baseMovementSpeed = 320f;
        [SerializeField] private float attackRange = 140f;
        [SerializeField] private AttackType attackType = AttackType.Hero;
        [SerializeField] private ArmorType armorType = ArmorType.Hero;

        [Header("Vitals State")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExp = 0;
        [SerializeField] private float currentHealth;
        [SerializeField] private float currentMana;
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool isInvulnerable = false;
        [SerializeField] private bool isStunned = false;
        [SerializeField] private bool isSilenced = false;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnHeroDied;
        public event Action OnHeroRespawned;
        public event Action OnStatsRecalculated;

        public const int HP_PER_STR = 25;
        public const float HP_REGEN_PER_STR = 0.05f;
        public const float ARMOR_PER_AGI = 0.30f;
        public const float ATTACK_SPEED_PER_AGI = 0.02f;
        public const int MANA_PER_INT = 15;
        public const float MANA_REGEN_PER_INT = 0.05f;
        public const float MIN_MOVE_SPEED = 100f;
        public const float MAX_MOVE_SPEED = 522f; // Warcraft III absolute engine cap

        public AttributeType PrimaryAttribute => primaryAttribute;
        public AttackType UnitAttackType => attackType;
        public ArmorType UnitArmorType => armorType;
        public int CurrentLevel => currentLevel;
        public float CurrentHealth => currentHealth;
        public float CurrentMana => currentMana;
        public bool IsDead => isDead;
        public bool IsInvulnerable => isInvulnerable;
        public bool IsStunned => isStunned;
        public bool IsSilenced => isSilenced;
        public float AttackRange => attackRange;

        private void Awake()
        {
            InitializeStats();
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public void InitializeFromHeroData(HeroData data, int startingLevel = 1)
        {
            if (data == null) return;

            primaryAttribute = data.primaryAttribute;
            baseStrength = data.baseStrength;
            strengthGainPerLevel = data.strengthGainPerLevel;
            baseAgility = data.baseAgility;
            agilityGainPerLevel = data.agilityGainPerLevel;
            baseIntelligence = data.baseIntelligence;
            intelligenceGainPerLevel = data.intelligenceGainPerLevel;

            baseHealth = data.baseHealth;
            baseMana = data.baseMana;
            baseArmor = data.baseArmor;
            baseMinDamage = data.baseMinDamage;
            baseMaxDamage = data.baseMaxDamage;
            baseAttackTime = data.attackSpeed > 0 ? data.attackSpeed : 1.7f;
            baseMovementSpeed = data.movementSpeed > 0 ? data.movementSpeed : 320f;
            currentLevel = Mathf.Max(1, startingLevel);

            InitializeStats();
        }

        public void InitializeStats()
        {
            currentHealth = GetMaxHealth();
            currentMana = GetMaxMana();
            isDead = false;
            isInvulnerable = false;
            isStunned = false;
            isSilenced = false;

            NotifyVitalsChanged();
            OnStatsRecalculated?.Invoke();
        }

        public int GetTotalStrength()
        {
            int growth = Mathf.FloorToInt((currentLevel - 1) * strengthGainPerLevel);
            return Mathf.Max(1, baseStrength + growth + bonusStrength);
        }

        public int GetTotalAgility()
        {
            int growth = Mathf.FloorToInt((currentLevel - 1) * agilityGainPerLevel);
            return Mathf.Max(1, baseAgility + growth + bonusAgility);
        }

        public int GetTotalIntelligence()
        {
            int growth = Mathf.FloorToInt((currentLevel - 1) * intelligenceGainPerLevel);
            return Mathf.Max(1, baseIntelligence + growth + bonusIntelligence);
        }

        public int GetPrimaryAttributeValue()
        {
            switch (primaryAttribute)
            {
                case AttributeType.Strength: return GetTotalStrength();
                case AttributeType.Agility: return GetTotalAgility();
                case AttributeType.Intelligence: return GetTotalIntelligence();
                default: return GetTotalAgility();
            }
        }

        public int GetMaxHealth()
        {
            return baseHealth + (GetTotalStrength() * HP_PER_STR) + bonusMaxHealth;
        }

        public int GetMaxMana()
        {
            return baseMana + (GetTotalIntelligence() * MANA_PER_INT) + bonusMaxMana;
        }

        public float GetHealthRegenPerSecond()
        {
            return 1.5f + (GetTotalStrength() * HP_REGEN_PER_STR);
        }

        public float GetManaRegenPerSecond()
        {
            return 0.75f + (GetTotalIntelligence() * MANA_REGEN_PER_INT);
        }

        public int GetTotalArmor()
        {
            int agiArmor = Mathf.FloorToInt(GetTotalAgility() * ARMOR_PER_AGI);
            return baseArmor + agiArmor + bonusArmor;
        }

        public (int min, int max) GetDamageRange()
        {
            int primaryBonus = GetPrimaryAttributeValue();
            int min = baseMinDamage + primaryBonus + bonusDamage;
            int max = baseMaxDamage + primaryBonus + bonusDamage;
            return (Mathf.Max(1, min), Mathf.Max(1, max));
        }

        public float GetAttackInterval()
        {
            float totalAgi = GetTotalAgility();
            float ias = (totalAgi * ATTACK_SPEED_PER_AGI) + (bonusAttackSpeedPercent / 100f);
            float denominator = Mathf.Clamp(1.0f + ias, 0.20f, 5.0f);
            return baseAttackTime / denominator;
        }

        public float GetEffectiveMovementSpeed()
        {
            float totalSpeed = baseMovementSpeed + bonusMovementSpeed;
            return Mathf.Clamp(totalSpeed, MIN_MOVE_SPEED, MAX_MOVE_SPEED);
        }

        public void ApplyRegeneration(float deltaTime)
        {
            if (isDead) return;

            int maxHp = GetMaxHealth();
            if (currentHealth < maxHp)
            {
                currentHealth = Mathf.Min(maxHp, currentHealth + (GetHealthRegenPerSecond() * deltaTime));
                OnHealthChanged?.Invoke(currentHealth, maxHp);
            }

            int maxMp = GetMaxMana();
            if (currentMana < maxMp)
            {
                currentMana = Mathf.Min(maxMp, currentMana + (GetManaRegenPerSecond() * deltaTime));
                OnManaChanged?.Invoke(currentMana, maxMp);
            }
        }

        public bool ConsumeMana(int amount)
        {
            if (amount <= 0) return true;
            if (currentMana >= amount)
            {
                currentMana -= amount;
                OnManaChanged?.Invoke(currentMana, GetMaxMana());
                return true;
            }
            return false;
        }

        public void RestoreHealth(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(GetMaxHealth(), currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, GetMaxHealth());
        }

        public void RestoreMana(float amount)
        {
            if (isDead) return;
            currentMana = Mathf.Min(GetMaxMana(), currentMana + amount);
            OnManaChanged?.Invoke(currentMana, GetMaxMana());
        }

        public void TakeDirectDamage(int amount, HeroStats sourceAttacker)
        {
            if (isDead || isInvulnerable) return;

            currentHealth -= amount;
            NotifyVitalsChanged();

            if (currentHealth <= 0f)
            {
                Die(sourceAttacker);
            }
        }

        public void SetInvulnerable(bool invul)
        {
            isInvulnerable = invul;
        }

        public void SetStunned(bool stunned)
        {
            isStunned = stunned;
        }

        public void SetSilenced(bool silenced)
        {
            isSilenced = silenced;
        }

        public void LevelUp()
        {
            currentLevel++;
            currentHealth = GetMaxHealth();
            currentMana = GetMaxMana();
            NotifyVitalsChanged();
            OnStatsRecalculated?.Invoke();
        }

        private void Die(HeroStats killer)
        {
            isDead = true;
            currentHealth = 0f;
            NotifyVitalsChanged();
            OnHeroDied?.Invoke();
        }

        public void Respawn()
        {
            isDead = false;
            currentHealth = GetMaxHealth();
            currentMana = GetMaxMana();
            NotifyVitalsChanged();
            OnHeroRespawned?.Invoke();
        }

        private void NotifyVitalsChanged()
        {
            OnHealthChanged?.Invoke(currentHealth, GetMaxHealth());
            OnManaChanged?.Invoke(currentMana, GetMaxMana());
        }
    }
}
