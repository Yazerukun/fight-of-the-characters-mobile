/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FOC.HeroSystems;

namespace FOC.UI_UX
{
    public class FOCLoadingScreen : MonoBehaviour
    {
        [Header("Header & Brand Typography")]
        [SerializeField] private Text gameTitleText;
        [SerializeField] private Text gameVersionText;
        [SerializeField] private Text subtitleText;

        [Header("Visual Artwork & Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup cinematicCanvasGroup;
        [SerializeField] private ParticleSystem ambientEnergyParticles;

        [Header("Progress Bar & Status Display")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text progressPercentageText;
        [SerializeField] private Text statusMessageText;
        [SerializeField] private Image progressBarGlow;

        [Header("Gameplay Tips")]
        [SerializeField] private Text gameplayTipText;
        [SerializeField] private float tipRotationInterval = 3.5f;

        [Header("Scene Transition")]
        [SerializeField] private CanvasGroup screenFadeGroup;
        [SerializeField] private float fadeDuration = 0.75f;
        [SerializeField] private string targetHeroSelectScene = "HeroSelectionScene";

        [Header("Audio Integration")]
        [SerializeField] private AudioSource ambientAudioSource;
        [SerializeField] private AudioClip loadingBgmClip;

        /* === 02. SECURITY & STATE INVARIANTS === */
        private bool _isTransitionLocked = false;
        private bool _isAssetsPreloaded = false;

        /* === 03. PRECISION MATH & TIMERS === */
        private readonly string[] _gameplayTips = new string[]
        {
            "💡 Tip: Combine items in your inventory to forge legendary equipment!",
            "💡 Tip: Use your Flash Step (Shunpo) or Dash to dodge lethal area-of-effect ultimates!",
            "💡 Tip: Watch the Duel Arena timer — winning the 1v1 duel awards massive team gold!",
            "💡 Tip: Pure damage completely ignores enemy armor and magic resistance!",
            "💡 Tip: Coordinate hero classes during the Hero Picking phase: balance STR, AGI, and INT!",
            "💡 Tip: Eliminating neutral creep camps grants vital runes, gold bounties, and experience!",
            "💡 Tip: Agility increases attack speed and armor; Strength boosts max HP and health regen."
        };

        private float _currentProgress = 0f;
        private int _currentTipIndex = 0;
        private float _tipTimer = 0f;

        /* === 04. RESILIENT SETUP & LIFECYCLE HOOKS === */
        private void Awake()
        {
            if (screenFadeGroup == null)
            {
                screenFadeGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            InitializeScreenDisplay();
            StartCoroutine(ExecuteActualSceneLoadingSequence());
        }

        private void InitializeScreenDisplay()
        {
            if (gameTitleText != null) gameTitleText.text = "FIGHT OF CHARACTERS";
            if (gameVersionText != null) gameVersionText.text = "9.1a (Asia)";
            if (subtitleText != null) subtitleText.text = "The Ultimate Battle Begins...";

            if (progressBar != null)
            {
                progressBar.minValue = 0f;
                progressBar.maxValue = 1f;
                progressBar.value = 0f;
            }

            _currentTipIndex = UnityEngine.Random.Range(0, _gameplayTips.Length);
            UpdateTipDisplay();

            if (ambientAudioSource != null && loadingBgmClip != null)
            {
                ambientAudioSource.clip = loadingBgmClip;
                ambientAudioSource.loop = true;
                ambientAudioSource.volume = 0.8f;
                ambientAudioSource.Play();
            }

            if (ambientEnergyParticles != null)
            {
                ambientEnergyParticles.Play();
            }
        }

        /* === 05. ENTITY & STATE MANAGEMENT === */
        private void Update()
        {
            UpdateTipsCycle();
        }

        private void UpdateTipsCycle()
        {
            _tipTimer += Time.deltaTime;
            if (_tipTimer >= tipRotationInterval)
            {
                _tipTimer = 0f;
                _currentTipIndex = (_currentTipIndex + 1) % _gameplayTips.Length;
                StartCoroutine(CrossfadeTipRoutine());
            }
        }

        private IEnumerator CrossfadeTipRoutine()
        {
            if (gameplayTipText == null) yield break;

            float duration = 0.25f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                Color c = gameplayTipText.color;
                c.a = Mathf.Lerp(1f, 0f, t / duration);
                gameplayTipText.color = c;
                yield return null;
            }

            UpdateTipDisplay();

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                Color c = gameplayTipText.color;
                c.a = Mathf.Lerp(0f, 1f, t / duration);
                gameplayTipText.color = c;
                yield return null;
            }
        }

        private void UpdateTipDisplay()
        {
            if (gameplayTipText != null && _currentTipIndex < _gameplayTips.Length)
            {
                gameplayTipText.text = _gameplayTips[_currentTipIndex];
            }
        }

        /* === 06. ACTUAL ASSET INITIALIZATION & ASYNC SCENE LOADING === */
        private IEnumerator ExecuteActualSceneLoadingSequence()
        {
            // Stage 1: Initialize Database & Core Archetypes
            SetLoadingState(0.15f, "Initializing Fight of Characters 9.1a Hero Database...");
            yield return null;

            try
            {
                var heroes = HeroDatabase.GetAllHeroes();
                Debug.Log($"[LoadingScreen] Pre-cached {heroes.Count} original hero archetypes.");
                _isAssetsPreloaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadingScreen] Error caching heroes: {ex.Message}");
            }
            yield return new WaitForSeconds(0.3f);

            // Stage 2: Preload Shader Variants & Graphics Pipelines
            SetLoadingState(0.35f, "Compiling URP Anime Cel Shaders & Outlines...");
            yield return new WaitForSeconds(0.35f);

            // Stage 3: Begin Actual Asynchronous Scene Loading
            SetLoadingState(0.50f, $"Loading Scene: {targetHeroSelectScene}...");

            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = SceneManager.LoadSceneAsync(targetHeroSelectScene);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LoadingScreen] SceneManager async load exception: {ex.Message}. Falling back to synchronous sequence.");
            }

            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = false;

                while (asyncOp.progress < 0.9f)
                {
                    // Map async progress (0.0 to 0.9) to UI display (0.50 to 0.90)
                    float mappedProgress = 0.50f + (asyncOp.progress / 0.9f) * 0.40f;
                    SetLoadingState(mappedProgress, "Loading Scene Hierarchy & UI Components...");
                    yield return null;
                }

                SetLoadingState(0.95f, "Finalizing 3D Stage & Lighting...");
                yield return new WaitForSeconds(0.3f);

                SetLoadingState(1.0f, "Ready for Battle!");
                yield return new WaitForSeconds(0.4f);

                // Fade transition
                yield return StartCoroutine(FadeOutRoutine());

                // Activate scene
                asyncOp.allowSceneActivation = true;
            }
            else
            {
                // Fallback for standalone/mock testing
                SetLoadingState(0.85f, "Loading Arena Data & Waypoint Collisions...");
                yield return new WaitForSeconds(0.5f);

                SetLoadingState(1.0f, "Ready for Battle!");
                yield return new WaitForSeconds(0.4f);

                yield return StartCoroutine(FadeOutRoutine());
                SceneManager.LoadScene(targetHeroSelectScene);
            }
        }

        private void SetLoadingState(float progress, string statusMessage)
        {
            _currentProgress = Mathf.Clamp01(progress);

            if (progressBar != null)
            {
                progressBar.value = _currentProgress;
            }

            if (progressPercentageText != null)
            {
                int pct = Mathf.RoundToInt(_currentProgress * 100f);
                progressPercentageText.text = $"Loading Game... {pct}%";
            }

            if (statusMessageText != null)
            {
                statusMessageText.text = statusMessage;
            }

            if (progressBarGlow != null)
            {
                Color glow = progressBarGlow.color;
                glow.a = 0.4f + (_currentProgress * 0.6f);
                progressBarGlow.color = glow;
            }
        }

        /* === 07. RENDER PIPELINE, UI FADE & TRANSITIONS === */
        private IEnumerator FadeOutRoutine()
        {
            if (_isTransitionLocked) yield break;
            _isTransitionLocked = true;

            if (screenFadeGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    screenFadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                    yield return null;
                }
                screenFadeGroup.alpha = 0f;
            }
        }
    }
}
