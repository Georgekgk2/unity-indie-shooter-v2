using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Покращені UI компоненти з анімаціями, accessibility та сучасними UX patterns
/// ФАЗА 3.4: Покращення UI - створення професійних компонентів
/// </summary>

// ================================
// ПОКРАЩЕНА КНОПКА
// ================================

public class UIButton : UIComponent, IUIAnimatable, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Button Components")]
    public Button button;
    public TextMeshProUGUI buttonText;
    public Image buttonImage;
    public Image iconImage;
    public ParticleSystem clickEffect;

    [Header("Animation Settings")]
    public bool enableHoverAnimation = true;
    public bool enableClickAnimation = true;
    public bool enableRippleEffect = true;
    public float animationDuration = 0.2f;

    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // Приватні змінні
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovered = false;
    private bool isPressed = false;
    private Coroutine currentAnimation;

    // Accessibility
    private string accessibilityLabel;
    private string accessibilityHint;

    public bool IsAnimating { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        if (button == null)
            button = GetComponent<Button>();
        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        originalScale = transform.localScale;
        if (buttonImage != null)
            originalColor = buttonImage.color;
    }

    protected override void Start()
    {
        base.Start();
        
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// Ініціалізує кнопку з текстом та callback
    /// </summary>
    public void Initialize(string text, ButtonStyle style, System.Action onClick = null)
    {
        if (buttonText != null)
            buttonText.text = text;

        ApplyButtonStyle(style);

        if (onClick != null && button != null)
        {
            button.onClick.AddListener(() => onClick());
        }

        // Налаштовуємо accessibility
        SetAccessibilityLabel(text);
    }

    /// <summary>
    /// Застосовує стиль кнопки
    /// </summary>
    void ApplyButtonStyle(ButtonStyle style)
    {
        if (buttonImage != null)
        {
            buttonImage.color = style.normalColor;
            originalColor = style.normalColor;
        }

        if (buttonText != null)
        {
            buttonText.fontSize = style.fontSize;
            buttonText.fontStyle = style.fontStyle;
        }

        // Налаштовуємо ColorBlock для кнопки
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = style.normalColor;
            colors.highlightedColor = style.hoverColor;
            colors.pressedColor = style.pressedColor;
            colors.disabledColor = style.disabledColor;
            button.colors = colors;
        }
    }

    protected override void OnThemeApplied()
    {
        if (currentTheme?.buttonStyle != null)
        {
            ApplyButtonStyle(currentTheme.buttonStyle);
        }
    }

    protected override void OnAccessibilityApplied()
    {
        if (accessibilitySettings != null && buttonText != null)
        {
            // Застосовуємо масштабування шрифту
            float originalSize = buttonText.fontSize;
            buttonText.fontSize = originalSize * accessibilitySettings.fontSizeMultiplier;

            // Застосовуємо high contrast
            if (accessibilitySettings.enableHighContrast)
            {
                buttonText.color = Color.white;
                if (buttonImage != null)
                    buttonImage.color = Color.black;
            }
        }
    }

    // Event Handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isHovered = true;
        
        if (enableHoverAnimation)
        {
            PlayAnimation(UIAnimationType.ScaleIn, animationDuration);
        }

        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isHovered = false;
        
        if (enableHoverAnimation && !isPressed)
        {
            PlayAnimation(UIAnimationType.ScaleOut, animationDuration);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = true;
        
        if (enableClickAnimation)
        {
            PlayAnimation(UIAnimationType.Bounce, animationDuration * 0.5f);
        }

        if (enableRippleEffect)
        {
            CreateRippleEffect(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = false;
        
        if (isHovered && enableHoverAnimation)
        {
            PlayAnimation(UIAnimationType.ScaleIn, animationDuration);
        }
    }

    void OnButtonClicked()
    {
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }

        if (clickEffect != null)
        {
            clickEffect.Play();
        }
    }

    /// <summary>
    /// Створює ripple ефект
    /// </summary>
    void CreateRippleEffect(Vector2 position)
    {
        // Створюємо тимчасовий GameObject для ripple ефекту
        GameObject ripple = new GameObject("Ripple");
        ripple.transform.SetParent(transform, false);
        
        Image rippleImage = ripple.AddComponent<Image>();
        rippleImage.color = new Color(1, 1, 1, 0.3f);
        rippleImage.raycastTarget = false;

        RectTransform rippleRect = ripple.GetComponent<RectTransform>();
        rippleRect.anchoredPosition = Vector2.zero;
        rippleRect.sizeDelta = Vector2.zero;

        // Анімуємо ripple
        StartCoroutine(AnimateRipple(rippleRect, rippleImage));
    }

    IEnumerator AnimateRipple(RectTransform rippleRect, Image rippleImage)
    {
        float duration = 0.6f;
        float maxSize = Mathf.Max(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height) * 2f;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Масштабуємо ripple
            float currentSize = Mathf.Lerp(0f, maxSize, progress);
            rippleRect.sizeDelta = Vector2.one * currentSize;

            // Зменшуємо прозорість
            Color color = rippleImage.color;
            color.a = Mathf.Lerp(0.3f, 0f, progress);
            rippleImage.color = color;

            yield return null;
        }

        // Видаляємо ripple
        if (rippleRect != null)
            Destroy(rippleRect.gameObject);
    }

    // IUIAnimatable Implementation
    public void PlayAnimation(UIAnimationType animationType, float duration = 0.3f)
    {
        if (!enableAnimations) return;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(PlayAnimationCoroutine(animationType, duration));
    }

    public void StopAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        IsAnimating = false;
    }

    IEnumerator PlayAnimationCoroutine(UIAnimationType animationType, float duration)
    {
        IsAnimating = true;

        switch (animationType)
        {
            case UIAnimationType.ScaleIn:
                yield return ScaleAnimation(originalScale * 1.1f, duration);
                break;
            case UIAnimationType.ScaleOut:
                yield return ScaleAnimation(originalScale, duration);
                break;
            case UIAnimationType.Bounce:
                yield return BounceAnimation(duration);
                break;
            case UIAnimationType.Pulse:
                yield return PulseAnimation(duration);
                break;
        }

        IsAnimating = false;
        currentAnimation = null;
    }

    IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    IEnumerator BounceAnimation(float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 bounceScale = startScale * 0.9f;

        // Стискаємо
        yield return ScaleAnimation(bounceScale, duration * 0.3f);
        // Повертаємо
        yield return ScaleAnimation(startScale, duration * 0.7f);
    }

    IEnumerator PulseAnimation(float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 pulseScale = startScale * 1.2f;

        // Збільшуємо
        yield return ScaleAnimation(pulseScale, duration * 0.5f);
        // Зменшуємо
        yield return ScaleAnimation(startScale, duration * 0.5f);
    }

    // Accessibility Methods
    public void SetAccessibilityLabel(string label)
    {
        accessibilityLabel = label;
        // Тут можна додати інтеграцію з screen reader
    }

    public void SetAccessibilityHint(string hint)
    {
        accessibilityHint = hint;
    }

    public string GetAccessibilityDescription()
    {
        string description = accessibilityLabel ?? (buttonText?.text ?? "Button");
        if (!string.IsNullOrEmpty(accessibilityHint))
        {
            description += ". " + accessibilityHint;
        }
        return description;
    }
}

// ================================
// ПОКРАЩЕНА ПАНЕЛЬ
// ================================

public class UIPanel : UIComponent, IUIAnimatable
{
    [Header("Panel Components")]
    public Image backgroundImage;
    public Image borderImage;
    public Shadow shadowComponent;

    [Header("Animation Settings")]
    public bool enableShowHideAnimation = true;
    public float showHideDuration = 0.3f;
    public AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isVisible = true;

    public bool IsAnimating { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

    /// <summary>
    /// Ініціалізує панель зі стилем
    /// </summary>
    public void Initialize(PanelStyle style)
    {
        ApplyPanelStyle(style);
    }

    /// <summary>
    /// Застосовує стиль панелі
    /// </summary>
    void ApplyPanelStyle(PanelStyle style)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = style.backgroundColor;
        }

        if (borderImage != null)
        {
            borderImage.color = style.borderColor;
        }

        // Налаштовуємо тінь
        if (style.enableShadow && shadowComponent == null)
        {
            shadowComponent = gameObject.AddComponent<Shadow>();
        }

        if (shadowComponent != null)
        {
            shadowComponent.effectColor = style.shadowColor;
            shadowComponent.enabled = style.enableShadow;
        }
    }

    protected override void OnThemeApplied()
    {
        if (currentTheme?.panelStyle != null)
        {
            ApplyPanelStyle(currentTheme.panelStyle);
        }
    }

    /// <summary>
    /// Показує панель з анімацією
    /// </summary>
    public void Show(bool animated = true)
    {
        if (isVisible) return;

        gameObject.SetActive(true);
        isVisible = true;

        if (animated && enableShowHideAnimation)
        {
            PlayAnimation(UIAnimationType.FadeIn, showHideDuration);
        }
        else
        {
            canvasGroup.alpha = 1f;
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Ховає панель з анімацією
    /// </summary>
    public void Hide(bool animated = true)
    {
        if (!isVisible) return;

        isVisible = false;

        if (animated && enableShowHideAnimation)
        {
            StartCoroutine(HideWithAnimation());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator HideWithAnimation()
    {
        yield return StartCoroutine(PlayAnimationCoroutine(UIAnimationType.FadeOut, showHideDuration));
        gameObject.SetActive(false);
    }

    // IUIAnimatable Implementation
    public void PlayAnimation(UIAnimationType animationType, float duration = 0.3f)
    {
        if (!enableAnimations) return;
        StartCoroutine(PlayAnimationCoroutine(animationType, duration));
    }

    public void StopAnimation()
    {
        StopAllCoroutines();
        IsAnimating = false;
    }

    IEnumerator PlayAnimationCoroutine(UIAnimationType animationType, float duration)
    {
        IsAnimating = true;

        switch (animationType)
        {
            case UIAnimationType.FadeIn:
                yield return FadeAnimation(0f, 1f, duration);
                break;
            case UIAnimationType.FadeOut:
                yield return FadeAnimation(1f, 0f, duration);
                break;
            case UIAnimationType.SlideIn:
                yield return SlideAnimation(Vector3.zero, duration);
                break;
            case UIAnimationType.SlideOut:
                yield return SlideAnimation(new Vector3(rectTransform.rect.width, 0, 0), duration);
                break;
        }

        IsAnimating = false;
    }

    IEnumerator FadeAnimation(float fromAlpha, float toAlpha, float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = showCurve.Evaluate(elapsedTime / duration);
            
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    IEnumerator SlideAnimation(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = showCurve.Evaluate(elapsedTime / duration);
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}

// ================================
// ПОКРАЩЕНЕ INPUT FIELD
// ================================

public class UIInputField : UIComponent, IUIAnimatable
{
    [Header("Input Components")]
    public TMP_InputField inputField;
    public TextMeshProUGUI placeholderText;
    public Image backgroundImage;
    public Image borderImage;

    [Header("Validation")]
    public bool enableValidation = false;
    public InputValidationType validationType = InputValidationType.None;
    public string customValidationPattern = "";

    [Header("Animation Settings")]
    public bool enableFocusAnimation = true;
    public float focusAnimationDuration = 0.2f;

    private Color originalBorderColor;
    private Color focusedBorderColor;
    private bool isFocused = false;

    public bool IsAnimating { get; private set; }

    // Validation
    public bool IsValid { get; private set; } = true;

    protected override void Awake()
    {
        base.Awake();
        
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

    protected override void Start()
    {
        base.Start();
        
        if (inputField != null)
        {
            inputField.onSelect.AddListener(OnInputSelected);
            inputField.onDeselect.AddListener(OnInputDeselected);
            inputField.onValueChanged.AddListener(OnInputValueChanged);
        }
    }

    /// <summary>
    /// Ініціалізує input field
    /// </summary>
    public void Initialize(string placeholder, InputStyle style)
    {
        if (placeholderText != null)
            placeholderText.text = placeholder;

        ApplyInputStyle(style);
    }

    /// <summary>
    /// Застосовує стиль input field
    /// </summary>
    void ApplyInputStyle(InputStyle style)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = style.backgroundColor;
        }

        if (borderImage != null)
        {
            originalBorderColor = style.borderColor;
            focusedBorderColor = style.borderColor * 1.5f;
            borderImage.color = originalBorderColor;
        }

        if (inputField?.textComponent != null)
        {
            inputField.textComponent.color = style.textColor;
            inputField.textComponent.fontSize = style.fontSize;
        }

        if (placeholderText != null)
        {
            placeholderText.color = style.placeholderColor;
        }
    }

    protected override void OnThemeApplied()
    {
        if (currentTheme?.inputStyle != null)
        {
            ApplyInputStyle(currentTheme.inputStyle);
        }
    }

    void OnInputSelected(string value)
    {
        isFocused = true;
        
        if (enableFocusAnimation)
        {
            PlayAnimation(UIAnimationType.ScaleIn, focusAnimationDuration);
        }

        if (borderImage != null)
        {
            borderImage.color = focusedBorderColor;
        }
    }

    void OnInputDeselected(string value)
    {
        isFocused = false;
        
        if (enableFocusAnimation)
        {
            PlayAnimation(UIAnimationType.ScaleOut, focusAnimationDuration);
        }

        if (borderImage != null)
        {
            borderImage.color = originalBorderColor;
        }

        ValidateInput();
    }

    void OnInputValueChanged(string value)
    {
        if (enableValidation)
        {
            ValidateInput();
        }
    }

    /// <summary>
    /// Валідує введення
    /// </summary>
    void ValidateInput()
    {
        if (!enableValidation || inputField == null)
        {
            IsValid = true;
            return;
        }

        string value = inputField.text;
        IsValid = ValidateValue(value);

        // Змінюємо колір рамки залежно від валідності
        if (borderImage != null)
        {
            borderImage.color = IsValid ? originalBorderColor : Color.red;
        }
    }

    bool ValidateValue(string value)
    {
        switch (validationType)
        {
            case InputValidationType.Email:
                return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            case InputValidationType.Number:
                return float.TryParse(value, out _);
            case InputValidationType.Custom:
                return System.Text.RegularExpressions.Regex.IsMatch(value, customValidationPattern);
            default:
                return true;
        }
    }

    // IUIAnimatable Implementation
    public void PlayAnimation(UIAnimationType animationType, float duration = 0.3f)
    {
        if (!enableAnimations) return;
        StartCoroutine(PlayAnimationCoroutine(animationType, duration));
    }

    public void StopAnimation()
    {
        StopAllCoroutines();
        IsAnimating = false;
    }

    IEnumerator PlayAnimationCoroutine(UIAnimationType animationType, float duration)
    {
        IsAnimating = true;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale;

        switch (animationType)
        {
            case UIAnimationType.ScaleIn:
                targetScale = originalScale * 1.05f;
                break;
            case UIAnimationType.ScaleOut:
                targetScale = originalScale;
                break;
        }

        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
        IsAnimating = false;
    }
}

// ================================
// ЕНУМИ ТА ДОПОМІЖНІ КЛАСИ
// ================================

public enum InputValidationType
{
    None,
    Email,
    Number,
    Custom
}