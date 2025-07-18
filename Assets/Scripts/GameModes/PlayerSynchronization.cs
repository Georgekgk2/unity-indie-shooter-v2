using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.GameModes
{
    /// <summary>
    /// Система синхронізації гравців у кооперативному режимі.
    /// Обробляє синхронізацію позицій, дій та станів гравців.
    /// </summary>
    public class PlayerSynchronization : MonoBehaviour
    {
        [Header("Synchronization Settings")]
        [Tooltip("Інтерполяція позицій для плавності")]
        public bool useInterpolation = true;
        [Tooltip("Швидкість інтерполяції")]
        public float interpolationSpeed = 10f;
        [Tooltip("Поріг відстані для телепортації")]
        public float teleportThreshold = 5f;
        [Tooltip("Синхронізувати анімації")]
        public bool syncAnimations = true;
        
        // Посилання на основний контролер
        private CooperativeMode cooperativeMode;
        private NetworkManager networkManager;
        
        // Дані синхронізації
        private Dictionary<int, PlayerSyncData> playerSyncData = new Dictionary<int, PlayerSyncData>();
        
        public void Initialize(CooperativeMode coopMode)
        {
            cooperativeMode = coopMode;
            networkManager = coopMode.GetComponent<NetworkManager>();
        }
        
        void Update()
        {
            if (networkManager != null && networkManager.IsConnected())
            {
                UpdatePlayerSynchronization();
            }
        }
        
        /// <summary>
        /// Оновлює синхронізацію всіх гравців
        /// </summary>
        private void UpdatePlayerSynchronization()
        {
            foreach (var kvp in playerSyncData)
            {
                int playerId = kvp.Key;
                PlayerSyncData syncData = kvp.Value;
                
                if (syncData.playerObject != null)
                {
                    UpdatePlayerPosition(playerId, syncData);
                    UpdatePlayerAnimation(playerId, syncData);
                }
            }
        }
        
        /// <summary>
        /// Оновлює позицію гравця
        /// </summary>
        private void UpdatePlayerPosition(int playerId, PlayerSyncData syncData)
        {
            if (!useInterpolation)
            {
                syncData.playerObject.transform.position = syncData.targetPosition;
                syncData.playerObject.transform.rotation = syncData.targetRotation;
                return;
            }
            
            // Перевіряємо чи потрібна телепортація
            float distance = Vector3.Distance(syncData.playerObject.transform.position, syncData.targetPosition);
            if (distance > teleportThreshold)
            {
                syncData.playerObject.transform.position = syncData.targetPosition;
                syncData.playerObject.transform.rotation = syncData.targetRotation;
                return;
            }
            
            // Плавна інтерполяція
            syncData.playerObject.transform.position = Vector3.Lerp(
                syncData.playerObject.transform.position,
                syncData.targetPosition,
                interpolationSpeed * Time.deltaTime
            );
            
            syncData.playerObject.transform.rotation = Quaternion.Lerp(
                syncData.playerObject.transform.rotation,
                syncData.targetRotation,
                interpolationSpeed * Time.deltaTime
            );
        }
        
        /// <summary>
        /// Оновлює анімацію гравця
        /// </summary>
        private void UpdatePlayerAnimation(int playerId, PlayerSyncData syncData)
        {
            if (!syncAnimations || syncData.animator == null) return;
            
            // Синхронізуємо параметри анімації
            syncData.animator.SetFloat("Speed", syncData.animationSpeed);
            syncData.animator.SetBool("IsGrounded", syncData.isGrounded);
            syncData.animator.SetBool("IsJumping", syncData.isJumping);
            syncData.animator.SetBool("IsCrouching", syncData.isCrouching);
        }
        
        /// <summary>
        /// Реєструє гравця для синхронізації
        /// </summary>
        public void RegisterPlayer(int playerId, GameObject playerObject)
        {
            if (playerSyncData.ContainsKey(playerId))
            {
                Debug.LogWarning($"Player {playerId} already registered for synchronization");
                return;
            }
            
            var syncData = new PlayerSyncData
            {
                playerId = playerId,
                playerObject = playerObject,
                targetPosition = playerObject.transform.position,
                targetRotation = playerObject.transform.rotation,
                animator = playerObject.GetComponent<Animator>()
            };
            
            playerSyncData[playerId] = syncData;
            Debug.Log($"Player {playerId} registered for synchronization");
        }
        
        /// <summary>
        /// Видаляє гравця з синхронізації
        /// </summary>
        public void UnregisterPlayer(int playerId)
        {
            if (playerSyncData.ContainsKey(playerId))
            {
                playerSyncData.Remove(playerId);
                Debug.Log($"Player {playerId} unregistered from synchronization");
            }
        }
        
        /// <summary>
        /// Оновлює позицію гравця від мережі
        /// </summary>
        public void UpdatePlayerPositionFromNetwork(int playerId, Vector3 position, Quaternion rotation)
        {
            if (playerSyncData.ContainsKey(playerId))
            {
                var syncData = playerSyncData[playerId];
                syncData.targetPosition = position;
                syncData.targetRotation = rotation;
                syncData.lastUpdateTime = Time.time;
            }
        }
        
        /// <summary>
        /// Оновлює анімацію гравця від мережі
        /// </summary>
        public void UpdatePlayerAnimationFromNetwork(int playerId, float speed, bool isGrounded, bool isJumping, bool isCrouching)
        {
            if (playerSyncData.ContainsKey(playerId))
            {
                var syncData = playerSyncData[playerId];
                syncData.animationSpeed = speed;
                syncData.isGrounded = isGrounded;
                syncData.isJumping = isJumping;
                syncData.isCrouching = isCrouching;
            }
        }
        
        /// <summary>
        /// Відправляє позицію локального гравця
        /// </summary>
        public void SendLocalPlayerPosition(GameObject localPlayer)
        {
            if (networkManager == null || !networkManager.IsConnected()) return;
            
            var position = localPlayer.transform.position;
            var rotation = localPlayer.transform.rotation;
            
            // Отримуємо дані анімації
            var animator = localPlayer.GetComponent<Animator>();
            float speed = 0f;
            bool isGrounded = true;
            bool isJumping = false;
            bool isCrouching = false;
            
            if (animator != null)
            {
                speed = animator.GetFloat("Speed");
                isGrounded = animator.GetBool("IsGrounded");
                isJumping = animator.GetBool("IsJumping");
                isCrouching = animator.GetBool("IsCrouching");
            }
            
            // Відправляємо через NetworkManager
            networkManager.BroadcastMessage("PlayerPositionUpdate", new {
                position = position,
                rotation = rotation,
                speed = speed,
                isGrounded = isGrounded,
                isJumping = isJumping,
                isCrouching = isCrouching
            });
        }
        
        /// <summary>
        /// Синхронізує дію гравця
        /// </summary>
        public void SyncPlayerAction(int playerId, string actionName, object actionData = null)
        {
            if (networkManager == null) return;
            
            networkManager.BroadcastMessage("PlayerAction", new {
                playerId = playerId,
                actionName = actionName,
                actionData = actionData
            });
        }
        
        /// <summary>
        /// Обробляє дію гравця від мережі
        /// </summary>
        public void HandlePlayerActionFromNetwork(int playerId, string actionName, object actionData)
        {
            if (!playerSyncData.ContainsKey(playerId)) return;
            
            var syncData = playerSyncData[playerId];
            if (syncData.playerObject == null) return;
            
            // Обробляємо різні типи дій
            switch (actionName)
            {
                case "Shoot":
                    HandleShootAction(syncData, actionData);
                    break;
                    
                case "Reload":
                    HandleReloadAction(syncData, actionData);
                    break;
                    
                case "Jump":
                    HandleJumpAction(syncData, actionData);
                    break;
                    
                case "Interact":
                    HandleInteractAction(syncData, actionData);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown player action: {actionName}");
                    break;
            }
        }
        
        /// <summary>
        /// Обробляє дію стрільби
        /// </summary>
        private void HandleShootAction(PlayerSyncData syncData, object actionData)
        {
            // Відтворюємо ефект стрільби
            var weaponController = syncData.playerObject.GetComponent<MonoBehaviour>(); // WeaponController
            if (weaponController != null)
            {
                // Викликаємо метод стрільби без витрати патронів
            }
        }
        
        /// <summary>
        /// Обробляє дію перезарядки
        /// </summary>
        private void HandleReloadAction(PlayerSyncData syncData, object actionData)
        {
            // Відтворюємо анімацію перезарядки
            if (syncData.animator != null)
            {
                syncData.animator.SetTrigger("Reload");
            }
        }
        
        /// <summary>
        /// Обробляє дію стрибка
        /// </summary>
        private void HandleJumpAction(PlayerSyncData syncData, object actionData)
        {
            // Відтворюємо анімацію стрибка
            if (syncData.animator != null)
            {
                syncData.animator.SetTrigger("Jump");
            }
        }
        
        /// <summary>
        /// Обробляє дію взаємодії
        /// </summary>
        private void HandleInteractAction(PlayerSyncData syncData, object actionData)
        {
            // Обробляємо взаємодію з об'єктами
        }
        
        // === Публічні методи ===
        
        public bool IsPlayerRegistered(int playerId) => playerSyncData.ContainsKey(playerId);
        public int GetRegisteredPlayersCount() => playerSyncData.Count;
        
        public PlayerSyncData GetPlayerSyncData(int playerId)
        {
            return playerSyncData.ContainsKey(playerId) ? playerSyncData[playerId] : null;
        }
        
        void OnDestroy()
        {
            playerSyncData.Clear();
        }
    }
    
    /// <summary>
    /// Дані синхронізації гравця
    /// </summary>
    [System.Serializable]
    public class PlayerSyncData
    {
        public int playerId;
        public GameObject playerObject;
        public Animator animator;
        
        // Позиція та обертання
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        
        // Дані анімації
        public float animationSpeed;
        public bool isGrounded;
        public bool isJumping;
        public bool isCrouching;
        
        // Метадані
        public float lastUpdateTime;
        public bool isLocalPlayer;
        
        public PlayerSyncData()
        {
            lastUpdateTime = Time.time;
        }
    }
}