using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Базовий клас для всіх конфігурацій гри. Забезпечує валідацію та версіонування.
/// </summary>
public abstract class BaseConfiguration : ScriptableObject
{
    [Header("Configuration Info")]
    [Tooltip("Унікальний ідентифікатор конфігурації")]
    public string configId;
    [Tooltip("Назва конфігурації для відображення")]
    public string displayName;
    [Tooltip("Опис конфігурації")]
    [TextArea(3, 5)]
    public string description;
    [Tooltip("Версія конфігурації")]
    public string version = "1.0.0";
    [Tooltip("Автор конфігурації")]
    public string author = "Game Designer";

    [Header("Configuration Settings")]
    [Tooltip("Чи активна ця конфігурація?")]
    public bool isActive = true;
    [Tooltip("Категорія конфігурації")]
    public string category = "Default";
    [Tooltip("Теги для пошуку")]
    public string[] tags = new string[0];

    /// <summary>
    /// Викликається при валідації в Editor
    /// </summary>
    protected virtual void OnValidate()
    {
        ValidateConfiguration();
    }

    /// <summary>
    /// Абстрактний метод для валідації конфігурації
    /// </summary>
    protected abstract void ValidateConfiguration();

    /// <summary>
    /// Перевіряє, чи конфігурація валідна
    /// </summary>
    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(configId) && !string.IsNullOrEmpty(displayName);
    }

    /// <summary>
    /// Отримує інформацію про конфігурацію
    /// </summary>
    public virtual string GetConfigInfo()
    {
        return $"{displayName} v{version} by {author}";
    }

    /// <summary>
    /// Клонує конфігурацію
    /// </summary>
    public virtual T Clone<T>() where T : BaseConfiguration
    {
        return Instantiate(this) as T;
    }
}

/// <summary>
/// Менеджер конфігурацій для централізованого управління
/// </summary>
[CreateAssetMenu(fileName = "ConfigurationManager", menuName = "Game/Configuration Manager")]
public class ConfigurationManager : ScriptableObject
{
    [Header("Configuration Collections")]
    [Tooltip("Всі конфігурації зброї")]
    public WeaponConfiguration[] weaponConfigurations;
    [Tooltip("Всі конфігурації гравця")]
    public PlayerConfiguration[] playerConfigurations;
    [Tooltip("Глобальні налаштування")]
    public GameSettings gameSettings;
    [Tooltip("Налаштування рівнів")]
    public LevelConfiguration[] levelConfigurations;

    [Header("Manager Settings")]
    [Tooltip("Автоматично завантажувати конфігурації при старті?")]
    public bool autoLoadOnStart = true;
    [Tooltip("Логувати завантаження конфігурацій?")]
    public bool logConfigurationLoading = true;

    // Кеш конфігурацій для швидкого доступу
    private Dictionary<string, BaseConfiguration> configurationCache = new Dictionary<string, BaseConfiguration>();
    private Dictionary<Type, List<BaseConfiguration>> configurationsByType = new Dictionary<Type, List<BaseConfiguration>>();

    // Singleton для глобального доступу
    private static ConfigurationManager instance;
    public static ConfigurationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<ConfigurationManager>("ConfigurationManager");
                if (instance == null)
                {
                    Debug.LogError("ConfigurationManager не знайдено в Resources! Створіть його через меню.");
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Ініціалізує менеджер конфігурацій
    /// </summary>
    public void Initialize()
    {
        BuildConfigurationCache();
        ValidateAllConfigurations();
        
        if (logConfigurationLoading)
        {
            Debug.Log($"ConfigurationManager ініціалізовано. Завантажено {configurationCache.Count} конфігурацій.");
        }
    }

    /// <summary>
    /// Будує кеш конфігурацій
    /// </summary>
    void BuildConfigurationCache()
    {
        configurationCache.Clear();
        configurationsByType.Clear();

        // Додаємо конфігурації зброї
        AddConfigurationsToCache(weaponConfigurations);
        
        // Додаємо конфігурації гравця
        AddConfigurationsToCache(playerConfigurations);
        
        // Додаємо конфігурації рівнів
        AddConfigurationsToCache(levelConfigurations);
        
        // Додаємо глобальні налаштування
        if (gameSettings != null)
        {
            AddConfigurationToCache(gameSettings);
        }
    }

    /// <summary>
    /// Додає масив конфігурацій до кешу
    /// </summary>
    void AddConfigurationsToCache<T>(T[] configurations) where T : BaseConfiguration
    {
        if (configurations == null) return;

        foreach (var config in configurations)
        {
            if (config != null)
            {
                AddConfigurationToCache(config);
            }
        }
    }

    /// <summary>
    /// Додає конфігурацію до кешу
    /// </summary>
    void AddConfigurationToCache(BaseConfiguration config)
    {
        if (config == null || string.IsNullOrEmpty(config.configId)) return;

        // Додаємо до загального кешу
        configurationCache[config.configId] = config;

        // Додаємо до кешу за типом
        Type configType = config.GetType();
        if (!configurationsByType.ContainsKey(configType))
        {
            configurationsByType[configType] = new List<BaseConfiguration>();
        }
        configurationsByType[configType].Add(config);
    }

    /// <summary>
    /// Отримує конфігурацію за ID
    /// </summary>
    public T GetConfiguration<T>(string configId) where T : BaseConfiguration
    {
        if (configurationCache.ContainsKey(configId))
        {
            return configurationCache[configId] as T;
        }
        return null;
    }

    /// <summary>
    /// Отримує всі конфігурації певного типу
    /// </summary>
    public List<T> GetConfigurations<T>() where T : BaseConfiguration
    {
        Type configType = typeof(T);
        if (configurationsByType.ContainsKey(configType))
        {
            return configurationsByType[configType].ConvertAll(config => config as T);
        }
        return new List<T>();
    }

    /// <summary>
    /// Отримує активні конфігурації певного типу
    /// </summary>
    public List<T> GetActiveConfigurations<T>() where T : BaseConfiguration
    {
        var allConfigs = GetConfigurations<T>();
        return allConfigs.FindAll(config => config.isActive);
    }

    /// <summary>
    /// Перевіряє, чи існує конфігурація
    /// </summary>
    public bool HasConfiguration(string configId)
    {
        return configurationCache.ContainsKey(configId);
    }

    /// <summary>
    /// Валідує всі конфігурації
    /// </summary>
    public void ValidateAllConfigurations()
    {
        int validCount = 0;
        int invalidCount = 0;

        foreach (var config in configurationCache.Values)
        {
            if (config.IsValid())
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogWarning($"Невалідна конфігурація: {config.name} ({config.GetType().Name})");
            }
        }

        if (logConfigurationLoading)
        {
            Debug.Log($"Валідація конфігурацій: {validCount} валідних, {invalidCount} невалідних");
        }
    }

    /// <summary>
    /// Отримує статистику конфігурацій
    /// </summary>
    public void GetConfigurationStats(out int totalConfigs, out int activeConfigs, out int configTypes)
    {
        totalConfigs = configurationCache.Count;
        activeConfigs = 0;
        configTypes = configurationsByType.Count;

        foreach (var config in configurationCache.Values)
        {
            if (config.isActive) activeConfigs++;
        }
    }

    /// <summary>
    /// Виводить статистику в консоль
    /// </summary>
    [ContextMenu("Print Configuration Stats")]
    public void PrintConfigurationStats()
    {
        GetConfigurationStats(out int total, out int active, out int types);
        Debug.Log($"Configuration Stats - Total: {total}, Active: {active}, Types: {types}");

        foreach (var kvp in configurationsByType)
        {
            Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} configurations");
        }
    }

    /// <summary>
    /// Перезавантажує всі конфігурації
    /// </summary>
    [ContextMenu("Reload All Configurations")]
    public void ReloadAllConfigurations()
    {
        Initialize();
        Debug.Log("Всі конфігурації перезавантажено");
    }

    void OnValidate()
    {
        // Автоматично присвоюємо ID, якщо вони порожні
        AssignMissingIds(weaponConfigurations);
        AssignMissingIds(playerConfigurations);
        AssignMissingIds(levelConfigurations);
    }

    /// <summary>
    /// Присвоює відсутні ID конфігураціям
    /// </summary>
    void AssignMissingIds<T>(T[] configurations) where T : BaseConfiguration
    {
        if (configurations == null) return;

        for (int i = 0; i < configurations.Length; i++)
        {
            if (configurations[i] != null && string.IsNullOrEmpty(configurations[i].configId))
            {
                configurations[i].configId = $"{typeof(T).Name}_{i:D3}";
            }
        }
    }
}

/// <summary>
/// Атрибут для автоматичної реєстрації конфігурацій
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class)]
public class ConfigurationAttribute : System.Attribute
{
    public string MenuPath { get; }
    public string Category { get; }

    public ConfigurationAttribute(string menuPath, string category = "Default")
    {
        MenuPath = menuPath;
        Category = category;
    }
}

/// <summary>
/// Утилітарний клас для роботи з конфігураціями
/// </summary>
public static class ConfigurationUtility
{
    /// <summary>
    /// Створює нову конфігурацію з базовими налаштуваннями
    /// </summary>
    public static T CreateConfiguration<T>(string id, string displayName) where T : BaseConfiguration
    {
        T config = ScriptableObject.CreateInstance<T>();
        config.configId = id;
        config.displayName = displayName;
        config.version = "1.0.0";
        return config;
    }

    /// <summary>
    /// Валідує ID конфігурації
    /// </summary>
    public static bool IsValidConfigId(string configId)
    {
        return !string.IsNullOrEmpty(configId) && 
               configId.Length >= 3 && 
               !configId.Contains(" ") &&
               System.Text.RegularExpressions.Regex.IsMatch(configId, @"^[a-zA-Z0-9_]+$");
    }

    /// <summary>
    /// Генерує унікальний ID для конфігурації
    /// </summary>
    public static string GenerateUniqueId(string baseName, Type configType)
    {
        string cleanBaseName = baseName.Replace(" ", "_").ToLower();
        string typePrefix = configType.Name.Replace("Configuration", "").ToLower();
        return $"{typePrefix}_{cleanBaseName}_{System.Guid.NewGuid().ToString("N")[..8]}";
    }

    /// <summary>
    /// Копіює значення з однієї конфігурації в іншу
    /// </summary>
    public static void CopyConfiguration<T>(T source, T destination) where T : BaseConfiguration
    {
        if (source == null || destination == null) return;

        var fields = typeof(T).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.Name != "configId") // Не копіюємо ID
            {
                field.SetValue(destination, field.GetValue(source));
            }
        }
    }
}