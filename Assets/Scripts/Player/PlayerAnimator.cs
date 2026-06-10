using UnityEngine;

/// <summary>
/// Contrôle les animations du joueur selon son état.
/// Attacher sur le même GameObject que l'Animator (Player/Sprite).
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour,
    IEventListener<PlayerDamagedEvent>,
    IEventListener<PlayerDeathEvent>
{
    // ─── Références ───────────────────────────────────────────────────────────
    private Animator        _animator;
    private Rigidbody2D     _rb;

    // ─── Hash des paramètres (plus performant que des strings) ────────────────
    private static readonly int SPEED    = Animator.StringToHash("Speed");
    private static readonly int GET_HIT  = Animator.StringToHash("GetHit");
    private static readonly int DIE      = Animator.StringToHash("Die");

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        // Le Rigidbody2D est sur le parent (Player root)
        _rb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        EventBus<PlayerDamagedEvent>.Subscribe(this);
        EventBus<PlayerDeathEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus<PlayerDamagedEvent>.Unsubscribe(this);
        EventBus<PlayerDeathEvent>.Unsubscribe(this);
    }

    [Header("Lissage vitesse")]
    [SerializeField] private float _speedSmoothTime = 0.1f;
    private float _currentSpeed;
    private float _speedVelocity; // used by SmoothDamp

    private void Update()
    {
        if (_rb == null) return;

        // Lisse la vitesse pour éviter le flickering Idle↔Walk
        float targetSpeed = _rb.linearVelocity.magnitude;
        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedVelocity, _speedSmoothTime);
        _animator.SetFloat(SPEED, _currentSpeed);
    }

    // ─── Events ───────────────────────────────────────────────────────────────
    public void OnEvent(PlayerDamagedEvent e)
    {
        if (e.Damage > 0f)
            _animator.SetTrigger(GET_HIT);
    }

    public void OnEvent(PlayerDeathEvent e)
    {
        _animator.SetTrigger(DIE);
    }
}
