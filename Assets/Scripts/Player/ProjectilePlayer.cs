using UnityEngine;

/// <summary>
/// Projectile tiré par le joueur.
/// Inflige des dégâts Magiques — ignorent partiellement l'Armor ennemie.
/// Layer recommandé : "PlayerProjectile"
/// </summary>
public class ProjectilePlayer : ProjectileBase
{
    protected override void OnHit(IDamageable target)
    {
        target.TakeDamage(_damage, DamageType.Magic);
    }

    protected override void OnExpire()
    {
        // Rien de spécial — le projectile disparaît silencieusement
    }
}
