using UnityEngine;

/// <summary>
/// Composant joueur — déclenche une explosion de projectiles à chaque kill.
/// Ajouté dynamiquement par WeaponSystem quand l'upgrade BurstOnKill est appliqué.
/// </summary>
public class KillBurstSystem : MonoBehaviour, IEventListener<EnemyKilledEvent>
{
    private int          _burstCount       = 0;
    private GameObject   _projectilePrefab;
    private WeaponSystem _weaponSystem;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void OnEnable()  => EventBus<EnemyKilledEvent>.Subscribe(this);
    private void OnDisable() => EventBus<EnemyKilledEvent>.Unsubscribe(this);

    // ─── API ──────────────────────────────────────────────────────────────────
    public void Setup(int burstCount, GameObject projectilePrefab, WeaponSystem weaponSystem)
    {
        _burstCount      += burstCount;
        _projectilePrefab = projectilePrefab;
        _weaponSystem     = weaponSystem;
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(EnemyKilledEvent e)
    {
        if (_burstCount <= 0 || _projectilePrefab == null) return;
        if (ObjectPoolManager.Instance == null) return;

        // 40% des dégâts normaux — beaucoup de projectiles compensent
        float damage = _weaponSystem != null ? _weaponSystem.AttackDamage * 0.4f : 10f;

        for (int i = 0; i < _burstCount; i++)
        {
            float angleRad = i * (Mathf.PI * 2f / _burstCount);
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            Vector3 spawnPos = e.Position + (Vector3)(dir * 0.3f);

            var proj = ObjectPoolManager.Instance.Get<ProjectilePlayer>(_projectilePrefab, spawnPos);
            proj?.Init(dir, damage);
        }
    }
}
