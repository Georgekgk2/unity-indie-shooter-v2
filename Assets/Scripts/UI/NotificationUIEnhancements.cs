using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Покращені UI компоненти для системи нотифікацій
/// Включає адаптивний дизайн, анімації та спеціальні ефекти
/// </summary>

// ================================
// АДАПТИВНИЙ КОНТЕЙНЕР НОТИФІКАЦІЙ
// ================================

public class AdaptiveNotificationContainer : MonoBehaviour
{
    [Header("Layout Settings")]
    [Tooltip("Відступ від краю екрану")]
    public Vector2 screenMargin = new Vector2(20f, 20f);
    [Tooltip("Відстань між нотифікаціями")]
    public float notificationSpacing = 10f;
    [Tooltip("Максимальна ширина нотифікації")]
    public float maxNotificationWidth = 400f;
    [Tooltip("Мінімальна ширина нотифікації")]
    public float minNotificationWidth = 250f;

    [Header("Responsive Design")]
    [Tooltip("Автоматично адаптуватися до розміру екрану")]
    public bool enableResponsiveDesign = true;
    [Tooltip("Точки перелому для різних розмірів екрану")]
    public ResponsiveBreakpoints breakpoints;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        
        if (enableResponsiveDesign)
        {
            UpdateLayoutForScreenSize();
        }
    }

    void Update()
    {
        if (enableResponsiveDesign && Time.frameCount % 30 == 0) // Перевіряємо кожні 30 кадрів
        {
            UpdateLayoutForScreenSize();
        }
    }

    void UpdateLayoutForScreenSize()
    {
        if (parentCanvas == null) return;

        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        
        // Визначаємо поточний breakpoint
        ResponsiveSettings settings = GetSettingsForScreenSize(screenSize);
        
        // Оновлюємо позицію контейнера
        UpdateContainerPosition(settings);
        
        // Оновлюємо розміри нотифікацій
        UpdateNotificationSizes(settings);
    }

    ResponsiveSettings GetSettingsForScreenSize(Vector2 screenSize)
    {
        if (screenSize.x <= breakpoints.mobile.maxWidth)
            return breakpoints.mobile;
        else if (screenSize.x <= breakpoints.tablet.maxWidth)
            return breakpoints.tablet;
        else
            return breakpoints.desktop;
    }

    void UpdateContainerPosition(ResponsiveSettings settings)
    {
        rectTransform.anchorMin = settings.anchorMin;
        rectTransform.anchorMax = settings.anchorMax;
        rectTransform.anchoredPosition = settings.position;
    }

    void UpdateNotificationSizes(ResponsiveSettings settings)
    {
        // Оновлюємо розміри всіх дочірніх нотифікацій
        foreach (Transform child in transform)
        {
            RectTransform childRect = child.GetComponent<RectTransform>();
            if (childRect != null)
            {
                childRect.sizeDelta = new Vector2(settings.notificationWidth, childRect.sizeDelta.y);
            }
        }
    }
}

[System.Serializable]
public struct ResponsiveBreakpoints
{
    public ResponsiveSettings mobile;
    public ResponsiveSettings tablet;
    public ResponsiveSettings desktop;
}

[System.Serializable]
public struct ResponsiveSettings
{
    public float maxWidth;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 position;
    public float notificationWidth;
}

// ================================
// ПОКРАЩЕНИЙ КОМПОНЕНТ НОТИФІКАЦІЇ
// ================================

public class EnhancedNotificationItem : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform mainContainer;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image iconImage;
    public Image backgroundImage;
    public Image progressBar;
    public Button actionButton;
    public Button closeButton;
    public ParticleSystem particleEffect;

    [Header("Animation Settings")]
    public float slideInDuration = 0.5f;
    public float slideOutDuration = 0.3f;
    public AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    public bool enableGlowEffect = true;
    public bool enableShakeOnImportant = true;
    public bool enablePulseAnimation = false;

    private NotificationData data;
    private EnhancedNotificationSystem notificationSystem;
    private Coroutine currentAnimation;
    private bool isInitialized = false;

    public void Initialize(NotificationData notificationData, EnhancedNotificationSystem system)
    {
        data = notificationData;
        notificationSystem = system;
        isInitialized = true;

        SetupUI();
        SetupAnimations();
        SetupInteractions();
    }

    void SetupUI()
    {
        // Налаштовуємо текст
        if (titleText != null)
        {
            titleText.text = data.title;
            titleText.color = GetTextColorForType(data.type);
        }

        if (messageText != null)
        {
            messageText.text = data.message;
            messageText.gameObject.SetActive(!string.IsNullOrEmpty(data.message));
        }

        // Налаштовуємо іконку
        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.sprite = GetDefaultIconForType(data.type);
                iconImage.gameObject.SetActive(iconImage.sprite != null);
            }
        }

        // Налаштовуємо фон
        if (backgroundImage != null)
        {
            backgroundImage.color = GetBackgroundColorForType(data.type);
            
            if (enableGlowEffect && IsImportantType(data.type))
            {
                StartCoroutine(GlowEffect());
            }
        }

        // Налаштовуємо прогрес бар (для XP нотифікацій)
        if (progressBar != null)
        {
            bool showProgress = data.type == NotificationType.XPGained || data.type == NotificationType.LevelUp;
            progressBar.gameObject.SetActive(showProgress);
            
            if (showProgress)
            {
                StartCoroutine(AnimateProgressBar());
            }
        }

        // Налаштовуємо кнопки
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (actionButton != null)
        {
            bool hasAction = data.onClicked != null;
            actionButton.gameObject.SetActive(hasAction);
            
            if (hasAction)
            {
                actionButton.onClick.AddListener(OnActionClicked);
            }
        }
    }

    void SetupAnimations()
    {
        // Початкова позиція для анімації
        if (mainContainer != null)
        {
            Vector3 startPos = mainContainer.anchoredPosition;
            startPos.x += mainContainer.rect.width;
            mainContainer.anchoredPosition = startPos;
        }

        // Спеціальні ефекти для важливих нотифікацій
        if (enableShakeOnImportant && data.priority == NotificationPriority.Critical)
        {
            StartCoroutine(ShakeEffect());
        }

        if (enablePulseAnimation && IsImportantType(data.type))
        {
            StartCoroutine(PulseEffect());
        }
    }

    void SetupInteractions()
    {
        // Hover ефекти
        var eventTrigger = gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Hover enter
        var hoverEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => OnHoverEnter());
        eventTrigger.triggers.Add(hoverEnter);

        // Hover exit
        var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        hoverExit.callback.AddListener((data) => OnHoverExit());
        eventTrigger.triggers.Add(hoverExit);
    }

    public void PlaySlideInAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(SlideInAnimation());
    }

    public void PlaySlideOutAnimation(System.Action onComplete = null)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(SlideOutAnimation(onComplete));
    }

    IEnumerator SlideInAnimation()
    {
        if (mainContainer == null) yield break;

        Vector3 startPos = mainContainer.anchoredPosition;
        Vector3 targetPos = startPos;
        targetPos.x -= mainContainer.rect.width;

        float elapsedTime = 0f;
        while (elapsedTime < slideInDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideInDuration;
            float easedProgress = slideInCurve.Evaluate(progress);

            mainContainer.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedProgress);
            yield return null;
        }

        mainContainer.anchoredPosition = targetPos;

        // Запускаємо ефекти частинок
        if (particleEffect != null && IsImportantType(data.type))
        {
            particleEffect.Play();
        }
    }

    IEnumerator SlideOutAnimation(System.Action onComplete = null)
    {
        if (mainContainer == null) yield break;

        Vector3 startPos = mainContainer.anchoredPosition;
        Vector3 targetPos = startPos;
        targetPos.x += mainContainer.rect.width;

        float elapsedTime = 0f;
        while (elapsedTime < slideOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / slideOutDuration;
            float easedProgress = slideOutCurve.Evaluate(progress);

            mainContainer.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedProgress);
            yield return null;
        }

        onComplete?.Invoke();
    }

    IEnumerator GlowEffect()
    {
        if (backgroundImage == null) yield break;

        Color originalColor = backgroundImage.color;
        Color glowColor = originalColor;
        glowColor.a *= 1.5f;

        while (isInitialized)
        {
            // Glow in
            float elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                backgroundImage.color = Color.Lerp(originalColor, glowColor, elapsedTime);
                yield return null;
            }

            // Glow out
            elapsedTime = 0f;
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                backgroundImage.color = Color.Lerp(glowColor, originalColor, elapsedTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator ShakeEffect()
    {
        if (mainContainer == null) yield break;

        Vector3 originalPos = mainContainer.anchoredPosition;
        float shakeIntensity = 5f;
        float shakeDuration = 0.5f;

        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0
            );

            mainContainer.anchoredPosition = originalPos + shakeOffset;
            yield return null;
        }

        mainContainer.anchoredPosition = originalPos;
    }

    IEnumerator PulseEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 pulseScale = originalScale * 1.1f;

        while (isInitialized)
        {
            // Pulse up
            float elapsedTime = 0f;
            while (elapsedTime < 0.5f)
            {
                elapsedTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(originalScale, pulseScale, elapsedTime / 0.5f);
                yield return null;
            }

            // Pulse down
            elapsedTime = 0f;
            while (elapsedTime < 0.5f)
            {
                elapsedTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(pulseScale, originalScale, elapsedTime / 0.5f);
                yield return null;
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator AnimateProgressBar()
    {
        if (progressBar == null) yield break;

        progressBar.fillAmount = 0f;
        
        float targetFill = 0.7f; // Приклад прогресу
        float duration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            progressBar.fillAmount = Mathf.Lerp(0f, targetFill, elapsedTime / duration);
            yield return null;
        }
    }

    void OnHoverEnter()
    {
        if (mainContainer != null)
        {
            LeanTween.scale(mainContainer.gameObject, Vector3.one * 1.05f, 0.2f)
                .setEase(LeanTweenType.easeOutBack);
        }
    }

    void OnHoverExit()
    {
        if (mainContainer != null)
        {
            LeanTween.scale(mainContainer.gameObject, Vector3.one, 0.2f)
                .setEase(LeanTweenType.easeOutBack);
        }
    }

    void OnCloseClicked()
    {
        if (notificationSystem != null)
        {
            notificationSystem.RemoveNotification(this.GetComponent<NotificationItem>());
        }
    }

    void OnActionClicked()
    {
        data.onClicked?.Invoke();
        OnCloseClicked(); // Закриваємо після виконання дії
    }

    // Допоміжні методи
    Color GetTextColorForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Error: return Color.white;
            case NotificationType.Warning: return Color.black;
            default: return Color.white;
        }
    }

    Color GetBackgroundColorForType(NotificationType type)
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

    Sprite GetDefaultIconForType(NotificationType type)
    {
        // Тут можна завантажити іконки з Resources або використати атлас
        return null; // Поки що повертаємо null
    }

    bool IsImportantType(NotificationType type)
    {
        return type == NotificationType.Achievement || 
               type == NotificationType.LevelUp || 
               type == NotificationType.Error ||
               type == NotificationType.PerkUnlocked;
    }

    void OnDestroy()
    {
        isInitialized = false;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
    }
}

// ================================
// МЕНЕДЖЕР ЗВУКОВИХ ЕФЕКТІВ
// ================================

public class NotificationAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] infoSounds;
    public AudioClip[] warningSounds;
    public AudioClip[] errorSounds;
    public AudioClip[] successSounds;
    public AudioClip[] achievementSounds;
    public AudioClip[] levelUpSounds;
    public AudioClip[] combatSounds;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float notificationVolume = 0.8f;
    public bool enableSpatialAudio = false;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupAudioSource();
    }

    void SetupAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume * notificationVolume;
        audioSource.spatialBlend = enableSpatialAudio ? 1f : 0f;
    }

    public void PlayNotificationSound(NotificationType type)
    {
        AudioClip[] clips = GetClipsForType(type);
        if (clips != null && clips.Length > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            if (randomClip != null)
            {
                audioSource.PlayOneShot(randomClip, masterVolume * notificationVolume);
            }
        }
    }

    AudioClip[] GetClipsForType(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Info: return infoSounds;
            case NotificationType.Warning: return warningSounds;
            case NotificationType.Error: return errorSounds;
            case NotificationType.Success: return successSounds;
            case NotificationType.Achievement: return achievementSounds;
            case NotificationType.LevelUp: return levelUpSounds;
            case NotificationType.Combat: return combatSounds;
            default: return infoSounds;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        audioSource.volume = masterVolume * notificationVolume;
    }

    public void SetNotificationVolume(float volume)
    {
        notificationVolume = Mathf.Clamp01(volume);
        audioSource.volume = masterVolume * notificationVolume;
    }
}