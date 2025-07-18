using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// SURVIVAL MODE - ENDLESS WAVES
/// Режим виживання з нескінченними хвилями ворогів та прогресивною складністю
/// Включає систему магазину, лідерборд та унікальні нагороди
/// </summary>

// ================================
// ТИПИ ХВИЛЬ
// ================================

public enum WaveType
{
    Standard,       // Звичайна хвиля
    Elite,          // Хвиля з елітними ворогами
    Boss,           // Хвиля з босом
    Swarm,          // Хвиля з великою кількістю слабких ворогів
    Mixed,          // Змішана хвиля
    Special         // Спеціальна хвиля з унікальними умовами
}

public enum SurvivalDifficulty
{
    Easy,           // Легкий рівень
    Normal,         // Нормальний рівень
    Hard,           // Важкий рівень
    Nightmare,      // Кошмарний рівень
    Impossible      // Неможливий рівень
}

// ================================
// КОНФІГУРАЦІЯ ХВИЛІ
// ================================

[System.Serializable]
public class WaveConfiguration
{
    [Header("Wave Settings")]
    public WaveType waveType = WaveType.Standard;
    public int waveNumber = 1;
    public float preparationTime = 10f;
    public float waveDuration = 60f;
    
    [Header("Enemy Spawning")]
    public EnemySpawnData[] enemySpawns;
    public int totalEnemies = 10;
    public float spawnInterval = 2f;
    public float difficultyMultiplier = 1f;
    
    [Header("Rewards")]
    public int experienceReward = 100;
    public int currencyReward = 50;
    public LootDrop[] possibleLoot;
    
    [Header("Special Conditions")]
    public bool hasTimeLimit = false;
    public float timeLimit = 300f;
    public string specialCondition = "";
}

[System.Serializable]
public class EnemySpawnData
{
    public EnemyType enemyType;
    public int count;
    public float spawnWeight = 1f;
    public Vector3 spawnOffset = Vector3.zero;
}

// ================================
// ГОЛОВНИЙ МЕНЕДЖЕР SURVIVAL MODE
// ================================

public class SurvivalManager : MonoBehaviour
{
    [Header("Survival Settings")]
    public SurvivalDifficulty difficulty = SurvivalDifficulty.Normal;
    public int currentWave = 0;
    public int maxWaves = 100; // Теоретичний максимум
    public bool isInfinite = true;
    
    [Header("Wave Configuration")]
    public WaveConfiguration[] predefinedWaves;
    public AnimationCurve difficultyScaling;
    public float baseEnemyHealth = 100f;
    public float baseDamageMultiplier = 1f;
    
    [Header("Spawning")]
    public Transform[] spawnPoints;
    public float spawnRadius = 20f;
    public LayerMask spawnObstacles;
    
    [Header("UI References")]
    public SurvivalUI survivalUI;
    public ShopSystem shopSystem;
    public LeaderboardSystem leaderboard;
    
    [Header("Audio")]
    public AudioSource waveStartSound;
    public AudioSource waveCompleteSound;
    public AudioSource survivalMusic;
    
    // Приватні змінні
    private bool isWaveActive = false;
    private bool isPreparationPhase = false;
    private int enemiesRemaining = 0;
    private int enemiesKilled = 0;
    private float waveTimer = 0f;
    private float preparationTimer = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private WaveConfiguration currentWaveConfig;
    
    // Статистика
    private float survivalTime = 0f;
    private int totalKills = 0;
    private int totalScore = 0;
    private float accuracyPercentage = 0f;
    
    public static SurvivalManager Instance { get; private set; }
    
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
        InitializeSurvival();
    }
    
    void Update()
    {
        if (isWaveActive)
        {
            UpdateWave();
        }
        else if (isPreparationPhase)
        {
            UpdatePreparation();
        }
        
        survivalTime += Time.deltaTime;
    }
    
    void InitializeSurvival()
    {
        // Налаштування початкових параметрів
        currentWave = 0;
        totalKills = 0;
        totalScore = 0;
        survivalTime = 0f;
        
        // Запуск музики
        if (survivalMusic != null)
        {
            survivalMusic.Play();
        }
        
        // Ініціалізація UI
        if (survivalUI != null)
        {
            survivalUI.Initialize();
            survivalUI.UpdateWaveInfo(currentWave, 0);
        }
        
        // Початок першої хвилі
        StartNextWave();
    }
    
    public void StartSurvival()
    {
        InitializeSurvival();
        UIManager.Instance?.ShowNotification("SURVIVAL MODE РОЗПОЧАТО!", NotificationType.Info);
    }
    
    void StartNextWave()
    {
        currentWave++;
        
        // Генерація конфігурації хвилі
        currentWaveConfig = GenerateWaveConfiguration();
        
        // Початок фази підготовки
        StartPreparationPhase();
    }
    
    void StartPreparationPhase()
    {
        isPreparationPhase = true;
        preparationTimer = currentWaveConfig.preparationTime;
        
        // Відкриття магазину
        if (shopSystem != null && currentWave > 1)
        {
            shopSystem.OpenShop();
        }
        
        // Оновлення UI
        if (survivalUI != null)
        {
            survivalUI.StartPreparation(currentWaveConfig.preparationTime);
            survivalUI.UpdateWaveInfo(currentWave, currentWaveConfig.totalEnemies);
        }
        
        UIManager.Instance?.ShowNotification($"ХВИЛЯ {currentWave} ЧЕРЕЗ {currentWaveConfig.preparationTime:F0} СЕКУНД", NotificationType.Warning);
    }
    
    void UpdatePreparation()
    {
        preparationTimer -= Time.deltaTime;
        
        if (survivalUI != null)
        {
            survivalUI.UpdatePreparationTimer(preparationTimer);
        }
        
        if (preparationTimer <= 0f)
        {
            EndPreparationPhase();
        }
    }
    
    void EndPreparationPhase()
    {
        isPreparationPhase = false;
        
        // Закриття магазину
        if (shopSystem != null)
        {
            shopSystem.CloseShop();
        }
        
        // Початок хвилі
        StartWave();
    }
    
    void StartWave()
    {
        isWaveActive = true;
        waveTimer = 0f;
        enemiesRemaining = currentWaveConfig.totalEnemies;
        enemiesKilled = 0;
        
        // Звуковий ефект
        if (waveStartSound != null)
        {
            waveStartSound.Play();
        }
        
        // Оновлення UI
        if (survivalUI != null)
        {
            survivalUI.StartWave(currentWaveConfig);
        }
        
        UIManager.Instance?.ShowNotification($"ХВИЛЯ {currentWave} РОЗПОЧАЛАСЯ!", NotificationType.Danger);
        
        // Початок спавну ворогів
        StartCoroutine(SpawnEnemiesCoroutine());
    }
    
    void UpdateWave()
    {
        waveTimer += Time.deltaTime;
        
        // Оновлення UI
        if (survivalUI != null)
        {
            survivalUI.UpdateWaveProgress(enemiesKilled, enemiesRemaining);
        }
        
        // Перевірка завершення хвилі
        if (enemiesRemaining <= 0 && activeEnemies.Count == 0)
        {
            CompleteWave();
        }
        
        // Перевірка ліміту часу
        if (currentWaveConfig.hasTimeLimit && waveTimer >= currentWaveConfig.timeLimit)
        {
            FailWave();
        }
    }
    
    IEnumerator SpawnEnemiesCoroutine()
    {
        int enemiesToSpawn = currentWaveConfig.totalEnemies;
        
        while (enemiesToSpawn > 0 && isWaveActive)
        {
            // Вибір типу ворога для спавну
            EnemySpawnData spawnData = SelectEnemyToSpawn();
            
            if (spawnData != null)
            {
                SpawnEnemy(spawnData);
                enemiesToSpawn--;
            }
            
            yield return new WaitForSeconds(currentWaveConfig.spawnInterval);
        }
    }
    
    EnemySpawnData SelectEnemyToSpawn()
    {
        if (currentWaveConfig.enemySpawns.Length == 0) return null;
        
        // Вибір на основі ваги
        float totalWeight = 0f;
        foreach (var spawn in currentWaveConfig.enemySpawns)
        {
            totalWeight += spawn.spawnWeight;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var spawn in currentWaveConfig.enemySpawns)
        {
            currentWeight += spawn.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return spawn;
            }
        }
        
        return currentWaveConfig.enemySpawns[0];
    }
    
    void SpawnEnemy(EnemySpawnData spawnData)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Створення ворога
        GameObject enemyPrefab = EnemyManager.Instance?.GetEnemyPrefab(spawnData.enemyType);
        if (enemyPrefab != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            
            // Налаштування ворога для survival mode
            ConfigureEnemyForSurvival(enemy);
            
            activeEnemies.Add(enemy);
            
            // Підписка на смерть ворога
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.onDeath += OnEnemyKilled;
            }
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            randomOffset.y = 0f;
            
            Vector3 spawnPosition = spawnPoint.position + randomOffset;
            
            // Перевірка на перешкоди
            if (Physics.CheckSphere(spawnPosition, 1f, spawnObstacles))
            {
                return spawnPoint.position; // Fallback до базової позиції
            }
            
            return spawnPosition;
        }
        
        return Vector3.zero;
    }
    
    void ConfigureEnemyForSurvival(GameObject enemy)
    {
        // Збільшення здоров'я та урону залежно від хвилі
        float difficultyMultiplier = GetDifficultyMultiplier();
        
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth *= difficultyMultiplier;
            enemyHealth.currentHealth = enemyHealth.maxHealth;
        }
        
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.attackDamage *= difficultyMultiplier;
        }
        
        // Додавання компонента для survival mode
        SurvivalEnemy survivalComponent = enemy.AddComponent<SurvivalEnemy>();
        survivalComponent.waveNumber = currentWave;
        survivalComponent.difficultyMultiplier = difficultyMultiplier;
    }
    
    float GetDifficultyMultiplier()
    {
        float baseMultiplier = 1f;
        
        // Множник залежно від складності
        switch (difficulty)
        {
            case SurvivalDifficulty.Easy:
                baseMultiplier = 0.8f;
                break;
            case SurvivalDifficulty.Normal:
                baseMultiplier = 1f;
                break;
            case SurvivalDifficulty.Hard:
                baseMultiplier = 1.3f;
                break;
            case SurvivalDifficulty.Nightmare:
                baseMultiplier = 1.6f;
                break;
            case SurvivalDifficulty.Impossible:
                baseMultiplier = 2f;
                break;
        }
        
        // Прогресивне збільшення складності
        float waveMultiplier = difficultyScaling.Evaluate(currentWave / 50f); // Нормалізація до 50 хвиль
        
        return baseMultiplier * (1f + waveMultiplier);
    }
    
    void OnEnemyKilled(GameObject enemy)
    {
        enemiesKilled++;
        totalKills++;
        enemiesRemaining--;
        
        // Видалення з списку активних ворогів
        activeEnemies.Remove(enemy);
        
        // Додавання очок
        SurvivalEnemy survivalEnemy = enemy.GetComponent<SurvivalEnemy>();
        if (survivalEnemy != null)
        {
            int scoreReward = Mathf.RoundToInt(10 * survivalEnemy.difficultyMultiplier);
            AddScore(scoreReward);
        }
        
        // Оновлення UI
        if (survivalUI != null)
        {
            survivalUI.UpdateKillCount(totalKills);
            survivalUI.UpdateScore(totalScore);
        }
    }
    
    void AddScore(int points)
    {
        totalScore += points;
        
        // Показати додавання очок
        UIManager.Instance?.ShowFloatingText($"+{points}", Color.yellow);
    }
    
    void CompleteWave()
    {
        isWaveActive = false;
        
        // Звуковий ефект
        if (waveCompleteSound != null)
        {
            waveCompleteSound.Play();
        }
        
        // Нагороди
        GiveWaveRewards();
        
        UIManager.Instance?.ShowNotification($"ХВИЛЯ {currentWave} ЗАВЕРШЕНА!", NotificationType.Success);
        
        // Перехід до наступної хвилі
        if (isInfinite || currentWave < maxWaves)
        {
            StartNextWave();
        }
        else
        {
            CompleteSurvival();
        }
    }
    
    void GiveWaveRewards()
    {
        // Досвід
        int expReward = Mathf.RoundToInt(currentWaveConfig.experienceReward * GetDifficultyMultiplier());
        ExperienceManager.Instance?.AddExperience(expReward);
        
        // Валюта
        int currencyReward = Mathf.RoundToInt(currentWaveConfig.currencyReward * GetDifficultyMultiplier());
        CurrencyManager.Instance?.AddCurrency(currencyReward);
        
        // Лут
        SpawnWaveLoot();
        
        UIManager.Instance?.ShowNotification($"НАГОРОДА: {expReward} EXP, {currencyReward} МОНЕТ", NotificationType.Reward);
    }
    
    void SpawnWaveLoot()
    {
        if (currentWaveConfig.possibleLoot.Length > 0)
        {
            foreach (var loot in currentWaveConfig.possibleLoot)
            {
                if (Random.value <= loot.dropChance)
                {
                    Vector3 lootPosition = GetRandomSpawnPosition();
                    LootManager.Instance?.SpawnLoot(loot.itemType, lootPosition);
                }
            }
        }
    }
    
    void FailWave()
    {
        isWaveActive = false;
        
        UIManager.Instance?.ShowNotification("ХВИЛЯ ПРОВАЛЕНА! ЧАС ВИЙШОВ!", NotificationType.Danger);
        
        // Штраф за провал
        int scorePenalty = totalScore / 10;
        totalScore = Mathf.Max(0, totalScore - scorePenalty);
        
        // Можливість спробувати знову або завершити
        ShowFailOptions();
    }
    
    void ShowFailOptions()
    {
        // Показати UI з опціями: спробувати знову, завершити, тощо
        if (survivalUI != null)
        {
            survivalUI.ShowFailOptions();
        }
    }
    
    void CompleteSurvival()
    {
        // Завершення survival mode
        UIManager.Instance?.ShowNotification("SURVIVAL MODE ЗАВЕРШЕНО!", NotificationType.Victory);
        
        // Збереження результатів
        SaveSurvivalResults();
        
        // Показати фінальні результати
        ShowFinalResults();
    }
    
    void SaveSurvivalResults()
    {
        SurvivalResult result = new SurvivalResult
        {
            difficulty = difficulty,
            wavesCompleted = currentWave,
            totalKills = totalKills,
            totalScore = totalScore,
            survivalTime = survivalTime,
            accuracy = accuracyPercentage
        };
        
        // Збереження в лідерборд
        if (leaderboard != null)
        {
            leaderboard.SubmitScore(result);
        }
        
        // Збереження локально
        PlayerPrefs.SetInt("BestWave", Mathf.Max(PlayerPrefs.GetInt("BestWave", 0), currentWave));
        PlayerPrefs.SetInt("BestScore", Mathf.Max(PlayerPrefs.GetInt("BestScore", 0), totalScore));
        PlayerPrefs.Save();
    }
    
    void ShowFinalResults()
    {
        if (survivalUI != null)
        {
            survivalUI.ShowFinalResults(currentWave, totalKills, totalScore, survivalTime);
        }
    }
    
    WaveConfiguration GenerateWaveConfiguration()
    {
        // Використання предвизначених хвиль або генерація нової
        if (currentWave <= predefinedWaves.Length)
        {
            return predefinedWaves[currentWave - 1];
        }
        else
        {
            return GenerateProceduralWave();
        }
    }
    
    WaveConfiguration GenerateProceduralWave()
    {
        WaveConfiguration config = new WaveConfiguration();
        
        config.waveNumber = currentWave;
        config.waveType = GetRandomWaveType();
        config.preparationTime = Mathf.Max(5f, 15f - (currentWave * 0.1f)); // Менше часу на підготовку
        
        // Налаштування ворогів
        config.totalEnemies = Mathf.RoundToInt(10 + (currentWave * 1.5f));
        config.spawnInterval = Mathf.Max(0.5f, 2f - (currentWave * 0.02f)); // Швидший спавн
        
        // Генерація списку ворогів
        config.enemySpawns = GenerateEnemySpawns(config.totalEnemies);
        
        // Нагороди
        config.experienceReward = 100 + (currentWave * 10);
        config.currencyReward = 50 + (currentWave * 5);
        
        return config;
    }
    
    WaveType GetRandomWaveType()
    {
        // Більша ймовірність спеціальних хвиль на вищих рівнях
        float specialChance = Mathf.Min(0.3f, currentWave * 0.01f);
        
        if (Random.value < specialChance)
        {
            if (currentWave % 10 == 0) return WaveType.Boss;
            if (currentWave % 5 == 0) return WaveType.Elite;
            
            return (WaveType)Random.Range(1, System.Enum.GetValues(typeof(WaveType)).Length);
        }
        
        return WaveType.Standard;
    }
    
    EnemySpawnData[] GenerateEnemySpawns(int totalEnemies)
    {
        List<EnemySpawnData> spawns = new List<EnemySpawnData>();
        
        // Базові вороги
        spawns.Add(new EnemySpawnData
        {
            enemyType = EnemyType.Soldier,
            count = totalEnemies / 2,
            spawnWeight = 1f
        });
        
        // Додавання складніших ворогів на вищих хвилях
        if (currentWave >= 5)
        {
            spawns.Add(new EnemySpawnData
            {
                enemyType = EnemyType.Heavy,
                count = totalEnemies / 4,
                spawnWeight = 0.7f
            });
        }
        
        if (currentWave >= 10)
        {
            spawns.Add(new EnemySpawnData
            {
                enemyType = EnemyType.Sniper,
                count = totalEnemies / 6,
                spawnWeight = 0.5f
            });
        }
        
        return spawns.ToArray();
    }
    
    // Публічні методи для UI
    public void PauseSurvival()
    {
        Time.timeScale = 0f;
        if (survivalUI != null)
        {
            survivalUI.ShowPauseMenu();
        }
    }
    
    public void ResumeSurvival()
    {
        Time.timeScale = 1f;
        if (survivalUI != null)
        {
            survivalUI.HidePauseMenu();
        }
    }
    
    public void QuitSurvival()
    {
        // Збереження прогресу та вихід
        SaveSurvivalResults();
        LevelManager.Instance?.LoadMainMenu();
    }
    
    public void RestartWave()
    {
        // Перезапуск поточної хвилі
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        
        isWaveActive = false;
        StartWave();
    }
}

// ================================
// ДОПОМІЖНІ КЛАСИ
// ================================

[System.Serializable]
public class LootDrop
{
    public ItemType itemType;
    public float dropChance = 0.1f;
    public int quantity = 1;
}

[System.Serializable]
public class SurvivalResult
{
    public SurvivalDifficulty difficulty;
    public int wavesCompleted;
    public int totalKills;
    public int totalScore;
    public float survivalTime;
    public float accuracy;
    public System.DateTime completionDate;
    
    public SurvivalResult()
    {
        completionDate = System.DateTime.Now;
    }
}

public class SurvivalEnemy : MonoBehaviour
{
    public int waveNumber;
    public float difficultyMultiplier;
    
    void Start()
    {
        // Додаткові налаштування для survival ворогів
        ApplySurvivalModifications();
    }
    
    void ApplySurvivalModifications()
    {
        // Зміна кольору залежно від хвилі
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color waveColor = Color.Lerp(Color.white, Color.red, waveNumber / 50f);
            renderer.material.color = waveColor;
        }
        
        // Додавання ефектів для високих хвиль
        if (waveNumber >= 20)
        {
            AddEliteEffects();
        }
    }
    
    void AddEliteEffects()
    {
        // Додавання particle effects для елітних ворогів
        GameObject effect = new GameObject("Elite Effect");
        effect.transform.SetParent(transform);
        effect.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.gold;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.maxParticles = 10;
    }
}