using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings (Mouse Input)")]
    [Tooltip("Наскільки сильно зброя буде рухатися по горизонталі (від руху миші X)")]
    public float swayAmountX = 0.5f;
    [Tooltip("Наскільки сильно зброя буде рухатися по вертикалі (від руху миші Y)")]
    public float swayAmountY = 0.5f;
    [Tooltip("Плавність повернення зброї до початкової позиції")]
    public float smoothAmount = 6f;
    [Tooltip("Мінімальний рух миші для активації sway (мертва зона)")]
    public float mouseDeadZone = 0.001f;
    [Tooltip("Максимальне відхилення зброї від центру (у градусах)")]
    public float maxSwayAngle = 5f;


    [Header("Bob Settings (Walk/Run Input)")]
    [Tooltip("Амплітуда (величина) хитання зброї по X та Y осях під час ходьби.")]
    public Vector2 walkBobAmount = new Vector2(0.1f, 0.1f);
    [Tooltip("Частота хитання зброї під час ходьби.")]
    public float walkBobFrequency = 10f;
    [Tooltip("Множник амплітуди хитання зброї при бігу.")]
    public float sprintBobAmountMultiplier = 2f;
    [Tooltip("Множник частоти хитання зброї при бігу.")]
    public float sprintBobFrequencyMultiplier = 1.5f;
    [Tooltip("Амплітуда хитання зброї при присіданні.")]
    public Vector2 crouchBobAmount = new Vector2(0.006f, 0.006f);
    [Tooltip("Частота хитання зброї при присіданні.")]
    public float crouchBobFrequency = 7f;
    [Tooltip("Мінімальна швидкість руху гравця для активації bob.")]
    public float minBobSpeed = 0.1f;

    [Header("Vertical Speed Tilt Settings")]
    [Tooltip("Наскільки сильно зброя нахиляється вниз при стрибку (від 0 до 1).")]
    [Range(0f, 1f)]
    public float jumpTiltAmount = 0.3f;
    [Tooltip("Наскільки сильно зброя нахиляється вгору при падінні (від 0 до 1).")]
    [Range(0f, 1f)]
    public float fallTiltAmount = 0.3f;
    [Tooltip("Максимальний кут нахилу зброї від вертикальної швидкості.")]
    public float maxTiltAngle = 10f;
    [Tooltip("Плавність нахилу зброї від вертикальної швидкості.")]
    public float tiltSmoothSpeed = 5f;


    // Приватні змінні
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    private float bobTimer;
    
    // Посилання на PlayerMovement та Rigidbody гравця
    private PlayerMovement playerMovement;
    private Rigidbody playerRb; 

    // Проміжні обертання для комбінування
    private Quaternion calculatedSwayRotation; // Обертання від руху миші
    private Quaternion calculatedTiltRotation; // Обертання від вертикальної швидкості


    void Awake() // Використовуємо Awake для ініціалізації
    {
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("WeaponSway: PlayerMovement скрипт не знайдено в батьківських об'єктах. Деякі функції WeaponSway не працюватимуть.", this);
        }
        else
        {
            playerRb = playerMovement.GetComponent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogWarning("WeaponSway: Rigidbody не знайдено на об'єкті PlayerMovement. Vertical Speed Tilt не працюватиме.", this);
            }
        }
    }

    void Update()
    {
        // 1. Обчислюємо Sway (рух миші)
        calculatedSwayRotation = CalculateWeaponSway();

        // 2. Обчислюємо Bob (рух гравця WASD)
        HandleWeaponBob(); // Цей метод змінює transform.localPosition

        // 3. Обчислюємо Vertical Speed Tilt
        calculatedTiltRotation = CalculateVerticalSpeedTilt();

        // 4. Комбінуємо всі обертання та застосовуємо до зброї
        // Порядок множення: initialLocalRotation * (sway) * (tilt)
        // (x * y) - це спочатку y, потім x
        // Тому якщо ми хочемо, щоб Tilt був "над" Sway, то Tilt йде останнім у множенні,
        // або першим, якщо множимо справа наліво (Quaternion.Euler(tilt) * Quaternion.Euler(sway))
        // Спробуємо: initial (базове обертання) * tilt * sway.
        // Зазвичай, sway/tilt застосовуються відносно базового обертання.
        
        // Final Rotation = Базове обертання * (Sway Rotation) * (Tilt Rotation)
        Quaternion finalRotation = initialLocalRotation * calculatedSwayRotation * calculatedTiltRotation;

        // Плавно застосовуємо кінцеве обертання
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * smoothAmount);
    }

    /// <summary>
    /// Обчислює обертання зброї від руху миші (Sway).
    /// </summary>
    /// <returns>Quaternion, що представляє обертання Sway.</returns>
    Quaternion CalculateWeaponSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) < mouseDeadZone) mouseX = 0f;
        if (Mathf.Abs(mouseY) < mouseDeadZone) mouseY = 0f;
        
        float rotationAmountX = -mouseY * swayAmountY;
        float rotationAmountY = mouseX * swayAmountX;

        rotationAmountX = Mathf.Clamp(rotationAmountX, -maxSwayAngle, maxSwayAngle);
        rotationAmountY = Mathf.Clamp(rotationAmountY, -maxSwayAngle, maxSwayAngle);
        
        return Quaternion.Euler(rotationAmountX, rotationAmountY, 0);
    }

    /// <summary>
    /// Обробляє позиційне хитання зброї від руху гравця (Bob).
    /// </summary>
    void HandleWeaponBob()
    {
        if (playerMovement == null || playerRb == null) return;

        float currentHorizontalSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
        
        if (currentHorizontalSpeed < minBobSpeed) {
             bobTimer = 0;
             transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * walkBobFrequency);
             return;
        }

        float currentBobFrequency = walkBobFrequency;
        Vector2 currentBobAmount = walkBobAmount;

        if (playerMovement.IsSprinting())
        {
            currentBobFrequency *= sprintBobFrequencyMultiplier;
            currentBobAmount *= sprintBobAmountMultiplier;
        }
        else if (playerMovement.IsCrouching() || playerMovement.IsSliding() || playerMovement.IsDashing())
        {
            currentBobFrequency = crouchBobFrequency;
            currentBobAmount = crouchBobAmount;
        }

        bobTimer += Time.deltaTime * currentBobFrequency;

        float bobOffsetX = Mathf.Cos(bobTimer / 2f) * currentBobAmount.x;
        float bobOffsetY = Mathf.Sin(bobTimer) * currentBobAmount.y;

        transform.localPosition = new Vector3(
            initialLocalPosition.x + bobOffsetX,
            initialLocalPosition.y + bobOffsetY,
            initialLocalPosition.z
        );
    }

    /// <summary>
    /// Обчислює обертання зброї від вертикальної швидкості гравця (Tilt).
    /// </summary>
    /// <returns>Quaternion, що представляє обертання Tilt.</returns>
    Quaternion CalculateVerticalSpeedTilt()
    {
        if (playerRb == null) return Quaternion.identity;

        float verticalVelocity = playerRb.velocity.y;
        float targetTiltAngle = 0f;

        if (verticalVelocity > 0.1f) // Стрибаємо (рухаємося вгору)
        {
            targetTiltAngle = -verticalVelocity * jumpTiltAmount; // Мінус, щоб нахиляло вниз
        }
        else if (verticalVelocity < -0.1f) // Падаємо (рухаємося вниз)
        {
            targetTiltAngle = -verticalVelocity * fallTiltAmount; // Мінус, щоб нахиляло вгору (від'ємна швидкість * від'ємний коефіцієнт = позитивний кут)
        }

        targetTiltAngle = Mathf.Clamp(targetTiltAngle, -maxTiltAngle, maxTiltAngle);

        // Плавно переходимо до цільового кута
        // Це значення буде Lerp'атися в Update() при застосуванні finalRotation.
        // Тому тут просто повертаємо обертання, що представляє цей нахил.
        return Quaternion.Euler(targetTiltAngle, 0, 0);
    }
}