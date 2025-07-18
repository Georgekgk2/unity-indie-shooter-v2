using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using IndieShooter.Core;

namespace IndieShooter.UI.Managers
{
    public enum UIState
    {
        MainMenu,
        InGame,
        Paused,
        GameOver,
        Settings,
        Loading
    }
    
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("UI Panels")]
        public GameObject mainMenuPanel;
        public GameObject inGameHUD;
        public GameObject pauseMenuPanel;
        public GameObject gameOverPanel;
        public GameObject settingsPanel;
        public GameObject loadingPanel;
        
        [Header("Transition Settings")]
        public float transitionDuration = 0.3f;
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private UIState currentState = UIState.MainMenu;
        private Dictionary<UIState, GameObject> uiPanels;
        private bool isTransitioning = false;
        
        public UIState CurrentState => currentState;
        public bool IsTransitioning => isTransitioning;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Subscribe to game events
            EventSystem.Instance?.Subscribe("GameStarted", OnGameStarted);
            EventSystem.Instance?.Subscribe("GamePaused", OnGamePaused);
            EventSystem.Instance?.Subscribe("GameOver", OnGameOver);
            EventSystem.Instance?.Subscribe("PlayerDied", OnPlayerDied);
            EventSystem.Instance?.Subscribe("LevelLoaded", OnLevelLoaded);
        }
        
        void Update()
        {
            HandleInput();
        }
        
        void InitializeUI()
        {
            // Initialize UI panels dictionary
            uiPanels = new Dictionary<UIState, GameObject>
            {
                { UIState.MainMenu, mainMenuPanel },
                { UIState.InGame, inGameHUD },
                { UIState.Paused, pauseMenuPanel },
                { UIState.GameOver, gameOverPanel },
                { UIState.Settings, settingsPanel },
                { UIState.Loading, loadingPanel }
            };
            
            // Set initial state
            SetUIState(UIState.MainMenu, false);
        }
        
        void HandleInput()
        {
            // Handle escape key for pause/unpause
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (currentState)
                {
                    case UIState.InGame:
                        PauseGame();
                        break;
                    case UIState.Paused:
                        ResumeGame();
                        break;
                    case UIState.Settings:
                        if (Time.timeScale == 0) // Paused settings
                            SetUIState(UIState.Paused);
                        else // Main menu settings
                            SetUIState(UIState.MainMenu);
                        break;
                }
            }
        }
        
        public void SetUIState(UIState newState, bool animate = true)
        {
            if (currentState == newState || isTransitioning) return;
            
            if (animate)
            {
                StartCoroutine(TransitionToState(newState));
            }
            else
            {
                ActivateState(newState);
            }
        }
        
        System.Collections.IEnumerator TransitionToState(UIState newState)
        {
            isTransitioning = true;
            
            // Fade out current panel
            if (uiPanels.ContainsKey(currentState) && uiPanels[currentState] != null)
            {
                yield return StartCoroutine(FadePanel(uiPanels[currentState], false));
            }
            
            // Activate new state
            ActivateState(newState);
            
            // Fade in new panel
            if (uiPanels.ContainsKey(newState) && uiPanels[newState] != null)
            {
                yield return StartCoroutine(FadePanel(uiPanels[newState], true));
            }
            
            isTransitioning = false;
        }
        
        System.Collections.IEnumerator FadePanel(GameObject panel, bool fadeIn)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            
            float startAlpha = fadeIn ? 0f : 1f;
            float endAlpha = fadeIn ? 1f : 0f;
            float elapsed = 0f;
            
            if (fadeIn)
            {
                panel.SetActive(true);
            }
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / transitionDuration;
                float curveValue = transitionCurve.Evaluate(t);
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
                yield return null;
            }
            
            canvasGroup.alpha = endAlpha;
            
            if (!fadeIn)
            {
                panel.SetActive(false);
            }
        }
        
        void ActivateState(UIState newState)
        {
            // Deactivate all panels
            foreach (var panel in uiPanels.Values)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
            
            // Activate new panel
            if (uiPanels.ContainsKey(newState) && uiPanels[newState] != null)
            {
                uiPanels[newState].SetActive(true);
            }
            
            currentState = newState;
            
            // Handle cursor state
            UpdateCursorState();
            
            // Trigger state change event
            EventSystem.Instance?.TriggerEvent("UIStateChanged", newState);
        }
        
        void UpdateCursorState()
        {
            switch (currentState)
            {
                case UIState.InGame:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;
                default:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }
        
        // Public methods for UI actions
        public void StartGame()
        {
            SetUIState(UIState.Loading);
            // Load game scene or start gameplay
            EventSystem.Instance?.TriggerEvent("GameStartRequested", null);
        }
        
        public void PauseGame()
        {
            Time.timeScale = 0f;
            SetUIState(UIState.Paused);
            EventSystem.Instance?.TriggerEvent("GamePaused", true);
        }
        
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            SetUIState(UIState.InGame);
            EventSystem.Instance?.TriggerEvent("GamePaused", false);
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SetUIState(UIState.Loading);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            SetUIState(UIState.MainMenu);
            // Load main menu scene if needed
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        public void OpenSettings()
        {
            SetUIState(UIState.Settings);
        }
        
        public void ShowGameOver()
        {
            Time.timeScale = 0f;
            SetUIState(UIState.GameOver);
        }
        
        // Event handlers
        void OnGameStarted(object data)
        {
            SetUIState(UIState.InGame);
        }
        
        void OnGamePaused(object data)
        {
            bool isPaused = (bool)data;
            if (isPaused && currentState == UIState.InGame)
            {
                SetUIState(UIState.Paused);
            }
            else if (!isPaused && currentState == UIState.Paused)
            {
                SetUIState(UIState.InGame);
            }
        }
        
        void OnGameOver(object data)
        {
            ShowGameOver();
        }
        
        void OnPlayerDied(object data)
        {
            // Delay game over screen
            Invoke("ShowGameOver", 2f);
        }
        
        void OnLevelLoaded(object data)
        {
            SetUIState(UIState.InGame);
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("GameStarted", OnGameStarted);
            EventSystem.Instance?.Unsubscribe("GamePaused", OnGamePaused);
            EventSystem.Instance?.Unsubscribe("GameOver", OnGameOver);
            EventSystem.Instance?.Unsubscribe("PlayerDied", OnPlayerDied);
            EventSystem.Instance?.Unsubscribe("LevelLoaded", OnLevelLoaded);
        }
    }
}