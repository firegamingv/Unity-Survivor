using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const string PREFS_KEY   = "EFRITY_Leaderboard";
    private const int    MAX_ENTRIES = 10;

    private LeaderboardData _data = new LeaderboardData();

    public IReadOnlyList<RunResult> Entries => _data.Entries;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);      // DontDestroyOnLoad exige un root GO
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AddRun(RunResult result)
    {
        _data.Entries.Add(result);
        _data.Entries.Sort((a, b) => b.Score.CompareTo(a.Score));
        if (_data.Entries.Count > MAX_ENTRIES)
            _data.Entries.RemoveRange(MAX_ENTRIES, _data.Entries.Count - MAX_ENTRIES);
        Save();
    }

    public void ClearAll()
    {
        _data = new LeaderboardData();
        PlayerPrefs.DeleteKey(PREFS_KEY);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string json = PlayerPrefs.GetString(PREFS_KEY, "");
        if (string.IsNullOrEmpty(json)) return;
        try   { _data = JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData(); }
        catch { _data = new LeaderboardData(); }
    }

    private void Save()
    {
        PlayerPrefs.SetString(PREFS_KEY, JsonUtility.ToJson(_data));
        PlayerPrefs.Save();
    }
}
