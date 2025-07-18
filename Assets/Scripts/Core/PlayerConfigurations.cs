using UnityEngine;

/// <summary>
/// Конфігурація руху гравця. Містить всі параметри для налаштування поведінки руху.
/// </summary>
[Configuration("Game/Player Movement Configuration", "Player")]
[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Game/Player/Movement Configuration")]
public class PlayerMovementConfiguration : BaseConfiguration
{
    [Header("Basic Movement")]
    [Tooltip("Швидкість ходьби")]
    [Range(1f, 20f)]
    public float walkSpeed = 5f;
    [Tooltip("Швидкість бігу")]
    [Range(5f, 30f)]
    public float sprintSpeed = 8f;
    [Tooltip("Швидкість присідання")]
    [Range(1f, 10f)]
    public float crouchSpeed = 2.5f;
    [Tooltip("Прискорення на землі")]
    [Range(5f, 50f)]
    public float groundAcceleration = 20f;
    [Tooltip("Гальмування на землі")]
    [Range(5f, 50f)]
    public float groundDeceleration = 25f;

    [Header("Air Movement")]
    [Tooltip("Контроль в повітрі (0-1)")]
    [Range(0f, 1f)]
    public float airControl = 0.3f;
    [Tooltip("Прискорення в повітрі")]
    [Range(1f, 20f)]
    public float airAcceleration = 5f;
    [Tooltip("Множник падіння")]
    [Range(1f, 5f)]
    public float fallMultiplier = 2.5f;
    [Tooltip("Множник низького стрибка")]
    [Range(1f, 5f)]
    public float lowJumpMultiplier = 2f;

    [Header("Jumping")]
    [Tooltip("Сила стрибка")]
    [Range(5f, 30f)]
    public float jumpForce = 12f;
    [Tooltip("Дозволити подвійний стрибок?")]
    public bool allowDoubleJump = false;
    [Tooltip("Час койота (секунди)")]
    [Range(0f, 0.5f)]
    public float coyoteTime = 0.1f;
    [Tooltip("Час буферу стрибка (секунди)")]
    [Range(0f, 0.5f)]
    public float jumpBufferTime = 0.1f;

    [Header("Stamina System")]
    [Tooltip("Максимальна стаміна")]
    [Range(50f, 200f)]
    public float maxStamina = 100f;
    [Tooltip("Швидкість витрати стаміни")]
    [Range(5f, 50f)]
    public float staminaDrainRate = 20f;
    [Tooltip("Швидкість відновлення стаміни")]
    [Range(10f, 100f)]
    public float staminaRegenRate = 30f;
    [Tooltip("Затримка відновлення стаміни")]
    [Range(0.5f, 5f)]
    public float staminaRegenDelay = 1f;
    [Tooltip("Мінімальна стаміна для бігу")]
    [Range(5f, 50f)]
    public float minStaminaToSprint = 10f;

    [Header("Advanced Movement")]
    [Tooltip("Дозволити ковзання?")]
    public bool allowSlide = true;
    [Tooltip("Сила ковзання")]
    [Range(5f, 30f)]
    public float slideForce = 15f;
    [Tooltip("Тривалість ковзання")]
    [Range(0.5f, 3f)]
    public float slideDuration = 1.5f;
    [Tooltip("Кулдаун ковзання")]
    [Range(1f, 10f)]
    public float slideCooldown = 3f;

    [Tooltip("Дозволити ривок?")]
    public bool allowDash = false;
    [Tooltip("Сила ривка")]
    [Range(10f, 50f)]
    public float dashForce = 25f;
    [Tooltip("Тривалість ривка")]
    [Range(0.1f, 1f)]
    public float dashDuration = 0.3f;
    [Tooltip("Кулдаун ривка")]
    [Range(2f, 15f)]
    public float dashCooldown = 5f;

    [Header("Collision Settings")]
    [Tooltip("Висота колайдера при присіданні")]
    [Range(0.5f, 2f)]
    public float crouchColliderHeight = 1f;
    [Tooltip("Висота колайдера при ковзанні")]
    [Range(0.3f, 1.5f)]
    public float slideColliderHeight = 0.8f;
    [Tooltip("Висота колайдера при ривку")]
    [Range(0.5f, 2f)]
    public float dashColliderHeight = 1.5f;

    [Header("Camera Effects")]
    [Tooltip("Амплітуда головокружіння")]
    [Range(0f, 0.2f)]
    public float headbobAmplitude = 0.05f;
    [Tooltip("Частота головокружіння")]
    [Range(5f, 20f)]
    public float headbobFrequency = 10f;
    [Tooltip("FOV при бігу")]
    [Range(60f, 120f)]
    public float sprintFOV = 75f;

    [Header("Audio Settings")]
    [Tooltip("Інтервал кроків при ходьбі")]
    [Range(0.2f, 1f)]
    public float walkFootstepInterval = 0.5f;
    [Tooltip("Інтервал кроків при бігу")]
    [Range(0.1f, 0.5f)]
    public float sprintFootstepInterval = 0.3f;
    [Tooltip("Інтервал кроків при присіданні")]
    [Range(0.3f, 1.5f)]
    public float crouchFootstepInterval = 0.7f;

    protected override void ValidateConfiguration()
    {
        // Валідація швидкостей
        walkSpeed = Mathf.Max(1f, walkSpeed);
        sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
        crouchSpeed = Mathf.Max(1f, crouchSpeed);
        
        // Валідація прискорень
        groundAcceleration = Mathf.Max(5f, groundAcceleration);
        groundDeceleration = Mathf.Max(5f, groundDeceleration);
        airAcceleration = Mathf.Max(1f, airAcceleration);
        
        // Валідація стрибків
        jumpForce = Mathf.Max(5f, jumpForce);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        
        // Валідація стаміни
        maxStamina = Mathf.Max(10f, maxStamina);
        staminaDrainRate = Mathf.Max(1f, staminaDrainRate);
        staminaRegenRate = Mathf.Max(1f, staminaRegenRate);
        minStaminaToSprint = Mathf.Min(maxStamina * 0.5f, minStaminaToSprint);
        
        // Валідація просунутого руху
        slideForce = Mathf.Max(5f, slideForce);
        slideDuration = Mathf.Max(0.1f, slideDuration);
        slideCooldown = Mathf.Max(0f, slideCooldown);
        dashForce = Mathf.Max(10f, dashForce);
        dashDuration = Mathf.Max(0.1f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);
        
        // Валідація колайдерів
        crouchColliderHeight = Mathf.Max(0.5f, crouchColliderHeight);
        slideColliderHeight = Mathf.Max(0.3f, slideColliderHeight);
        dashColliderHeight = Mathf.Max(0.5f, dashColliderHeight);
        
        // Валідація аудіо
        walkFootstepInterval = Mathf.Max(0.1f, walkFootstepInterval);
        sprintFootstepInterval = Mathf.Max(0.1f, sprintFootstepInterval);
        crouchFootstepInterval = Mathf.Max(0.1f, crouchFootstepInterval);

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Player Movement Config";
        }
    }

    /// <summary>
    /// Розраховує максимальну горизонтальну швидкість
    /// </summary>
    public float GetMaxHorizontalSpeed()
    {
        return Mathf.Max(walkSpeed, sprintSpeed, crouchSpeed);
    }

    /// <summary>
    /// Розраховує час виснаження стаміни при бігу
    /// </summary>
    public float GetStaminaExhaustionTime()
    {
        return maxStamina / staminaDrainRate;
    }

    /// <summary>
    /// Розраховує час повного відновлення стаміни
    /// </summary>
    public float GetStaminaRecoveryTime()
    {
        return staminaRegenDelay + (maxStamina / staminaRegenRate);
    }
}

/// <summary>
/// Конфігурація здоров'я та виживання гравця
/// </summary>
[Configuration("Game/Player Health Configuration", "Player")]
[CreateAssetMenu(fileName = "PlayerHealthConfig", menuName = "Game/Player/Health Configuration")]
public class PlayerHealthConfiguration : BaseConfiguration
{
    [Header("Health Settings")]
    [Tooltip("Максимальне здоров'я")]
    [Range(50f, 500f)]
    public float maxHealth = 100f;
    [Tooltip("Початкове здоров'я")]
    [Range(1f, 500f)]
    public float startingHealth = 100f;
    [Tooltip("Мінімальне здоров'я (не може бути менше)")]
    [Range(0f, 50f)]
    public float minHealth = 0f;

    [Header("Regeneration")]
    [Tooltip("Дозволити автоматичну регенерацію?")]
    public bool allowAutoRegeneration = false;
    [Tooltip("Швидкість регенерації (HP/сек)")]
    [Range(1f, 50f)]
    public float regenerationRate = 5f;
    [Tooltip("Затримка регенерації після урону")]
    [Range(1f, 10f)]
    public float regenerationDelay = 3f;
    [Tooltip("Максимальне здоров'я для регенерації")]
    [Range(10f, 500f)]
    public float maxRegenerationHealth = 75f;

    [Header("Damage Resistance")]
    [Tooltip("Час невразливості після урону")]
    [Range(0f, 3f)]
    public float invulnerabilityTime = 0.5f;
    [Tooltip("Опір урону (0-1)")]
    [Range(0f, 0.9f)]
    public float damageResistance = 0f;
    [Tooltip("Множник урону в голову")]
    [Range(1f, 5f)]
    public float headshotDamageMultiplier = 2f;
    [Tooltip("Множник урону в спину")]
    [Range(1f, 3f)]
    public float backstabDamageMultiplier = 1.5f;

    [Header("Death and Respawn")]
    [Tooltip("Час до автоматичного відродження")]
    [Range(1f, 30f)]
    public float respawnTime = 5f;
    [Tooltip("Дозволити миттєве відродження?")]
    public bool allowInstantRespawn = false;
    [Tooltip("Втрачати здоров'я при відродженні?")]
    public bool loseHealthOnRespawn = false;
    [Tooltip("Відсоток здоров'я при відродженні")]
    [Range(0.1f, 1f)]
    public float respawnHealthPercentage = 1f;

    [Header("Special States")]
    [Tooltip("Поріг критичного здоров'я")]
    [Range(0.1f, 0.5f)]
    public float criticalHealthThreshold = 0.25f;
    [Tooltip("Ефекти при критичному здоров'ї")]
    public bool enableCriticalHealthEffects = true;
    [Tooltip("Швидкість серцебиття при критичному здоров'ї")]
    [Range(0.5f, 3f)]
    public float criticalHeartbeatRate = 1.5f;

    protected override void ValidateConfiguration()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        startingHealth = Mathf.Clamp(startingHealth, 1f, maxHealth);
        minHealth = Mathf.Max(0f, minHealth);
        
        regenerationRate = Mathf.Max(0.1f, regenerationRate);
        regenerationDelay = Mathf.Max(0f, regenerationDelay);
        maxRegenerationHealth = Mathf.Clamp(maxRegenerationHealth, 1f, maxHealth);
        
        invulnerabilityTime = Mathf.Max(0f, invulnerabilityTime);
        damageResistance = Mathf.Clamp01(damageResistance);
        headshotDamageMultiplier = Mathf.Max(1f, headshotDamageMultiplier);
        backstabDamageMultiplier = Mathf.Max(1f, backstabDamageMultiplier);
        
        respawnTime = Mathf.Max(0.1f, respawnTime);
        respawnHealthPercentage = Mathf.Clamp01(respawnHealthPercentage);
        
        criticalHealthThreshold = Mathf.Clamp(criticalHealthThreshold, 0.01f, 0.9f);
        criticalHeartbeatRate = Mathf.Max(0.1f, criticalHeartbeatRate);

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Player Health Config";
        }
    }

    /// <summary>
    /// Розраховує ефективне здоров'я з урахуванням опору
    /// </summary>
    public float GetEffectiveHealth()
    {
        return maxHealth / (1f - damageResistance);
    }

    /// <summary>
    /// Розраховує час повного відновлення здоров'я
    /// </summary>
    public float GetFullRecoveryTime()
    {
        if (!allowAutoRegeneration) return float.MaxValue;
        
        float healthToRegenerate = Mathf.Min(maxRegenerationHealth, maxHealth) - minHealth;
        return regenerationDelay + (healthToRegenerate / regenerationRate);
    }

    /// <summary>
    /// Перевіряє, чи здоров'я критичне
    /// </summary>
    public bool IsHealthCritical(float currentHealth)
    {
        return (currentHealth / maxHealth) <= criticalHealthThreshold;
    }
}

/// <summary>
/// Конфігурація навичок та прогресії гравця
/// </summary>
[Configuration("Game/Player Skills Configuration", "Player")]
[CreateAssetMenu(fileName = "PlayerSkillsConfig", menuName = "Game/Player/Skills Configuration")]
public class PlayerSkillsConfiguration : BaseConfiguration
{
    [Header("Skill Categories")]
    [Tooltip("Навички руху")]
    public MovementSkills movementSkills;
    [Tooltip("Навички бою")]
    public CombatSkills combatSkills;
    [Tooltip("Навички виживання")]
    public SurvivalSkills survivalSkills;

    [System.Serializable]
    public class MovementSkills
    {
        [Header("Movement Bonuses")]
        [Tooltip("Бонус до швидкості ходьби")]
        [Range(0f, 2f)]
        public float walkSpeedMultiplier = 1f;
        [Tooltip("Бонус до швидкості бігу")]
        [Range(0f, 2f)]
        public float sprintSpeedMultiplier = 1f;
        [Tooltip("Бонус до стаміни")]
        [Range(0f, 2f)]
        public float staminaMultiplier = 1f;
        [Tooltip("Зменшення витрати стаміни")]
        [Range(0.5f, 1f)]
        public float staminaEfficiency = 1f;
        [Tooltip("Бонус до висоти стрибка")]
        [Range(0f, 2f)]
        public float jumpHeightMultiplier = 1f;
    }

    [System.Serializable]
    public class CombatSkills
    {
        [Header("Combat Bonuses")]
        [Tooltip("Бонус до урону")]
        [Range(0f, 2f)]
        public float damageMultiplier = 1f;
        [Tooltip("Бонус до точності")]
        [Range(0f, 2f)]
        public float accuracyMultiplier = 1f;
        [Tooltip("Бонус до швидкості перезарядки")]
        [Range(0.5f, 2f)]
        public float reloadSpeedMultiplier = 1f;
        [Tooltip("Зменшення віддачі")]
        [Range(0.5f, 1f)]
        public float recoilReduction = 1f;
        [Tooltip("Бонус до критичного урону")]
        [Range(0f, 3f)]
        public float criticalDamageMultiplier = 1f;
    }

    [System.Serializable]
    public class SurvivalSkills
    {
        [Header("Survival Bonuses")]
        [Tooltip("Бонус до максимального здоров'я")]
        [Range(0f, 2f)]
        public float healthMultiplier = 1f;
        [Tooltip("Бонус до регенерації")]
        [Range(0f, 3f)]
        public float regenerationMultiplier = 1f;
        [Tooltip("Опір урону")]
        [Range(0f, 0.5f)]
        public float damageResistance = 0f;
        [Tooltip("Швидкість відновлення після урону")]
        [Range(0.5f, 2f)]
        public float recoverySpeedMultiplier = 1f;
    }

    protected override void ValidateConfiguration()
    {
        // Валідація навичок руху
        movementSkills.walkSpeedMultiplier = Mathf.Max(0.1f, movementSkills.walkSpeedMultiplier);
        movementSkills.sprintSpeedMultiplier = Mathf.Max(0.1f, movementSkills.sprintSpeedMultiplier);
        movementSkills.staminaMultiplier = Mathf.Max(0.1f, movementSkills.staminaMultiplier);
        movementSkills.staminaEfficiency = Mathf.Clamp(movementSkills.staminaEfficiency, 0.1f, 1f);
        movementSkills.jumpHeightMultiplier = Mathf.Max(0.1f, movementSkills.jumpHeightMultiplier);

        // Валідація навичок бою
        combatSkills.damageMultiplier = Mathf.Max(0.1f, combatSkills.damageMultiplier);
        combatSkills.accuracyMultiplier = Mathf.Max(0.1f, combatSkills.accuracyMultiplier);
        combatSkills.reloadSpeedMultiplier = Mathf.Max(0.1f, combatSkills.reloadSpeedMultiplier);
        combatSkills.recoilReduction = Mathf.Clamp(combatSkills.recoilReduction, 0.1f, 1f);
        combatSkills.criticalDamageMultiplier = Mathf.Max(0.1f, combatSkills.criticalDamageMultiplier);

        // Валідація навичок виживання
        survivalSkills.healthMultiplier = Mathf.Max(0.1f, survivalSkills.healthMultiplier);
        survivalSkills.regenerationMultiplier = Mathf.Max(0.1f, survivalSkills.regenerationMultiplier);
        survivalSkills.damageResistance = Mathf.Clamp01(survivalSkills.damageResistance);
        survivalSkills.recoverySpeedMultiplier = Mathf.Max(0.1f, survivalSkills.recoverySpeedMultiplier);

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = "Player Skills Config";
        }
    }

    /// <summary>
    /// Застосовує навички до конфігурації руху
    /// </summary>
    public void ApplyToMovementConfig(PlayerMovementConfiguration movementConfig)
    {
        movementConfig.walkSpeed *= movementSkills.walkSpeedMultiplier;
        movementConfig.sprintSpeed *= movementSkills.sprintSpeedMultiplier;
        movementConfig.maxStamina *= movementSkills.staminaMultiplier;
        movementConfig.staminaDrainRate *= movementSkills.staminaEfficiency;
        movementConfig.jumpForce *= movementSkills.jumpHeightMultiplier;
    }

    /// <summary>
    /// Застосовує навички до конфігурації здоров'я
    /// </summary>
    public void ApplyToHealthConfig(PlayerHealthConfiguration healthConfig)
    {
        healthConfig.maxHealth *= survivalSkills.healthMultiplier;
        healthConfig.regenerationRate *= survivalSkills.regenerationMultiplier;
        healthConfig.damageResistance += survivalSkills.damageResistance;
        healthConfig.regenerationDelay /= survivalSkills.recoverySpeedMultiplier;
    }

    /// <summary>
    /// Застосовує навички до конфігурації зброї
    /// </summary>
    public void ApplyToWeaponConfig(WeaponConfiguration weaponConfig)
    {
        weaponConfig.damage *= combatSkills.damageMultiplier;
        weaponConfig.bulletSpread /= combatSkills.accuracyMultiplier;
        weaponConfig.reloadTime /= combatSkills.reloadSpeedMultiplier;
        weaponConfig.recoilAmount *= combatSkills.recoilReduction;
        weaponConfig.headshotMultiplier *= combatSkills.criticalDamageMultiplier;
    }
}