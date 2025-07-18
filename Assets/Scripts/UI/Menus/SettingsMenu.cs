using UnityEngine;
using UnityEngine.UI;
using IndieShooter.UI.Managers;
using IndieShooter.Audio;

namespace IndieShooter.UI.Menus
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Audio Settings")]
        public Slider masterVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider musicVolumeSlider;
        public Text masterVolumeText;
        public Text sfxVolumeText;
        public Text musicVolumeText;
        
        [Header("Graphics Settings")]
        public Dropdown qualityDropdown;
        public Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Toggle vsyncToggle;
        public Slider fovSlider;
        public Text fovText;
        
        [Header("Gameplay Settings")]
        public Slider mouseSensitivitySlider;
        public Text mouseSensitivityText;
        public Toggle invertYToggle;
        public Dropdown difficultyDropdown;
        
        [Header("Controls")]
        public Button backButton;
        public Button resetToDefaultsButton;
        public Button applyButton;
        
        [Header("Tabs")]
        public Button audioTabButton;
        public Button graphicsTabButton;
        public Button gameplayTabButton;
        public GameObject audioPanel;
        public GameObject graphicsPanel;
        public GameObject gameplayPanel;
        
        private Resolution[] resolutions;
        
        void Start()
        {
            SetupButtons();
            SetupResolutions();
            LoadSettings();
            ShowAudioTab(); // Default tab
        }
        
        void SetupButtons()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
                
            if (resetToDefaultsButton != null)
                resetToDefaultsButton.onClick.AddListener(OnResetToDefaultsClicked);
                
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyClicked);
                
            // Tab buttons
            if (audioTabButton != null)
                audioTabButton.onClick.AddListener(ShowAudioTab);
                
            if (graphicsTabButton != null)
                graphicsTabButton.onClick.AddListener(ShowGraphicsTab);
                
            if (gameplayTabButton != null)
                gameplayTabButton.onClick.AddListener(ShowGameplayTab);
                
            // Audio sliders
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
                
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
                
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
                
            // Graphics controls
            if (qualityDropdown != null)
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
                
            if (resolutionDropdown != null)
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
                
            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
                
            if (vsyncToggle != null)
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
                
            if (fovSlider != null)
                fovSlider.onValueChanged.AddListener(OnFOVChanged);
                
            // Gameplay controls
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
                
            if (invertYToggle != null)
                invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
                
            if (difficultyDropdown != null)
                difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }
        
        void SetupResolutions()
        {
            resolutions = Screen.resolutions;
            
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                
                var options = new System.Collections.Generic.List<string>();
                int currentResolutionIndex = 0;
                
                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = resolutions[i].width + " x " + resolutions[i].height;
                    options.Add(option);
                    
                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }
                
                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = currentResolutionIndex;
                resolutionDropdown.RefreshShownValue();
            }
        }
        
        void LoadSettings()
        {
            // Audio settings
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                OnMasterVolumeChanged(masterVolumeSlider.value);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
                OnSFXVolumeChanged(sfxVolumeSlider.value);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
                OnMusicVolumeChanged(musicVolumeSlider.value);
            }
            
            // Graphics settings
            if (qualityDropdown != null)
                qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
                
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = Screen.fullScreen;
                
            if (vsyncToggle != null)
                vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
                
            if (fovSlider != null)
            {
                fovSlider.value = PlayerPrefs.GetFloat("FOV", 60f);
                OnFOVChanged(fovSlider.value);
            }
            
            // Gameplay settings
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2f);
                OnMouseSensitivityChanged(mouseSensitivitySlider.value);
            }
            
            if (invertYToggle != null)
                invertYToggle.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
                
            if (difficultyDropdown != null)
                difficultyDropdown.value = PlayerPrefs.GetInt("Difficulty", 1);
        }
        
        void SaveSettings()
        {
            // Audio
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
                
            // Graphics
            if (qualityDropdown != null)
                PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
            if (fovSlider != null)
                PlayerPrefs.SetFloat("FOV", fovSlider.value);
                
            // Gameplay
            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);
            if (invertYToggle != null)
                PlayerPrefs.SetInt("InvertY", invertYToggle.isOn ? 1 : 0);
            if (difficultyDropdown != null)
                PlayerPrefs.SetInt("Difficulty", difficultyDropdown.value);
                
            PlayerPrefs.Save();
        }
        
        // Tab switching
        void ShowAudioTab()
        {
            SetActiveTab(audioPanel, audioTabButton);
        }
        
        void ShowGraphicsTab()
        {
            SetActiveTab(graphicsPanel, graphicsTabButton);
        }
        
        void ShowGameplayTab()
        {
            SetActiveTab(gameplayPanel, gameplayTabButton);
        }
        
        void SetActiveTab(GameObject activePanel, Button activeButton)
        {
            // Hide all panels
            if (audioPanel != null) audioPanel.SetActive(false);
            if (graphicsPanel != null) graphicsPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            
            // Show active panel
            if (activePanel != null) activePanel.SetActive(true);
            
            // Update button states (you can add visual feedback here)
        }
        
        // Audio event handlers
        void OnMasterVolumeChanged(float value)
        {
            if (masterVolumeText != null)
                masterVolumeText.text = $"{value * 100:F0}%";
                
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMasterVolume(value);
        }
        
        void OnSFXVolumeChanged(float value)
        {
            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{value * 100:F0}%";
                
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetSFXVolume(value);
        }
        
        void OnMusicVolumeChanged(float value)
        {
            if (musicVolumeText != null)
                musicVolumeText.text = $"{value * 100:F0}%";
                
            // Set music volume (implement in AudioManager if needed)
        }
        
        // Graphics event handlers
        void OnQualityChanged(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
        }
        
        void OnResolutionChanged(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
        
        void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }
        
        void OnVSyncChanged(bool isEnabled)
        {
            QualitySettings.vSyncCount = isEnabled ? 1 : 0;
        }
        
        void OnFOVChanged(float value)
        {
            if (fovText != null)
                fovText.text = $"{value:F0}°";
                
            // Apply FOV to camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                mainCamera.fieldOfView = value;
        }
        
        // Gameplay event handlers
        void OnMouseSensitivityChanged(float value)
        {
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = $"{value:F1}";
        }
        
        void OnInvertYChanged(bool isInverted)
        {
            // This will be used by player controller
        }
        
        void OnDifficultyChanged(int difficultyIndex)
        {
            // Apply difficulty settings
        }
        
        // Button event handlers
        void OnBackClicked()
        {
            SaveSettings();
            
            if (UIManager.Instance != null)
            {
                if (Time.timeScale == 0) // Coming from pause menu
                    UIManager.Instance.SetUIState(UIState.Paused);
                else // Coming from main menu
                    UIManager.Instance.SetUIState(UIState.MainMenu);
            }
        }
        
        void OnResetToDefaultsClicked()
        {
            // Reset all settings to defaults
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 0.8f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.6f;
            if (qualityDropdown != null) qualityDropdown.value = 2;
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            if (vsyncToggle != null) vsyncToggle.isOn = true;
            if (fovSlider != null) fovSlider.value = 60f;
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = 2f;
            if (invertYToggle != null) invertYToggle.isOn = false;
            if (difficultyDropdown != null) difficultyDropdown.value = 1;
        }
        
        void OnApplyClicked()
        {
            SaveSettings();
        }
    }
}