using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lame orbitale qui tourne autour du joueur et blesse les ennemis au contact.
/// Créée dynamiquement par OrbitalWeaponSystem.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class OrbitalBlade : MonoBehaviour
{
    private Transform _player;
    private float     _angleDeg;
    private float     _damage;

    private const float ORBIT_RADIUS = 1.8f;
    private const float ORBIT_SPEED  = 150f;  // degrés / seconde
    private const float HIT_COOLDOWN = 1.0f;  // 1 touche / seconde / ennemi

    // Cooldown par ennemi pour éviter le spam de dégâts
    private readonly Dictionary<IDamageable, float> _hitTimers = new Dictionary<IDamageable, float>();

    // ─── Init ─────────────────────────────────────────────────────────────────
    public void Init(Transform player, float startAngleDeg, float damage)
    {
        _player   = player;
        _angleDeg = startAngleDeg;
        _damage   = damage;
    }

    public void SetDamage(float damage) => _damage = damage;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (_player == null) { gameObject.SetActive(false); return; }

        // Rotation orbitale
        _angleDeg += ORBIT_SPEED * Time.deltaTime;

        float rad = _angleDeg * Mathf.Deg2Rad;
        transform.position = _player.position + new Vector3(
            Mathf.Cos(rad) * ORBIT_RADIUS,
            Mathf.Sin(rad) * ORBIT_RADIUS, 0f);

        // La lame pointe dans sa direction de déplacement
        transform.rotation = Quaternion.Euler(0f, 0f, _angleDeg + 90f);

        // Décrémenter les cooldowns par ennemi
        var keys = new List<IDamageable>(_hitTimers.Keys);
        foreach (var k in keys)
        {
            _hitTimers[k] -= Time.deltaTime;
            if (_hitTimers[k] <= 0f) _hitTimers.Remove(k);
        }
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (!col.TryGetComponent<IDamageable>(out var target)) return;
        if (target.IsDead) return;
        if (_hitTimers.ContainsKey(target)) return;

        target.TakeDamage(_damage, DamageType.Physical);
        _hitTimers[target] = HIT_COOLDOWN;
    }
}
