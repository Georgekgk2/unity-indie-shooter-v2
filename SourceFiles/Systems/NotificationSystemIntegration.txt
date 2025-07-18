using UnityEngine;
using System.Collections;

/// <summary>
/// Інтеграційний компонент для зв'язку старої та нової систем нотифікацій
/// Забезпечує backward compatibility та поступовий перехід
/// </summary>
public class NotificationSystemIntegration : MonoBehaviour
{
    [Header("System References")]
    [Tooltip("Посилання на стару систему нотифікацій")]
    public NotificationSystems oldNotificationSystem;
    [Tooltip("Посилання на нову покращену систему")]
    public EnhancedNotificationSystem enhancedNotificationSystem;
    
    [Header("Migration Settings")]
    [Tooltip("Використовувати нову систему замість старої")]
    public bool useEnhancedSystem = true;
    [Tooltip("Показувати міграційні повідомлення")]
    public bool showMigrationMessages = false;

    public static NotificationSystemIntegration Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeIntegration();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeIntegration()
    {
        // Автоматично знаходимо системи, якщо не призначені
        if (oldNotificationSystem == null)
            oldNotificationSystem = FindObjectOfType<NotificationSystems>();

        if (enhancedNotificationSystem == null)
            enhancedNotificationSystem = FindObjectOfType<EnhancedNotificationSystem>();

        // Перевіряємо доступність систем
        if (useEnhancedSystem && enhancedNotificationSystem == null)
        {
            Debug.LogWarning("NotificationSystemIntegration: Enhanced system не знайдено, використовуємо стару систему");
            useEnhancedSystem = false;
        }

        if (showMigrationMessages)
        {
            string systemInUse = useEnhancedSystem ? "Enhanced" : "Legacy";
            Debug.Log($"NotificationSystemIntegration: Використовується {systemInUse} система нотифікацій");
        }
    }

    /// <summary>
    /// Універсальний метод для показу нотифікацій (backward compatibility)
    /// </summary>
    public void ShowNotification(string message, ModernUISystem.NotificationType legacyType = ModernUISystem.NotificationType.Info, float duration = 3f)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            // Конвертуємо старий тип в новий
            NotificationType newType = ConvertLegacyType(legacyType);
            enhancedNotificationSystem.ShowNotification(message, "", newType, NotificationPriority.Normal, duration);
        }
        else if (oldNotificationSystem != null)
        {
            // Використовуємо стару систему
            oldNotificationSystem.ShowNotification(message, legacyType, duration);
        }
        else
        {
            Debug.LogWarning($"NotificationSystemIntegration: Жодна система нотифікацій не доступна! Повідомлення: {message}");
        }
    }

    /// <summary>
    /// Показує нотифікацію досягнення (спеціальний метод)
    /// </summary>
    public void ShowAchievementNotification(string title, string description, Sprite icon = null)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ShowNotification(
                title, 
                description, 
                NotificationType.Achievement, 
                NotificationPriority.High, 
                6f, 
                icon
            );
        }
        else
        {
            // Fallback до старої системи
            ShowNotification($"{title}: {description}", ModernUISystem.NotificationType.Success, 6f);
        }
    }

    /// <summary>
    /// Показує нотифікацію підвищення рівня
    /// </summary>
    public void ShowLevelUpNotification(int newLevel, int perkPoints)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ShowNotification(
                $"РІВЕНЬ {newLevel}!",
                $"Отримано {perkPoints} очок перків",
                NotificationType.LevelUp,
                NotificationPriority.High,
                5f
            );
        }
        else
        {
            ShowNotification($"Рівень підвищено до {newLevel}! +{perkPoints} очок перків", ModernUISystem.NotificationType.Success, 5f);
        }
    }

    /// <summary>
    /// Показує нотифікацію розблокування перка
    /// </summary>
    public void ShowPerkUnlockedNotification(string perkName, int rank)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ShowNotification(
                "НОВИЙ ПЕРК!",
                $"{perkName} (Ранг {rank})",
                NotificationType.PerkUnlocked,
                NotificationPriority.High,
                4f
            );
        }
        else
        {
            ShowNotification($"Розблоковано перк: {perkName} (Ранг {rank})", ModernUISystem.NotificationType.Success, 4f);
        }
    }

    /// <summary>
    /// Показує нотифікацію отримання XP
    /// </summary>
    public void ShowXPGainedNotification(int xpAmount, int totalXP, int xpToNext)
    {
        // Показуємо тільки значні прирости XP
        if (xpAmount < 50) return;

        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ShowNotification(
                $"+{xpAmount} XP",
                $"Всього: {totalXP} | До наступного: {xpToNext}",
                NotificationType.XPGained,
                NotificationPriority.Normal,
                2f
            );
        }
        else
        {
            ShowNotification($"+{xpAmount} XP (До наступного рівня: {xpToNext})", ModernUISystem.NotificationType.Info, 2f);
        }
    }

    /// <summary>
    /// Показує бойову нотифікацію
    /// </summary>
    public void ShowCombatNotification(string message, bool isImportant = false)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            NotificationPriority priority = isImportant ? NotificationPriority.High : NotificationPriority.Normal;
            enhancedNotificationSystem.ShowNotification(
                message,
                "",
                NotificationType.Combat,
                priority,
                1.5f
            );
        }
        else
        {
            ModernUISystem.NotificationType type = isImportant ? ModernUISystem.NotificationType.Warning : ModernUISystem.NotificationType.Info;
            ShowNotification(message, type, 1.5f);
        }
    }

    /// <summary>
    /// Показує системну нотифікацію
    /// </summary>
    public void ShowSystemNotification(string message, bool isError = false)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            NotificationType type = isError ? NotificationType.Error : NotificationType.System;
            NotificationPriority priority = isError ? NotificationPriority.High : NotificationPriority.Normal;
            
            enhancedNotificationSystem.ShowNotification(
                message,
                "",
                type,
                priority,
                isError ? 5f : 3f
            );
        }
        else
        {
            ModernUISystem.NotificationType type = isError ? ModernUISystem.NotificationType.Error : ModernUISystem.NotificationType.Info;
            ShowNotification(message, type, isError ? 5f : 3f);
        }
    }

    /// <summary>
    /// Показує навчальну нотифікацію
    /// </summary>
    public void ShowTutorialNotification(string title, string description, float duration = 8f)
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ShowNotification(
                title,
                description,
                NotificationType.Tutorial,
                NotificationPriority.High,
                duration
            );
        }
        else
        {
            ShowNotification($"{title}: {description}", ModernUISystem.NotificationType.Info, duration);
        }
    }

    /// <summary>
    /// Конвертує старий тип нотифікації в новий
    /// </summary>
    NotificationType ConvertLegacyType(ModernUISystem.NotificationType legacyType)
    {
        switch (legacyType)
        {
            case ModernUISystem.NotificationType.Info:
                return NotificationType.Info;
            case ModernUISystem.NotificationType.Warning:
                return NotificationType.Warning;
            case ModernUISystem.NotificationType.Error:
                return NotificationType.Error;
            case ModernUISystem.NotificationType.Success:
                return NotificationType.Success;
            default:
                return NotificationType.Info;
        }
    }

    /// <summary>
    /// Очищає всі нотифікації в активній системі
    /// </summary>
    public void ClearAllNotifications()
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            enhancedNotificationSystem.ClearAllNotifications();
        }
        else if (oldNotificationSystem != null)
        {
            // Якщо стара система має метод очищення
            // oldNotificationSystem.ClearAllNotifications();
        }
    }

    /// <summary>
    /// Перемикає між системами (для тестування)
    /// </summary>
    [ContextMenu("Toggle Notification System")]
    public void ToggleNotificationSystem()
    {
        useEnhancedSystem = !useEnhancedSystem;
        
        string newSystem = useEnhancedSystem ? "Enhanced" : "Legacy";
        Debug.Log($"NotificationSystemIntegration: Перемкнуто на {newSystem} систему");
        
        // Показуємо тестову нотифікацію
        ShowNotification($"Тепер використовується {newSystem} система нотифікацій", ModernUISystem.NotificationType.Info);
    }

    /// <summary>
    /// Тестує всі типи нотифікацій
    /// </summary>
    [ContextMenu("Test All Notification Types")]
    public void TestAllNotificationTypes()
    {
        StartCoroutine(TestNotificationsSequence());
    }

    IEnumerator TestNotificationsSequence()
    {
        ShowNotification("Тестова інформаційна нотифікація", ModernUISystem.NotificationType.Info);
        yield return new WaitForSeconds(1f);

        ShowNotification("Тестове попередження", ModernUISystem.NotificationType.Warning);
        yield return new WaitForSeconds(1f);

        ShowNotification("Тестова помилка", ModernUISystem.NotificationType.Error);
        yield return new WaitForSeconds(1f);

        ShowNotification("Тестовий успіх", ModernUISystem.NotificationType.Success);
        yield return new WaitForSeconds(1f);

        ShowAchievementNotification("Тестове досягнення", "Ви успішно протестували систему!");
        yield return new WaitForSeconds(1f);

        ShowLevelUpNotification(25, 3);
        yield return new WaitForSeconds(1f);

        ShowPerkUnlockedNotification("Тестовий перк", 2);
        yield return new WaitForSeconds(1f);

        ShowXPGainedNotification(150, 2500, 350);
        yield return new WaitForSeconds(1f);

        ShowCombatNotification("ХЕДШОТ! +50 XP", true);
        yield return new WaitForSeconds(1f);

        ShowSystemNotification("Гра збережена");
        yield return new WaitForSeconds(1f);

        ShowTutorialNotification("Підказка", "Натисніть P для відкриття меню перків");
    }

    /// <summary>
    /// Отримує статистику активної системи
    /// </summary>
    public void ShowNotificationStatistics()
    {
        if (useEnhancedSystem && enhancedNotificationSystem != null)
        {
            var stats = enhancedNotificationSystem.GetNotificationStatistics();
            Debug.Log("=== СТАТИСТИКА НОТИФІКАЦІЙ ===");
            foreach (var kvp in stats)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
        else
        {
            Debug.Log("Статистика доступна тільки для Enhanced системи");
        }
    }
}

/// <summary>
/// Статичний helper клас для легкого доступу до нотифікацій
/// </summary>
public static class NotificationHelper
{
    /// <summary>
    /// Швидкий доступ до показу нотифікації
    /// </summary>
    public static void Show(string message, ModernUISystem.NotificationType type = ModernUISystem.NotificationType.Info, float duration = 3f)
    {
        if (NotificationSystemIntegration.Instance != null)
        {
            NotificationSystemIntegration.Instance.ShowNotification(message, type, duration);
        }
        else
        {
            Debug.Log($"[NOTIFICATION] {message}");
        }
    }

    /// <summary>
    /// Швидкий доступ до показу досягнення
    /// </summary>
    public static void ShowAchievement(string title, string description, Sprite icon = null)
    {
        if (NotificationSystemIntegration.Instance != null)
        {
            NotificationSystemIntegration.Instance.ShowAchievementNotification(title, description, icon);
        }
        else
        {
            Debug.Log($"[ACHIEVEMENT] {title}: {description}");
        }
    }

    /// <summary>
    /// Швидкий доступ до показу підвищення рівня
    /// </summary>
    public static void ShowLevelUp(int level, int perkPoints)
    {
        if (NotificationSystemIntegration.Instance != null)
        {
            NotificationSystemIntegration.Instance.ShowLevelUpNotification(level, perkPoints);
        }
        else
        {
            Debug.Log($"[LEVEL UP] Рівень {level}! +{perkPoints} очок перків");
        }
    }

    /// <summary>
    /// Швидкий доступ до показу XP
    /// </summary>
    public static void ShowXP(int amount, int total, int toNext)
    {
        if (NotificationSystemIntegration.Instance != null)
        {
            NotificationSystemIntegration.Instance.ShowXPGainedNotification(amount, total, toNext);
        }
        else
        {
            Debug.Log($"[XP] +{amount} (До наступного: {toNext})");
        }
    }

    /// <summary>
    /// Швидкий доступ до бойових нотифікацій
    /// </summary>
    public static void ShowCombat(string message, bool important = false)
    {
        if (NotificationSystemIntegration.Instance != null)
        {
            NotificationSystemIntegration.Instance.ShowCombatNotification(message, important);
        }
        else
        {
            Debug.Log($"[COMBAT] {message}");
        }
    }
}