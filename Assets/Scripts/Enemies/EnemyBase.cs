using UnityEngine;

/// <summary>
/// Classe abstraite — racine de tous les ennemis.
/// Implémente IDamageable et IPoolable.
/// Les sous-classes définissent Move() et AttackBehavior().
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    // ─── Données ──────────────────────────────────────────────────────────────
    [SerializeField] protected EnemyData _data;
    public EnemyData Data => _data;

    // ─── Références runtime ───────────────────────────────────────────────────
    protected Transform    _playerTransform;
    protected Rigidbody2D  _rb;
    private   SpriteRenderer _spriteRenderer;

    // ─── IDamageable ──────────────────────────────────────────────────────────
    public float MaxHP     => _data != null ? _data.MaxHP : 1f;
    public float CurrentHP { get; protected set; }
    public bool  IsDead    => CurrentHP <= 0f;

    // ─── Attaque ──────────────────────────────────────────────────────────────
    protected float _attackCooldownTimer = 0f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _rb.gravityScale   = 0f;
        _rb.freezeRotation = true;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (_playerTransform == null) return;

        _attackCooldownTimer -= Time.deltaTime;

        Move();

        if (_attackCooldownTimer <= 0f)
        {
            AttackBehavior();
        }
    }

    // ─── IDamageable ──────────────────────────────────────────────────────────
    public void TakeDamage(float amount, DamageType damageType = DamageType.Physical)
    {
        if (IsDead) return;

        // Pas d'armure ennemie de base — peut être surchargée dans les sous-classes
        float actualDamage = Mathf.Max(0f, amount);
        CurrentHP -= actualDamage;

        OnDamageReceived(actualDamage);

        if (IsDead) Die();
    }

    public void Heal(float amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
    }

    public void Die()
    {
        // Publie l'event (XPSystem, EnemyManager, GameManager l'écoutent)
        EventBus<EnemyKilledEvent>.Publish(new EnemyKilledEvent
        {
            EnemyDataRef = _data,
            Position     = transform.position
        });

        GameManager.Instance?.RegisterKill();

        // Drop XP
        SpawnXPOrb();

        OnDeath();

        // Retour au pool
        ObjectPoolManager.Instance?.Release(gameObject);
    }

    // ─── IPoolable ────────────────────────────────────────────────────────────
    public virtual void OnSpawn()
    {
        CurrentHP = MaxHP;
        _attackCooldownTimer = 0f;
        gameObject.SetActive(true);

        // Retrouve le joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player != null ? player.transform : null;
    }

    public virtual void OnDespawn()
    {
        gameObject.SetActive(false);
        _playerTransform = null;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private void SpawnXPOrb()
    {
        if (_data?.XPOrbPrefab == null) return;
        var orb = ObjectPoolManager.Instance?.Get<XPOrb>(_data.XPOrbPrefab);
        if (orb != null)
        {
            orb.transform.position = transform.position;
            orb.SetXPValue(_data.XPReward);
        }
    }

    /// <summary>Flash rouge au coup reçu.</summary>
    protected void FlashDamage()
    {
        if (_spriteRenderer != null)
            StartCoroutine(DamageFlashRoutine());
    }

    private System.Collections.IEnumerator DamageFlashRoutine()
    {
        if (_spriteRenderer == null) yield break;
        Color original = _spriteRenderer.color;
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = original;
    }

    // ─── Abstraits ────────────────────────────────────────────────────────────
    /// <summary>Logique de déplacement spécifique à chaque type d'ennemi.</summary>
    protected abstract void Move();

    /// <summary>Comportement d'attaque — appelé quand le cooldown expire.</summary>
    protected abstract void AttackBehavior();

    /// <summary>Réaction visuelle/sonore à la réception de dégâts.</summary>
    protected abstract void OnDamageReceived(float amount);

    /// <summary>Effets à la mort (animations, sons) avant retour au pool.</summary>
    protected abstract void OnDeath();
}
