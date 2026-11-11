/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOC.CombatSystems
{
    public enum AbilityCastType
    {
        InstantNoTarget,
        PointTarget,
        UnitTarget,
        Passive
    }

    [System.Serializable]
    public abstract class AbilityBase : ScriptableObject
    {
        [Header("Ability Identity")]
        public string abilityId;
        public string abilityName;
        public string keySlot; // "Q", "W", "E", "R", "D"
        public Sprite icon;
        [TextArea(2, 4)]
        public string description;
        public AbilityCastType castType = AbilityCastType.InstantNoTarget;

        [Header("Progression & Tuning")]
        public int currentLevel = 1;
        public int maxLevel = 5;
        public int[] manaCosts = new int[] { 65, 80, 95, 110, 125 };
        public float[] cooldownDurations = new float[] { 7.0f, 7.0f, 7.0f, 7.0f, 7.0f };
        public float castRange = 600f;

        [NonSerialized] protected float cooldownTimer = 0f;

        public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
        public bool IsOnCooldown => cooldownTimer > 0f;

        public virtual int GetCurrentManaCost()
        {
            int idx = Mathf.Clamp(currentLevel - 1, 0, manaCosts.Length - 1);
            return manaCosts[idx];
        }

        public virtual float GetCurrentCooldownDuration()
        {
            int idx = Mathf.Clamp(currentLevel - 1, 0, cooldownDurations.Length - 1);
            return cooldownDurations[idx];
        }

        public virtual void UpdateCooldown(float deltaTime)
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= deltaTime;
            }
        }

        public virtual bool CanCast(HeroStats caster)
        {
            if (caster == null || caster.IsDead || caster.IsSilenced || caster.IsStunned)
                return false;

            if (IsOnCooldown)
                return false;

            if (caster.CurrentMana < GetCurrentManaCost())
                return false;

            return true;
        }

        public virtual bool TriggerCast(CombatController caster, Vector3 targetPoint, CombatController targetUnit)
        {
            if (!CanCast(caster.Stats)) return false;

            // Deduct mana
            caster.Stats.ConsumeMana(GetCurrentManaCost());
            cooldownTimer = GetCurrentCooldownDuration();

            ExecuteEffect(caster, targetPoint, targetUnit);
            return true;
        }

        public abstract void ExecuteEffect(CombatController caster, Vector3 targetPoint, CombatController targetUnit);

        public virtual void LevelUp()
        {
            if (currentLevel < maxLevel)
            {
                currentLevel++;
            }
        }

        public virtual void ResetCooldown()
        {
            cooldownTimer = 0f;
        }
    }

    /* === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT === */
    public class AbilitySystem : MonoBehaviour
    {
        [Header("Slots Allocation")]
        [SerializeField] private AbilityBase abilityQ;
        [SerializeField] private AbilityBase abilityW;
        [SerializeField] private AbilityBase abilityE;
        [SerializeField] private AbilityBase abilityR;
        [SerializeField] private AbilityBase abilityD;

        [Header("Host Combat Controller")]
        [SerializeField] private CombatController combatController;

        public event Action<string, float, float> OnCooldownUpdated; // (slot, current, max)
        public event Action<string, bool> OnCastResolved; // (slot, success)

        private void Awake()
        {
            if (combatController == null)
            {
                combatController = GetComponent<CombatController>();
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            TickAbility(abilityQ, "Q", dt);
            TickAbility(abilityW, "W", dt);
            TickAbility(abilityE, "E", dt);
            TickAbility(abilityR, "R", dt);
            TickAbility(abilityD, "D", dt);
        }

        private void TickAbility(AbilityBase ability, string slot, float dt)
        {
            if (ability != null)
            {
                ability.UpdateCooldown(dt);
                if (ability.IsOnCooldown)
                {
                    OnCooldownUpdated?.Invoke(slot, ability.CooldownRemaining, ability.GetCurrentCooldownDuration());
                }
            }
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public bool CastSlot(string slot, Vector3 targetPoint = default, CombatController targetUnit = null)
        {
            AbilityBase targetAbility = GetAbilityInSlot(slot);
            if (targetAbility == null)
            {
                OnCastResolved?.Invoke(slot, false);
                return false;
            }

            bool success = targetAbility.TriggerCast(combatController, targetPoint, targetUnit);
            OnCastResolved?.Invoke(slot, success);
            return success;
        }

        public AbilityBase GetAbilityInSlot(string slot)
        {
            switch (slot.ToUpperInvariant())
            {
                case "Q": return abilityQ;
                case "W": return abilityW;
                case "E": return abilityE;
                case "R": return abilityR;
                case "D": return abilityD;
                default: return null;
            }
        }

        public void AssignAbility(string slot, AbilityBase ability)
        {
            switch (slot.ToUpperInvariant())
            {
                case "Q": abilityQ = ability; break;
                case "W": abilityW = ability; break;
                case "E": abilityE = ability; break;
                case "R": abilityR = ability; break;
                case "D": abilityD = ability; break;
            }
        }
    }
}
