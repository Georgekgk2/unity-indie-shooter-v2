using UnityEngine;
using UnityEngine.UI;
using IndieShooter.UI.Managers;

namespace IndieShooter.UI.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button resumeButton;
        public Button settingsButton;
        public Button mainMenuButton;
        public Button quitButton;
        
        [Header("Panels")]
        public GameObject pausePanel;
        public CanvasGroup pauseCanvasGroup;
        
        [Header("Animation")]
        public float fadeInDuration = 0.3f;
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        void Start()
        {
            SetupButtons();
            
            if (pauseCanvasGroup == null)
                pauseCanvasGroup = GetComponent<CanvasGroup>();
        }
        
        void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        void OnEnable()
        {
            if (pauseCanvasGroup != null)
            {
                StartCoroutine(FadeIn());
            }
        }
        
        System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            pauseCanvasGroup.alpha = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;
                pauseCanvasGroup.alpha = fadeInCurve.Evaluate(t);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 1f;
        }
        
        void OnResumeClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ResumeGame();
        }
        
        void OnSettingsClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OpenSettings();
        }
        
        void OnMainMenuClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.QuitToMainMenu();
        }
        
        void OnQuitClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.QuitGame();
        }
    }
}