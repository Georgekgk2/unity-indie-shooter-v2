using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI меню системи перків
/// Завершення роботи попереднього агента - повна система перків
/// </summary>
public class PerkMenuUI : MonoBehaviour
{
    [Header("Main UI References")]
    [Tooltip("Головна панель меню перків")]
    public GameObject perkMenuPanel;
    [Tooltip("Кнопка відкриття меню перків")]
    public Button openPerkMenuButton;
    [Tooltip("Кнопка закриття меню")]
    public Button closePerkMenuButton;

    [Header("Player Info")]
    [Tooltip("Текст поточного рівня")]
    public TextMeshProUGUI playerLevelText;
    [Tooltip("Текст поточного XP")]
    public TextMeshProUGUI currentXPText;
    [Tooltip("Текст XP до наступного рівня")]
    public TextMeshProUGUI xpToNextLevelText;
    [Tooltip("Слайдер прогресу XP")]
    public Slider xpProgressSlider;
    [Tooltip("Текст доступних очок перків")]
    public TextMeshProUGUI availablePerkPointsText;

    [Header("Category Tabs")]
    [Tooltip("Кнопки категорій перків")]
    public Button[] categoryButtons;
    [Tooltip("Назви категорій")]
    public string[] categoryNames = { "Бій", "Виживання", "Рух", "Корисність", "Спеціальні" };
    [Tooltip("Кольори категорій")]
    public Color[] categoryColors;

    [Header("Perk Grid")]
    [Tooltip("Контейнер для перків")]
    public Transform perkGridContainer;
    [Tooltip("Префаб UI перка")]
    public GameObject perkUIPrefab;
    [Tooltip("Розмір сітки перків")]
    public Vector2 gridSize = new Vector2(5, 4);

    [Header("Perk Details")]
    [Tooltip("Панель деталей перка")]
    public GameObject perkDetailsPanel;
    [Tooltip("Назва вибраного перка")]
    public TextMeshProUGUI selectedPerkName;
    [Tooltip("Опис вибраного перка")]
    public TextMeshProUGUI selectedPerkDescription;
    [Tooltip("Іконка вибраного перка")]
    public Image selectedPerkIcon;
    [Tooltip("Кнопка розблокування")]
    public Button unlockPerkButton;

    [Header("Visual Effects")]
    [Tooltip("Анімація відкриття меню")]
    public AnimationCurve openAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Тривалість анімації")]
    public float animationDuration = 0.5f;

    // Приватні змінні
    private PerkSystem perkSystem;
    private PerkCategory currentCategory = PerkCategory.Combat;
    private Perk selectedPerk;
    private List<PerkUIComponent> perkUIComponents = new List<PerkUIComponent>();
    private bool isMenuOpen = false;

    // Кеш для оптимізації
    private Dictionary<PerkCategory, List<Perk>> categorizedPerks = new Dictionary<PerkCategory, List<Perk>>();

    void Start()
    {
        InitializeUI();
        SetupEventHandlers();
        CachePerks();
    }

    void OnEnable()
    {
        // Підписуємося на події
        if (PerkSystem.Instance != null)
        {
            PerkSystem.OnLevelUp += OnPlayerLevelUp;
            PerkSystem.OnPerkUnlocked += OnPerkUnlocked;
            PerkSystem.OnXPGained += OnXPGained;
        }
    }

    void OnDisable()
    {
        // Відписуємося від подій
        if (PerkSystem.Instance != null)
        {
            PerkSystem.OnLevelUp -= OnPlayerLevelUp;
            PerkSystem.OnPerkUnlocked -= OnPerkUnlocked;
            PerkSystem.OnXPGained -= OnXPGained;
        }
    }

    /// <summary>
    /// Ініціалізація UI
    /// </summary>
    void InitializeUI()
    {
        perkSystem = PerkSystem.Instance;
        
        if (perkSystem == null)
        {
            Debug.LogError("PerkMenuUI: PerkSystem не знайдено!");
            return;
        }

        // Закриваємо меню на початку
        if (perkMenuPanel != null)
            perkMenuPanel.SetActive(false);

        // Налаштовуємо кнопки категорій
        SetupCategoryButtons();

        // Оновлюємо інформацію про гравця
        UpdatePlayerInfo();

        // Показуємо перки першої категорії
        ShowPerksForCategory(currentCategory);
    }

    /// <summary>
    /// Налаштування обробників подій
    /// </summary>
    void SetupEventHandlers()
    {
        // Кнопка відкриття меню
        if (openPerkMenuButton != null)
            openPerkMenuButton.onClick.AddListener(OpenPerkMenu);

        // Кнопка закриття меню
        if (closePerkMenuButton != null)
            closePerkMenuButton.onClick.AddListener(ClosePerkMenu);

        // Кнопка розблокування перка
        if (unlockPerkButton != null)
            unlockPerkButton.onClick.AddListener(UnlockSelectedPerk);
    }

    /// <summary>
    /// Кешування перків за категоріями
    /// </summary>
    void CachePerks()
    {
        if (perkSystem == null) return;

        categorizedPerks.Clear();

        foreach (PerkCategory category in System.Enum.GetValues(typeof(PerkCategory)))
        {
            categorizedPerks[category] = perkSystem.GetPerksByCategory(category);
        }
    }

    /// <summary>
    /// Налаштування кнопок категорій
    /// </summary>
    void SetupCategoryButtons()
    {
        if (categoryButtons == null) return;

        for (int i = 0; i < categoryButtons.Length && i < categoryNames.Length; i++)
        {
            int categoryIndex = i;
            PerkCategory category = (PerkCategory)categoryIndex;
            
            // Налаштовуємо текст кнопки
            TextMeshProUGUI buttonText = categoryButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = categoryNames[i];

            // Налаштовуємо колір
            if (categoryColors != null && i < categoryColors.Length)
            {
                ColorBlock colors = categoryButtons[i].colors;
                colors.normalColor = categoryColors[i];
                categoryButtons[i].colors = colors;
            }

            // Додаємо обробник
            categoryButtons[i].onClick.AddListener(() => SelectCategory(category));
        }

        // Виділяємо першу категорію
        UpdateCategorySelection();
    }

    /// <summary>
    /// Вибір категорії перків
    /// </summary>
    public void SelectCategory(PerkCategory category)
    {
        currentCategory = category;
        UpdateCategorySelection();
        ShowPerksForCategory(category);
    }

    /// <summary>
    /// Оновлення виділення категорії
    /// </summary>
    void UpdateCategorySelection()
    {
        if (categoryButtons == null) return;

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            bool isSelected = (PerkCategory)i == currentCategory;
            
            // Змінюємо масштаб вибраної кнопки
            Transform buttonTransform = categoryButtons[i].transform;
            Vector3 targetScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
            
            LeanTween.scale(buttonTransform.gameObject, targetScale, 0.2f)
                .setEase(LeanTweenType.easeOutBack);
        }
    }

    /// <summary>
    /// Показує перки для вибраної категорії
    /// </summary>
    void ShowPerksForCategory(PerkCategory category)
    {
        if (!categorizedPerks.ContainsKey(category)) return;

        // Очищаємо попередні перки
        ClearPerkGrid();

        List<Perk> perks = categorizedPerks[category];
        
        // Сортуємо перки за рівнем та рангом
        perks = perks.OrderBy(p => p.tier).ThenBy(p => p.requiredLevel).ToList();

        // Створюємо UI для кожного перка
        for (int i = 0; i < perks.Count; i++)
        {
            CreatePerkUI(perks[i], i);
        }
    }

    /// <summary>
    /// Очищає сітку перків
    /// </summary>
    void ClearPerkGrid()
    {
        foreach (var component in perkUIComponents)
        {
            if (component != null && component.gameObject != null)
                Destroy(component.gameObject);
        }
        perkUIComponents.Clear();
    }

    /// <summary>
    /// Створює UI для перка
    /// </summary>
    void CreatePerkUI(Perk perk, int index)
    {
        if (perkUIPrefab == null || perkGridContainer == null) return;

        GameObject perkUI = Instantiate(perkUIPrefab, perkGridContainer);
        PerkUIComponent perkComponent = perkUI.GetComponent<PerkUIComponent>();

        if (perkComponent != null)
        {
            perkComponent.Initialize(perk, perkSystem);
            perkUIComponents.Add(perkComponent);

            // Додаємо обробник вибору перка
            Button perkButton = perkUI.GetComponent<Button>();
            if (perkButton != null)
            {
                perkButton.onClick.AddListener(() => SelectPerk(perk));
            }

            // Позиціонуємо перк у сітці
            PositionPerkInGrid(perkUI, index);

            // Анімація появи
            AnimatePerkAppearance(perkUI, index);
        }
    }

    /// <summary>
    /// Позиціонує перк у сітці
    /// </summary>
    void PositionPerkInGrid(GameObject perkUI, int index)
    {
        GridLayoutGroup gridLayout = perkGridContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout != null) return; // Якщо є GridLayoutGroup, він сам позиціонує

        // Ручне позиціонування
        int row = index / (int)gridSize.x;
        int col = index % (int)gridSize.x;

        RectTransform rectTransform = perkUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 position = new Vector2(col * 120f, -row * 120f);
            rectTransform.anchoredPosition = position;
        }
    }

    /// <summary>
    /// Анімація появи перка
    /// </summary>
    void AnimatePerkAppearance(GameObject perkUI, int index)
    {
        // Початкові значення
        perkUI.transform.localScale = Vector3.zero;
        
        // Анімація з затримкою
        float delay = index * 0.05f;
        
        LeanTween.scale(perkUI, Vector3.one, 0.3f)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutBack);
    }

    /// <summary>
    /// Вибір перка для деталей
    /// </summary>
    void SelectPerk(Perk perk)
    {
        selectedPerk = perk;
        UpdatePerkDetails();
    }

    /// <summary>
    /// Оновлення панелі деталей перка
    /// </summary>
    void UpdatePerkDetails()
    {
        if (selectedPerk == null || perkDetailsPanel == null) return;

        // Показуємо панель деталей
        perkDetailsPanel.SetActive(true);

        // Оновлюємо інформацію
        if (selectedPerkName != null)
            selectedPerkName.text = selectedPerk.name;

        if (selectedPerkDescription != null)
            selectedPerkDescription.text = GetDetailedDescription(selectedPerk);

        if (selectedPerkIcon != null && selectedPerk.icon != null)
            selectedPerkIcon.sprite = selectedPerk.icon;

        // Оновлюємо кнопку розблокування
        UpdateUnlockButton();
    }

    /// <summary>
    /// Отримує детальний опис перка
    /// </summary>
    string GetDetailedDescription(Perk perk)
    {
        string description = perk.description;

        // Додаємо статистику
        description += $"\n\n<b>Статистика:</b>";
        description += $"\nКатегорія: {GetCategoryName(perk.category)}";
        description += $"\nРанг: {perk.tier}";
        description += $"\nВимагає рівень: {perk.requiredLevel}";
        description += $"\nВартість: {perk.cost} очок";
        description += $"\nПрогрес: {perk.currentRank}/{perk.maxRank}";

        // Додаємо передумови
        if (perk.prerequisites.Count > 0)
        {
            description += $"\n\n<b>Передумови:</b>";
            foreach (string prereq in perk.prerequisites)
            {
                string prereqName = GetPerkNameById(prereq);
                bool isMet = perkSystem.unlockedPerkIds.Contains(prereq);
                string status = isMet ? "<color=green>✓</color>" : "<color=red>✗</color>";
                description += $"\n{status} {prereqName}";
            }
        }

        return description;
    }

    /// <summary>
    /// Отримує назву категорії
    /// </summary>
    string GetCategoryName(PerkCategory category)
    {
        int index = (int)category;
        return index < categoryNames.Length ? categoryNames[index] : category.ToString();
    }

    /// <summary>
    /// Отримує назву перка за ID
    /// </summary>
    string GetPerkNameById(string perkId)
    {
        var perk = perkSystem.availablePerks.FirstOrDefault(p => p.id == perkId);
        return perk?.name ?? perkId;
    }

    /// <summary>
    /// Оновлення кнопки розблокування
    /// </summary>
    void UpdateUnlockButton()
    {
        if (unlockPerkButton == null || selectedPerk == null) return;

        bool canUnlock = perkSystem.CanUnlockPerk(selectedPerk);
        bool isMaxRank = selectedPerk.IsMaxRank;

        unlockPerkButton.interactable = canUnlock && !isMaxRank;

        // Оновлюємо текст кнопки
        TextMeshProUGUI buttonText = unlockPerkButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            if (isMaxRank)
                buttonText.text = "МАКСИМАЛЬНИЙ РАНГ";
            else if (canUnlock)
                buttonText.text = $"РОЗБЛОКУВАТИ ({selectedPerk.cost} очок)";
            else
                buttonText.text = "НЕ ДОСТУПНО";
        }
    }

    /// <summary>
    /// Розблокування вибраного перка
    /// </summary>
    void UnlockSelectedPerk()
    {
        if (selectedPerk != null && perkSystem != null)
        {
            bool success = perkSystem.UnlockPerk(selectedPerk.id);
            
            if (success)
            {
                // Оновлюємо UI
                UpdatePlayerInfo();
                UpdatePerkDetails();
                RefreshPerkGrid();
                
                // Звуковий ефект
                AudioManager.Instance?.PlaySFX("perk_unlock");
            }
        }
    }

    /// <summary>
    /// Оновлення інформації про гравця
    /// </summary>
    void UpdatePlayerInfo()
    {
        if (perkSystem == null) return;

        // Рівень
        if (playerLevelText != null)
            playerLevelText.text = $"Рівень {perkSystem.currentLevel}";

        // Поточний XP
        if (currentXPText != null)
            currentXPText.text = $"XP: {perkSystem.currentXP}";

        // XP до наступного рівня
        int xpToNext = perkSystem.GetXPRequiredForNextLevel();
        if (xpToNextLevelText != null)
        {
            if (xpToNext > 0)
                xpToNextLevelText.text = $"До наступного: {xpToNext}";
            else
                xpToNextLevelText.text = "МАКСИМАЛЬНИЙ РІВЕНЬ";
        }

        // Прогрес XP
        if (xpProgressSlider != null)
        {
            if (xpToNext > 0)
            {
                float progress = (float)perkSystem.currentXP / xpToNext;
                xpProgressSlider.value = progress;
            }
            else
            {
                xpProgressSlider.value = 1f;
            }
        }

        // Доступні очки перків
        if (availablePerkPointsText != null)
            availablePerkPointsText.text = $"Очки перків: {perkSystem.availablePerkPoints}";
    }

    /// <summary>
    /// Оновлення сітки перків
    /// </summary>
    void RefreshPerkGrid()
    {
        foreach (var component in perkUIComponents)
        {
            if (component != null)
                component.UpdateUI();
        }
    }

    /// <summary>
    /// Відкриття меню перків
    /// </summary>
    public void OpenPerkMenu()
    {
        if (isMenuOpen || perkMenuPanel == null) return;

        isMenuOpen = true;
        perkMenuPanel.SetActive(true);

        // Оновлюємо дані
        UpdatePlayerInfo();
        RefreshPerkGrid();

        // Анімація відкриття
        perkMenuPanel.transform.localScale = Vector3.zero;
        LeanTween.scale(perkMenuPanel, Vector3.one, animationDuration)
            .setEase(openAnimationCurve);

        // Паузимо гру
        Time.timeScale = 0f;
        
        // Звуковий ефект
        AudioManager.Instance?.PlaySFX("menu_open");
    }

    /// <summary>
    /// Закриття меню перків
    /// </summary>
    public void ClosePerkMenu()
    {
        if (!isMenuOpen) return;

        // Анімація закриття
        LeanTween.scale(perkMenuPanel, Vector3.zero, animationDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                perkMenuPanel.SetActive(false);
                isMenuOpen = false;
            });

        // Відновлюємо гру
        Time.timeScale = 1f;
        
        // Звуковий ефект
        AudioManager.Instance?.PlaySFX("menu_close");
    }

    // Обробники подій
    void OnPlayerLevelUp(int newLevel)
    {
        UpdatePlayerInfo();
        
        // Показуємо повідомлення про підвищення рівня
        if (NotificationSystem.Instance != null)
        {
            NotificationSystem.Instance.ShowNotification(
                $"Рівень підвищено до {newLevel}!",
                "Отримано 1 очко перків",
                NotificationType.LevelUp
            );
        }
    }

    void OnPerkUnlocked(Perk perk)
    {
        RefreshPerkGrid();
        UpdatePerkDetails();
    }

    void OnXPGained(int xp)
    {
        UpdatePlayerInfo();
    }

    void Update()
    {
        // Клавіша для відкриття/закриття меню перків
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isMenuOpen)
                ClosePerkMenu();
            else
                OpenPerkMenu();
        }

        // ESC для закриття меню
        if (Input.GetKeyDown(KeyCode.Escape) && isMenuOpen)
        {
            ClosePerkMenu();
        }
    }
}