using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Колекція всіх меню систем для гри. Включає головне меню, паузу, налаштування та інші.
/// </summary>

// ================================
// ГОЛОВНЕ МЕНЮ
// ================================

[System.Serializable]
public class MainMenu : UIComponent
{
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button exitButton;

    [Header("Menu Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject loadingPanel;

    [Header("Loading")]
    public Slider loadingProgressBar;
    public TextMeshProUGUI loadingText;

    public void Initialize()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Перевіряємо, чи є збережена гра
        UpdateContinueButton();
    }

    public void StartNewGame()
    {
        PlayButtonSound();
        StartCoroutine(LoadGameScene("GameScene"));
    }

    public void ContinueGame()
    {
        if (HasSavedGame())
        {
            PlayButtonSound();
            // Завантажуємо збережену гру
            LoadSavedGame();
        }
    }

    public void OpenSettings()
    {
        PlayButtonSound();
        if (ModernUISystem.Instance?.settingsMenu != null)
        {
            Hide();
            ModernUISystem.Instance.settingsMenu.Show();
        }
    }

    public void ShowCredits()
    {
        PlayButtonSound();
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        PlayButtonSound();
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void ExitGame()
    {
        PlayButtonSound();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.interactable = HasSavedGame();
        }
    }

    bool HasSavedGame()
    {
        return PlayerPrefs.HasKey("SavedGame");
    }

    void LoadSavedGame()
    {
        // Логіка завантаження збереженої гри
        StartCoroutine(LoadGameScene("GameScene"));
    }

    IEnumerator LoadGameScene(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;
            
            if (loadingText != null)
                loadingText.text = $"Завантаження... {progress * 100:F0}%";

            if (asyncLoad.progress >= 0.9f)
            {
                if (loadingText != null)
                    loadingText.text = "Натисніть будь-яку клавішу для продовження";
                
                if (Input.anyKeyDown)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.uiAudio.buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound2D(AudioManager.Instance.uiAudio.buttonClickSound);
        }
    }
}

// ================================
// МЕНЮ ПАУЗИ
// ================================

[System.Serializable]
public class PauseMenu : UIComponent
{
    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button exitButton;

    [Header("Pause Menu UI")]
    public TextMeshProUGUI pauseTitle;
    public GameObject pausePanel;

    public void Initialize()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    public void ResumeGame()
    {
        PlayButtonSound();
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ResumeGame();
        }
    }

    public void OpenSettings()
    {
        PlayButtonSound();
        if (ModernUISystem.Instance?.settingsMenu != null)
        {
            Hide();
            ModernUISystem.Instance.settingsMenu.Show();
        }
    }

    public void ReturnToMainMenu()
    {
        PlayButtonSound();
        
        // Зберігаємо гру перед виходом
        SaveGame();
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ExitGame()
    {
        PlayButtonSound();
        
        // Зберігаємо гру перед виходом
        SaveGame();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void SaveGame()
    {
        // Логіка збереження гри
        var playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SavePlayerState();
        }
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.uiAudio.buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound2D(AudioManager.Instance.uiAudio.buttonClickSound);
        }
    }
}

// ================================
// МЕНЮ НАЛАШТУВАНЬ
// ================================

[System.Serializable]
public class SettingsMenu : UIComponent
{
    [Header("Settings Tabs")]
    public Button graphicsTab;
    public Button audioTab;
    public Button controlsTab;
    public Button gameplayTab;

    [Header("Settings Panels")]
    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject controlsPanel;
    public GameObject gameplayPanel;

    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;
    public Slider fovSlider;
    public TextMeshProUGUI fovValueText;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider voiceVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    public TextMeshProUGUI voiceVolumeText;

    [Header("Controls Settings")]
    public Slider mouseSensitivitySlider;
    public Toggle invertYToggle;
    public Button[] keyBindingButtons;
    public TextMeshProUGUI mouseSensitivityText;

    [Header("Gameplay Settings")]
    public Dropdown difficultyDropdown;
    public Toggle autoAimToggle;
    public Toggle subtitlesToggle;
    public Slider uiScaleSlider;
    public TextMeshProUGUI uiScaleText;

    [Header("Settings Buttons")]
    public Button applyButton;
    public Button resetButton;
    public Button backButton;

    private GameSettings gameSettings;

    public void Initialize()
    {
        gameSettings = ConfigurationManager.Instance?.gameSettings;
        
        SetupTabButtons();
        SetupSettingsControls();
        LoadCurrentSettings();
        
        ShowGraphicsPanel();
    }

    void SetupTabButtons()
    {
        if (graphicsTab != null)
            graphicsTab.onClick.AddListener(ShowGraphicsPanel);
        if (audioTab != null)
            audioTab.onClick.AddListener(ShowAudioPanel);
        if (controlsTab != null)
            controlsTab.onClick.AddListener(ShowControlsPanel);
        if (gameplayTab != null)
            gameplayTab.onClick.AddListener(ShowGameplayPanel);

        if (applyButton != null)
            applyButton.onClick.AddListener(ApplySettings);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetToDefaults);
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
    }

    void SetupSettingsControls()
    {
        // Graphics
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (fovSlider != null)
            fovSlider.onValueChanged.AddListener(OnFOVChanged);

        // Audio
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Controls
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);

        // Gameplay
        if (uiScaleSlider != null)
            uiScaleSlider.onValueChanged.AddListener(OnUIScaleChanged);
    }

    void LoadCurrentSettings()
    {
        if (gameSettings == null) return;

        // Graphics
        if (qualityDropdown != null)
            qualityDropdown.value = (int)gameSettings.graphicsQuality;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
        if (vsyncToggle != null)
            vsyncToggle.isOn = gameSettings.enableVSync;

        // Audio
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = gameSettings.masterVolume;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = gameSettings.musicVolume;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = gameSettings.sfxVolume;

        // Controls
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = gameSettings.mouseSensitivity;
        if (invertYToggle != null)
            invertYToggle.isOn = gameSettings.invertMouseY;

        // Gameplay
        if (difficultyDropdown != null)
            difficultyDropdown.value = (int)gameSettings.difficulty;
        if (autoAimToggle != null)
            autoAimToggle.isOn = gameSettings.enableAutoAim;
        if (uiScaleSlider != null)
            uiScaleSlider.value = gameSettings.uiScale;

        UpdateValueTexts();
    }

    void UpdateValueTexts()
    {
        if (fovValueText != null && fovSlider != null)
            fovValueText.text = fovSlider.value.ToString("F0");
        if (masterVolumeText != null && masterVolumeSlider != null)
            masterVolumeText.text = (masterVolumeSlider.value * 100).ToString("F0") + "%";
        if (musicVolumeText != null && musicVolumeSlider != null)
            musicVolumeText.text = (musicVolumeSlider.value * 100).ToString("F0") + "%";
        if (sfxVolumeText != null && sfxVolumeSlider != null)
            sfxVolumeText.text = (sfxVolumeSlider.value * 100).ToString("F0") + "%";
        if (mouseSensitivityText != null && mouseSensitivitySlider != null)
            mouseSensitivityText.text = mouseSensitivitySlider.value.ToString("F1");
        if (uiScaleText != null && uiScaleSlider != null)
            uiScaleText.text = (uiScaleSlider.value * 100).ToString("F0") + "%";
    }

    public void ShowGraphicsPanel()
    {
        HideAllPanels();
        if (graphicsPanel != null) graphicsPanel.SetActive(true);
    }

    public void ShowAudioPanel()
    {
        HideAllPanels();
        if (audioPanel != null) audioPanel.SetActive(true);
    }

    public void ShowControlsPanel()
    {
        HideAllPanels();
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void ShowGameplayPanel()
    {
        HideAllPanels();
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
    }

    void HideAllPanels()
    {
        if (graphicsPanel != null) graphicsPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
    }

    // Event handlers
    void OnQualityChanged(int value)
    {
        QualitySettings.SetQualityLevel(value);
    }

    void OnFOVChanged(float value)
    {
        if (fovValueText != null)
            fovValueText.text = value.ToString("F0");
    }

    void OnMasterVolumeChanged(float value)
    {
        if (masterVolumeText != null)
            masterVolumeText.text = (value * 100).ToString("F0") + "%";
        if (AudioManager.Instance != null)
            AudioManager.Instance.masterVolume = value;
    }

    void OnMusicVolumeChanged(float value)
    {
        if (musicVolumeText != null)
            musicVolumeText.text = (value * 100).ToString("F0") + "%";
        if (AudioManager.Instance != null)
            AudioManager.Instance.musicVolume = value;
    }

    void OnSFXVolumeChanged(float value)
    {
        if (sfxVolumeText != null)
            sfxVolumeText.text = (value * 100).ToString("F0") + "%";
        if (AudioManager.Instance != null)
            AudioManager.Instance.sfxVolume = value;
    }

    void OnMouseSensitivityChanged(float value)
    {
        if (mouseSensitivityText != null)
            mouseSensitivityText.text = value.ToString("F1");
    }

    void OnUIScaleChanged(float value)
    {
        if (uiScaleText != null)
            uiScaleText.text = (value * 100).ToString("F0") + "%";
    }

    public void ApplySettings()
    {
        if (gameSettings == null) return;

        // Зберігаємо налаштування
        SaveSettings();
        
        // Застосовуємо налаштування
        gameSettings.ApplyGraphicsSettings();
        gameSettings.ApplyPhysicsSettings();
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.ApplyGameSettings();

        PlayButtonSound();
        ShowNotification("Налаштування застосовано", ModernUISystem.NotificationType.Success);
    }

    public void ResetToDefaults()
    {
        // Скидаємо до стандартних налаштувань
        if (gameSettings != null)
        {
            gameSettings.difficulty = GameSettings.DifficultyLevel.Normal;
            gameSettings.masterVolume = 1f;
            gameSettings.musicVolume = 0.7f;
            gameSettings.sfxVolume = 1f;
            gameSettings.mouseSensitivity = 2f;
            gameSettings.uiScale = 1f;
        }

        LoadCurrentSettings();
        PlayButtonSound();
        ShowNotification("Налаштування скинуто", ModernUISystem.NotificationType.Info);
    }

    public void GoBack()
    {
        PlayButtonSound();
        Hide();
        
        // Повертаємося до попереднього меню
        if (ModernUISystem.Instance != null)
        {
            if (ModernUISystem.Instance.pauseMenu != null && Time.timeScale == 0f)
            {
                ModernUISystem.Instance.pauseMenu.Show();
            }
            else if (ModernUISystem.Instance.mainMenu != null)
            {
                ModernUISystem.Instance.mainMenu.Show();
            }
        }
    }

    void SaveSettings()
    {
        // Зберігаємо налаштування в PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider?.value ?? 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider?.value ?? 0.7f);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider?.value ?? 1f);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider?.value ?? 2f);
        PlayerPrefs.SetFloat("UIScale", uiScaleSlider?.value ?? 1f);
        PlayerPrefs.SetInt("Quality", qualityDropdown?.value ?? 2);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle?.isOn == true ? 1 : 0);
        PlayerPrefs.SetInt("VSync", vsyncToggle?.isOn == true ? 1 : 0);
        PlayerPrefs.Save();
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.uiAudio.buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound2D(AudioManager.Instance.uiAudio.buttonClickSound);
        }
    }

    void ShowNotification(string message, ModernUISystem.NotificationType type)
    {
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(message, type);
        }
    }
}

// ================================
// МЕНЮ ІНВЕНТАРЮ
// ================================

[System.Serializable]
public class InventoryMenu : UIComponent
{
    [Header("Inventory UI")]
    public Transform weaponSlotsParent;
    public Transform itemSlotsParent;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public Image selectedItemIcon;
    public Button useItemButton;
    public Button dropItemButton;

    [Header("Inventory Settings")]
    public int maxWeaponSlots = 4;
    public int maxItemSlots = 20;

    private InventorySlot[] weaponSlots;
    private InventorySlot[] itemSlots;
    private InventorySlot selectedSlot;

    public void Initialize()
    {
        CreateInventorySlots();
        
        if (useItemButton != null)
            useItemButton.onClick.AddListener(UseSelectedItem);
        if (dropItemButton != null)
            dropItemButton.onClick.AddListener(DropSelectedItem);

        RefreshInventory();
    }

    void CreateInventorySlots()
    {
        // Створюємо слоти для зброї
        weaponSlots = new InventorySlot[maxWeaponSlots];
        for (int i = 0; i < maxWeaponSlots; i++)
        {
            GameObject slotObj = new GameObject($"WeaponSlot_{i}");
            slotObj.transform.SetParent(weaponSlotsParent);
            
            InventorySlot slot = slotObj.AddComponent<InventorySlot>();
            slot.Initialize(this, InventorySlot.SlotType.Weapon);
            weaponSlots[i] = slot;
        }

        // Створюємо слоти для предметів
        itemSlots = new InventorySlot[maxItemSlots];
        for (int i = 0; i < maxItemSlots; i++)
        {
            GameObject slotObj = new GameObject($"ItemSlot_{i}");
            slotObj.transform.SetParent(itemSlotsParent);
            
            InventorySlot slot = slotObj.AddComponent<InventorySlot>();
            slot.Initialize(this, InventorySlot.SlotType.Item);
            itemSlots[i] = slot;
        }
    }

    public void RefreshInventory()
    {
        // Оновлюємо відображення інвентарю
        RefreshWeaponSlots();
        RefreshItemSlots();
    }

    void RefreshWeaponSlots()
    {
        var weaponSwitching = FindObjectOfType<WeaponSwitching>();
        if (weaponSwitching == null) return;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (i < weaponSwitching.GetWeaponSlotCount())
            {
                var weapon = weaponSwitching.GetCurrentWeaponGameObject();
                weaponSlots[i].SetItem(weapon?.name, null, 1);
            }
            else
            {
                weaponSlots[i].SetEmpty();
            }
        }
    }

    void RefreshItemSlots()
    {
        // Логіка оновлення слотів предметів
        // Тут можна додати систему предметів
    }

    public void SelectSlot(InventorySlot slot)
    {
        selectedSlot = slot;
        UpdateSelectedItemInfo();
    }

    void UpdateSelectedItemInfo()
    {
        if (selectedSlot == null || selectedSlot.IsEmpty)
        {
            if (selectedItemName != null) selectedItemName.text = "";
            if (selectedItemDescription != null) selectedItemDescription.text = "";
            if (selectedItemIcon != null) selectedItemIcon.sprite = null;
            if (useItemButton != null) useItemButton.interactable = false;
            if (dropItemButton != null) dropItemButton.interactable = false;
        }
        else
        {
            if (selectedItemName != null) selectedItemName.text = selectedSlot.ItemName;
            if (selectedItemDescription != null) selectedItemDescription.text = "Опис предмета"; // Можна розширити
            if (selectedItemIcon != null) selectedItemIcon.sprite = selectedSlot.ItemIcon;
            if (useItemButton != null) useItemButton.interactable = true;
            if (dropItemButton != null) dropItemButton.interactable = true;
        }
    }

    public void UseSelectedItem()
    {
        if (selectedSlot != null && !selectedSlot.IsEmpty)
        {
            // Логіка використання предмета
            PlayButtonSound();
        }
    }

    public void DropSelectedItem()
    {
        if (selectedSlot != null && !selectedSlot.IsEmpty)
        {
            // Логіка викидання предмета
            selectedSlot.SetEmpty();
            UpdateSelectedItemInfo();
            PlayButtonSound();
        }
    }

    void PlayButtonSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.uiAudio.buttonClickSound != null)
        {
            AudioManager.Instance.PlaySound2D(AudioManager.Instance.uiAudio.buttonClickSound);
        }
    }
}

// ================================
// СЛОТ ІНВЕНТАРЮ
// ================================

public class InventorySlot : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    public enum SlotType { Weapon, Item }

    [Header("Slot UI")]
    public Image slotBackground;
    public Image itemIcon;
    public TextMeshProUGUI itemCountText;

    public string ItemName { get; private set; }
    public Sprite ItemIcon { get; private set; }
    public int ItemCount { get; private set; }
    public bool IsEmpty => string.IsNullOrEmpty(ItemName);

    private InventoryMenu parentInventory;
    private SlotType slotType;

    public void Initialize(InventoryMenu parent, SlotType type)
    {
        parentInventory = parent;
        slotType = type;
        SetEmpty();
    }

    public void SetItem(string itemName, Sprite icon, int count)
    {
        ItemName = itemName;
        ItemIcon = icon;
        ItemCount = count;

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }

        if (itemCountText != null)
        {
            itemCountText.text = count > 1 ? count.ToString() : "";
        }
    }

    public void SetEmpty()
    {
        ItemName = "";
        ItemIcon = null;
        ItemCount = 0;

        if (itemIcon != null)
            itemIcon.enabled = false;
        if (itemCountText != null)
            itemCountText.text = "";
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (parentInventory != null)
        {
            parentInventory.SelectSlot(this);
        }
    }
}