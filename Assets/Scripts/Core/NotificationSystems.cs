using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Система повідомлень та досягнень з анімаціями та чергою відображення.
/// </summary>

// ================================
// ПАНЕЛЬ ПОВІДОМЛЕНЬ
// ================================

[System.Serializable]
public class NotificationPanel : UIComponent
{
    [Header("Notification UI")]
    public Transform notificationContainer;
    public GameObject notificationPrefab;
    public int maxNotifications = 5;
    public float defaultDuration = 3f;
    public float animationDuration = 0.3f;

    [Header("Notification Colors")]
    public Color infoColor = Color.blue;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    public Color successColor = Color.green;

    private Queue<NotificationData> notificationQueue = new Queue<NotificationData>();
    private List<NotificationItem> activeNotifications = new List<NotificationItem>();
    private bool isProcessingQueue = false;

    [System.Serializable]
    public struct NotificationData
    {
        public string message;
        public ModernUISystem.NotificationType type;
        public float duration;
        public Sprite icon;

        public NotificationData(string msg, ModernUISystem.NotificationType notType, float dur, Sprite ico = null)
        {
            message = msg;
            type = notType;
            duration = dur;
            icon = ico;
        }
    }

    public void Initialize()
    {
        if (notificationContainer == null)
        {
            Debug.LogError("NotificationPanel: notificationContainer не призначено!");
            return;
        }

        StartCoroutine(ProcessNotificationQueue());
    }

    public void ShowNotification(string message, ModernUISystem.NotificationType type = ModernUISystem.NotificationType.Info, float duration = 0f, Sprite icon = null)
    {
        if (duration <= 0f)
            duration = defaultDuration;

        NotificationData notification = new NotificationData(message, type, duration, icon);
        notificationQueue.Enqueue(notification);
    }

    IEnumerator ProcessNotificationQueue()
    {
        while (true)
        {
            if (notificationQueue.Count > 0 && activeNotifications.Count < maxNotifications)
            {
                NotificationData notification = notificationQueue.Dequeue();
                yield return StartCoroutine(DisplayNotification(notification));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DisplayNotification(NotificationData notification)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("NotificationPanel: notificationPrefab не призначено!");
            yield break;
        }

        // Створюємо новий об'єкт повідомлення
        GameObject notificationObj = Instantiate(notificationPrefab, notificationContainer);
        NotificationItem notificationItem = notificationObj.GetComponent<NotificationItem>();

        if (notificationItem == null)
        {
            notificationItem = notificationObj.AddComponent<NotificationItem>();
        }

        // Налаштовуємо повідомлення
        notificationItem.Setup(notification.message, GetColorForType(notification.type), notification.icon);
        activeNotifications.Add(notificationItem);

        // Анімація появи
        yield return StartCoroutine(AnimateNotificationIn(notificationItem));

        // Чекаємо тривалість показу
        yield return new WaitForSeconds(notification.duration);

        // Анімація зникнення
        yield return StartCoroutine(AnimateNotificationOut(notificationItem));

        // Видаляємо з списку та знищуємо
        activeNotifications.Remove(notificationItem);
        if (notificationObj != null)
        {
            Destroy(notificationObj);
        }

        // Переміщуємо інші повідомлення вгору
        StartCoroutine(ReorganizeNotifications());
    }

    IEnumerator AnimateNotificationIn(NotificationItem notification)
    {
        if (notification == null) yield break;

        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = notification.gameObject.AddComponent<CanvasGroup>();
        }

        // Початкові значення
        Vector3 startPos = rectTransform.localPosition + Vector3.right * 300f;
        Vector3 endPos = rectTransform.localPosition;
        canvasGroup.alpha = 0f;

        rectTransform.localPosition = startPos;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rectTransform.localPosition = endPos;
        canvasGroup.alpha = 1f;
    }

    IEnumerator AnimateNotificationOut(NotificationItem notification)
    {
        if (notification == null) yield break;

        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();

        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + Vector3.right * 300f;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }
    }

    IEnumerator ReorganizeNotifications()
    {
        for (int i = 0; i < activeNotifications.Count; i++)
        {
            if (activeNotifications[i] != null)
            {
                RectTransform rectTransform = activeNotifications[i].GetComponent<RectTransform>();
                Vector3 targetPos = new Vector3(0f, -i * 80f, 0f); // 80 пікселів між повідомленнями

                StartCoroutine(MoveNotificationToPosition(rectTransform, targetPos));
            }
        }
        yield return null;
    }

    IEnumerator MoveNotificationToPosition(RectTransform rectTransform, Vector3 targetPos)
    {
        Vector3 startPos = rectTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration * 0.5f)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / (animationDuration * 0.5f);
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        rectTransform.localPosition = targetPos;
    }

    Color GetColorForType(ModernUISystem.NotificationType type)
    {
        switch (type)
        {
            case ModernUISystem.NotificationType.Info:
                return infoColor;
            case ModernUISystem.NotificationType.Warning:
                return warningColor;
            case ModernUISystem.NotificationType.Error:
                return errorColor;
            case ModernUISystem.NotificationType.Success:
                return successColor;
            default:
                return infoColor;
        }
    }

    public void ClearAllNotifications()
    {
        notificationQueue.Clear();
        
        foreach (var notification in activeNotifications)
        {
            if (notification != null && notification.gameObject != null)
            {
                Destroy(notification.gameObject);
            }
        }
        
        activeNotifications.Clear();
    }
}

// ================================
// ЕЛЕМЕНТ ПОВІДОМЛЕННЯ
// ================================

public class NotificationItem : MonoBehaviour
{
    [Header("Notification Item UI")]
    public TextMeshProUGUI messageText;
    public Image backgroundImage;
    public Image iconImage;
    public Button closeButton;

    public void Setup(string message, Color backgroundColor, Sprite icon = null)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseNotification);
        }
    }

    public void CloseNotification()
    {
        // Знаходимо батьківську панель та видаляємо це повідомлення
        NotificationPanel parentPanel = GetComponentInParent<NotificationPanel>();
        if (parentPanel != null)
        {
            parentPanel.activeNotifications.Remove(this);
            StartCoroutine(parentPanel.ReorganizeNotifications());
        }

        Destroy(gameObject);
    }
}

// ================================
// ПАНЕЛЬ ДОСЯГНЕНЬ
// ================================

[System.Serializable]
public class AchievementPanel : UIComponent
{
    [Header("Achievement UI")]
    public Transform achievementContainer;
    public GameObject achievementPrefab;
    public float displayDuration = 4f;
    public float animationDuration = 0.5f;

    [Header("Achievement Settings")]
    public AudioClip achievementSound;
    public Color achievementColor = Color.gold;

    private Queue<AchievementData> achievementQueue = new Queue<AchievementData>();
    private bool isDisplayingAchievement = false;

    [System.Serializable]
    public struct AchievementData
    {
        public string title;
        public string description;
        public Sprite icon;

        public AchievementData(string t, string d, Sprite i)
        {
            title = t;
            description = d;
            icon = i;
        }
    }

    public void Initialize()
    {
        StartCoroutine(ProcessAchievementQueue());
    }

    public void ShowAchievement(string title, string description, Sprite icon = null)
    {
        AchievementData achievement = new AchievementData(title, description, icon);
        achievementQueue.Enqueue(achievement);
    }

    IEnumerator ProcessAchievementQueue()
    {
        while (true)
        {
            if (achievementQueue.Count > 0 && !isDisplayingAchievement)
            {
                AchievementData achievement = achievementQueue.Dequeue();
                yield return StartCoroutine(DisplayAchievement(achievement));
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator DisplayAchievement(AchievementData achievement)
    {
        isDisplayingAchievement = true;

        // Відтворюємо звук досягнення
        if (achievementSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound2D(achievementSound, AudioManager.AudioType.SFX, 0.8f);
        }

        // Створюємо об'єкт досягнення
        GameObject achievementObj = Instantiate(achievementPrefab, achievementContainer);
        AchievementItem achievementItem = achievementObj.GetComponent<AchievementItem>();

        if (achievementItem == null)
        {
            achievementItem = achievementObj.AddComponent<AchievementItem>();
        }

        // Налаштовуємо досягнення
        achievementItem.Setup(achievement.title, achievement.description, achievement.icon, achievementColor);

        // Анімація появи
        yield return StartCoroutine(AnimateAchievementIn(achievementItem));

        // Показуємо протягом встановленого часу
        yield return new WaitForSeconds(displayDuration);

        // Анімація зникнення
        yield return StartCoroutine(AnimateAchievementOut(achievementItem));

        // Знищуємо об'єкт
        if (achievementObj != null)
        {
            Destroy(achievementObj);
        }

        isDisplayingAchievement = false;
    }

    IEnumerator AnimateAchievementIn(AchievementItem achievement)
    {
        if (achievement == null) yield break;

        RectTransform rectTransform = achievement.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = achievement.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = achievement.gameObject.AddComponent<CanvasGroup>();
        }

        // Початкові значення - зверху екрану
        Vector3 startPos = rectTransform.localPosition + Vector3.up * 200f;
        Vector3 endPos = rectTransform.localPosition;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one;

        rectTransform.localPosition = startPos;
        rectTransform.localScale = startScale;
        canvasGroup.alpha = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            
            // Використовуємо bounce easing
            t = BounceEaseOut(t);

            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        rectTransform.localPosition = endPos;
        rectTransform.localScale = endScale;
        canvasGroup.alpha = 1f;
    }

    IEnumerator AnimateAchievementOut(AchievementItem achievement)
    {
        if (achievement == null) yield break;

        RectTransform rectTransform = achievement.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = achievement.GetComponent<CanvasGroup>();

        Vector3 startPos = rectTransform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 200f;
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = Vector3.zero;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }
    }

    // Bounce easing function
    float BounceEaseOut(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        }
        else
        {
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }
}

// ================================
// ЕЛЕМЕНТ ДОСЯГНЕННЯ
// ================================

public class AchievementItem : MonoBehaviour
{
    [Header("Achievement Item UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image backgroundImage;
    public Image iconImage;
    public Image borderImage;

    public void Setup(string title, string description, Sprite icon, Color backgroundColor)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        if (borderImage != null)
        {
            borderImage.color = backgroundColor * 1.2f; // Трохи світліший для рамки
        }

        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
    }
}

// ================================
// СИСТЕМА ДОСЯГНЕНЬ
// ================================

public class AchievementSystem : MonoBehaviour,
    IEventHandler<WeaponFiredEvent>,
    IEventHandler<PlayerDeathEvent>,
    IEventHandler<WeaponReloadCompletedEvent>
{
    [Header("Achievement Settings")]
    public AchievementData[] availableAchievements;

    [Header("Achievement Icons")]
    public Sprite defaultAchievementIcon;
    public Sprite weaponAchievementIcon;
    public Sprite survivalAchievementIcon;
    public Sprite skillAchievementIcon;

    // Статистика гравця
    private int totalShots = 0;
    private int totalReloads = 0;
    private int totalDeaths = 0;
    private float totalPlayTime = 0f;

    // Отримані досягнення
    private HashSet<string> unlockedAchievements = new HashSet<string>();

    public static AchievementSystem Instance { get; private set; }

    [System.Serializable]
    public struct AchievementData
    {
        public string id;
        public string title;
        public string description;
        public Sprite icon;
        public AchievementType type;
        public int targetValue;
        public bool isSecret;
    }

    public enum AchievementType
    {
        TotalShots,
        TotalReloads,
        SurvivalTime,
        NoDeaths,
        FirstKill,
        Marksman
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        Events.Subscribe<PlayerDeathEvent>(this);
        Events.Subscribe<WeaponReloadCompletedEvent>(this);

        LoadAchievementProgress();
    }

    void Update()
    {
        totalPlayTime += Time.deltaTime;
        CheckTimeBasedAchievements();
    }

    public void HandleEvent(WeaponFiredEvent eventData)
    {
        totalShots++;
        CheckShotBasedAchievements();
    }

    public void HandleEvent(PlayerDeathEvent eventData)
    {
        totalDeaths++;
    }

    public void HandleEvent(WeaponReloadCompletedEvent eventData)
    {
        totalReloads++;
        CheckReloadBasedAchievements();
    }

    void CheckShotBasedAchievements()
    {
        foreach (var achievement in availableAchievements)
        {
            if (achievement.type == AchievementType.TotalShots && 
                totalShots >= achievement.targetValue &&
                !unlockedAchievements.Contains(achievement.id))
            {
                UnlockAchievement(achievement);
            }
        }
    }

    void CheckReloadBasedAchievements()
    {
        foreach (var achievement in availableAchievements)
        {
            if (achievement.type == AchievementType.TotalReloads && 
                totalReloads >= achievement.targetValue &&
                !unlockedAchievements.Contains(achievement.id))
            {
                UnlockAchievement(achievement);
            }
        }
    }

    void CheckTimeBasedAchievements()
    {
        foreach (var achievement in availableAchievements)
        {
            if (achievement.type == AchievementType.SurvivalTime && 
                totalPlayTime >= achievement.targetValue &&
                !unlockedAchievements.Contains(achievement.id))
            {
                UnlockAchievement(achievement);
            }
        }
    }

    void UnlockAchievement(AchievementData achievement)
    {
        unlockedAchievements.Add(achievement.id);
        
        // Показуємо досягнення
        if (ModernUISystem.Instance?.achievementPanel != null)
        {
            Sprite icon = achievement.icon != null ? achievement.icon : GetDefaultIconForType(achievement.type);
            ModernUISystem.Instance.achievementPanel.ShowAchievement(achievement.title, achievement.description, icon);
        }

        // Зберігаємо прогрес
        SaveAchievementProgress();

        Debug.Log($"Achievement Unlocked: {achievement.title}");
    }

    Sprite GetDefaultIconForType(AchievementType type)
    {
        switch (type)
        {
            case AchievementType.TotalShots:
            case AchievementType.TotalReloads:
            case AchievementType.Marksman:
                return weaponAchievementIcon;
            case AchievementType.SurvivalTime:
            case AchievementType.NoDeaths:
                return survivalAchievementIcon;
            default:
                return defaultAchievementIcon;
        }
    }

    void SaveAchievementProgress()
    {
        PlayerPrefs.SetInt("TotalShots", totalShots);
        PlayerPrefs.SetInt("TotalReloads", totalReloads);
        PlayerPrefs.SetInt("TotalDeaths", totalDeaths);
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);

        // Зберігаємо отримані досягнення
        string achievementsString = string.Join(",", unlockedAchievements);
        PlayerPrefs.SetString("UnlockedAchievements", achievementsString);
        
        PlayerPrefs.Save();
    }

    void LoadAchievementProgress()
    {
        totalShots = PlayerPrefs.GetInt("TotalShots", 0);
        totalReloads = PlayerPrefs.GetInt("TotalReloads", 0);
        totalDeaths = PlayerPrefs.GetInt("TotalDeaths", 0);
        totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);

        // Завантажуємо отримані досягнення
        string achievementsString = PlayerPrefs.GetString("UnlockedAchievements", "");
        if (!string.IsNullOrEmpty(achievementsString))
        {
            string[] achievementIds = achievementsString.Split(',');
            foreach (string id in achievementIds)
            {
                unlockedAchievements.Add(id);
            }
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return unlockedAchievements.Contains(achievementId);
    }

    public float GetAchievementProgress(string achievementId)
    {
        var achievement = System.Array.Find(availableAchievements, a => a.id == achievementId);
        if (achievement.id == null) return 0f;

        switch (achievement.type)
        {
            case AchievementType.TotalShots:
                return Mathf.Clamp01((float)totalShots / achievement.targetValue);
            case AchievementType.TotalReloads:
                return Mathf.Clamp01((float)totalReloads / achievement.targetValue);
            case AchievementType.SurvivalTime:
                return Mathf.Clamp01(totalPlayTime / achievement.targetValue);
            default:
                return 0f;
        }
    }

    void OnDestroy()
    {
        SaveAchievementProgress();
        
        Events.Unsubscribe<WeaponFiredEvent>(this);
        Events.Unsubscribe<PlayerDeathEvent>(this);
        Events.Unsubscribe<WeaponReloadCompletedEvent>(this);
    }
}