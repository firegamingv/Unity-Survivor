/// <summary>
/// Contrat pour tout objet capable d'attaquer.
/// Implémenté par : WeaponSystem, EnemyMelee, EnemyRanged, BossController.
/// </summary>
public interface IAttacker
{
    float AttackDamage { get; }
    float AttackSpeed  { get; }
    float AttackRange  { get; }

    void Attack(IDamageable target);
}
