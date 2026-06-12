using System.Collections.Generic;
using UnityEngine;

// Achievement + AchievementTrigger sont définis dans Achievement.cs

// ─── Manager ──────────────────────────────────────────────────────────────────

public class AchievementManager : MonoBehaviour,
    IEventListener<EnemyKilledEvent>,
    IEventListener<LevelUpEvent>,
    IEventListener<WaveTimerTickEvent>
{
    public static AchievementManager Instance { get; private set; }

    private const string KEY_UNLOCKED    = "EFRITY_AchUnlocked";
    private const string KEY_TOTAL_KILLS = "EFRITY_StatTotalKills";
    private const string KEY_TOTAL_RUNS  = "EFRITY_StatTotalRuns";

    // ─── 15 achievements ──────────────────────────────────────────────────────
    private readonly Achievement[] _defs = new Achievement[]
    {
        new Achievement { ID="first_blood",    Name="Premier Sang",   Description="Tuer le premier ennemi.",         Trigger=AchievementTrigger.KillsInRun,    Target=1     },
        new Achievement { ID="centurion",      Name="Centurion",      Description="100 kills en un seul run.",       Trigger=AchievementTrigger.KillsInRun,    Target=100   },
        new Achievement { ID="exterminator",   Name="Exterminateur",  Description="500 kills en un seul run.",       Trigger=AchievementTrigger.KillsInRun,    Target=500   },
        new Achievement { ID="genocide",       Name="Genocide",       Description="1000 kills en un seul run.",      Trigger=AchievementTrigger.KillsInRun,    Target=1000  },
        new Achievement { ID="assassin",       Name="Assassin",       Description="5000 kills en un seul run.",      Trigger=AchievementTrigger.KillsInRun,    Target=5000  },
        new Achievement { ID="veteran",        Name="Veterant",       Description="5000 kills au total.",            Trigger=AchievementTrigger.KillsTotal,    Target=5000  },
        new Achievement { ID="survivor_2min",  Name="Survivant",      Description="Survivre 2 minutes.",             Trigger=AchievementTrigger.SurviveSeconds, Target=120  },
        new Achievement { ID="survivor_5min",  Name="Marathonien",    Description="Survivre 5 minutes.",             Trigger=AchievementTrigger.SurviveSeconds, Target=300  },
        new Achievement { ID="survivor_10min", Name="Indestructible", Description="Survivre 10 minutes.",            Trigger=AchievementTrigger.SurviveSeconds, Target=600  },
        new Achievement { ID="level5",         Name="Apprenti",       Description="Atteindre le niveau 5.",          Trigger=AchievementTrigger.ReachLevel,    Target=5     },
        new Achievement { ID="level10",        Name="Expert",         Description="Atteindre le niveau 10.",         Trigger=AchievementTrigger.ReachLevel,    Target=10    },
        new Achievement { ID="level20",        Name="Maitre",         Description="Atteindre le niveau 20.",         Trigger=AchievementTrigger.ReachLevel,    Target=20    },
        new Achievement { ID="run1",           Name="Debutant",       Description="Terminer un run.",                Trigger=AchievementTrigger.RunsCompleted, Target=1     },
        new Achievement { ID="run10",          Name="Habitue",        Description="Terminer 10 runs.",               Trigger=AchievementTrigger.RunsCompleted, Target=10    },
        new Achievement { ID="run50",          Name="Acharne",        Description="Terminer 50 runs.",               Trigger=AchievementTrigger.RunsCompleted, Target=50    },
    };

    public IReadOnlyList<Achievement> Achievements => _defs;

    private HashSet<string> _unlocked    = new HashSet<string>();
    private int             _totalKills;
    private int             _totalRuns;
    private int             _killsThisRun;
    private float           _timeThisRun;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void OnEnable()
    {
        EventBus<EnemyKilledEvent>.Subscribe(this);
        EventBus<LevelUpEvent>.Subscribe(this);
        EventBus<WaveTimerTickEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus<EnemyKilledEvent>.Unsubscribe(this);
        EventBus<LevelUpEvent>.Unsubscribe(this);
        EventBus<WaveTimerTickEvent>.Unsubscribe(this);
    }

    // ─── Events ───────────────────────────────────────────────────────────────
    public void OnEvent(EnemyKilledEvent e)
    {
        _killsThisRun++;
        _totalKills++;
        CheckAll();
    }

    public void OnEvent(LevelUpEvent e)
    {
        CheckAll();
    }

    public void OnEvent(WaveTimerTickEvent e)
    {
        _timeThisRun = e.CurrentTime;
        // Only check survival milestones (avoids per-frame CheckAll cost)
        CheckSurvival();
    }

    /// <summary>Appelé par GameManager lors d'un game over pour comptabiliser le run.</summary>
    public void OnRunEnd()
    {
        _totalRuns++;
        CheckAll();
        PlayerPrefs.SetInt(KEY_TOTAL_KILLS, _totalKills);
        PlayerPrefs.SetInt(KEY_TOTAL_RUNS,  _totalRuns);
        PlayerPrefs.Save();
        _killsThisRun = 0;
        _timeThisRun  = 0f;
    }

    // ─── Progress ─────────────────────────────────────────────────────────────
    public float GetProgress(Achievement ach)
    {
        if (ach.IsUnlocked) return 1f;
        float current = 0f;
        switch (ach.Trigger)
        {
            case AchievementTrigger.KillsInRun:     current = _killsThisRun; break;
            case AchievementTrigger.KillsTotal:     current = _totalKills;   break;
            case AchievementTrigger.SurviveSeconds: current = _timeThisRun;  break;
            case AchievementTrigger.ReachLevel:     current = XPSystem.Instance != null ? XPSystem.Instance.Level : 0; break;
            case AchievementTrigger.RunsCompleted:  current = _totalRuns;    break;
        }
        return Mathf.Clamp01(current / ach.Target);
    }

    // ─── Checks ───────────────────────────────────────────────────────────────
    private void CheckAll()
    {
        int currentLevel = XPSystem.Instance != null ? XPSystem.Instance.Level : 0;
        foreach (var ach in _defs)
        {
            if (_unlocked.Contains(ach.ID)) continue;
            bool passed = false;
            switch (ach.Trigger)
            {
                case AchievementTrigger.KillsInRun:     passed = _killsThisRun >= ach.Target; break;
                case AchievementTrigger.KillsTotal:     passed = _totalKills   >= ach.Target; break;
                case AchievementTrigger.SurviveSeconds: passed = _timeThisRun  >= ach.Target; break;
                case AchievementTrigger.ReachLevel:     passed = currentLevel  >= ach.Target; break;
                case AchievementTrigger.RunsCompleted:  passed = _totalRuns    >= ach.Target; break;
            }
            if (passed) Unlock(ach);
        }
    }

    private void CheckSurvival()
    {
        foreach (var ach in _defs)
        {
            if (_unlocked.Contains(ach.ID)) continue;
            if (ach.Trigger == AchievementTrigger.SurviveSeconds && _timeThisRun >= ach.Target)
                Unlock(ach);
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
            Description = ach.Description
        });
    }

    // ─── Persistence ──────────────────────────────────────────────────────────
    private void Load()
    {
        _totalKills = PlayerPrefs.GetInt(KEY_TOTAL_KILLS, 0);
        _totalRuns  = PlayerPrefs.GetInt(KEY_TOTAL_RUNS,  0);

        string json = PlayerPrefs.GetString(KEY_UNLOCKED, "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var list = JsonUtility.FromJson<StringList>(json);
                if (list?.Items != null)
                    foreach (var id in list.Items) { _unlocked.Add(id); }
            }
            catch { }
        }
        foreach (var ach in _defs)
            ach.IsUnlocked = _unlocked.Contains(ach.ID);
    }

    private void Save()
    {
        PlayerPrefs.SetString(KEY_UNLOCKED,
            JsonUtility.ToJson(new StringList { Items = new List<string>(_unlocked) }));
        PlayerPrefs.Save();
    }

    public void ClearAll()
    {
        _unlocked.Clear();
        _totalKills = _totalRuns = _killsThisRun = 0;
        _timeThisRun = 0f;
        foreach (var ach in _defs) ach.IsUnlocked = false;
        PlayerPrefs.DeleteKey(KEY_UNLOCKED);
        PlayerPrefs.DeleteKey(KEY_TOTAL_KILLS);
        PlayerPrefs.DeleteKey(KEY_TOTAL_RUNS);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class StringList { public List<string> Items; }
}
