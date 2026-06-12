using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button     _resumeButton;
    [SerializeField] private Button     _restartButton;
    [SerializeField] private Button     _menuButton;

    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        _resumeButton?.onClick.AddListener(Resume);
        _restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartRun());
        _menuButton?.onClick.AddListener(()    => GameManager.Instance?.GoToMainMenu());
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (Keyboard.current?.escapeKey.wasPressedThisFrame != true) return;

        var state = GameManager.Instance.CurrentState;
        if      (state == GameState.Playing) Pause();
        else if (state == GameState.Paused)  Resume();
    }

    private void Pause()
    {
        GameManager.Instance?.ChangeState(GameState.Paused);
        _panel?.SetActive(true);
    }

    private void Resume()
    {
        GameManager.Instance?.ChangeState(GameState.Playing);
        _panel?.SetActive(false);
    }
}
