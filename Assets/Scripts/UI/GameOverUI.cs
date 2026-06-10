using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Écran Game Over — affiché quand le joueur meurt.
/// Écoute l'event PlayerDeathEvent.
/// </summary>
public class GameOverUI : MonoBehaviour, IEventListener<PlayerDeathEvent>
{
    // ─── Références ───────────────────────────────────────────────────────────
    [Header("Panel principal")]
    [SerializeField] private GameObject _panel;

    [Header("Statistiques de run")]
    [SerializeField] private TMP_Text   _timeText;
    [SerializeField] private TMP_Text   _killsText;
    [SerializeField] private TMP_Text   _levelText;

    [Header("Boutons")]
    [SerializeField] private Button     _restartButton;
    [SerializeField] private Button     _menuButton;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void OnEnable()  => EventBus<PlayerDeathEvent>.Subscribe(this);
    private void OnDisable() => EventBus<PlayerDeathEvent>.Unsubscribe(this);

    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);

        _restartButton?.onClick.AddListener(OnRestartClicked);
        _menuButton?.onClick.AddListener(OnMenuClicked);
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(PlayerDeathEvent e)
    {
        ShowGameOver();
    }

    // ─── Logique ──────────────────────────────────────────────────────────────
    private void ShowGameOver()
    {
        if (_panel != null) _panel.SetActive(true);

        // Statistiques de run
        if (GameManager.Instance != null)
        {
            float  t   = GameManager.Instance.RunTime;
            int    min = Mathf.FloorToInt(t / 60f);
            int    sec = Mathf.FloorToInt(t % 60f);

            if (_timeText  != null) _timeText.text  = $"Temps : {min:00}:{sec:00}";
            if (_killsText != null) _killsText.text = $"Kills : {GameManager.Instance.TotalKills}";
        }

        if (XPSystem.Instance != null && _levelText != null)
            _levelText.text = $"Niveau atteint : {XPSystem.Instance.Level}";
    }

    // ─── Boutons ──────────────────────────────────────────────────────────────
    private void OnRestartClicked() => GameManager.Instance?.RestartRun();
    private void OnMenuClicked()    => GameManager.Instance?.GoToMainMenu();
}
