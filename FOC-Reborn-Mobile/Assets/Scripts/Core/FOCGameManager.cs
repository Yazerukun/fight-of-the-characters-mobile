/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FOC.HeroSystems;
using FOC.UI_UX;

namespace FOC.Game
{
    public enum MatchState
    {
        Initializing,
        LoadingScreen,
        HeroSelection,
        PreMatchWait,
        ActiveBattle,
        DuelPhase,
        MatchFinished
    }

    public enum TeamType
    {
        Sentinel_Team1,
        Scourge_Team2
    }

    public class FOCGameManager : MonoBehaviour
    {
        public static FOCGameManager Instance { get; private set; }

        [Header("Match Settings")]
        [SerializeField] private int killTarget = 30; // 10, 20, 30, or unlimited (-1)
        [SerializeField] private float duelIntervalSeconds = 180f; // 3 minutes per WC3
        [SerializeField] private float creepWaveInterval = 30f; // 30s per wave

        [Header("Team Scores")]
        private int _team1Kills = 0;
        private int _team2Kills = 0;
        public int Team1Kills => _team1Kills;
        public int Team2Kills => _team2Kills;

        [Header("Runtime State")]
        public MatchState CurrentState { get; private set; } = MatchState.Initializing;
        private float _matchTimer = 0f;
        private float _nextDuelTimer = 0f;
        private float _nextCreepWaveTimer = 0f;

        public event Action<int, int> OnScoreChanged;
        public event Action<TeamType> OnMatchEnded;
        public event Action OnDuelTriggered;
        public event Action OnCreepWaveSpawn;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeMatch();
        }

        public void InitializeMatch()
        {
            _team1Kills = 0;
            _team2Kills = 0;
            _matchTimer = 0f;
            _nextDuelTimer = duelIntervalSeconds;
            _nextCreepWaveTimer = creepWaveInterval;
            CurrentState = MatchState.ActiveBattle;
        }

        private void FixedUpdate()
        {
            if (CurrentState != MatchState.ActiveBattle && CurrentState != MatchState.DuelPhase)
                return;

            _matchTimer += Time.fixedDeltaTime;

            // 1. Creep Wave Periodic Spawning
            _nextCreepWaveTimer -= Time.fixedDeltaTime;
            if (_nextCreepWaveTimer <= 0f)
            {
                _nextCreepWaveTimer = creepWaveInterval;
                OnCreepWaveSpawn?.Invoke();
            }

            // 2. Periodic Duel Ring Arena Trigger
            _nextDuelTimer -= Time.fixedDeltaTime;
            if (_nextDuelTimer <= 0f)
            {
                _nextDuelTimer = duelIntervalSeconds;
                TriggerDuelPhase();
            }
        }

        public void RegisterKill(TeamType killerTeam)
        {
            if (CurrentState == MatchState.MatchFinished) return;

            if (killerTeam == TeamType.Sentinel_Team1)
            {
                _team1Kills++;
            }
            else
            {
                _team2Kills++;
            }

            OnScoreChanged?.Invoke(_team1Kills, _team2Kills);

            // Check Victory Condition
            if (killTarget > 0)
            {
                if (_team1Kills >= killTarget)
                {
                    EndMatch(TeamType.Sentinel_Team1);
                }
                else if (_team2Kills >= killTarget)
                {
                    EndMatch(TeamType.Scourge_Team2);
                }
            }
        }

        private void TriggerDuelPhase()
        {
            CurrentState = MatchState.DuelPhase;
            OnDuelTriggered?.Invoke();
        }

        public void ConcludeDuelPhase()
        {
            CurrentState = MatchState.ActiveBattle;
        }

        private void EndMatch(TeamType winner)
        {
            CurrentState = MatchState.MatchFinished;
            OnMatchEnded?.Invoke(winner);
        }
    }
}
