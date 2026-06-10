using UnityEngine;

/// <summary>
/// Ennemi à distance — "Corrupts".
/// Comportement : strafe à distance fixe + tir de projectile lent.
/// </summary>
public class EnemyRanged : EnemyBase
{
    [Header("Ranged Config")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _preferredDistance = 6f;
    [SerializeField] private float _strafeSpeed       = 2f;
    [SerializeField] private float _projectileDamage  = 10f;

    private float _strafeDir = 1f;  // +1 ou -1 pour la direction du strafe

    // ─── Mouvement ────────────────────────────────────────────────────────────
    protected override void Move()
    {
        if (_playerTransform == null) return;

        Vector2 toPlayer = (Vector2)_playerTransform.position - (Vector2)transform.position;
        float   dist     = toPlayer.magnitude;

        Vector2 velocity;

        if (dist > _preferredDistance + 1f)
        {
            // Trop loin → s'approche
            velocity = toPlayer.normalized * _data.MoveSpeed;
        }
        else if (dist < _preferredDistance - 1f)
        {
            // Trop proche → recule
            velocity = -toPlayer.normalized * _data.MoveSpeed;
        }
        else
        {
            // Distance correcte → strafe perpendiculaire
            Vector2 perp = new Vector2(-toPlayer.normalized.y, toPlayer.normalized.x);
            velocity = perp * _strafeSpeed * _strafeDir;

            // Change la direction du strafe aléatoirement
            if (Random.value < 0.005f) _strafeDir *= -1f;
        }

        _rb.linearVelocity = velocity;

        // Flip sprite vers le joueur
        float sx = Mathf.Abs(transform.localScale.x);
        transform.localScale = toPlayer.x > 0
            ? new Vector3( sx, transform.localScale.y, 1f)
            : new Vector3(-sx, transform.localScale.y, 1f);
    }

    // ─── Attaque ──────────────────────────────────────────────────────────────
    protected override void AttackBehavior()
    {
        if (_playerTransform == null) return;
        if (_projectilePrefab == null) return;

        float dist = Vector2.Distance(transform.position, _playerTransform.position);
        if (dist > _preferredDistance + 3f) return;  // trop loin pour tirer

        Vector2 dir = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;

        // Spawn projectile ennemi depuis le pool
        var proj = ObjectPoolManager.Instance?.Get<ProjectileEnemy>(_projectilePrefab);
        if (proj != null)
        {
            proj.transform.position = transform.position;
            proj.Init(dir, _projectileDamage);
        }

        _attackCooldownTimer = _data.AttackCooldown;
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────
    protected override void OnDamageReceived(float amount) => FlashDamage();
    protected override void OnDeath() { }
}
