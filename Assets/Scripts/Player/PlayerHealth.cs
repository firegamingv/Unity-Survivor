using UnityEngine;

/// <summary>
/// Gère les points de vie du joueur.
/// Publie PlayerDamagedEvent sur chaque coup, appelle GameManager.GameOver() à la mort.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    // ─── Références ───────────────────────────────────────────────────────────
    private PlayerStats _stats;

    // ─── IDamageable ──────────────────────────────────────────────────────────
    public float MaxHP     => _stats.MaxHP;
    public float CurrentHP { get; private set; }
    public bool  IsDead    => CurrentHP <= 0f;

    // ─── Invincibilité temporaire (i-frames) ──────────────────────────────────
    [Header("I-Frames")]
    [SerializeField] private float _iFrameDuration = 0.5f;
    private float _iFrameTimer = 0f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        CurrentHP = MaxHP;
    }

    private void Update()
    {
        if (_iFrameTimer > 0f)
            _iFrameTimer -= Time.deltaTime;
    }

    // ─── IDamageable ──────────────────────────────────────────────────────────
    public void TakeDamage(float amount, DamageType damageType = DamageType.Physical)
    {
        if (IsDead || _iFrameTimer > 0f) return;

        // Réduction par l'armure (sauf dégâts True)
        float actualDamage = damageType == DamageType.True
            ? amount
            : Mathf.Max(0f, amount - _stats.Armor);

        CurrentHP -= actualDamage;
        CurrentHP  = Mathf.Clamp(CurrentHP, 0f, MaxHP);

        _iFrameTimer = _iFrameDuration;

        // Publie l'event (HUD se met à jour)
        EventBus<PlayerDamagedEvent>.Publish(new PlayerDamagedEvent { Damage = actualDamage });

        if (IsDead) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        // Damage négatif = soin (le HUD l'interprète)
        EventBus<PlayerDamagedEvent>.Publish(new PlayerDamagedEvent { Damage = -amount });
    }

    public void Die()
    {
        GameManager.Instance?.GameOver();
    }
}
