/// <summary>
/// Contrat pour tout objet pouvant recevoir des dégâts.
/// Implémenté par : PlayerHealth, EnemyBase (et sous-classes).
/// </summary>
public interface IDamageable
{
    float MaxHP     { get; }
    float CurrentHP { get; }
    bool  IsDead    { get; }

    void TakeDamage(float amount, DamageType damageType = DamageType.Physical);
    void Heal(float amount);
    void Die();
}
