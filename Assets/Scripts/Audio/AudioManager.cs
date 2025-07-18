using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Централізований менеджер аудіо системи. Інтегрований з Event System для автоматичного відтворення звуків.
/// </summary>
public class AudioManager : MonoBehaviour, 
    IEventHandler<WeaponFiredEvent>,
    IEventHandler<PlayerHealthChangedEvent>,
    IEventHandler<PlayerMovementStateChangedEvent>,
    IEventHandler<WeaponReloadStartedEvent>,
    IEventHandler<WeaponSwitchedEvent>,
    IEventHandler<PlaySoundEvent>
{
    [Header("Audio Mixer")]
    [Tooltip("Головний Audio Mixer")]
    public AudioMixerGroup masterMixer;
    [Tooltip("Група для музики")]
    public AudioMixerGroup musicMixer;
    [Tooltip("Група для звукових ефектів")]
    public AudioMixerGroup sfxMixer;
    [Tooltip("Група для голосу")]
    public AudioMixerGroup voiceMixer;
    [Tooltip("Група для звуків оточення")]
    public AudioMixerGroup ambientMixer;

    [Header("Audio Sources")]
    [Tooltip("Кількість AudioSource для SFX")]
    [Range(5, 20)]
    public int sfxSourceCount = 10;
    [Tooltip("AudioSource для музики")]
    public AudioSource musicSource;
    [Tooltip("AudioSource для голосу")]
    public AudioSource voiceSource;
    [Tooltip("AudioSource для звуків оточення")]
    public AudioSource ambientSource;

    [Header("Audio Settings")]
    [Tooltip("Загальна гучність")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Tooltip("Гучність музики")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Tooltip("Гучність SFX")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Tooltip("Гучність голосу")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
    [Tooltip("Гучність оточення")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;

    [Header("3D Audio Settings")]
    [Tooltip("Увімкнути 3D звук?")]
    public bool enable3DAudio = true;
    [Tooltip("Максимальна дистанція 3D звуку")]
    [Range(10f, 100f)]
    public float maxAudioDistance = 50f;
    [Tooltip("Мінімальна дистанція 3D звуку")]
    [Range(1f, 10f)]
    public float minAudioDistance = 5f;

    [Header("Audio Collections")]
    [Tooltip("Колекція звуків зброї")]
    public WeaponAudioCollection weaponAudio;
    [Tooltip("Колекція звуків гравця")]
    public PlayerAudioCollection playerAudio;
    [Tooltip("Колекція звуків UI")]
    public UIAudioCollection uiAudio;
    [Tooltip("Колекція звуків оточення")]
    public EnvironmentAudioCollection environmentAudio;

    // Приватні змінні
    private Queue<AudioSource> availableSFXSources = new Queue<AudioSource>();
    private List<AudioSource> allSFXSources = new List<AudioSource>();
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    private Coroutine musicFadeCoroutine;

    // Singleton
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Підписуємося на події
        Events.Subscribe<WeaponFiredEvent>(this);
        Events.Subscribe<PlayerHealthChangedEvent>(this);
        Events.Subscribe<PlayerMovementStateChangedEvent>(this);
        Events.Subscribe<WeaponReloadStartedEvent>(this);
        Events.Subscribe<WeaponSwitchedEvent>(this);
        Events.Subscribe<PlaySoundEvent>(this);

        // Застосовуємо налаштування гучності
        ApplyVolumeSettings();
    }

    void OnDestroy()
    {
        // Відписуємося від подій
        Events.Unsubscribe<WeaponFiredEvent>(this);
        Events.Unsubscribe<PlayerHealthChangedEvent>(this);
        Events.Unsubscribe<PlayerMovementStateChangedEvent>(this);
        Events.Unsubscribe<WeaponReloadStartedEvent>(this);
        Events.Unsubscribe<WeaponSwitchedEvent>(this);
        Events.Unsubscribe<PlaySoundEvent>(this);
    }

    /// <summary>
    /// Ініціалізує Audio Manager
    /// </summary>
    void InitializeAudioManager()
    {
        // Створюємо пул AudioSource для SFX
        CreateSFXSourcePool();

        // Налаштовуємо основні AudioSource
        SetupMainAudioSources();

        // Кешуємо аудіо кліпи
        CacheAudioClips();

        Debug.Log($"AudioManager ініціалізовано з {sfxSourceCount} SFX джерелами");
    }

    /// <summary>
    /// Створює пул AudioSource для звукових ефектів
    /// </summary>
    void CreateSFXSourcePool()
    {
        for (int i = 0; i < sfxSourceCount; i++)
        {
            GameObject sfxObject = new GameObject($"SFX_AudioSource_{i}");
            sfxObject.transform.SetParent(transform);

            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixer;
            source.playOnAwake = false;
            source.spatialBlend = enable3DAudio ? 1f : 0f;
            source.maxDistance = maxAudioDistance;
            source.minDistance = minAudioDistance;
            source.rolloffMode = AudioRolloffMode.Logarithmic;

            allSFXSources.Add(source);
            availableSFXSources.Enqueue(source);
        }
    }

    /// <summary>
    /// Налаштовує основні AudioSource
    /// </summary>
    void SetupMainAudioSources()
    {
        // Музика
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("Music_AudioSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
        }
        musicSource.outputAudioMixerGroup = musicMixer;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f; // 2D звук

        // Голос
        if (voiceSource == null)
        {
            GameObject voiceObject = new GameObject("Voice_AudioSource");
            voiceObject.transform.SetParent(transform);
            voiceSource = voiceObject.AddComponent<AudioSource>();
        }
        voiceSource.outputAudioMixerGroup = voiceMixer;
        voiceSource.playOnAwake = false;
        voiceSource.spatialBlend = 0f; // 2D звук

        // Оточення
        if (ambientSource == null)
        {
            GameObject ambientObject = new GameObject("Ambient_AudioSource");
            ambientObject.transform.SetParent(transform);
            ambientSource = ambientObject.AddComponent<AudioSource>();
        }
        ambientSource.outputAudioMixerGroup = ambientMixer;
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.spatialBlend = 0f; // 2D звук
    }

    /// <summary>
    /// Кешує аудіо кліпи для швидкого доступу
    /// </summary>
    void CacheAudioClips()
    {
        if (weaponAudio != null) CacheAudioCollection(weaponAudio.GetAllClips());
        if (playerAudio != null) CacheAudioCollection(playerAudio.GetAllClips());
        if (uiAudio != null) CacheAudioCollection(uiAudio.GetAllClips());
        if (environmentAudio != null) CacheAudioCollection(environmentAudio.GetAllClips());
    }

    void CacheAudioCollection(Dictionary<string, AudioClip> clips)
    {
        foreach (var kvp in clips)
        {
            audioClipCache[kvp.Key] = kvp.Value;
        }
    }

    // ================================
    // EVENT HANDLERS
    // ================================

    public void HandleEvent(WeaponFiredEvent eventData)
    {
        if (weaponAudio != null)
        {
            var shootSound = weaponAudio.GetWeaponFireSound(eventData.WeaponName);
            if (shootSound != null)
            {
                PlaySFX(shootSound, eventData.FirePosition);
            }
        }
    }

    public void HandleEvent(PlayerHealthChangedEvent eventData)
    {
        if (playerAudio != null)
        {
            if (eventData.IsDamage)
            {
                var damageSound = playerAudio.GetDamageSound();
                if (damageSound != null)
                {
                    PlaySFX(damageSound);
                }
            }
            else if (eventData.IsHealing)
            {
                var healSound = playerAudio.GetHealSound();
                if (healSound != null)
                {
                    PlaySFX(healSound);
                }
            }
        }
    }

    public void HandleEvent(PlayerMovementStateChangedEvent eventData)
    {
        if (playerAudio != null)
        {
            switch (eventData.NewState)
            {
                case PlayerMovementStateChangedEvent.MovementState.Jumping:
                    var jumpSound = playerAudio.GetJumpSound();
                    if (jumpSound != null) PlaySFX(jumpSound);
                    break;
                case PlayerMovementStateChangedEvent.MovementState.Falling:
                    // Можна додати звук падіння
                    break;
            }
        }
    }

    public void HandleEvent(WeaponReloadStartedEvent eventData)
    {
        if (weaponAudio != null)
        {
            var reloadSound = weaponAudio.GetWeaponReloadSound(eventData.WeaponName);
            if (reloadSound != null)
            {
                PlaySFX(reloadSound);
            }
        }
    }

    public void HandleEvent(WeaponSwitchedEvent eventData)
    {
        if (weaponAudio != null)
        {
            var switchSound = weaponAudio.GetWeaponSwitchSound();
            if (switchSound != null)
            {
                PlaySFX(switchSound);
            }
        }
    }

    public void HandleEvent(PlaySoundEvent eventData)
    {
        switch (eventData.Type)
        {
            case PlaySoundEvent.SoundType.SFX:
                PlaySFX(eventData.AudioClip, eventData.Position, eventData.Volume, eventData.Is3D);
                break;
            case PlaySoundEvent.SoundType.Music:
                PlayMusic(eventData.AudioClip, eventData.Volume);
                break;
            case PlaySoundEvent.SoundType.Voice:
                PlayVoice(eventData.AudioClip, eventData.Volume);
                break;
            case PlaySoundEvent.SoundType.Ambient:
                PlayAmbient(eventData.AudioClip, eventData.Volume);
                break;
        }
    }

    // ================================
    // PUBLIC METHODS
    // ================================

    /// <summary>
    /// Відтворює звуковий ефект
    /// </summary>
    public void PlaySFX(AudioClip clip, Vector3 position = default, float volume = 1f, bool is3D = false)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source == null) return;

        source.clip = clip;
        source.volume = volume * sfxVolume * masterVolume;
        source.spatialBlend = (enable3DAudio && is3D) ? 1f : 0f;

        if (is3D && position != default)
        {
            source.transform.position = position;
        }

        source.Play();
        StartCoroutine(ReturnSFXSourceWhenFinished(source, clip.length));
    }

    /// <summary>
    /// Відтворює музику
    /// </summary>
    public void PlayMusic(AudioClip clip, float volume = 1f, bool fadeIn = true)
    {
        if (clip == null || musicSource == null) return;

        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        if (fadeIn && musicSource.isPlaying)
        {
            musicFadeCoroutine = StartCoroutine(FadeMusic(clip, volume));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = volume * musicVolume * masterVolume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Відтворює голос
    /// </summary>
    public void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip == null || voiceSource == null) return;

        voiceSource.clip = clip;
        voiceSource.volume = volume * voiceVolume * masterVolume;
        voiceSource.Play();
    }

    /// <summary>
    /// Відтворює звуки оточення
    /// </summary>
    public void PlayAmbient(AudioClip clip, float volume = 1f)
    {
        if (clip == null || ambientSource == null) return;

        ambientSource.clip = clip;
        ambientSource.volume = volume * ambientVolume * masterVolume;
        ambientSource.Play();
    }

    /// <summary>
    /// Зупиняє всю музику
    /// </summary>
    public void StopMusic(bool fadeOut = true)
    {
        if (musicSource == null) return;

        if (fadeOut)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Застосовує налаштування гучності
    /// </summary>
    public void ApplyVolumeSettings()
    {
        if (masterMixer != null)
        {
            masterMixer.audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            masterMixer.audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            masterMixer.audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
            masterMixer.audioMixer.SetFloat("VoiceVolume", Mathf.Log10(voiceVolume) * 20);
            masterMixer.audioMixer.SetFloat("AmbientVolume", Mathf.Log10(ambientVolume) * 20);
        }
    }

    // ================================
    // PRIVATE METHODS
    // ================================

    AudioSource GetAvailableSFXSource()
    {
        if (availableSFXSources.Count > 0)
        {
            return availableSFXSources.Dequeue();
        }

        // Якщо немає доступних джерел, знаходимо найстаріше
        AudioSource oldestSource = null;
        float oldestTime = float.MaxValue;

        foreach (var source in allSFXSources)
        {
            if (!source.isPlaying)
            {
                availableSFXSources.Enqueue(source);
                return availableSFXSources.Dequeue();
            }

            if (source.time < oldestTime)
            {
                oldestTime = source.time;
                oldestSource = source;
            }
        }

        return oldestSource;
    }

    IEnumerator ReturnSFXSourceWhenFinished(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (source != null)
        {
            source.Stop();
            availableSFXSources.Enqueue(source);
        }
    }

    IEnumerator FadeMusic(AudioClip newClip, float targetVolume)
    {
        float startVolume = musicSource.volume;
        
        // Fade out
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 1f; // 1 секунда fade
            yield return null;
        }

        // Змінюємо кліп
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        float finalVolume = targetVolume * musicVolume * masterVolume;
        while (musicSource.volume < finalVolume)
        {
            musicSource.volume += finalVolume * Time.deltaTime / 1f; // 1 секунда fade
            yield return null;
        }

        musicSource.volume = finalVolume;
        musicFadeCoroutine = null;
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / 1f;
            yield return null;
        }

        musicSource.Stop();
        musicFadeCoroutine = null;
    }
}

// ================================
// AUDIO COLLECTIONS
// ================================

[System.Serializable]
public class WeaponAudioCollection
{
    [Header("Weapon Sounds")]
    public AudioClip[] rifleFireSounds;
    public AudioClip[] pistolFireSounds;
    public AudioClip[] shotgunFireSounds;
    public AudioClip[] reloadSounds;
    public AudioClip weaponSwitchSound;
    public AudioClip emptyClipSound;

    public AudioClip GetWeaponFireSound(string weaponName)
    {
        // Логіка вибору звуку на основі назви зброї
        if (weaponName.ToLower().Contains("rifle") && rifleFireSounds.Length > 0)
            return rifleFireSounds[Random.Range(0, rifleFireSounds.Length)];
        if (weaponName.ToLower().Contains("pistol") && pistolFireSounds.Length > 0)
            return pistolFireSounds[Random.Range(0, pistolFireSounds.Length)];
        if (weaponName.ToLower().Contains("shotgun") && shotgunFireSounds.Length > 0)
            return shotgunFireSounds[Random.Range(0, shotgunFireSounds.Length)];
        
        return rifleFireSounds.Length > 0 ? rifleFireSounds[0] : null;
    }

    public AudioClip GetWeaponReloadSound(string weaponName)
    {
        return reloadSounds.Length > 0 ? reloadSounds[Random.Range(0, reloadSounds.Length)] : null;
    }

    public AudioClip GetWeaponSwitchSound()
    {
        return weaponSwitchSound;
    }

    public Dictionary<string, AudioClip> GetAllClips()
    {
        var clips = new Dictionary<string, AudioClip>();
        // Додаємо всі кліпи до словника
        return clips;
    }
}

[System.Serializable]
public class PlayerAudioCollection
{
    [Header("Player Sounds")]
    public AudioClip[] footstepSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] damageSounds;
    public AudioClip[] healSounds;
    public AudioClip deathSound;
    public AudioClip respawnSound;

    public AudioClip GetFootstepSound()
    {
        return footstepSounds.Length > 0 ? footstepSounds[Random.Range(0, footstepSounds.Length)] : null;
    }

    public AudioClip GetJumpSound()
    {
        return jumpSounds.Length > 0 ? jumpSounds[Random.Range(0, jumpSounds.Length)] : null;
    }

    public AudioClip GetDamageSound()
    {
        return damageSounds.Length > 0 ? damageSounds[Random.Range(0, damageSounds.Length)] : null;
    }

    public AudioClip GetHealSound()
    {
        return healSounds.Length > 0 ? healSounds[Random.Range(0, healSounds.Length)] : null;
    }

    public Dictionary<string, AudioClip> GetAllClips()
    {
        var clips = new Dictionary<string, AudioClip>();
        // Додаємо всі кліпи до словника
        return clips;
    }
}

[System.Serializable]
public class UIAudioCollection
{
    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;
    public AudioClip menuOpenSound;
    public AudioClip menuCloseSound;
    public AudioClip errorSound;
    public AudioClip successSound;

    public Dictionary<string, AudioClip> GetAllClips()
    {
        var clips = new Dictionary<string, AudioClip>();
        if (buttonClickSound != null) clips["button_click"] = buttonClickSound;
        if (buttonHoverSound != null) clips["button_hover"] = buttonHoverSound;
        return clips;
    }
}

[System.Serializable]
public class EnvironmentAudioCollection
{
    [Header("Environment Sounds")]
    public AudioClip[] ambientSounds;
    public AudioClip[] windSounds;
    public AudioClip[] rainSounds;
    public AudioClip[] explosionSounds;

    public Dictionary<string, AudioClip> GetAllClips()
    {
        var clips = new Dictionary<string, AudioClip>();
        // Додаємо всі кліпи до словника
        return clips;
    }
}