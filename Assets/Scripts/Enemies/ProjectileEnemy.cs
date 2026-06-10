using UnityEngine;

/// <summary>
/// Projectile tiré par les ennemis ranged.
/// Inflige des dégâts Physiques au joueur.
/// Layer recommandé : "EnemyProjectile"
/// </summary>
public class ProjectileEnemy : ProjectileBase
{
    protected override void OnHit(IDamageable target)
    {
        // Ne blesse que le joueur (filtrage par layer dans WeaponSystem / collider layer matrix)
        target.TakeDamage(_damage, DamageType.Physical);
    }

    protected override void OnExpire()
    {
        // Disparaît silencieusement
    }
}
