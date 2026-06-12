using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton central. Orchestre les états globaux du jeu via une FSM.
/// Ne contient PAS de logique de gameplay — coordonne uniquement les managers.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─── État ─────────────────────────────────────────────────────────────────
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    // ─── Statistiques de run ──────────────────────────────────────────────────
    public float RunTime    { get; private set; }
    public int   TotalKills { get; private set; }

    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Configuration")]
    [SerializeField] private GameConfig _config;
    public GameConfig Config => _config;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Pas de DontDestroyOnLoad : jeu mono-scène, la scène se recharge à chaque restart.
    }

    private void Start()
    {
        ConfigureLayerCollisions();
        ChangeState(GameState.Playing);
    }

    /// <summary>Configure la collision matrix par code pour éviter les interactions non voulues.</summary>
    private void ConfigureLayerCollisions()
    {
        int defaultLayer    = LayerMask.NameToLayer("Default");
        int enemyLayer      = LayerMask.NameToLayer("Enemy");
        int playerProjLayer = LayerMask.NameToLayer("PlayerProjectile");
        int enemyProjLayer  = LayerMask.NameToLayer("EnemyProjectile");

        // Projectiles joueur ne touchent pas le joueur (Default)
        if (playerProjLayer >= 0 && defaultLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerProjLayer, defaultLayer, true);

        // Projectiles ennemis ne touchent pas les ennemis
        if (enemyProjLayer >= 0 && enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyProjLayer, enemyLayer, true);

        // Les ennemis se traversent entre eux (performance)
        if (enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        RunTime += Time.deltaTime;

        // Publie le tick de timer (HUD l'écoute)
        EventBus<WaveTimerTickEvent>.Publish(new WaveTimerTickEvent { CurrentTime = RunTime });
    }

    // ─── FSM ──────────────────────────────────────────────────────────────────
    /// <summary>Change l'état global du jeu.</summary>
    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.LevelUp:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    // ─── API publique ─────────────────────────────────────────────────────────

    /// <summary>Appelé par EnemyBase à la mort d'un ennemi.</summary>
    public void RegisterKill()
    {
        TotalKills++;
    }

    /// <summary>Appelé par XPSystem quand le joueur monte de niveau.</summary>
    public void TriggerLevelUp(int newLevel)
    {
        ChangeState(GameState.LevelUp);
        UpgradeManager.Instance?.PrepareCards();
        EventBus<LevelUpEvent>.Publish(new LevelUpEvent { NewLevel = newLevel });
    }

    /// <summary>Appelé par UpgradeMenuUI après le choix d'un upgrade.</summary>
    public void OnUpgradeChosen()
    {
        ChangeState(GameState.Playing);
    }

    /// <summary>Appelé par PlayerHealth quand le joueur meurt.</summary>
    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        ChangeState(GameState.GameOver);

        int level = XPSystem.Instance != null ? XPSystem.Instance.Level : 1;
        LeaderboardManager.Instance?.AddRun(RunResult.Create(RunTime, TotalKills, level));
        AchievementManager.Instance?.OnRunEnd();
    }

    /// <summary>Relance complète du niveau.</summary>
    public void RestartRun()
    {
        Time.timeScale = 1f;
        RunTime    = 0f;
        TotalKills = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Retour au menu principal (scène index 1).</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

// ─── Enum FSM ─────────────────────────────────────────────────────────────────
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    LevelUp,
    GameOver
}
