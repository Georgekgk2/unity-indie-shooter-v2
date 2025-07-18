using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Приклад компонента UI, який використовує Event System для відображення здоров'я гравця.
/// Демонструє, як компоненти можуть підписуватися на події та реагувати на них.
/// </summary>
public class UIHealthDisplay : MonoBehaviour, 
    IEventHandler<PlayerHealthChangedEvent>, 
    IEventHandler<PlayerDeathEvent>,
    IEventHandler<PlayerRespawnEvent>
{
    [Header("UI References")]
    [Tooltip("Slider для відображення здоров'я")]
    public Slider healthSlider;
    [Tooltip("Text для відображення числового значення здоров'я")]
    public Text healthText;
    [Tooltip("Image для зміни кольору залежно від рівня здоров'я")]
    public Image healthFillImage;

    [Header("Visual Settings")]
    [Tooltip("Колір при повному здоров'ї")]
    public Color fullHealthColor = Color.green;
    [Tooltip("Колір при середньому здоров'ї")]
    public Color mediumHealthColor = Color.yellow;
    [Tooltip("Колір при низькому здоров'ї")]
    public Color lowHealthColor = Color.red;
    [Tooltip("Поріг низького здоров'я (у відсотках)")]
    [Range(0f, 1f)]
    public float lowHealthThreshold = 0.25f;
    [Tooltip("Поріг середнього здоров'я (у відсотках)")]
    [Range(0f, 1f)]
    public float mediumHealthThreshold = 0.5f;

    [Header("Animation Settings")]
    [Tooltip("Швидкість анімації зміни здоров'я")]
    public float animationSpeed = 2f;
    [Tooltip("Чи анімувати зміни здоров'я?")]
    public bool animateChanges = true;

    // Приватні змінні
    private float targetHealthPercentage = 1f;
    private float currentDisplayedPercentage = 1f;
    private bool isAnimating = false;

    void Start()
    {
        // Підписуємося на події при старті
        Events.Subscribe<PlayerHealthChangedEvent>(this);
        Events.Subscribe<PlayerDeathEvent>(this);
        Events.Subscribe<PlayerRespawnEvent>(this);

        // Ініціалізуємо UI
        InitializeHealthDisplay();
    }

    void OnDestroy()
    {
        // Відписуємося від подій при знищенні
        Events.Unsubscribe<PlayerHealthChangedEvent>(this);
        Events.Unsubscribe<PlayerDeathEvent>(this);
        Events.Unsubscribe<PlayerRespawnEvent>(this);
    }

    void Update()
    {
        // Анімуємо зміни здоров'я
        if (animateChanges && isAnimating)
        {
            AnimateHealthChange();
        }
    }

    /// <summary>
    /// Ініціалізує відображення здоров'я
    /// </summary>
    void InitializeHealthDisplay()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        UpdateHealthDisplay(1f, 100f, 100f);
    }

    /// <summary>
    /// Обробляє подію зміни здоров'я гравця
    /// </summary>
    public void HandleEvent(PlayerHealthChangedEvent eventData)
    {
        float healthPercentage = eventData.CurrentHealth / eventData.MaxHealth;
        
        if (animateChanges)
        {
            targetHealthPercentage = healthPercentage;
            isAnimating = true;
        }
        else
        {
            UpdateHealthDisplay(healthPercentage, eventData.CurrentHealth, eventData.MaxHealth);
        }

        // Логуємо зміну для налагодження
        Debug.Log($"UIHealthDisplay: Health changed to {eventData.CurrentHealth:F1}/{eventData.MaxHealth:F1} " +
                  $"({healthPercentage:P1}) - {(eventData.IsDamage ? "DAMAGE" : "HEAL")}");
    }

    /// <summary>
    /// Обробляє подію смерті гравця
    /// </summary>
    public void HandleEvent(PlayerDeathEvent eventData)
    {
        Debug.Log("UIHealthDisplay: Player died - updating UI");
        
        // Миттєво встановлюємо здоров'я на 0
        targetHealthPercentage = 0f;
        if (!animateChanges)
        {
            UpdateHealthDisplay(0f, 0f, 100f);
        }
        else
        {
            isAnimating = true;
        }

        // Можна додати спеціальні ефекти смерті
        if (healthFillImage != null)
        {
            healthFillImage.color = lowHealthColor;
        }
    }

    /// <summary>
    /// Обробляє подію відродження гравця
    /// </summary>
    public void HandleEvent(PlayerRespawnEvent eventData)
    {
        Debug.Log($"UIHealthDisplay: Player respawned with {eventData.RespawnHealth} health");
        
        float healthPercentage = eventData.RespawnHealth / 100f; // Припускаємо max health = 100
        
        if (animateChanges)
        {
            targetHealthPercentage = healthPercentage;
            isAnimating = true;
        }
        else
        {
            UpdateHealthDisplay(healthPercentage, eventData.RespawnHealth, 100f);
        }
    }

    /// <summary>
    /// Анімує зміну здоров'я
    /// </summary>
    void AnimateHealthChange()
    {
        currentDisplayedPercentage = Mathf.MoveTowards(
            currentDisplayedPercentage, 
            targetHealthPercentage, 
            animationSpeed * Time.deltaTime
        );

        UpdateHealthDisplay(currentDisplayedPercentage, 
            currentDisplayedPercentage * 100f, 100f);

        // Зупиняємо анімацію, коли досягли цілі
        if (Mathf.Approximately(currentDisplayedPercentage, targetHealthPercentage))
        {
            isAnimating = false;
        }
    }

    /// <summary>
    /// Оновлює відображення здоров'я на UI
    /// </summary>
    void UpdateHealthDisplay(float healthPercentage, float currentHealth, float maxHealth)
    {
        // Оновлюємо slider
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
        }

        // Оновлюємо текст
        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }

        // Оновлюємо колір
        if (healthFillImage != null)
        {
            healthFillImage.color = GetHealthColor(healthPercentage);
        }

        // Зберігаємо поточне відображене значення
        currentDisplayedPercentage = healthPercentage;
    }

    /// <summary>
    /// Визначає колір здоров'я залежно від відсотка
    /// </summary>
    Color GetHealthColor(float healthPercentage)
    {
        if (healthPercentage <= lowHealthThreshold)
        {
            return lowHealthColor;
        }
        else if (healthPercentage <= mediumHealthThreshold)
        {
            // Інтерполяція між низьким та середнім кольором
            float t = (healthPercentage - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            return Color.Lerp(lowHealthColor, mediumHealthColor, t);
        }
        else
        {
            // Інтерполяція між середнім та повним кольором
            float t = (healthPercentage - mediumHealthThreshold) / (1f - mediumHealthThreshold);
            return Color.Lerp(mediumHealthColor, fullHealthColor, t);
        }
    }

    /// <summary>
    /// Публічний метод для ручного оновлення здоров'я (для тестування)
    /// </summary>
    [ContextMenu("Test Health Change")]
    public void TestHealthChange()
    {
        // Симулюємо подію зміни здоров'я для тестування
        var testEvent = new PlayerHealthChangedEvent(50f, 100f, 75f);
        HandleEvent(testEvent);
    }

    /// <summary>
    /// Валідація компонентів в Editor
    /// </summary>
    void OnValidate()
    {
        // Переконуємося, що пороги в правильному порядку
        if (lowHealthThreshold > mediumHealthThreshold)
        {
            mediumHealthThreshold = lowHealthThreshold;
        }

        animationSpeed = Mathf.Max(0.1f, animationSpeed);
    }
}