/* === 07. RENDER PIPELINE, UI HUD, MODALS & TOASTS === */
using UnityEngine;

namespace FOC.CombatSystems
{
    public class ActionCameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform lockedEnemyTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 4.5f, -6.5f);
        [SerializeField] private float followDamping = 8.0f;
        [SerializeField] private float rotationDamping = 10.0f;
        [SerializeField] private float lookAheadDistance = 2.0f;

        [Header("Collision & Occlusion")]
        [SerializeField] private LayerMask obstacleLayerMask;
        [SerializeField] private float collisionRadius = 0.35f;
        [SerializeField] private float minDistanceToPlayer = 2.0f;

        [Header("Trauma Screen Shake (Juice)")]
        [SerializeField] private float traumaDecayRate = 1.4f;
        [SerializeField] private float maxShakeAngle = 4.0f;
        [SerializeField] private float maxShakeOffset = 0.3f;

        private float _currentTrauma = 0f;
        private Vector3 _currentVelocity;

        private void LateUpdate()
        {
            if (playerTarget == null) return;

            UpdatePositionAndRotation();
            ApplyTraumaScreenShake();
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        public void SetLockedEnemy(Transform enemy)
        {
            lockedEnemyTarget = enemy;
        }

        public void AddTrauma(float amount)
        {
            _currentTrauma = Mathf.Clamp01(_currentTrauma + amount);
        }

        private void UpdatePositionAndRotation()
        {
            Vector3 focusPoint = playerTarget.position + Vector3.up * 1.5f;

            if (lockedEnemyTarget != null)
            {
                // Midpoint bias between player and locked enemy
                Vector3 midPoint = (playerTarget.position + lockedEnemyTarget.position) * 0.5f;
                focusPoint = Vector3.Lerp(focusPoint, midPoint + Vector3.up * 1.5f, 0.45f);
            }
            else
            {
                // Dynamic lookahead
                Vector3 playerForward = playerTarget.forward;
                focusPoint += playerForward * lookAheadDistance;
            }

            // Calculate ideal camera position
            Vector3 desiredPosition = playerTarget.position + (playerTarget.rotation * followOffset);

            // Sphere cast collision avoidance
            Vector3 direction = desiredPosition - focusPoint;
            float desiredDist = direction.magnitude;
            direction.Normalize();

            if (Physics.SphereCast(focusPoint, collisionRadius, direction, out RaycastHit hit, desiredDist, obstacleLayerMask))
            {
                float safeDist = Mathf.Max(hit.distance - 0.2f, minDistanceToPlayer);
                desiredPosition = focusPoint + (direction * safeDist);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, 1.0f / followDamping);

            // Smooth rotation towards focus point
            Quaternion targetRotation = Quaternion.LookRotation(focusPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationDamping);
        }

        private void ApplyTraumaScreenShake()
        {
            if (_currentTrauma > 0.001f)
            {
                // Quadratic decay trauma formula: Shake = Trauma^2
                float shakeIntensity = _currentTrauma * _currentTrauma;

                float offsetX = (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * 2f * maxShakeOffset * shakeIntensity;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * 2f * maxShakeOffset * shakeIntensity;
                float rotZ = (Mathf.PerlinNoise(Time.time * 25f, Time.time * 25f) - 0.5f) * 2f * maxShakeAngle * shakeIntensity;

                transform.position += new Vector3(offsetX, offsetY, 0f);
                transform.rotation *= Quaternion.Euler(0f, 0f, rotZ);

                _currentTrauma = Mathf.MoveTowards(_currentTrauma, 0f, Time.deltaTime * traumaDecayRate);
            }
        }
    }
}
