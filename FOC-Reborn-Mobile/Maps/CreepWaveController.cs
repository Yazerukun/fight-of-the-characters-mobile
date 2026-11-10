/* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FOC.Game;
using FOC.HeroSystems;

namespace FOC.Maps
{
    public class CreepWaveController : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float waveIntervalSeconds = 30.0f;
        [SerializeField] private Transform[] team1SpawnPoints;
        [SerializeField] private Transform[] team2SpawnPoints;
        [SerializeField] private Transform[] centerWaypoints;

        [Header("Creep Prefabs (Extracted Original Units)")]
        [SerializeField] private GameObject meleeCreepPrefab; // e.g. n000 / Footman type
        [SerializeField] private GameObject rangedCreepPrefab; // e.g. n001 / Archer type

        private float _waveTimer = 0f;
        private int _waveCount = 0;

        private void Start()
        {
            _waveTimer = waveIntervalSeconds;
            if (FOCGameManager.Instance != null)
            {
                FOCGameManager.Instance.OnCreepWaveSpawn += TriggerWaveSpawn;
            }
        }

        private void OnDestroy()
        {
            if (FOCGameManager.Instance != null)
            {
                FOCGameManager.Instance.OnCreepWaveSpawn -= TriggerWaveSpawn;
            }
        }

        private void Update()
        {
            _waveTimer -= Time.deltaTime;
            if (_waveTimer <= 0f)
            {
                _waveTimer = waveIntervalSeconds;
                TriggerWaveSpawn();
            }
        }

        public void TriggerWaveSpawn()
        {
            _waveCount++;
            SpawnWaveForTeam(TeamType.Sentinel_Team1, team1SpawnPoints);
            SpawnWaveForTeam(TeamType.Scourge_Team2, team2SpawnPoints);
        }

        private void SpawnWaveForTeam(TeamType team, Transform[] spawnPoints)
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform sp = spawnPoints[i];

                // Spawn 3 Melee Creeps
                for (int m = 0; m < 3; m++)
                {
                    Vector3 offset = new Vector3((m - 1) * 1.5f, 0f, 0f);
                    if (meleeCreepPrefab != null)
                    {
                        GameObject creepObj = Instantiate(meleeCreepPrefab, sp.position + offset, sp.rotation);
                        var creepAi = creepObj.GetComponent<CreepAIInstance>();
                        if (creepAi != null)
                        {
                            creepAi.Initialize(team, centerWaypoints);
                        }
                    }
                }

                // Spawn 1 Ranged Creep
                if (rangedCreepPrefab != null)
                {
                    Vector3 offset = new Vector3(0f, 0f, -2.5f);
                    GameObject rangedObj = Instantiate(rangedCreepPrefab, sp.position + offset, sp.rotation);
                    var creepAi = rangedObj.GetComponent<CreepAIInstance>();
                    if (creepAi != null)
                    {
                        creepAi.Initialize(team, centerWaypoints);
                    }
                }
            }
        }
    }

    public class CreepAIInstance : MonoBehaviour
    {
        public TeamType team;
        public float maxHealth = 350f;
        public float currentHealth = 350f;
        public float attackDamage = 22f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1.4f;
        public float moveSpeed = 3.2f;
        public float aggroRadius = 8.0f;

        private Transform[] _waypoints;
        private int _currentWaypointIndex = 0;
        private HeroInstance _targetHero;
        private CreepAIInstance _targetCreep;
        private float _attackTimer = 0f;
        private CharacterController _controller;

        public void Initialize(TeamType assignedTeam, Transform[] waypoints)
        {
            team = assignedTeam;
            _waypoints = waypoints;
            currentHealth = maxHealth;
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (currentHealth <= 0f) return;

            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

            FindNearestEnemyTarget();

            if (_targetHero != null)
            {
                EngageTarget(_targetHero.transform.position, () => {
                    _targetHero.TakePhysicalDamage(attackDamage, null);
                });
            }
            else if (_targetCreep != null)
            {
                EngageTarget(_targetCreep.transform.position, () => {
                    _targetCreep.TakeDamage(attackDamage);
                });
            }
            else
            {
                MarchWaypoints();
            }
        }

        private void EngageTarget(Vector3 targetPos, System.Action onHit)
        {
            Vector3 diff = targetPos - transform.position;
            diff.y = 0;
            float dist = diff.magnitude;

            if (dist > attackRange)
            {
                // Move towards target
                Vector3 move = diff.normalized * moveSpeed * Time.deltaTime;
                if (_controller != null) _controller.Move(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * 8f);
            }
            else
            {
                // In range: Attack
                if (_attackTimer <= 0f)
                {
                    _attackTimer = attackCooldown;
                    onHit?.Invoke();
                }
            }
        }

        private void MarchWaypoints()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;
            if (_currentWaypointIndex >= _waypoints.Length) return;

            Vector3 target = _waypoints[_currentWaypointIndex].position;
            Vector3 diff = target - transform.position;
            diff.y = 0;

            if (diff.magnitude < 2.0f)
            {
                _currentWaypointIndex++;
            }
            else
            {
                Vector3 move = diff.normalized * moveSpeed * Time.deltaTime;
                if (_controller != null) _controller.Move(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(diff), Time.deltaTime * 6f);
            }
        }

        private void FindNearestEnemyTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, aggroRadius);
            float nearestDist = aggroRadius;

            _targetHero = null;
            _targetCreep = null;

            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i].GetComponent<HeroInstance>();
                if (h != null && !h.isDead)
                {
                    float d = Vector3.Distance(transform.position, h.transform.position);
                    if (d < nearestDist)
                    {
                        nearestDist = d;
                        _targetHero = h;
                    }
                }
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
