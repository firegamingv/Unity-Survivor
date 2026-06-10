/// <summary>
/// Contrat pour tout objet géré par l'ObjectPoolManager.
/// Implémenté par : EnemyBase, ProjectileBase, XPOrb.
/// </summary>
public interface IPoolable
{
    /// <summary>Appelé quand l'objet est sorti du pool (SetActive true).</summary>
    void OnSpawn();

    /// <summary>Appelé avant le retour dans le pool — reset l'état complet.</summary>
    void OnDespawn();
}
