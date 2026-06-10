using UnityEngine;

/// <summary>
/// Classe de base abstraite pour tous les projectiles.
/// Gère le mouvement, la durée de vie et le retour au pool.
/// Les sous-classes définissent OnHit() et OnExpire().
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public abstract class ProjectileBase : MonoBehaviour, IPoolable
{
    // ─── Données ──────────────────────────────────────────────────────────────
    [SerializeField] protected ProjectileData _data;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    protected Vector2 _direction;
    protected float   _damage;
    protected float   _lifetimeRemaining;
    private   bool    _released = false; // guard anti double-release

    private Rigidbody2D _rb;

    // ─── Unity ────────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale    = 0f;
        _rb.freezeRotation  = true;
        _rb.bodyType        = RigidbodyType2D.Kinematic;

        // Le collider est en trigger (pas de physique, juste détection)
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        // Déplacement
        transform.Translate(_direction * (_data.Speed * Time.deltaTime));

        // Durée de vie
        _lifetimeRemaining -= Time.deltaTime;
        if (_lifetimeRemaining <= 0f && !_released)
        {
            OnExpire();
            ReleaseToPool();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent<IDamageable>(out var target)) return;
        if (target.IsDead) return;

        OnHit(target);

        if (_data != null && _data.HitFXPrefab != null)
            Instantiate(_data.HitFXPrefab, transform.position, Quaternion.identity);

        ReleaseToPool();
    }

    // ─── IPoolable ────────────────────────────────────────────────────────────
    public virtual void OnSpawn()
    {
        _released          = false;
        _lifetimeRemaining = _data != null ? _data.Lifetime : 3f;
        gameObject.SetActive(true);
    }

    public virtual void OnDespawn()
    {
        _released  = true;
        _direction = Vector2.zero;
        gameObject.SetActive(false);
    }

    // ─── API d'initialisation ─────────────────────────────────────────────────
    /// <summary>Initialise le projectile avec une direction et des dégâts.</summary>
    public void Init(Vector2 direction, float damage)
    {
        _direction = direction.normalized;
        _damage    = damage;
        _lifetimeRemaining = _data != null ? _data.Lifetime : 3f;

        // Orientation visuelle du projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // ─── Release sécurisé ─────────────────────────────────────────────────────
    private void ReleaseToPool()
    {
        if (_released) return;
        _released = true;
        ObjectPoolManager.Instance?.Release(gameObject);
    }

    // ─── Abstraits ────────────────────────────────────────────────────────────
    /// <summary>Appelé quand le projectile touche un IDamageable.</summary>
    protected abstract void OnHit(IDamageable target);

    /// <summary>Appelé quand la durée de vie expire sans impact.</summary>
    protected abstract void OnExpire();
}
