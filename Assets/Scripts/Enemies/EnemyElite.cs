using UnityEngine;

/// <summary>
/// Ennemi d'élite — variante renforcée de EnemyMelee.
/// Particularité : burst de vitesse quand PV tombent sous 30%.
/// </summary>
public class EnemyElite : EnemyMelee
{
    [Header("Elite — Burst")]
    [SerializeField] private float _burstSpeedMultiplier = 2.5f;
    [SerializeField] private float _burstHPThreshold     = 0.3f;  // 30%

    private bool _burstActive = false;

    // ─── Mouvement ────────────────────────────────────────────────────────────
    protected override void Move()
    {
        // Active le burst si PV < 30% et pas encore activé
        if (!_burstActive && CurrentHP / MaxHP < _burstHPThreshold)
        {
            _burstActive = true;
            // Flash jaune pour signaler le burst
            StartCoroutine(BurstFlash());
        }

        if (_playerTransform == null) return;

        Vector2 direction = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
        float   speed     = _data.MoveSpeed * (_burstActive ? _burstSpeedMultiplier : 1f);
        _rb.linearVelocity      = direction * speed;

        float sx = Mathf.Abs(transform.localScale.x);
        transform.localScale = direction.x > 0
            ? new Vector3( sx, transform.localScale.y, 1f)
            : new Vector3(-sx, transform.localScale.y, 1f);
    }

    // ─── Callbacks ────────────────────────────────────────────────────────────
    protected override void OnDamageReceived(float amount) => FlashDamage();

    public override void OnSpawn()
    {
        base.OnSpawn();
        _burstActive = false;
    }

    // ─── Flash burst ──────────────────────────────────────────────────────────
    private System.Collections.IEnumerator BurstFlash()
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;
        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
