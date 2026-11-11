/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using UnityEngine;

namespace FOC.CombatSystems
{
    [System.Serializable]
    public class ItemData : ScriptableObject
    {
        [Header("Item Identity")]
        public string itemId;
        public string itemName;
        public Sprite icon;
        [TextArea(2, 4)]
        public string description;
        public int cost;

        [Header("Stacking & Actives")]
        public bool isConsumable = false;
        public int maxCharges = 1;
        public float activeCooldown = 0f;
        public AbilityBase activeAbility;

        [Header("Passive Stat Modifiers")]
        public int bonusStrength = 0;
        public int bonusAgility = 0;
        public int bonusIntelligence = 0;
        public int bonusDamage = 0;
        public int bonusArmor = 0;
        public float bonusAttackSpeedPercent = 0f;
        public float bonusMovementSpeed = 0f;
        public int bonusMaxHealth = 0;
        public int bonusMaxMana = 0;
    }

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int charges;
        public float cooldownRemaining;

        public bool IsEmpty => item == null;
        public bool IsOnCooldown => cooldownRemaining > 0f;
    }

    /* === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT === */
    public class InventorySystem : MonoBehaviour
    {
        public const int INVENTORY_SLOT_COUNT = 6; // Authentic Warcraft III 6-slot inventory

        [Header("Slots [0..5]")]
        [SerializeField] private InventorySlot[] slots = new InventorySlot[INVENTORY_SLOT_COUNT];

        [Header("Host Hero References")]
        [SerializeField] private HeroStats heroStats;
        [SerializeField] private CombatController combatController;

        public event Action<int, InventorySlot> OnSlotUpdated;
        public event Action<int, bool> OnItemUsed;

        private void Awake()
        {
            if (heroStats == null) heroStats = GetComponent<HeroStats>();
            if (combatController == null) combatController = GetComponent<CombatController>();

            for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = new InventorySlot();
                }
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            {
                if (slots[i] != null && slots[i].cooldownRemaining > 0f)
                {
                    slots[i].cooldownRemaining -= dt;
                    if (slots[i].cooldownRemaining <= 0f)
                    {
                        slots[i].cooldownRemaining = 0f;
                        OnSlotUpdated?.Invoke(i, slots[i]);
                    }
                }
            }
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public bool AddItem(ItemData newItem, int initialCharges = 1)
        {
            if (newItem == null) return false;

            // 1. Check existing stackable slots
            if (newItem.maxCharges > 1)
            {
                for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
                {
                    if (!slots[i].IsEmpty && slots[i].item.itemId == newItem.itemId)
                    {
                        if (slots[i].charges < newItem.maxCharges)
                        {
                            slots[i].charges = Mathf.Min(newItem.maxCharges, slots[i].charges + initialCharges);
                            OnSlotUpdated?.Invoke(i, slots[i]);
                            RecalculateBonuses();
                            return true;
                        }
                    }
                }
            }

            // 2. Find first empty slot
            for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].item = newItem;
                    slots[i].charges = Mathf.Clamp(initialCharges, 1, newItem.maxCharges);
                    slots[i].cooldownRemaining = 0f;
                    OnSlotUpdated?.Invoke(i, slots[i]);
                    RecalculateBonuses();
                    return true;
                }
            }

            // Inventory full
            return false;
        }

        public bool RemoveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= INVENTORY_SLOT_COUNT) return false;
            if (slots[slotIndex].IsEmpty) return false;

            slots[slotIndex].item = null;
            slots[slotIndex].charges = 0;
            slots[slotIndex].cooldownRemaining = 0f;

            OnSlotUpdated?.Invoke(slotIndex, slots[slotIndex]);
            RecalculateBonuses();
            return true;
        }

        public bool SwapSlots(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= INVENTORY_SLOT_COUNT ||
                toSlot < 0 || toSlot >= INVENTORY_SLOT_COUNT ||
                fromSlot == toSlot)
                return false;

            InventorySlot temp = slots[fromSlot];
            slots[fromSlot] = slots[toSlot];
            slots[toSlot] = temp;

            OnSlotUpdated?.Invoke(fromSlot, slots[fromSlot]);
            OnSlotUpdated?.Invoke(toSlot, slots[toSlot]);
            return true;
        }

        public bool UseItem(int slotIndex, Vector3 targetPoint = default, CombatController targetUnit = null)
        {
            if (slotIndex < 0 || slotIndex >= INVENTORY_SLOT_COUNT) return false;
            InventorySlot slot = slots[slotIndex];

            if (slot.IsEmpty || slot.IsOnCooldown || heroStats.IsDead || heroStats.IsSilenced)
            {
                OnItemUsed?.Invoke(slotIndex, false);
                return false;
            }

            if (slot.item.activeAbility != null)
            {
                bool success = slot.item.activeAbility.TriggerCast(combatController, targetPoint, targetUnit);
                if (success)
                {
                    slot.cooldownRemaining = slot.item.activeCooldown;
                    if (slot.item.isConsumable)
                    {
                        slot.charges--;
                        if (slot.charges <= 0)
                        {
                            RemoveItem(slotIndex);
                            return true;
                        }
                    }
                    OnSlotUpdated?.Invoke(slotIndex, slot);
                    OnItemUsed?.Invoke(slotIndex, true);
                    return true;
                }
            }

            OnItemUsed?.Invoke(slotIndex, false);
            return false;
        }

        public InventorySlot GetSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= INVENTORY_SLOT_COUNT) return null;
            return slots[slotIndex];
        }

        public void RecalculateBonuses()
        {
            if (heroStats == null) return;

            int sumStr = 0;
            int sumAgi = 0;
            int sumInt = 0;
            int sumDmg = 0;
            int sumArm = 0;
            float sumAs = 0f;
            float sumMs = 0f;
            int sumHp = 0;
            int sumMp = 0;

            for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    ItemData itm = slots[i].item;
                    sumStr += itm.bonusStrength;
                    sumAgi += itm.bonusAgility;
                    sumInt += itm.bonusIntelligence;
                    sumDmg += itm.bonusDamage;
                    sumArm += itm.bonusArmor;
                    sumAs += itm.bonusAttackSpeedPercent;
                    sumMs += itm.bonusMovementSpeed;
                    sumHp += itm.bonusMaxHealth;
                    sumMp += itm.bonusMaxMana;
                }
            }

            heroStats.bonusStrength = sumStr;
            heroStats.bonusAgility = sumAgi;
            heroStats.bonusIntelligence = sumInt;
            heroStats.bonusDamage = sumDmg;
            heroStats.bonusArmor = sumArm;
            heroStats.bonusAttackSpeedPercent = sumAs;
            heroStats.bonusMovementSpeed = sumMs;
            heroStats.bonusMaxHealth = sumHp;
            heroStats.bonusMaxMana = sumMp;

            heroStats.InitializeStats();
        }
    }
}
