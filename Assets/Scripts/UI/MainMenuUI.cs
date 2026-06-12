using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contrôleur du menu principal.
/// Gère les 4 panneaux : Accueil / Classement / Succès / Paramètres.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button _btnPlay;
    [SerializeField] private Button _btnLeaderboard;
    [SerializeField] private Button _btnAchievements;
    [SerializeField] private Button _btnSettings;
    [SerializeField] private Button _btnQuit;

    [Header("Panneaux")]
    [SerializeField] private GameObject _panelHome;
    [SerializeField] private GameObject _panelLeaderboard;
    [SerializeField] private GameObject _panelAchievements;
    [SerializeField] private GameObject _panelSettings;

    [Header("Classement")]
    [SerializeField] private Transform _leaderboardContent;
    [SerializeField] private TMP_Text  _leaderboardEmptyText;

    [Header("Succès")]
    [SerializeField] private Transform _achievementsContent;

    [Header("Paramètres")]
    [SerializeField] private Slider   _musicSlider;
    [SerializeField] private Slider   _sfxSlider;
    [SerializeField] private TMP_Text _musicLabel;
    [SerializeField] private TMP_Text _sfxLabel;

    private const string KEY_MUSIC = "EFRITY_MusicVol";
    private const string KEY_SFX   = "EFRITY_SFXVol";

    private GameObject[] _allPanels;
    private readonly List<GameObject> _lbRows  = new List<GameObject>();
    private readonly List<GameObject> _achRows = new List<GameObject>();

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Start()
    {
        _allPanels = new[] { _panelHome, _panelLeaderboard, _panelAchievements, _panelSettings };

        _btnPlay?.onClick.AddListener(OnPlay);
        _btnLeaderboard?.onClick.AddListener(() => ShowPanel(_panelLeaderboard));
        _btnAchievements?.onClick.AddListener(() => ShowPanel(_panelAchievements));
        _btnSettings?.onClick.AddListener(() => ShowPanel(_panelSettings));
        _btnQuit?.onClick.AddListener(OnQuit);

        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        float sfxVol   = PlayerPrefs.GetFloat(KEY_SFX,   1f);
        if (_musicSlider != null)
        {
            _musicSlider.value = musicVol;
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = sfxVol;
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }
        UpdateVolumeLabels(musicVol, sfxVol);
        AudioListener.volume = musicVol;

        ShowPanel(_panelHome);
    }

    // ─── Navigation ───────────────────────────────────────────────────────────
    private void ShowPanel(GameObject target)
    {
        foreach (var p in _allPanels)
            if (p != null) p.SetActive(p == target);

        if (target == _panelLeaderboard)  PopulateLeaderboard();
        if (target == _panelAchievements) PopulateAchievements();
    }

    // ─── Jouer ────────────────────────────────────────────────────────────────
    private void OnPlay()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─── Classement ───────────────────────────────────────────────────────────
    private void PopulateLeaderboard()
    {
        if (_leaderboardContent == null) return;
        ClearRows(_lbRows);

        var lb = LeaderboardManager.Instance;
        bool empty = lb == null || lb.Entries.Count == 0;
        if (_leaderboardEmptyText != null) _leaderboardEmptyText.gameObject.SetActive(empty);
        if (empty) return;

        for (int i = 0; i < lb.Entries.Count; i++)
        {
            var row = CreateLeaderboardRow(i + 1, lb.Entries[i]);
            row.transform.SetParent(_leaderboardContent, false);
            _lbRows.Add(row);
        }
    }

    private GameObject CreateLeaderboardRow(int rank, RunResult e)
    {
        var go = new GameObject($"Row_{rank}", typeof(RectTransform));
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;
        le.flexibleWidth   = 1f;

        var bg = go.AddComponent<Image>();
        bg.color = rank % 2 == 0
            ? new Color(0.10f, 0.10f, 0.16f, 0.9f)
            : new Color(0.07f, 0.07f, 0.12f, 0.9f);

        if (rank == 1) bg.color = new Color(0.22f, 0.18f, 0.05f, 0.95f);
        if (rank == 2) bg.color = new Color(0.16f, 0.16f, 0.18f, 0.95f);
        if (rank == 3) bg.color = new Color(0.18f, 0.10f, 0.05f, 0.95f);

        var textGO = new GameObject("Text", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        var rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(15, 0); rt.offsetMax = new Vector2(-15, 0);

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        string medal = rank == 1 ? "[OR]  " : rank == 2 ? "[ARG] " : rank == 3 ? "[BRZ] " : $"  #{rank,2} ";
        tmp.text      = $"{medal}  Score {e.Score:N0}   |   {e.FormattedTime}   |   {e.Kills} kills   |   Niv.{e.Level}   |   {e.Date}";
        tmp.fontSize  = 17f;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return go;
    }

    // ─── Succès ───────────────────────────────────────────────────────────────
    private void PopulateAchievements()
    {
        if (_achievementsContent == null) return;
        ClearRows(_achRows);

        var am = AchievementManager.Instance;
        if (am == null) return;

        foreach (var ach in am.Achievements)
        {
            var row = CreateAchievementRow(ach, am.GetProgress(ach));
            row.transform.SetParent(_achievementsContent, false);
            _achRows.Add(row);
        }
    }

    private GameObject CreateAchievementRow(Achievement ach, float progress)
    {
        var go = new GameObject(ach.ID, typeof(RectTransform));
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 72f;
        le.flexibleWidth   = 1f;

        var bg = go.AddComponent<Image>();
        bg.color = ach.IsUnlocked
            ? new Color(0.10f, 0.20f, 0.10f, 0.9f)
            : new Color(0.10f, 0.10f, 0.16f, 0.9f);

        // Icône
        var iconGO  = new GameObject("Icon", typeof(RectTransform));
        iconGO.transform.SetParent(go.transform, false);
        var iconRT  = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0.5f); iconRT.anchorMax = new Vector2(0, 0.5f);
        iconRT.pivot     = new Vector2(0, 0.5f);
        iconRT.anchoredPosition = new Vector2(15, 0);
        iconRT.sizeDelta = new Vector2(36, 36);
        var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        iconTMP.text      = ach.IsUnlocked ? "[OK]" : "[--]";
        iconTMP.fontSize  = 14;
        iconTMP.fontStyle = FontStyles.Bold;
        iconTMP.color     = ach.IsUnlocked ? new Color(0.3f, 1f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
        iconTMP.alignment = TextAlignmentOptions.Center;

        // Nom + description
        var infoGO  = new GameObject("Info", typeof(RectTransform));
        infoGO.transform.SetParent(go.transform, false);
        var infoRT  = infoGO.GetComponent<RectTransform>();
        infoRT.anchorMin = Vector2.zero; infoRT.anchorMax = Vector2.one;
        infoRT.offsetMin = new Vector2(60, 6); infoRT.offsetMax = new Vector2(-145, -6);
        var infoTMP = infoGO.AddComponent<TextMeshProUGUI>();
        string nameStr = ach.IsUnlocked ? ach.Name : "???";
        string descStr = ach.IsUnlocked ? ach.Description : "A debloquer...";
        infoTMP.text              = $"<b>{nameStr}</b>\n<size=13><color=#AAAAAA>{descStr}</color></size>";
        infoTMP.fontSize          = 17;
        infoTMP.color             = Color.white;
        infoTMP.enableWordWrapping = true;
        infoTMP.alignment         = TextAlignmentOptions.MidlineLeft;

        // Barre de progression (sauf si débloqué)
        if (!ach.IsUnlocked)
        {
            var barBgGO = new GameObject("ProgressBg", typeof(RectTransform));
            barBgGO.transform.SetParent(go.transform, false);
            var barBgRT = barBgGO.GetComponent<RectTransform>();
            barBgRT.anchorMin = new Vector2(1, 0.5f); barBgRT.anchorMax = new Vector2(1, 0.5f);
            barBgRT.pivot     = new Vector2(1, 0.5f);
            barBgRT.anchoredPosition = new Vector2(-15, 0);
            barBgRT.sizeDelta = new Vector2(110, 14);
            barBgGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

            var fillGO = new GameObject("Fill", typeof(RectTransform));
            fillGO.transform.SetParent(barBgGO.transform, false);
            var fillRT = fillGO.GetComponent<RectTransform>();
            fillRT.anchorMin = new Vector2(0, 0); fillRT.anchorMax = new Vector2(progress, 1);
            fillRT.offsetMin = Vector2.zero;      fillRT.offsetMax  = Vector2.zero;
            fillGO.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f);

            var pctGO = new GameObject("Pct", typeof(RectTransform));
            pctGO.transform.SetParent(barBgGO.transform, false);
            var pctRT = pctGO.GetComponent<RectTransform>();
            pctRT.anchorMin = Vector2.zero; pctRT.anchorMax = Vector2.one;
            pctRT.offsetMin = Vector2.zero; pctRT.offsetMax = Vector2.zero;
            var pctTMP = pctGO.AddComponent<TextMeshProUGUI>();
            pctTMP.text      = $"{Mathf.RoundToInt(progress * 100)}%";
            pctTMP.fontSize  = 11;
            pctTMP.color     = Color.white;
            pctTMP.alignment = TextAlignmentOptions.Center;
        }

        return go;
    }

    // ─── Paramètres ───────────────────────────────────────────────────────────
    private void OnMusicChanged(float v)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC, v);
        AudioListener.volume = v;
        if (_musicLabel != null) _musicLabel.text = $"Musique : {Mathf.RoundToInt(v * 100)}%";
    }

    private void OnSfxChanged(float v)
    {
        PlayerPrefs.SetFloat(KEY_SFX, v);
        if (_sfxLabel != null) _sfxLabel.text = $"Effets : {Mathf.RoundToInt(v * 100)}%";
    }

    private void UpdateVolumeLabels(float music, float sfx)
    {
        if (_musicLabel != null) _musicLabel.text = $"Musique : {Mathf.RoundToInt(music * 100)}%";
        if (_sfxLabel   != null) _sfxLabel.text   = $"Effets : {Mathf.RoundToInt(sfx   * 100)}%";
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static void ClearRows(List<GameObject> rows)
    {
        foreach (var r in rows) if (r != null) Destroy(r);
        rows.Clear();
    }
}
