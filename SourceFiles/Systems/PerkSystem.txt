using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Система перків та прогресії персонажа (продовження роботи попереднього агента)
/// Інтегрована з системою досягнень та Event System
/// </summary>

// ================================
// МЕНЕДЖЕР СИСТЕМИ ПЕРКІВ
// ================================

public class PerkSystem : MonoBehaviour, 
    IEventHandler<PlayerLevelUpEvent>,
    IEventHandler<AchievementUnlockedEvent>,
    IEventHandler<ExperienceGainedEvent>
{
    [Header("Perk Configuration")]
    [Tooltip("Список всіх доступних перків")]
    public List<Perk> availablePerks = new List<Perk>();
    [Tooltip("Максимальний рівень гравця")]
    public int maxPlayerLevel = 50;
    [Tooltip("Базова кількість XP для рівня")]
    public int baseXPRequired = 100;
    [Tooltip("Множник XP для кожного рівня")]
    public float xpMultiplier = 1.2f;

    [Header("Player Progress")]
    [Tooltip("Поточний рівень гравця")]
    public int currentLevel = 1;
    [Tooltip("Поточний XP")]
    public int currentXP = 0;
    [Tooltip("Доступні очки перків")]
    public int availablePerkPoints = 0;
    [Tooltip("Активні перки")]
    public List<string> unlockedPerkIds = new List<string>();

    [Header("UI References")]
    [Tooltip("Панель перків")]
    public GameObject perkPanel;
    [Tooltip("Контейнер для UI перків")]
    public Transform perkContainer;
    [Tooltip("Префаб UI перка")]
    public GameObject perkUIPrefab;

    // Singleton
    public static PerkSystem Instance { get; private set; }

    // Приватні змінні
    private Dictionary<string, Perk> perkDictionary = new Dictionary<string, Perk>();
    private Dictionary<int, int> levelXPRequirements = new Dictionary<int, int>();

    // Події
    public static event System.Action<int> OnLevelUp;
    public static event System.Action<Perk> OnPerkUnlocked;
    public static event System.Action<int> OnXPGained;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePerkSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Підписуємося на події
        Events.Subscribe<PlayerLevelUpEvent>(this);
        Events.Subscribe<AchievementUnlockedEvent>(this);
        Events.Subscribe<ExperienceGainedEvent>(this);

        LoadPlayerProgress();
        InitializePerkUI();
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PlayerLevelUpEvent>(this);
        Events.Unsubscribe<AchievementUnlockedEvent>(this);
        Events.Unsubscribe<ExperienceGainedEvent>(this);
    }

    /// <summary>
    /// Ініціалізація системи перків
    /// </summary>
    void InitializePerkSystem()
    {
        // Створюємо словник перків для швидкого доступу
        foreach (var perk in availablePerks)
        {
            if (!perkDictionary.ContainsKey(perk.id))
            {
                perkDictionary.Add(perk.id, perk);
            }
        }

        // Обчислюємо вимоги XP для кожного рівня
        CalculateLevelRequirements();

        // Створюємо базові перки, якщо список порожній
        if (availablePerks.Count == 0)
        {
            CreateDefaultPerks();
        }

        Debug.Log($"PerkSystem: Ініціалізовано {availablePerks.Count} перків");
    }

    /// <summary>
    /// Обчислює вимоги XP для кожного рівня
    /// </summary>
    void CalculateLevelRequirements()
    {
        levelXPRequirements.Clear();
        
        for (int level = 1; level <= maxPlayerLevel; level++)
        {
            int xpRequired = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplier, level - 1));
            levelXPRequirements[level] = xpRequired;
        }
    }

    /// <summary>
    /// Створює базові перки за замовчуванням
    /// </summary>
    void CreateDefaultPerks()
    {
        // Бойові перки
        availablePerks.Add(new Perk
        {
            id = "damage_boost_1",
            name = "Посилений урон I",
            description = "Збільшує урон зброї на 10%",
            category = PerkCategory.Combat,
            tier = 1,
            requiredLevel = 2,
            cost = 1,
            maxRank = 3,
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.DamageMultiplier, value = 0.1f }
            }
        });

        availablePerks.Add(new Perk
        {
            id = "health_boost_1",
            name = "Міцне здоров'я I",
            description = "Збільшує максимальне здоров'я на 25",
            category = PerkCategory.Survival,
            tier = 1,
            requiredLevel = 3,
            cost = 1,
            maxRank = 5,
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.HealthBonus, value = 25f }
            }
        });

        availablePerks.Add(new Perk
        {
            id = "reload_speed_1",
            name = "Швидка перезарядка I",
            description = "Зменшує час перезарядки на 15%",
            category = PerkCategory.Utility,
            tier = 1,
            requiredLevel = 4,
            cost = 1,
            maxRank = 3,
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.ReloadSpeedMultiplier, value = 0.15f }
            }
        });

        availablePerks.Add(new Perk
        {
            id = "movement_speed_1",
            name = "Швидкість руху I",
            description = "Збільшує швидкість пересування на 10%",
            category = PerkCategory.Movement,
            tier = 1,
            requiredLevel = 5,
            cost = 1,
            maxRank = 3,
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.MovementSpeedMultiplier, value = 0.1f }
            }
        });

        // Перки 2-го рівня
        availablePerks.Add(new Perk
        {
            id = "critical_hit_1",
            name = "Критичні удари I",
            description = "5% шанс критичного удару (подвійний урон)",
            category = PerkCategory.Combat,
            tier = 2,
            requiredLevel = 8,
            cost = 2,
            maxRank = 3,
            prerequisites = new List<string> { "damage_boost_1" },
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.CriticalChance, value = 0.05f }
            }
        });

        availablePerks.Add(new Perk
        {
            id = "stamina_boost_1",
            name = "Витривалість I",
            description = "Збільшує максимальну стаміну на 30",
            category = PerkCategory.Survival,
            tier = 2,
            requiredLevel = 10,
            cost = 2,
            maxRank = 3,
            prerequisites = new List<string> { "health_boost_1" },
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.StaminaBonus, value = 30f }
            }
        });

        // Легендарні перки
        availablePerks.Add(new Perk
        {
            id = "berserker_mode",
            name = "Режим Берсерка",
            description = "При здоров'ї нижче 25% урон збільшується на 50%",
            category = PerkCategory.Combat,
            tier = 3,
            requiredLevel = 20,
            cost = 3,
            maxRank = 1,
            rarity = PerkRarity.Legendary,
            prerequisites = new List<string> { "critical_hit_1", "damage_boost_1" },
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.BerserkerMode, value = 0.5f }
            }
        });

        availablePerks.Add(new Perk
        {
            id = "regeneration",
            name = "Регенерація",
            description = "Відновлює 2 HP кожні 3 секунди",
            category = PerkCategory.Survival,
            tier = 3,
            requiredLevel = 25,
            cost = 3,
            maxRank = 1,
            rarity = PerkRarity.Epic,
            prerequisites = new List<string> { "stamina_boost_1", "health_boost_1" },
            effects = new List<PerkEffect>
            {
                new PerkEffect { type = PerkEffectType.HealthRegeneration, value = 2f }
            }
        });

        // Оновлюємо словник
        foreach (var perk in availablePerks)
        {
            if (!perkDictionary.ContainsKey(perk.id))
            {
                perkDictionary.Add(perk.id, perk);
            }
        }

        Debug.Log($"PerkSystem: Створено {availablePerks.Count} базових перків");
    }

    /// <summary>
    /// Додає XP гравцю
    /// </summary>
    public void AddExperience(int xp)
    {
        if (currentLevel >= maxPlayerLevel) return;

        currentXP += xp;
        OnXPGained?.Invoke(xp);

        // Перевіряємо, чи досягнуто нового рівня
        CheckLevelUp();

        // Відправляємо подію
        Events.Publish(new ExperienceGainedEvent(xp, currentXP, GetXPRequiredForNextLevel()));
    }

    /// <summary>
    /// Перевіряє, чи досягнуто нового рівня
    /// </summary>
    void CheckLevelUp()
    {
        int xpForNextLevel = GetXPRequiredForNextLevel();
        
        while (currentXP >= xpForNextLevel && currentLevel < maxPlayerLevel)
        {
            currentXP -= xpForNextLevel;
            currentLevel++;
            availablePerkPoints++;

            OnLevelUp?.Invoke(currentLevel);
            Events.Publish(new PlayerLevelUpEvent(currentLevel, availablePerkPoints));

            Debug.Log($"PerkSystem: Рівень підвищено до {currentLevel}! Доступно очок перків: {availablePerkPoints}");

            xpForNextLevel = GetXPRequiredForNextLevel();
        }
    }

    /// <summary>
    /// Отримує кількість XP, необхідну для наступного рівня
    /// </summary>
    public int GetXPRequiredForNextLevel()
    {
        if (currentLevel >= maxPlayerLevel) return 0;
        return levelXPRequirements.ContainsKey(currentLevel + 1) ? levelXPRequirements[currentLevel + 1] : 0;
    }

    /// <summary>
    /// Розблоковує перк
    /// </summary>
    public bool UnlockPerk(string perkId)
    {
        // Null checks для безпеки
        if (string.IsNullOrEmpty(perkId))
        {
            Debug.LogWarning("PerkSystem: Спроба розблокувати перк з пустим ID");
            return false;
        }

        if (perkDictionary == null)
        {
            Debug.LogError("PerkSystem: perkDictionary не ініціалізований!");
            return false;
        }

        if (!perkDictionary.ContainsKey(perkId))
        {
            Debug.LogWarning($"PerkSystem: Перк з ID '{perkId}' не знайдено");
            return false;
        }

        Perk perk = perkDictionary[perkId];

        // Перевіряємо вимоги
        if (!CanUnlockPerk(perk))
        {
            Debug.LogWarning($"PerkSystem: Не можна розблокувати перк '{perk.name}' - не виконані вимоги");
            return false;
        }

        // Витрачаємо очки перків
        availablePerkPoints -= perk.cost;

        // Додаємо перк до розблокованих
        if (!unlockedPerkIds.Contains(perkId))
        {
            unlockedPerkIds.Add(perkId);
        }

        // Збільшуємо ранг перка
        perk.currentRank++;

        // Застосовуємо ефекти перка
        ApplyPerkEffects(perk);

        OnPerkUnlocked?.Invoke(perk);
        Events.Publish(new PerkUnlockedEvent(perk));

        Debug.Log($"PerkSystem: Розблоковано перк '{perk.name}' (Ранг {perk.currentRank}/{perk.maxRank})");

        SavePlayerProgress();
        return true;
    }

    /// <summary>
    /// Перевіряє, чи можна розблокувати перк
    /// </summary>
    public bool CanUnlockPerk(Perk perk)
    {
        // Перевіряємо рівень
        if (currentLevel < perk.requiredLevel)
            return false;

        // Перевіряємо очки перків
        if (availablePerkPoints < perk.cost)
            return false;

        // Перевіряємо максимальний ранг
        if (perk.currentRank >= perk.maxRank)
            return false;

        // Перевіряємо передумови
        foreach (string prerequisite in perk.prerequisites)
        {
            if (!unlockedPerkIds.Contains(prerequisite))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Застосовує ефекти перка
    /// </summary>
    void ApplyPerkEffects(Perk perk)
    {
        foreach (var effect in perk.effects)
        {
            float totalValue = effect.value * perk.currentRank;

            switch (effect.type)
            {
                case PerkEffectType.DamageMultiplier:
                    // Інтеграція з WeaponController
                    Events.Publish(new PerkEffectAppliedEvent("damage_multiplier", totalValue));
                    break;

                case PerkEffectType.HealthBonus:
                    // Інтеграція з PlayerHealth
                    Events.Publish(new PerkEffectAppliedEvent("health_bonus", totalValue));
                    break;

                case PerkEffectType.MovementSpeedMultiplier:
                    // Інтеграція з PlayerMovement
                    Events.Publish(new PerkEffectAppliedEvent("movement_speed", totalValue));
                    break;

                case PerkEffectType.ReloadSpeedMultiplier:
                    // Інтеграція з WeaponController
                    Events.Publish(new PerkEffectAppliedEvent("reload_speed", totalValue));
                    break;

                case PerkEffectType.CriticalChance:
                    Events.Publish(new PerkEffectAppliedEvent("critical_chance", totalValue));
                    break;

                case PerkEffectType.StaminaBonus:
                    Events.Publish(new PerkEffectAppliedEvent("stamina_bonus", totalValue));
                    break;

                case PerkEffectType.BerserkerMode:
                    Events.Publish(new PerkEffectAppliedEvent("berserker_mode", totalValue));
                    break;

                case PerkEffectType.HealthRegeneration:
                    Events.Publish(new PerkEffectAppliedEvent("health_regen", totalValue));
                    break;
            }
        }
    }

    /// <summary>
    /// Ініціалізує UI перків
    /// </summary>
    void InitializePerkUI()
    {
        if (perkContainer == null || perkUIPrefab == null) return;

        // Очищаємо контейнер
        foreach (Transform child in perkContainer)
        {
            Destroy(child.gameObject);
        }

        // Створюємо UI для кожного перка
        foreach (var perk in availablePerks.OrderBy(p => p.tier).ThenBy(p => p.requiredLevel))
        {
            GameObject perkUI = Instantiate(perkUIPrefab, perkContainer);
            PerkUIComponent perkComponent = perkUI.GetComponent<PerkUIComponent>();
            
            if (perkComponent != null)
            {
                perkComponent.Initialize(perk, this);
            }
        }
    }

    /// <summary>
    /// Отримує всі доступні перки за категорією
    /// </summary>
    public List<Perk> GetPerksByCategory(PerkCategory category)
    {
        return availablePerks.Where(p => p.category == category).ToList();
    }

    /// <summary>
    /// Отримує всі розблоковані перки
    /// </summary>
    public List<Perk> GetUnlockedPerks()
    {
        return availablePerks.Where(p => unlockedPerkIds.Contains(p.id)).ToList();
    }

    /// <summary>
    /// Зберігає прогрес гравця
    /// </summary>
    void SavePlayerProgress()
    {
        PlayerPrefs.SetInt("PlayerLevel", currentLevel);
        PlayerPrefs.SetInt("PlayerXP", currentXP);
        PlayerPrefs.SetInt("PerkPoints", availablePerkPoints);
        
        // Зберігаємо розблоковані перки
        string unlockedPerks = string.Join(",", unlockedPerkIds);
        PlayerPrefs.SetString("UnlockedPerks", unlockedPerks);

        // Зберігаємо ранги перків
        foreach (var perk in availablePerks)
        {
            PlayerPrefs.SetInt($"PerkRank_{perk.id}", perk.currentRank);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Завантажує прогрес гравця
    /// </summary>
    void LoadPlayerProgress()
    {
        currentLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentXP = PlayerPrefs.GetInt("PlayerXP", 0);
        availablePerkPoints = PlayerPrefs.GetInt("PerkPoints", 0);

        // Завантажуємо розблоковані перки
        string unlockedPerks = PlayerPrefs.GetString("UnlockedPerks", "");
        if (!string.IsNullOrEmpty(unlockedPerks))
        {
            unlockedPerkIds = unlockedPerks.Split(',').ToList();
        }

        // Завантажуємо ранги перків
        foreach (var perk in availablePerks)
        {
            perk.currentRank = PlayerPrefs.GetInt($"PerkRank_{perk.id}", 0);
            
            // Застосовуємо ефекти розблокованих перків
            if (unlockedPerkIds.Contains(perk.id) && perk.currentRank > 0)
            {
                ApplyPerkEffects(perk);
            }
        }

        Debug.Log($"PerkSystem: Завантажено прогрес - Рівень: {currentLevel}, XP: {currentXP}, Очки перків: {availablePerkPoints}");
    }

    // Обробники подій
    public void HandleEvent(PlayerLevelUpEvent eventData)
    {
        // Додаткова логіка при підвищенні рівня
        Debug.Log($"PerkSystem: Обробка події підвищення рівня до {eventData.NewLevel}");
    }

    public void HandleEvent(AchievementUnlockedEvent eventData)
    {
        // Null check для безпеки
        if (eventData?.Achievement == null)
        {
            Debug.LogWarning("PerkSystem: Отримано null achievement event");
            return;
        }

        // Додаємо XP за досягнення
        AddExperience(eventData.Achievement.xpReward);
        Debug.Log($"PerkSystem: Отримано {eventData.Achievement.xpReward} XP за досягнення '{eventData.Achievement.title}'");
    }

    public void HandleEvent(ExperienceGainedEvent eventData)
    {
        // Додаткова логіка при отриманні XP
    }

    /// <summary>
    /// Скидає всі перки (для тестування)
    /// </summary>
    [ContextMenu("Reset All Perks")]
    public void ResetAllPerks()
    {
        unlockedPerkIds.Clear();
        availablePerkPoints = currentLevel - 1; // 1 очко за рівень
        
        foreach (var perk in availablePerks)
        {
            perk.currentRank = 0;
        }

        SavePlayerProgress();
        InitializePerkUI();
        
        Debug.Log("PerkSystem: Всі перки скинуто");
    }

    /// <summary>
    /// Додає очки перків (для тестування)
    /// </summary>
    [ContextMenu("Add 5 Perk Points")]
    public void AddPerkPoints()
    {
        availablePerkPoints += 5;
        Debug.Log($"PerkSystem: Додано 5 очок перків. Всього: {availablePerkPoints}");
    }
}

// ================================
// СТРУКТУРИ ДАНИХ ПЕРКІВ
// ================================

[System.Serializable]
public class Perk
{
    [Header("Basic Info")]
    public string id;
    public string name;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Requirements")]
    public PerkCategory category;
    public int tier = 1;
    public int requiredLevel = 1;
    public int cost = 1;
    public List<string> prerequisites = new List<string>();

    [Header("Progression")]
    public int currentRank = 0;
    public int maxRank = 1;
    public PerkRarity rarity = PerkRarity.Common;

    [Header("Effects")]
    public List<PerkEffect> effects = new List<PerkEffect>();

    // Обчислювані властивості
    public bool IsUnlocked => currentRank > 0;
    public bool IsMaxRank => currentRank >= maxRank;
    public float ProgressPercentage => maxRank > 0 ? (float)currentRank / maxRank * 100f : 0f;
}

[System.Serializable]
public class PerkEffect
{
    public PerkEffectType type;
    public float value;
    public string description;
}

public enum PerkCategory
{
    Combat,     // Бойові перки
    Survival,   // Виживання
    Movement,   // Рух
    Utility,    // Корисність
    Special     // Спеціальні
}

public enum PerkRarity
{
    Common,     // Звичайний (білий)
    Uncommon,   // Незвичайний (зелений)
    Rare,       // Рідкісний (синій)
    Epic,       // Епічний (фіолетовий)
    Legendary   // Легендарний (помаранчевий)
}

public enum PerkEffectType
{
    DamageMultiplier,           // Множник урону
    HealthBonus,                // Бонус здоров'я
    MovementSpeedMultiplier,    // Множник швидкості руху
    ReloadSpeedMultiplier,      // Множник швидкості перезарядки
    CriticalChance,             // Шанс критичного удару
    StaminaBonus,               // Бонус стаміни
    BerserkerMode,              // Режим берсерка
    HealthRegeneration,         // Регенерація здоров'я
    ArmorBonus,                 // Бонус броні
    AccuracyBonus,              // Бонус точності
    FireRateMultiplier,         // Множник швидкості стрільби
    AmmoCapacityMultiplier,     // Множник ємності магазину
    XPMultiplier,               // Множник досвіду
    SpecialAbility              // Спеціальна здібність
}

// ================================
// ПОДІЇ СИСТЕМИ ПЕРКІВ
// ================================

public class PlayerLevelUpEvent : GameEvent
{
    public int NewLevel { get; }
    public int PerkPointsGained { get; }

    public PlayerLevelUpEvent(int newLevel, int perkPointsGained)
    {
        NewLevel = newLevel;
        PerkPointsGained = perkPointsGained;
    }
}

public class PerkUnlockedEvent : GameEvent
{
    public Perk Perk { get; }

    public PerkUnlockedEvent(Perk perk)
    {
        Perk = perk;
    }
}

public class ExperienceGainedEvent : GameEvent
{
    public int XPGained { get; }
    public int TotalXP { get; }
    public int XPToNextLevel { get; }

    public ExperienceGainedEvent(int xpGained, int totalXP, int xpToNextLevel)
    {
        XPGained = xpGained;
        TotalXP = totalXP;
        XPToNextLevel = xpToNextLevel;
    }
}

public class PerkEffectAppliedEvent : GameEvent
{
    public string EffectType { get; }
    public float Value { get; }

    public PerkEffectAppliedEvent(string effectType, float value)
    {
        EffectType = effectType;
        Value = value;
    }
}