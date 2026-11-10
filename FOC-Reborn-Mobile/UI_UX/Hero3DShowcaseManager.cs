/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FOC.HeroSystems;

namespace FOC.UI_UX
{
    [System.Serializable]
    public class HeroPrefabMapping
    {
        public string heroId;
        public GameObject heroPrefab;
        public ParticleSystem heroAuthenticAura;
        public AudioClip selectVoiceClip;
    }

    public class Hero3DShowcaseManager : MonoBehaviour
    {
        [Header("Showcase Pedestal & Stage")]
        [SerializeField] private Transform characterSpawnPoint;
        [SerializeField] private Transform turntablePlatform;
        [SerializeField] private Light pedestalSpotlight;
        [SerializeField] private ParticleSystem floorGlyphParticles;

        [Header("Hero Prefabs Registry")]
        [SerializeField] private List<HeroPrefabMapping> heroPrefabs = new List<HeroPrefabMapping>();
        [SerializeField] private GameObject defaultFallbackHeroPrefab;

        [Header("Turntable & Touch Controls")]
        [SerializeField] private float touchRotationSpeed = 0.35f;
        [SerializeField] private float autoRotationSpeed = 12f;
        [SerializeField] private float inertiaDamping = 0.92f;
        [SerializeField] private float autoRotateIdleDelay = 2.5f;

        /* === 02. SECURITY & STATE LOCKS === */
        private GameObject _activeHeroInstance;
        private ParticleSystem _activeAuraInstance;
        private Animator _currentAnimator;
        private float _currentAngularVelocity = 0f;
        private float _lastInteractionTime = 0f;
        private bool _isDragging = false;
        private Vector2 _previousTouchPos;

        private static readonly int IdleHash = Animator.StringToHash("Idle");
        private static readonly int StandHash = Animator.StringToHash("Stand");
        private static readonly int SkillCastHash = Animator.StringToHash("SkillCast");
        private static readonly int SkillIndexHash = Animator.StringToHash("SkillIndex");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        /* === 03. PRECISION MATH & ROTATION HELPERS === */
        private void Awake()
        {
            _lastInteractionTime = Time.time;
        }

        private void Update()
        {
            HandleTouchInput();
            UpdateTurntableRotation();
        }

        /* === 04. RESILIENT ASSET LOADING & MODEL SPAWNING === */
        public void LoadHero3DModel(HeroData heroData)
        {
            if (heroData == null) return;

            // Clear previous hero instance and aura
            if (_activeHeroInstance != null)
            {
                Destroy(_activeHeroInstance);
                _activeHeroInstance = null;
            }
            if (_activeAuraInstance != null)
            {
                Destroy(_activeAuraInstance.gameObject);
                _activeAuraInstance = null;
            }

            GameObject prefabToSpawn = null;
            ParticleSystem auraToSpawn = null;

            // 1. Check registered mappings
            for (int i = 0; i < heroPrefabs.Count; i++)
            {
                if (heroPrefabs[i] != null && 
                    !string.IsNullOrEmpty(heroPrefabs[i].heroId) &&
                    heroPrefabs[i].heroId.Equals(heroData.heroId, StringComparison.OrdinalIgnoreCase))
                {
                    prefabToSpawn = heroPrefabs[i].heroPrefab;
                    auraToSpawn = heroPrefabs[i].heroAuthenticAura;
                    break;
                }
            }

            // 2. Fallback to Resources.Load or modelPrefabPath
            if (prefabToSpawn == null && !string.IsNullOrEmpty(heroData.modelPrefabPath))
            {
                prefabToSpawn = Resources.Load<GameObject>(heroData.modelPrefabPath);
            }

            if (prefabToSpawn == null)
            {
                prefabToSpawn = defaultFallbackHeroPrefab;
            }

            // 3. Instantiate model on pedestal
            Transform spawnParent = characterSpawnPoint != null ? characterSpawnPoint : transform;
            if (prefabToSpawn != null)
            {
                _activeHeroInstance = Instantiate(prefabToSpawn, spawnParent.position, spawnParent.rotation, spawnParent);
                _activeHeroInstance.transform.localPosition = Vector3.zero;
                _activeHeroInstance.transform.localRotation = Quaternion.identity;

                // Adjust scale if necessary
                _activeHeroInstance.transform.localScale = Vector3.one;

                _currentAnimator = _activeHeroInstance.GetComponentInChildren<Animator>();
                if (_currentAnimator != null)
                {
                    // Trigger Stand / Idle animation
                    if (_currentAnimator.HasState(0, StandHash))
                    {
                        _currentAnimator.Play(StandHash);
                    }
                    else
                    {
                        _currentAnimator.SetTrigger(IdleHash);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Hero3DShowcase] No 3D model found for {heroData.heroName} ({heroData.heroId}). Model path: {heroData.modelPrefabPath}");
            }

            // 4. Instantiate Authentic Hero Aura if present
            if (auraToSpawn != null)
            {
                _activeAuraInstance = Instantiate(auraToSpawn, spawnParent.position, spawnParent.rotation, spawnParent);
                _activeAuraInstance.Play();
            }

            // 5. Stage lighting tune
            if (pedestalSpotlight != null)
            {
                pedestalSpotlight.color = GetFranchiseLightColor(heroData.franchise);
            }

            if (floorGlyphParticles != null)
            {
                var main = floorGlyphParticles.main;
                main.startColor = GetFranchiseLightColor(heroData.franchise);
                floorGlyphParticles.Clear();
                floorGlyphParticles.Play();
            }

            _lastInteractionTime = Time.time;
        }

        private Color GetFranchiseLightColor(FranchiseType franchise)
        {
            switch (franchise)
            {
                case FranchiseType.Bleach:
                    return new Color(0.2f, 0.8f, 1.0f); // Spiritual Azure
                case FranchiseType.Fate:
                    return new Color(1.0f, 0.85f, 0.2f); // Holy Gold
                case FranchiseType.Naruto:
                    return new Color(1.0f, 0.5f, 0.1f); // Sage Orange
                case FranchiseType.OnePiece:
                    return new Color(1.0f, 0.25f, 0.25f); // Crimson Pirate
                case FranchiseType.DragonBall:
                    return new Color(1.0f, 0.95f, 0.4f); // Golden Ki
                default:
                    return new Color(0.7f, 0.8f, 1.0f);
            }
        }

        /* === 05. ANIMATION SEQUENCE PLAYBACK === */
        public void PlaySkillPreviewAnimation(int skillIndex)
        {
            if (_currentAnimator == null) return;

            _currentAnimator.SetInteger(SkillIndexHash, skillIndex);
            _currentAnimator.SetTrigger(SkillCastHash);
            _lastInteractionTime = Time.time;
        }

        public void PlayAttackAnimation()
        {
            if (_currentAnimator == null) return;

            _currentAnimator.SetTrigger(AttackHash);
            _lastInteractionTime = Time.time;
        }

        /* === 06. INPUT CONTROLLER & TURNTABLE INTERACTION === */
        private void HandleTouchInput()
        {
            // Mouse Drag or Touch swipe
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _previousTouchPos = Input.mousePosition;
                _currentAngularVelocity = 0f;
                _lastInteractionTime = Time.time;
            }
            else if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector2 currentPos = Input.mousePosition;
                float deltaX = currentPos.x - _previousTouchPos.x;

                _currentAngularVelocity = -deltaX * touchRotationSpeed;
                RotateTurntable(_currentAngularVelocity);

                _previousTouchPos = currentPos;
                _lastInteractionTime = Time.time;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            // Mobile Multi-touch support
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    _isDragging = true;
                    _previousTouchPos = t.position;
                    _currentAngularVelocity = 0f;
                    _lastInteractionTime = Time.time;
                }
                else if (t.phase == TouchPhase.Moved && _isDragging)
                {
                    float deltaX = t.deltaPosition.x;
                    _currentAngularVelocity = -deltaX * touchRotationSpeed;
                    RotateTurntable(_currentAngularVelocity);
                    _lastInteractionTime = Time.time;
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    _isDragging = false;
                }
            }
        }

        private void UpdateTurntableRotation()
        {
            if (!_isDragging)
            {
                // Apply inertia damping
                if (Mathf.Abs(_currentAngularVelocity) > 0.01f)
                {
                    RotateTurntable(_currentAngularVelocity);
                    _currentAngularVelocity *= inertiaDamping;
                }
                else
                {
                    _currentAngularVelocity = 0f;

                    // Auto-rotate idle showcase after timeout
                    if (Time.time - _lastInteractionTime > autoRotateIdleDelay)
                    {
                        RotateTurntable(autoRotationSpeed * Time.deltaTime);
                    }
                }
            }
        }

        private void RotateTurntable(float angleDelta)
        {
            Transform target = turntablePlatform != null ? turntablePlatform : characterSpawnPoint;
            if (target != null)
            {
                target.Rotate(Vector3.up, angleDelta, Space.World);
            }
            else if (_activeHeroInstance != null)
            {
                _activeHeroInstance.transform.Rotate(Vector3.up, angleDelta, Space.World);
            }
        }
    }
}
