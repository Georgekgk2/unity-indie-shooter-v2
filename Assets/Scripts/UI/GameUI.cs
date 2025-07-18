using UnityEngine;
using UnityEngine.UI;
using IndieShooter.Core;

namespace IndieShooter.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        public Text ammoText;
        public Text healthText;
        public Text scoreText;
        public GameObject crosshair;
        
        [Header("Menus")]
        public GameObject pauseMenu;
        public GameObject gameOverMenu;
        public Button resumeButton;
        public Button restartButton;
        public Button quitButton;
        
        private int currentAmmo = 30;
        private int maxAmmo = 30;
        private float currentHealth = 100f;
        private int score = 0;
        
        void Start()
        {
            // Subscribe to events
            EventSystem.Instance?.Subscribe("AmmoUpdated", OnAmmoUpdated);
            EventSystem.Instance?.Subscribe("HealthUpdated", OnHealthUpdated);
            EventSystem.Instance?.Subscribe("ScoreUpdated", OnScoreUpdated);
            EventSystem.Instance?.Subscribe("GamePaused", OnGamePaused);
            EventSystem.Instance?.Subscribe("GameOver", OnGameOver);
            
            // Setup button events
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
                
            // Initialize UI
            UpdateAmmoDisplay();
            UpdateHealthDisplay();
            UpdateScoreDisplay();
        }
        
        void OnAmmoUpdated(object data)
        {
            if (data != null)
            {
                var ammoData = (dynamic)data;
                currentAmmo = ammoData.current;
                maxAmmo = ammoData.max;
                UpdateAmmoDisplay();
            }
        }
        
        void OnHealthUpdated(object health)
        {
            if (health != null)
            {
                currentHealth = (float)health;
                UpdateHealthDisplay();
            }
        }
        
        void OnScoreUpdated(object newScore)
        {
            if (newScore != null)
            {
                score = (int)newScore;
                UpdateScoreDisplay();
            }
        }
        
        void OnGamePaused(object isPaused)
        {
            bool paused = (bool)isPaused;
            if (pauseMenu != null)
                pauseMenu.SetActive(paused);
        }
        
        void OnGameOver(object data)
        {
            if (gameOverMenu != null)
                gameOverMenu.SetActive(true);
        }
        
        void UpdateAmmoDisplay()
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
        }
        
        void UpdateHealthDisplay()
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {currentHealth:F0}";
                
                // Change color based on health
                if (currentHealth > 70)
                    healthText.color = Color.green;
                else if (currentHealth > 30)
                    healthText.color = Color.yellow;
                else
                    healthText.color = Color.red;
            }
        }
        
        void UpdateScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }
        
        void ResumeGame()
        {
            GameManager.Instance?.TogglePause();
        }
        
        void RestartGame()
        {
            GameManager.Instance?.RestartGame();
        }
        
        void QuitGame()
        {
            GameManager.Instance?.QuitGame();
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("AmmoUpdated", OnAmmoUpdated);
            EventSystem.Instance?.Unsubscribe("HealthUpdated", OnHealthUpdated);
            EventSystem.Instance?.Unsubscribe("ScoreUpdated", OnScoreUpdated);
            EventSystem.Instance?.Unsubscribe("GamePaused", OnGamePaused);
            EventSystem.Instance?.Unsubscribe("GameOver", OnGameOver);
        }
    }
}