using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Інтеграція системи перків з існуючими системами гри
/// Продовження роботи попереднього агента - завершення системи перків
/// </summary>

// ================================
// ІНТЕГРАЦІЯ З СИСТЕМОЮ РУХУ
// ================================

public class PlayerMovementPerkIntegration : MonoBehaviour, IEventHandler<PerkEffectAppliedEvent>
{
    [Header("Base Values")]
    public float baseWalkSpeed = 5f;
    public float baseRunSpeed = 8f;
    public float baseStamina = 100f;

    // Модифікатори від перків
    private float movementSpeedMultiplier = 1f;
    private float staminaBonus = 0f;

    // Посилання на основний компонент
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        Events.Subscribe<PerkEffectAppliedEvent>(this);
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PerkEffectAppliedEvent>(this);
    }

    public void HandleEvent(PerkEffectAppliedEvent eventData)
    {
        switch (eventData.EffectType)
        {
            case "movement_speed":
                movementSpeedMultiplier += eventData.Value;
                ApplyMovementSpeedBonus();
                break;

            case "stamina_bonus":
                staminaBonus += eventData.Value;
                ApplyStaminaBonus();
                break;
        }
    }

    void ApplyMovementSpeedBonus()
    {
        if (playerMovement != null)
        {
            playerMovement.walkSpeed = baseWalkSpeed * movementSpeedMultiplier;
            playerMovement.runSpeed = baseRunSpeed * movementSpeedMultiplier;
        }
    }

    void ApplyStaminaBonus()
    {
        if (playerMovement != null)
        {
            playerMovement.maxStamina = baseStamina + staminaBonus;
        }
    }
}

// ================================
// ІНТЕГРАЦІЯ З СИСТЕМОЮ ЗБРОЇ
// ================================

public class WeaponPerkIntegration : MonoBehaviour, IEventHandler<PerkEffectAppliedEvent>
{
    [Header("Base Values")]
    public float baseDamageMultiplier = 1f;
    public float baseReloadSpeed = 1f;
    public float baseCriticalChance = 0f;

    // Модифікатори від перків
    private float damageMultiplier = 1f;
    private float reloadSpeedMultiplier = 1f;
    private float criticalChance = 0f;
    private bool berserkerModeActive = false;
    private float berserkerDamageBonus = 0f;

    // Посилання на компоненти
    private WeaponController weaponController;
    private PlayerHealth playerHealth;

    void Start()
    {
        weaponController = GetComponent<WeaponController>();
        playerHealth = GetComponent<PlayerHealth>();
        Events.Subscribe<PerkEffectAppliedEvent>(this);
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PerkEffectAppliedEvent>(this);
    }

    public void HandleEvent(PerkEffectAppliedEvent eventData)
    {
        switch (eventData.EffectType)
        {
            case "damage_multiplier":
                damageMultiplier += eventData.Value;
                break;

            case "reload_speed":
                reloadSpeedMultiplier += eventData.Value;
                ApplyReloadSpeedBonus();
                break;

            case "critical_chance":
                criticalChance += eventData.Value;
                break;

            case "berserker_mode":
                berserkerModeActive = true;
                berserkerDamageBonus = eventData.Value;
                break;
        }
    }

    void ApplyReloadSpeedBonus()
    {
        if (weaponController != null)
        {
            // Зменшуємо час перезарядки
            weaponController.reloadTime = weaponController.baseReloadTime / reloadSpeedMultiplier;
        }
    }

    /// <summary>
    /// Обчислює фінальний урон з урахуванням перків
    /// </summary>
    public float CalculateFinalDamage(float baseDamage)
    {
        float finalDamage = baseDamage * damageMultiplier;

        // Берсерк режим
        if (berserkerModeActive && playerHealth != null)
        {
            float healthPercentage = playerHealth.currentHealth / playerHealth.maxHealth;
            if (healthPercentage <= 0.25f)
            {
                finalDamage *= (1f + berserkerDamageBonus);
            }
        }

        // Критичний удар
        if (Random.Range(0f, 1f) <= criticalChance)
        {
            finalDamage *= 2f; // Подвійний урон
            
            // Показуємо критичний урон
            Events.Publish(new CriticalHitEvent(finalDamage));
        }

        return finalDamage;
    }
}

// ================================
// ІНТЕГРАЦІЯ З СИСТЕМОЮ ЗДОРОВ'Я
// ================================

public class PlayerHealthPerkIntegration : MonoBehaviour, IEventHandler<PerkEffectAppliedEvent>
{
    [Header("Base Values")]
    public float baseMaxHealth = 100f;

    // Модифікатори від перків
    private float healthBonus = 0f;
    private float healthRegenRate = 0f;
    private bool regenerationActive = false;

    // Посилання на компоненти
    private PlayerHealth playerHealth;

    // Таймер регенерації
    private float regenTimer = 0f;
    private const float REGEN_INTERVAL = 3f;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        Events.Subscribe<PerkEffectAppliedEvent>(this);
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PerkEffectAppliedEvent>(this);
    }

    void Update()
    {
        if (regenerationActive)
        {
            HandleHealthRegeneration();
        }
    }

    public void HandleEvent(PerkEffectAppliedEvent eventData)
    {
        switch (eventData.EffectType)
        {
            case "health_bonus":
                healthBonus += eventData.Value;
                ApplyHealthBonus();
                break;

            case "health_regen":
                healthRegenRate += eventData.Value;
                regenerationActive = true;
                break;
        }
    }

    void ApplyHealthBonus()
    {
        if (playerHealth != null)
        {
            float oldMaxHealth = playerHealth.maxHealth;
            playerHealth.maxHealth = baseMaxHealth + healthBonus;
            
            // Збільшуємо поточне здоров'я пропорційно
            float healthRatio = playerHealth.currentHealth / oldMaxHealth;
            playerHealth.currentHealth = playerHealth.maxHealth * healthRatio;
        }
    }

    void HandleHealthRegeneration()
    {
        regenTimer += Time.deltaTime;
        
        if (regenTimer >= REGEN_INTERVAL)
        {
            regenTimer = 0f;
            
            if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(healthRegenRate);
                
                // Візуальний ефект регенерації
                Events.Publish(new HealthRegenerationEvent(healthRegenRate));
            }
        }
    }
}

// ================================
// МЕНЕДЖЕР ІНТЕГРАЦІЇ ПЕРКІВ
// ================================

public class PerkIntegrationManager : MonoBehaviour
{
    [Header("Integration Components")]
    public PlayerMovementPerkIntegration movementIntegration;
    public WeaponPerkIntegration weaponIntegration;
    public PlayerHealthPerkIntegration healthIntegration;

    public static PerkIntegrationManager Instance { get; private set; }

    void Awake()
    {
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

    void Start()
    {
        InitializeIntegrations();
    }

    void InitializeIntegrations()
    {
        // Автоматично знаходимо компоненти інтеграції
        if (movementIntegration == null)
            movementIntegration = FindObjectOfType<PlayerMovementPerkIntegration>();

        if (weaponIntegration == null)
            weaponIntegration = FindObjectOfType<WeaponPerkIntegration>();

        if (healthIntegration == null)
            healthIntegration = FindObjectOfType<PlayerHealthPerkIntegration>();

        Debug.Log("PerkIntegrationManager: Ініціалізовано інтеграції перків");
    }

    /// <summary>
    /// Отримує фінальний урон з урахуванням всіх перків
    /// </summary>
    public float GetFinalDamage(float baseDamage)
    {
        if (weaponIntegration != null)
        {
            return weaponIntegration.CalculateFinalDamage(baseDamage);
        }
        return baseDamage;
    }
}

// ================================
// ДОДАТКОВІ ПОДІЇ ДЛЯ ІНТЕГРАЦІЇ
// ================================

public class CriticalHitEvent : GameEvent
{
    public float Damage { get; }

    public CriticalHitEvent(float damage)
    {
        Damage = damage;
    }
}

public class HealthRegenerationEvent : GameEvent
{
    public float Amount { get; }

    public HealthRegenerationEvent(float amount)
    {
        Amount = amount;
    }
}

// ================================
// РОЗШИРЕННЯ ІСНУЮЧИХ СИСТЕМ
// ================================

/// <summary>
/// Розширення для WeaponController для інтеграції з перками
/// ІНСТРУКЦІЯ: Додати ці методи до існуючого WeaponController.txt
/// </summary>
public static class WeaponControllerPerkExtensions
{
    [Header("Perk Integration")]
    public float baseReloadTime = 2f;
    
    // Модифікований метод стрільби з урахуванням перків
    public void FireWithPerks()
    {
        if (CanFire())
        {
            // Отримуємо базовий урон
            float baseDamage = currentWeapon.damage;
            
            // Застосовуємо модифікатори перків
            float finalDamage = PerkIntegrationManager.Instance?.GetFinalDamage(baseDamage) ?? baseDamage;
            
            // Виконуємо постріл з модифікованим уроном
            PerformShot(finalDamage);
        }
    }
    
    void PerformShot(float damage)
    {
        // Логіка пострілу з урахуванням урону від перків
        // ... існуючий код стрільби ...
    }
}

/// <summary>
/// Розширення для PlayerMovement для інтеграції з перками
/// ІНСТРУКЦІЯ: Додати ці поля до існуючого PlayerMovement.txt
/// </summary>
public static class PlayerMovementPerkExtensions
{
    [Header("Perk Integration")]
    public float baseWalkSpeed = 5f;
    public float baseRunSpeed = 8f;
    public float baseMaxStamina = 100f;
    
    // Ці значення будуть модифіковані перками
    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float runSpeed;
    [HideInInspector] public float maxStamina;
    
    void Start()
    {
        // Ініціалізуємо базові значення
        walkSpeed = baseWalkSpeed;
        runSpeed = baseRunSpeed;
        maxStamina = baseMaxStamina;
    }
}

/// <summary>
/// Розширення для PlayerHealth для інтеграції з перками
/// ІНСТРУКЦІЯ: Додати ці методи до існуючого PlayerHealth.txt
/// </summary>
public static class PlayerHealthPerkExtensions
{
    [Header("Perk Integration")]
    public float baseMaxHealth = 100f;
    
    // Ці значення будуть модифіковані перками
    [HideInInspector] public float maxHealth;
    
    void Start()
    {
        // Ініціалізуємо базові значення
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
    }
    
    /// <summary>
    /// Метод для лікування (використовується перками)
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Оновлюємо UI
        Events.Publish(new PlayerHealthChangedEvent(currentHealth, maxHealth));
    }
}