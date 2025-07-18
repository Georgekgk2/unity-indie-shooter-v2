using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Менеджер ініціалізації всіх систем гри
/// Вирішує проблему множинних Singleton'ів та циклічних залежностей
/// </summary>
public class SystemInitializationManager : MonoBehaviour
{
    [Header("System References")]
    [Tooltip("Порядок ініціалізації систем (важливо!)")]
    public List<MonoBehaviour> systemsToInitialize = new List<MonoBehaviour>();
    
    [Header("Initialization Settings")]
    [Tooltip("Затримка між ініціалізацією систем (секунди)")]
    public float initializationDelay = 0.1f;
    [Tooltip("Показувати прогрес ініціалізації")]
    public bool showInitializationProgress = true;
    
    // Singleton
    public static SystemInitializationManager Instance { get; private set; }
    
    // Статус ініціалізації
    private Dictionary<System.Type, bool> systemInitializationStatus = new Dictionary<System.Type, bool>();
    private bool allSystemsInitialized = false;
    
    // Події
    public static event System.Action<System.Type> OnSystemInitialized;
    public static event System.Action OnAllSystemsInitialized;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Починаємо ініціалізацію
            StartCoroutine(InitializeSystemsSequentially());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Послідовна ініціалізація всіх систем
    /// </summary>
    IEnumerator InitializeSystemsSequentially()
    {
        Debug.Log("SystemInitializationManager: Початок ініціалізації систем...");
        
        // Фаза 1: Базові системи (без залежностей)
        yield return StartCoroutine(InitializePhase1());
        
        // Фаза 2: Системи з залежностями
        yield return StartCoroutine(InitializePhase2());
        
        // Фаза 3: UI та інтеграційні системи
        yield return StartCoroutine(InitializePhase3());
        
        // Завершення
        allSystemsInitialized = true;
        OnAllSystemsInitialized?.Invoke();
        
        Debug.Log("SystemInitializationManager: Всі системи ініціалізовано успішно!");
    }
    
    /// <summary>
    /// Фаза 1: Базові системи
    /// </summary>
    IEnumerator InitializePhase1()
    {
        Debug.Log("SystemInitializationManager: Фаза 1 - Базові системи");
        
        // 1. Event System (найважливіший)
        yield return InitializeSystem<Events>();
        
        // 2. Audio Manager
        yield return InitializeSystem<AudioManager>();
        
        // 3. Input Manager
        yield return InitializeSystem<InputManager>();
        
        // 4. Bullet Pool
        yield return InitializeSystem<BulletPool>();
        
        Debug.Log("SystemInitializationManager: Фаза 1 завершена");
    }
    
    /// <summary>
    /// Фаза 2: Системи з залежностями
    /// </summary>
    IEnumerator InitializePhase2()
    {
        Debug.Log("SystemInitializationManager: Фаза 2 - Системи з залежностями");
        
        // 1. Achievement Manager (перший, бо від нього залежить PerkSystem)
        yield return InitializeSystem<AchievementManager>();
        
        // 2. Perk System (залежить від AchievementManager)
        yield return InitializeSystem<PerkSystem>();
        
        // 3. Level System
        yield return InitializeSystem<LevelSystem>();
        
        // 4. Dynamic Music Manager
        yield return InitializeSystem<DynamicMusicManager>();
        
        Debug.Log("SystemInitializationManager: Фаза 2 завершена");
    }
    
    /// <summary>
    /// Фаза 3: UI та інтеграційні системи
    /// </summary>
    IEnumerator InitializePhase3()
    {
        Debug.Log("SystemInitializationManager: Фаза 3 - UI та інтеграція");
        
        // 1. Notification Systems
        yield return InitializeSystem<NotificationSystem>();
        
        // 2. Perk Integration Manager
        yield return InitializeSystem<PerkIntegrationManager>();
        
        // 3. Modern UI System
        yield return InitializeSystem<ModernUISystem>();
        
        // 4. Menu Systems
        yield return InitializeSystem<MenuSystems>();
        
        Debug.Log("SystemInitializationManager: Фаза 3 завершена");
    }
    
    /// <summary>
    /// Ініціалізує конкретну систему
    /// </summary>
    IEnumerator InitializeSystem<T>() where T : MonoBehaviour
    {
        System.Type systemType = typeof(T);
        
        if (showInitializationProgress)
            Debug.Log($"SystemInitializationManager: Ініціалізація {systemType.Name}...");
        
        // Знаходимо систему
        T system = FindObjectOfType<T>();
        
        if (system == null)
        {
            Debug.LogWarning($"SystemInitializationManager: Система {systemType.Name} не знайдена!");
            yield break;
        }
        
        // Чекаємо, поки система ініціалізується
        yield return new WaitForSeconds(initializationDelay);
        
        // Перевіряємо, чи система має Instance (для Singleton'ів)
        try
        {
            var instanceProperty = systemType.GetProperty("Instance", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            if (instanceProperty != null)
            {
                // Чекаємо, поки Instance не буде null
                int timeout = 0;
                while (instanceProperty.GetValue(null) == null && timeout < 100)
                {
                    timeout++;
                    yield return new WaitForEndOfFrame();
                }
                
                if (timeout >= 100)
                {
                    Debug.LogWarning($"SystemInitializationManager: Timeout waiting for {systemType.Name} Instance");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SystemInitializationManager: Error checking Instance for {systemType.Name}: {e.Message}");
        }
        
        // Позначаємо як ініціалізовану
        systemInitializationStatus[systemType] = true;
        OnSystemInitialized?.Invoke(systemType);
        
        if (showInitializationProgress)
            Debug.Log($"SystemInitializationManager: {systemType.Name} ініціалізовано ✅");
    }
    
    /// <summary>
    /// Перевіряє, чи ініціалізована система
    /// </summary>
    public bool IsSystemInitialized<T>() where T : MonoBehaviour
    {
        System.Type systemType = typeof(T);
        return systemInitializationStatus.ContainsKey(systemType) && systemInitializationStatus[systemType];
    }
    
    /// <summary>
    /// Перевіряє, чи всі системи ініціалізовані
    /// </summary>
    public bool AreAllSystemsInitialized()
    {
        return allSystemsInitialized;
    }
    
    /// <summary>
    /// Отримує прогрес ініціалізації (0-1)
    /// </summary>
    public float GetInitializationProgress()
    {
        if (systemsToInitialize.Count == 0) return 1f;
        
        int initializedCount = 0;
        foreach (var kvp in systemInitializationStatus)
        {
            if (kvp.Value) initializedCount++;
        }
        
        return (float)initializedCount / systemsToInitialize.Count;
    }
    
    /// <summary>
    /// Чекає, поки система не буде ініціалізована
    /// </summary>
    public IEnumerator WaitForSystem<T>() where T : MonoBehaviour
    {
        while (!IsSystemInitialized<T>())
        {
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// Чекає, поки всі системи не будуть ініціалізовані
    /// </summary>
    public IEnumerator WaitForAllSystems()
    {
        while (!allSystemsInitialized)
        {
            yield return new WaitForEndOfFrame();
        }
    }
    
    /// <summary>
    /// Безпечне отримання Instance системи
    /// </summary>
    public static T GetSystemInstance<T>() where T : MonoBehaviour
    {
        if (Instance == null || !Instance.IsSystemInitialized<T>())
        {
            Debug.LogWarning($"SystemInitializationManager: Система {typeof(T).Name} ще не ініціалізована!");
            return null;
        }
        
        // Використовуємо рефлексію для отримання Instance
        var instanceProperty = typeof(T).GetProperty("Instance", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        if (instanceProperty != null)
        {
            return instanceProperty.GetValue(null) as T;
        }
        
        return FindObjectOfType<T>();
    }
    
    /// <summary>
    /// Виконує дію, коли система буде готова
    /// </summary>
    public void ExecuteWhenSystemReady<T>(System.Action<T> action) where T : MonoBehaviour
    {
        StartCoroutine(ExecuteWhenSystemReadyCoroutine(action));
    }
    
    IEnumerator ExecuteWhenSystemReadyCoroutine<T>(System.Action<T> action) where T : MonoBehaviour
    {
        yield return WaitForSystem<T>();
        
        T system = GetSystemInstance<T>();
        if (system != null)
        {
            action(system);
        }
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    // Методи для налагодження
    [ContextMenu("Show Initialization Status")]
    void ShowInitializationStatus()
    {
        Debug.Log("=== СТАТУС ІНІЦІАЛІЗАЦІЇ СИСТЕМ ===");
        foreach (var kvp in systemInitializationStatus)
        {
            string status = kvp.Value ? "✅ Готово" : "❌ Очікування";
            Debug.Log($"{kvp.Key.Name}: {status}");
        }
        Debug.Log($"Загальний прогрес: {GetInitializationProgress() * 100:F1}%");
    }
    
    [ContextMenu("Force Initialize All")]
    void ForceInitializeAll()
    {
        if (!allSystemsInitialized)
        {
            StopAllCoroutines();
            StartCoroutine(InitializeSystemsSequentially());
        }
    }
}