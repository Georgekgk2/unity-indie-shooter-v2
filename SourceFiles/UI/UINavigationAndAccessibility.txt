using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Система навігації та accessibility для UI
/// ФАЗА 3.4: Покращення UI - навігація клавіатурою та підтримка accessibility
/// </summary>

// ================================
// МЕНЕДЖЕР НАВІГАЦІЇ
// ================================

public class UINavigationManager : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("Увімкнути навігацію клавіатурою")]
    public bool enableKeyboardNavigation = true;
    [Tooltip("Увімкнути навігацію геймпадом")]
    public bool enableGamepadNavigation = true;
    [Tooltip("Показувати індикатор фокусу")]
    public bool showFocusIndicator = true;

    [Header("Navigation Keys")]
    public KeyCode nextKey = KeyCode.Tab;
    public KeyCode previousKey = KeyCode.Tab; // + Shift
    public KeyCode activateKey = KeyCode.Return;
    public KeyCode cancelKey = KeyCode.Escape;

    [Header("Focus Indicator")]
    [Tooltip("Префаб індикатора фокусу")]
    public GameObject focusIndicatorPrefab;
    [Tooltip("Колір індикатора фокусу")]
    public Color focusIndicatorColor = Color.cyan;

    // Приватні змінні
    private List<Selectable> navigableElements = new List<Selectable>();
    private int currentFocusIndex = -1;
    private GameObject currentFocusIndicator;
    private EventSystem eventSystem;

    // Breadcrumb навігація
    private Stack<IUINavigable> navigationStack = new Stack<IUINavigable>();

    // Singleton
    public static UINavigationManager Instance { get; private set; }

    // Події
    public static event System.Action<Selectable> OnFocusChanged;
    public static event System.Action<IUINavigable> OnNavigationPushed;
    public static event System.Action<IUINavigable> OnNavigationPopped;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNavigation();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeNavigation()
    {
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        // Створюємо індикатор фокусу
        if (showFocusIndicator && focusIndicatorPrefab != null)
        {
            currentFocusIndicator = Instantiate(focusIndicatorPrefab);
            currentFocusIndicator.SetActive(false);
        }

        RefreshNavigableElements();
    }

    void Update()
    {
        if (!enableKeyboardNavigation) return;

        HandleKeyboardInput();
        UpdateFocusIndicator();
    }

    /// <summary>
    /// Обробляє введення з клавіатури
    /// </summary>
    void HandleKeyboardInput()
    {
        // Навігація вперед
        if (Input.GetKeyDown(nextKey) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            NavigateNext();
        }
        // Навігація назад
        else if ((Input.GetKeyDown(previousKey) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) ||
                 Input.GetKeyDown(KeyCode.UpArrow))
        {
            NavigatePrevious();
        }
        // Активація
        else if (Input.GetKeyDown(activateKey))
        {
            ActivateCurrentElement();
        }
        // Скасування
        else if (Input.GetKeyDown(cancelKey))
        {
            HandleCancel();
        }
        // Навігація стрілками
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            NavigateNext();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NavigateLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NavigateRight();
        }
    }

    /// <summary>
    /// Оновлює індикатор фокусу
    /// </summary>
    void UpdateFocusIndicator()
    {
        if (!showFocusIndicator || currentFocusIndicator == null) return;

        Selectable currentSelected = eventSystem?.currentSelectedGameObject?.GetComponent<Selectable>();
        
        if (currentSelected != null && currentSelected.IsInteractable())
        {
            // Позиціонуємо індикатор
            currentFocusIndicator.SetActive(true);
            currentFocusIndicator.transform.position = currentSelected.transform.position;
            currentFocusIndicator.transform.SetParent(currentSelected.transform.parent, true);
            
            // Масштабуємо під розмір елемента
            RectTransform indicatorRect = currentFocusIndicator.GetComponent<RectTransform>();
            RectTransform elementRect = currentSelected.GetComponent<RectTransform>();
            
            if (indicatorRect != null && elementRect != null)
            {
                indicatorRect.sizeDelta = elementRect.sizeDelta + Vector2.one * 10f; // Трохи більше
            }
        }
        else
        {
            currentFocusIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Переходить до наступного елемента
    /// </summary>
    public void NavigateNext()
    {
        if (navigableElements.Count == 0) return;

        currentFocusIndex = (currentFocusIndex + 1) % navigableElements.Count;
        SetFocus(navigableElements[currentFocusIndex]);
    }

    /// <summary>
    /// Переходить до попереднього елемента
    /// </summary>
    public void NavigatePrevious()
    {
        if (navigableElements.Count == 0) return;

        currentFocusIndex = (currentFocusIndex - 1 + navigableElements.Count) % navigableElements.Count;
        SetFocus(navigableElements[currentFocusIndex]);
    }

    /// <summary>
    /// Навігація вліво (для сітки елементів)
    /// </summary>
    public void NavigateLeft()
    {
        Selectable current = eventSystem?.currentSelectedGameObject?.GetComponent<Selectable>();
        if (current?.navigation.selectOnLeft != null)
        {
            SetFocus(current.navigation.selectOnLeft);
        }
    }

    /// <summary>
    /// Навігація вправо (для сітки елементів)
    /// </summary>
    public void NavigateRight()
    {
        Selectable current = eventSystem?.currentSelectedGameObject?.GetComponent<Selectable>();
        if (current?.navigation.selectOnRight != null)
        {
            SetFocus(current.navigation.selectOnRight);
        }
    }

    /// <summary>
    /// Встановлює фокус на елемент
    /// </summary>
    public void SetFocus(Selectable element)
    {
        if (element == null || !element.IsInteractable()) return;

        eventSystem?.SetSelectedGameObject(element.gameObject);
        currentFocusIndex = navigableElements.IndexOf(element);
        
        OnFocusChanged?.Invoke(element);

        // Звуковий ефект навігації
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ui_navigate");
        }
    }

    /// <summary>
    /// Активує поточний елемент
    /// </summary>
    void ActivateCurrentElement()
    {
        GameObject selected = eventSystem?.currentSelectedGameObject;
        if (selected == null) return;

        // Спробуємо активувати кнопку
        Button button = selected.GetComponent<Button>();
        if (button != null && button.IsInteractable())
        {
            button.onClick.Invoke();
            return;
        }

        // Спробуємо активувати toggle
        Toggle toggle = selected.GetComponent<Toggle>();
        if (toggle != null && toggle.IsInteractable())
        {
            toggle.isOn = !toggle.isOn;
            return;
        }

        // Спробуємо активувати input field
        TMP_InputField inputField = selected.GetComponent<TMP_InputField>();
        if (inputField != null && inputField.IsInteractable())
        {
            inputField.ActivateInputField();
            return;
        }
    }

    /// <summary>
    /// Обробляє скасування
    /// </summary>
    void HandleCancel()
    {
        // Спробуємо повернутися назад в навігації
        if (navigationStack.Count > 0)
        {
            PopNavigation();
        }
        else
        {
            // Відкриваємо меню паузи або виходимо
            if (MenuSystems.Instance != null)
            {
                MenuSystems.Instance.TogglePauseMenu();
            }
        }
    }

    /// <summary>
    /// Оновлює список навігаційних елементів
    /// </summary>
    public void RefreshNavigableElements()
    {
        navigableElements.Clear();
        
        // Знаходимо всі Selectable компоненти
        Selectable[] selectables = FindObjectsOfType<Selectable>();
        
        foreach (var selectable in selectables)
        {
            if (selectable.IsInteractable() && selectable.gameObject.activeInHierarchy)
            {
                navigableElements.Add(selectable);
            }
        }

        // Сортуємо за позицією (зверху вниз, зліва направо)
        navigableElements.Sort((a, b) => {
            Vector3 posA = a.transform.position;
            Vector3 posB = b.transform.position;
            
            if (Mathf.Abs(posA.y - posB.y) > 50f) // Різні рядки
            {
                return posB.y.CompareTo(posA.y); // Зверху вниз
            }
            else // Той же рядок
            {
                return posA.x.CompareTo(posB.x); // Зліва направо
            }
        });

        // Встановлюємо фокус на перший елемент, якщо немає поточного
        if (currentFocusIndex == -1 && navigableElements.Count > 0)
        {
            currentFocusIndex = 0;
            SetFocus(navigableElements[0]);
        }
    }

    /// <summary>
    /// Додає сторінку до стеку навігації
    /// </summary>
    public void PushNavigation(IUINavigable navigable)
    {
        navigationStack.Push(navigable);
        OnNavigationPushed?.Invoke(navigable);
    }

    /// <summary>
    /// Повертається до попередньої сторінки
    /// </summary>
    public void PopNavigation()
    {
        if (navigationStack.Count > 0)
        {
            IUINavigable previous = navigationStack.Pop();
            previous.OnNavigatedBack();
            OnNavigationPopped?.Invoke(previous);
        }
    }

    /// <summary>
    /// Очищає стек навігації
    /// </summary>
    public void ClearNavigationStack()
    {
        navigationStack.Clear();
    }
}

// ================================
// ACCESSIBILITY MANAGER
// ================================

public class UIAccessibilityManager : MonoBehaviour
{
    [Header("Accessibility Settings")]
    [Tooltip("Увімкнути підтримку screen reader")]
    public bool enableScreenReader = false;
    [Tooltip("Увімкнути high contrast режим")]
    public bool enableHighContrast = false;
    [Tooltip("Увімкнути підтримку дальтоніків")]
    public bool enableColorBlindSupport = false;

    [Header("Font Scaling")]
    [Range(0.5f, 3f)]
    [Tooltip("Множник розміру шрифту")]
    public float fontSizeMultiplier = 1f;

    [Header("Animation Settings")]
    [Range(0.1f, 3f)]
    [Tooltip("Множник швидкості анімацій")]
    public float animationSpeedMultiplier = 1f;
    [Tooltip("Вимкнути анімації")]
    public bool disableAnimations = false;

    [Header("Audio Cues")]
    [Tooltip("Увімкнути звукові підказки")]
    public bool enableAudioCues = true;
    [Tooltip("Звук фокусу")]
    public AudioClip focusSound;
    [Tooltip("Звук активації")]
    public AudioClip activationSound;

    // Singleton
    public static UIAccessibilityManager Instance { get; private set; }

    // Screen Reader
    private Queue<string> screenReaderQueue = new Queue<string>();
    private bool isReadingText = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAccessibility();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAccessibility()
    {
        // Завантажуємо збережені налаштування
        LoadAccessibilitySettings();

        // Підписуємося на події навігації
        if (UINavigationManager.Instance != null)
        {
            UINavigationManager.OnFocusChanged += OnFocusChanged;
        }

        // Застосовуємо поточні налаштування
        ApplyAccessibilitySettings();
    }

    void OnFocusChanged(Selectable element)
    {
        if (enableAudioCues && focusSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(focusSound, 0.3f);
        }

        if (enableScreenReader)
        {
            string description = GetElementDescription(element);
            SpeakText(description);
        }
    }

    /// <summary>
    /// Отримує опис елемента для screen reader
    /// </summary>
    string GetElementDescription(Selectable element)
    {
        if (element == null) return "";

        // Спробуємо отримати опис з UIButton
        UIButton uiButton = element.GetComponent<UIButton>();
        if (uiButton != null)
        {
            return uiButton.GetAccessibilityDescription();
        }

        // Спробуємо отримати текст з кнопки
        Button button = element.GetComponent<Button>();
        if (button != null)
        {
            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                return $"Button: {text.text}";
            }
        }

        // Спробуємо отримати текст з toggle
        Toggle toggle = element.GetComponent<Toggle>();
        if (toggle != null)
        {
            TextMeshProUGUI text = toggle.GetComponentInChildren<TextMeshProUGUI>();
            string state = toggle.isOn ? "checked" : "unchecked";
            return $"Checkbox: {text?.text ?? "Toggle"}, {state}";
        }

        // Спробуємо отримати текст з input field
        TMP_InputField inputField = element.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            string placeholder = inputField.placeholder?.GetComponent<TextMeshProUGUI>()?.text ?? "";
            return $"Input field: {placeholder}";
        }

        return element.name;
    }

    /// <summary>
    /// Промовляє текст (симуляція screen reader)
    /// </summary>
    public void SpeakText(string text)
    {
        if (!enableScreenReader || string.IsNullOrEmpty(text)) return;

        screenReaderQueue.Enqueue(text);
        
        if (!isReadingText)
        {
            StartCoroutine(ProcessScreenReaderQueue());
        }
    }

    IEnumerator ProcessScreenReaderQueue()
    {
        isReadingText = true;

        while (screenReaderQueue.Count > 0)
        {
            string text = screenReaderQueue.Dequeue();
            
            // Тут можна додати інтеграцію з реальним screen reader API
            Debug.Log($"[SCREEN READER] {text}");
            
            // Симулюємо час читання
            yield return new WaitForSeconds(text.Length * 0.05f);
        }

        isReadingText = false;
    }

    /// <summary>
    /// Застосовує налаштування accessibility
    /// </summary>
    public void ApplyAccessibilitySettings()
    {
        // Створюємо налаштування для UI Framework
        AccessibilitySettings settings = new AccessibilitySettings
        {
            enableHighContrast = enableHighContrast,
            fontSizeMultiplier = fontSizeMultiplier,
            enableColorBlindSupport = enableColorBlindSupport,
            enableScreenReader = enableScreenReader,
            enableKeyboardNavigation = true,
            animationSpeedMultiplier = animationSpeedMultiplier
        };

        // Застосовуємо до UI Framework
        if (UnifiedUIFramework.Instance != null)
        {
            UnifiedUIFramework.Instance.UpdateAccessibilitySettings(settings);
        }

        // Застосовуємо до Theme Manager
        if (UIThemeManager.Instance != null)
        {
            UIThemeManager.Instance.ApplyAccessibilityModifications(enableHighContrast, enableColorBlindSupport);
        }

        // Оновлюємо швидкість анімацій
        if (disableAnimations)
        {
            Time.timeScale = 0f; // Тимчасово, потрібно кращий спосіб
        }
    }

    /// <summary>
    /// Увімкнути/вимкнути high contrast
    /// </summary>
    public void SetHighContrast(bool enabled)
    {
        enableHighContrast = enabled;
        ApplyAccessibilitySettings();
        SaveAccessibilitySettings();
    }

    /// <summary>
    /// Встановити множник розміру шрифту
    /// </summary>
    public void SetFontSizeMultiplier(float multiplier)
    {
        fontSizeMultiplier = Mathf.Clamp(multiplier, 0.5f, 3f);
        ApplyAccessibilitySettings();
        SaveAccessibilitySettings();
    }

    /// <summary>
    /// Увімкнути/вимкнути підтримку дальтоніків
    /// </summary>
    public void SetColorBlindSupport(bool enabled)
    {
        enableColorBlindSupport = enabled;
        ApplyAccessibilitySettings();
        SaveAccessibilitySettings();
    }

    /// <summary>
    /// Увімкнути/вимкнути screen reader
    /// </summary>
    public void SetScreenReader(bool enabled)
    {
        enableScreenReader = enabled;
        SaveAccessibilitySettings();
    }

    /// <summary>
    /// Зберігає налаштування accessibility
    /// </summary>
    void SaveAccessibilitySettings()
    {
        PlayerPrefs.SetInt("Accessibility_HighContrast", enableHighContrast ? 1 : 0);
        PlayerPrefs.SetFloat("Accessibility_FontSize", fontSizeMultiplier);
        PlayerPrefs.SetInt("Accessibility_ColorBlind", enableColorBlindSupport ? 1 : 0);
        PlayerPrefs.SetInt("Accessibility_ScreenReader", enableScreenReader ? 1 : 0);
        PlayerPrefs.SetFloat("Accessibility_AnimSpeed", animationSpeedMultiplier);
        PlayerPrefs.SetInt("Accessibility_DisableAnim", disableAnimations ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Завантажує налаштування accessibility
    /// </summary>
    void LoadAccessibilitySettings()
    {
        enableHighContrast = PlayerPrefs.GetInt("Accessibility_HighContrast", 0) == 1;
        fontSizeMultiplier = PlayerPrefs.GetFloat("Accessibility_FontSize", 1f);
        enableColorBlindSupport = PlayerPrefs.GetInt("Accessibility_ColorBlind", 0) == 1;
        enableScreenReader = PlayerPrefs.GetInt("Accessibility_ScreenReader", 0) == 1;
        animationSpeedMultiplier = PlayerPrefs.GetFloat("Accessibility_AnimSpeed", 1f);
        disableAnimations = PlayerPrefs.GetInt("Accessibility_DisableAnim", 0) == 1;
    }

    void OnDestroy()
    {
        SaveAccessibilitySettings();
    }
}

// ================================
// BREADCRUMB НАВІГАЦІЯ
// ================================

public class UIBreadcrumbNavigation : MonoBehaviour
{
    [Header("Breadcrumb Settings")]
    [Tooltip("Контейнер для breadcrumb елементів")]
    public Transform breadcrumbContainer;
    [Tooltip("Префаб breadcrumb елемента")]
    public GameObject breadcrumbItemPrefab;
    [Tooltip("Максимальна кількість breadcrumb")]
    public int maxBreadcrumbs = 5;

    private List<BreadcrumbItem> breadcrumbItems = new List<BreadcrumbItem>();

    /// <summary>
    /// Додає новий breadcrumb
    /// </summary>
    public void AddBreadcrumb(string title, System.Action onClicked = null)
    {
        // Видаляємо зайві breadcrumb
        while (breadcrumbItems.Count >= maxBreadcrumbs)
        {
            RemoveBreadcrumb(0);
        }

        // Створюємо новий breadcrumb
        GameObject breadcrumbObj = Instantiate(breadcrumbItemPrefab, breadcrumbContainer);
        BreadcrumbItem item = breadcrumbObj.GetComponent<BreadcrumbItem>();
        
        if (item == null)
        {
            item = breadcrumbObj.AddComponent<BreadcrumbItem>();
        }

        item.Initialize(title, onClicked);
        breadcrumbItems.Add(item);

        UpdateBreadcrumbAppearance();
    }

    /// <summary>
    /// Видаляє breadcrumb за індексом
    /// </summary>
    public void RemoveBreadcrumb(int index)
    {
        if (index >= 0 && index < breadcrumbItems.Count)
        {
            Destroy(breadcrumbItems[index].gameObject);
            breadcrumbItems.RemoveAt(index);
            UpdateBreadcrumbAppearance();
        }
    }

    /// <summary>
    /// Очищає всі breadcrumb
    /// </summary>
    public void ClearBreadcrumbs()
    {
        foreach (var item in breadcrumbItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        breadcrumbItems.Clear();
    }

    /// <summary>
    /// Оновлює зовнішній вигляд breadcrumb
    /// </summary>
    void UpdateBreadcrumbAppearance()
    {
        for (int i = 0; i < breadcrumbItems.Count; i++)
        {
            bool isLast = i == breadcrumbItems.Count - 1;
            breadcrumbItems[i].SetAsLast(isLast);
        }
    }
}

// ================================
// ІНТЕРФЕЙСИ
// ================================

public interface IUINavigable
{
    void OnNavigatedTo();
    void OnNavigatedBack();
    string GetNavigationTitle();
}

// ================================
// BREADCRUMB ITEM
// ================================

public class BreadcrumbItem : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI titleText;
    public Image separatorImage;

    private System.Action onClicked;

    public void Initialize(string title, System.Action clickCallback)
    {
        if (titleText != null)
            titleText.text = title;

        onClicked = clickCallback;

        if (button != null)
        {
            button.onClick.AddListener(() => onClicked?.Invoke());
        }
    }

    public void SetAsLast(bool isLast)
    {
        if (separatorImage != null)
        {
            separatorImage.gameObject.SetActive(!isLast);
        }

        if (button != null)
        {
            button.interactable = !isLast;
        }
    }
}