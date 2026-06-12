using UnityEngine;

/// <summary>
/// Tous les événements du jeu centralisés ici.
/// Structs = zéro allocation heap, plus performant que des classes.
/// Ajouter de nouveaux events ici au fur et à mesure des systèmes créés.
/// </summary>

// ─── Joueur ───────────────────────────────────────────────────────────────────

public struct PlayerDamagedEvent : IGameEvent
{
    public float Damage;
}

public struct PlayerDeathEvent : IGameEvent { }

// ─── XP & Niveaux ─────────────────────────────────────────────────────────────

public struct XPGainedEvent : IGameEvent
{
    public float Amount;
}

public struct LevelUpEvent : IGameEvent
{
    public int NewLevel;
}

// ─── Timer de run ─────────────────────────────────────────────────────────────

public struct WaveTimerTickEvent : IGameEvent
{
    public float CurrentTime;
}

// ─── Ennemis ──────────────────────────────────────────────────────────────────

public struct EnemyKilledEvent : IGameEvent
{
    public EnemyData  EnemyDataRef; // Données de l'ennemi tué (XP, type…)
    public Vector3    Position;     // Position de la mort (pour spawner XP orb)
}

// ─── Upgrades ─────────────────────────────────────────────────────────────────

public struct UpgradeChosenEvent : IGameEvent
{
    public UpgradeData Upgrade; // L'upgrade sélectionné par le joueur
}

// ─── Succès ───────────────────────────────────────────────────────────────────

public struct AchievementUnlockedEvent : IGameEvent
{
    public string Name;
    public string Description;
}

// ─── Boss ─────────────────────────────────────────────────────────────────────
// BossSpawnEvent → ajouté quand BossController sera implémenté
