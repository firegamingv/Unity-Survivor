using UnityEngine;

/// <summary>
/// Gemme d'XP droppée à la mort d'un ennemi.
/// S'attire automatiquement vers le joueur quand il entre dans son rayon de collecte.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class XPOrb : MonoBehaviour, IPoolable
{
    // ─── Config ───────────────────────────────────────────────────────────────
    [SerializeField] private float _attractSpeed = 8f;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private float     _xpValue;
    private Transform _playerTransform;
    private PlayerStats _playerStats;
    private bool      _attracted = false;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.3f;
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        if (!_attracted) return;

        // Glisse vers le joueur
        transform.position = Vector2.MoveTowards(
            transform.position,
            _playerTransform.position,
            _attractSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Collecte si c'est le joueur
        if (!col.CompareTag("Player")) return;

        float bonus = _playerStats != null ? _playerStats.XPMultiplier : 1f;
        XPSystem.Instance?.AddXP(_xpValue * bonus);

        ObjectPoolManager.Instance?.Release(gameObject);
    }

    // ─── IPoolable ────────────────────────────────────────────────────────────
    public void OnSpawn()
    {
        _attracted = false;
        gameObject.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerStats     = player.GetComponent<PlayerStats>();
        }
    }

    public void OnDespawn()
    {
        _attracted       = false;
        _playerTransform = null;
        gameObject.SetActive(false);
    }

    // ─── API ──────────────────────────────────────────────────────────────────
    public void SetXPValue(float value) => _xpValue = value;

    /// <summary>Appelé depuis une zone de détection sur le joueur.</summary>
    public void Attract() => _attracted = true;
}
