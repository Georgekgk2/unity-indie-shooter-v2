using UnityEngine;
using System;

/// <summary>
/// Структури даних для системи досягнень (Claude рекомендація)
/// Визначає типи, категорії та класи для досягнень
/// </summary>

/// <summary>
/// Клас досягнення
/// </summary>
[System.Serializable]
public class Achievement
{
    [Header("Basic Info")]
    public string id;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    
    [Header("Progress")]
    public bool isUnlocked;
    public DateTime unlockedDate;
    public int currentProgress;
    public int targetValue;
    
    [Header("Classification")]
    public AchievementCategory category;
    public AchievementType type;
    public AchievementRarity rarity;
    
    [Header("Rewards")]
    public int xpReward;
    public string[] unlockRewards; // Додаткові нагороди (зброя, скіни тощо)
    
    [Header("Requirements")]
    public string[] prerequisites; // ID досягнень, які потрібно розблокувати спочатку
    public bool isSecret; // Приховане досягнення
    public bool isRepeatable; // Чи можна отримати кілька разів
    
    // Обчислювані властивості
    public float ProgressPercentage => targetValue > 0 ? (float)currentProgress / targetValue * 100f : 0f;
    public bool IsCompleted => currentProgress >= targetValue;
    public bool IsInProgress => currentProgress > 0 && !isUnlocked;
    
    /// <summary>
    /// Перевіряє, чи виконані всі передумови
    /// </summary>
    public bool ArePrerequisitesMet()
    {
        if (prerequisites == null || prerequisites.Length == 0)
            return true;
            
        foreach (string prereqId in prerequisites)
        {
            var prereq = AchievementManager.Instance?.GetAchievement(prereqId);
            if (prereq == null || !prereq.isUnlocked)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Повертає колір залежно від рідкості
    /// </summary>
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return Color.white;
            case AchievementRarity.Uncommon: return Color.green;
            case AchievementRarity.Rare: return Color.blue;
            case AchievementRarity.Epic: return Color.magenta;
            case AchievementRarity.Legendary: return Color.yellow;
            default: return Color.gray;
        }
    }
    
    /// <summary>
    /// Повертає текст рідкості
    /// </summary>
    public string GetRarityText()
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return "Звичайне";
            case AchievementRarity.Uncommon: return "Незвичайне";
            case AchievementRarity.Rare: return "Рідкісне";
            case AchievementRarity.Epic: return "Епічне";
            case AchievementRarity.Legendary: return "Легендарне";
            default: return "Невідоме";
        }
    }
}

/// <summary>
/// Категорії досягнень
/// </summary>
public enum AchievementCategory
{
    Combat,      // Бойові досягнення
    Precision,   // Точність та влучність
    Survival,    // Виживання
    Progress,    // Прогресія в грі
    Special,     // Спеціальні досягнення
    Collection,  // Збирання предметів
    Social,      // Соціальні досягнення (мультиплеєр)
    Hidden       // Приховані досягнення
}

/// <summary>
/// Типи досягнень для відстеження прогресу
/// </summary>
public enum AchievementType
{
    KillEnemies,        // Вбивство ворогів
    Headshots,          // Хедшоти
    SurviveTime,        // Час виживання
    CompleteLevel,      // Завершення рівнів
    UseWeapon,          // Використання зброї
    TakeDamage,         // Отримання урону
    WalkDistance,       // Пройдена відстань
    CollectItems,       // Збирання предметів
    Accuracy,           // Точність стрільби
    NoDeathRun,         // Прохід без смертей
    PerfectRun,         // Досконалий прохід
    KillingSpree,       // Серія вбивств
    Multikill,          // Мультикіл
    WeaponMastery,      // Майстерність зброї
    SpeedRun,           // Швидкий прохід
    Exploration,        // Дослідження
    Achievement         // Мета-досягнення (розблокувати X досягнень)
}

/// <summary>
/// Рідкість досягнень
/// </summary>
public enum AchievementRarity
{
    Common,      // Звичайне (білий)
    Uncommon,    // Незвичайне (зелений)
    Rare,        // Рідкісне (синій)
    Epic,        // Епічне (фіолетовий)
    Legendary    // Легендарне (жовтий)
}

/// <summary>
/// Статистика гравця
/// </summary>
[System.Serializable]
public class PlayerStatistics : MonoBehaviour
{
    [Header("Combat Stats")]
    public int enemiesKilled;
    public int headshots;
    public int shotsFired;
    public int shotsHit;
    public float damageTaken;
    public float damageDealt;
    
    [Header("Game Stats")]
    public int levelsCompleted;
    public int deathsTotal;
    public int deathsThisLevel;
    public float totalPlayTime;
    public float distanceWalked;
    public int itemsCollected;
    
    [Header("Session Stats")]
    public int sessionKills;
    public int sessionHeadshots;
    public int sessionShotsFired;
    public int sessionShotsHit;
    public float sessionPlayTime;
    public float sessionStartTime;
    
    [Header("Weapon Stats")]
    public SerializableDictionary<string, int> weaponKills = new SerializableDictionary<string, int>();
    public SerializableDictionary<string, int> weaponShots = new SerializableDictionary<string, int>();
    public SerializableDictionary<string, float> weaponAccuracy = new SerializableDictionary<string, float>();
    
    [Header("Achievement Stats")]
    public int achievementsUnlocked;
    public int totalXPEarned;
    public DateTime firstPlayDate;
    public DateTime lastPlayDate;
    
    void Start()
    {
        sessionStartTime = Time.time;
        lastPlayDate = DateTime.Now;
        
        if (firstPlayDate == default(DateTime))
        {
            firstPlayDate = DateTime.Now;
        }
    }
    
    void Update()
    {
        totalPlayTime += Time.deltaTime;
        sessionPlayTime += Time.deltaTime;
    }
    
    /// <summary>
    /// Обчислює загальну точність
    /// </summary>
    public float GetAccuracy()
    {
        if (shotsFired == 0) return 0f;
        return (float)shotsHit / shotsFired * 100f;
    }
    
    /// <summary>
    /// Обчислює відсоток хедшотів
    /// </summary>
    public float GetHeadshotPercentage()
    {
        if (enemiesKilled == 0) return 0f;
        return (float)headshots / enemiesKilled * 100f;
    }
    
    /// <summary>
    /// Обчислює середній урон за постріл
    /// </summary>
    public float GetAverageDamagePerShot()
    {
        if (shotsFired == 0) return 0f;
        return damageDealt / shotsFired;
    }
    
    /// <summary>
    /// Обчислює K/D співвідношення
    /// </summary>
    public float GetKDRatio()
    {
        if (deathsTotal == 0) return enemiesKilled;
        return (float)enemiesKilled / deathsTotal;
    }
    
    /// <summary>
    /// Повертає час гри у форматованому вигляді
    /// </summary>
    public string GetFormattedPlayTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(totalPlayTime);
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }
    
    /// <summary>
    /// Повертає точність для конкретної зброї
    /// </summary>
    public float GetWeaponAccuracy(string weaponName)
    {
        if (!weaponShots.ContainsKey(weaponName) || weaponShots[weaponName] == 0)
            return 0f;
            
        int hits = weaponKills.ContainsKey(weaponName) ? weaponKills[weaponName] : 0;
        return (float)hits / weaponShots[weaponName] * 100f;
    }
    
    /// <summary>
    /// Додає статистику для зброї
    /// </summary>
    public void AddWeaponStat(string weaponName, bool hit, bool kill)
    {
        if (!weaponShots.ContainsKey(weaponName))
            weaponShots[weaponName] = 0;
        if (!weaponKills.ContainsKey(weaponName))
            weaponKills[weaponName] = 0;
            
        weaponShots[weaponName]++;
        
        if (hit)
            shotsHit++;
            
        if (kill)
            weaponKills[weaponName]++;
    }
    
    /// <summary>
    /// Скидає статистику сесії
    /// </summary>
    public void ResetSessionStats()
    {
        sessionKills = 0;
        sessionHeadshots = 0;
        sessionShotsFired = 0;
        sessionShotsHit = 0;
        sessionPlayTime = 0f;
        sessionStartTime = Time.time;
    }
    
    /// <summary>
    /// Зберігає статистику
    /// </summary>
    public void SaveStatistics()
    {
        PlayerPrefs.SetInt("stats_enemies_killed", enemiesKilled);
        PlayerPrefs.SetInt("stats_headshots", headshots);
        PlayerPrefs.SetInt("stats_shots_fired", shotsFired);
        PlayerPrefs.SetInt("stats_shots_hit", shotsHit);
        PlayerPrefs.SetFloat("stats_damage_taken", damageTaken);
        PlayerPrefs.SetFloat("stats_damage_dealt", damageDealt);
        PlayerPrefs.SetInt("stats_levels_completed", levelsCompleted);
        PlayerPrefs.SetInt("stats_deaths_total", deathsTotal);
        PlayerPrefs.SetFloat("stats_total_playtime", totalPlayTime);
        PlayerPrefs.SetFloat("stats_distance_walked", distanceWalked);
        PlayerPrefs.SetInt("stats_items_collected", itemsCollected);
        PlayerPrefs.SetInt("stats_achievements_unlocked", achievementsUnlocked);
        PlayerPrefs.SetInt("stats_total_xp_earned", totalXPEarned);
        
        // Зберігаємо дати
        PlayerPrefs.SetString("stats_first_play_date", firstPlayDate.ToBinary().ToString());
        PlayerPrefs.SetString("stats_last_play_date", lastPlayDate.ToBinary().ToString());
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Завантажує статистику
    /// </summary>
    public void LoadStatistics()
    {
        enemiesKilled = PlayerPrefs.GetInt("stats_enemies_killed", 0);
        headshots = PlayerPrefs.GetInt("stats_headshots", 0);
        shotsFired = PlayerPrefs.GetInt("stats_shots_fired", 0);
        shotsHit = PlayerPrefs.GetInt("stats_shots_hit", 0);
        damageTaken = PlayerPrefs.GetFloat("stats_damage_taken", 0f);
        damageDealt = PlayerPrefs.GetFloat("stats_damage_dealt", 0f);
        levelsCompleted = PlayerPrefs.GetInt("stats_levels_completed", 0);
        deathsTotal = PlayerPrefs.GetInt("stats_deaths_total", 0);
        totalPlayTime = PlayerPrefs.GetFloat("stats_total_playtime", 0f);
        distanceWalked = PlayerPrefs.GetFloat("stats_distance_walked", 0f);
        itemsCollected = PlayerPrefs.GetInt("stats_items_collected", 0);
        achievementsUnlocked = PlayerPrefs.GetInt("stats_achievements_unlocked", 0);
        totalXPEarned = PlayerPrefs.GetInt("stats_total_xp_earned", 0);
        
        // Завантажуємо дати
        string firstDateString = PlayerPrefs.GetString("stats_first_play_date", "");
        if (!string.IsNullOrEmpty(firstDateString) && long.TryParse(firstDateString, out long firstDateBinary))
        {
            firstPlayDate = DateTime.FromBinary(firstDateBinary);
        }
        
        string lastDateString = PlayerPrefs.GetString("stats_last_play_date", "");
        if (!string.IsNullOrEmpty(lastDateString) && long.TryParse(lastDateString, out long lastDateBinary))
        {
            lastPlayDate = DateTime.FromBinary(lastDateBinary);
        }
    }
}

/// <summary>
/// Подія розблокування досягнення
/// </summary>
public class AchievementUnlockedEvent : GameEvent
{
    public Achievement Achievement { get; }
    public int XPReward { get; }
    
    public AchievementUnlockedEvent(Achievement achievement)
    {
        Achievement = achievement;
        XPReward = achievement.xpReward;
    }
}

/// <summary>
/// Подія прогресу досягнення
/// </summary>
public class AchievementProgressEvent : GameEvent
{
    public Achievement Achievement { get; }
    public int PreviousProgress { get; }
    public int NewProgress { get; }
    
    public AchievementProgressEvent(Achievement achievement, int previousProgress, int newProgress)
    {
        Achievement = achievement;
        PreviousProgress = previousProgress;
        NewProgress = newProgress;
    }
}

/// <summary>
/// Серіалізований словник для Unity Inspector
/// </summary>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();
    
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
    
    public void OnAfterDeserialize()
    {
        this.Clear();
        
        if (keys.Count != values.Count)
        {
            Debug.LogError("SerializableDictionary: Keys and values count mismatch!");
            return;
        }
        
        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}