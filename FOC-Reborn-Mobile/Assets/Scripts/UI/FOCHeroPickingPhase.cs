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
    public class FOCHeroPickingPhase : MonoBehaviour
    {
        [Header("Phase Header & Timer")]
        [SerializeField] private Text phaseTitleText;
        [SerializeField] private Text countdownTimerText;
        [SerializeField] private int totalPickTimeSeconds = 30;

        [Header("Filter Tabs")]
        [SerializeField] private Button filterAllButton;
        [SerializeField] private Button filterStrButton;
        [SerializeField] private Button filterAgiButton;
        [SerializeField] private Button filterIntButton;
        [SerializeField] private Image filterAllGlow;
        [SerializeField] private Image filterStrGlow;
        [SerializeField] private Image filterAgiGlow;
        [SerializeField] private Image filterIntGlow;

        [Header("Hero Selection Grid")]
        [SerializeField] private Transform heroGridContainer;
        [SerializeField] private GameObject heroCardPrefab;

        [Header("Hero Showcase Preview Panel (Right Side)")]
        [SerializeField] private Image heroPortraitLarge;
        [SerializeField] private Text heroNameText;
        [SerializeField] private Text heroTitleText;
        [SerializeField] private Text heroAttributesSummaryText;
        [SerializeField] private Text heroLoreDescriptionText;
        [SerializeField] private Transform skillsContainer;
        [SerializeField] private GameObject skillIconPrefab;
        [SerializeField] private Hero3DShowcaseManager showcase3DManager;

        [Header("Skill Tooltip Details (Popup Modal)")]
        [SerializeField] private GameObject skillTooltipPanel;
        [SerializeField] private Text skillTooltipNameText;
        [SerializeField] private Text skillTooltipFormulaText;
        [SerializeField] private Text skillTooltipCostText;
        [SerializeField] private Text skillTooltipDetailsText;
        [SerializeField] private Button closeSkillTooltipButton;

        [Header("Action Buttons")]
        [SerializeField] private Button confirmPickButton;
        [SerializeField] private Text confirmButtonText;
        [SerializeField] private Image confirmButtonGlow;
        [SerializeField] private Button battlefieldInfoButton;
        [SerializeField] private GameObject battlefieldInfoModal;
        [SerializeField] private Button closeBattlefieldModalButton;

        [Header("Player Slots (Left Side Roster)")]
        [SerializeField] private Transform playerSlotsContainer;
        [SerializeField] private GameObject playerSlotPrefab;
        [SerializeField] private Image miniMapThumbnail;

        [Header("Audio")]
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip selectHeroSfx;
        [SerializeField] private AudioClip lockInSfx;
        [SerializeField] private AudioClip timerTickSfx;
        [SerializeField] private AudioClip skillInspectSfx;

        /* === 02. SECURITY, CRYPTO & STATE INVARIANTS === */
        private readonly HashSet<string> _lockedHeroIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _isLocalPlayerLockedIn = false;
        private bool _isActionLocked = false;
        private bool _hasTransitioned = false;
        private int _lastTickSecond = -1;
        private double _pauseTimestamp = 0;

        /* === 03. PRECISION MATH, STATE & ENUMS === */
        private List<HeroData> _allHeroes = new List<HeroData>();
        private List<HeroData> _filteredHeroes = new List<HeroData>();
        private HeroFilterCategory _activeFilter = HeroFilterCategory.All;
        private HeroData _selectedHero;
        private float _timeRemaining;
        private List<PlayerSlotData> _playerSlots = new List<PlayerSlotData>();

        /* === 04. RESILIENT SETUP & LIFECYCLE HOOKS === */
        private void Awake()
        {
            if (sfxAudioSource == null)
            {
                sfxAudioSource = gameObject.AddComponent<AudioSource>();
                sfxAudioSource.playOnAwake = false;
            }

            RegisterStaticEventListeners();
        }

        private void Start()
        {
            BeginHeroPickSession();
        }

        private void RegisterStaticEventListeners()
        {
            // Register button listeners once in Awake to prevent duplicate callbacks
            if (filterAllButton != null) filterAllButton.onClick.AddListener(() => ApplyFilter(HeroFilterCategory.All));
            if (filterStrButton != null) filterStrButton.onClick.AddListener(() => ApplyFilter(HeroFilterCategory.Strength));
            if (filterAgiButton != null) filterAgiButton.onClick.AddListener(() => ApplyFilter(HeroFilterCategory.Agility));
            if (filterIntButton != null) filterIntButton.onClick.AddListener(() => ApplyFilter(HeroFilterCategory.Intelligence));

            if (confirmPickButton != null) confirmPickButton.onClick.AddListener(OnConfirmPickClicked);

            if (battlefieldInfoButton != null)
            {
                battlefieldInfoButton.onClick.AddListener(() =>
                {
                    if (battlefieldInfoModal != null) battlefieldInfoModal.SetActive(true);
                    PlaySfx(skillInspectSfx);
                });
            }

            if (closeBattlefieldModalButton != null)
            {
                closeBattlefieldModalButton.onClick.AddListener(() =>
                {
                    if (battlefieldInfoModal != null) battlefieldInfoModal.SetActive(false);
                    PlaySfx(skillInspectSfx);
                });
            }

            if (closeSkillTooltipButton != null)
            {
                closeSkillTooltipButton.onClick.AddListener(HideSkillTooltip);
            }
        }

        public void BeginHeroPickSession()
        {
            _allHeroes = HeroDatabase.GetAllHeroes();
            _lockedHeroIds.Clear();
            _timeRemaining = totalPickTimeSeconds;
            _isLocalPlayerLockedIn = false;
            _isActionLocked = false;
            _hasTransitioned = false;
            _lastTickSecond = -1;

            if (battlefieldInfoModal != null) battlefieldInfoModal.SetActive(false);
            if (skillTooltipPanel != null) skillTooltipPanel.SetActive(false);

            InitializePlayerSlots();
            ApplyFilter(HeroFilterCategory.All);

            // Item 1: Set consistent initial hero selection
            if (_filteredHeroes.Count > 0)
            {
                SelectHero(_filteredHeroes[0]);
            }

            StartCoroutine(CountdownRoutine());
            StartCoroutine(SimulateBotPicksRoutine());
        }

        /* === 05. ENTITY COMPONENT SYSTEM & ROSTER MANAGEMENT === */
        private void InitializePlayerSlots()
        {
            _playerSlots.Clear();

            // 5-Slot Blue Team Roster (matching user design mockup)
            for (int i = 0; i < 5; i++)
            {
                var slot = new PlayerSlotData
                {
                    playerIndex = i,
                    playerName = (i == 0) ? "Player 1 (You)" : $"Player {i + 1}",
                    avatarIconPath = "",
                    state = PlayerPickState.Picking,
                    selectedHeroId = "",
                    isLocalPlayer = (i == 0),
                    isBot = (i != 0)
                };
                _playerSlots.Add(slot);
            }

            RefreshPlayerSlotsUI();
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & SELECTION LOGIC === */
        public void ApplyFilter(HeroFilterCategory filter)
        {
            _activeFilter = filter;
            _filteredHeroes.Clear();

            for (int i = 0; i < _allHeroes.Count; i++)
            {
                var hero = _allHeroes[i];
                if (filter == HeroFilterCategory.All)
                {
                    _filteredHeroes.Add(hero);
                }
                else if (filter == HeroFilterCategory.Strength && hero.primaryAttribute == AttributeType.Strength)
                {
                    _filteredHeroes.Add(hero);
                }
                else if (filter == HeroFilterCategory.Agility && hero.primaryAttribute == AttributeType.Agility)
                {
                    _filteredHeroes.Add(hero);
                }
                else if (filter == HeroFilterCategory.Intelligence && hero.primaryAttribute == AttributeType.Intelligence)
                {
                    _filteredHeroes.Add(hero);
                }
            }

            UpdateFilterGlows();
            PopulateHeroGrid();
            PlaySfx(skillInspectSfx);
        }

        private void UpdateFilterGlows()
        {
            if (filterAllGlow != null) filterAllGlow.enabled = (_activeFilter == HeroFilterCategory.All);
            if (filterStrGlow != null) filterStrGlow.enabled = (_activeFilter == HeroFilterCategory.Strength);
            if (filterAgiGlow != null) filterAgiGlow.enabled = (_activeFilter == HeroFilterCategory.Agility);
            if (filterIntGlow != null) filterIntGlow.enabled = (_activeFilter == HeroFilterCategory.Intelligence);
        }

        private void PopulateHeroGrid()
        {
            if (heroGridContainer == null) return;

            foreach (Transform child in heroGridContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < _filteredHeroes.Count; i++)
            {
                var hero = _filteredHeroes[i];
                bool isLockedBySomeoneElse = _lockedHeroIds.Contains(hero.heroId);

                if (heroCardPrefab != null)
                {
                    var cardObj = Instantiate(heroCardPrefab, heroGridContainer);
                    var cardBtn = cardObj.GetComponent<Button>();
                    var cardOutline = cardObj.GetComponent<Outline>();

                    // Bind Item 2 & 3: Hero Name, Portrait, Primary Attribute
                    var texts = cardObj.GetComponentsInChildren<Text>();
                    for (int t = 0; t < texts.Length; t++)
                    {
                        if (texts[t].name.Contains("Name", StringComparison.OrdinalIgnoreCase))
                        {
                            texts[t].text = hero.heroName;
                        }
                        else if (texts[t].name.Contains("Attr", StringComparison.OrdinalIgnoreCase))
                        {
                            texts[t].text = hero.primaryAttribute.ToString().ToUpper();
                        }
                    }

                    var images = cardObj.GetComponentsInChildren<Image>();
                    for (int img = 0; img < images.Length; img++)
                    {
                        if (images[img].name.Contains("Portrait", StringComparison.OrdinalIgnoreCase))
                        {
                            Sprite portrait = Resources.Load<Sprite>(hero.portraitAssetPath);
                            if (portrait != null) images[img].sprite = portrait;
                        }
                    }

                    // Item 9: Unique Pick Rule - disable if already locked by someone else
                    if (cardBtn != null)
                    {
                        if (isLockedBySomeoneElse)
                        {
                            cardBtn.interactable = false;
                        }
                        else
                        {
                            cardBtn.interactable = !_isLocalPlayerLockedIn;
                            cardBtn.onClick.AddListener(() => SelectHero(hero));
                        }
                    }

                    if (cardOutline != null)
                    {
                        cardOutline.enabled = (_selectedHero != null && _selectedHero.heroId == hero.heroId);
                    }
                }
            }
        }

        public void SelectHero(HeroData hero)
        {
            if (hero == null || _isLocalPlayerLockedIn) return;

            // Item 9: Cannot select hero already locked by another player
            if (_lockedHeroIds.Contains(hero.heroId))
            {
                Debug.LogWarning($"[HeroPick] Cannot select {hero.heroName} — already locked by another player!");
                return;
            }

            _selectedHero = hero;

            // Item 7: Update Local Player Slot
            if (_playerSlots.Count > 0)
            {
                _playerSlots[0].selectedHeroId = hero.heroId;
                _playerSlots[0].state = PlayerPickState.Picking;
                RefreshPlayerSlotsUI();
            }

            // Item 6: Update 3D Showcase Pedestal
            UpdateShowcasePanel();

            // Refresh selection highlight in grid
            UpdateGridSelectionOutline();
            PlaySfx(selectHeroSfx);
        }

        private void UpdateGridSelectionOutline()
        {
            if (heroGridContainer == null) return;
            for (int i = 0; i < heroGridContainer.childCount && i < _filteredHeroes.Count; i++)
            {
                var cardObj = heroGridContainer.GetChild(i);
                var outline = cardObj.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = (_selectedHero != null && _selectedHero.heroId == _filteredHeroes[i].heroId);
                }
            }
        }

        private void UpdateShowcasePanel()
        {
            if (_selectedHero == null) return;

            if (heroNameText != null) heroNameText.text = _selectedHero.heroName;
            if (heroTitleText != null) heroTitleText.text = _selectedHero.title;
            if (heroLoreDescriptionText != null) heroLoreDescriptionText.text = _selectedHero.loreDescription;

            if (heroAttributesSummaryText != null)
            {
                heroAttributesSummaryText.text = $"HP: {_selectedHero.baseHealth}   |   MP: {_selectedHero.baseMana}   |   ARMOR: {_selectedHero.baseArmor}\n" +
                                                 $"DAMAGE: {_selectedHero.baseMinDamage}-{_selectedHero.baseMaxDamage}   |   MOVE SPEED: {_selectedHero.movementSpeed}\n" +
                                                 $"STR: {_selectedHero.baseStrength} (+{_selectedHero.strengthGainPerLevel:F1})   |   " +
                                                 $"AGI: {_selectedHero.baseAgility} (+{_selectedHero.agilityGainPerLevel:F1})   |   " +
                                                 $"INT: {_selectedHero.baseIntelligence} (+{_selectedHero.intelligenceGainPerLevel:F1})";
            }

            if (heroPortraitLarge != null && !string.IsNullOrEmpty(_selectedHero.portraitAssetPath))
            {
                Sprite portrait = Resources.Load<Sprite>(_selectedHero.portraitAssetPath);
                if (portrait != null) heroPortraitLarge.sprite = portrait;
            }

            // Item 4: Populate Skill Icons
            PopulateSkillIcons();

            // Item 6: Actual 3D Model Loading
            if (showcase3DManager != null)
            {
                showcase3DManager.LoadHero3DModel(_selectedHero);
            }
        }

        private void PopulateSkillIcons()
        {
            if (skillsContainer == null || _selectedHero == null) return;

            foreach (Transform child in skillsContainer)
            {
                Destroy(child.gameObject);
            }

            if (_selectedHero.skills == null) return;

            for (int i = 0; i < _selectedHero.skills.Count; i++)
            {
                var skill = _selectedHero.skills[i];
                int skillIndex = i;

                if (skillIconPrefab != null)
                {
                    var iconObj = Instantiate(skillIconPrefab, skillsContainer);
                    var iconBtn = iconObj.GetComponent<Button>();
                    var iconImg = iconObj.GetComponent<Image>();
                    var keyText = iconObj.GetComponentInChildren<Text>();

                    if (keyText != null) keyText.text = skill.keyBinding;

                    if (iconImg != null && !string.IsNullOrEmpty(skill.iconPath))
                    {
                        Sprite spr = Resources.Load<Sprite>(skill.iconPath);
                        if (spr != null) iconImg.sprite = spr;
                    }

                    if (iconBtn != null)
                    {
                        iconBtn.onClick.AddListener(() => ShowSkillTooltip(skill, skillIndex));
                    }
                }
            }
        }

        // Item 5: Skill Tooltip verified info
        public void ShowSkillTooltip(HeroSkillDefinition skill, int skillIndex)
        {
            if (skill == null || skillTooltipPanel == null) return;

            skillTooltipPanel.SetActive(true);
            if (skillTooltipNameText != null) skillTooltipNameText.text = $"[{skill.keyBinding}] {skill.skillName}";
            if (skillTooltipCostText != null) skillTooltipCostText.text = $"Mana: {skill.manaCost}   |   Cooldown: {skill.cooldownSeconds:F1}s";
            if (skillTooltipFormulaText != null) skillTooltipFormulaText.text = $"Scaling: {skill.scalingFormula}";
            if (skillTooltipDetailsText != null) skillTooltipDetailsText.text = skill.description;

            if (showcase3DManager != null)
            {
                showcase3DManager.PlaySkillPreviewAnimation(skillIndex);
            }

            PlaySfx(skillInspectSfx);
        }

        public void HideSkillTooltip()
        {
            if (skillTooltipPanel != null)
            {
                skillTooltipPanel.SetActive(false);
            }
        }

        // Item 8: Confirming locks correct hero ID
        public void OnConfirmPickClicked()
        {
            if (_isActionLocked || _isLocalPlayerLockedIn || _selectedHero == null) return;

            // Verify hero is not locked by another player
            if (_lockedHeroIds.Contains(_selectedHero.heroId))
            {
                Debug.LogError($"[HeroPick] Hero {_selectedHero.heroName} was already locked by someone else!");
                return;
            }

            _isActionLocked = true;
            _isLocalPlayerLockedIn = true;
            _lockedHeroIds.Add(_selectedHero.heroId);

            if (_playerSlots.Count > 0)
            {
                _playerSlots[0].state = PlayerPickState.LockedIn;
                _playerSlots[0].selectedHeroId = _selectedHero.heroId;
                RefreshPlayerSlotsUI();
            }

            if (confirmPickButton != null) confirmPickButton.interactable = false;
            if (confirmButtonText != null) confirmButtonText.text = "LOCKED IN";

            PlaySfx(lockInSfx);
            PopulateHeroGrid(); // Gray out locked cards
        }

        /* === 07. RENDER PIPELINE, UI HUD & ANIMATED TIMERS === */
        private IEnumerator CountdownRoutine()
        {
            while (_timeRemaining > 0f)
            {
                _timeRemaining -= Time.deltaTime;
                int displaySeconds = Mathf.Max(0, Mathf.CeilToInt(_timeRemaining));

                if (countdownTimerText != null)
                {
                    countdownTimerText.text = displaySeconds.ToString();

                    // Item 11: Only play tick audio on SECOND BOUNDARY transitions
                    if (displaySeconds <= 5 && displaySeconds != _lastTickSecond)
                    {
                        _lastTickSecond = displaySeconds;
                        countdownTimerText.color = Color.red;
                        PlaySfx(timerTickSfx);
                    }
                    else if (displaySeconds > 5)
                    {
                        countdownTimerText.color = new Color(1.0f, 0.85f, 0.2f);
                    }
                }

                yield return null;
            }

            // Item 14: Timeout Auto-Selection of valid unpicked hero
            if (!_isLocalPlayerLockedIn)
            {
                ExecuteTimeoutAutoSelect();
            }

            yield return new WaitForSeconds(1.5f);

            // Item 13: Prevent multiple scene transitions
            TransitionToCombatArena();
        }

        // Item 14: Valid unpicked hero auto-select
        private void ExecuteTimeoutAutoSelect()
        {
            if (_selectedHero != null && !_lockedHeroIds.Contains(_selectedHero.heroId))
            {
                OnConfirmPickClicked();
                return;
            }

            // Search for first unpicked hero
            for (int i = 0; i < _allHeroes.Count; i++)
            {
                if (!_lockedHeroIds.Contains(_allHeroes[i].heroId))
                {
                    _selectedHero = _allHeroes[i];
                    OnConfirmPickClicked();
                    return;
                }
            }
        }

        // Item 15: Bot picking with duplicate checks and valid IDs
        private IEnumerator SimulateBotPicksRoutine()
        {
            yield return new WaitForSeconds(2.5f);

            for (int i = 1; i < _playerSlots.Count; i++)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 4.0f));

                // Find valid hero not locked
                HeroData botChoice = null;
                for (int h = 0; h < _allHeroes.Count; h++)
                {
                    if (!_lockedHeroIds.Contains(_allHeroes[h].heroId))
                    {
                        botChoice = _allHeroes[h];
                        break;
                    }
                }

                if (botChoice != null)
                {
                    _lockedHeroIds.Add(botChoice.heroId);
                    _playerSlots[i].selectedHeroId = botChoice.heroId;
                    _playerSlots[i].state = PlayerPickState.LockedIn;
                    RefreshPlayerSlotsUI();
                    PopulateHeroGrid(); // Refresh disabled states
                }
            }
        }

        private void RefreshPlayerSlotsUI()
        {
            if (playerSlotsContainer == null) return;

            for (int i = 0; i < _playerSlots.Count && i < playerSlotsContainer.childCount; i++)
            {
                var slot = _playerSlots[i];
                var slotTransform = playerSlotsContainer.GetChild(i);
                var texts = slotTransform.GetComponentsInChildren<Text>();

                if (texts.Length > 0)
                {
                    texts[0].text = slot.playerName;
                }

                if (texts.Length > 1)
                {
                    if (slot.state == PlayerPickState.LockedIn)
                    {
                        HeroData chosen = HeroDatabase.GetHeroById(slot.selectedHeroId);
                        texts[1].text = chosen != null ? $"Ready ({chosen.heroName})" : "Ready";
                        texts[1].color = new Color(0.2f, 1.0f, 0.3f);
                    }
                    else
                    {
                        texts[1].text = "Picking...";
                        texts[1].color = new Color(1.0f, 0.8f, 0.2f);
                    }
                }
            }
        }

        // Item 13: Prevent multiple scene transitions
        private void TransitionToCombatArena()
        {
            if (_hasTransitioned) return;
            _hasTransitioned = true;

            Debug.Log("[FOC] Hero Picking Phase complete! Loading OfflineGameScene.");
            SceneManager.LoadScene("OfflineGameScene");
        }

        // Item 12: Handle application pause and resume safely
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _pauseTimestamp = Time.realtimeSinceStartupAsDouble;
            }
            else
            {
                if (_pauseTimestamp > 0)
                {
                    double elapsed = Time.realtimeSinceStartupAsDouble - _pauseTimestamp;
                    _timeRemaining = Mathf.Max(0f, _timeRemaining - (float)elapsed);
                    _pauseTimestamp = 0;
                }
            }
        }

        /* === 08. INPUT CONTROLLER & AUDIO PLAYBACK === */
        private void PlaySfx(AudioClip clip)
        {
            if (sfxAudioSource != null && clip != null)
            {
                sfxAudioSource.PlayOneShot(clip, 0.85f);
            }
        }
    }
}
