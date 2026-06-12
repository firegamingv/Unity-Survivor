public enum AchievementTrigger
{
    KillsInRun,
    KillsTotal,
    SurviveSeconds,
    ReachLevel,
    RunsCompleted,
}

public class Achievement
{
    public string            ID;
    public string            Name;
    public string            Description;
    public AchievementTrigger Trigger;
    public int               Target;
    public bool              IsUnlocked;
}
