using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Notification "Succès débloqué !" qui glisse depuis la droite.
/// S'ajoute dans le Canvas de jeu — écoute AchievementUnlockedEvent.
/// </summary>
public class AchievementToastUI : MonoBehaviour, IEventListener<AchievementUnlockedEvent>
{
    [SerializeField] private RectTransform _toastPanel;
    [SerializeField] private TMP_Text      _titleText;
    [SerializeField] private TMP_Text      _nameText;
    [SerializeField] private TMP_Text      _descText;

    private const float HIDDEN_X  = 450f;
    private const float SHOWN_X   = -20f;
    private const float ANIM_TIME = 0.35f;
    private const float HOLD_TIME = 3.2f;

    private readonly Queue<AchievementUnlockedEvent> _queue = new Queue<AchievementUnlockedEvent>();
    private bool _isShowing;

    private void OnEnable()  => EventBus<AchievementUnlockedEvent>.Subscribe(this);
    private void OnDisable() => EventBus<AchievementUnlockedEvent>.Unsubscribe(this);

    private void Start()
    {
        SetX(HIDDEN_X);
    }

    public void OnEvent(AchievementUnlockedEvent e)
    {
        _queue.Enqueue(e);
        if (!_isShowing) StartCoroutine(ShowNext());
    }

    private IEnumerator ShowNext()
    {
        while (_queue.Count > 0)
        {
            _isShowing = true;
            var e = _queue.Dequeue();

            if (_titleText != null) _titleText.text = "SUCCES DEBLOQUE !";
            if (_nameText  != null) _nameText.text  = e.Name;
            if (_descText  != null) _descText.text  = e.Description;

            yield return StartCoroutine(AnimateX(HIDDEN_X, SHOWN_X, ANIM_TIME));
            yield return new WaitForSecondsRealtime(HOLD_TIME);
            yield return StartCoroutine(AnimateX(SHOWN_X, HIDDEN_X, ANIM_TIME));
        }
        _isShowing = false;
    }

    private IEnumerator AnimateX(float from, float to, float duration)
    {
        if (_toastPanel == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetX(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration)));
            yield return null;
        }
        SetX(to);
    }

    private void SetX(float x)
    {
        if (_toastPanel == null) return;
        var pos = _toastPanel.anchoredPosition;
        pos.x = x;
        _toastPanel.anchoredPosition = pos;
    }
}
