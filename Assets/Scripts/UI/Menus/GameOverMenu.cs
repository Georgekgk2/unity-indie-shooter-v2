using UnityEngine;
using UnityEngine.UI;
using IndieShooter.UI.Managers;
using IndieShooter.Core;

namespace IndieShooter.UI.Menus
{
    public class GameOverMenu : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button restartButton;
        public Button mainMenuButton;
        public Button quitButton;
        
        [Header("Stats Display")]
        public Text finalScoreText;
        public Text killCountText;
        public Text survivalTimeText;
        public Text accuracyText;
        
        [Header("Animation")]
        public Animator gameOverAnimator;
        public float delayBeforeShow = 1f;
        
        private int finalScore = 0;
        private int totalKills = 0;
        private float totalTime = 0f;
        private float accuracy = 0f;
        
        void Start()
        {
            SetupButtons();
            
            // Subscribe to events
            EventSystem.Instance?.Subscribe("GameOver", OnGameOver);
        }
        
        void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        void OnEnable()
        {
            // Delay showing the menu for dramatic effect
            Invoke("ShowGameOverMenu", delayBeforeShow);
        }
        
        void ShowGameOverMenu()
        {
            if (gameOverAnimator != null)
                gameOverAnimator.SetTrigger("Show");
                
            UpdateStatsDisplay();
        }
        
        void UpdateStatsDisplay()
        {
            // Get stats from game session
            finalScore = PlayerPrefs.GetInt("SessionScore", 0);
            totalKills = PlayerPrefs.GetInt("SessionKills", 0);
            totalTime = PlayerPrefs.GetFloat("SessionTime", 0f);
            accuracy = PlayerPrefs.GetFloat("SessionAccuracy", 0f);
            
            // Update UI
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {finalScore}";
                
            if (killCountText != null)
                killCountText.text = $"Enemies Eliminated: {totalKills}";
                
            if (survivalTimeText != null)
            {
                int minutes = Mathf.FloorToInt(totalTime / 60);
                int seconds = Mathf.FloorToInt(totalTime % 60);
                survivalTimeText.text = $"Survival Time: {minutes:00}:{seconds:00}";
            }
            
            if (accuracyText != null)
                accuracyText.text = $"Accuracy: {accuracy:F1}%";
                
            // Save high score
            SaveHighScore();
        }
        
        void SaveHighScore()
        {
            int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (finalScore > currentHighScore)
            {
                PlayerPrefs.SetInt("HighScore", finalScore);
                PlayerPrefs.Save();
                
                // Show new high score notification
                ShowNewHighScoreEffect();
            }
        }
        
        void ShowNewHighScoreEffect()
        {
            // Add visual effect for new high score
            if (gameOverAnimator != null)
                gameOverAnimator.SetTrigger("NewHighScore");
        }
        
        void OnRestartClicked()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.RestartGame();
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
        
        void OnGameOver(object data)
        {
            // Game over event received
            if (data != null)
            {
                // Extract game stats from data if provided
                var gameStats = data as System.Collections.Generic.Dictionary<string, object>;
                if (gameStats != null)
                {
                    if (gameStats.ContainsKey("score"))
                        finalScore = (int)gameStats["score"];
                    if (gameStats.ContainsKey("kills"))
                        totalKills = (int)gameStats["kills"];
                    if (gameStats.ContainsKey("time"))
                        totalTime = (float)gameStats["time"];
                    if (gameStats.ContainsKey("accuracy"))
                        accuracy = (float)gameStats["accuracy"];
                }
            }
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("GameOver", OnGameOver);
        }
    }
}