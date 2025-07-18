using UnityEngine;

/// <summary>
/// Базовий клас для всіх UI компонентів
/// ВИПРАВЛЕННЯ КРИТИЧНОЇ ПРОБЛЕМИ #2: Створення базового класу для сумісності
/// </summary>
public abstract class UIComponentBase : MonoBehaviour
{
    [Header("Base Component Settings")]
    public string componentId;
    public bool isInteractable = true;
    public bool enableAnimations = true;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(componentId))
        {
            componentId = GetInstanceID().ToString();
        }
    }

    /// <summary>
    /// Встановлює інтерактивність компонента
    /// </summary>
    public virtual void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        OnInteractabilityChanged();
    }

    /// <summary>
    /// Викликається при зміні інтерактивності
    /// </summary>
    protected virtual void OnInteractabilityChanged() { }
}

// Alias для backward compatibility
public abstract class UIComponent : UIComponentBase
{
    // Додаткова функціональність для нових компонентів
    protected UITheme currentTheme;
    protected AccessibilitySettings accessibilitySettings;

    protected virtual void Start()
    {
        // Реєструємо компонент в системі, якщо вона доступна
        if (UnifiedUIFramework.Instance != null)
        {
            UnifiedUIFramework.Instance.RegisterComponent(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // Видаляємо компонент з системи
        if (UnifiedUIFramework.Instance != null)
        {
            UnifiedUIFramework.Instance.UnregisterComponent(this);
        }
    }

    /// <summary>
    /// Застосовує тему до компонента
    /// </summary>
    public virtual void ApplyTheme(UITheme theme)
    {
        currentTheme = theme;
        OnThemeApplied();
    }

    /// <summary>
    /// Застосовує налаштування доступності
    /// </summary>
    public virtual void ApplyAccessibilitySettings(AccessibilitySettings settings)
    {
        accessibilitySettings = settings;
        OnAccessibilityApplied();
    }

    /// <summary>
    /// Викликається при застосуванні теми
    /// </summary>
    protected virtual void OnThemeApplied() { }

    /// <summary>
    /// Викликається при застосуванні налаштувань доступності
    /// </summary>
    protected virtual void OnAccessibilityApplied() { }
}