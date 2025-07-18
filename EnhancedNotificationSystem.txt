using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Покращена система нотифікацій з інтеграцією до системи перків та досягнень
/// ФАЗА 3.3: Розширення існуючої системи нотифікацій
/// </summary>

// ================================
// РОЗШИРЕНІ ТИПИ НОТИФІКАЦІЙ
// ================================

public enum EnhancedNotificationType
{
    Info,           // Загальна інформація
    Warning,        // Попередження
    Error,          // Помилки
    Success,        // Успішні дії
    Achievement,    // Досягнення (з особливими ефектами)
    LevelUp,        // Підвищення рівня
    PerkUnlocked,   // Розблокування перка
    XPGained,       // Отримання досвіду
    Combat,         // Бойові повідомлення
    System,         // Системні повідомлення
    Tutorial        // Навчальні підказки
}

public enum NotificationPriority
{
    Low = 0,        // Можна пропустити
    Normal = 1,     // Звичайний пріоритет
    High = 2,       // Важливе повідомлення
    Critical = 3    // Критично важливе
}

// ================================
// ПОКРАЩЕНА СИСТЕМА НОТИФІКАЦІЙ
// ================================

public class EnhancedNotificationSystem : MonoBehaviour, 
    IEventHandler<PlayerLevelUpEvent>,
    IEventHandler<PerkUnlockedEvent>,
    IEventHandler<ExperienceGainedEvent>,
    IEventHandler<AchievementUnlockedEvent>,
    IEventHandler<EnemyKilledEvent>
{
    [Header("Notification Settings")]
    [Tooltip("Контейнер для нотифікацій")]
    public Transform notificationContainer;
    [Tooltip("Префаб стандартної нотифікації")]
    public GameObject standardNotificationPrefab;
    [Tooltip("Префаб нотифікації досягнення")]
    public GameObject achievementNotificationPrefab;
    [Tooltip("Префаб нотифікації підвищення рівня")]
    public GameObject levelUpNotificationPrefab;
    [Tooltip("Максимальна кількість активних нотифікацій")]
    public int maxActiveNotifications = 5;

    [Header("Animation Settings")]
    [Tooltip("Тривалість анімації появи")]
    public float slideInDuration = 0.5f;
    [Tooltip("Тривалість анімації зникнення")]
    public float slideOutDuration = 0.3f;
    [Tooltip("Крива анімації")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio Settings")]
    [Tooltip("Звуки для різних типів нотифікацій")]
    public NotificationAudioClips audioClips;

    [Header("Visual Effects")]
    [Tooltip("Ефекти частинок для спеціальних нотифікацій")]
    public ParticleSystem achievementParticles;
    [Tooltip("Ефекти частинок для підвищення рівня")]
    public ParticleSystem levelUpParticles;

    // Singleton
    public static EnhancedNotificationSystem Instance { get; private set; }

    // Приватні змінні
    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
    private List<NotificationItem> activeNotifications = new List<NotificationItem>();
    private bool isProcessingQueue = false;

    // Статистика
    private int totalNotificationsShown = 0;
    private Dictionary<NotificationType, int> notificationStats = new Dictionary<NotificationType, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystem();
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
        Events.Subscribe<PerkUnlockedEvent>(this);
        Events.Subscribe<ExperienceGainedEvent>(this);
        Events.Subscribe<AchievementUnlockedEvent>(this);
        Events.Subscribe<EnemyKilledEvent>(this);

        // Запускаємо обробку черги
        StartCoroutine(ProcessNotificationQueue());
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PlayerLevelUpEvent>(this);
        Events.Unsubscribe<PerkUnlockedEvent>(this);
        Events.Unsubscribe<ExperienceGainedEvent>(this);
        Events.Unsubscribe<AchievementUnlockedEvent>(this);
        Events.Unsubscribe<EnemyKilledEvent>(this);
    }

    void InitializeSystem()
    {
        // Ініціалізуємо статистику
        foreach (NotificationType type in System.Enum.GetValues(typeof(NotificationType)))
        {
            notificationStats[type] = 0;
        }

        Debug.Log("EnhancedNotificationSystem: Система ініціалізована");
    }

    /// <summary>
    /// Показує нотифікацію з розширеними параметрами
    /// </summary>
    public void ShowNotification(string title, string message = "", EnhancedNotificationType type = EnhancedNotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, float duration = 3f, Sprite icon = null, 
        System.Action onClicked = null)
    {
        NotificationData notification = new NotificationData
        {
            title = title,
            message = message,
            type = type,
            priority = priority,
            duration = duration,
            icon = icon,
            onClicked = onClicked,
            timestamp = Time.time
        };

        // Додаємо до черги з урахуванням пріоритету
        AddToQueueWithPriority(notification);
    }

    /// <summary>
    /// Додає нотифікацію до черги з урахуванням пріоритету
    /// </summary>
    void AddToQueueWithPriority(NotificationData notification)
    {
        // Якщо критичний пріоритет, показуємо негайно
        if (notification.priority == NotificationPriority.Critical)
        {
            // Видаляємо найстарішу нотифікацію, якщо потрібно
            if (activeNotifications.Count >= maxActiveNotifications)
            {
                RemoveOldestNotification();
            }
            
            CreateNotificationItem(notification);
            return;
        }

        // Інакше додаємо до черги
        notificationQueue.Enqueue(notification);
    }

    /// <summary>
    /// Обробка черги нотифікацій
    /// </summary>
    IEnumerator ProcessNotificationQueue()
    {
        while (true)
        {
            if (notificationQueue.Count > 0 && activeNotifications.Count < maxActiveNotifications)
            {
                NotificationData notification = notificationQueue.Dequeue();
                CreateNotificationItem(notification);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Створює UI елемент нотифікації
    /// </summary>
    void CreateNotificationItem(NotificationData data)
    {
        GameObject prefab = GetPrefabForType(data.type);
        if (prefab == null || notificationContainer == null) return;

        GameObject notificationObj = Instantiate(prefab, notificationContainer);
        NotificationItem item = notificationObj.GetComponent<NotificationItem>();

        if (item == null)
        {
            item = notificationObj.AddComponent<NotificationItem>();
        }

        item.Initialize(data, this);
        activeNotifications.Add(item);

        // Анімація появи
        StartCoroutine(AnimateNotificationIn(item));

        // Звуковий ефект
        PlayNotificationSound(data.type);

        // Візуальні ефекти
        PlayVisualEffects(data.type);

        // Статистика
        UpdateStatistics(data.type);

        // Автоматичне видалення
        if (data.duration > 0)
        {
            StartCoroutine(RemoveNotificationAfterDelay(item, data.duration));
        }
    }

    /// <summary>
    /// Отримує префаб для типу нотифікації
    /// </summary>
    GameObject GetPrefabForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Achievement:
                return achievementNotificationPrefab ?? standardNotificationPrefab;
            case NotificationType.LevelUp:
                return levelUpNotificationPrefab ?? standardNotificationPrefab;
            default:
                return standardNotificationPrefab;
        }
    }

    /// <summary>
    /// Анімація появи нотифікації
    /// </summary>
    IEnumerator AnimateNotificationIn(NotificationItem item)
    {
        if (item == null) yield break;

        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Початкова позиція (за межами екрану)
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 targetPos = startPos;
        startPos.x += rectTransform.rect.width;

        rectTransform.anchoredPosition = startPos;

        // Анімація
        float elapsedTime = 0f;
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideInDuration;
            float easedProgress = animationCurve.Evaluate(progress);

            rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedProgress);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }

    /// <summary>
    /// Анімація зникнення нотифікації
    /// </summary>
    IEnumerator AnimateNotificationOut(NotificationItem item)
    {
        if (item == null) yield break;

        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 targetPos = startPos;
        targetPos.x += rectTransform.rect.width;

        // Анімація
        float elapsedTime = 0f;
        while (elapsedTime < slideOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideOutDuration;

            rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }

        // Видаляємо з списку та знищуємо
        activeNotifications.Remove(item);
        if (item.gameObject != null)
        {
            Destroy(item.gameObject);
        }
    }

    /// <summary>
    /// Видаляє нотифікацію після затримки
    /// </summary>
    IEnumerator RemoveNotificationAfterDelay(NotificationItem item, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveNotification(item);
    }

    /// <summary>
    /// Видаляє конкретну нотифікацію
    /// </summary>
    public void RemoveNotification(NotificationItem item)
    {
        if (item != null && activeNotifications.Contains(item))
        {
            StartCoroutine(AnimateNotificationOut(item));
        }
    }

    /// <summary>
    /// Видаляє найстарішу нотифікацію
    /// </summary>
    void RemoveOldestNotification()
    {
        if (activeNotifications.Count > 0)
        {
            RemoveNotification(activeNotifications[0]);
        }
    }

    /// <summary>
    /// Відтворює звук нотифікації
    /// </summary>
    void PlayNotificationSound(EnhancedNotificationType type)
    {
        if (audioClips == null || AudioManager.Instance == null) return;

        AudioClip clip = audioClips.GetClipForType(type);
        if (clip != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
    }

    /// <summary>
    /// Відтворює візуальні ефекти
    /// </summary>
    void PlayVisualEffects(EnhancedNotificationType type)
    {
        switch (type)
        {
            case NotificationType.Achievement:
                if (achievementParticles != null)
                    achievementParticles.Play();
                break;
            case NotificationType.LevelUp:
                if (levelUpParticles != null)
                    levelUpParticles.Play();
                break;
        }
    }

    /// <summary>
    /// Оновлює статистику
    /// </summary>
    void UpdateStatistics(EnhancedNotificationType type)
    {
        totalNotificationsShown++;
        if (notificationStats.ContainsKey(type))
        {
            notificationStats[type]++;
        }
    }

    // ================================
    // ОБРОБНИКИ ПОДІЙ
    // ================================

    public void HandleEvent(PlayerLevelUpEvent eventData)
    {
        ShowNotification(
            $"РІВЕНЬ {eventData.NewLevel}!",
            $"Отримано {eventData.PerkPointsGained} очок перків",
            NotificationType.LevelUp,
            NotificationPriority.High,
            5f
        );
    }

    public void HandleEvent(PerkUnlockedEvent eventData)
    {
        ShowNotification(
            "НОВИЙ ПЕРК!",
            $"{eventData.PerkName} (Ранг {eventData.NewRank})",
            NotificationType.PerkUnlocked,
            NotificationPriority.High,
            4f
        );
    }

    public void HandleEvent(ExperienceGainedEvent eventData)
    {
        // Показуємо тільки значні прирости XP
        if (eventData.XPGained >= 50)
        {
            ShowNotification(
                $"+{eventData.XPGained} XP",
                $"До наступного рівня: {eventData.XPToNextLevel}",
                NotificationType.XPGained,
                NotificationPriority.Normal,
                2f
            );
        }
    }

    public void HandleEvent(AchievementUnlockedEvent eventData)
    {
        ShowNotification(
            "ДОСЯГНЕННЯ РОЗБЛОКОВАНО!",
            eventData.Achievement.title,
            NotificationType.Achievement,
            NotificationPriority.High,
            6f,
            eventData.Achievement.icon
        );
    }

    public void HandleEvent(EnemyKilledEvent eventData)
    {
        // Показуємо тільки спеціальні вбивства
        if (eventData.IsHeadshot)
        {
            ShowNotification(
                "ХЕДШОТ!",
                $"+{eventData.XPReward} XP",
                NotificationType.Combat,
                NotificationPriority.Normal,
                1.5f
            );
        }
    }

    // ================================
    // ПУБЛІЧНІ МЕТОДИ
    // ================================

    /// <summary>
    /// Очищає всі активні нотифікації
    /// </summary>
    public void ClearAllNotifications()
    {
        foreach (var item in activeNotifications.ToArray())
        {
            RemoveNotification(item);
        }
        notificationQueue.Clear();
    }

    /// <summary>
    /// Отримує статистику нотифікацій
    /// </summary>
    public Dictionary<NotificationType, int> GetNotificationStatistics()
    {
        return new Dictionary<NotificationType, int>(notificationStats);
    }

    /// <summary>
    /// Показує швидку нотифікацію (без черги)
    /// </summary>
    public void ShowQuickNotification(string message, NotificationType type = NotificationType.Info)
    {
        ShowNotification(message, "", type, NotificationPriority.Critical, 2f);
    }
}

// ================================
// СТРУКТУРИ ДАНИХ
// ================================

[System.Serializable]
public struct NotificationData
{
    public string title;
    public string message;
    public EnhancedNotificationType type;
    public NotificationPriority priority;
    public float duration;
    public Sprite icon;
    public System.Action onClicked;
    public float timestamp;
}

[System.Serializable]
public class NotificationAudioClips
{
    [Header("Audio Clips")]
    public AudioClip infoSound;
    public AudioClip warningSound;
    public AudioClip errorSound;
    public AudioClip successSound;
    public AudioClip achievementSound;
    public AudioClip levelUpSound;
    public AudioClip perkUnlockedSound;
    public AudioClip xpGainedSound;
    public AudioClip combatSound;
    public AudioClip systemSound;
    public AudioClip tutorialSound;

    public AudioClip GetClipForType(EnhancedNotificationType type)
    {
        switch (type)
        {
            case NotificationType.Info: return infoSound;
            case NotificationType.Warning: return warningSound;
            case NotificationType.Error: return errorSound;
            case NotificationType.Success: return successSound;
            case NotificationType.Achievement: return achievementSound;
            case NotificationType.LevelUp: return levelUpSound;
            case NotificationType.PerkUnlocked: return perkUnlockedSound;
            case NotificationType.XPGained: return xpGainedSound;
            case NotificationType.Combat: return combatSound;
            case NotificationType.System: return systemSound;
            case NotificationType.Tutorial: return tutorialSound;
            default: return infoSound;
        }
    }
}

// ================================
// КОМПОНЕНТ НОТИФІКАЦІЇ
// ================================

public class NotificationItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image iconImage;
    public Image backgroundImage;
    public Button closeButton;

    private NotificationData data;
    private EnhancedNotificationSystem notificationSystem;

    public void Initialize(NotificationData notificationData, EnhancedNotificationSystem system)
    {
        data = notificationData;
        notificationSystem = system;

        // Налаштовуємо UI
        if (titleText != null)
            titleText.text = data.title;

        if (messageText != null)
            messageText.text = data.message;

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.gameObject.SetActive(true);
        }
        else if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        // Налаштовуємо колір фону
        if (backgroundImage != null)
        {
            backgroundImage.color = GetColorForType(data.type);
        }

        // Налаштовуємо кнопку закриття
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => {
                if (notificationSystem != null)
                    notificationSystem.RemoveNotification(this);
            });
        }

        // Обробник кліку
        Button mainButton = GetComponent<Button>();
        if (mainButton != null && data.onClicked != null)
        {
            mainButton.onClick.AddListener(() => data.onClicked());
        }
    }

    Color GetColorForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Info: return new Color(0.2f, 0.6f, 1f, 0.9f);
            case NotificationType.Warning: return new Color(1f, 0.8f, 0.2f, 0.9f);
            case NotificationType.Error: return new Color(1f, 0.3f, 0.3f, 0.9f);
            case NotificationType.Success: return new Color(0.3f, 1f, 0.3f, 0.9f);
            case NotificationType.Achievement: return new Color(1f, 0.6f, 0.2f, 0.9f);
            case NotificationType.LevelUp: return new Color(0.8f, 0.2f, 1f, 0.9f);
            case NotificationType.PerkUnlocked: return new Color(0.2f, 1f, 0.8f, 0.9f);
            case NotificationType.XPGained: return new Color(0.6f, 0.8f, 1f, 0.9f);
            case NotificationType.Combat: return new Color(1f, 0.4f, 0.4f, 0.9f);
            case NotificationType.System: return new Color(0.7f, 0.7f, 0.7f, 0.9f);
            case NotificationType.Tutorial: return new Color(0.9f, 0.9f, 0.3f, 0.9f);
            default: return Color.white;
        }
    }
}