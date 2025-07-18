using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.GameModes
{
    /// <summary>
    /// Мережевий менеджер для кооперативного режиму.
    /// Обробляє підключення, синхронізацію та мережеві події.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        [Header("Network Settings")]
        [Tooltip("Максимальна кількість гравців")]
        public int maxPlayers = 4;
        [Tooltip("Порт для мережевого з'єднання")]
        public int networkPort = 7777;
        [Tooltip("Інтервал синхронізації (секунди)")]
        public float syncInterval = 0.1f;
        [Tooltip("Таймаут з'єднання (секунди)")]
        public float connectionTimeout = 30f;
        
        [Header("Synchronization")]
        [Tooltip("Синхронізувати позиції гравців")]
        public bool syncPlayerPositions = true;
        [Tooltip("Синхронізувати стани ворогів")]
        public bool syncEnemyStates = true;
        [Tooltip("Синхронізувати об'єкти світу")]
        public bool syncWorldObjects = true;
        
        // Події мережі
        public System.Action<int> OnPlayerConnected;
        public System.Action<int> OnPlayerDisconnected;
        public System.Action<string> OnNetworkError;
        public System.Action OnServerStarted;
        public System.Action OnServerStopped;
        
        // Стан мережі
        private bool isServer = false;
        private bool isClient = false;
        private List<ConnectedPlayer> connectedPlayers = new List<ConnectedPlayer>();
        private float lastSyncTime = 0f;
        private CooperativeMode cooperativeMode;
        
        public void Initialize(CooperativeMode coopMode)
        {
            cooperativeMode = coopMode;
        }
        
        void Update()
        {
            if (isServer)
            {
                UpdateServerSync();
            }
            
            UpdateNetworkStatus();
        }
        
        /// <summary>
        /// Запускає сервер
        /// </summary>
        public bool StartServer()
        {
            try
            {
                // Тут буде логіка запуску сервера
                // В реальному проекті використовувався б Unity Netcode або Mirror
                
                isServer = true;
                Debug.Log($"Server started on port {networkPort}");
                
                OnServerStarted?.Invoke();
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start server: {e.Message}");
                OnNetworkError?.Invoke($"Server start failed: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Підключається до сервера як клієнт
        /// </summary>
        public bool ConnectToServer(string serverIP)
        {
            try
            {
                // Тут буде логіка підключення до сервера
                
                isClient = true;
                Debug.Log($"Connected to server at {serverIP}:{networkPort}");
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to connect to server: {e.Message}");
                OnNetworkError?.Invoke($"Connection failed: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Відключається від мережі
        /// </summary>
        public void Disconnect()
        {
            if (isServer)
            {
                StopServer();
            }
            else if (isClient)
            {
                DisconnectFromServer();
            }
        }
        
        /// <summary>
        /// Зупиняє сервер
        /// </summary>
        public void StopServer()
        {
            if (!isServer) return;
            
            // Відключаємо всіх клієнтів
            foreach (var player in connectedPlayers)
            {
                DisconnectPlayer(player.playerId);
            }
            
            connectedPlayers.Clear();
            isServer = false;
            
            OnServerStopped?.Invoke();
            Debug.Log("Server stopped");
        }
        
        /// <summary>
        /// Відключається від сервера
        /// </summary>
        public void DisconnectFromServer()
        {
            if (!isClient) return;
            
            isClient = false;
            Debug.Log("Disconnected from server");
        }
        
        /// <summary>
        /// Оновлює синхронізацію сервера
        /// </summary>
        private void UpdateServerSync()
        {
            if (Time.time - lastSyncTime >= syncInterval)
            {
                SynchronizeGameState();
                lastSyncTime = Time.time;
            }
        }
        
        /// <summary>
        /// Синхронізує стан гри
        /// </summary>
        private void SynchronizeGameState()
        {
            if (!isServer) return;
            
            if (syncPlayerPositions)
            {
                SyncPlayerPositions();
            }
            
            if (syncEnemyStates)
            {
                SyncEnemyStates();
            }
            
            if (syncWorldObjects)
            {
                SyncWorldObjects();
            }
        }
        
        /// <summary>
        /// Синхронізує позиції гравців
        /// </summary>
        private void SyncPlayerPositions()
        {
            foreach (var player in connectedPlayers)
            {
                if (player != null && player.playerObject != null)
                {
                    SendPlayerPositionUpdate(player);
                }
            }
        }
        
        /// <summary>
        /// Синхронізує стани ворогів
        /// </summary>
        private void SyncEnemyStates()
        {
            // Знаходимо всіх ворогів на сцені
            var enemies = FindObjectsOfType<MonoBehaviour>(); // В реальності це буде EnemyController
            
            foreach (var enemy in enemies)
            {
                if (enemy.name.Contains("Enemy"))
                {
                    SendEnemyStateUpdate(enemy);
                }
            }
        }
        
        /// <summary>
        /// Синхронізує об'єкти світу
        /// </summary>
        private void SyncWorldObjects()
        {
            // Синхронізація дверей, предметів, тощо
        }
        
        /// <summary>
        /// Відправляє оновлення позиції гравця
        /// </summary>
        private void SendPlayerPositionUpdate(ConnectedPlayer player)
        {
            if (player.playerObject == null) return;
            
            var position = player.playerObject.transform.position;
            var rotation = player.playerObject.transform.rotation;
            
            // В реальному проекті тут буде RPC або інший мережевий виклик
            BroadcastPlayerPosition(player.playerId, position, rotation);
        }
        
        /// <summary>
        /// Відправляє оновлення стану ворога
        /// </summary>
        private void SendEnemyStateUpdate(MonoBehaviour enemy)
        {
            // В реальному проекті тут буде синхронізація стану ворога
        }
        
        /// <summary>
        /// Транслює позицію гравця всім клієнтам
        /// </summary>
        private void BroadcastPlayerPosition(int playerId, Vector3 position, Quaternion rotation)
        {
            // Симуляція мережевого виклику
            foreach (var player in connectedPlayers)
            {
                if (player.playerId != playerId)
                {
                    // Відправити позицію іншим гравцям
                }
            }
        }
        
        /// <summary>
        /// Обробляє підключення нового гравця
        /// </summary>
        public void HandlePlayerConnection(int playerId, GameObject playerObject)
        {
            var newPlayer = new ConnectedPlayer
            {
                playerId = playerId,
                playerObject = playerObject,
                connectionTime = Time.time,
                isReady = false
            };
            
            connectedPlayers.Add(newPlayer);
            OnPlayerConnected?.Invoke(playerId);
            
            Debug.Log($"Player {playerId} connected. Total players: {connectedPlayers.Count}");
        }
        
        /// <summary>
        /// Обробляє відключення гравця
        /// </summary>
        public void HandlePlayerDisconnection(int playerId)
        {
            var player = connectedPlayers.Find(p => p.playerId == playerId);
            if (player != null)
            {
                connectedPlayers.Remove(player);
                OnPlayerDisconnected?.Invoke(playerId);
                
                Debug.Log($"Player {playerId} disconnected. Remaining players: {connectedPlayers.Count}");
            }
        }
        
        /// <summary>
        /// Відключає гравця
        /// </summary>
        public void DisconnectPlayer(int playerId)
        {
            HandlePlayerDisconnection(playerId);
        }
        
        /// <summary>
        /// Оновлює статус мережі
        /// </summary>
        private void UpdateNetworkStatus()
        {
            // Перевірка таймаутів, втрачених з'єднань, тощо
            CheckConnectionTimeouts();
        }
        
        /// <summary>
        /// Перевіряє таймаути з'єднань
        /// </summary>
        private void CheckConnectionTimeouts()
        {
            if (!isServer) return;
            
            var currentTime = Time.time;
            var playersToDisconnect = new List<int>();
            
            foreach (var player in connectedPlayers)
            {
                if (currentTime - player.lastPingTime > connectionTimeout)
                {
                    playersToDisconnect.Add(player.playerId);
                }
            }
            
            foreach (var playerId in playersToDisconnect)
            {
                Debug.LogWarning($"Player {playerId} timed out");
                DisconnectPlayer(playerId);
            }
        }
        
        // === Публічні методи ===
        
        public bool IsServer() => isServer;
        public bool IsClient() => isClient;
        public bool IsConnected() => isServer || isClient;
        public int GetConnectedPlayersCount() => connectedPlayers.Count;
        public List<ConnectedPlayer> GetConnectedPlayers() => new List<ConnectedPlayer>(connectedPlayers);
        
        public ConnectedPlayer GetPlayerById(int playerId)
        {
            return connectedPlayers.Find(p => p.playerId == playerId);
        }
        
        /// <summary>
        /// Відправляє повідомлення всім гравцям
        /// </summary>
        public void BroadcastMessage(string message, object data = null)
        {
            // В реальному проекті тут буде мережевий виклик
            Debug.Log($"Broadcasting: {message}");
        }
        
        /// <summary>
        /// Відправляє повідомлення конкретному гравцю
        /// </summary>
        public void SendMessageToPlayer(int playerId, string message, object data = null)
        {
            // В реальному проекті тут буде мережевий виклик
            Debug.Log($"Sending to player {playerId}: {message}");
        }
    }
    
    /// <summary>
    /// Інформація про підключеного гравця
    /// </summary>
    [System.Serializable]
    public class ConnectedPlayer
    {
        public int playerId;
        public GameObject playerObject;
        public float connectionTime;
        public float lastPingTime;
        public bool isReady;
        public string playerName;
        public PlayerRole role;
        
        public ConnectedPlayer()
        {
            lastPingTime = Time.time;
        }
    }
    
    /// <summary>
    /// Ролі гравців у кооперативному режимі
    /// </summary>
    public enum PlayerRole
    {
        Leader,     // Лідер команди
        Assault,    // Штурмовик
        Support,    // Підтримка
        Specialist, // Спеціаліст
        Medic       // Медик
    }
}