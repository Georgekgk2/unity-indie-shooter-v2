using UnityEngine;
using System.Collections;

/// <summary>
/// Менеджер динамічної музики (Claude рекомендація)
/// Автоматично змінює музику залежно від ігрових ситуацій
/// </summary>
public class DynamicMusicManager : MonoBehaviour, 
    IEventHandler<CombatStartedEvent>,
    IEventHandler<CombatEndedEvent>,
    IEventHandler<PlayerDeathEvent>,
    IEventHandler<LevelCompletedEvent>,
    IEventHandler<PlayerHealthChangedEvent>
{
    [Header("Music Tracks")]
    [Tooltip("Спокійна музика для дослідження")]
    public AudioClip ambientMusic;
    [Tooltip("Бойова музика")]
    public AudioClip combatMusic;
    [Tooltip("Музика для меню")]
    public AudioClip menuMusic;
    [Tooltip("Музика перемоги")]
    public AudioClip victoryMusic;
    [Tooltip("Музика поразки")]
    public AudioClip defeatMusic;
    [Tooltip("Напружена музика (низьке здоров'я)")]
    public AudioClip tensionMusic;
    
    [Header("Audio Sources")]
    [Tooltip("Перший AudioSource для crossfade")]
    public AudioSource musicSource1;
    [Tooltip("Другий AudioSource для crossfade")]
    public AudioSource musicSource2;
    
    [Header("Transition Settings")]
    [Tooltip("Час переходу між треками")]
    public float transitionTime = 2f;
    [Tooltip("Затримка перед зміною музики в бою")]
    public float combatMusicDelay = 1f;
    [Tooltip("Час затухання при смерті")]
    public float deathFadeTime = 3f;
    [Tooltip("Поріг здоров'я для напруженої музики (%)")]
    [Range(0f, 0.5f)]
    public float tensionHealthThreshold = 0.25f;
    
    [Header("Combat Detection")]
    [Tooltip("Час без ворогів для повернення до ambient")]
    public float combatCooldownTime = 10f;
    [Tooltip("Радіус виявлення ворогів")]
    public float enemyDetectionRadius = 20f;
    [Tooltip("Шари ворогів")]
    public LayerMask enemyLayers = -1;
    
    // Приватні змінні
    private AudioSource currentSource;
    private AudioSource nextSource;
    private MusicState currentState = MusicState.Ambient;
    private MusicState previousState = MusicState.Ambient;
    private Coroutine transitionCoroutine;
    private bool isInCombat = false;
    private float combatTimer = 0f;
    private float lastEnemyDetectionTime = 0f;
    private bool isLowHealth = false;
    
    public enum MusicState
    {
        Menu,
        Ambient,
        Combat,
        Tension,
        Victory,
        Defeat,
        Silence
    }
    
    // Singleton
    public static DynamicMusicManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Ініціалізація
        if (musicSource1 == null || musicSource2 == null)
        {
            Debug.LogError("DynamicMusicManager: AudioSource не призначені!");
            CreateAudioSources();
        }
        
        currentSource = musicSource1;
        nextSource = musicSource2;
        
        // Підписка на події
        Events.Subscribe<CombatStartedEvent>(this);
        Events.Subscribe<CombatEndedEvent>(this);
        Events.Subscribe<PlayerDeathEvent>(this);
        Events.Subscribe<LevelCompletedEvent>(this);
        Events.Subscribe<PlayerHealthChangedEvent>(this);
        
        // Запускаємо ambient музику
        PlayMusic(MusicState.Ambient);
        
        Debug.Log("DynamicMusicManager: Ініціалізовано");
    }
    
    void Update()
    {
        // Автоматичне виявлення ворогів
        DetectNearbyEnemies();
        
        // Автоматичне повернення до ambient музики після бою
        if (isInCombat)
        {
            combatTimer += Time.deltaTime;
            
            // Якщо бою немає довго, повертаємося до ambient
            if (combatTimer > combatCooldownTime)
            {
                HandleEvent(new CombatEndedEvent(true, combatTimer));
            }
        }
        
        // Перевіряємо стан здоров'я для напруженої музики
        CheckHealthBasedMusic();
    }
    
    /// <summary>
    /// Виявляє ворогів поблизу для автоматичної зміни музики
    /// </summary>
    void DetectNearbyEnemies()
    {
        if (Time.time - lastEnemyDetectionTime < 1f) return; // Перевіряємо раз на секунду
        lastEnemyDetectionTime = Time.time;
        
        Collider[] enemies = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayers);
        bool enemiesNearby = enemies.Length > 0;
        
        if (enemiesNearby && !isInCombat)
        {
            // Знайшли ворогів - починаємо бій
            Events.Publish(new CombatStartedEvent(enemies.Length, transform.position));
        }
        else if (!enemiesNearby && isInCombat)
        {
            // Ворогів немає - можливо, бій закінчився
            combatTimer += Time.deltaTime;
        }
        else if (enemiesNearby && isInCombat)
        {
            // Вороги ще є - скидаємо таймер
            combatTimer = 0f;
        }
    }
    
    /// <summary>
    /// Перевіряє здоров'я гравця для напруженої музики
    /// </summary>
    void CheckHealthBasedMusic()
    {
        if (currentState == MusicState.Combat || currentState == MusicState.Tension)
        {
            if (isLowHealth && currentState != MusicState.Tension)
            {
                PlayMusic(MusicState.Tension);
            }
            else if (!isLowHealth && currentState == MusicState.Tension)
            {
                PlayMusic(MusicState.Combat);
            }
        }
    }
    
    // Обробники подій
    public void HandleEvent(CombatStartedEvent eventData)
    {
        isInCombat = true;
        combatTimer = 0f;
        
        Debug.Log($"DynamicMusicManager: Бій почався! Ворогів: {eventData.EnemyCount}");
        
        StartCoroutine(DelayedCombatMusic());
    }
    
    public void HandleEvent(CombatEndedEvent eventData)
    {
        isInCombat = false;
        combatTimer = 0f;
        
        Debug.Log($"DynamicMusicManager: Бій закінчився! Тривалість: {eventData.CombatDuration:F1}с");
        
        // Повертаємося до ambient або tension залежно від здоров'я
        MusicState targetState = isLowHealth ? MusicState.Tension : MusicState.Ambient;
        PlayMusic(targetState);
    }
    
    public void HandleEvent(PlayerDeathEvent eventData)
    {
        Debug.Log("DynamicMusicManager: Гравець помер");
        PlayMusic(MusicState.Defeat);
    }
    
    public void HandleEvent(LevelCompletedEvent eventData)
    {
        Debug.Log("DynamicMusicManager: Рівень завершено");
        PlayMusic(MusicState.Victory);
    }
    
    public void HandleEvent(PlayerHealthChangedEvent eventData)
    {
        float healthPercentage = eventData.CurrentHealth / eventData.MaxHealth;
        bool wasLowHealth = isLowHealth;
        isLowHealth = healthPercentage <= tensionHealthThreshold;
        
        // Якщо стан здоров'я змінився, оновлюємо музику
        if (wasLowHealth != isLowHealth)
        {
            Debug.Log($"DynamicMusicManager: Здоров'я {healthPercentage:P0} - {(isLowHealth ? "Низьке" : "Нормальне")}");
            
            if (!isInCombat)
            {
                MusicState targetState = isLowHealth ? MusicState.Tension : MusicState.Ambient;
                PlayMusic(targetState);
            }
        }
    }
    
    IEnumerator DelayedCombatMusic()
    {
        yield return new WaitForSeconds(combatMusicDelay);
        if (isInCombat) // Перевіряємо, чи ще в бою
        {
            MusicState targetState = isLowHealth ? MusicState.Tension : MusicState.Combat;
            PlayMusic(targetState);
        }
    }
    
    /// <summary>
    /// Змінює музику на вказаний стан
    /// </summary>
    public void PlayMusic(MusicState newState)
    {
        if (currentState == newState) return;
        
        previousState = currentState;
        currentState = newState;
        
        AudioClip newClip = GetClipForState(newState);
        
        Debug.Log($"DynamicMusicManager: Зміна музики {previousState} → {newState}");
        
        if (newClip != null)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
                
            transitionCoroutine = StartCoroutine(TransitionToMusic(newClip, newState));
        }
        else if (newState == MusicState.Silence)
        {
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
                
            transitionCoroutine = StartCoroutine(FadeOutMusic());
        }
    }
    
    /// <summary>
    /// Повертає аудіо кліп для стану
    /// </summary>
    AudioClip GetClipForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Menu: return menuMusic;
            case MusicState.Ambient: return ambientMusic;
            case MusicState.Combat: return combatMusic;
            case MusicState.Tension: return tensionMusic ?? combatMusic; // Fallback до combat
            case MusicState.Victory: return victoryMusic;
            case MusicState.Defeat: return defeatMusic;
            default: return null;
        }
    }
    
    /// <summary>
    /// Плавний перехід між треками
    /// </summary>
    IEnumerator TransitionToMusic(AudioClip newClip, MusicState newState)
    {
        // Налаштовуємо наступний source
        nextSource.clip = newClip;
        nextSource.volume = 0f;
        nextSource.loop = (newState != MusicState.Victory && newState != MusicState.Defeat);
        nextSource.Play();
        
        // Плавний перехід
        float elapsed = 0f;
        float startVolume = currentSource.volume;
        float targetVolume = GetVolumeForState(newState);
        
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionTime;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            nextSource.volume = Mathf.Lerp(0f, targetVolume, progress);
            
            yield return null;
        }
        
        // Завершуємо перехід
        currentSource.Stop();
        currentSource.volume = targetVolume;
        
        // Міняємо місцями sources
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }
    
    /// <summary>
    /// Повертає гучність для стану
    /// </summary>
    float GetVolumeForState(MusicState state)
    {
        switch (state)
        {
            case MusicState.Combat: return 0.8f;
            case MusicState.Tension: return 0.7f;
            case MusicState.Victory: return 0.9f;
            case MusicState.Defeat: return 0.6f;
            default: return 0.7f;
        }
    }
    
    /// <summary>
    /// Затухання музики
    /// </summary>
    IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = currentSource.volume;
        
        while (elapsed < deathFadeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathFadeTime;
            
            currentSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            
            yield return null;
        }
        
        currentSource.Stop();
        currentSource.volume = 0.7f;
    }
    
    /// <summary>
    /// Створює AudioSource компоненти, якщо не призначені
    /// </summary>
    void CreateAudioSources()
    {
        if (musicSource1 == null)
        {
            musicSource1 = gameObject.AddComponent<AudioSource>();
            musicSource1.playOnAwake = false;
            musicSource1.loop = true;
        }
        
        if (musicSource2 == null)
        {
            musicSource2 = gameObject.AddComponent<AudioSource>();
            musicSource2.playOnAwake = false;
            musicSource2.loop = true;
        }
        
        Debug.Log("DynamicMusicManager: Створено AudioSource компоненти");
    }
    
    // Публічні методи для ручного управління
    public void SetCombatState(bool inCombat)
    {
        if (inCombat && !isInCombat)
        {
            Events.Publish(new CombatStartedEvent(1, transform.position));
        }
        else if (!inCombat && isInCombat)
        {
            Events.Publish(new CombatEndedEvent(true, combatTimer));
        }
    }
    
    public void ForcePlayMusic(MusicState state)
    {
        PlayMusic(state);
    }
    
    public MusicState GetCurrentState()
    {
        return currentState;
    }
    
    void OnDestroy()
    {
        // Відписка від подій
        Events.Unsubscribe<CombatStartedEvent>(this);
        Events.Unsubscribe<CombatEndedEvent>(this);
        Events.Unsubscribe<PlayerDeathEvent>(this);
        Events.Unsubscribe<LevelCompletedEvent>(this);
        Events.Unsubscribe<PlayerHealthChangedEvent>(this);
    }
    
    void OnDrawGizmosSelected()
    {
        // Показуємо радіус виявлення ворогів в Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);
    }
}