using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Уніфікований UI Framework для стандартизації всіх UI систем
/// ФАЗА 3.4: Покращення UI - створення єдиної системи стилів та компонентів
/// </summary>

// ================================
// ГОЛОВНИЙ UI FRAMEWORK МЕНЕДЖЕР
// ================================

public class UnifiedUIFramework : MonoBehaviour
{
    [Header("Framework Settings")]
    [Tooltip("Поточна тема UI")]
    public UITheme currentTheme;
    [Tooltip("Налаштування доступності")]
    public AccessibilitySettings accessibility;
    [Tooltip("Налаштування анімацій")]
    public AnimationSettings animations;

    [Header("Component Prefabs")]
    [Tooltip("Префаб стандартної кнопки")]
    public GameObject standardButtonPrefab;
    [Tooltip("Префаб панелі")]
    public GameObject standardPanelPrefab;
    [Tooltip("Префаб input field")]
    public GameObject standardInputPrefab;
    [Tooltip("Префаб slider")]
    public GameObject standardSliderPrefab;

    // Singleton
    public static UnifiedUIFramework Instance { get; private set; }

    // Кеш компонентів
    private Dictionary<string, UIComponent> componentCache = new Dictionary<string, UIComponent>();
    private List<IUIAnimatable> animatableComponents = new List<IUIAnimatable>();

    // Події
    public static event System.Action<UITheme> OnThemeChanged;
    public static event System.Action<AccessibilitySettings> OnAccessibilityChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFramework();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFramework()
    {
        // Ініціалізуємо тему за замовчуванням
        if (currentTheme == null)
        {
            currentTheme = CreateDefaultTheme();
        }

        // Ініціалізуємо налаштування доступності
        if (accessibility == null)
        {
            accessibility = CreateDefaultAccessibilitySettings();
        }

        // Ініціалізуємо налаштування анімацій
        if (animations == null)
        {
            animations = CreateDefaultAnimationSettings();
        }

        // Застосовуємо тему до всіх існуючих UI елементів
        ApplyThemeToAllComponents();

        Debug.Log("UnifiedUIFramework: Ініціалізовано успішно");
    }

    /// <summary>
    /// Створює стандартну кнопку з уніфікованим стилем
    /// </summary>
    public UIButton CreateStandardButton(string text, Transform parent = null, System.Action onClick = null)
    {
        GameObject buttonObj = Instantiate(standardButtonPrefab, parent);
        UIButton uiButton = buttonObj.GetComponent<UIButton>();
        
        if (uiButton == null)
        {
            uiButton = buttonObj.AddComponent<UIButton>();
        }

        uiButton.Initialize(text, currentTheme.buttonStyle, onClick);
        RegisterComponent(uiButton);

        return uiButton;
    }

    /// <summary>
    /// Створює стандартну панель з уніфікованим стилем
    /// </summary>
    public UIPanel CreateStandardPanel(Transform parent = null)
    {
        GameObject panelObj = Instantiate(standardPanelPrefab, parent);
        UIPanel uiPanel = panelObj.GetComponent<UIPanel>();
        
        if (uiPanel == null)
        {
            uiPanel = panelObj.AddComponent<UIPanel>();
        }

        uiPanel.Initialize(currentTheme.panelStyle);
        RegisterComponent(uiPanel);

        return uiPanel;
    }

    /// <summary>
    /// Створює стандартне input field з уніфікованим стилем
    /// </summary>
    public UIInputField CreateStandardInput(string placeholder, Transform parent = null)
    {
        GameObject inputObj = Instantiate(standardInputPrefab, parent);
        UIInputField uiInput = inputObj.GetComponent<UIInputField>();
        
        if (uiInput == null)
        {
            uiInput = inputObj.AddComponent<UIInputField>();
        }

        uiInput.Initialize(placeholder, currentTheme.inputStyle);
        RegisterComponent(uiInput);

        return uiInput;
    }

    /// <summary>
    /// Реєструє UI компонент в системі
    /// </summary>
    public void RegisterComponent(UIComponent component)
    {
        if (component == null) return;

        string id = component.GetInstanceID().ToString();
        if (!componentCache.ContainsKey(id))
        {
            componentCache.Add(id, component);
        }

        // Додаємо до списку анімованих компонентів
        if (component is IUIAnimatable animatable)
        {
            animatableComponents.Add(animatable);
        }

        // Застосовуємо поточну тему
        ApplyThemeToComponent(component);
    }

    /// <summary>
    /// Видаляє компонент з системи
    /// </summary>
    public void UnregisterComponent(UIComponent component)
    {
        if (component == null) return;

        string id = component.GetInstanceID().ToString();
        componentCache.Remove(id);

        if (component is IUIAnimatable animatable)
        {
            animatableComponents.Remove(animatable);
        }
    }

    /// <summary>
    /// Змінює тему UI
    /// </summary>
    public void ChangeTheme(UITheme newTheme)
    {
        if (newTheme == null) return;

        currentTheme = newTheme;
        ApplyThemeToAllComponents();
        OnThemeChanged?.Invoke(currentTheme);

        Debug.Log($"UnifiedUIFramework: Тему змінено на {newTheme.themeName}");
    }

    /// <summary>
    /// Застосовує тему до всіх компонентів
    /// </summary>
    void ApplyThemeToAllComponents()
    {
        foreach (var component in componentCache.Values)
        {
            ApplyThemeToComponent(component);
        }
    }

    /// <summary>
    /// Застосовує тему до конкретного компонента
    /// </summary>
    void ApplyThemeToComponent(UIComponent component)
    {
        if (component == null || currentTheme == null) return;

        component.ApplyTheme(currentTheme);
    }

    /// <summary>
    /// Оновлює налаштування доступності
    /// </summary>
    public void UpdateAccessibilitySettings(AccessibilitySettings newSettings)
    {
        accessibility = newSettings;
        ApplyAccessibilityToAllComponents();
        OnAccessibilityChanged?.Invoke(accessibility);

        Debug.Log("UnifiedUIFramework: Налаштування доступності оновлено");
    }

    /// <summary>
    /// Застосовує налаштування доступності до всіх компонентів
    /// </summary>
    void ApplyAccessibilityToAllComponents()
    {
        foreach (var component in componentCache.Values)
        {
            component.ApplyAccessibilitySettings(accessibility);
        }
    }

    /// <summary>
    /// Анімує всі компоненти одночасно
    /// </summary>
    public void AnimateAllComponents(UIAnimationType animationType, float duration = 0.3f)
    {
        foreach (var animatable in animatableComponents)
        {
            animatable.PlayAnimation(animationType, duration);
        }
    }

    /// <summary>
    /// Створює тему за замовчуванням
    /// </summary>
    UITheme CreateDefaultTheme()
    {
        UITheme theme = ScriptableObject.CreateInstance<UITheme>();
        theme.themeName = "Default";
        
        // Кольори
        theme.primaryColor = new Color(0.2f, 0.6f, 1f);
        theme.secondaryColor = new Color(0.8f, 0.8f, 0.8f);
        theme.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        theme.textColor = Color.white;
        theme.accentColor = new Color(1f, 0.6f, 0.2f);

        // Стилі кнопок
        theme.buttonStyle = new ButtonStyle
        {
            normalColor = theme.primaryColor,
            hoverColor = theme.primaryColor * 1.2f,
            pressedColor = theme.primaryColor * 0.8f,
            disabledColor = Color.gray,
            fontSize = 16,
            fontStyle = FontStyles.Bold
        };

        // Стилі панелей
        theme.panelStyle = new PanelStyle
        {
            backgroundColor = theme.backgroundColor,
            borderColor = theme.secondaryColor,
            borderWidth = 2f,
            cornerRadius = 8f
        };

        // Стилі input полів
        theme.inputStyle = new InputStyle
        {
            backgroundColor = Color.white,
            textColor = Color.black,
            placeholderColor = Color.gray,
            borderColor = theme.primaryColor,
            fontSize = 14
        };

        return theme;
    }

    /// <summary>
    /// Створює налаштування доступності за замовчуванням
    /// </summary>
    AccessibilitySettings CreateDefaultAccessibilitySettings()
    {
        return new AccessibilitySettings
        {
            enableHighContrast = false,
            fontSizeMultiplier = 1f,
            enableColorBlindSupport = false,
            enableScreenReader = false,
            enableKeyboardNavigation = true,
            animationSpeedMultiplier = 1f
        };
    }

    /// <summary>
    /// Створює налаштування анімацій за замовчуванням
    /// </summary>
    AnimationSettings CreateDefaultAnimationSettings()
    {
        return new AnimationSettings
        {
            enableAnimations = true,
            defaultDuration = 0.3f,
            defaultEasing = AnimationCurve.EaseInOut(0, 0, 1, 1),
            enableParticleEffects = true,
            enableSoundEffects = true
        };
    }

    /// <summary>
    /// Зберігає поточні налаштування
    /// </summary>
    public void SaveSettings()
    {
        // Зберігаємо тему
        PlayerPrefs.SetString("UI_Theme", currentTheme.themeName);
        
        // Зберігаємо доступність
        PlayerPrefs.SetInt("UI_HighContrast", accessibility.enableHighContrast ? 1 : 0);
        PlayerPrefs.SetFloat("UI_FontSize", accessibility.fontSizeMultiplier);
        PlayerPrefs.SetInt("UI_ColorBlind", accessibility.enableColorBlindSupport ? 1 : 0);
        
        // Зберігаємо анімації
        PlayerPrefs.SetInt("UI_Animations", animations.enableAnimations ? 1 : 0);
        PlayerPrefs.SetFloat("UI_AnimSpeed", animations.defaultDuration);
        
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Завантажує збережені налаштування
    /// </summary>
    public void LoadSettings()
    {
        // Завантажуємо доступність
        accessibility.enableHighContrast = PlayerPrefs.GetInt("UI_HighContrast", 0) == 1;
        accessibility.fontSizeMultiplier = PlayerPrefs.GetFloat("UI_FontSize", 1f);
        accessibility.enableColorBlindSupport = PlayerPrefs.GetInt("UI_ColorBlind", 0) == 1;
        
        // Завантажуємо анімації
        animations.enableAnimations = PlayerPrefs.GetInt("UI_Animations", 1) == 1;
        animations.defaultDuration = PlayerPrefs.GetFloat("UI_AnimSpeed", 0.3f);
        
        // Застосовуємо налаштування
        ApplyAccessibilityToAllComponents();
    }

    void OnDestroy()
    {
        SaveSettings();
    }
}

// ================================
// БАЗОВИЙ UI КОМПОНЕНТ
// ================================

public abstract class UIComponent : MonoBehaviour
{
    [Header("Base Component Settings")]
    public string componentId;
    public bool isInteractable = true;
    public bool enableAnimations = true;

    protected UITheme currentTheme;
    protected AccessibilitySettings accessibilitySettings;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(componentId))
        {
            componentId = GetInstanceID().ToString();
        }
    }

    protected virtual void Start()
    {
        // Реєструємо компонент в системі
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

// ================================
// ІНТЕРФЕЙС ДЛЯ АНІМОВАНИХ КОМПОНЕНТІВ
// ================================

public interface IUIAnimatable
{
    void PlayAnimation(UIAnimationType animationType, float duration = 0.3f);
    void StopAnimation();
    bool IsAnimating { get; }
}

public enum UIAnimationType
{
    FadeIn,
    FadeOut,
    SlideIn,
    SlideOut,
    ScaleIn,
    ScaleOut,
    Bounce,
    Shake,
    Pulse,
    Glow
}

// ================================
// СТРУКТУРИ ДАНИХ
// ================================

[System.Serializable]
public class AccessibilitySettings
{
    [Header("Visual Accessibility")]
    public bool enableHighContrast = false;
    [Range(0.5f, 2f)]
    public float fontSizeMultiplier = 1f;
    public bool enableColorBlindSupport = false;

    [Header("Interaction Accessibility")]
    public bool enableScreenReader = false;
    public bool enableKeyboardNavigation = true;
    [Range(0.1f, 3f)]
    public float animationSpeedMultiplier = 1f;
}

[System.Serializable]
public class AnimationSettings
{
    public bool enableAnimations = true;
    [Range(0.1f, 2f)]
    public float defaultDuration = 0.3f;
    public AnimationCurve defaultEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool enableParticleEffects = true;
    public bool enableSoundEffects = true;
}

[System.Serializable]
public class ButtonStyle
{
    public Color normalColor = Color.white;
    public Color hoverColor = Color.gray;
    public Color pressedColor = Color.darkGray;
    public Color disabledColor = Color.gray;
    public int fontSize = 16;
    public FontStyles fontStyle = FontStyles.Normal;
    public float borderRadius = 4f;
}

[System.Serializable]
public class PanelStyle
{
    public Color backgroundColor = Color.white;
    public Color borderColor = Color.black;
    public float borderWidth = 1f;
    public float cornerRadius = 4f;
    public bool enableShadow = true;
    public Color shadowColor = Color.black;
}

[System.Serializable]
public class InputStyle
{
    public Color backgroundColor = Color.white;
    public Color textColor = Color.black;
    public Color placeholderColor = Color.gray;
    public Color borderColor = Color.gray;
    public int fontSize = 14;
    public float borderRadius = 4f;
}