using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contrôle le HUD en jeu.
/// Écoute les events et met à jour les éléments UI en temps réel.
/// Assigner les références dans l'Inspector sur le Canvas HUD.
/// </summary>
public class HUDController : MonoBehaviour,
    IEventListener<PlayerDamagedEvent>,
    IEventListener<LevelUpEvent>,
    IEventListener<WaveTimerTickEvent>,
    IEventListener<EnemyKilledEvent>
{
    // ─── Références UI (à assigner dans l'Inspector) ──────────────────────────
    [Header("Vie")]
    [SerializeField] private Slider   _healthBar;
    [SerializeField] private TMP_Text _healthText;

    [Header("XP")]
    [SerializeField] private Slider   _xpBar;
    [SerializeField] private TMP_Text _levelText;

    [Header("Timer")]
    [SerializeField] private TMP_Text _timerText;

    [Header("Kills")]
    [SerializeField] private TMP_Text _killsText;

    [Header("Flash de dégâts (Image plein écran)")]
    [SerializeField] private Image    _damageFlash;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private PlayerHealth _playerHealth;
    private float        _flashTimer = 0f;
    private const float  FLASH_DURATION = 0.15f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        EventBus<PlayerDamagedEvent>.Subscribe(this);
        EventBus<LevelUpEvent>.Subscribe(this);
        EventBus<WaveTimerTickEvent>.Subscribe(this);
        EventBus<EnemyKilledEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus<PlayerDamagedEvent>.Unsubscribe(this);
        EventBus<LevelUpEvent>.Unsubscribe(this);
        EventBus<WaveTimerTickEvent>.Unsubscribe(this);
        EventBus<EnemyKilledEvent>.Unsubscribe(this);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        _playerHealth = player?.GetComponent<PlayerHealth>();

        if (_damageFlash != null)
            _damageFlash.color = new Color(1f, 0f, 0f, 0f);

        RefreshAll();
    }

    private void Update()
    {
        // Fondu du flash de dégâts
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.unscaledDeltaTime;
            if (_damageFlash != null)
                _damageFlash.color = new Color(1f, 0f, 0f, _flashTimer / FLASH_DURATION * 0.4f);
        }

        // Barre de vie en temps réel (pour le soin progressif futur)
        UpdateHealthBar();
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(PlayerDamagedEvent e)
    {
        if (e.Damage > 0f) _flashTimer = FLASH_DURATION;
        UpdateHealthBar();
    }

    public void OnEvent(LevelUpEvent e)
    {
        if (_levelText != null) _levelText.text = $"Niv. {e.NewLevel}";
        UpdateXPBar();
    }

    public void OnEvent(WaveTimerTickEvent e)
    {
        UpdateTimer(e.CurrentTime);
        UpdateXPBar();
    }

    public void OnEvent(EnemyKilledEvent e)
    {
        if (_killsText != null && GameManager.Instance != null)
            _killsText.text = $"Kills : {GameManager.Instance.TotalKills}";
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private void RefreshAll()
    {
        UpdateHealthBar();
        UpdateXPBar();
        if (_killsText != null) _killsText.text = "Kills : 0";
        if (_timerText  != null) _timerText.text = "00:00";
    }

    private void UpdateHealthBar()
    {
        if (_playerHealth == null) return;

        float ratio = _playerHealth.MaxHP > 0f
            ? _playerHealth.CurrentHP / _playerHealth.MaxHP
            : 0f;

        if (_healthBar != null) _healthBar.value = ratio;
        if (_healthText != null)
            _healthText.text = $"{Mathf.CeilToInt(_playerHealth.CurrentHP)} / {Mathf.CeilToInt(_playerHealth.MaxHP)}";
    }

    private void UpdateXPBar()
    {
        if (XPSystem.Instance == null) return;

        if (_xpBar  != null) _xpBar.value   = XPSystem.Instance.GetXPProgress();
        if (_levelText != null) _levelText.text = $"Niv. {XPSystem.Instance.Level}";
    }

    private void UpdateTimer(float seconds)
    {
        if (_timerText == null) return;
        int min = Mathf.FloorToInt(seconds / 60f);
        int sec = Mathf.FloorToInt(seconds % 60f);
        _timerText.text = $"{min:00}:{sec:00}";
    }
}
