/* === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT === */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FOC.Game;

namespace FOC.HeroSystems
{
    public class HeroInstance : MonoBehaviour
    {
        [Header("Hero Data Profile")]
        [SerializeField] private HeroData heroData;
        [SerializeField] private TeamType team = TeamType.Sentinel_Team1;

        [Header("Vitals & Level State")]
        public int currentLevel = 1;
        public int currentExperience = 0;
        public float currentHealth;
        public float currentMana;
        public bool isDead { get; private set; } = false;

        [Header("Movement & Physics")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float gravity = 20.0f;
        private Vector3 _moveDirection = Vector3.zero;

        [Header("Original Animation Controller")]
        [SerializeField] private Animator animator;
        private static readonly int AnimSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int AnimAttack1Hash = Animator.StringToHash("Attack1");
        private static readonly int AnimAttack2Hash = Animator.StringToHash("Attack2");
        private static readonly int AnimSpellHash = Animator.StringToHash("Spell");
        private static readonly int AnimDeathHash = Animator.StringToHash("Death");
        private static readonly int AnimMorphHash = Animator.StringToHash("Morph");

        [Header("Combat Cooldowns & Attack Timing")]
        private float _attackCooldownTimer = 0f;
        private float _invulnerabilityTimer = 0f;
        private bool _isSilenced = false;
        private bool _isStunned = false;

        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action OnHeroDied;

        private void Awake()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            InitializeHeroStats();
        }

        public void InitializeHeroStats()
        {
            if (heroData == null) return;

            currentHealth = heroData.CalculateMaxHealth(currentLevel);
            currentMana = heroData.CalculateMaxMana(currentLevel);
            isDead = false;

            OnHealthChanged?.Invoke(currentHealth, heroData.CalculateMaxHealth(currentLevel));
            OnManaChanged?.Invoke(currentMana, heroData.CalculateMaxMana(currentLevel));
        }

        private void Update()
        {
            if (isDead) return;

            UpdateRegeneration();
            UpdateTimers();
        }

        public void ApplyMoveInput(Vector2 inputVector)
        {
            if (isDead || _isStunned) return;

            float speed = heroData != null ? heroData.movementSpeed * 0.015f : 4.5f;
            Vector3 move = new Vector3(inputVector.x, 0f, inputVector.y);

            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);
                _moveDirection = move * speed;
            }
            else
            {
                _moveDirection = Vector3.zero;
            }

            // Apply gravity
            if (characterController != null)
            {
                if (!characterController.isGrounded)
                {
                    _moveDirection.y -= gravity * Time.deltaTime;
                }
                characterController.Move(_moveDirection * Time.deltaTime);
            }

            // Update Animator
            if (animator != null)
            {
                animator.SetFloat(AnimSpeedHash, inputVector.magnitude);
            }
        }

        public void ExecuteBasicAttack(HeroInstance targetEnemy)
        {
            if (isDead || _isStunned || _attackCooldownTimer > 0f) return;

            float atkInterval = heroData != null ? heroData.attackSpeed : 1.7f;
            _attackCooldownTimer = atkInterval;

            if (animator != null)
            {
                if (UnityEngine.Random.value > 0.5f)
                    animator.SetTrigger(AnimAttack1Hash);
                else
                    animator.SetTrigger(AnimAttack2Hash);
            }

            if (targetEnemy != null)
            {
                int avgDmg = heroData != null ? heroData.CalculateAverageDamage(currentLevel) : 50;
                targetEnemy.TakePhysicalDamage(avgDmg, this);
            }
        }

        public void TakePhysicalDamage(float rawDamage, HeroInstance sourceAttacker)
        {
            if (isDead || _invulnerabilityTimer > 0f) return;

            // Warcraft III Armor Mitigation Formula
            int armor = heroData != null ? heroData.baseArmor : 4;
            float armorFactor = 0.06f;
            float reduction = (armor * armorFactor) / (1f + (armorFactor * Mathf.Abs(armor)));
            reduction = Mathf.Clamp(reduction, -0.71f, 0.90f);

            float netDamage = rawDamage * (1f - reduction);
            currentHealth -= netDamage;

            float maxHp = heroData != null ? heroData.CalculateMaxHealth(currentLevel) : 650;
            OnHealthChanged?.Invoke(currentHealth, maxHp);

            if (currentHealth <= 0f)
            {
                Die(sourceAttacker);
            }
        }

        public void TakeMagicalDamage(float rawDamage, HeroInstance sourceAttacker)
        {
            if (isDead || _invulnerabilityTimer > 0f) return;

            // Base 25% Hero Magic Resistance per WC3
            float netDamage = rawDamage * 0.75f;
            currentHealth -= netDamage;

            float maxHp = heroData != null ? heroData.CalculateMaxHealth(currentLevel) : 650;
            OnHealthChanged?.Invoke(currentHealth, maxHp);

            if (currentHealth <= 0f)
            {
                Die(sourceAttacker);
            }
        }

        private void Die(HeroInstance killer)
        {
            isDead = true;
            currentHealth = 0f;

            if (animator != null)
            {
                animator.SetTrigger(AnimDeathHash);
            }

            OnHeroDied?.Invoke();

            if (killer != null && FOCGameManager.Instance != null)
            {
                FOCGameManager.Instance.RegisterKill(killer.team);
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            // WC3 hero respawn time formula: 5 + (level * 3) seconds
            float respawnDelay = 5f + (currentLevel * 2.5f);
            yield return new WaitForSeconds(respawnDelay);

            InitializeHeroStats();
        }

        private void UpdateRegeneration()
        {
            float maxHp = heroData != null ? heroData.CalculateMaxHealth(currentLevel) : 650;
            float maxMp = heroData != null ? heroData.CalculateMaxMana(currentLevel) : 260;

            if (currentHealth < maxHp)
            {
                currentHealth = Mathf.Min(maxHp, currentHealth + (heroData.baseHealthRegen * Time.deltaTime));
                OnHealthChanged?.Invoke(currentHealth, maxHp);
            }

            if (currentMana < maxMp)
            {
                currentMana = Mathf.Min(maxMp, currentMana + (heroData.baseManaRegen * Time.deltaTime));
                OnManaChanged?.Invoke(currentMana, maxMp);
            }
        }

        private void UpdateTimers()
        {
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;
            if (_invulnerabilityTimer > 0f) _invulnerabilityTimer -= Time.deltaTime;
        }
    }
}
