using UnityEngine;

/// <summary>
/// Configuration globale de la partie. Créer via :
/// Assets → clic droit → Create → EFRITY/Config/GameConfig
/// Assigner à GameManager dans l'Inspector.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "EFRITY/Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("XP & Niveaux")]
    [Tooltip("XP de base requis pour le niveau 2")]
    public float BaseXP       = 100f;
    [Tooltip("Exposant de la courbe XP : XP_requis = BaseXP * level^Exponent")]
    public float XPExponent   = 1.4f;

    [Header("Boss — Paliers temporels (en secondes)")]
    [Tooltip("Ex : 300 = boss à 5 min, 600 = boss à 10 min")]
    public float[] BossTimers = { 300f, 600f };

    [Header("Spawn d'ennemis")]
    public float SpawnRadius  = 15f;   // distance de spawn autour du joueur
}
