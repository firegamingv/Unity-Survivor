using UnityEngine;

// ─── Enums ────────────────────────────────────────────────────────────────────

public enum UpgradeType   { Stat, Weapon, Passive }
public enum UpgradeRarity { Common, Rare, Epic }

// ─── UpgradeData ScriptableObject ─────────────────────────────────────────────

/// <summary>
/// Définit une amélioration disponible lors d'un level-up.
/// Créer via : Assets → clic droit → Create → EFRITY/Upgrade/UpgradeData
///
/// Exemples inclus dans l'architecture :
///   "Tir rapide"    → Weapon  / Common / AttackSpeed ×1.20
///   "Blindage léger"→ Stat    / Common / Armor +10, MaxHP +20
///   "Drain d'âme"   → Passive / Rare   / Soin 3% PV max / kill
///   "Tir double"    → Weapon  / Rare   / 2ème projectile parallèle
///   "Nova de données"→ Weapon  / Epic   / Explosion AoE au level-up
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "EFRITY/Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    [Header("Identité")]
    public string        UpgradeName;
    [TextArea(2, 4)]
    public string        Description;
    public Sprite        Icon;

    [Header("Classification")]
    public UpgradeType   Type;
    public UpgradeRarity Rarity;

    [Header("Modificateurs de stats")]
    [Tooltip("Liste des mods appliqués à PlayerStats ou WeaponSystem")]
    public StatModifier[] Modifiers;

    [Header("Règles")]
    [Tooltip("Si vrai, ne peut pas être sélectionné deux fois dans le même run")]
    public bool IsUnique = false;
}
