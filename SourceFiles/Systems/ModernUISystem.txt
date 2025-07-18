using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Сучасна UI система з анімаціями, адаптивністю та підтримкою Event System.
/// Забезпечує професійний інтерфейс користувача для всіх аспектів гри.
/// </summary>
public class ModernUISystem : MonoBehaviour,
    IEventHandler<PlayerHealthChangedEvent>,
    IEventHandler<AmmoChangedEvent>,
    IEventHandler<WeaponSwitchedEvent>,
    IEventHandler<StaminaChangedEvent>,
    IEventHandler<ShowMessageEvent>
{
    [Header("HUD Elements")]
    [Tooltip("Панель здоров'я")]
    public HealthHUD healthHUD;
    [Tooltip("Панель зброї")]
    public WeaponHUD weaponHUD;
    [Tooltip("Панель стаміни")]
    public StaminaHUD staminaHUD;
    [Tooltip("Мініатюра")]
    public MinimapHUD minimapHUD;
    [Tooltip("Прицільна сітка")]
    public CrosshairHUD crosshairHUD;

    [Header("Menu Systems")]
    [Tooltip("Головне меню")]
    public MainMenu mainMenu;
    [Tooltip("Меню паузи")]
    public PauseMenu pauseMenu;
    [Tooltip("Меню налаштувань")]
    public SettingsMenu settingsMenu;
    [Tooltip("Меню інвентарю")]
    public InventoryMenu inventoryMenu;

    [Header("Notification System")]
    [Tooltip("Панель повідомлень")]
    public NotificationPanel notificationPanel;
    [Tooltip("Панель досягнень")]
    public AchievementPanel achievementPanel;

    [Header("Animation Settings")]
    [Tooltip("Швидкість анімацій UI")]
    [Range(0.1f, 3f)]
    public float animationSpeed = 1f;
    [Tooltip("Тип ease для анімацій")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Adaptive Settings")]
    [Tooltip("Автоматично адаптувати до розміру екрану?")]
    public bool autoAdaptToScreen = true;
    [Tooltip("Мінімальний розмір UI")]
    [Range(0.5f, 1f)]
    public float minUIScale = 0.8f;
    [Tooltip("Максимальний розмір UI")]
    [Range(1f, 2f)]
    public float maxUIScale = 1.5f;

    // Приватні змінні
    private Canvas mainCanvas;
    private CanvasScaler canvasScaler;
    private bool isGamePaused = false;
    private bool isMenuOpen = false;

    // Singleton
    public static ModernUISystem Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUISystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Підписуємося на події
        Events.Subscribe<PlayerHealthChangedEvent>(this);
        Events.Subscribe<AmmoChangedEvent>(this);
        Events.Subscribe<WeaponSwitchedEvent>(this);
        Events.Subscribe<StaminaChangedEvent>(this);
        Events.Subscribe<ShowMessageEvent>(this);

        // Ініціалізуємо UI
        SetupInitialUI();
    }

    void InitializeUISystem()
    {
        mainCanvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        if (autoAdaptToScreen)
        {
            AdaptToScreenSize();
        }

        Debug.Log("ModernUISystem ініціалізовано");
    }

    void SetupInitialUI()
    {
        // Показуємо HUD
        ShowHUD();
        
        // Приховуємо меню
        HideAllMenus();
        
        // Налаштовуємо початкові значення
        InitializeHUDValues();
    }

    /// <summary>
    /// Адаптує UI до розміру екрану
    /// </summary>
    void AdaptToScreenSize()
    {
        if (canvasScaler == null) return;

        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = 16f / 9f; // Цільове співвідношення

        if (screenRatio >= targetRatio)
        {
            // Широкий екран
            canvasScaler.matchWidthOrHeight = 0f; // Match width
        }
        else
        {
            // Вузький екран
            canvasScaler.matchWidthOrHeight = 1f; // Match height
        }

        // Адаптуємо масштаб UI
        float scale = Mathf.Clamp(screenRatio / targetRatio, minUIScale, maxUIScale);
        canvasScaler.scaleFactor = scale;
    }

    /// <summary>
    /// Показує HUD
    /// </summary>
    public void ShowHUD()
    {
        if (healthHUD != null) healthHUD.Show();
        if (weaponHUD != null) weaponHUD.Show();
        if (staminaHUD != null) staminaHUD.Show();
        if (minimapHUD != null) minimapHUD.Show();
        if (crosshairHUD != null) crosshairHUD.Show();
    }

    /// <summary>
    /// Приховує HUD
    /// </summary>
    public void HideHUD()
    {
        if (healthHUD != null) healthHUD.Hide();
        if (weaponHUD != null) weaponHUD.Hide();
        if (staminaHUD != null) staminaHUD.Hide();
        if (minimapHUD != null) minimapHUD.Hide();
        if (crosshairHUD != null) crosshairHUD.Hide();
    }

    /// <summary>
    /// Приховує всі меню
    /// </summary>
    public void HideAllMenus()
    {
        if (mainMenu != null) mainMenu.Hide();
        if (pauseMenu != null) pauseMenu.Hide();
        if (settingsMenu != null) settingsMenu.Hide();
        if (inventoryMenu != null) inventoryMenu.Hide();
        isMenuOpen = false;
    }

    /// <summary>
    /// Ініціалізує початкові значення HUD
    /// </summary>
    void InitializeHUDValues()
    {
        // Отримуємо початкові значення з компонентів гравця
        var playerHealth = FindObjectOfType<PlayerHealth>();
        var weaponController = FindObjectOfType<WeaponController>();
        var playerMovement = FindObjectOfType<PlayerMovement>();

        if (playerHealth != null && healthHUD != null)
        {
            healthHUD.UpdateHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }

        if (weaponController != null && weaponHUD != null)
        {
            weaponHUD.UpdateAmmo(weaponController.currentAmmo, weaponController.magazineSize);
        }

        if (playerMovement != null && staminaHUD != null)
        {
            staminaHUD.UpdateStamina(playerMovement.currentStamina, playerMovement.maxStamina);
        }
    }

    /// <summary>
    /// Перемикає меню паузи
    /// </summary>
    public void TogglePauseMenu()
    {
        if (pauseMenu == null) return;

        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Ставить гру на паузу
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
        isMenuOpen = true;
        
        HideHUD();
        if (pauseMenu != null) pauseMenu.Show();
        
        // Розблоковуємо курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Відновлює гру
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        isMenuOpen = false;
        
        ShowHUD();
        HideAllMenus();
        
        // Блокуємо курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Показує повідомлення
    /// </summary>
    public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = 3f)
    {
        if (notificationPanel != null)
        {
            notificationPanel.ShowNotification(message, type, duration);
        }
    }

    /// <summary>
    /// Показує досягнення
    /// </summary>
    public void ShowAchievement(string title, string description, Sprite icon = null)
    {
        if (achievementPanel != null)
        {
            achievementPanel.ShowAchievement(title, description, icon);
        }
    }

    // ================================
    // EVENT HANDLERS
    // ================================

    public void HandleEvent(PlayerHealthChangedEvent eventData)
    {
        if (healthHUD != null)
        {
            healthHUD.UpdateHealth(eventData.CurrentHealth, eventData.MaxHealth);
            
            if (eventData.IsDamage)
            {
                healthHUD.PlayDamageEffect();
            }
            else if (eventData.IsHealing)
            {
                healthHUD.PlayHealEffect();
            }
        }
    }

    public void HandleEvent(AmmoChangedEvent eventData)
    {
        if (weaponHUD != null)
        {
            weaponHUD.UpdateAmmo(eventData.CurrentAmmo, eventData.MaxAmmo);
            weaponHUD.UpdateReloadCharges(eventData.ReloadCharges);
            
            if (eventData.IsEmpty)
            {
                weaponHUD.PlayEmptyEffect();
            }
        }
    }

    public void HandleEvent(WeaponSwitchedEvent eventData)
    {
        if (weaponHUD != null)
        {
            weaponHUD.UpdateWeaponName(eventData.NewWeapon);
            weaponHUD.PlaySwitchAnimation();
        }
    }

    public void HandleEvent(StaminaChangedEvent eventData)
    {
        if (staminaHUD != null)
        {
            staminaHUD.UpdateStamina(eventData.CurrentStamina, eventData.MaxStamina);
            
            if (eventData.IsExhausted)
            {
                staminaHUD.PlayExhaustedEffect();
            }
        }
    }

    public void HandleEvent(ShowMessageEvent eventData)
    {
        NotificationType type = NotificationType.Info;
        switch (eventData.Type)
        {
            case ShowMessageEvent.MessageType.Warning:
                type = NotificationType.Warning;
                break;
            case ShowMessageEvent.MessageType.Error:
                type = NotificationType.Error;
                break;
            case ShowMessageEvent.MessageType.Success:
                type = NotificationType.Success;
                break;
        }
        
        ShowNotification(eventData.Message, type, eventData.Duration);
    }

    void Update()
    {
        // Обробка input для UI
        HandleUIInput();
        
        // Адаптація до зміни розміру екрану
        if (autoAdaptToScreen && Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            AdaptToScreenSize();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    private int lastScreenWidth, lastScreenHeight;

    void HandleUIInput()
    {
        // Пауза
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
        
        // Інвентар
        if (Input.GetKeyDown(KeyCode.Tab) && !isGamePaused)
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// Перемикає інвентар
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryMenu == null) return;

        if (inventoryMenu.IsVisible)
        {
            inventoryMenu.Hide();
            ShowHUD();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            inventoryMenu.Show();
            HideHUD();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnDestroy()
    {
        // Відписуємося від подій
        Events.Unsubscribe<PlayerHealthChangedEvent>(this);
        Events.Unsubscribe<AmmoChangedEvent>(this);
        Events.Unsubscribe<WeaponSwitchedEvent>(this);
        Events.Unsubscribe<StaminaChangedEvent>(this);
        Events.Unsubscribe<ShowMessageEvent>(this);
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}

// ================================
// HUD COMPONENTS
// ================================

[System.Serializable]
public class HealthHUD : UIComponent
{
    [Header("Health UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image healthFill;
    public Image damageOverlay;
    
    [Header("Health Colors")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color criticalHealthColor = Color.red;
    
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
        
        if (healthFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
    }
    
    public void PlayDamageEffect()
    {
        if (damageOverlay != null)
        {
            StartCoroutine(FlashEffect(damageOverlay, Color.red, 0.3f));
        }
    }
    
    public void PlayHealEffect()
    {
        if (damageOverlay != null)
        {
            StartCoroutine(FlashEffect(damageOverlay, Color.green, 0.3f));
        }
    }
}

[System.Serializable]
public class WeaponHUD : UIComponent
{
    [Header("Weapon UI")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;
    public Slider reloadProgress;
    public Image weaponIcon;
    public Transform reloadChargesParent;
    
    public void UpdateWeaponName(string weaponName)
    {
        if (weaponNameText != null)
        {
            weaponNameText.text = weaponName;
        }
    }
    
    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo}/{maxAmmo}";
        }
    }
    
    public void UpdateReloadCharges(int charges)
    {
        if (reloadChargesParent != null)
        {
            for (int i = 0; i < reloadChargesParent.childCount; i++)
            {
                reloadChargesParent.GetChild(i).gameObject.SetActive(i < charges);
            }
        }
    }
    
    public void PlaySwitchAnimation()
    {
        if (weaponIcon != null)
        {
            StartCoroutine(ScaleAnimation(weaponIcon.transform, 1.2f, 0.2f));
        }
    }
    
    public void PlayEmptyEffect()
    {
        if (ammoText != null)
        {
            StartCoroutine(FlashEffect(ammoText, Color.red, 0.5f));
        }
    }
}

[System.Serializable]
public class StaminaHUD : UIComponent
{
    [Header("Stamina UI")]
    public Slider staminaSlider;
    public Image staminaFill;
    public Image exhaustedOverlay;
    
    [Header("Stamina Colors")]
    public Color fullStaminaColor = Color.blue;
    public Color lowStaminaColor = Color.yellow;
    public Color exhaustedColor = Color.red;
    
    public void UpdateStamina(float currentStamina, float maxStamina)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina / maxStamina;
        }
        
        if (staminaFill != null)
        {
            float staminaPercent = currentStamina / maxStamina;
            staminaFill.color = Color.Lerp(lowStaminaColor, fullStaminaColor, staminaPercent);
        }
    }
    
    public void PlayExhaustedEffect()
    {
        if (exhaustedOverlay != null)
        {
            StartCoroutine(FlashEffect(exhaustedOverlay, exhaustedColor, 1f));
        }
    }
}

[System.Serializable]
public class CrosshairHUD : UIComponent
{
    [Header("Crosshair UI")]
    public RectTransform crosshairContainer;
    public Image[] crosshairParts;
    
    [Header("Crosshair Settings")]
    public float baseSpread = 20f;
    public float maxSpread = 100f;
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    
    public void UpdateSpread(float spreadAmount)
    {
        if (crosshairContainer != null)
        {
            float spread = Mathf.Lerp(baseSpread, maxSpread, spreadAmount);
            // Логіка розширення прицільної сітки
        }
    }
    
    public void SetEnemyTarget(bool isEnemy)
    {
        Color targetColor = isEnemy ? enemyColor : normalColor;
        foreach (var part in crosshairParts)
        {
            if (part != null)
            {
                part.color = targetColor;
            }
        }
    }
}

// ================================
// BASE UI COMPONENT
// ================================

[System.Serializable]
public abstract class UIComponent
{
    [Header("Base UI Settings")]
    public GameObject rootObject;
    public CanvasGroup canvasGroup;
    
    public bool IsVisible => rootObject != null && rootObject.activeInHierarchy;
    
    public virtual void Show()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(true);
            if (canvasGroup != null)
            {
                StartCoroutine(FadeIn(0.3f));
            }
        }
    }
    
    public virtual void Hide()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut(0.3f));
        }
        else if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }
    
    protected IEnumerator FadeIn(float duration)
    {
        if (canvasGroup == null) yield break;
        
        float startAlpha = canvasGroup.alpha;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    protected IEnumerator FadeOut(float duration)
    {
        if (canvasGroup == null) yield break;
        
        float startAlpha = canvasGroup.alpha;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }
    
    protected IEnumerator FlashEffect(Graphic graphic, Color flashColor, float duration)
    {
        if (graphic == null) yield break;
        
        Color originalColor = graphic.color;
        graphic.color = flashColor;
        
        yield return new WaitForSecondsRealtime(duration);
        
        graphic.color = originalColor;
    }
    
    protected IEnumerator ScaleAnimation(Transform target, float scale, float duration)
    {
        if (target == null) yield break;
        
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * scale;
        
        // Scale up
        for (float t = 0; t < duration / 2; t += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.Lerp(originalScale, targetScale, t / (duration / 2));
            yield return null;
        }
        
        // Scale down
        for (float t = 0; t < duration / 2; t += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.Lerp(targetScale, originalScale, t / (duration / 2));
            yield return null;
        }
        
        target.localScale = originalScale;
    }
    
    protected void StartCoroutine(IEnumerator coroutine)
    {
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.StartCoroutine(coroutine);
        }
    }
}