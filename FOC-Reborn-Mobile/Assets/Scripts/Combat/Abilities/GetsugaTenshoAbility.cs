/* === 01. SYSTEM / ENGINE CONFIG, CONSTANTS & SCHEMA === */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FOC.CombatSystems.Abilities
{
    [CreateAssetMenu(fileName = "GetsugaTenshoAbility", menuName = "FOC/Abilities/Getsuga Tenshou")]
    public class GetsugaTenshoAbility : AbilityBase
    {
        [Header("Getsuga Tenshou Tuning")]
        public int[] baseDamages = new int[] { 85, 135, 185, 235, 285 };
        public float agilityMultiplier = 2.0f; // JASS authentic formula: base + (AGI * 2.0)
        public float projectileSpeed = 22.0f;  // Unity world units per second
        public float projectileRadius = 1.8f;
        public float maxTravelDistance = 14.0f; // ~900 WC3 units

        [Header("Visual & SFX")]
        public GameObject projectilePrefab;
        public Color normalColor = new Color(0.2f, 0.7f, 1.0f, 1.0f); // Blue spirit energy
        public Color kuroiColor = new Color(0.9f, 0.1f, 0.1f, 1.0f);  // Crimson/Black Bankai energy

        private void OnEnable()
        {
            abilityId = "ichigo_q";
            abilityName = "Getsuga Tenshou";
            keySlot = "Q";
            castType = AbilityCastType.PointTarget;
            maxLevel = 5;
            manaCosts = new int[] { 65, 80, 95, 110, 125 };
            cooldownDurations = new float[] { 7.0f, 7.0f, 7.0f, 7.0f, 7.0f };
            castRange = 750f;
        }

        /* === 06. CORE GAMEPLAY, BUSINESS & TRANSACTION LOGIC === */
        public override void ExecuteEffect(CombatController caster, Vector3 targetPoint, CombatController targetUnit)
        {
            if (caster == null) return;

            // Trigger Spell Animation
            Animator anim = caster.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Spell");
            }

            // Direction calculation
            Vector3 origin = caster.transform.position + Vector3.up * 1.0f;
            Vector3 direction = (targetPoint - caster.transform.position);
            direction.y = 0;
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = caster.transform.forward;
            }
            direction.Normalize();

            // Face target point
            caster.transform.rotation = Quaternion.LookRotation(direction);

            // Compute Damage Value
            int levelIdx = Mathf.Clamp(currentLevel - 1, 0, baseDamages.Length - 1);
            int baseDmg = baseDamages[levelIdx];
            int heroAgi = caster.Stats.GetTotalAgility();
            int totalCalculatedDamage = Mathf.RoundToInt(baseDmg + (heroAgi * agilityMultiplier));

            // Bankai State Check (Empowers into Kuroi Getsuga)
            bool isBankai = false;
            // Check if caster has Bankai buff or aura active
            if (caster.Stats.bonusAttackSpeedPercent >= 80f)
            {
                isBankai = true;
                totalCalculatedDamage = Mathf.RoundToInt(totalCalculatedDamage * 1.5f);
            }

            // Spawn Projectile Entity
            SpawnGetsugaProjectile(caster, origin, direction, totalCalculatedDamage, isBankai);
        }

        private void SpawnGetsugaProjectile(
            CombatController caster,
            Vector3 origin,
            Vector3 direction,
            int damage,
            bool isBankai)
        {
            GameObject projObj;
            if (projectilePrefab != null)
            {
                projObj = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));
            }
            else
            {
                // Self-contained procedural projectile
                projObj = new GameObject("Getsuga_Crescent_Wave");
                projObj.transform.position = origin;
                projObj.transform.rotation = Quaternion.LookRotation(direction);

                // Add procedural crescent mesh
                MeshFilter mf = projObj.AddComponent<MeshFilter>();
                MeshRenderer mr = projObj.AddComponent<MeshRenderer>();
                mf.sharedMesh = CreateCrescentMesh();

                // Material styling
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Mobile/Particles/Additive"));
                mat.color = isBankai ? kuroiColor : normalColor;
                mr.sharedMaterial = mat;
            }

            // Attach Projectile Logic
            GetsugaProjectile projectileLogic = projObj.AddComponent<GetsugaProjectile>();
            projectileLogic.Initialize(caster, direction, projectileSpeed, projectileRadius, maxTravelDistance, damage);
        }

        private static Mesh CreateCrescentMesh()
        {
            Mesh m = new Mesh { name = "Procedural_Crescent" };
            // Simple diamond/crescent fan
            Vector3[] verts = new Vector3[]
            {
                new Vector3(-1.8f, 0f, 0f),
                new Vector3(0f, 0.4f, 1.2f),
                new Vector3(1.8f, 0f, 0f),
                new Vector3(0f, -0.2f, 0.2f),
                new Vector3(0f, 0.2f, 1.5f)
            };
            int[] tris = new int[]
            {
                0, 1, 3,
                1, 2, 3,
                0, 4, 1,
                1, 4, 2
            };
            m.vertices = verts;
            m.triangles = tris;
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    /* === 05. ENTITY COMPONENT SYSTEM & GAME STATE MANAGEMENT === */
    public class GetsugaProjectile : MonoBehaviour
    {
        private CombatController _caster;
        private Vector3 _direction;
        private float _speed;
        private float _radius;
        private float _maxDistance;
        private int _damage;
        private float _traveled = 0f;
        private readonly HashSet<CombatController> _hitUnits = new HashSet<CombatController>();

        public void Initialize(
            CombatController caster,
            Vector3 direction,
            float speed,
            float radius,
            float maxDistance,
            int damage)
        {
            _caster = caster;
            _direction = direction;
            _speed = speed;
            _radius = radius;
            _maxDistance = maxDistance;
            _damage = damage;
        }

        private void Update()
        {
            float step = _speed * Time.deltaTime;
            transform.position += _direction * step;
            _traveled += step;

            // Broadphase overlap check for piercing damage
            Collider[] colliders = Physics.OverlapSphere(transform.position, _radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                CombatController target = colliders[i].GetComponent<CombatController>();
                if (target != null && target != _caster && !target.Stats.IsDead && !_hitUnits.Contains(target))
                {
                    _hitUnits.Add(target);

                    // Compute mitigated damage per WC3 formula
                    DamageResult result = DamageCalculation.ComputeMagicalDamage(_damage);
                    target.Stats.TakeDirectDamage(result.mitigatedDamage, _caster.Stats);

                    // Trigger screen shake juice if ActionCameraController exists
                    ActionCameraController cam = Camera.main != null ? Camera.main.GetComponent<ActionCameraController>() : null;
                    if (cam != null)
                    {
                        cam.AddTrauma(0.25f);
                    }
                }
            }

            if (_traveled >= _maxDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
