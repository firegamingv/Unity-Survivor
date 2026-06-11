using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère le catalogue d'upgrades et la sélection de 3 cartes au level-up.
/// Singleton — placé sur "_Managers/UpgradeManager".
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static UpgradeManager Instance { get; private set; }

    // ─── Config ───────────────────────────────────────────────────────────────
    [Header("Catalogue complet des upgrades")]
    [SerializeField] private List<UpgradeData> _allUpgrades = new List<UpgradeData>();

    [Header("Nombre de cartes proposées")]
    [SerializeField] private int _cardCount = 3;

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private List<UpgradeData> _chosenCards = new List<UpgradeData>();
    private PlayerStats       _playerStats;
    private WeaponSystem      _weaponSystem;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerStats  = player.GetComponent<PlayerStats>();
            _weaponSystem = player.GetComponent<WeaponSystem>();
        }
    }

    // ─── API ──────────────────────────────────────────────────────────────────
    /// <summary>Tire les cartes pour ce level-up. Appelé par GameManager avant la publication de l'event.</summary>
    public void PrepareCards() => _chosenCards = DrawCards(_cardCount);

    /// <summary>Retourne les cartes tirées pour l'affichage dans l'UI.</summary>
    public List<UpgradeData> GetCurrentCards() => _chosenCards;

    /// <summary>Applique l'upgrade sélectionné par le joueur.</summary>
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        // Distribue aux systèmes concernés selon le type
        switch (upgrade.Type)
        {
            case UpgradeType.Stat:
                _playerStats?.ApplyUpgrade(upgrade);
                break;
            case UpgradeType.Weapon:
                _weaponSystem?.ApplyUpgrade(upgrade);
                _playerStats?.ApplyUpgrade(upgrade);  // peut aussi modifier des stats
                break;
            case UpgradeType.Passive:
                _playerStats?.ApplyUpgrade(upgrade);
                break;
        }

        GameManager.Instance?.OnUpgradeChosen();
    }

    // ─── Tirage des cartes ────────────────────────────────────────────────────
    /// <summary>
    /// Tire N cartes depuis le catalogue, en tenant compte de la rareté
    /// et du stat Luck du joueur.
    /// </summary>
    private List<UpgradeData> DrawCards(int count)
    {
        if (_allUpgrades.Count == 0) return new List<UpgradeData>();

        float luck = _playerStats != null ? _playerStats.Luck : 0f;

        // Poids par rareté — la Luck augmente les chances Rare/Epic
        float wCommon = Mathf.Max(0.1f, 0.60f - luck * 0.02f);
        float wRare   = 0.30f + luck * 0.01f;
        float wEpic   = Mathf.Min(0.60f, 0.10f + luck * 0.01f);

        // Filtre les upgrades déjà uniques sélectionnés
        var alreadyPicked = _playerStats != null
            ? _playerStats.GetActiveUpgrades()
            : new List<UpgradeData>();

        var available = new List<UpgradeData>(_allUpgrades);
        available.RemoveAll(u => u.IsUnique && alreadyPicked.Contains(u));

        var result = new List<UpgradeData>();
        var used   = new List<UpgradeData>();

        for (int i = 0; i < count; i++)
        {
            if (available.Count == 0) break;

            UpgradeData pick = WeightedRandom(available, used, wCommon, wRare, wEpic);
            if (pick != null)
            {
                result.Add(pick);
                used.Add(pick);
            }
        }

        return result;
    }

    private UpgradeData WeightedRandom(List<UpgradeData> pool, List<UpgradeData> exclude,
                                       float wCommon, float wRare, float wEpic)
    {
        // Sépare par rareté
        var commons = pool.FindAll(u => u.Rarity == UpgradeRarity.Common  && !exclude.Contains(u));
        var rares   = pool.FindAll(u => u.Rarity == UpgradeRarity.Rare    && !exclude.Contains(u));
        var epics   = pool.FindAll(u => u.Rarity == UpgradeRarity.Epic    && !exclude.Contains(u));

        // Roll de rareté
        float roll = Random.value;
        List<UpgradeData> bucket;

        if (roll < wEpic && epics.Count > 0)        bucket = epics;
        else if (roll < wEpic + wRare && rares.Count > 0) bucket = rares;
        else if (commons.Count > 0)                  bucket = commons;
        else                                         bucket = pool.FindAll(u => !exclude.Contains(u));

        if (bucket.Count == 0) return null;
        return bucket[Random.Range(0, bucket.Count)];
    }
}
