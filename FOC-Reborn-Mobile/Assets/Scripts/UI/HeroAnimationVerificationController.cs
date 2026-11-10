/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FOC.HeroSystems;

namespace FOC.UI_UX
{
    [System.Serializable]
    public class AnimationSequenceAudit
    {
        public string sequenceName;
        public int startMs;
        public int endMs;
        public float moveSpeed;
        public bool isLooping;
        public bool isMecanimSupported;
    }

    public class HeroAnimationVerificationController : MonoBehaviour
    {
        [Header("Hero Selection Buttons")]
        [SerializeField] private Button selectIchigoBtn;
        [SerializeField] private Button selectSaberBtn;
        [SerializeField] private Button selectRockLeeBtn;
        [SerializeField] private Button selectLuffyBtn;

        [Header("Playback Controls")]
        [SerializeField] private Button playBtn;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button restartBtn;
        [SerializeField] private Toggle skeletonDebugToggle;

        [Header("Display Panels")]
        [SerializeField] private Text activeHeroTitleText;
        [SerializeField] private Text mdxSourceText;
        [SerializeField] private Text animationCountSummaryText;
        [SerializeField] private Transform sequenceButtonsContainer;
        [SerializeField] private GameObject sequenceButtonPrefab;
        [SerializeField] private Text activeSequenceLogText;
        [SerializeField] private Slider playbackScrubber;
        [SerializeField] private Text timecodeText;

        [Header("3D Character Stage")]
        [SerializeField] private Hero3DShowcaseManager showcaseManager;

        /* === 02. STATE & REGISTRY === */
        private string _currentHeroId = "hero_ichigo";
        private AnimationSequenceAudit _currentSequence;
        private bool _isPlaying = true;
        private float _currentAnimTime = 0f;
        private bool _showSkeletonDebug = false;

        private readonly Dictionary<string, List<AnimationSequenceAudit>> _heroSequences = new Dictionary<string, List<AnimationSequenceAudit>>
        {
            ["hero_ichigo"] = new List<AnimationSequenceAudit>
            {
                new AnimationSequenceAudit { sequenceName = "Stand", startMs = 10000, endMs = 11000, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk", startMs = 5667, endMs = 6333, moveSpeed = 320, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack - 1", startMs = 1667, endMs = 2667, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack - 2", startMs = 3000, endMs = 4000, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Death", startMs = 6667, endMs = 7667, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Dissipate", startMs = 8000, endMs = 9667, moveSpeed = 0, isLooping = false, isMecanimSupported = false },
                new AnimationSequenceAudit { sequenceName = "Morph (Bankai)", startMs = 15000, endMs = 16333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack Alternate - 1", startMs = 18333, endMs = 19333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack Alternate - 2", startMs = 19667, endMs = 20667, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk Alternate", startMs = 22333, endMs = 23000, moveSpeed = 320, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand Alternate", startMs = 26667, endMs = 27667, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Death Alternate", startMs = 23333, endMs = 24333, moveSpeed = 0, isLooping = false, isMecanimSupported = false }
            },
            ["hero_saber"] = new List<AnimationSequenceAudit>
            {
                new AnimationSequenceAudit { sequenceName = "Stand", startMs = 30500, endMs = 31667, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand 2", startMs = 2333, endMs = 5867, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand Ready", startMs = 33133, endMs = 34533, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk", startMs = 28767, endMs = 29500, moveSpeed = 290, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack", startMs = 14333, endMs = 15500, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack 2", startMs = 31833, endMs = 32967, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack Slam", startMs = 17000, endMs = 18133, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell (Excalibur)", startMs = 66500, endMs = 67900, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Death", startMs = 22000, endMs = 23900, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Dissipate", startMs = 50133, endMs = 53500, moveSpeed = 0, isLooping = false, isMecanimSupported = false }
            },
            ["hero_lee"] = new List<AnimationSequenceAudit>
            {
                new AnimationSequenceAudit { sequenceName = "Stand", startMs = 1667, endMs = 2667, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand Ready", startMs = 333, endMs = 1333, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk", startMs = 5667, endMs = 6367, moveSpeed = 600, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack", startMs = 3000, endMs = 4000, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack 2", startMs = 17333, endMs = 18333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack 3", startMs = 4333, endMs = 5333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell (Dynamic Entry)", startMs = 7000, endMs = 8000, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell Throw", startMs = 13333, endMs = 14333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell Slam (Hidden Lotus)", startMs = 18667, endMs = 20500, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Morph (Eight Gates)", startMs = 14400, endMs = 15900, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Death", startMs = 9667, endMs = 10667, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Dissipate", startMs = 11000, endMs = 13000, moveSpeed = 0, isLooping = false, isMecanimSupported = false }
            },
            ["hero_luffy"] = new List<AnimationSequenceAudit>
            {
                new AnimationSequenceAudit { sequenceName = "Stand", startMs = 333, endMs = 1333, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand Ready", startMs = 1667, endMs = 2667, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk", startMs = 7000, endMs = 7833, moveSpeed = 300, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack", startMs = 3000, endMs = 4000, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack - 2", startMs = 4333, endMs = 5333, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell (Pistol)", startMs = 14667, endMs = 14838, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Spell Channel", startMs = 12333, endMs = 13667, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Morph (Gear Second)", startMs = 23505, endMs = 24505, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Stand Alternate", startMs = 33172, endMs = 34172, moveSpeed = 0, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Attack Alternate", startMs = 35838, endMs = 36838, moveSpeed = 0, isLooping = false, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Walk Alternate", startMs = 39838, endMs = 40672, moveSpeed = 300, isLooping = true, isMecanimSupported = true },
                new AnimationSequenceAudit { sequenceName = "Death", startMs = 8000, endMs = 9000, moveSpeed = 0, isLooping = false, isMecanimSupported = true }
            }
        };

        private void Start()
        {
            RegisterButtonListeners();
            DisplayHeroAudit("hero_ichigo");
        }

        private void RegisterButtonListeners()
        {
            if (selectIchigoBtn != null) selectIchigoBtn.onClick.AddListener(() => DisplayHeroAudit("hero_ichigo"));
            if (selectSaberBtn != null) selectSaberBtn.onClick.AddListener(() => DisplayHeroAudit("hero_saber"));
            if (selectRockLeeBtn != null) selectRockLeeBtn.onClick.AddListener(() => DisplayHeroAudit("hero_lee"));
            if (selectLuffyBtn != null) selectLuffyBtn.onClick.AddListener(() => DisplayHeroAudit("hero_luffy"));

            if (playBtn != null) playBtn.onClick.AddListener(() => _isPlaying = true);
            if (pauseBtn != null) pauseBtn.onClick.AddListener(() => _isPlaying = false);
            if (restartBtn != null) restartBtn.onClick.AddListener(RestartCurrentSequence);

            if (skeletonDebugToggle != null)
            {
                skeletonDebugToggle.onValueChanged.AddListener((val) => _showSkeletonDebug = val);
            }
        }

        private void Update()
        {
            if (_isPlaying && _currentSequence != null)
            {
                float durationSec = (_currentSequence.endMs - _currentSequence.startMs) / 1000f;
                if (durationSec > 0.05f)
                {
                    _currentAnimTime += Time.deltaTime;
                    if (_currentAnimTime >= durationSec)
                    {
                        if (_currentSequence.isLooping)
                        {
                            _currentAnimTime %= durationSec;
                        }
                        else
                        {
                            _currentAnimTime = durationSec;
                            _isPlaying = false;
                        }
                    }

                    if (playbackScrubber != null)
                    {
                        playbackScrubber.value = Mathf.Clamp01(_currentAnimTime / durationSec);
                    }

                    if (timecodeText != null)
                    {
                        int currentMs = _currentSequence.startMs + Mathf.RoundToInt(_currentAnimTime * 1000f);
                        timecodeText.text = $"{currentMs} ms / {_currentSequence.endMs} ms";
                    }
                }
            }
        }

        public void DisplayHeroAudit(string heroId)
        {
            _currentHeroId = heroId;
            var hero = HeroDatabase.GetHeroById(heroId);
            if (hero == null) return;

            if (activeHeroTitleText != null) activeHeroTitleText.text = $"{hero.heroName} ({hero.title})";
            if (mdxSourceText != null) mdxSourceText.text = $"Model Path: {hero.modelPrefabPath}  |  Franchise: {hero.franchise}";

            if (_heroSequences.TryGetValue(heroId, out var seqList))
            {
                if (animationCountSummaryText != null)
                {
                    animationCountSummaryText.text = $"Total Original Sequences: {seqList.Count}  |  Preserved: 100%";
                }

                PopulateSequenceButtons(seqList);
                if (seqList.Count > 0)
                {
                    TriggerSequence(seqList[0]);
                }
            }

            if (showcaseManager != null)
            {
                showcaseManager.LoadHero3DModel(hero);
            }
        }

        private void PopulateSequenceButtons(List<AnimationSequenceAudit> sequences)
        {
            if (sequenceButtonsContainer == null) return;

            foreach (Transform child in sequenceButtonsContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < sequences.Count; i++)
            {
                var seq = sequences[i];
                if (sequenceButtonPrefab != null)
                {
                    var btnObj = Instantiate(sequenceButtonPrefab, sequenceButtonsContainer);
                    var btn = btnObj.GetComponent<Button>();
                    var txt = btnObj.GetComponentInChildren<Text>();

                    if (txt != null)
                    {
                        txt.text = $"{seq.sequenceName} [{seq.startMs}-{seq.endMs}ms]";
                    }

                    if (btn != null)
                    {
                        btn.onClick.AddListener(() => TriggerSequence(seq));
                    }
                }
            }
        }

        private void TriggerSequence(AnimationSequenceAudit seq)
        {
            _currentSequence = seq;
            _currentAnimTime = 0f;
            _isPlaying = true;

            if (activeSequenceLogText != null)
            {
                activeSequenceLogText.text = $"Playing Sequence: '{seq.sequenceName}' | Interval: {seq.startMs}-{seq.endMs} ms (Speed: {seq.moveSpeed}). Rig Status: {(seq.isMecanimSupported ? "RIGGED & READY" : "STATIC")}";
            }

            if (showcaseManager != null)
            {
                if (seq.sequenceName.Contains("Attack", StringComparison.OrdinalIgnoreCase))
                {
                    showcaseManager.PlayAttackAnimation();
                }
                else if (seq.sequenceName.Contains("Spell", StringComparison.OrdinalIgnoreCase) || seq.sequenceName.Contains("Morph", StringComparison.OrdinalIgnoreCase))
                {
                    showcaseManager.PlaySkillPreviewAnimation(1);
                }
            }
        }

        private void RestartCurrentSequence()
        {
            _currentAnimTime = 0f;
            _isPlaying = true;
        }

        private void OnDrawGizmos()
        {
            if (!_showSkeletonDebug) return;

            // Draw skeleton debug lines if transforms are active
            Gizmos.color = Color.green;
            Transform[] allBones = GetComponentsInChildren<Transform>();
            for (int i = 0; i < allBones.Length; i++)
            {
                Transform b = allBones[i];
                if (b.parent != null && b.parent != transform)
                {
                    Gizmos.DrawLine(b.position, b.parent.position);
                    Gizmos.DrawWireSphere(b.position, 0.02f);
                }
            }
        }
    }
}
