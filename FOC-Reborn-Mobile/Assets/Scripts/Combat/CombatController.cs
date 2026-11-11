/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections;
using UnityEngine;

namespace FOC.CombatSystems
{
    public enum CombatState
    {
        Idle,
        Moving,
        PursuingTarget,
        AttackWindup,
        AttackBackswing,
        CastingAbility,
        Stunned,
        Dead
    }

    /* === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT === */
    [RequireComponent(typeof(HeroStats))]
    public class CombatController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private HeroStats heroStats;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;

        [Header("Targeting & Senses")]
        [SerializeField] private float acquisitionRadius = 600f; // WC3 600 default acquisition range
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private CombatController currentTarget;

        [Header("Animation Hash IDs")]
        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        private static readonly int DeathHash = Animator.StringToHash("Death");
        private static readonly int SpellHash = Animator.StringToHash("Spell");

        [Header("Combat Timings & State")]
        [SerializeField] private CombatState currentState = CombatState.Idle;
        private float _attackCooldownTimer = 0f;
        private float _attackWindupTimer = 0f;
        private float _attackBackswingTimer = 0f;
        private const float ATTACK_WINDUP_RATIO = 0.30f;   // 30% of interval is windup
        private const float ATTACK_BACKSWING_RATIO = 0.35f; // 35% of interval is backswing

        [Header("Physics & Grounding")]
        [SerializeField] private float gravity = 20.0f;
        private Vector3 _velocity = Vector3.zero;

        public event Action<CombatState> OnCombatStateChanged;
        public event Action<CombatController, int, bool> OnDamageDealt; // (target, amount, isCrit)
        public event Action<CombatController> OnTargetChanged;

        public HeroStats Stats => heroStats;
        public CombatState State => currentState;
        public CombatController CurrentTarget => currentTarget;

        private void Awake()
        {
            if (heroStats == null) heroStats = GetComponent<HeroStats>();
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            heroStats.OnHeroDied += HandleDeath;
            heroStats.OnHeroRespawned += HandleRespawn;
        }

        private void OnDestroy()
        {
            if (heroStats != null)
            {
                heroStats.OnHeroDied -= HandleDeath;
                heroStats.OnHeroRespawned -= HandleRespawn;
            }
        }

        private void Update()
        {
            if (heroStats.IsDead) return;

            // Handle Regeneration & Timers
            heroStats.ApplyRegeneration(Time.deltaTime);
            UpdateTimers(Time.deltaTime);

            if (heroStats.IsStunned)
            {
                SetState(CombatState.Stunned);
                return;
            }

            // Execute Combat State Machine
            switch (currentState)
            {
                case CombatState.PursuingTarget:
                    ProcessTargetPursuit();
                    break;
                case CombatState.AttackWindup:
                    ProcessAttackWindup();
                    break;
                case CombatState.AttackBackswing:
                    ProcessAttackBackswing();
                    break;
                case CombatState.Idle:
                    AutoAcquireTargetIfNone();
                    break;
            }
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public void IssueMoveCommand(Vector2 moveInput)
        {
            if (heroStats.IsDead || heroStats.IsStunned) return;

            // Manual move cancels current attack target
            if (moveInput.sqrMagnitude > 0.01f)
            {
                currentTarget = null;
                SetState(CombatState.Moving);

                float speed = heroStats.GetEffectiveMovementSpeed() * 0.015f; // scaled for Unity world units
                Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);

                _velocity.x = moveDir.x * speed;
                _velocity.z = moveDir.z * speed;
            }
            else
            {
                _velocity.x = 0f;
                _velocity.z = 0f;
                if (currentState == CombatState.Moving)
                {
                    SetState(CombatState.Idle);
                }
            }

            // Apply Gravity
            if (characterController != null)
            {
                if (characterController.isGrounded)
                {
                    _velocity.y = -1.0f;
                }
                else
                {
                    _velocity.y -= gravity * Time.deltaTime;
                }
                characterController.Move(_velocity * Time.deltaTime);
            }

            // Update Animator
            if (animator != null)
            {
                animator.SetFloat(MoveSpeedHash, moveInput.magnitude);
            }
        }

        public void IssueAttackTargetCommand(CombatController target)
        {
            if (heroStats.IsDead || target == null || target == this || target.Stats.IsDead) return;

            currentTarget = target;
            OnTargetChanged?.Invoke(currentTarget);
            SetState(CombatState.PursuingTarget);
        }

        public void StopAction()
        {
            currentTarget = null;
            SetState(CombatState.Idle);
            if (animator != null)
            {
                animator.SetFloat(MoveSpeedHash, 0f);
            }
        }

        private void ProcessTargetPursuit()
        {
            if (currentTarget == null || currentTarget.Stats.IsDead)
            {
                currentTarget = null;
                SetState(CombatState.Idle);
                return;
            }

            float distToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            float effectiveRange = (heroStats.AttackRange * 0.015f) + 0.5f; // Scale to Unity units

            if (distToTarget <= effectiveRange)
            {
                // In range: face target and begin attack cycle if cooldown ready
                Vector3 lookDir = (currentTarget.transform.position - transform.position);
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }

                if (_attackCooldownTimer <= 0f)
                {
                    StartAttackCycle();
                }
                else
                {
                    if (animator != null) animator.SetFloat(MoveSpeedHash, 0f);
                }
            }
            else
            {
                // Pursue target
                Vector3 moveDir = (currentTarget.transform.position - transform.position).normalized;
                moveDir.y = 0;

                float speed = heroStats.GetEffectiveMovementSpeed() * 0.015f;
                _velocity.x = moveDir.x * speed;
                _velocity.z = moveDir.z * speed;

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 12f);

                if (characterController != null)
                {
                    characterController.Move(_velocity * Time.deltaTime);
                }

                if (animator != null)
                {
                    animator.SetFloat(MoveSpeedHash, 1.0f);
                }
            }
        }

        private void StartAttackCycle()
        {
            float interval = heroStats.GetAttackInterval();
            _attackWindupTimer = interval * ATTACK_WINDUP_RATIO;
            _attackBackswingTimer = interval * ATTACK_BACKSWING_RATIO;
            _attackCooldownTimer = interval;

            SetState(CombatState.AttackWindup);

            if (animator != null)
            {
                animator.SetFloat(MoveSpeedHash, 0f);
                if (UnityEngine.Random.value > 0.5f)
                    animator.SetTrigger(Attack1Hash);
                else
                    animator.SetTrigger(Attack2Hash);
            }
        }

        private void ProcessAttackWindup()
        {
            if (currentTarget == null || currentTarget.Stats.IsDead)
            {
                SetState(CombatState.Idle);
                return;
            }

            if (_attackWindupTimer <= 0f)
            {
                // Execute Attack Impact Point
                ExecuteAttackHit();
                SetState(CombatState.AttackBackswing);
            }
        }

        private void ProcessAttackBackswing()
        {
            if (_attackBackswingTimer <= 0f)
            {
                if (currentTarget != null && !currentTarget.Stats.IsDead)
                {
                    SetState(CombatState.PursuingTarget);
                }
                else
                {
                    SetState(CombatState.Idle);
                }
            }
        }

        private void ExecuteAttackHit()
        {
            if (currentTarget == null || currentTarget.Stats.IsDead) return;

            var (minDmg, maxDmg) = heroStats.GetDamageRange();
            int rolledDamage = UnityEngine.Random.Range(minDmg, maxDmg + 1);

            DamageResult result = DamageCalculation.ComputePhysicalDamage(
                rolledDamage,
                heroStats.UnitAttackType,
                currentTarget.Stats.UnitArmorType,
                currentTarget.Stats.GetTotalArmor(),
                critMultiplier: 1.0f,
                evasionChance: 0.0f
            );

            if (!result.isEvaded)
            {
                currentTarget.Stats.TakeDirectDamage(result.mitigatedDamage, heroStats);
                OnDamageDealt?.Invoke(currentTarget, result.mitigatedDamage, result.isCritical);
            }
        }

        private void AutoAcquireTargetIfNone()
        {
            if (currentTarget != null) return;

            float searchRadius = acquisitionRadius * 0.015f;
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, enemyLayerMask);

            CombatController closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                CombatController candidate = hits[i].GetComponent<CombatController>();
                if (candidate != null && candidate != this && !candidate.Stats.IsDead)
                {
                    float dist = Vector3.Distance(transform.position, candidate.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = candidate;
                    }
                }
            }

            if (closest != null)
            {
                currentTarget = closest;
                OnTargetChanged?.Invoke(currentTarget);
                SetState(CombatState.PursuingTarget);
            }
        }

        private void UpdateTimers(float dt)
        {
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= dt;
            if (_attackWindupTimer > 0f) _attackWindupTimer -= dt;
            if (_attackBackswingTimer > 0f) _attackBackswingTimer -= dt;
        }

        private void SetState(CombatState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnCombatStateChanged?.Invoke(currentState);
        }

        private void HandleDeath()
        {
            SetState(CombatState.Dead);
            currentTarget = null;
            if (animator != null)
            {
                animator.SetTrigger(DeathHash);
            }

            // Start respawn sequence: 5 + (level * 2.5) seconds
            StartCoroutine(RespawnRoutine());
        }

        private void HandleRespawn()
        {
            SetState(CombatState.Idle);
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        private IEnumerator RespawnRoutine()
        {
            float delay = 5.0f + (heroStats.CurrentLevel * 2.5f);
            yield return new WaitForSeconds(delay);
            heroStats.Respawn();
        }
    }
}
