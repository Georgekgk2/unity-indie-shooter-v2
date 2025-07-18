using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI компонент для відображення popup досягнень (Claude рекомендація)
/// Показує анімовані повідомлення про розблоковані досягнення
/// </summary>
public class AchievementPopup : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Іконка досягнення")]
    public Image achievementIcon;
    [Tooltip("Заголовок досягнення")]
    public TextMeshProUGUI achievementTitle;
    [Tooltip("Опис досягнення")]
    public TextMeshProUGUI achievementDescription;
    [Tooltip("Текст XP нагороди")]
    public TextMeshProUGUI xpRewardText;
    [Tooltip("Фон popup")]
    public Image backgroundImage;
    [Tooltip("Рамка для рідкості")]
    public Image rarityBorder;
    [Tooltip("Ефект частинок")]
    public ParticleSystem celebrationEffect;
    
    [Header("Animation Settings")]
    [Tooltip("Тривалість анімації появи")]
    public float slideInDuration = 0.5f;
    [Tooltip("Тривалість анімації зникнення")]
    public float slideOutDuration = 0.3f;
    [Tooltip("Затримка перед зникненням")]
    public float displayDuration = 3f;
    [Tooltip("Відстань слайду")]
    public float slideDistance = 300f;
    
    [Header("Audio")]
    [Tooltip("Звук появи popup")]
    public AudioClip popupSound;
    [Tooltip("AudioSource для звуків")]
    public AudioSource audioSource;
    
    [Header("Colors")]
    [Tooltip("Кольори фону для різних рідкостей")]
    public Color[] rarityBackgroundColors = new Color[]
    {
        new Color(0.8f, 0.8f, 0.8f, 0.9f), // Common
        new Color(0.6f, 0.9f, 0.6f, 0.9f), // Uncommon
        new Color(0.6f, 0.6f, 0.9f, 0.9f), // Rare
        new Color(0.9f, 0.6f, 0.9f, 0.9f), // Epic
        new Color(0.9f, 0.9f, 0.6f, 0.9f)  // Legendary
    };
    
    // Приватні змінні
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector3 offScreenPosition;
    private bool isAnimating = false;
    
    void Awake()
    {
        // Отримуємо компоненти
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Налаштовуємо початковий стан
        originalPosition = rectTransform.anchoredPosition;
        offScreenPosition = originalPosition + Vector3.right * slideDistance;
        
        // Починаємо за межами екрану
        rectTransform.anchoredPosition = offScreenPosition;
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Показує досягнення з анімацією
    /// </summary>
    public void ShowAchievement(Achievement achievement, float duration = 3f)
    {
        if (isAnimating) return;
        
        displayDuration = duration;
        
        // Налаштовуємо UI елементи
        SetupAchievementUI(achievement);
        
        // Запускаємо анімацію
        StartCoroutine(ShowAchievementCoroutine());
        
        Debug.Log($"AchievementPopup: Показуємо досягнення '{achievement.title}'");
    }
    
    /// <summary>
    /// Налаштовує UI елементи для досягнення
    /// </summary>
    void SetupAchievementUI(Achievement achievement)
    {
        // Заголовок
        if (achievementTitle != null)
        {
            achievementTitle.text = achievement.title;
            achievementTitle.color = achievement.GetRarityColor();
        }
        
        // Опис
        if (achievementDescription != null)
        {
            achievementDescription.text = achievement.description;
        }
        
        // XP нагорода
        if (xpRewardText != null)
        {
            xpRewardText.text = $"+{achievement.xpReward} XP";
            xpRewardText.color = Color.yellow;
        }
        
        // Іконка
        if (achievementIcon != null)
        {
            if (achievement.icon != null)
            {
                achievementIcon.sprite = achievement.icon;
            }
            else
            {
                // Використовуємо стандартну іконку залежно від категорії
                achievementIcon.sprite = GetDefaultIcon(achievement.category);
            }
        }
        
        // Фон та рамка залежно від рідкості
        SetupRarityVisuals(achievement.rarity);
        
        // Ефект частинок для рідкісних досягнень
        if (celebrationEffect != null && achievement.rarity >= AchievementRarity.Rare)
        {
            var main = celebrationEffect.main;
            main.startColor = achievement.GetRarityColor();
            celebrationEffect.Play();
        }
    }
    
    /// <summary>
    /// Налаштовує візуальні елементи залежно від рідкості
    /// </summary>
    void SetupRarityVisuals(AchievementRarity rarity)
    {
        int rarityIndex = (int)rarity;
        
        // Фон
        if (backgroundImage != null && rarityIndex < rarityBackgroundColors.Length)
        {
            backgroundImage.color = rarityBackgroundColors[rarityIndex];
        }
        
        // Рамка
        if (rarityBorder != null)
        {
            rarityBorder.color = GetRarityColor(rarity);
            
            // Додаємо свічення для епічних та легендарних
            if (rarity >= AchievementRarity.Epic)
            {
                StartCoroutine(PulseBorder());
            }
        }
    }
    
    /// <summary>
    /// Повертає колір для рідкості
    /// </summary>
    Color GetRarityColor(AchievementRarity rarity)
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
    /// Повертає стандартну іконку для категорії
    /// </summary>
    Sprite GetDefaultIcon(AchievementCategory category)
    {
        // Тут можна завантажити іконки з Resources або використати стандартні
        // Поки що повертаємо null - буде використана стандартна іконка UI
        return null;
    }
    
    /// <summary>
    /// Корутина для показу досягнення
    /// </summary>
    IEnumerator ShowAchievementCoroutine()
    {
        isAnimating = true;
        
        // Звук появи
        if (popupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(popupSound);
        }
        
        // Анімація появи
        yield return StartCoroutine(SlideIn());
        
        // Затримка показу
        yield return new WaitForSeconds(displayDuration);
        
        // Анімація зникнення
        yield return StartCoroutine(SlideOut());
        
        isAnimating = false;
        
        // Знищуємо об'єкт
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Анімація появи
    /// </summary>
    IEnumerator SlideIn()
    {
        float elapsed = 0f;
        Vector3 startPos = offScreenPosition;
        Vector3 endPos = originalPosition;
        
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideInDuration;
            
            // Easing функція для плавної анімації
            float easedProgress = EaseOutBack(progress);
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, easedProgress);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Анімація зникнення
    /// </summary>
    IEnumerator SlideOut()
    {
        float elapsed = 0f;
        Vector3 startPos = originalPosition;
        Vector3 endPos = originalPosition + Vector3.left * slideDistance;
        
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideOutDuration;
            
            // Easing функція для плавної анімації
            float easedProgress = EaseInBack(progress);
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, easedProgress);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = endPos;
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Пульсація рамки для рідкісних досягнень
    /// </summary>
    IEnumerator PulseBorder()
    {
        if (rarityBorder == null) yield break;
        
        Color originalColor = rarityBorder.color;
        float pulseSpeed = 2f;
        float elapsed = 0f;
        
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            
            float pulse = Mathf.Sin(elapsed * pulseSpeed) * 0.3f + 0.7f;
            Color pulseColor = originalColor * pulse;
            pulseColor.a = originalColor.a;
            
            rarityBorder.color = pulseColor;
            
            yield return null;
        }
        
        rarityBorder.color = originalColor;
    }
    
    /// <summary>
    /// Easing функція - Ease Out Back
    /// </summary>
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    /// <summary>
    /// Easing функція - Ease In Back
    /// </summary>
    float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        
        return c3 * t * t * t - c1 * t * t;
    }
    
    /// <summary>
    /// Дозволяє закрити popup вручну (наприклад, кліком)
    /// </summary>
    public void ClosePopup()
    {
        if (!isAnimating)
        {
            StartCoroutine(SlideOut());
        }
    }
    
    /// <summary>
    /// Встановлює позицію popup (для множинних popup)
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        originalPosition = position;
        offScreenPosition = position + Vector3.right * slideDistance;
        
        if (!isAnimating)
        {
            rectTransform.anchoredPosition = offScreenPosition;
        }
    }
    
    /// <summary>
    /// Змінює тривалість показу
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        displayDuration = duration;
    }
    
    void OnDestroy()
    {
        // Зупиняємо всі корутини
        StopAllCoroutines();
        
        // Зупиняємо ефект частинок
        if (celebrationEffect != null)
        {
            celebrationEffect.Stop();
        }
    }
}