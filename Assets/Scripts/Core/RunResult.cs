using UnityEngine;

[System.Serializable]
public class RunResult
{
    public float  Time;
    public int    Kills;
    public int    Level;
    public int    Score;
    public string Date;

    public static RunResult Create(float time, int kills, int level)
    {
        return new RunResult
        {
            Time  = time,
            Kills = kills,
            Level = level,
            Score = Mathf.RoundToInt(kills * 100 + level * 1000 + time * 10),
            Date  = System.DateTime.Now.ToString("dd/MM HH:mm")
        };
    }

    public string FormattedTime
    {
        get
        {
            int min = Mathf.FloorToInt(Time / 60f);
            int sec = Mathf.FloorToInt(Time % 60f);
            return $"{min:00}:{sec:00}";
        }
    }
}

[System.Serializable]
public class LeaderboardData
{
    public System.Collections.Generic.List<RunResult> Entries =
        new System.Collections.Generic.List<RunResult>();
}
