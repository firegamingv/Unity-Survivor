using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère le spawn continu des ennemis et la montée en difficulté par paliers.
/// Singleton — placé sur le GameObject "_Managers/EnemyManager" dans la scène.
/// </summary>
public class EnemyManager : MonoBehaviour, IEventListener<EnemyKilledEvent>
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static EnemyManager Instance { get; private set; }

    // ─── Phases de difficulté ─────────────────────────────────────────────────
    [System.Serializable]
    public class WavePhase
    {
        [Tooltip("Temps de début de cette phase (secondes)")]
        public float StartTime;
        [Tooltip("Délai entre chaque spawn (secondes)")]
        public float SpawnInterval = 1.5f;
        [Tooltip("Nombre max d'ennemis actifs simultanément")]
        public int   MaxEnemies    = 15;
        [Tooltip("Liste des EnemyData à spawner (choix aléatoire dans la liste)")]
        public List<EnemyData> EnemyPool;
    }

    [Header("Configuration des phases")]
    [SerializeField] private List<WavePhase> _phases = new List<WavePhase>();

    [Header("Spawn")]
    [SerializeField] private float _spawnRadius = 15f;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private float            _spawnTimer    = 0f;
    private int              _activeEnemies = 0;
    private WavePhase        _currentPhase;
    private Transform        _playerTransform;
    private List<EnemyData>  _spawnedTypes  = new List<EnemyData>();

    public int ActiveEnemies => _activeEnemies;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus<EnemyKilledEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilledEvent>.Unsubscribe(this);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = player != null ? player.transform : null;

        if (_phases.Count == 0)
            Debug.LogWarning("[EnemyManager] Aucune phase configurée dans l'Inspector !");
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (_playerTransform == null) return;

        UpdateCurrentPhase();

        if (_currentPhase == null) return;

        int   scaledMax      = Mathf.RoundToInt(_currentPhase.MaxEnemies * DifficultyScaler.MaxEnemiesMultiplier);
        float scaledInterval = _currentPhase.SpawnInterval / DifficultyScaler.SpawnMultiplier;

        if (_activeEnemies >= scaledMax) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = scaledInterval;
        }
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(EnemyKilledEvent e)
    {
        _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
    }

    // ─── Logique interne ──────────────────────────────────────────────────────
    private void UpdateCurrentPhase()
    {
        float t = GameManager.Instance.RunTime;
        WavePhase best = null;

        foreach (var phase in _phases)
        {
            if (t >= phase.StartTime)
                best = phase;
        }
        _currentPhase = best;
    }

    private void SpawnEnemy()
    {
        if (_currentPhase == null || _currentPhase.EnemyPool == null
            || _currentPhase.EnemyPool.Count == 0) return;

        // Choix aléatoire dans le pool de cette phase
        EnemyData data = _currentPhase.EnemyPool[Random.Range(0, _currentPhase.EnemyPool.Count)];
        if (data?.Prefab == null) return;

        // Position aléatoire autour du joueur, hors champ de caméra
        Vector2 spawnPos = GetSpawnPosition();

        var enemy = ObjectPoolManager.Instance?.Get<EnemyBase>(data.Prefab);
        if (enemy == null) return;

        enemy.transform.position = spawnPos;
        _activeEnemies++;
    }

    private Vector2 GetSpawnPosition()
    {
        float angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = GameManager.Instance?.Config != null
            ? GameManager.Instance.Config.SpawnRadius
            : _spawnRadius;

        return (Vector2)_playerTransform.position
               + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
