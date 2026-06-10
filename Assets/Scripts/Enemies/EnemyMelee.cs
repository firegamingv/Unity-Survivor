using UnityEngine;

/// <summary>
/// Ennemi mêlée — "Bits" (Fodder).
/// Comportement : chase directe + dégâts au contact.
/// </summary>
public class EnemyMelee : EnemyBase
{
    // ─── Mouvement ────────────────────────────────────────────────────────────
    protected override void Move()
    {
        if (_playerTransform == null) return;

        Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = direction * _data.MoveSpeed;

        // Flip sprite
        if (direction.x != 0)
        {
            float sx = Mathf.Abs(transform.localScale.x);
            transform.localScale = direction.x > 0
                ? new Vector3( sx, transform.localScale.y, 1f)
                : new Vector3(-sx, transform.localScale.y, 1f);
        }
    }

    // ─── Attaque ──────────────────────────────────────────────────────────────
    protected override void AttackBehavior()
    {
        if (_playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (dist > _data.AttackRange) return;

        // Inflige des dégâts au joueur
        if (_playerTransform.TryGetComponent<IDamageable>(out var player))
        {
            player.TakeDamage(_data.Damage, DamageType.Physical);
            _attackCooldownTimer = _data.AttackCooldown;
        }
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────
    protected override void OnDamageReceived(float amount)
    {
        FlashDamage();
    }

    protected override void OnDeath()
    {
        // Sons / particules éventuels ici
    }
}
