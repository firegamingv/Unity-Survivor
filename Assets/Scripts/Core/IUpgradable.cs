using System.Collections.Generic;

/// <summary>
/// Contrat pour tout objet pouvant recevoir des modifications de stats via upgrades.
/// Implémenté par : PlayerStats, WeaponSystem.
/// </summary>
public interface IUpgradable
{
    void ApplyUpgrade(UpgradeData upgrade);
    List<UpgradeData> GetActiveUpgrades();
}
