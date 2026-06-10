using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Composant joueur — contient toutes les stats modifiables par les upgrades.
/// Les valeurs exposées dans l'Inspector sont les bases de départ.
/// Les propriétés publiques reflètent les valeurs calculées (base + upgrades).
/// </summary>
public class PlayerStats : MonoBehaviour, IUpgradable
{
    // ─── Stats de base (Inspector) ────────────────────────────────────────────
    [Header("Stats de base")]
    [SerializeField] private float _baseMaxHP                = 100f;
    [SerializeField] private float _baseMoveSpeed            = 5f;
    [SerializeField] private float _baseArmor                = 0f;
    [SerializeField] private float _baseXPMultiplier         = 1f;
    [SerializeField] private float _basePickupRadius         = 2f;
    [SerializeField] private float _baseLuck                 = 0f;
    [SerializeField] private float _baseDamageMultiplier     = 1f;
    [SerializeField] private float _baseAttackSpeedMultiplier = 1f;

    // ─── Stats calculées (runtime) ────────────────────────────────────────────
    public float MaxHP                 { get; private set; }
    public float MoveSpeed             { get; private set; }
    public float Armor                 { get; private set; }
    public float XPMultiplier          { get; private set; }
    public float PickupRadius          { get; private set; }
    public float Luck                  { get; private set; }
    public float DamageMultiplier      { get; private set; }
    public float AttackSpeedMultiplier { get; private set; }

    // ─── Upgrades actifs ──────────────────────────────────────────────────────
    private readonly List<UpgradeData> _activeUpgrades = new List<UpgradeData>();

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake() => RecalculateStats();

    // ─── IUpgradable ──────────────────────────────────────────────────────────
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;
        if (upgrade.IsUnique && _activeUpgrades.Contains(upgrade)) return;

        _activeUpgrades.Add(upgrade);
        RecalculateStats();
    }

    public List<UpgradeData> GetActiveUpgrades() => _activeUpgrades;

    // ─── Calcul ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Recalcule toutes les stats depuis la base + upgrades.
    /// Ordre : 1) Somme des additifs → 2) Produit des multiplicatifs.
    /// </summary>
    private void RecalculateStats()
    {
        float addHP = 0, addSpd = 0, addArmor = 0, addXP = 0;
        float addPick = 0, addLuck = 0, addDmg = 0, addAtkSpd = 0;

        float multHP = 1, multSpd = 1, multArmor = 1, multXP = 1;
        float multPick = 1, multLuck = 1, multDmg = 1, multAtkSpd = 1;

        foreach (var upgrade in _activeUpgrades)
        {
            if (upgrade.Modifiers == null) continue;
            foreach (var mod in upgrade.Modifiers)
            {
                bool isAdd = mod.ModType == ModifierType.Additive;
                switch (mod.TargetStat)
                {
                    case StatType.MaxHP:
                        if (isAdd) addHP    += mod.Value; else multHP    *= 1f + mod.Value; break;
                    case StatType.MoveSpeed:
                        if (isAdd) addSpd   += mod.Value; else multSpd   *= 1f + mod.Value; break;
                    case StatType.Armor:
                        if (isAdd) addArmor += mod.Value; else multArmor *= 1f + mod.Value; break;
                    case StatType.XPMultiplier:
                        if (isAdd) addXP    += mod.Value; else multXP    *= 1f + mod.Value; break;
                    case StatType.PickupRadius:
                        if (isAdd) addPick  += mod.Value; else multPick  *= 1f + mod.Value; break;
                    case StatType.Luck:
                        if (isAdd) addLuck  += mod.Value; else multLuck  *= 1f + mod.Value; break;
                    case StatType.DamageMultiplier:
                        if (isAdd) addDmg   += mod.Value; else multDmg   *= 1f + mod.Value; break;
                    case StatType.AttackSpeedMultiplier:
                        if (isAdd) addAtkSpd+= mod.Value; else multAtkSpd*= 1f + mod.Value; break;
                }
            }
        }

        MaxHP                 = (_baseMaxHP                 + addHP)     * multHP;
        MoveSpeed             = (_baseMoveSpeed             + addSpd)    * multSpd;
        Armor                 = (_baseArmor                 + addArmor)  * multArmor;
        XPMultiplier          = (_baseXPMultiplier          + addXP)     * multXP;
        PickupRadius          = (_basePickupRadius          + addPick)   * multPick;
        Luck                  = (_baseLuck                  + addLuck)   * multLuck;
        DamageMultiplier      = (_baseDamageMultiplier      + addDmg)    * multDmg;
        AttackSpeedMultiplier = (_baseAttackSpeedMultiplier + addAtkSpd) * multAtkSpd;
    }
}
