using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// КООПЕРАТИВНИЙ РЕЖИМ - COOPERATIVE MULTIPLAYER
/// Підтримка до 4 гравців в онлайн та локальному кооперативі
/// Включає синхронізацію, балансування складності та спільні цілі
/// </summary>

// ================================
// ТИПИ КООПЕРАТИВНИХ РЕЖИМІВ
// ================================

public enum CoopMode
{
    LocalSplitScreen,   // Локальний розділений екран
    OnlineCooperative,  // Онлайн кооператив
    LAN,                // Локальна мережа
    Hybrid              // Змішаний режим
}

public enum CoopDifficulty
{
    Casual,             // Легкий (для новачків)
    Normal,             // Нормальний
    Hardcore,           // Важкий
    Nightmare,          // Кошмарний
    Adaptive            // Адаптивний до команди
}

public enum PlayerRole
{
    Leader,             // Лідер команди
    Assault,            // Штурмовик
    Support,            // Підтримка
    Specialist,         // Спеціаліст
    Medic              // Медик
}

// ================================
// МЕРЕЖЕВИЙ МЕНЕДЖЕР КООПЕРАТИВУ
// ================================

public class CooperativeNetworkManager : NetworkManager
{
    [Header("Cooperative Settings")]
    public int maxPlayers = 4;
    public CoopMode currentMode = CoopMode.OnlineCooperative;
    public CoopDifficulty difficulty = CoopDifficulty.Normal;
    public bool allowDropInDropOut = true;
    public bool friendlyFire = false;
    
    [Header("Session Settings")]
    public string sessionName = "Coop Session";
    public string sessionPassword = "";
    public bool isPrivateSession = false;
    public float sessionTimeout = 300f; // 5 хвилин
    
    [Header("Synchronization")]
    public float syncInterval = 0.1f;
    public bool syncPlayerPositions = true;
    public bool syncEnemyStates = true;
    public bool syncWorldObjects = true;
    
    private List<CooperativePlayer> connectedPlayers = new List<CooperativePlayer>();
    private CooperativeGameState gameState;
    private float lastSyncTime = 0f;
    
    public static CooperativeNetworkManager Instance { get; private set; }
    
    public override void Awake()
    {
        base.Awake();
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void Start()
    {
        base.Start();
        InitializeCooperativeMode();
    }
    
    void InitializeCooperativeMode()
    {
        gameState = new CooperativeGameState();
        
        // Налаштування мережевих параметрів
        maxConnections = maxPlayers;
        networkPort = 7777;
        
        // Реєстрація prefab'ів
        RegisterCooperativePrefabs();
    }
    
    void RegisterCooperativePrefabs()
    {
        // Реєстрація мережевих об'єктів
        GameObject playerPrefab = Resources.Load<GameObject>("NetworkPlayer");
        if (playerPrefab != null)
        {
            playerPrefab.AddComponent<NetworkIdentity>();
            ClientScene.RegisterPrefab(playerPrefab);
        }
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if (connectedPlayers.Count >= maxPlayers)
        {
            Debug.LogWarning("Максимальна кількість гравців досягнута");
            return;
        }
        
        // Створення гравця
        GameObject playerObject = CreateCooperativePlayer(conn, playerControllerId);
        NetworkServer.AddPlayerForConnection(conn, playerObject, playerControllerId);
        
        // Синхронізація стану гри з новим гравцем
        SyncGameStateWithPlayer(conn);
    }
    
    GameObject CreateCooperativePlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject playerPrefab = Resources.Load<GameObject>("NetworkPlayer");
        Vector3 spawnPosition = GetPlayerSpawnPosition(connectedPlayers.Count);
        
        GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        CooperativePlayer coopPlayer = playerObject.GetComponent<CooperativePlayer>();
        if (coopPlayer == null)
        {
            coopPlayer = playerObject.AddComponent<CooperativePlayer>();
        }
        
        coopPlayer.playerId = connectedPlayers.Count;
        coopPlayer.playerName = $"Player {coopPlayer.playerId + 1}";
        coopPlayer.role = AssignPlayerRole(coopPlayer.playerId);
        
        connectedPlayers.Add(coopPlayer);
        
        return playerObject;
    }
    
    Vector3 GetPlayerSpawnPosition(int playerIndex)
    {
        Vector3[] spawnPositions = {
            new Vector3(0, 0, 0),
            new Vector3(2, 0, 0),
            new Vector3(-2, 0, 0),
            new Vector3(0, 0, 2)
        };
        
        return spawnPositions[playerIndex % spawnPositions.Length];
    }
    
    PlayerRole AssignPlayerRole(int playerIndex)
    {
        PlayerRole[] roles = { PlayerRole.Leader, PlayerRole.Assault, PlayerRole.Support, PlayerRole.Specialist };
        return roles[playerIndex % roles.Length];
    }
    
    void SyncGameStateWithPlayer(NetworkConnection conn)
    {
        // Відправка поточного стану гри новому гравцю
        RpcSyncGameState(gameState);
    }
    
    [ClientRpc]
    void RpcSyncGameState(CooperativeGameState state)
    {
        gameState = state;
        ApplyGameState();
    }
    
    void ApplyGameState()
    {
        // Застосування синхронізованого стану гри
        if (gameState != null)
        {
            LevelManager.Instance?.LoadLevel(gameState.currentLevel);
            // Синхронізація інших параметрів
        }
    }
    
    void Update()
    {
        if (isNetworkActive && Time.time - lastSyncTime >= syncInterval)
        {
            SynchronizeGameState();
            lastSyncTime = Time.time;
        }
    }
    
    void SynchronizeGameState()
    {
        if (!NetworkServer.active) return;
        
        // Синхронізація позицій гравців
        if (syncPlayerPositions)
        {
            SyncPlayerPositions();
        }
        
        // Синхронізація ворогів
        if (syncEnemyStates)
        {
            SyncEnemyStates();
        }
        
        // Синхронізація об'єктів світу
        if (syncWorldObjects)
        {
            SyncWorldObjects();
        }
    }
    
    void SyncPlayerPositions()
    {
        foreach (var player in connectedPlayers)
        {
            if (player != null)
            {
                RpcSyncPlayerPosition(player.playerId, player.transform.position, player.transform.rotation);
            }
        }
    }
    
    [ClientRpc]
    void RpcSyncPlayerPosition(int playerId, Vector3 position, Quaternion rotation)
    {
        CooperativePlayer player = GetPlayerById(playerId);
        if (player != null && !player.isLocalPlayer)
        {
            player.SyncPosition(position, rotation);
        }
    }
    
    void SyncEnemyStates()
    {
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        
        foreach (var enemy in enemies)
        {
            NetworkIdentity netId = enemy.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                RpcSyncEnemyState(netId.netId, enemy.currentState, enemy.health, enemy.transform.position);
            }
        }
    }
    
    [ClientRpc]
    void RpcSyncEnemyState(NetworkInstanceId enemyId, EnemyState state, float health, Vector3 position)
    {
        GameObject enemyObject = ClientScene.FindLocalObject(enemyId);
        if (enemyObject != null)
        {
            EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.currentState = state;
                enemy.health = health;
                enemy.transform.position = position;
            }
        }
    }
    
    void SyncWorldObjects()
    {
        // Синхронізація інтерактивних об'єктів, дверей, тощо
        InteractableObject[] objects = FindObjectsOfType<InteractableObject>();
        
        foreach (var obj in objects)
        {
            NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                RpcSyncWorldObject(netId.netId, obj.isActivated, obj.currentState);
            }
        }
    }
    
    [ClientRpc]
    void RpcSyncWorldObject(NetworkInstanceId objectId, bool isActivated, int state)
    {
        GameObject worldObject = ClientScene.FindLocalObject(objectId);
        if (worldObject != null)
        {
            InteractableObject interactable = worldObject.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.isActivated = isActivated;
                interactable.currentState = state;
            }
        }
    }
    
    CooperativePlayer GetPlayerById(int playerId)
    {
        return connectedPlayers.FirstOrDefault(p => p.playerId == playerId);
    }
    
    public void StartCooperativeSession()
    {
        switch (currentMode)
        {
            case CoopMode.OnlineCooperative:
                StartHost();
                break;
            case CoopMode.LAN:
                StartHost();
                break;
            case CoopMode.LocalSplitScreen:
                StartLocalCooperative();
                break;
        }
    }
    
    void StartLocalCooperative()
    {
        // Ініціалізація локального кооперативу
        LocalCooperativeManager localManager = gameObject.AddComponent<LocalCooperativeManager>();
        localManager.Initialize(maxPlayers);
    }
    
    public void JoinCooperativeSession(string serverAddress)
    {
        networkAddress = serverAddress;
        StartClient();
    }
    
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Підключено до кооперативної сесії");
    }
    
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Відключено від кооперативної сесії");
    }
    
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        
        // Видалення гравця зі списку
        CooperativePlayer disconnectedPlayer = connectedPlayers.FirstOrDefault(p => p.connectionToClient == conn);
        if (disconnectedPlayer != null)
        {
            connectedPlayers.Remove(disconnectedPlayer);
            RpcPlayerDisconnected(disconnectedPlayer.playerId);
        }
    }
    
    [ClientRpc]
    void RpcPlayerDisconnected(int playerId)
    {
        UIManager.Instance?.ShowNotification($"Гравець {playerId + 1} відключився", NotificationType.Warning);
    }
}

// ================================
// КООПЕРАТИВНИЙ ГРАВЕЦЬ
// ================================

public class CooperativePlayer : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar] public int playerId;
    [SyncVar] public string playerName;
    [SyncVar] public PlayerRole role;
    [SyncVar] public int playerLevel = 1;
    
    [Header("Player Stats")]
    [SyncVar] public float health = 100f;
    [SyncVar] public float maxHealth = 100f;
    [SyncVar] public int kills = 0;
    [SyncVar] public int deaths = 0;
    [SyncVar] public int assists = 0;
    
    [Header("Team Mechanics")]
    public float reviveTime = 5f;
    public float reviveRange = 3f;
    public bool isDown = false;
    public bool isDead = false;
    
    [Header("Communication")]
    public List<string> quickCommands = new List<string>();
    public GameObject pingMarker;
    
    private PlayerController playerController;
    private PlayerHealth playerHealth;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isReviving = false;
    private float reviveProgress = 0f;
    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            playerHealth.onDeath += OnPlayerDeath;
            playerHealth.onDamage += OnPlayerDamage;
        }
        
        InitializeRole();
        SetupQuickCommands();
    }
    
    void InitializeRole()
    {
        switch (role)
        {
            case PlayerRole.Leader:
                ApplyLeaderBonuses();
                break;
            case PlayerRole.Assault:
                ApplyAssaultBonuses();
                break;
            case PlayerRole.Support:
                ApplySupportBonuses();
                break;
            case PlayerRole.Medic:
                ApplyMedicBonuses();
                break;
        }
    }
    
    void ApplyLeaderBonuses()
    {
        // Лідер може позначати цілі та давати бонуси команді
        maxHealth *= 1.1f;
        health = maxHealth;
        
        // Додавання компонента лідерства
        TeamLeadership leadership = gameObject.AddComponent<TeamLeadership>();
        leadership.damageBonus = 0.1f;
        leadership.experienceBonus = 0.15f;
    }
    
    void ApplyAssaultBonuses()
    {
        // Штурмовик має бонуси до урону та швидкості
        if (playerController != null)
        {
            playerController.movementSpeed *= 1.15f;
        }
        
        WeaponController weaponController = GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.damageMultiplier *= 1.1f;
        }
    }
    
    void ApplySupportBonuses()
    {
        // Підтримка має бонуси до боєприпасів та перезарядки
        WeaponController weaponController = GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.maxAmmo = Mathf.RoundToInt(weaponController.maxAmmo * 1.3f);
            weaponController.reloadSpeed *= 1.2f;
        }
    }
    
    void ApplyMedicBonuses()
    {
        // Медик може лікувати інших гравців
        maxHealth *= 1.2f;
        health = maxHealth;
        
        MedicAbilities medic = gameObject.AddComponent<MedicAbilities>();
        medic.healingPower = 25f;
        medic.reviveSpeedBonus = 0.5f;
    }
    
    void SetupQuickCommands()
    {
        quickCommands.Add("Допоможіть!");
        quickCommands.Add("Ворог тут!");
        quickCommands.Add("Йдемо сюди!");
        quickCommands.Add("Перезаряджаюся!");
        quickCommands.Add("Потрібні боєприпаси!");
        quickCommands.Add("Лікую!");
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;
        
        HandleInput();
        HandleReviving();
        
        // Плавна синхронізація позиції для інших гравців
        if (!isLocalPlayer)
        {
            SmoothSync();
        }
    }
    
    void HandleInput()
    {
        // Швидкі команди
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowQuickCommandMenu();
        }
        
        // Пінг
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreatePing();
        }
        
        // Взаємодія з командою
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteractWithTeammate();
        }
    }
    
    void ShowQuickCommandMenu()
    {
        // Показати UI меню швидких команд
        QuickCommandUI.Instance?.Show(quickCommands, SendQuickCommand);
    }
    
    void SendQuickCommand(string command)
    {
        CmdSendQuickCommand(command);
    }
    
    [Command]
    void CmdSendQuickCommand(string command)
    {
        RpcReceiveQuickCommand(playerName, command);
    }
    
    [ClientRpc]
    void RpcReceiveQuickCommand(string senderName, string command)
    {
        UIManager.Instance?.ShowNotification($"{senderName}: {command}", NotificationType.Communication);
    }
    
    void CreatePing()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            CmdCreatePing(hit.point);
        }
    }
    
    [Command]
    void CmdCreatePing(Vector3 position)
    {
        RpcShowPing(position, playerName);
    }
    
    [ClientRpc]
    void RpcShowPing(Vector3 position, string senderName)
    {
        if (pingMarker != null)
        {
            GameObject ping = Instantiate(pingMarker, position, Quaternion.identity);
            
            PingMarker marker = ping.GetComponent<PingMarker>();
            if (marker != null)
            {
                marker.SetPing(senderName, 5f); // 5 секунд
            }
        }
    }
    
    void TryInteractWithTeammate()
    {
        Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position, reviveRange);
        
        foreach (var collider in nearbyPlayers)
        {
            CooperativePlayer teammate = collider.GetComponent<CooperativePlayer>();
            if (teammate != null && teammate != this && teammate.isDown)
            {
                StartReviving(teammate);
                break;
            }
        }
    }
    
    void StartReviving(CooperativePlayer teammate)
    {
        if (isReviving) return;
        
        isReviving = true;
        reviveProgress = 0f;
        
        StartCoroutine(ReviveTeammate(teammate));
    }
    
    IEnumerator ReviveTeammate(CooperativePlayer teammate)
    {
        float reviveSpeed = 1f / reviveTime;
        
        // Бонус швидкості для медика
        MedicAbilities medic = GetComponent<MedicAbilities>();
        if (medic != null)
        {
            reviveSpeed *= (1f + medic.reviveSpeedBonus);
        }
        
        while (reviveProgress < 1f && isReviving && teammate.isDown)
        {
            reviveProgress += reviveSpeed * Time.deltaTime;
            
            // Показати прогрес
            UIManager.Instance?.UpdateReviveProgress(reviveProgress);
            
            // Перевірка дистанції
            if (Vector3.Distance(transform.position, teammate.transform.position) > reviveRange)
            {
                break;
            }
            
            yield return null;
        }
        
        if (reviveProgress >= 1f && teammate.isDown)
        {
            CmdRevivePlayer(teammate.playerId);
        }
        
        isReviving = false;
        reviveProgress = 0f;
        UIManager.Instance?.HideReviveProgress();
    }
    
    [Command]
    void CmdRevivePlayer(int targetPlayerId)
    {
        RpcRevivePlayer(targetPlayerId);
    }
    
    [ClientRpc]
    void RpcRevivePlayer(int targetPlayerId)
    {
        CooperativePlayer target = CooperativeNetworkManager.Instance.GetPlayerById(targetPlayerId);
        if (target != null)
        {
            target.Revive();
        }
    }
    
    void Revive()
    {
        isDown = false;
        isDead = false;
        health = maxHealth * 0.3f; // Відновлення з 30% здоров'я
        
        playerController.enabled = true;
        
        UIManager.Instance?.ShowNotification("Вас підняли!", NotificationType.Success);
    }
    
    void HandleReviving()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            isReviving = false;
            reviveProgress = 0f;
        }
    }
    
    void SmoothSync()
    {
        // Плавна синхронізація позиції та ротації
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
    
    public void SyncPosition(Vector3 position, Quaternion rotation)
    {
        targetPosition = position;
        targetRotation = rotation;
    }
    
    void OnPlayerDeath()
    {
        isDown = true;
        playerController.enabled = false;
        
        CmdPlayerDown(playerId);
        
        // Запуск таймера bleeding out
        StartCoroutine(BleedingOutTimer());
    }
    
    [Command]
    void CmdPlayerDown(int downedPlayerId)
    {
        RpcPlayerDown(downedPlayerId);
    }
    
    [ClientRpc]
    void RpcPlayerDown(int downedPlayerId)
    {
        UIManager.Instance?.ShowNotification($"Гравець {downedPlayerId + 1} поранений!", NotificationType.Warning);
    }
    
    IEnumerator BleedingOutTimer()
    {
        float bleedOutTime = 30f; // 30 секунд до смерті
        float elapsed = 0f;
        
        while (elapsed < bleedOutTime && isDown && !isDead)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / bleedOutTime;
            
            UIManager.Instance?.UpdateBleedOutProgress(1f - progress);
            
            yield return null;
        }
        
        if (isDown && !isDead)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        isDown = false;
        deaths++;
        
        CmdPlayerDied(playerId);
    }
    
    [Command]
    void CmdPlayerDied(int deadPlayerId)
    {
        RpcPlayerDied(deadPlayerId);
    }
    
    [ClientRpc]
    void RpcPlayerDied(int deadPlayerId)
    {
        UIManager.Instance?.ShowNotification($"Гравець {deadPlayerId + 1} загинув!", NotificationType.Danger);
        
        // Перевірка на game over для всієї команди
        CheckTeamGameOver();
    }
    
    void CheckTeamGameOver()
    {
        CooperativePlayer[] allPlayers = FindObjectsOfType<CooperativePlayer>();
        bool anyAlive = allPlayers.Any(p => !p.isDead && !p.isDown);
        
        if (!anyAlive)
        {
            GameManager.Instance?.GameOver();
        }
    }
    
    void OnPlayerDamage(float damage, DamageType damageType)
    {
        // Синхронізація урону
        CmdSyncDamage(damage, damageType);
    }
    
    [Command]
    void CmdSyncDamage(float damage, DamageType damageType)
    {
        RpcSyncDamage(playerId, damage, damageType);
    }
    
    [ClientRpc]
    void RpcSyncDamage(int targetPlayerId, float damage, DamageType damageType)
    {
        // Синхронізація урону між клієнтами
        if (targetPlayerId != playerId) return;
        
        health -= damage;
        health = Mathf.Max(0f, health);
    }
}

// ================================
// ДОПОМІЖНІ КОМПОНЕНТИ
// ================================

public class TeamLeadership : MonoBehaviour
{
    public float damageBonus = 0.1f;
    public float experienceBonus = 0.15f;
    public float leadershipRadius = 10f;
    
    void Update()
    {
        ApplyLeadershipBonuses();
    }
    
    void ApplyLeadershipBonuses()
    {
        Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position, leadershipRadius);
        
        foreach (var collider in nearbyPlayers)
        {
            CooperativePlayer teammate = collider.GetComponent<CooperativePlayer>();
            if (teammate != null && teammate != GetComponent<CooperativePlayer>())
            {
                ApplyBonusToPlayer(teammate);
            }
        }
    }
    
    void ApplyBonusToPlayer(CooperativePlayer player)
    {
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.damageMultiplier = 1f + damageBonus;
        }
    }
}

public class MedicAbilities : MonoBehaviour
{
    public float healingPower = 25f;
    public float reviveSpeedBonus = 0.5f;
    public float healingRange = 5f;
    public float healingCooldown = 10f;
    
    private float lastHealTime = 0f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && CanHeal())
        {
            TryHealNearbyPlayers();
        }
    }
    
    bool CanHeal()
    {
        return Time.time - lastHealTime >= healingCooldown;
    }
    
    void TryHealNearbyPlayers()
    {
        Collider[] nearbyPlayers = Physics.OverlapSphere(transform.position, healingRange);
        
        foreach (var collider in nearbyPlayers)
        {
            CooperativePlayer teammate = collider.GetComponent<CooperativePlayer>();
            if (teammate != null && teammate.health < teammate.maxHealth)
            {
                HealPlayer(teammate);
            }
        }
        
        lastHealTime = Time.time;
    }
    
    void HealPlayer(CooperativePlayer player)
    {
        float healAmount = healingPower;
        player.health = Mathf.Min(player.maxHealth, player.health + healAmount);
        
        // Ефект лікування
        CreateHealingEffect(player.transform.position);
        
        UIManager.Instance?.ShowNotification($"Вилікувано {healAmount} HP", NotificationType.Healing);
    }
    
    void CreateHealingEffect(Vector3 position)
    {
        GameObject healEffect = new GameObject("Healing Effect");
        healEffect.transform.position = position;
        
        ParticleSystem particles = healEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.green;
        main.startLifetime = 2f;
        main.startSpeed = 5f;
        
        Destroy(healEffect, 3f);
    }
}

[System.Serializable]
public class CooperativeGameState
{
    public int currentLevel = 1;
    public float gameTime = 0f;
    public int totalKills = 0;
    public bool isGameActive = true;
    public CoopDifficulty difficulty = CoopDifficulty.Normal;
    
    public CooperativeGameState()
    {
        currentLevel = 1;
        gameTime = 0f;
        totalKills = 0;
        isGameActive = true;
    }
}

public class PingMarker : MonoBehaviour
{
    public string senderName;
    public float duration;
    private float startTime;
    
    public void SetPing(string sender, float time)
    {
        senderName = sender;
        duration = time;
        startTime = Time.time;
        
        // Візуальний ефект пінгу
        CreatePingEffect();
    }
    
    void Update()
    {
        if (Time.time - startTime >= duration)
        {
            Destroy(gameObject);
        }
    }
    
    void CreatePingEffect()
    {
        GameObject pingEffect = new GameObject("Ping Effect");
        pingEffect.transform.SetParent(transform);
        pingEffect.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = pingEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.yellow;
        main.startLifetime = duration;
        main.startSpeed = 3f;
        main.loop = true;
    }
}