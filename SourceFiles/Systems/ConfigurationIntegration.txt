using UnityEngine;

/// <summary>
/// Інтеграційний компонент для застосування конфігурацій до існуючих систем гри.
/// Автоматично завантажує та застосовує налаштування при старті.
/// </summary>
public class ConfigurationIntegration : MonoBehaviour
{
    [Header("Configuration References")]
    [Tooltip("Менеджер конфігурацій")]
    public ConfigurationManager configurationManager;
    [Tooltip("ID конфігурації руху гравця")]
    public string playerMovementConfigId = "player_movement_default";
    [Tooltip("ID конфігурації здоров'я гравця")]
    public string playerHealthConfigId = "player_health_default";
    [Tooltip("ID конфігурації навичок гравця")]
    public string playerSkillsConfigId = "player_skills_default";
    [Tooltip("ID поточного набору зброї")]
    public string weaponSetConfigId = "weapon_set_balanced";

    [Header("Component References")]
    [Tooltip("Компонент руху гравця")]
    public PlayerMovement playerMovement;
    [Tooltip("Компонент здоров'я гравця")]
    public PlayerHealth playerHealth;
    [Tooltip("Компонент зброї")]
    public WeaponController weaponController;
    [Tooltip("Компонент перемикання зброї")]
    public WeaponSwitching weaponSwitching;

    [Header("Integration Settings")]
    [Tooltip("Автоматично застосовувати конфігурації при старті?")]
    public bool autoApplyOnStart = true;
    [Tooltip("Логувати застосування конфігурацій?")]
    public bool logConfigurationApplication = true;
    [Tooltip("Дозволити перезавантаження конфігурацій в runtime?")]
    public bool allowRuntimeReload = true;

    // Кешовані конфігурації
    private PlayerMovementConfiguration cachedMovementConfig;
    private PlayerHealthConfiguration cachedHealthConfig;
    private PlayerSkillsConfiguration cachedSkillsConfig;
    private WeaponSet cachedWeaponSet;

    void Start()
    {
        if (autoApplyOnStart)
        {
            InitializeConfigurations();
            ApplyAllConfigurations();
        }
    }

    /// <summary>
    /// Ініціалізує всі конфігурації
    /// </summary>
    public void InitializeConfigurations()
    {
        if (configurationManager == null)
        {
            configurationManager = ConfigurationManager.Instance;
        }

        if (configurationManager == null)
        {
            Debug.LogError("ConfigurationIntegration: ConfigurationManager не знайдено!");
            return;
        }

        configurationManager.Initialize();
        LoadConfigurations();
    }

    /// <summary>
    /// Завантажує всі необхідні конфігурації
    /// </summary>
    void LoadConfigurations()
    {
        // Завантажуємо конфігурації гравця
        cachedMovementConfig = configurationManager.GetConfiguration<PlayerMovementConfiguration>(playerMovementConfigId);
        cachedHealthConfig = configurationManager.GetConfiguration<PlayerHealthConfiguration>(playerHealthConfigId);
        cachedSkillsConfig = configurationManager.GetConfiguration<PlayerSkillsConfiguration>(playerSkillsConfigId);
        
        // Завантажуємо набір зброї
        cachedWeaponSet = configurationManager.GetConfiguration<WeaponSet>(weaponSetConfigId);

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Завантажено конфігурації - Movement: {cachedMovementConfig != null}, " +
                     $"Health: {cachedHealthConfig != null}, Skills: {cachedSkillsConfig != null}, WeaponSet: {cachedWeaponSet != null}");
        }
    }

    /// <summary>
    /// Застосовує всі конфігурації до компонентів
    /// </summary>
    public void ApplyAllConfigurations()
    {
        ApplyMovementConfiguration();
        ApplyHealthConfiguration();
        ApplySkillsConfiguration();
        ApplyWeaponSetConfiguration();
        ApplyGameSettings();
    }

    /// <summary>
    /// Застосовує конфігурацію руху до PlayerMovement
    /// </summary>
    public void ApplyMovementConfiguration()
    {
        if (cachedMovementConfig == null || playerMovement == null) return;

        // Застосовуємо базові параметри руху
        playerMovement.walkSpeed = cachedMovementConfig.walkSpeed;
        playerMovement.sprintSpeed = cachedMovementConfig.sprintSpeed;
        playerMovement.crouchSpeed = cachedMovementConfig.crouchSpeed;
        playerMovement.groundAcceleration = cachedMovementConfig.groundAcceleration;
        playerMovement.groundDeceleration = cachedMovementConfig.groundDeceleration;

        // Застосовуємо параметри повітряного руху
        playerMovement.airControl = cachedMovementConfig.airControl;
        playerMovement.airAcceleration = cachedMovementConfig.airAcceleration;
        playerMovement.fallMultiplier = cachedMovementConfig.fallMultiplier;
        playerMovement.lowJumpMultiplier = cachedMovementConfig.lowJumpMultiplier;

        // Застосовуємо параметри стрибків
        playerMovement.jumpForce = cachedMovementConfig.jumpForce;
        playerMovement.allowDoubleJump = cachedMovementConfig.allowDoubleJump;
        playerMovement.coyoteTime = cachedMovementConfig.coyoteTime;
        playerMovement.jumpBufferTime = cachedMovementConfig.jumpBufferTime;

        // Застосовуємо параметри стаміни
        playerMovement.maxStamina = cachedMovementConfig.maxStamina;
        playerMovement.staminaDrainRate = cachedMovementConfig.staminaDrainRate;
        playerMovement.staminaRegenRate = cachedMovementConfig.staminaRegenRate;
        playerMovement.staminaRegenDelay = cachedMovementConfig.staminaRegenDelay;
        playerMovement.minStaminaToSprint = cachedMovementConfig.minStaminaToSprint;

        // Застосовуємо просунуті параметри руху
        playerMovement.allowSlide = cachedMovementConfig.allowSlide;
        playerMovement.slideForce = cachedMovementConfig.slideForce;
        playerMovement.slideDuration = cachedMovementConfig.slideDuration;
        playerMovement.slideCooldown = cachedMovementConfig.slideCooldown;

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано конфігурацію руху '{cachedMovementConfig.displayName}'");
        }
    }

    /// <summary>
    /// Застосовує конфігурацію здоров'я до PlayerHealth
    /// </summary>
    public void ApplyHealthConfiguration()
    {
        if (cachedHealthConfig == null || playerHealth == null) return;

        // Застосовуємо базові параметри здоров'я
        playerHealth.maxHealth = cachedHealthConfig.maxHealth;
        playerHealth.currentHealth = cachedHealthConfig.startingHealth;

        // Застосовуємо параметри регенерації
        playerHealth.allowAutoRegeneration = cachedHealthConfig.allowAutoRegeneration;
        playerHealth.regenerationRate = cachedHealthConfig.regenerationRate;
        playerHealth.regenerationDelay = cachedHealthConfig.regenerationDelay;

        // Застосовуємо параметри опору урону
        playerHealth.invulnerabilityTime = cachedHealthConfig.invulnerabilityTime;

        // Застосовуємо параметри відродження
        playerHealth.respawnTime = cachedHealthConfig.respawnTime;

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано конфігурацію здоров'я '{cachedHealthConfig.displayName}'");
        }
    }

    /// <summary>
    /// Застосовує конфігурацію навичок до всіх систем
    /// </summary>
    public void ApplySkillsConfiguration()
    {
        if (cachedSkillsConfig == null) return;

        // Застосовуємо навички до руху
        if (cachedMovementConfig != null && playerMovement != null)
        {
            cachedSkillsConfig.ApplyToMovementConfig(cachedMovementConfig);
            ApplyMovementConfiguration(); // Повторно застосовуємо з бонусами
        }

        // Застосовуємо навички до здоров'я
        if (cachedHealthConfig != null && playerHealth != null)
        {
            cachedSkillsConfig.ApplyToHealthConfig(cachedHealthConfig);
            ApplyHealthConfiguration(); // Повторно застосовуємо з бонусами
        }

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано конфігурацію навичок '{cachedSkillsConfig.displayName}'");
        }
    }

    /// <summary>
    /// Застосовує конфігурацію набору зброї
    /// </summary>
    public void ApplyWeaponSetConfiguration()
    {
        if (cachedWeaponSet == null || weaponSwitching == null) return;

        // Отримуємо всі зброї з набору
        var weapons = cachedWeaponSet.GetAllWeapons();
        
        // Застосовуємо зброї до слотів (якщо є відповідні методи)
        for (int i = 0; i < weapons.Length && i < weaponSwitching.GetWeaponSlotCount(); i++)
        {
            if (weapons[i] != null)
            {
                // Тут можна додати логіку застосування конфігурації зброї
                ApplyWeaponConfiguration(weapons[i], i);
            }
        }

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано набір зброї '{cachedWeaponSet.displayName}' з {weapons.Length} зброями");
        }
    }

    /// <summary>
    /// Застосовує конфігурацію конкретної зброї
    /// </summary>
    void ApplyWeaponConfiguration(WeaponConfiguration weaponConfig, int slotIndex)
    {
        if (weaponController == null) return;

        // Застосовуємо навички до конфігурації зброї
        if (cachedSkillsConfig != null)
        {
            cachedSkillsConfig.ApplyToWeaponConfig(weaponConfig);
        }

        // Тут можна додати логіку застосування параметрів зброї до WeaponController
        // Наприклад, якщо WeaponController має методи для зміни параметрів
        
        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано конфігурацію зброї '{weaponConfig.displayName}' до слота {slotIndex}");
        }
    }

    /// <summary>
    /// Застосовує глобальні налаштування гри
    /// </summary>
    public void ApplyGameSettings()
    {
        var gameSettings = configurationManager?.gameSettings;
        if (gameSettings == null) return;

        // Застосовуємо налаштування фізики
        gameSettings.ApplyPhysicsSettings();
        
        // Застосовуємо налаштування графіки
        gameSettings.ApplyGraphicsSettings();
        
        // Застосовуємо налаштування складності
        gameSettings.ApplyDifficultySettings();

        if (logConfigurationApplication)
        {
            Debug.Log($"ConfigurationIntegration: Застосовано глобальні налаштування '{gameSettings.displayName}'");
        }
    }

    /// <summary>
    /// Перезавантажує та повторно застосовує всі конфігурації
    /// </summary>
    [ContextMenu("Reload All Configurations")]
    public void ReloadAllConfigurations()
    {
        if (!allowRuntimeReload)
        {
            Debug.LogWarning("ConfigurationIntegration: Runtime reload заборонено");
            return;
        }

        InitializeConfigurations();
        ApplyAllConfigurations();
        
        Debug.Log("ConfigurationIntegration: Всі конфігурації перезавантажено та застосовано");
    }

    /// <summary>
    /// Змінює конфігурацію руху
    /// </summary>
    public void ChangeMovementConfiguration(string newConfigId)
    {
        playerMovementConfigId = newConfigId;
        cachedMovementConfig = configurationManager.GetConfiguration<PlayerMovementConfiguration>(newConfigId);
        ApplyMovementConfiguration();
    }

    /// <summary>
    /// Змінює конфігурацію здоров'я
    /// </summary>
    public void ChangeHealthConfiguration(string newConfigId)
    {
        playerHealthConfigId = newConfigId;
        cachedHealthConfig = configurationManager.GetConfiguration<PlayerHealthConfiguration>(newConfigId);
        ApplyHealthConfiguration();
    }

    /// <summary>
    /// Змінює набір зброї
    /// </summary>
    public void ChangeWeaponSet(string newConfigId)
    {
        weaponSetConfigId = newConfigId;
        cachedWeaponSet = configurationManager.GetConfiguration<WeaponSet>(newConfigId);
        ApplyWeaponSetConfiguration();
    }

    /// <summary>
    /// Отримує поточні конфігурації
    /// </summary>
    public void GetCurrentConfigurations(out PlayerMovementConfiguration movement, 
                                        out PlayerHealthConfiguration health,
                                        out PlayerSkillsConfiguration skills,
                                        out WeaponSet weaponSet)
    {
        movement = cachedMovementConfig;
        health = cachedHealthConfig;
        skills = cachedSkillsConfig;
        weaponSet = cachedWeaponSet;
    }

    /// <summary>
    /// Перевіряє, чи всі конфігурації завантажені
    /// </summary>
    public bool AreAllConfigurationsLoaded()
    {
        return cachedMovementConfig != null && 
               cachedHealthConfig != null && 
               cachedSkillsConfig != null && 
               cachedWeaponSet != null;
    }

    /// <summary>
    /// Виводить статистику застосованих конфігурацій
    /// </summary>
    [ContextMenu("Print Configuration Stats")]
    public void PrintConfigurationStats()
    {
        Debug.Log("=== Configuration Integration Stats ===");
        Debug.Log($"Movement Config: {(cachedMovementConfig != null ? cachedMovementConfig.displayName : "None")}");
        Debug.Log($"Health Config: {(cachedHealthConfig != null ? cachedHealthConfig.displayName : "None")}");
        Debug.Log($"Skills Config: {(cachedSkillsConfig != null ? cachedSkillsConfig.displayName : "None")}");
        Debug.Log($"Weapon Set: {(cachedWeaponSet != null ? cachedWeaponSet.displayName : "None")}");
        Debug.Log($"All Loaded: {AreAllConfigurationsLoaded()}");
        
        if (configurationManager != null)
        {
            configurationManager.PrintConfigurationStats();
        }
    }

    void OnValidate()
    {
        // Автоматично знаходимо компоненти, якщо не призначені
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
        if (weaponController == null)
            weaponController = FindObjectOfType<WeaponController>();
        if (weaponSwitching == null)
            weaponSwitching = FindObjectOfType<WeaponSwitching>();
    }
}

/// <summary>
/// Утилітарний компонент для швидкого тестування різних конфігурацій
/// </summary>
public class ConfigurationTester : MonoBehaviour
{
    [Header("Test Configurations")]
    public PlayerMovementConfiguration[] testMovementConfigs;
    public PlayerHealthConfiguration[] testHealthConfigs;
    public WeaponSet[] testWeaponSets;

    [Header("Test Settings")]
    public ConfigurationIntegration configurationIntegration;
    public float testDuration = 5f;
    public bool autoSwitchConfigurations = false;

    private int currentConfigIndex = 0;
    private float lastSwitchTime = 0f;

    void Update()
    {
        if (autoSwitchConfigurations && Time.time - lastSwitchTime >= testDuration)
        {
            SwitchToNextConfiguration();
            lastSwitchTime = Time.time;
        }
    }

    /// <summary>
    /// Перемикає на наступну тестову конфігурацію
    /// </summary>
    [ContextMenu("Switch to Next Configuration")]
    public void SwitchToNextConfiguration()
    {
        if (configurationIntegration == null) return;

        currentConfigIndex = (currentConfigIndex + 1) % Mathf.Max(testMovementConfigs.Length, 
                                                                   testHealthConfigs.Length, 
                                                                   testWeaponSets.Length);

        if (currentConfigIndex < testMovementConfigs.Length && testMovementConfigs[currentConfigIndex] != null)
        {
            configurationIntegration.ChangeMovementConfiguration(testMovementConfigs[currentConfigIndex].configId);
        }

        if (currentConfigIndex < testHealthConfigs.Length && testHealthConfigs[currentConfigIndex] != null)
        {
            configurationIntegration.ChangeHealthConfiguration(testHealthConfigs[currentConfigIndex].configId);
        }

        if (currentConfigIndex < testWeaponSets.Length && testWeaponSets[currentConfigIndex] != null)
        {
            configurationIntegration.ChangeWeaponSet(testWeaponSets[currentConfigIndex].configId);
        }

        Debug.Log($"ConfigurationTester: Перемкнуто на конфігурацію #{currentConfigIndex}");
    }

    /// <summary>
    /// Тестує всі конфігурації послідовно
    /// </summary>
    [ContextMenu("Test All Configurations")]
    public void TestAllConfigurations()
    {
        StartCoroutine(TestAllConfigurationsCoroutine());
    }

    System.Collections.IEnumerator TestAllConfigurationsCoroutine()
    {
        int maxConfigs = Mathf.Max(testMovementConfigs.Length, testHealthConfigs.Length, testWeaponSets.Length);
        
        for (int i = 0; i < maxConfigs; i++)
        {
            currentConfigIndex = i;
            SwitchToNextConfiguration();
            
            Debug.Log($"ConfigurationTester: Тестування конфігурації #{i} протягом {testDuration} секунд");
            yield return new WaitForSeconds(testDuration);
        }
        
        Debug.Log("ConfigurationTester: Тестування всіх конфігурацій завершено");
    }
}