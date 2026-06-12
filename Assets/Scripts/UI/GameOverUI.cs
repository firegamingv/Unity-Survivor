using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour, IEventListener<PlayerDeathEvent>
{
    [Header("Panel principal")]
    [SerializeField] private GameObject _panel;

    [Header("Statistiques")]
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _killsText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _bestScoreText;

    [Header("Boutons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    private void OnEnable()  => EventBus<PlayerDeathEvent>.Subscribe(this);
    private void OnDisable() => EventBus<PlayerDeathEvent>.Unsubscribe(this);

    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        _restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartRun());
        _menuButton?.onClick.AddListener(()    => GameManager.Instance?.GoToMainMenu());
    }

    public void OnEvent(PlayerDeathEvent e) => ShowGameOver();

    private void ShowGameOver()
    {
        if (_panel != null) _panel.SetActive(true);

        if (GameManager.Instance == null) return;

        float t   = GameManager.Instance.RunTime;
        int   min = Mathf.FloorToInt(t / 60f);
        int   sec = Mathf.FloorToInt(t % 60f);
        int   kills = GameManager.Instance.TotalKills;
        int   level = XPSystem.Instance != null ? XPSystem.Instance.Level : 1;
        int   score = Mathf.RoundToInt(kills * 100 + level * 1000 + t * 10);

        if (_timeText  != null) _timeText.text  = $"Temps : {min:00}:{sec:00}";
        if (_killsText != null) _killsText.text = $"Kills : {kills}";
        if (_levelText != null) _levelText.text = $"Niveau atteint : {level}";
        if (_scoreText != null) _scoreText.text = $"Score : {score:N0}";

        // Meilleur score du classement
        if (_bestScoreText != null)
        {
            int best = 0;
            if (LeaderboardManager.Instance != null && LeaderboardManager.Instance.Entries.Count > 0)
                best = LeaderboardManager.Instance.Entries[0].Score;
            _bestScoreText.text = best > 0 ? $"Meilleur : {best:N0}" : "";
            _bestScoreText.color = score >= best && best > 0
                ? new Color(1f, 0.85f, 0f)
                : new Color(0.65f, 0.65f, 0.65f);
        }
    }
}
