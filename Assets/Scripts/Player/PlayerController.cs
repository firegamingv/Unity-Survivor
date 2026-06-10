using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Déplace le joueur vers la position de la souris dans le monde.
/// Le personnage "suit" le curseur — s'arrête si la souris est très proche.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    // ─── Références ───────────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private PlayerStats _stats;
    private Camera      _cam;

    [Header("Comportement")]
    [Tooltip("Le joueur ne bouge pas si la souris est à moins de X unités")]
    [SerializeField] private float _deadZoneRadius = 0.8f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb    = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _cam   = Camera.main;

        // Setup Rigidbody2D pour un personnage 2D top-down
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 mouseScreen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector2 mouseWorld  = _cam.ScreenToWorldPoint(mouseScreen);
        Vector2 toMouse    = mouseWorld - (Vector2)transform.position;
        float   distance   = toMouse.magnitude;

        // Dead zone : arrête le personnage si la souris est trop proche
        if (distance < _deadZoneRadius)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toMouse.normalized;
        _rb.linearVelocity = direction * _stats.MoveSpeed;

        // Flip horizontal selon la direction
        float sx = transform.localScale.x;
        float absX = Mathf.Abs(sx);
        transform.localScale = direction.x >= 0
            ? new Vector3( absX, transform.localScale.y, 1f)
            : new Vector3(-absX, transform.localScale.y, 1f);
    }
}
