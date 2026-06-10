using UnityEngine;

/// <summary>
/// Gère l'XP et les montées de niveau.
/// Singleton — placé sur le GameObject "_Managers/XPSystem".
/// Formule : XP_requis(level) = BaseXP * level^1.4
/// </summary>
public class XPSystem : MonoBehaviour, IEventListener<XPGainedEvent>
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static XPSystem Instance { get; private set; }

    // ─── Stats ────────────────────────────────────────────────────────────────
    public float CurrentXP  { get; private set; }
    public float XPRequired { get; private set; }
    public int   Level      { get; private set; } = 1;

    // ─── Config ───────────────────────────────────────────────────────────────
    [SerializeField] private float _baseXP     = 100f;
    [SerializeField] private float _xpExponent = 1.4f;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()  => EventBus<XPGainedEvent>.Subscribe(this);
    private void OnDisable() => EventBus<XPGainedEvent>.Unsubscribe(this);

    private void Start()
    {
        Level      = 1;
        CurrentXP  = 0f;
        XPRequired = CalculateXPRequired(Level);

        // Utilise la config globale si disponible
        if (GameManager.Instance?.Config != null)
        {
            _baseXP     = GameManager.Instance.Config.BaseXP;
            _xpExponent = GameManager.Instance.Config.XPExponent;
        }
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(XPGainedEvent e) => AddXP(e.Amount);

    // ─── API ──────────────────────────────────────────────────────────────────
    /// <summary>Ajoute de l'XP et déclenche un level-up si nécessaire.</summary>
    public void AddXP(float amount)
    {
        if (amount <= 0f) return;

        CurrentXP += amount;

        while (CurrentXP >= XPRequired)
        {
            CurrentXP -= XPRequired;
            LevelUp();
        }
    }

    /// <summary>Pourcentage de remplissage de la barre XP (0..1).</summary>
    public float GetXPProgress() => XPRequired > 0f ? CurrentXP / XPRequired : 0f;

    // ─── Privé ────────────────────────────────────────────────────────────────
    private void LevelUp()
    {
        Level++;
        XPRequired = CalculateXPRequired(Level);
        GameManager.Instance?.TriggerLevelUp(Level);
    }

    private float CalculateXPRequired(int level)
    {
        return _baseXP * Mathf.Pow(level, _xpExponent);
    }
}
