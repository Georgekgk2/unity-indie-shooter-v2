using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.GameModes
{
    /// <summary>
    /// Основний контролер кооперативного режиму гри.
    /// Координує роботу всіх підсистем мультиплеєра.
    /// Замінює монолітний CooperativeMode_Multiplayer.cs (965 рядків).
    /// </summary>
    public class CooperativeMode : MonoBehaviour
    {
        [Header("Cooperative Mode Settings")]
        [Tooltip("Максимальна кількість гравців у кооперативному режимі")]
        public int maxPlayers = 4;
        [Tooltip("Чи потрібно чекати всіх гравців перед початком гри")]
        public bool waitForAllPlayers = true;
        [Tooltip("Час очікування гравців (у секундах)")]
        public float playerWaitTime = 30f;
        [Tooltip("Чи дозволено приєднання гравців під час гри")]
        public bool allowJoinInProgress = false;
        
        [Header("Game Balance")]
        [Tooltip("Множник складності залежно від кількості гравців")]
        public AnimationCurve difficultyMultiplier = AnimationCurve.Linear(1f, 1f, 4f, 2f);
        [Tooltip("Множник нагород залежно від кількості гравців")]
        public AnimationCurve rewardMultiplier = AnimationCurve.Linear(1f, 1f, 4f, 1.5f);
        
        // Підсистеми кооперативного режиму
        private NetworkManager networkManager;
        private PlayerSynchronization playerSync;
        private CoopLobbyManager lobbyManager;
        
        // Стан гри
        private CoopGameState currentState = CoopGameState.Lobby;
        private int connectedPlayers = 0;
        private float gameStartTimer = 0f;
        private bool gameInProgress = false;
        
        // Події
        public System.Action<CoopGameState> OnGameStateChanged;
        public System.Action<int> OnPlayerCountChanged;
        public System.Action OnGameStarted;
        public System.Action OnGameEnded;
        
        void Awake()
        {
            InitializeSubsystems();
        }
        
        void Start()
        {
            SetupCooperativeMode();
        }
        
        void Update()
        {
            UpdateGameState();
            UpdateTimers();
        }
        
        /// <summary>
        /// Ініціалізує підсистеми кооперативного режиму
        /// </summary>
        private void InitializeSubsystems()
        {
            // Отримуємо або створюємо підсистеми
            networkManager = GetComponent<NetworkManager>();
            playerSync = GetComponent<PlayerSynchronization>();
            lobbyManager = GetComponent<CoopLobbyManager>();
            
            // Якщо компоненти не знайдені, додаємо їх
            if (networkManager == null) networkManager = gameObject.AddComponent<NetworkManager>();
            if (playerSync == null) playerSync = gameObject.AddComponent<PlayerSynchronization>();
            if (lobbyManager == null) lobbyManager = gameObject.AddComponent<CoopLobbyManager>();
            
            // Ініціалізуємо підсистеми
            networkManager.Initialize(this);
            playerSync.Initialize(this);
            lobbyManager.Initialize(this);
        }
        
        /// <summary>
        /// Налаштовує кооперативний режим
        /// </summary>
        private void SetupCooperativeMode()
        {
            // Підписуємося на події підсистем
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected += OnPlayerConnected;
                networkManager.OnPlayerDisconnected += OnPlayerDisconnected;
                networkManager.OnNetworkError += OnNetworkError;
            }
            
            if (lobbyManager != null)
            {
                lobbyManager.OnLobbyReady += OnLobbyReady;
                lobbyManager.OnLobbyFull += OnLobbyFull;
            }
            
            // Інтеграція з EventSystem
            EventSystem.Instance?.Subscribe("PlayerJoined", OnPlayerJoinedEvent);
            EventSystem.Instance?.Subscribe("PlayerLeft", OnPlayerLeftEvent);
            EventSystem.Instance?.Subscribe("GameObjectiveCompleted", OnObjectiveCompleted);
            
            // Встановлюємо початковий стан
            ChangeGameState(CoopGameState.Lobby);
        }
        
        /// <summary>
        /// Оновлює стан гри
        /// </summary>
        private void UpdateGameState()
        {
            switch (currentState)
            {
                case CoopGameState.Lobby:
                    UpdateLobbyState();
                    break;
                    
                case CoopGameState.WaitingForPlayers:
                    UpdateWaitingState();
                    break;
                    
                case CoopGameState.Starting:
                    UpdateStartingState();
                    break;
                    
                case CoopGameState.InProgress:
                    UpdateGameplayState();
                    break;
                    
                case CoopGameState.Paused:
                    UpdatePausedState();
                    break;
                    
                case CoopGameState.Ending:
                    UpdateEndingState();
                    break;
            }
        }
        
        /// <summary>
        /// Оновлює таймери
        /// </summary>
        private void UpdateTimers()
        {
            if (currentState == CoopGameState.WaitingForPlayers)
            {
                gameStartTimer -= Time.deltaTime;
                if (gameStartTimer <= 0f)
                {
                    StartGameWithCurrentPlayers();
                }
            }
        }
        
        /// <summary>
        /// Оновлює стан лобі
        /// </summary>
        private void UpdateLobbyState()
        {
            // Перевіряємо готовність до початку гри
            if (CanStartGame())
            {
                if (waitForAllPlayers && connectedPlayers < maxPlayers)
                {
                    ChangeGameState(CoopGameState.WaitingForPlayers);
                }
                else
                {
                    ChangeGameState(CoopGameState.Starting);
                }
            }
        }
        
        /// <summary>
        /// Оновлює стан очікування гравців
        /// </summary>
        private void UpdateWaitingState()
        {
            // Якщо всі гравці приєдналися, починаємо гру
            if (connectedPlayers >= maxPlayers)
            {
                ChangeGameState(CoopGameState.Starting);
            }
        }
        
        /// <summary>
        /// Оновлює стан запуску гри
        /// </summary>
        private void UpdateStartingState()
        {
            // Тут можна додати countdown або інші підготовчі дії
            StartGame();
        }
        
        /// <summary>
        /// Оновлює стан геймплею
        /// </summary>
        private void UpdateGameplayState()
        {
            // Перевіряємо умови завершення гри
            if (ShouldEndGame())
            {
                ChangeGameState(CoopGameState.Ending);
            }
        }
        
        /// <summary>
        /// Оновлює стан паузи
        /// </summary>
        private void UpdatePausedState()
        {
            // Логіка паузи
        }
        
        /// <summary>
        /// Оновлює стан завершення гри
        /// </summary>
        private void UpdateEndingState()
        {
            EndGame();
        }
        
        /// <summary>
        /// Змінює стан гри
        /// </summary>
        private void ChangeGameState(CoopGameState newState)
        {
            if (currentState == newState) return;
            
            CoopGameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Cooperative Mode: State changed from {previousState} to {newState}");
            
            // Тригеримо події
            OnGameStateChanged?.Invoke(newState);
            
            // Інтеграція з EventSystem
            EventSystem.Instance?.TriggerEvent("CoopGameStateChanged", new {
                previousState = previousState,
                newState = newState,
                playerCount = connectedPlayers
            });
        }
        
        /// <summary>
        /// Перевіряє чи можна почати гру
        /// </summary>
        private bool CanStartGame()
        {
            return connectedPlayers >= 1 && lobbyManager != null && lobbyManager.IsLobbyReady();
        }
        
        /// <summary>
        /// Перевіряє чи потрібно завершити гру
        /// </summary>
        private bool ShouldEndGame()
        {
            // Гра завершується якщо всі гравці відключилися
            if (connectedPlayers <= 0) return true;
            
            // Або якщо виконані умови перемоги/поразки
            // Тут можна додати додаткову логіку
            
            return false;
        }
        
        /// <summary>
        /// Починає гру
        /// </summary>
        public void StartGame()
        {
            if (gameInProgress) return;
            
            gameInProgress = true;
            ChangeGameState(CoopGameState.InProgress);
            
            // Налаштовуємо складність та нагороди
            ApplyDifficultyScaling();
            
            // Тригеримо події
            OnGameStarted?.Invoke();
            
            Debug.Log($"Cooperative game started with {connectedPlayers} players");
        }
        
        /// <summary>
        /// Починає гру з поточними гравцями
        /// </summary>
        public void StartGameWithCurrentPlayers()
        {
            if (connectedPlayers > 0)
            {
                StartGame();
            }
        }
        
        /// <summary>
        /// Завершує гру
        /// </summary>
        public void EndGame()
        {
            if (!gameInProgress) return;
            
            gameInProgress = false;
            ChangeGameState(CoopGameState.Lobby);
            
            // Тригеримо події
            OnGameEnded?.Invoke();
            
            Debug.Log("Cooperative game ended");
        }
        
        /// <summary>
        /// Застосовує масштабування складності
        /// </summary>
        private void ApplyDifficultyScaling()
        {
            float difficulty = difficultyMultiplier.Evaluate(connectedPlayers);
            float rewards = rewardMultiplier.Evaluate(connectedPlayers);
            
            // Інтеграція з EventSystem для повідомлення інших систем
            EventSystem.Instance?.TriggerEvent("DifficultyScalingApplied", new {
                playerCount = connectedPlayers,
                difficultyMultiplier = difficulty,
                rewardMultiplier = rewards
            });
        }
        
        // === Обробники подій ===
        
        private void OnPlayerConnected(int playerId)
        {
            connectedPlayers++;
            OnPlayerCountChanged?.Invoke(connectedPlayers);
            
            Debug.Log($"Player {playerId} connected. Total players: {connectedPlayers}");
        }
        
        private void OnPlayerDisconnected(int playerId)
        {
            connectedPlayers--;
            OnPlayerCountChanged?.Invoke(connectedPlayers);
            
            Debug.Log($"Player {playerId} disconnected. Total players: {connectedPlayers}");
            
            // Якщо гра в процесі і всі гравці відключилися
            if (gameInProgress && connectedPlayers <= 0)
            {
                EndGame();
            }
        }
        
        private void OnNetworkError(string error)
        {
            Debug.LogError($"Network error in cooperative mode: {error}");
            // Тут можна додати обробку помилок мережі
        }
        
        private void OnLobbyReady()
        {
            Debug.Log("Lobby is ready for game start");
        }
        
        private void OnLobbyFull()
        {
            Debug.Log("Lobby is full, starting game immediately");
            ChangeGameState(CoopGameState.Starting);
        }
        
        private void OnPlayerJoinedEvent(object data)
        {
            // Обробка події приєднання гравця через EventSystem
        }
        
        private void OnPlayerLeftEvent(object data)
        {
            // Обробка події відключення гравця через EventSystem
        }
        
        private void OnObjectiveCompleted(object data)
        {
            // Обробка завершення цілей гри
        }
        
        // === Публічні методи ===
        
        public CoopGameState GetCurrentState() => currentState;
        public int GetConnectedPlayersCount() => connectedPlayers;
        public int GetMaxPlayers() => maxPlayers;
        public bool IsGameInProgress() => gameInProgress;
        public float GetDifficultyMultiplier() => difficultyMultiplier.Evaluate(connectedPlayers);
        public float GetRewardMultiplier() => rewardMultiplier.Evaluate(connectedPlayers);
        
        void OnDestroy()
        {
            // Відписуємося від подій
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected -= OnPlayerConnected;
                networkManager.OnPlayerDisconnected -= OnPlayerDisconnected;
                networkManager.OnNetworkError -= OnNetworkError;
            }
            
            EventSystem.Instance?.Unsubscribe("PlayerJoined", OnPlayerJoinedEvent);
            EventSystem.Instance?.Unsubscribe("PlayerLeft", OnPlayerLeftEvent);
            EventSystem.Instance?.Unsubscribe("GameObjectiveCompleted", OnObjectiveCompleted);
        }
    }
    
    /// <summary>
    /// Стани кооперативного режиму гри
    /// </summary>
    public enum CoopGameState
    {
        Lobby,              // Лобі, очікування гравців
        WaitingForPlayers,  // Очікування додаткових гравців
        Starting,           // Запуск гри
        InProgress,         // Гра в процесі
        Paused,             // Гра на паузі
        Ending              // Завершення гри
    }
}