using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система тем UI з підтримкою динамічної зміни стилів
/// ФАЗА 3.4: Покращення UI - система тем та стилізації
/// </summary>

// ================================
// SCRIPTABLE OBJECT ДЛЯ ТЕМ
// ================================

[CreateAssetMenu(fileName = "New UI Theme", menuName = "UI Framework/UI Theme")]
public class UITheme : ScriptableObject
{
    [Header("Theme Info")]
    public string themeName = "Default Theme";
    public string description = "Default UI theme";
    public Sprite themePreview;

    [Header("Color Palette")]
    public Color primaryColor = new Color(0.2f, 0.6f, 1f);
    public Color secondaryColor = new Color(0.8f, 0.8f, 0.8f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public Color surfaceColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    public Color textColor = Color.white;
    public Color textSecondaryColor = new Color(0.8f, 0.8f, 0.8f);
    public Color accentColor = new Color(1f, 0.6f, 0.2f);
    public Color errorColor = new Color(1f, 0.3f, 0.3f);
    public Color warningColor = new Color(1f, 0.8f, 0.2f);
    public Color successColor = new Color(0.3f, 1f, 0.3f);

    [Header("Component Styles")]
    public ButtonStyle buttonStyle;
    public PanelStyle panelStyle;
    public InputStyle inputStyle;
    public SliderStyle sliderStyle;
    public ToggleStyle toggleStyle;
    public DropdownStyle dropdownStyle;

    [Header("Typography")]
    public TMP_FontAsset primaryFont;
    public TMP_FontAsset secondaryFont;
    public FontSizes fontSizes;

    [Header("Spacing & Layout")]
    public LayoutSpacing spacing;

    [Header("Effects")]
    public ShadowSettings shadowSettings;
    public GlowSettings glowSettings;

    /// <summary>
    /// Створює копію теми
    /// </summary>
    public UITheme CreateCopy()
    {
        UITheme copy = CreateInstance<UITheme>();
        copy.themeName = themeName + " (Copy)";
        copy.description = description;
        
        // Копіюємо всі кольори
        copy.primaryColor = primaryColor;
        copy.secondaryColor = secondaryColor;
        copy.backgroundColor = backgroundColor;
        copy.surfaceColor = surfaceColor;
        copy.textColor = textColor;
        copy.textSecondaryColor = textSecondaryColor;
        copy.accentColor = accentColor;
        copy.errorColor = errorColor;
        copy.warningColor = warningColor;
        copy.successColor = successColor;

        // Копіюємо стилі
        copy.buttonStyle = buttonStyle;
        copy.panelStyle = panelStyle;
        copy.inputStyle = inputStyle;
        copy.sliderStyle = sliderStyle;
        copy.toggleStyle = toggleStyle;
        copy.dropdownStyle = dropdownStyle;

        return copy;
    }

    /// <summary>
    /// Застосовує high contrast модифікації
    /// </summary>
    public void ApplyHighContrast()
    {
        // Збільшуємо контраст кольорів
        textColor = Color.white;
        backgroundColor = Color.black;
        primaryColor = Color.cyan;
        secondaryColor = Color.yellow;
        accentColor = Color.magenta;
    }

    /// <summary>
    /// Застосовує модифікації для дальтоніків
    /// </summary>
    public void ApplyColorBlindFriendly()
    {
        // Використовуємо кольори, які добре розрізняють дальтоніки
        primaryColor = new Color(0f, 0.45f, 0.7f);      // Синій
        secondaryColor = new Color(0.8f, 0.4f, 0f);     // Помаранчевий
        successColor = new Color(0f, 0.6f, 0.5f);       // Бірюзовий
        errorColor = new Color(0.8f, 0.4f, 0.4f);       // Червоно-коричневий
        warningColor = new Color(0.9f, 0.6f, 0f);       // Жовто-помаранчевий
    }
}

// ================================
// СТИЛІ КОМПОНЕНТІВ
// ================================

[System.Serializable]
public class SliderStyle
{
    [Header("Colors")]
    public Color fillColor = Color.blue;
    public Color backgroundColor = Color.gray;
    public Color handleColor = Color.white;
    public Color borderColor = Color.black;

    [Header("Dimensions")]
    public float height = 20f;
    public float handleSize = 24f;
    public float borderWidth = 1f;
}

[System.Serializable]
public class ToggleStyle
{
    [Header("Colors")]
    public Color checkmarkColor = Color.white;
    public Color backgroundColorOff = Color.gray;
    public Color backgroundColorOn = Color.blue;
    public Color borderColor = Color.black;

    [Header("Dimensions")]
    public float size = 20f;
    public float borderWidth = 1f;
    public float checkmarkSize = 16f;
}

[System.Serializable]
public class DropdownStyle
{
    [Header("Colors")]
    public Color backgroundColor = Color.white;
    public Color textColor = Color.black;
    public Color arrowColor = Color.gray;
    public Color highlightColor = Color.blue;
    public Color borderColor = Color.gray;

    [Header("Dimensions")]
    public float height = 30f;
    public float borderWidth = 1f;
    public float itemHeight = 25f;
}

[System.Serializable]
public class FontSizes
{
    [Header("Text Sizes")]
    public int h1 = 32;
    public int h2 = 28;
    public int h3 = 24;
    public int h4 = 20;
    public int body = 16;
    public int caption = 14;
    public int small = 12;
}

[System.Serializable]
public class LayoutSpacing
{
    [Header("Spacing Values")]
    public float tiny = 4f;
    public float small = 8f;
    public float medium = 16f;
    public float large = 24f;
    public float huge = 32f;

    [Header("Padding")]
    public float paddingSmall = 8f;
    public float paddingMedium = 16f;
    public float paddingLarge = 24f;

    [Header("Margins")]
    public float marginSmall = 4f;
    public float marginMedium = 8f;
    public float marginLarge = 16f;
}

[System.Serializable]
public class ShadowSettings
{
    public bool enableShadows = true;
    public Color shadowColor = new Color(0, 0, 0, 0.3f);
    public Vector2 shadowOffset = new Vector2(2, -2);
    public float shadowBlur = 4f;
}

[System.Serializable]
public class GlowSettings
{
    public bool enableGlow = false;
    public Color glowColor = Color.cyan;
    public float glowIntensity = 1f;
    public float glowSize = 4f;
}

// ================================
// МЕНЕДЖЕР ТЕМ
// ================================

public class UIThemeManager : MonoBehaviour
{
    [Header("Available Themes")]
    [Tooltip("Список доступних тем")]
    public UITheme[] availableThemes;
    [Tooltip("Тема за замовчуванням")]
    public UITheme defaultTheme;

    [Header("Theme Switching")]
    [Tooltip("Тривалість анімації зміни теми")]
    public float themeTransitionDuration = 0.5f;
    [Tooltip("Анімаційна крива для переходу")]
    public AnimationCurve themeTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Поточна тема
    private UITheme currentTheme;
    private bool isTransitioning = false;

    // Події
    public static event System.Action<UITheme> OnThemeChanged;
    public static event System.Action<UITheme, UITheme> OnThemeTransitionStarted;
    public static event System.Action<UITheme> OnThemeTransitionCompleted;

    // Singleton
    public static UIThemeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeThemeManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeThemeManager()
    {
        // Встановлюємо тему за замовчуванням
        if (defaultTheme != null)
        {
            SetTheme(defaultTheme, false);
        }
        else if (availableThemes.Length > 0)
        {
            SetTheme(availableThemes[0], false);
        }

        // Завантажуємо збережену тему
        LoadSavedTheme();

        Debug.Log($"UIThemeManager: Ініціалізовано з темою '{currentTheme?.themeName}'");
    }

    /// <summary>
    /// Встановлює нову тему
    /// </summary>
    public void SetTheme(UITheme newTheme, bool animated = true)
    {
        if (newTheme == null || newTheme == currentTheme || isTransitioning)
            return;

        UITheme previousTheme = currentTheme;
        currentTheme = newTheme;

        if (animated && Application.isPlaying)
        {
            StartCoroutine(AnimateThemeTransition(previousTheme, newTheme));
        }
        else
        {
            ApplyThemeImmediately(newTheme);
        }
    }

    /// <summary>
    /// Встановлює тему за назвою
    /// </summary>
    public void SetThemeByName(string themeName, bool animated = true)
    {
        UITheme theme = GetThemeByName(themeName);
        if (theme != null)
        {
            SetTheme(theme, animated);
        }
        else
        {
            Debug.LogWarning($"UIThemeManager: Тему '{themeName}' не знайдено");
        }
    }

    /// <summary>
    /// Отримує тему за назвою
    /// </summary>
    public UITheme GetThemeByName(string themeName)
    {
        foreach (var theme in availableThemes)
        {
            if (theme != null && theme.themeName == themeName)
                return theme;
        }
        return null;
    }

    /// <summary>
    /// Анімований перехід між темами
    /// </summary>
    System.Collections.IEnumerator AnimateThemeTransition(UITheme fromTheme, UITheme toTheme)
    {
        isTransitioning = true;
        OnThemeTransitionStarted?.Invoke(fromTheme, toTheme);

        float elapsedTime = 0f;
        
        while (elapsedTime < themeTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / themeTransitionDuration;
            float easedProgress = themeTransitionCurve.Evaluate(progress);

            // Інтерполюємо між темами
            ApplyInterpolatedTheme(fromTheme, toTheme, easedProgress);

            yield return null;
        }

        // Застосовуємо фінальну тему
        ApplyThemeImmediately(toTheme);
        
        isTransitioning = false;
        OnThemeTransitionCompleted?.Invoke(toTheme);
    }

    /// <summary>
    /// Застосовує інтерпольовану тему
    /// </summary>
    void ApplyInterpolatedTheme(UITheme fromTheme, UITheme toTheme, float progress)
    {
        // Створюємо тимчасову тему з інтерпольованими значеннями
        UITheme interpolatedTheme = CreateInterpolatedTheme(fromTheme, toTheme, progress);
        
        // Застосовуємо до UI Framework
        if (UnifiedUIFramework.Instance != null)
        {
            UnifiedUIFramework.Instance.ChangeTheme(interpolatedTheme);
        }
    }

    /// <summary>
    /// Створює інтерпольовану тему
    /// </summary>
    UITheme CreateInterpolatedTheme(UITheme fromTheme, UITheme toTheme, float progress)
    {
        UITheme interpolated = ScriptableObject.CreateInstance<UITheme>();
        
        // Інтерполюємо кольори
        interpolated.primaryColor = Color.Lerp(fromTheme.primaryColor, toTheme.primaryColor, progress);
        interpolated.secondaryColor = Color.Lerp(fromTheme.secondaryColor, toTheme.secondaryColor, progress);
        interpolated.backgroundColor = Color.Lerp(fromTheme.backgroundColor, toTheme.backgroundColor, progress);
        interpolated.textColor = Color.Lerp(fromTheme.textColor, toTheme.textColor, progress);
        interpolated.accentColor = Color.Lerp(fromTheme.accentColor, toTheme.accentColor, progress);

        // Копіюємо стилі з цільової теми
        interpolated.buttonStyle = toTheme.buttonStyle;
        interpolated.panelStyle = toTheme.panelStyle;
        interpolated.inputStyle = toTheme.inputStyle;

        return interpolated;
    }

    /// <summary>
    /// Застосовує тему негайно
    /// </summary>
    void ApplyThemeImmediately(UITheme theme)
    {
        if (UnifiedUIFramework.Instance != null)
        {
            UnifiedUIFramework.Instance.ChangeTheme(theme);
        }

        OnThemeChanged?.Invoke(theme);
        SaveCurrentTheme();
    }

    /// <summary>
    /// Перемикає на наступну доступну тему
    /// </summary>
    public void CycleToNextTheme()
    {
        if (availableThemes.Length <= 1) return;

        int currentIndex = System.Array.IndexOf(availableThemes, currentTheme);
        int nextIndex = (currentIndex + 1) % availableThemes.Length;
        
        SetTheme(availableThemes[nextIndex]);
    }

    /// <summary>
    /// Створює custom тему на основі поточної
    /// </summary>
    public UITheme CreateCustomTheme(string name, Color primaryColor, Color backgroundColor)
    {
        UITheme customTheme = currentTheme.CreateCopy();
        customTheme.themeName = name;
        customTheme.primaryColor = primaryColor;
        customTheme.backgroundColor = backgroundColor;
        
        // Автоматично генеруємо похідні кольори
        customTheme.secondaryColor = primaryColor * 0.8f;
        customTheme.accentColor = new Color(1f - primaryColor.r, 1f - primaryColor.g, primaryColor.b);

        return customTheme;
    }

    /// <summary>
    /// Застосовує модифікації доступності до поточної теми
    /// </summary>
    public void ApplyAccessibilityModifications(bool highContrast, bool colorBlindFriendly)
    {
        if (currentTheme == null) return;

        UITheme modifiedTheme = currentTheme.CreateCopy();
        modifiedTheme.themeName = currentTheme.themeName + " (Modified)";

        if (highContrast)
        {
            modifiedTheme.ApplyHighContrast();
        }

        if (colorBlindFriendly)
        {
            modifiedTheme.ApplyColorBlindFriendly();
        }

        SetTheme(modifiedTheme, false);
    }

    /// <summary>
    /// Зберігає поточну тему
    /// </summary>
    void SaveCurrentTheme()
    {
        try
        {
            if (currentTheme != null)
            {
                PlayerPrefs.SetString("CurrentTheme", currentTheme.themeName);
                PlayerPrefs.Save();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIThemeManager: Failed to save current theme: {e.Message}");
        }
    }

    /// <summary>
    /// Завантажує збережену тему
    /// </summary>
    void LoadSavedTheme()
    {
        try
        {
            string savedThemeName = PlayerPrefs.GetString("CurrentTheme", "");
            if (!string.IsNullOrEmpty(savedThemeName))
            {
                SetThemeByName(savedThemeName, false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIThemeManager: Failed to load saved theme: {e.Message}");
            // Fallback to default theme
            if (defaultTheme != null)
            {
                SetTheme(defaultTheme, false);
            }
        }
    }

    /// <summary>
    /// Отримує поточну тему
    /// </summary>
    public UITheme GetCurrentTheme()
    {
        return currentTheme;
    }

    /// <summary>
    /// Отримує список доступних тем
    /// </summary>
    public UITheme[] GetAvailableThemes()
    {
        return availableThemes;
    }

    /// <summary>
    /// Перевіряє, чи відбувається перехід між темами
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }

    // Debug методи
    [ContextMenu("Cycle Theme")]
    void DebugCycleTheme()
    {
        CycleToNextTheme();
    }

    [ContextMenu("Apply High Contrast")]
    void DebugApplyHighContrast()
    {
        ApplyAccessibilityModifications(true, false);
    }

    [ContextMenu("Apply Color Blind Friendly")]
    void DebugApplyColorBlindFriendly()
    {
        ApplyAccessibilityModifications(false, true);
    }
}