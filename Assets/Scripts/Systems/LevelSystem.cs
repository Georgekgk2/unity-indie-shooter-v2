using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Система рівнів з динамічним спавном ворогів, цілями та прогресією.
/// Включає різні типи рівнів та адаптивну складність.
/// </summary>

// ================================
// МЕНЕДЖЕР РІВНЯ
// ================================

public class LevelManager : MonoBehaviour,
    IEventHandler<PlayerDeathEvent>,
    IEventHandler<CheckpointReachedEvent>
{
    [Header("Level Configuration")]
    [Tooltip("Конфігурація поточного рівня")]
    public LevelConfiguration levelConfig;
    [Tooltip("Менеджер спавну ворогів")]
    public EnemySpawnManager enemySpawnManager;
    [Tooltip("Менеджер цілей")]
    public ObjectiveManager objectiveManager;

    [Header("Level State")]
    [Tooltip("Поточний стан рівня")]
    public LevelState currentState = LevelState.NotStarted;
    [Tooltip("Час початку рівня")]
    public float levelStartTime;
    [Tooltip("Поточний час рівня")]
    public float currentLevelTime;

    [Header("Player Spawn")]
    [Tooltip("Точка спавну гравця")]
    public Transform playerSpawnPoint;
    [Tooltip("Чекпоінти рівня")]
    public Checkpoint[] checkpoints;

    [Header("Level Events")]
    [Tooltip("Події рівня")]
    public LevelEvent[] levelEvents;

    // Приватні змінні
    private bool isLevelCompleted = false;
    private bool isLevelFailed = false;
    private int currentCheckpointIndex = 0;
    private float levelTimer = 0f;

    public enum LevelState
    {
        NotStarted,
        Loading,
        InProgress,
        Paused,
        Completed,
        Failed
    }

    [System.Serializable]
    public struct LevelEvent
    {
        public string eventName;
        public float triggerTime;
        public LevelEventType eventType;
        public string eventData;
        public bool hasTriggered;
    }

    public enum LevelEventType
    {
        SpawnWave,
        ShowMessage,
        PlayAudio,
        TriggerCutscene,
        ChangeMusic,
        SpawnBoss
    }

    // Singleton
    public static LevelManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Підписуємося на події
        Events.Subscribe<PlayerDeathEvent>(this);
        Events.Subscribe<CheckpointReachedEvent>(this);

        // Ініціалізуємо рівень
        InitializeLevel();
    }

    void Update()
    {
        if (currentState == LevelState.InProgress)
        {
            UpdateLevelTimer();
            CheckLevelEvents();
            CheckTimeLimit();
        }
    }

    /// <summary>
    /// Ініціалізує рівень
    /// </summary>
    void InitializeLevel()
    {
        if (levelConfig == null)
        {
            Debug.LogError("LevelManager: levelConfig не призначено!");
            return;
        }

        currentState = LevelState.Loading;

        // Застосовуємо налаштування рівня
        ApplyLevelSettings();

        // Ініціалізуємо компоненти
        InitializeComponents();

        // Спавнимо гравця
        SpawnPlayer();

        // Починаємо рівень
        StartLevel();
    }

    void ApplyLevelSettings()
    {
        // Застосовуємо налаштування з конфігурації
        if (AudioManager.Instance != null && levelConfig.backgroundMusic != null)
        {
            AudioManager.Instance.PlayMusic(levelConfig.backgroundMusic, true, 2f);
        }

        // Налаштовуємо освітлення та погоду
        ApplyEnvironmentalSettings();

        // Застосовуємо модифікатори складності
        ApplyDifficultyModifiers();
    }

    void ApplyEnvironmentalSettings()
    {
        // Тут можна налаштувати освітлення, погоду, час доби
        switch (levelConfig.timeOfDay)
        {
            case LevelConfiguration.TimeOfDay.Dawn:
                RenderSettings.ambientLight = new Color(0.8f, 0.6f, 0.4f);
                break;
            case LevelConfiguration.TimeOfDay.Day:
                RenderSettings.ambientLight = Color.white;
                break;
            case LevelConfiguration.TimeOfDay.Dusk:
                RenderSettings.ambientLight = new Color(1f, 0.7f, 0.5f);
                break;
            case LevelConfiguration.TimeOfDay.Night:
                RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.4f);
                break;
        }
    }

    void ApplyDifficultyModifiers()
    {
        var gameSettings = ConfigurationManager.Instance?.gameSettings;
        if (gameSettings != null)
        {
            // Застосовуємо модифікатори складності до рівня
            float difficultyMultiplier = levelConfig.GetDifficultyModifier(1); // Припускаємо рівень гравця 1
            
            if (enemySpawnManager != null)
            {
                enemySpawnManager.ApplyDifficultyMultiplier(difficultyMultiplier);
            }
        }
    }

    void InitializeComponents()
    {
        if (enemySpawnManager == null)
            enemySpawnManager = GetComponent<EnemySpawnManager>();
        
        if (objectiveManager == null)
            objectiveManager = GetComponent<ObjectiveManager>();

        // Ініціалізуємо компоненти
        enemySpawnManager?.Initialize(levelConfig);
        objectiveManager?.Initialize(levelConfig);
    }

    void SpawnPlayer()
    {
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            player.transform.rotation = playerSpawnPoint.rotation;
        }
    }

    /// <summary>
    /// Починає рівень
    /// </summary>
    public void StartLevel()
    {
        currentState = LevelState.InProgress;
        levelStartTime = Time.time;
        levelTimer = 0f;

        // Показуємо інформацію про рівень
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(
                $"Рівень: {levelConfig.levelName}",
                ModernUISystem.NotificationType.Info,
                3f
            );
        }

        // Починаємо спавн ворогів
        enemySpawnManager?.StartSpawning();

        // Активуємо цілі
        objectiveManager?.StartObjectives();

        Debug.Log($"Рівень '{levelConfig.levelName}' розпочато!");
    }

    void UpdateLevelTimer()
    {
        levelTimer += Time.deltaTime;
        currentLevelTime = levelTimer;
    }

    void CheckLevelEvents()
    {
        if (levelEvents == null) return;

        for (int i = 0; i < levelEvents.Length; i++)
        {
            if (!levelEvents[i].hasTriggered && levelTimer >= levelEvents[i].triggerTime)
            {
                TriggerLevelEvent(i);
            }
        }
    }

    void TriggerLevelEvent(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex >= levelEvents.Length) return;

        var levelEvent = levelEvents[eventIndex];
        levelEvents[eventIndex].hasTriggered = true;

        Debug.Log($"Тригер події рівня: {levelEvent.eventName}");

        switch (levelEvent.eventType)
        {
            case LevelEventType.SpawnWave:
                enemySpawnManager?.TriggerWave(levelEvent.eventData);
                break;
            case LevelEventType.ShowMessage:
                if (ModernUISystem.Instance != null)
                {
                    ModernUISystem.Instance.ShowNotification(levelEvent.eventData, ModernUISystem.NotificationType.Info, 4f);
                }
                break;
            case LevelEventType.SpawnBoss:
                enemySpawnManager?.SpawnBoss(levelEvent.eventData);
                break;
        }
    }

    void CheckTimeLimit()
    {
        if (levelConfig.timeLimit > 0 && levelTimer >= levelConfig.timeLimit)
        {
            FailLevel("Час вичерпано!");
        }
    }

    /// <summary>
    /// Завершує рівень успішно
    /// </summary>
    public void CompleteLevel()
    {
        if (isLevelCompleted || isLevelFailed) return;

        isLevelCompleted = true;
        currentState = LevelState.Completed;

        Debug.Log($"Рівень '{levelConfig.levelName}' завершено!");

        // Зупиняємо спавн ворогів
        enemySpawnManager?.StopSpawning();

        // Показуємо повідомлення про перемогу
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(
                "РІВЕНЬ ЗАВЕРШЕНО!",
                ModernUISystem.NotificationType.Success,
                5f
            );
        }

        // Нагороди
        GiveRewards();

        // Переходимо до наступного рівня через деякий час
        StartCoroutine(TransitionToNextLevel(3f));
    }

    /// <summary>
    /// Провалює рівень
    /// </summary>
    public void FailLevel(string reason = "")
    {
        if (isLevelCompleted || isLevelFailed) return;

        isLevelFailed = true;
        currentState = LevelState.Failed;

        Debug.Log($"Рівень '{levelConfig.levelName}' провалено! Причина: {reason}");

        // Зупиняємо спавн ворогів
        enemySpawnManager?.StopSpawning();

        // Показуємо повідомлення про поразку
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(
                $"РІВЕНЬ ПРОВАЛЕНО! {reason}",
                ModernUISystem.NotificationType.Error,
                5f
            );
        }

        // Перезапускаємо рівень через деякий час
        StartCoroutine(RestartLevel(3f));
    }

    void GiveRewards()
    {
        // Досвід
        int experience = Mathf.RoundToInt(100 * levelConfig.experienceMultiplier);
        
        // Гроші
        int money = Mathf.RoundToInt(50 * levelConfig.moneyMultiplier);

        Debug.Log($"Нагороди: {experience} досвіду, {money} грошей");

        // Тут можна додати систему нагород
    }

    IEnumerator TransitionToNextLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Тут можна завантажити наступний рівень
        Debug.Log("Перехід до наступного рівня...");
    }

    IEnumerator RestartLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Перезапускаємо поточний рівень
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void HandleEvent(PlayerDeathEvent eventData)
    {
        if (levelConfig.allowRespawn)
        {
            // Респавн на останньому чекпоінті
            RespawnPlayerAtCheckpoint();
        }
        else
        {
            FailLevel("Гравець загинув");
        }
    }

    public void HandleEvent(CheckpointReachedEvent eventData)
    {
        // Оновлюємо поточний чекпоінт
        for (int i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i].checkpointId == eventData.CheckpointId)
            {
                currentCheckpointIndex = i;
                break;
            }
        }

        Debug.Log($"Досягнуто чекпоінт: {eventData.CheckpointId}");
    }

    void RespawnPlayerAtCheckpoint()
    {
        if (currentCheckpointIndex < checkpoints.Length)
        {
            var checkpoint = checkpoints[currentCheckpointIndex];
            var player = FindObjectOfType<PlayerMovement>();
            
            if (player != null)
            {
                player.transform.position = checkpoint.transform.position;
                player.transform.rotation = checkpoint.transform.rotation;
            }
        }
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PlayerDeathEvent>(this);
        Events.Unsubscribe<CheckpointReachedEvent>(this);
    }
}

// ================================
// МЕНЕДЖЕР СПАВНУ ВОРОГІВ
// ================================

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Точки спавну ворогів")]
    public Transform[] spawnPoints;
    [Tooltip("Префаби ворогів")]
    public GameObject[] enemyPrefabs;
    [Tooltip("Максимальна кількість ворогів одночасно")]
    public int maxEnemiesAtOnce = 10;
    [Tooltip("Інтервал між спавнами")]
    public float spawnInterval = 5f;

    [Header("Wave System")]
    [Tooltip("Хвилі ворогів")]
    public EnemyWave[] enemyWaves;
    [Tooltip("Поточна хвиля")]
    public int currentWave = 0;

    [Header("Adaptive Spawning")]
    [Tooltip("Адаптивний спавн на основі продуктивності гравця")]
    public bool useAdaptiveSpawning = true;
    [Tooltip("Множник складності")]
    public float difficultyMultiplier = 1f;

    // Приватні змінні
    private List<Enemy> activeEnemies = new List<Enemy>();
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private LevelConfiguration levelConfig;

    [System.Serializable]
    public struct EnemyWave
    {
        public string waveName;
        public EnemySpawnData[] enemies;
        public float delayBeforeWave;
        public bool spawnAllAtOnce;
    }

    [System.Serializable]
    public struct EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int count;
        public float spawnDelay;
        public Transform specificSpawnPoint;
    }

    public void Initialize(LevelConfiguration config)
    {
        levelConfig = config;
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawnManager: Немає точок спавну!");
        }
    }

    public void StartSpawning()
    {
        if (isSpawning) return;

        isSpawning = true;
        
        if (enemyWaves != null && enemyWaves.Length > 0)
        {
            spawnCoroutine = StartCoroutine(SpawnWaves());
        }
        else
        {
            spawnCoroutine = StartCoroutine(ContinuousSpawning());
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnWaves()
    {
        for (int waveIndex = 0; waveIndex < enemyWaves.Length; waveIndex++)
        {
            if (!isSpawning) break;

            currentWave = waveIndex;
            var wave = enemyWaves[waveIndex];

            Debug.Log($"Спавн хвилі {waveIndex + 1}: {wave.waveName}");

            // Затримка перед хвилею
            yield return new WaitForSeconds(wave.delayBeforeWave);

            // Повідомлення про хвилю
            if (ModernUISystem.Instance != null)
            {
                ModernUISystem.Instance.ShowNotification(
                    $"Хвиля {waveIndex + 1}: {wave.waveName}",
                    ModernUISystem.NotificationType.Warning,
                    3f
                );
            }

            // Спавнимо ворогів хвилі
            yield return StartCoroutine(SpawnWaveEnemies(wave));

            // Чекаємо, поки всі вороги хвилі не будуть знищені
            yield return StartCoroutine(WaitForWaveCompletion());
        }

        Debug.Log("Всі хвилі завершено!");
    }

    IEnumerator SpawnWaveEnemies(EnemyWave wave)
    {
        if (wave.spawnAllAtOnce)
        {
            // Спавнимо всіх одразу
            foreach (var enemyData in wave.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    SpawnEnemy(enemyData.enemyPrefab, enemyData.specificSpawnPoint);
                }
            }
        }
        else
        {
            // Спавнимо з затримками
            foreach (var enemyData in wave.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    SpawnEnemy(enemyData.enemyPrefab, enemyData.specificSpawnPoint);
                    yield return new WaitForSeconds(enemyData.spawnDelay);
                }
            }
        }
    }

    IEnumerator WaitForWaveCompletion()
    {
        while (activeEnemies.Count > 0)
        {
            // Видаляємо знищених ворогів зі списку
            activeEnemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ContinuousSpawning()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemiesAtOnce)
            {
                SpawnRandomEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        SpawnEnemy(enemyPrefab);
    }

    void SpawnEnemy(GameObject enemyPrefab, Transform specificSpawnPoint = null)
    {
        if (enemyPrefab == null) return;

        // Вибираємо точку спавну
        Transform spawnPoint = specificSpawnPoint;
        if (spawnPoint == null && spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("EnemySpawnManager: Немає доступних точок спавну!");
            return;
        }

        // Створюємо ворога
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Застосовуємо модифікатори складності
            ApplyDifficultyToEnemy(enemy);
            
            activeEnemies.Add(enemy);
            Debug.Log($"Заспавнено ворога: {enemy.enemyName} в позиції {spawnPoint.position}");
        }
    }

    void ApplyDifficultyToEnemy(Enemy enemy)
    {
        if (levelConfig == null) return;

        // Застосовуємо модифікатори з конфігурації рівня
        enemy.maxHealth *= levelConfig.enemyDifficultyMultiplier * difficultyMultiplier;
        enemy.currentHealth = enemy.maxHealth;
        enemy.attackDamage *= levelConfig.enemyDifficultyMultiplier * difficultyMultiplier;
        enemy.moveSpeed *= levelConfig.enemyDifficultyMultiplier;
        enemy.runSpeed *= levelConfig.enemyDifficultyMultiplier;
    }

    public void ApplyDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = multiplier;
    }

    public void TriggerWave(string waveData)
    {
        // Тригер конкретної хвилі за назвою або індексом
        int waveIndex;
        if (int.TryParse(waveData, out waveIndex))
        {
            if (waveIndex >= 0 && waveIndex < enemyWaves.Length)
            {
                StartCoroutine(SpawnWaveEnemies(enemyWaves[waveIndex]));
            }
        }
    }

    public void SpawnBoss(string bossData)
    {
        // Спавн боса
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab.name.Contains("Boss") || prefab.name.Contains(bossData))
            {
                SpawnEnemy(prefab);
                break;
            }
        }
    }

    void Update()
    {
        // Очищуємо список від знищених ворогів
        activeEnemies.RemoveAll(enemy => enemy == null || enemy.IsDead);

        // Адаптивний спавн
        if (useAdaptiveSpawning && isSpawning)
        {
            AdaptSpawning();
        }
    }

    void AdaptSpawning()
    {
        // Простий алгоритм адаптації на основі кількості живих ворогів
        float enemyRatio = (float)activeEnemies.Count / maxEnemiesAtOnce;
        
        if (enemyRatio < 0.3f)
        {
            // Мало ворогів - збільшуємо спавн
            spawnInterval = Mathf.Max(1f, spawnInterval * 0.95f);
        }
        else if (enemyRatio > 0.8f)
        {
            // Багато ворогів - зменшуємо спавн
            spawnInterval = Mathf.Min(10f, spawnInterval * 1.05f);
        }
    }
}

// ================================
// МЕНЕДЖЕР ЦІЛЕЙ
// ================================

public class ObjectiveManager : MonoBehaviour
{
    [Header("Objectives")]
    [Tooltip("Цілі рівня")]
    public LevelObjective[] objectives;

    private int completedObjectives = 0;
    private bool allObjectivesCompleted = false;

    [System.Serializable]
    public struct LevelObjective
    {
        public string objectiveName;
        public string description;
        public ObjectiveType type;
        public int targetValue;
        public int currentValue;
        public bool isCompleted;
        public bool isOptional;
    }

    public enum ObjectiveType
    {
        KillEnemies,
        SurviveTime,
        ReachLocation,
        CollectItems,
        DefendArea,
        EscortNPC
    }

    public void Initialize(LevelConfiguration config)
    {
        // Ініціалізуємо цілі на основі конфігурації рівня
        ParseObjectivesFromConfig(config);
    }

    void ParseObjectivesFromConfig(LevelConfiguration config)
    {
        // Парсимо цілі з рядків конфігурації
        if (!string.IsNullOrEmpty(config.primaryObjectives))
        {
            string[] primaryGoals = config.primaryObjectives.Split('\n');
            // Тут можна додати логіку парсингу цілей
        }
    }

    public void StartObjectives()
    {
        // Показуємо цілі гравцю
        ShowObjectives();
    }

    void ShowObjectives()
    {
        if (ModernUISystem.Instance != null)
        {
            foreach (var objective in objectives)
            {
                if (!objective.isCompleted)
                {
                    ModernUISystem.Instance.ShowNotification(
                        $"Ціль: {objective.description}",
                        ModernUISystem.NotificationType.Info,
                        4f
                    );
                }
            }
        }
    }

    public void UpdateObjective(ObjectiveType type, int value = 1)
    {
        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i].type == type && !objectives[i].isCompleted)
            {
                objectives[i].currentValue += value;
                
                if (objectives[i].currentValue >= objectives[i].targetValue)
                {
                    CompleteObjective(i);
                }
                
                break;
            }
        }
    }

    void CompleteObjective(int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= objectives.Length) return;

        objectives[objectiveIndex].isCompleted = true;
        completedObjectives++;

        Debug.Log($"Ціль завершена: {objectives[objectiveIndex].objectiveName}");

        // Показуємо повідомлення
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(
                $"Ціль завершена: {objectives[objectiveIndex].objectiveName}",
                ModernUISystem.NotificationType.Success,
                3f
            );
        }

        // Перевіряємо, чи всі обов'язкові цілі завершені
        CheckAllObjectivesCompleted();
    }

    void CheckAllObjectivesCompleted()
    {
        bool allRequired = true;
        
        foreach (var objective in objectives)
        {
            if (!objective.isOptional && !objective.isCompleted)
            {
                allRequired = false;
                break;
            }
        }

        if (allRequired && !allObjectivesCompleted)
        {
            allObjectivesCompleted = true;
            
            // Завершуємо рівень
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CompleteLevel();
            }
        }
    }
}