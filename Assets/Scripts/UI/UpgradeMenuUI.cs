using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Menu affiché lors d'un level-up (timeScale = 0).
/// Génère dynamiquement 3 cartes depuis UpgradeManager.
/// </summary>
public class UpgradeMenuUI : MonoBehaviour, IEventListener<LevelUpEvent>
{
    // ─── Références ───────────────────────────────────────────────────────────
    [Header("Container des cartes (HorizontalLayoutGroup recommandé)")]
    [SerializeField] private Transform      _cardContainer;

    [Header("Prefab d'une carte upgrade")]
    [SerializeField] private GameObject     _cardPrefab;

    [Header("Panel principal à activer/désactiver")]
    [SerializeField] private GameObject     _panel;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private readonly List<GameObject> _spawnedCards = new List<GameObject>();

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void OnEnable()  => EventBus<LevelUpEvent>.Subscribe(this);
    private void OnDisable() => EventBus<LevelUpEvent>.Unsubscribe(this);

    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(LevelUpEvent e)
    {
        ShowUpgradeMenu();
    }

    // ─── Logique ──────────────────────────────────────────────────────────────
    private void ShowUpgradeMenu()
    {
        if (_panel != null) _panel.SetActive(true);

        foreach (var card in _spawnedCards) Destroy(card);
        _spawnedCards.Clear();

        // Si le prefab de carte n'est pas assigné → reprend le jeu automatiquement
        if (_cardPrefab == null)
        {
            Debug.LogWarning("[EFRITY] UpgradeMenuUI : _cardPrefab non assigné !\n" +
                             "Crée un prefab UpgradeCard et assigne-le dans l'Inspector du LevelUp_Panel.");
            Invoke(nameof(AutoResume), 0.5f);
            return;
        }

        List<UpgradeData> cards = UpgradeManager.Instance?.GetCurrentCards()
                                  ?? new List<UpgradeData>();

        foreach (var upgradeData in cards)
        {
            if (_cardContainer == null) continue;
            GameObject cardGO = Instantiate(_cardPrefab, _cardContainer);
            var cardUI = cardGO.GetComponent<UpgradeCardUI>();
            cardUI?.Setup(upgradeData);
            _spawnedCards.Add(cardGO);
        }
    }

    private void AutoResume()
    {
        if (_panel != null) _panel.SetActive(false);
        // Applique un upgrade aléatoire automatiquement
        var cards = UpgradeManager.Instance?.GetCurrentCards();
        if (cards != null && cards.Count > 0)
            UpgradeManager.Instance.ApplyUpgrade(cards[0]);
        else
            GameManager.Instance?.OnUpgradeChosen();
    }

    /// <summary>Masque le menu — appelé automatiquement après ApplyUpgrade → GameManager.OnUpgradeChosen.</summary>
    public void HideUpgradeMenu()
    {
        if (_panel != null) _panel.SetActive(false);
    }
}
