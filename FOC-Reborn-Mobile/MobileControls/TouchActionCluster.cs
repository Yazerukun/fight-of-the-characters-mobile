/* === 08. INPUT CONTROLLER, EVENT LISTENERS & BOOTSTRAP === */
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FOC.MobileControls
{
    public class TouchActionCluster : MonoBehaviour
    {
        [Header("Virtual Joystick")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float joystickRadius = 75f;
        [SerializeField] private float deadZone = 0.12f;

        [Header("Action Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button dodgeButton;
        [SerializeField] private Button targetLockButton;

        [Header("Skill Buttons (Q, W, E, R)")]
        [SerializeField] private Button[] skillButtons;
        [SerializeField] private Image[] skillCooldownFills;
        [SerializeField] private Text[] skillCooldownTexts;

        // Input Values
        public Vector2 MoveVector { get; private set; }
        public event Action OnAttackPressed;
        public event Action OnDodgePressed;
        public event Action OnTargetLockPressed;
        public event Action<int, Vector2> OnSkillCast; // skillIndex, aimVector

        private bool _isJoystickActive = false;
        private Vector2 _joystickOrigin;
        private readonly float[] _skillCooldownRemaining = new float[5];
        private readonly float[] _skillMaxCooldown = new float[5] { 8f, 10f, 14f, 75f, 0f };

        private void Start()
        {
            if (attackButton != null) attackButton.onClick.AddListener(() => OnAttackPressed?.Invoke());
            if (dodgeButton != null) dodgeButton.onClick.AddListener(() => OnDodgePressed?.Invoke());
            if (targetLockButton != null) targetLockButton.onClick.AddListener(() => OnTargetLockPressed?.Invoke());

            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                if (skillButtons[i] != null)
                {
                    skillButtons[i].onClick.AddListener(() => TriggerSkill(index));
                }
            }
        }

        private void Update()
        {
            UpdateCooldowns();
        }

        public void TriggerSkill(int skillIndex)
        {
            if (skillIndex < 0 || skillIndex >= _skillCooldownRemaining.Length) return;
            if (_skillCooldownRemaining[skillIndex] > 0.01f) return;

            // Start cooldown
            _skillCooldownRemaining[skillIndex] = _skillMaxCooldown[skillIndex];
            OnSkillCast?.Invoke(skillIndex, MoveVector.normalized);
        }

        private void UpdateCooldowns()
        {
            for (int i = 0; i < _skillCooldownRemaining.Length; i++)
            {
                if (_skillCooldownRemaining[i] > 0f)
                {
                    _skillCooldownRemaining[i] -= Time.deltaTime;
                    if (_skillCooldownRemaining[i] <= 0f)
                    {
                        _skillCooldownRemaining[i] = 0f;
                    }

                    if (i < skillCooldownFills.Length && skillCooldownFills[i] != null)
                    {
                        skillCooldownFills[i].fillAmount = _skillCooldownRemaining[i] / _skillMaxCooldown[i];
                    }

                    if (i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
                    {
                        skillCooldownTexts[i].text = _skillCooldownRemaining[i] > 0f ? Mathf.CeilToInt(_skillCooldownRemaining[i]).ToString() : "";
                    }

                    if (i < skillButtons.Length && skillButtons[i] != null)
                    {
                        skillButtons[i].interactable = (_skillCooldownRemaining[i] <= 0f);
                    }
                }
            }
        }

        // Joystick Event Callbacks
        public void OnJoystickPointerDown(BaseEventData eventData)
        {
            var pointerEvent = (PointerEventData)eventData;
            _isJoystickActive = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, pointerEvent.position, pointerEvent.pressEventCamera, out _joystickOrigin);
            OnJoystickDrag(eventData);
        }

        public void OnJoystickDrag(BaseEventData eventData)
        {
            if (!_isJoystickActive) return;

            var pointerEvent = (PointerEventData)eventData;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackground, pointerEvent.position, pointerEvent.pressEventCamera, out Vector2 localPos);

            Vector2 offset = localPos - _joystickOrigin;
            Vector2 clamped = Vector2.ClampMagnitude(offset, joystickRadius);

            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = clamped;
            }

            Vector2 normalized = clamped / joystickRadius;
            if (normalized.magnitude < deadZone)
            {
                MoveVector = Vector2.zero;
            }
            else
            {
                MoveVector = normalized;
            }
        }

        public void OnJoystickPointerUp(BaseEventData eventData)
        {
            _isJoystickActive = false;
            MoveVector = Vector2.zero;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
