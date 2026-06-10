using UnityEngine;

/// <summary>
/// Gestionnaire audio centralisé.
/// Joue la musique de fond et les SFX via des AudioSources dédiées.
/// Singleton — placé sur "_Managers/AudioManager".
/// </summary>
public class AudioManager : MonoBehaviour,
    IEventListener<PlayerDamagedEvent>,
    IEventListener<LevelUpEvent>,
    IEventListener<EnemyKilledEvent>
{
    // ─── Singleton ────────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ─── Sources ──────────────────────────────────────────────────────────────
    [Header("Sources Audio")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    // ─── Clips ────────────────────────────────────────────────────────────────
    [Header("Musique")]
    [SerializeField] private AudioClip _bgmGame;

    [Header("SFX")]
    [SerializeField] private AudioClip _sfxPlayerHit;
    [SerializeField] private AudioClip _sfxEnemyDeath;
    [SerializeField] private AudioClip _sfxLevelUp;
    [SerializeField] private AudioClip _sfxUpgradePick;
    [SerializeField] private AudioClip _sfxPlayerShoot;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus<PlayerDamagedEvent>.Subscribe(this);
        EventBus<LevelUpEvent>.Subscribe(this);
        EventBus<EnemyKilledEvent>.Subscribe(this);
    }

    private void OnDisable()
    {
        EventBus<PlayerDamagedEvent>.Unsubscribe(this);
        EventBus<LevelUpEvent>.Unsubscribe(this);
        EventBus<EnemyKilledEvent>.Unsubscribe(this);
    }

    private void Start()
    {
        PlayBGM(_bgmGame);
    }

    // ─── IEventListener ───────────────────────────────────────────────────────
    public void OnEvent(PlayerDamagedEvent e)
    {
        if (e.Damage > 0f) PlaySFX(_sfxPlayerHit);
    }

    public void OnEvent(LevelUpEvent e)
    {
        PlaySFX(_sfxLevelUp);
    }

    public void OnEvent(EnemyKilledEvent e)
    {
        PlaySFX(_sfxEnemyDeath, 0.4f);
    }

    // ─── API ──────────────────────────────────────────────────────────────────
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || _sfxSource == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (_musicSource == null || clip == null) return;
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    public void StopBGM() => _musicSource?.Stop();

    public void SetMusicVolume(float v)
    {
        if (_musicSource != null) _musicSource.volume = Mathf.Clamp01(v);
    }

    public void SetSFXVolume(float v)
    {
        if (_sfxSource != null) _sfxSource.volume = Mathf.Clamp01(v);
    }
}
