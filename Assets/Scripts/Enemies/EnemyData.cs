using UnityEngine;

/// <summary>
/// Données statiques d'un type d'ennemi. Créer via :
/// Assets → clic droit → Create → EFRITY/Enemies/EnemyData
///
/// Types prévus dans l'architecture :
///   "Bits"    — Fodder melee,  30 PV,   5 dmg
///   "Corrupts"— Ranged,        50 PV,  10 dmg
///   "Élite"   — Melee+,       120 PV,  12 dmg
///   "Gardien" — Boss,        2000 PV,  25 dmg
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "EFRITY/Enemies/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identité")]
    public string EnemyName  = "Ennemi";

    [Header("Stats")]
    public float  MaxHP      = 30f;
    public float  MoveSpeed  = 3f;
    public float  Damage     = 5f;
    public float  XPReward   = 10f;

    [Header("Comportement")]
    [Tooltip("Rayon d'attaque corps à corps (0 = pas de mêlée)")]
    public float  AttackRange = 0.6f;
    [Tooltip("Délai entre deux attaques en secondes")]
    public float  AttackCooldown = 1f;

    [Header("Prefab")]
    [Tooltip("Prefab de cet ennemi (doit avoir EnemyBase en composant)")]
    public GameObject Prefab;

    [Header("XP Orb")]
    [Tooltip("Prefab des gemmes XP droppées à la mort")]
    public GameObject XPOrbPrefab;
}
