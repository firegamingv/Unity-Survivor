using UnityEngine;

/// <summary>
/// Zone de collecte XP autour du joueur.
/// Placer comme enfant du Player, avec un CircleCollider2D en trigger.
/// Le rayon se met à jour automatiquement depuis PlayerStats.PickupRadius.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class XPPickupZone : MonoBehaviour
{
    private CircleCollider2D _col;
    private PlayerStats      _stats;

    private void Awake()
    {
        _col       = GetComponent<CircleCollider2D>();
        _col.isTrigger = true;
        _stats     = GetComponentInParent<PlayerStats>();
    }

    private void Update()
    {
        // Met à jour le rayon dynamiquement (affecté par les upgrades)
        if (_stats != null)
            _col.radius = _stats.PickupRadius;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<XPOrb>(out var orb))
            orb.Attract();
    }
}
