using UnityEngine;
using UnityEngine.SceneManagement;

namespace IndieShooter.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        public bool isGamePaused = false;
        public bool isGameOver = false;
        
        [Header("Player References")]
        public GameObject playerPrefab;
        public Transform playerSpawnPoint;
        
        [Header("UI References")]
        public GameObject pauseMenu;
        public GameObject gameOverMenu;
        
        private GameObject currentPlayer;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            SpawnPlayer();
        }
        
        void Update()
        {
            HandleInput();
        }
        
        void InitializeGame()
        {
            // Initialize core systems
            if (EventSystem.Instance == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
            }
            
            // Subscribe to events
            EventSystem.Instance.Subscribe("PlayerDied", OnPlayerDied);
            EventSystem.Instance.Subscribe("GameWon", OnGameWon);
        }
        
        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
        
        public void SpawnPlayer()
        {
            if (playerPrefab != null && playerSpawnPoint != null)
            {
                if (currentPlayer != null)
                {
                    Destroy(currentPlayer);
                }
                
                currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
                EventSystem.Instance.TriggerEvent("PlayerSpawned", currentPlayer);
            }
        }
        
        public void TogglePause()
        {
            isGamePaused = !isGamePaused;
            Time.timeScale = isGamePaused ? 0f : 1f;
            
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(isGamePaused);
            }
            
            EventSystem.Instance.TriggerEvent("GamePaused", isGamePaused);
        }
        
        public void GameOver()
        {
            isGameOver = true;
            Time.timeScale = 0f;
            
            if (gameOverMenu != null)
            {
                gameOverMenu.SetActive(true);
            }
            
            EventSystem.Instance.TriggerEvent("GameOver", null);
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            isGameOver = false;
            isGamePaused = false;
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        void OnPlayerDied(object data)
        {
            Debug.Log("Player died!");
            Invoke("GameOver", 2f); // Delay for death animation
        }
        
        void OnGameWon(object data)
        {
            Debug.Log("Game won!");
            // Handle victory logic
        }
        
        void OnDestroy()
        {
            if (EventSystem.Instance != null)
            {
                EventSystem.Instance.Unsubscribe("PlayerDied", OnPlayerDied);
                EventSystem.Instance.Unsubscribe("GameWon", OnGameWon);
            }
        }
    }
}