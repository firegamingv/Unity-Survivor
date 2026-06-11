using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système d'arme auto-aim / auto-fire du joueur.
/// Gère le tir de base + modificateurs (multi-shot, burst-on-kill, orbital).
/// </summary>
public class WeaponSystem : MonoBehaviour, IAttacker, IUpgradable
{
    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Arme de base")]
    [SerializeField] private WeaponData _weaponData;

    [Header("Debug")]
    [SerializeField] private bool _showRangeGizmo = true;

    [SerializeField] private LayerMask _enemyLayer;

    // ─── Références ───────────────────────────────────────────────────────────
    private PlayerStats _stats;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private float _cooldownTimer = 0f;
    private int   _extraProjectiles = 0;
    private const float SPREAD_ANGLE = 20f; // degrés entre projectiles adjacents

    private readonly List<UpgradeData> _activeUpgrades = new List<UpgradeData>();

    // ─── IAttacker ────────────────────────────────────────────────────────────
    public float AttackDamage => _weaponData != null
        ? _weaponData.BaseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)
        : 0f;

    public float AttackSpeed => _weaponData != null
        ? _weaponData.BaseAttackSpeed * (_stats != null ? _stats.AttackSpeedMultiplier : 1f)
        : 1f;

    public float AttackRange => _weaponData != null ? _weaponData.BaseRange : 8f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;
        if (_weaponData == null || _weaponData.ProjectilePrefab == null) return;

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0f) return;

        IDamageable target = FindClosestEnemy();
        if (target == null) return;

        Attack(target);
        _cooldownTimer = 1f / AttackSpeed;
    }

    // ─── IAttacker ────────────────────────────────────────────────────────────
    public void Attack(IDamageable target)
    {
        if (target == null || ObjectPoolManager.Instance == null) return;

        Vector2 mainDir = Vector2.zero;
        if (target is MonoBehaviour mb)
            mainDir = (mb.transform.position - transform.position).normalized;

        int total = 1 + _extraProjectiles;
        for (int i = 0; i < total; i++)
        {
            float offset = (i - (total - 1) * 0.5f) * SPREAD_ANGLE;
            Vector2 dir = RotateVector(mainDir, offset);
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.7f);
            var proj = ObjectPoolManager.Instance.Get<ProjectilePlayer>(_weaponData.ProjectilePrefab, spawnPos);
            proj?.Init(dir, AttackDamage);
        }
    }

    // ─── IUpgradable ──────────────────────────────────────────────────────────
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        _activeUpgrades.Add(upgrade);

        switch (upgrade.WeaponModifier)
        {
            case WeaponModifierType.ExtraProjectile:
                _extraProjectiles += upgrade.WeaponModifierValue;
                break;

            case WeaponModifierType.BurstOnKill:
                var burst = GetComponent<KillBurstSystem>() ?? gameObject.AddComponent<KillBurstSystem>();
                burst.Setup(upgrade.WeaponModifierValue, _weaponData.ProjectilePrefab, this);
                break;

            case WeaponModifierType.OrbitalWeapon:
                var orbital = GetComponent<OrbitalWeaponSystem>() ?? gameObject.AddComponent<OrbitalWeaponSystem>();
                orbital.AddBlades(upgrade.WeaponModifierValue, this);
                break;
        }
    }

    public List<UpgradeData> GetActiveUpgrades() => _activeUpgrades;

    // ─── Auto-aim ─────────────────────────────────────────────────────────────
    private IDamageable FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, AttackRange, _enemyLayer);
        if (hits.Length == 0) return null;

        IDamageable closest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist && hit.TryGetComponent<IDamageable>(out var dmg) && !dmg.IsDead)
            {
                minDist = dist;
                closest = dmg;
            }
        }
        return closest;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!_showRangeGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
