using UnityEngine;

/// <summary>
/// Колекція всіх типів подій в грі. Кожна подія містить специфічні дані для своєї системи.
/// </summary>

// ================================
// ПОДІЇ ЗДОРОВ'Я ГРАВЦЯ
// ================================

/// <summary>
/// Подія зміни здоров'я гравця
/// </summary>
public class PlayerHealthChangedEvent : GameEvent
{
    public float CurrentHealth { get; }
    public float MaxHealth { get; }
    public float PreviousHealth { get; }
    public float HealthChange { get; }
    public bool IsDamage => HealthChange < 0;
    public bool IsHealing => HealthChange > 0;

    public PlayerHealthChangedEvent(float currentHealth, float maxHealth, float previousHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        PreviousHealth = previousHealth;
        HealthChange = currentHealth - previousHealth;
    }
}

/// <summary>
/// Подія смерті гравця
/// </summary>
public class PlayerDeathEvent : GameEvent
{
    public Vector3 DeathPosition { get; }
    public string CauseOfDeath { get; }

    public PlayerDeathEvent(Vector3 deathPosition, string causeOfDeath = "Unknown")
    {
        DeathPosition = deathPosition;
        CauseOfDeath = causeOfDeath;
    }
}

/// <summary>
/// Подія відродження гравця
/// </summary>
public class PlayerRespawnEvent : GameEvent
{
    public Vector3 RespawnPosition { get; }
    public float RespawnHealth { get; }

    public PlayerRespawnEvent(Vector3 respawnPosition, float respawnHealth)
    {
        RespawnPosition = respawnPosition;
        RespawnHealth = respawnHealth;
    }
}

// ================================
// ПОДІЇ ЗБРОЇ ТА БОЮ
// ================================

/// <summary>
/// Подія пострілу зброї
/// </summary>
public class WeaponFiredEvent : GameEvent
{
    public string WeaponName { get; }
    public Vector3 FirePosition { get; }
    public Vector3 FireDirection { get; }
    public int RemainingAmmo { get; }
    public GameObject BulletObject { get; }

    public WeaponFiredEvent(string weaponName, Vector3 firePosition, Vector3 fireDirection, int remainingAmmo, GameObject bulletObject = null)
    {
        WeaponName = weaponName;
        FirePosition = firePosition;
        FireDirection = fireDirection;
        RemainingAmmo = remainingAmmo;
        BulletObject = bulletObject;
    }
}

/// <summary>
/// Подія зміни кількості патронів
/// </summary>
public class AmmoChangedEvent : GameEvent
{
    public string WeaponName { get; }
    public int CurrentAmmo { get; }
    public int MaxAmmo { get; }
    public int ReloadCharges { get; }
    public bool IsEmpty => CurrentAmmo <= 0;

    public AmmoChangedEvent(string weaponName, int currentAmmo, int maxAmmo, int reloadCharges)
    {
        WeaponName = weaponName;
        CurrentAmmo = currentAmmo;
        MaxAmmo = maxAmmo;
        ReloadCharges = reloadCharges;
    }
}

/// <summary>
/// Подія початку перезарядки
/// </summary>
public class WeaponReloadStartedEvent : GameEvent
{
    public string WeaponName { get; }
    public float ReloadDuration { get; }
    public int ReloadChargesLeft { get; }

    public WeaponReloadStartedEvent(string weaponName, float reloadDuration, int reloadChargesLeft)
    {
        WeaponName = weaponName;
        ReloadDuration = reloadDuration;
        ReloadChargesLeft = reloadChargesLeft;
    }
}

/// <summary>
/// Подія завершення перезарядки
/// </summary>
public class WeaponReloadCompletedEvent : GameEvent
{
    public string WeaponName { get; }
    public int NewAmmoCount { get; }

    public WeaponReloadCompletedEvent(string weaponName, int newAmmoCount)
    {
        WeaponName = weaponName;
        NewAmmoCount = newAmmoCount;
    }
}

/// <summary>
/// Подія перемикання зброї
/// </summary>
public class WeaponSwitchedEvent : GameEvent
{
    public string PreviousWeapon { get; }
    public string NewWeapon { get; }
    public int WeaponSlotIndex { get; }

    public WeaponSwitchedEvent(string previousWeapon, string newWeapon, int weaponSlotIndex)
    {
        PreviousWeapon = previousWeapon;
        NewWeapon = newWeapon;
        WeaponSlotIndex = weaponSlotIndex;
    }
}

// ================================
// ПОДІЇ РУХУ ТА СТАМІНИ
// ================================

/// <summary>
/// Подія зміни стаміни
/// </summary>
public class StaminaChangedEvent : GameEvent
{
    public float CurrentStamina { get; }
    public float MaxStamina { get; }
    public float StaminaPercentage => CurrentStamina / MaxStamina;
    public bool IsExhausted => CurrentStamina <= 0;

    public StaminaChangedEvent(float currentStamina, float maxStamina)
    {
        CurrentStamina = currentStamina;
        MaxStamina = maxStamina;
    }
}

/// <summary>
/// Подія зміни стану руху гравця
/// </summary>
public class PlayerMovementStateChangedEvent : GameEvent
{
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        Sliding,
        Dashing,
        Jumping,
        Falling
    }

    public MovementState PreviousState { get; }
    public MovementState NewState { get; }
    public Vector3 PlayerPosition { get; }
    public Vector3 PlayerVelocity { get; }

    public PlayerMovementStateChangedEvent(MovementState previousState, MovementState newState, Vector3 playerPosition, Vector3 playerVelocity)
    {
        PreviousState = previousState;
        NewState = newState;
        PlayerPosition = playerPosition;
        PlayerVelocity = playerVelocity;
    }
}

// ================================
// ПОДІЇ ВЗАЄМОДІЇ
// ================================

/// <summary>
/// Подія взаємодії з об'єктом
/// </summary>
public class InteractionEvent : GameEvent
{
    public GameObject InteractedObject { get; }
    public GameObject Interactor { get; }
    public string InteractionType { get; }
    public bool WasSuccessful { get; }

    public InteractionEvent(GameObject interactedObject, GameObject interactor, string interactionType, bool wasSuccessful)
    {
        InteractedObject = interactedObject;
        Interactor = interactor;
        InteractionType = interactionType;
        WasSuccessful = wasSuccessful;
    }
}

/// <summary>
/// Подія підбирання предмета
/// </summary>
public class ItemPickedUpEvent : GameEvent
{
    public enum ItemType
    {
        Weapon,
        Ammo,
        Health,
        Key,
        Other
    }

    public ItemType Type { get; }
    public string ItemName { get; }
    public Vector3 PickupPosition { get; }
    public GameObject PickedUpObject { get; }

    public ItemPickedUpEvent(ItemType type, string itemName, Vector3 pickupPosition, GameObject pickedUpObject)
    {
        Type = type;
        ItemName = itemName;
        PickupPosition = pickupPosition;
        PickedUpObject = pickedUpObject;
    }
}

// ================================
// ПОДІЇ ПРОГРЕСІЇ ТА ЧЕКПОІНТІВ
// ================================

/// <summary>
/// Подія досягнення чекпоінта
/// </summary>
public class CheckpointReachedEvent : GameEvent
{
    public string CheckpointId { get; }
    public Vector3 CheckpointPosition { get; }
    public bool IsNewCheckpoint { get; }

    public CheckpointReachedEvent(string checkpointId, Vector3 checkpointPosition, bool isNewCheckpoint)
    {
        CheckpointId = checkpointId;
        CheckpointPosition = checkpointPosition;
        IsNewCheckpoint = isNewCheckpoint;
    }
}

/// <summary>
/// Подія збереження гри
/// </summary>
public class GameSavedEvent : GameEvent
{
    public string SaveSlot { get; }
    public bool WasSuccessful { get; }

    public GameSavedEvent(string saveSlot, bool wasSuccessful)
    {
        SaveSlot = saveSlot;
        WasSuccessful = wasSuccessful;
    }
}

// ================================
// ПОДІЇ UI ТА ЕФЕКТІВ
// ================================

/// <summary>
/// Подія для тряски камери
/// </summary>
public class CameraShakeEvent : GameEvent
{
    public float Duration { get; }
    public float Magnitude { get; }
    public string Reason { get; }

    public CameraShakeEvent(float duration, float magnitude, string reason = "")
    {
        Duration = duration;
        Magnitude = magnitude;
        Reason = reason;
    }
}

/// <summary>
/// Подія для відображення повідомлення
/// </summary>
public class ShowMessageEvent : GameEvent
{
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public string Message { get; }
    public MessageType Type { get; }
    public float Duration { get; }

    public ShowMessageEvent(string message, MessageType type = MessageType.Info, float duration = 3f)
    {
        Message = message;
        Type = type;
        Duration = duration;
    }
}

// ================================
// ПОДІЇ АУДІО
// ================================

/// <summary>
/// Подія відтворення звуку
/// </summary>
public class PlaySoundEvent : GameEvent
{
    public enum SoundType
    {
        SFX,
        Music,
        Voice,
        Ambient
    }

    public AudioClip AudioClip { get; }
    public SoundType Type { get; }
    public Vector3 Position { get; }
    public float Volume { get; }
    public bool Is3D { get; }

    public PlaySoundEvent(AudioClip audioClip, SoundType type, Vector3 position = default, float volume = 1f, bool is3D = false)
    {
        AudioClip = audioClip;
        Type = type;
        Position = position;
        Volume = volume;
        Is3D = is3D;
    }
}

// ================================
// ПОДІЇ НАЛАГОДЖЕННЯ
// ================================

/// <summary>
/// Подія для налагодження та логування
/// </summary>
public class DebugEvent : GameEvent
{
    public enum DebugLevel
    {
        Info,
        Warning,
        Error
    }

    public string Message { get; }
    public DebugLevel Level { get; }
    public string Category { get; }
    public UnityEngine.Object Context { get; }

    public DebugEvent(string message, DebugLevel level = DebugLevel.Info, string category = "General", UnityEngine.Object context = null)
    {
        Message = message;
        Level = level;
        Category = category;
        Context = context;
    }
}

// ================================
// ПОДІЇ СИСТЕМИ ПЕРКІВ
// ================================

/// <summary>
/// Подія підвищення рівня гравця
/// </summary>
public class PlayerLevelUpEvent : GameEvent
{
    public int NewLevel { get; }
    public int PerkPointsGained { get; }

    public PlayerLevelUpEvent(int newLevel, int perkPointsGained)
    {
        NewLevel = newLevel;
        PerkPointsGained = perkPointsGained;
    }
}

/// <summary>
/// Подія отримання досвіду
/// </summary>
public class ExperienceGainedEvent : GameEvent
{
    public int XPGained { get; }
    public int TotalXP { get; }
    public int XPToNextLevel { get; }

    public ExperienceGainedEvent(int xpGained, int totalXP, int xpToNextLevel)
    {
        XPGained = xpGained;
        TotalXP = totalXP;
        XPToNextLevel = xpToNextLevel;
    }
}

/// <summary>
/// Подія застосування ефекту перка
/// </summary>
public class PerkEffectAppliedEvent : GameEvent
{
    public string EffectType { get; }
    public float Value { get; }

    public PerkEffectAppliedEvent(string effectType, float value)
    {
        EffectType = effectType;
        Value = value;
    }
}

/// <summary>
/// Подія розблокування перка
/// </summary>
public class PerkUnlockedEvent : GameEvent
{
    public string PerkId { get; }
    public string PerkName { get; }
    public int NewRank { get; }

    public PerkUnlockedEvent(string perkId, string perkName, int newRank)
    {
        PerkId = perkId;
        PerkName = perkName;
        NewRank = newRank;
    }
}