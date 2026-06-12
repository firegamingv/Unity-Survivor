using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private const string KEY_UNLOCKED    = "EFRITY_AchUnlocked";
    private const string KEY_TOTAL_KILLS = "EFRITY_StatTotalKills";
    private const string KEY_TOTAL_RUNS  = "EFRITY_StatTotalRuns";

    private Achievement[] _defs;

    public Achievement[] Achievements => _defs;

    private HashSet<string> _unlocked      = new HashSet<string>();
    private int             _lifetimeKills;
    private int             _lifetimeRuns;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);      // DontDestroyOnLoad exige un root GO
        DontDestroyOnLoad(gameObject);
        BuildDefs();
        Load();
    }

    private void BuildDefs()
    {
        _defs = new Achievement[]
        {
            new Achievement { ID = "first_blood",    Name = "Premier Sang",   Description = "Tuer le premier ennemi.",   Trigger = AchievementTrigger.KillsInRun,     Target = 1    },
            new Achievement { ID = "centurion",      Name = "Centurion",      Description = "100 kills en un run.",      Trigger = AchievementTrigger.KillsInRun,     Target = 100  },
            new Achievement { ID = "exterminator",   Name = "Exterminateur",  Description = "500 kills en un run.",      Trigger = AchievementTrigger.KillsInRun,     Target = 500  },
            new Achievement { ID = "genocide",       Name = "Genocide",       Description = "1000 kills en un run.",     Trigger = AchievementTrigger.KillsInRun,     Target = 1000 },
            new Achievement { ID = "assassin",       Name = "Assassin",       Description = "5000 kills en un run.",     Trigger = AchievementTrigger.KillsInRun,     Target = 5000 },
            new Achievement { ID = "veteran",        Name = "Veteran",        Description = "5000 kills au total.",      Trigger = AchievementTrigger.KillsTotal,     Target = 5000 },
            new Achievement { ID = "survivor_2min",  Name = "Survivant",      Description = "Survivre 2 minutes.",       Trigger = AchievementTrigger.SurviveSeconds, Target = 120  },
            new Achievement { ID = "survivor_5min",  Name = "Marathonien",    Description = "Survivre 5 minutes.",       Trigger = AchievementTrigger.SurviveSeconds, Target = 300  },
            new Achievement { ID = "survivor_10min", Name = "Indestructible", Description = "Survivre 10 minutes.",      Trigger = AchievementTrigger.SurviveSeconds, Target = 600  },
            new Achievement { ID = "level5",         Name = "Apprenti",       Description = "Atteindre le niveau 5.",    Trigger = AchievementTrigger.ReachLevel,     Target = 5    },
            new Achievement { ID = "level10",        Name = "Expert",         Description = "Atteindre le niveau 10.",   Trigger = AchievementTrigger.ReachLevel,     Target = 10   },
            new Achievement { ID = "level20",        Name = "Maitre",         Description = "Atteindre le niveau 20.",   Trigger = AchievementTrigger.ReachLevel,     Target = 20   },
            new Achievement { ID = "run1",           Name = "Debutant",       Description = "Terminer un run.",          Trigger = AchievementTrigger.RunsCompleted,  Target = 1    },
            new Achievement { ID = "run10",          Name = "Habitue",        Description = "Terminer 10 runs.",         Trigger = AchievementTrigger.RunsCompleted,  Target = 10   },
            new Achievement { ID = "run50",          Name = "Acharne",        Description = "Terminer 50 runs.",         Trigger = AchievementTrigger.RunsCompleted,  Target = 50   },
        };
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        CheckAll(GameManager.Instance.TotalKills, GameManager.Instance.RunTime);
    }

    public void OnRunEnd()
    {
        int   kills = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
        float time  = GameManager.Instance != null ? GameManager.Instance.RunTime    : 0f;
        _lifetimeKills += kills;
        _lifetimeRuns++;
        CheckAll(kills, time);
        PlayerPrefs.SetInt(KEY_TOTAL_KILLS, _lifetimeKills);
        PlayerPrefs.SetInt(KEY_TOTAL_RUNS,  _lifetimeRuns);
        PlayerPrefs.Save();
    }

    public float GetProgress(Achievement ach)
    {
        if (ach.IsUnlocked) return 1f;
        int   kills = GameManager.Instance != null ? GameManager.Instance.TotalKills : 0;
        float time  = GameManager.Instance != null ? GameManager.Instance.RunTime    : 0f;
        int   level = XPSystem.Instance    != null ? XPSystem.Instance.Level         : 0;
        float current = 0f;
        switch (ach.Trigger)
        {
            case AchievementTrigger.KillsInRun:     current = kills;                        break;
            case AchievementTrigger.KillsTotal:     current = _lifetimeKills + kills;       break;
            case AchievementTrigger.SurviveSeconds: current = time;                         break;
            case AchievementTrigger.ReachLevel:     current = level;                        break;
            case AchievementTrigger.RunsCompleted:  current = _lifetimeRuns;                break;
        }
        return Mathf.Clamp01(current / ach.Target);
    }

    public void ClearAll()
    {
        _unlocked.Clear();
        _lifetimeKills = 0;
        _lifetimeRuns  = 0;
        foreach (Achievement ach in _defs) ach.IsUnlocked = false;
        PlayerPrefs.DeleteKey(KEY_UNLOCKED);
        PlayerPrefs.DeleteKey(KEY_TOTAL_KILLS);
        PlayerPrefs.DeleteKey(KEY_TOTAL_RUNS);
        PlayerPrefs.Save();
    }

    private void CheckAll(int killsThisRun, float runTime)
    {
        if (_defs == null) return;
        int level    = XPSystem.Instance != null ? XPSystem.Instance.Level : 0;
        int allKills = _lifetimeKills + killsThisRun;
        foreach (Achievement ach in _defs)
        {
            if (_unlocked.Contains(ach.ID)) continue;
            bool passed = false;
            switch (ach.Trigger)
            {
                case AchievementTrigger.KillsInRun:     passed = killsThisRun >= ach.Target; break;
                case AchievementTrigger.KillsTotal:     passed = allKills     >= ach.Target; break;
                case AchievementTrigger.SurviveSeconds: passed = runTime      >= ach.Target; break;
                case AchievementTrigger.ReachLevel:     passed = level        >= ach.Target; break;
                case AchievementTrigger.RunsCompleted:  passed = _lifetimeRuns >= ach.Target; break;
            }
            if (passed) Unlock(ach);
        }
    }

    private void Unlock(Achievement ach)
    {
        if (_unlocked.Contains(ach.ID)) return;
        ach.IsUnlocked = true;
        _unlocked.Add(ach.ID);
        Save();
        EventBus<AchievementUnlockedEvent>.Publish(new AchievementUnlockedEvent
        {
            Name        = ach.Name,
            Description = ach.Description,
        });
    }

    private void Load()
    {
        _lifetimeKills = PlayerPrefs.GetInt(KEY_TOTAL_KILLS, 0);
        _lifetimeRuns  = PlayerPrefs.GetInt(KEY_TOTAL_RUNS,  0);
        string raw = PlayerPrefs.GetString(KEY_UNLOCKED, "");
        if (string.IsNullOrEmpty(raw)) return;
        string[] ids = raw.Split(',');
        foreach (string id in ids)
        {
            string trimmed = id.Trim();
            if (!string.IsNullOrEmpty(trimmed)) _unlocked.Add(trimmed);
        }
        if (_defs == null) return;
        foreach (Achievement ach in _defs)
            ach.IsUnlocked = _unlocked.Contains(ach.ID);
    }

    private void Save()
    {
        PlayerPrefs.SetString(KEY_UNLOCKED, string.Join(",", _unlocked));
        PlayerPrefs.Save();
    }
}
