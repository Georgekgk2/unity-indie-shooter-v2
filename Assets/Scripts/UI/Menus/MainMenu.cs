using UnityEngine;
using UnityEngine.UI;
using IndieShooter.UI.Managers;
using IndieShooter.Core;

namespace IndieShooter.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button playButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        
        [Header("Game Mode Buttons")]
        public Button singlePlayerButton;
        public Button multiPlayerButton;
        public Button survivalButton;
        
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject gameModePanel;
        public GameObject creditsPanel;
        
        [Header("Animation")]
        public Animator menuAnimator;
        public float buttonAnimationDelay = 0.1f;
        
        [Header("Audio")]
        public AudioClip buttonHoverSound;
        public AudioClip buttonClickSound;
        public AudioClip menuMusicClip;
        
        private AudioSource audioSource;
        
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
                
            SetupButtons();
            ShowMainPanel();
            PlayMenuMusic();
        }
        
        void SetupButtons()
        {
            // Main menu buttons
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
                AddButtonSounds(playButton);
            }
            
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
                AddButtonSounds(settingsButton);
            }
            
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
                AddButtonSounds(creditsButton);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
                AddButtonSounds(quitButton);
            }
            
            // Game mode buttons
            if (singlePlayerButton != null)
            {
                singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
                AddButtonSounds(singlePlayerButton);
            }
            
            if (multiPlayerButton != null)
            {
                multiPlayerButton.onClick.AddListener(OnMultiPlayerClicked);
                AddButtonSounds(multiPlayerButton);
            }
            
            if (survivalButton != null)
            {
                survivalButton.onClick.AddListener(OnSurvivalClicked);
                AddButtonSounds(survivalButton);
            }
        }
        
        void AddButtonSounds(Button button)
        {
            // Add hover sound
            var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => { PlayButtonHover(); });
            eventTrigger.triggers.Add(hoverEntry);
            
            // Click sound is handled in button methods
        }
        
        void PlayButtonHover()
        {
            if (buttonHoverSound != null && audioSource != null)
                audioSource.PlayOneShot(buttonHoverSound);
        }
        
        void PlayButtonClick()
        {
            if (buttonClickSound != null && audioSource != null)
                audioSource.PlayOneShot(buttonClickSound);
        }
        
        void PlayMenuMusic()
        {
            if (menuMusicClip != null && audioSource != null)
            {
                audioSource.clip = menuMusicClip;
                audioSource.loop = true;
                audioSource.volume = 0.5f;
                audioSource.Play();
            }
        }
        
        void ShowMainPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (gameModePanel != null) gameModePanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            
            if (menuAnimator != null)
                menuAnimator.SetTrigger("ShowMain");
        }
        
        void ShowGameModePanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (gameModePanel != null) gameModePanel.SetActive(true);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            
            if (menuAnimator != null)
                menuAnimator.SetTrigger("ShowGameMode");
        }
        
        void ShowCreditsPanel()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (gameModePanel != null) gameModePanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
            
            if (menuAnimator != null)
                menuAnimator.SetTrigger("ShowCredits");
        }
        
        // Button event handlers
        void OnPlayClicked()
        {
            PlayButtonClick();
            ShowGameModePanel();
        }
        
        void OnSettingsClicked()
        {
            PlayButtonClick();
            if (UIManager.Instance != null)
                UIManager.Instance.OpenSettings();
        }
        
        void OnCreditsClicked()
        {
            PlayButtonClick();
            ShowCreditsPanel();
        }
        
        void OnQuitClicked()
        {
            PlayButtonClick();
            if (UIManager.Instance != null)
                UIManager.Instance.QuitGame();
        }
        
        void OnSinglePlayerClicked()
        {
            PlayButtonClick();
            StartSinglePlayerGame();
        }
        
        void OnMultiPlayerClicked()
        {
            PlayButtonClick();
            // Placeholder for multiplayer
            Debug.Log("Multiplayer not implemented yet");
        }
        
        void OnSurvivalClicked()
        {
            PlayButtonClick();
            StartSurvivalMode();
        }
        
        void StartSinglePlayerGame()
        {
            // Set game mode
            PlayerPrefs.SetString("GameMode", "SinglePlayer");
            
            // Trigger game start
            EventSystem.Instance?.TriggerEvent("GameStartRequested", "SinglePlayer");
            
            if (UIManager.Instance != null)
                UIManager.Instance.StartGame();
        }
        
        void StartSurvivalMode()
        {
            // Set game mode
            PlayerPrefs.SetString("GameMode", "Survival");
            
            // Trigger game start
            EventSystem.Instance?.TriggerEvent("GameStartRequested", "Survival");
            
            if (UIManager.Instance != null)
                UIManager.Instance.StartGame();
        }
        
        // Public methods for back buttons
        public void BackToMain()
        {
            PlayButtonClick();
            ShowMainPanel();
        }
        
        public void BackToGameMode()
        {
            PlayButtonClick();
            ShowGameModePanel();
        }
        
        // Animation events
        public void OnMenuAnimationComplete()
        {
            // Called by animation events
        }
        
        void OnEnable()
        {
            // Reset to main panel when menu is enabled
            ShowMainPanel();
        }
    }
}