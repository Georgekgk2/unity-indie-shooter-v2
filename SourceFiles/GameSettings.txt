using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Глобальні налаштування гри. Містить всі основні параметри для налаштування геймплею.
/// </summary>
[Configuration("Game/Game Settings", "Core")]
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Core/Game Settings")]
public class GameSettings : BaseConfiguration
{
    [Header("Game Difficulty")]
    [Tooltip("Рівень складності гри")]
    public DifficultyLevel difficulty = DifficultyLevel.Normal;
    [Tooltip("Множник урону ворогів")]
    [Range(0.1f, 5f)]
    public float enemyDamageMultiplier = 1f;
    [Tooltip("Множник здоров'я ворогів")]
    [Range(0.1f, 5f)]
    public float enemyHealthMultiplier = 1f;
    [Tooltip("Множник кількості ворогів")]
    [Range(0.1f, 3f)]
    public float enemySpawnMultiplier = 1f;
    [Tooltip("Множник швидкості ворогів")]
    [Range(0.5f, 2f)]
    public float enemySpeedMultiplier = 1f;

    [Header("Player Settings")]
    [Tooltip("Множник урону гравця")]
    [Range(0.1f, 5f)]
    public float playerDamageMultiplier = 1f;
    [Tooltip("Множник здоров'я гравця")]
    [Range(0.1f, 5f)]
    public float playerHealthMultiplier = 1f;
    [Tooltip("Множник швидкості гравця")]
    [Range(0.5f, 2f)]
    public float playerSpeedMultiplier = 1f;
    [Tooltip("Дозволити автоматичне прицілювання?")]
    public bool enableAutoAim = false;
    [Tooltip("Сила автоматичного прицілювання")]
    [Range(0f, 1f)]
    public float autoAimStrength = 0.3f;

    [Header("Gameplay Features")]
    [Tooltip("Дозволити дружній вогонь?")]
    public bool enableFriendlyFire = false;
    [Tooltip("Дозволити падіння зброї?")]
    public bool enableWeaponDrop = true;
    [Tooltip("Дозволити підбирання зброї?")]
    public bool enableWeaponPickup = true;
    [Tooltip("Дозволити взаємодію з об'єктами?")]
    public bool enableInteraction = true;
    [Tooltip("Дозволити систему досягнень?")]
    public bool enableAchievements = true;

    [Header("Physics Settings")]
    [Tooltip("Гравітація")]
    [Range(-50f, -1f)]
    public float gravity = -9.81f;
    [Tooltip("Множник часу")]
    [Range(0.1f, 2f)]
    public float timeScale = 1f;
    [Tooltip("Фіксований часовий крок")]
    [Range(0.01f, 0.05f)]
    public float fixedTimeStep = 0.02f;
    [Tooltip("Максимальний дозволений часовий крок")]
    [Range(0.1f, 1f)]
    public float maximumAllowedTimeStep = 0.33f;

    [Header("Audio Settings")]
    [Tooltip("Загальна гучність")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Tooltip("Гучність музики")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Tooltip("Гучність звукових ефектів")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Tooltip("Гучність голосу")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;
    [Tooltip("Увімкнути 3D звук?")]
    public bool enable3DAudio = true;

    [Header("Visual Settings")]
    [Tooltip("Якість графіки")]
    public GraphicsQuality graphicsQuality = GraphicsQuality.High;
    [Tooltip("Цільовий FPS")]
    public TargetFrameRate targetFrameRate = TargetFrameRate.FPS60;
    [Tooltip("Увімкнути VSync?")]
    public bool enableVSync = true;
    [Tooltip("Увімкнути HDR?")]
    public bool enableHDR = false;
    [Tooltip("Увімкнути постобробку?")]
    public bool enablePostProcessing = true;

    [Header("UI Settings")]
    [Tooltip("Розмір UI")]
    [Range(0.5f, 2f)]
    public float uiScale = 1f;
    [Tooltip("Прозорість UI")]
    [Range(0.1f, 1f)]
    public float uiOpacity = 1f;
    [Tooltip("Показувати FPS?")]
    public bool showFPS = false;
    [Tooltip("Показувати мініатюру?")]
    public bool showMinimap = true;
    [Tooltip("Показувати прицільну сітку?")]
    public bool showCrosshair = true;

    [Header("Input Settings")]
    [Tooltip("Чутливість миші")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2f;
    [Tooltip("Інвертувати вісь Y миші?")]
    public bool invertMouseY = false;
    [Tooltip("Увімкнути згладжування миші?")]
    public bool enableMouseSmoothing = true;
    [Tooltip("Увімкнути прискорення миші?")]
    public bool enableMouseAcceleration = false;

    [Header("Accessibility")]
    [Tooltip("Увімкнути субтитри?")]
    public bool enableSubtitles = false;
    [Tooltip("Розмір шрифту субтитрів")]
    [Range(0.5f, 2f)]
    public float subtitleFontSize = 1f;
    [Tooltip("Увімкнути індикатори для глухих?")]
    public bool enableDeafIndicators = false;
    [Tooltip("Увімкнути контрастні кольори?")]
    public bool enableHighContrast = false;
    [Tooltip("Увімкнути допомогу для дальтоніків?")]
    public bool enableColorBlindAssist = false;

    public enum DifficultyLevel
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        VeryHard,
        Nightmare,
        Custom
    }

    public enum GraphicsQuality
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Ultra
    }

    public enum TargetFrameRate
    {
        FPS30 = 30,
        FPS60 = 60,
        FPS120 = 120,
        FPS144 = 144,
        Unlimited = -1
    }

    protected override void ValidateConfiguration()
    {
        // Валідація складності
        enemyDamageMultiplier = Mathf.Max(0.1f, enemyDamageMultiplier);
        enemyHealthMultiplier = Mathf.Max(0.1f, enemyHealthMultiplier);
        enemySpawnMultiplier = Mathf.Max(0.1f, enemySpawnMultiplier);
        enemySpeedMultiplier = Mathf.Max(0.1f, enemySpeedMultiplier);

        // Валідація гравця
        playerDamageMultiplier = Mathf.Max(0.1f, playerDamageMultiplier);
        playerHealthMultiplier = Mathf.Max(0.1f, playerHealthMultiplier);
        playerSpeedMultiplier = Mathf.Max(0.1f, playerSpeedMultiplier);
        autoAimStrength = Mathf.Clamp01(autoAimStrength);

        // Валідація фізики
        gravity = Mathf.Min(-1f, gravity);
        timeScale = Mathf.Max(0.1f, timeScale);
        fixedTimeStep = Mathf.Clamp(fixedTimeStep, 0.01f, 0.05f);
        maximumAllowedTimeStep = Mathf.Max(0.1f, maximumAllowedTimeStep);

        // Валідація аудіо
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        voiceVolume = Mathf.Clamp01(voiceVolume);

        // Валідація UI
        uiScale = Mathf.Max(0.1f, uiScale);
        uiOpacity = Mathf.Clamp01(uiOpacity);

        // Валідація вводу
        mouseSensitivity = Mathf.Max(0.1f, mouseSensitivity);

        // Валідація доступності
        subtitleFontSize = Mathf.Max(0.1f, subtitleFontSize);

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Game Settings";
        }
    }

    /// <summary>
    /// Застосовує налаштування складності
    /// </summary>
    public void ApplyDifficultySettings()
    {
        switch (difficulty)
        {
            case DifficultyLevel.VeryEasy:
                SetDifficultyMultipliers(0.5f, 0.7f, 0.8f, 0.8f, 1.5f, 1.2f, 1.2f);
                break;
            case DifficultyLevel.Easy:
                SetDifficultyMultipliers(0.7f, 0.8f, 0.9f, 0.9f, 1.3f, 1.1f, 1.1f);
                break;
            case DifficultyLevel.Normal:
                SetDifficultyMultipliers(1f, 1f, 1f, 1f, 1f, 1f, 1f);
                break;
            case DifficultyLevel.Hard:
                SetDifficultyMultipliers(1.3f, 1.2f, 1.1f, 1.1f, 0.8f, 0.9f, 0.9f);
                break;
            case DifficultyLevel.VeryHard:
                SetDifficultyMultipliers(1.6f, 1.5f, 1.3f, 1.2f, 0.7f, 0.8f, 0.8f);
                break;
            case DifficultyLevel.Nightmare:
                SetDifficultyMultipliers(2f, 2f, 1.5f, 1.5f, 0.5f, 0.7f, 0.7f);
                break;
        }
    }

    void SetDifficultyMultipliers(float enemyDmg, float enemyHp, float enemySpawn, float enemySpeed,
                                 float playerDmg, float playerHp, float playerSpeed)
    {
        enemyDamageMultiplier = enemyDmg;
        enemyHealthMultiplier = enemyHp;
        enemySpawnMultiplier = enemySpawn;
        enemySpeedMultiplier = enemySpeed;
        playerDamageMultiplier = playerDmg;
        playerHealthMultiplier = playerHp;
        playerSpeedMultiplier = playerSpeed;
    }

    /// <summary>
    /// Застосовує налаштування графіки
    /// </summary>
    public void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel((int)graphicsQuality);
        
        if (targetFrameRate == TargetFrameRate.Unlimited)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = (int)targetFrameRate;
        }

        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        Time.fixedDeltaTime = fixedTimeStep;
        Time.maximumDeltaTime = maximumAllowedTimeStep;
    }

    /// <summary>
    /// Застосовує налаштування фізики
    /// </summary>
    public void ApplyPhysicsSettings()
    {
        Physics.gravity = new Vector3(0, gravity, 0);
        Time.timeScale = timeScale;
    }
}

/// <summary>
/// Конфігурація рівня. Містить налаштування для конкретного рівня або місії.
/// </summary>
[Configuration("Game/Level Configuration", "Levels")]
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Levels/Level Configuration")]
public class LevelConfiguration : BaseConfiguration
{
    [Header("Level Info")]
    [Tooltip("Назва рівня")]
    public string levelName;
    [Tooltip("Сцена рівня")]
    public string sceneName;
    [Tooltip("Мініатюра рівня")]
    public Sprite levelThumbnail;
    [Tooltip("Тип рівня")]
    public LevelType levelType = LevelType.Campaign;
    [Tooltip("Рекомендований рівень гравця")]
    [Range(1, 100)]
    public int recommendedPlayerLevel = 1;

    [Header("Level Objectives")]
    [Tooltip("Основні цілі рівня")]
    [TextArea(3, 5)]
    public string primaryObjectives;
    [Tooltip("Додаткові цілі рівня")]
    [TextArea(3, 5)]
    public string secondaryObjectives;
    [Tooltip("Час на виконання (0 = без обмежень)")]
    [Range(0f, 3600f)]
    public float timeLimit = 0f;

    [Header("Enemy Configuration")]
    [Tooltip("Множник кількості ворогів")]
    [Range(0.1f, 5f)]
    public float enemyCountMultiplier = 1f;
    [Tooltip("Множник складності ворогів")]
    [Range(0.1f, 5f)]
    public float enemyDifficultyMultiplier = 1f;
    [Tooltip("Типи ворогів на рівні")]
    public string[] enemyTypes;
    [Tooltip("Боси на рівні")]
    public string[] bossEnemies;

    [Header("Loot and Rewards")]
    [Tooltip("Множник досвіду")]
    [Range(0.1f, 5f)]
    public float experienceMultiplier = 1f;
    [Tooltip("Множник грошей")]
    [Range(0.1f, 5f)]
    public float moneyMultiplier = 1f;
    [Tooltip("Доступна зброя на рівні")]
    public WeaponConfiguration[] availableWeapons;
    [Tooltip("Спеціальні нагороди")]
    public string[] specialRewards;

    [Header("Environmental Settings")]
    [Tooltip("Час доби")]
    public TimeOfDay timeOfDay = TimeOfDay.Day;
    [Tooltip("Погодні умови")]
    public WeatherCondition weather = WeatherCondition.Clear;
    [Tooltip("Температура")]
    [Range(-50f, 50f)]
    public float temperature = 20f;
    [Tooltip("Видимість (метри)")]
    [Range(10f, 1000f)]
    public float visibility = 500f;

    [Header("Audio Settings")]
    [Tooltip("Фонова музика")]
    public AudioClip backgroundMusic;
    [Tooltip("Звуки оточення")]
    public AudioClip[] ambientSounds;
    [Tooltip("Гучність фонової музики")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Gameplay Modifiers")]
    [Tooltip("Дозволити збереження на рівні?")]
    public bool allowSaving = true;
    [Tooltip("Дозволити паузу?")]
    public bool allowPause = true;
    [Tooltip("Дозволити респавн?")]
    public bool allowRespawn = true;
    [Tooltip("Кількість життів (0 = безмежно)")]
    [Range(0, 10)]
    public int playerLives = 0;

    public enum LevelType
    {
        Campaign,
        Survival,
        Arena,
        Stealth,
        Escort,
        Defense,
        Racing,
        Puzzle,
        Boss,
        Training
    }

    public enum TimeOfDay
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    public enum WeatherCondition
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Fog,
        Snow,
        Sandstorm
    }

    protected override void ValidateConfiguration()
    {
        recommendedPlayerLevel = Mathf.Clamp(recommendedPlayerLevel, 1, 100);
        timeLimit = Mathf.Max(0f, timeLimit);
        
        enemyCountMultiplier = Mathf.Max(0.1f, enemyCountMultiplier);
        enemyDifficultyMultiplier = Mathf.Max(0.1f, enemyDifficultyMultiplier);
        
        experienceMultiplier = Mathf.Max(0.1f, experienceMultiplier);
        moneyMultiplier = Mathf.Max(0.1f, moneyMultiplier);
        
        visibility = Mathf.Max(1f, visibility);
        musicVolume = Mathf.Clamp01(musicVolume);
        
        playerLives = Mathf.Max(0, playerLives);

        if (string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(levelName))
        {
            displayName = levelName;
        }

        if (string.IsNullOrEmpty(levelName))
        {
            levelName = displayName;
        }
    }

    /// <summary>
    /// Розраховує очікувану тривалість рівня
    /// </summary>
    public float GetEstimatedDuration()
    {
        float baseDuration = 600f; // 10 хвилин базово
        
        switch (levelType)
        {
            case LevelType.Campaign:
                baseDuration = 900f; // 15 хвилин
                break;
            case LevelType.Survival:
                baseDuration = 1800f; // 30 хвилин
                break;
            case LevelType.Arena:
                baseDuration = 300f; // 5 хвилин
                break;
            case LevelType.Boss:
                baseDuration = 600f; // 10 хвилин
                break;
            case LevelType.Training:
                baseDuration = 180f; // 3 хвилини
                break;
        }

        // Модифікуємо на основі складності
        baseDuration *= enemyDifficultyMultiplier;
        
        return timeLimit > 0 ? Mathf.Min(baseDuration, timeLimit) : baseDuration;
    }

    /// <summary>
    /// Перевіряє, чи підходить рівень для певного рівня гравця
    /// </summary>
    public bool IsSuitableForPlayerLevel(int playerLevel)
    {
        int levelDifference = Mathf.Abs(playerLevel - recommendedPlayerLevel);
        return levelDifference <= 5; // Дозволяємо різницю до 5 рівнів
    }

    /// <summary>
    /// Отримує модифікатор складності на основі рівня гравця
    /// </summary>
    public float GetDifficultyModifier(int playerLevel)
    {
        float levelDifference = recommendedPlayerLevel - playerLevel;
        return 1f + (levelDifference * 0.1f); // 10% за кожен рівень різниці
    }
}