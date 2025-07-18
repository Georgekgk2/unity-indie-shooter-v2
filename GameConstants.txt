using UnityEngine;

/// <summary>
/// Центральне сховище для всіх констант гри. Допомагає уникнути магічних чисел та полегшує налаштування.
/// </summary>
public static class GameConstants
{
    // === ТЕГИ ===
    public const string PLAYER_TAG = "Player";
    public const string ENEMY_TAG = "Enemy";
    public const string GROUND_TAG = "Ground";
    public const string MAIN_CAMERA_TAG = "MainCamera";

    // === РУХИ ГРАВЦЯ ===
    public const float MIN_FOOTSTEP_SPEED = 0.1f;
    public const float DEFAULT_GRAVITY_MULTIPLIER = 2.5f;
    public const float LOW_JUMP_MULTIPLIER = 2f;
    public const float MIN_SPEED_TO_DRAIN_STAMINA = 0.1f;

    // === ЗБРОЯ ===
    public const float DEFAULT_BULLET_LIFETIME = 3f;
    public const float DEFAULT_MAX_AIM_DISTANCE = 500f;
    public const int DEFAULT_MAGAZINE_SIZE = 30;
    public const float DEFAULT_FIRE_RATE = 8f;
    public const float DEFAULT_RELOAD_TIME = 2f;

    // === UI ===
    public const float SCREEN_CENTER_X = 0.5f;
    public const float SCREEN_CENTER_Y = 0.5f;
    public const float DEFAULT_FLASH_DURATION = 0.25f;
    public const float DEFAULT_SHAKE_DURATION = 0.2f;

    // === ФІЗИКА ===
    public const float PHYSICS_EPSILON = 0.01f;
    public const float ANGLE_NORMALIZATION_THRESHOLD = 180f;
    public const float DEFAULT_HIT_EFFECT_LIFETIME = 1f;

    // === АУДІО ===
    public const float DEFAULT_FOOTSTEP_WALK_INTERVAL = 0.5f;
    public const float DEFAULT_FOOTSTEP_SPRINT_INTERVAL = 0.3f;
    public const float DEFAULT_FOOTSTEP_CROUCH_INTERVAL = 0.7f;

    // === КАМЕРА ===
    public const float DEFAULT_MOUSE_SENSITIVITY = 100f;
    public const float MIN_CAMERA_X_ROTATION = -90f;
    public const float MAX_CAMERA_X_ROTATION = 90f;
    public const float DEFAULT_ROTATION_SMOOTHNESS = 0.1f;

    // === ЗДОРОВ'я ===
    public const float DEFAULT_MAX_HEALTH = 100f;
    public const float DEFAULT_REGEN_RATE = 5f;
    public const float DEFAULT_REGEN_DELAY = 3f;
    public const float DEFAULT_INVULNERABILITY_TIME = 0.5f;

    // === ВЗАЄМОДІЯ ===
    public const float DEFAULT_INTERACTION_RANGE = 3f;
    public const string INTERACTION_LAYER_NAME = "Interactable";

    // === ВАЛІДАЦІЯ ===
    public const float MIN_POSITIVE_VALUE = 0.01f;
    public const float MIN_SPEED = 0.1f;
    public const float MIN_HEALTH = 1f;
    public const int MIN_MAGAZINE_SIZE = 1;
    public const int MIN_POOL_SIZE = 1;
    public const int MAX_REASONABLE_POOL_SIZE = 1000;

    // === НАЛАГОДЖЕННЯ ===
    public const string DEBUG_PREFIX = "[DEBUG]";
    public const string WARNING_PREFIX = "[WARNING]";
    public const string ERROR_PREFIX = "[ERROR]";
}

/// <summary>
/// Клас для валідації параметрів компонентів
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Валідує та виправляє швидкість руху
    /// </summary>
    public static float ValidateSpeed(float speed, string parameterName = "Speed")
    {
        if (speed < GameConstants.MIN_SPEED)
        {
            Debug.LogWarning($"{parameterName} було менше {GameConstants.MIN_SPEED}, встановлено мінімальне значення.");
            return GameConstants.MIN_SPEED;
        }
        return speed;
    }

    /// <summary>
    /// Валідує та виправляє значення здоров'я
    /// </summary>
    public static float ValidateHealth(float health, string parameterName = "Health")
    {
        if (health < GameConstants.MIN_HEALTH)
        {
            Debug.LogWarning($"{parameterName} було менше {GameConstants.MIN_HEALTH}, встановлено мінімальне значення.");
            return GameConstants.MIN_HEALTH;
        }
        return health;
    }

    /// <summary>
    /// Валідує та виправляє розмір магазину
    /// </summary>
    public static int ValidateMagazineSize(int size, string parameterName = "Magazine Size")
    {
        if (size < GameConstants.MIN_MAGAZINE_SIZE)
        {
            Debug.LogWarning($"{parameterName} було менше {GameConstants.MIN_MAGAZINE_SIZE}, встановлено мінімальне значення.");
            return GameConstants.MIN_MAGAZINE_SIZE;
        }
        return size;
    }

    /// <summary>
    /// Валідує компонент на null та логує помилку
    /// </summary>
    public static bool ValidateComponent<T>(T component, string componentName, MonoBehaviour context) where T : class
    {
        if (component == null)
        {
            Debug.LogError($"{GameConstants.ERROR_PREFIX} {componentName} не знайдено або не призначено!", context);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Валідує компонент на null з можливістю вимкнення скрипта
    /// </summary>
    public static bool ValidateComponentCritical<T>(T component, string componentName, MonoBehaviour context) where T : class
    {
        if (component == null)
        {
            Debug.LogError($"{GameConstants.ERROR_PREFIX} КРИТИЧНО: {componentName} не знайдено! Скрипт вимкнено.", context);
            context.enabled = false;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Нормалізує кут до діапазону -180 до 180
    /// </summary>
    public static float NormalizeAngle(float angle)
    {
        while (angle > GameConstants.ANGLE_NORMALIZATION_THRESHOLD)
            angle -= 360f;
        while (angle < -GameConstants.ANGLE_NORMALIZATION_THRESHOLD)
            angle += 360f;
        return angle;
    }

    /// <summary>
    /// Перевіряє, чи значення знаходиться в допустимому діапазоні
    /// </summary>
    public static bool IsInRange(float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Обмежує значення до діапазону
    /// </summary>
    public static float ClampValue(float value, float min, float max, string parameterName = "Value")
    {
        if (value < min || value > max)
        {
            Debug.LogWarning($"{parameterName} ({value}) було обмежено до діапазону [{min}, {max}].");
            return Mathf.Clamp(value, min, max);
        }
        return value;
    }
}