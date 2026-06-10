using System;
using UnityEngine;

// ─── Enums ────────────────────────────────────────────────────────────────────

/// <summary>Toutes les stats modifiables par les upgrades.</summary>
public enum StatType
{
    MaxHP,
    MoveSpeed,
    Armor,
    DamageMultiplier,
    AttackSpeedMultiplier,
    XPMultiplier,
    PickupRadius,
    Luck
}

/// <summary>Mode d'application du modificateur.</summary>
public enum ModifierType
{
    /// <summary>Ajout direct : stat = base + Σ(additifs)</summary>
    Additive,
    /// <summary>Multiplicateur : stat = base * Π(1 + mult)</summary>
    Multiplicative
}

// ─── StatModifier ─────────────────────────────────────────────────────────────

/// <summary>
/// Un modificateur de stat unique, sérialisable dans l'Inspector.
/// Contenu dans UpgradeData.Modifiers[].
///
/// Ordre de calcul dans PlayerStats :
///   1. base + Σ(tous les additifs)
///   2. résultat * Π(1 + tous les multiplicatifs)
/// </summary>
[Serializable]
public class StatModifier
{
    [Tooltip("Quelle stat est modifiée")]
    public StatType    TargetStat;

    [Tooltip("Additif = valeur plate, Multiplicatif = pourcentage (0.2 = +20%)")]
    public ModifierType ModType;

    [Tooltip("Valeur du modificateur")]
    public float       Value;
}
