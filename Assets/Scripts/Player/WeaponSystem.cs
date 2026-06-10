using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système d'arme auto-aim / auto-fire du joueur.
/// À chaque cooldown, cherche l'ennemi le plus proche et tire un projectile.
/// Implémente IAttacker et IUpgradable.
/// </summary>
public class WeaponSystem : MonoBehaviour, IAttacker, IUpgradable
{
    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Arme de base")]
    [SerializeField] private WeaponData _weaponData;

    [Header("Debug")]
    [SerializeField] private bool _showRangeGizmo = true;

    // ─── Références ───────────────────────────────────────────────────────────
    private PlayerStats _stats;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private float              _cooldownTimer  = 0f;
    private readonly List<UpgradeData> _activeUpgrades = new List<UpgradeData>();

    // Masque de layer pour détecter les ennemis (configurer dans Unity)
    [SerializeField] private LayerMask _enemyLayer;

    // ─── IAttacker ────────────────────────────────────────────────────────────
    public float AttackDamage => _weaponData != null
        ? _weaponData.BaseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)
        : 0f;

    public float AttackSpeed  => _weaponData != null
        ? _weaponData.BaseAttackSpeed * (_stats != null ? _stats.AttackSpeedMultiplier : 1f)
        : 1f;

    public float AttackRange  => _weaponData != null ? _weaponData.BaseRange : 8f;

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

        // Direction vers la cible
        Vector2 dir = Vector2.zero;
        if (target is MonoBehaviour mb)
            dir = (mb.transform.position - transform.position).normalized;

        // Spawn 0.7u devant le joueur — la position est passée avant SetActive()
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.7f);
        var proj = ObjectPoolManager.Instance.Get<ProjectilePlayer>(_weaponData.ProjectilePrefab, spawnPos);
        if (proj == null) return;

        proj.Init(dir, AttackDamage);
    }

    // ─── IUpgradable ──────────────────────────────────────────────────────────
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        _activeUpgrades.Add(upgrade);
        // Les upgrades de type Weapon pourront modifier le comportement ici
    }

    public List<UpgradeData> GetActiveUpgrades() => _activeUpgrades;

    // ─── Auto-aim ─────────────────────────────────────────────────────────────
    /// <summary>Retourne l'IDamageable de l'ennemi le plus proche dans la portée.</summary>
    private IDamageable FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, AttackRange, _enemyLayer);
        if (hits.Length == 0) return null;

        IDamageable closest  = null;
        float       minDist  = float.MaxValue;

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

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!_showRangeGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
