/// <summary>
/// Utilitaire statique — calcule les multiplicateurs de difficulté en fonction
/// du temps de run. Pas de MonoBehaviour, pas de scène à modifier.
///
/// Formules linéaires (les plus lisibles dans l'Inspector) :
///   multiplier = 1 + (RunTime / 60) * ScalePerMinute
///
/// Exemples avec les valeurs par défaut à 5 min / 10 min :
///   HP    : ×2.75 / ×4.5
///   XP    : ×1.60 / ×2.2
///   Spawn : ×2.00 / ×3.0   (intervalle divisé)
///   Max   : ×1.75 / ×2.5   (ennemis max multipliés)
/// </summary>
public static class DifficultyScaler
{
    private static float Minutes =>
        GameManager.Instance != null ? GameManager.Instance.RunTime / 60f : 0f;

    private static GameConfig Cfg =>
        GameManager.Instance != null ? GameManager.Instance.Config : null;

    // ─── Multiplicateurs ──────────────────────────────────────────────────────

    public static float HpMultiplier =>
        1f + Minutes * (Cfg?.HpScalePerMinute ?? 0.35f);

    public static float XpMultiplier =>
        1f + Minutes * (Cfg?.XpScalePerMinute ?? 0.12f);

    /// <summary>Divise SpawnInterval par ce facteur pour accélérer les spawns.</summary>
    public static float SpawnMultiplier =>
        1f + Minutes * (Cfg?.SpawnRateScalePerMinute ?? 0.20f);

    /// <summary>Multiplie MaxEnemies de la phase active par ce facteur.</summary>
    public static float MaxEnemiesMultiplier =>
        1f + Minutes * (Cfg?.MaxEnemiesScalePerMinute ?? 0.15f);
}
