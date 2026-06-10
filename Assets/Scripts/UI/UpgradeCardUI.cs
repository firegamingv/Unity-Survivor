using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Représente une carte d'upgrade dans le menu de level-up.
/// Créer un prefab depuis ce script et l'assigner dans UpgradeMenuUI.
///
/// Structure recommandée du prefab :
///   UpgradeCard (Button)
///   ├── Background (Image) ← couleur de rareté via SetRarityColor
///   ├── Icon (Image)
///   ├── Name (TMP_Text)
///   ├── Description (TMP_Text)
///   └── Rarity (TMP_Text)
/// </summary>
[RequireComponent(typeof(Button))]
public class UpgradeCardUI : MonoBehaviour
{
    // ─── Références (à lier dans le prefab) ───────────────────────────────────
    [SerializeField] private Image    _background;
    [SerializeField] private Image    _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _rarityText;

    // ─── Couleurs de rareté ───────────────────────────────────────────────────
    private static readonly Color COLOR_COMMON = new Color(0.54f, 0.54f, 0.54f); // #8A8A8A
    private static readonly Color COLOR_RARE   = new Color(0.18f, 0.43f, 0.64f); // #2E6DA4
    private static readonly Color COLOR_EPIC   = new Color(0.55f, 0.17f, 0.89f); // #8B2BE2

    // ─── Runtime ──────────────────────────────────────────────────────────────
    private UpgradeData _upgradeData;

    // ─── Setup ────────────────────────────────────────────────────────────────
    /// <summary>Initialise la carte avec les données d'un upgrade.</summary>
    public void Setup(UpgradeData data)
    {
        _upgradeData = data;

        if (_nameText        != null) _nameText.text        = data.UpgradeName;
        if (_descriptionText != null) _descriptionText.text = data.Description;
        if (_icon            != null && data.Icon != null)  _icon.sprite = data.Icon;
        if (_rarityText      != null) _rarityText.text      = data.Rarity.ToString();

        SetRarityColor(data.Rarity);

        // Lie le bouton
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnCardClicked);
    }

    // ─── Interaction ──────────────────────────────────────────────────────────
    private void OnCardClicked()
    {
        UpgradeManager.Instance?.ApplyUpgrade(_upgradeData);
    }

    // ─── Visuel ───────────────────────────────────────────────────────────────
    private void SetRarityColor(UpgradeRarity rarity)
    {
        if (_background == null) return;

        Color c = rarity switch
        {
            UpgradeRarity.Common => COLOR_COMMON,
            UpgradeRarity.Rare   => COLOR_RARE,
            UpgradeRarity.Epic   => COLOR_EPIC,
            _                    => COLOR_COMMON
        };

        _background.color = c;
    }
}
