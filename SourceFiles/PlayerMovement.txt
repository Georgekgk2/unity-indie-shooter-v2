using UnityEngine;
using System.Collections.Generic; // Для List<AudioClip>

public class PlayerMovement : MonoBehaviour, IStateMachineOwner
{
    [Header("Movement Settings")]
    [Tooltip("Швидкість пересування гравця під час ходьби")]
    public float walkSpeed = 5f;
    [Tooltip("Швидкість пересування гравця під час бігу/спрінту")]
    public float sprintSpeed = 10f;
    [Tooltip("Наскільки швидко гравець розганяється до максимальної швидкості на землі")]
    public float groundAcceleration = 10f;
    [Tooltip("Наскільки швидко гравець зупиняється на землі")]
    public float groundDeceleration = 10f;
    [Tooltip("Контроль гравця у повітрі (0 = немає, 1 = повний)")]
    [Range(0f, 1f)]
    public float airControl = 0.5f; // Наскільки гравець може змінювати напрямок в повітрі
    [Tooltip("Швидкість, з якою гравець може змінити напрямок у повітрі.")]
    public float airAcceleration = 5f; // Нова змінна: Швидкість зміни напрямку в повітрі

    [Header("Jump Settings")]
    [Tooltip("Сила, з якою гравець стрибає")]
    public float jumpForce = 8f;
    [Tooltip("Дозволити подвійний стрибок?")]
    public bool allowDoubleJump = false;
    [Tooltip("Додатковий множник сили тяжіння, коли гравець падає (для більш швидкого падіння)")]
    public float fallMultiplier = 2.5f;
    [Tooltip("Множник сили тяжіння, якщо кнопка стрибка відпускається раніше (для низьких стрибків)")]
    public float lowJumpMultiplier = 2f;
    [Tooltip("Час (у секундах), протягом якого гравець може стрибнути після того, як залишив землю")]
    public float coyoteTime = 0.1f;
    [Tooltip("Час (у секундах), протягом якого натискання кнопки стрибка 'зберігається', щоб спрацювати при приземленні")]
    public float jumpBufferTime = 0.15f;

    [Header("Sprint Settings (Stamina)")]
    [Tooltip("Максимальна кількість стаміни")]
    public float maxStamina = 100f;
    [Tooltip("Швидкість витрати стаміни за секунду під час спрінту")]
    public float staminaDrainRate = 15f;
    [Tooltip("Швидкість відновлення стаміни за секунду, коли не спрінтує")]
    public float staminaRegenRate = 10f;
    [Tooltip("Затримка перед початком відновлення стаміни після припинення спрінту")]
    public float staminaRegenDelay = 1.5f;
    [Tooltip("Мінімальна кількість стаміни, необхідна для початку спрінту")]
    public float minStaminaToSprint = 10f;
    [Tooltip("Мінімальна швидкість руху гравця для витрати стаміни під час спрінту")]
    public float minSpeedToDrainStamina = 0.1f;

    [Header("Crouch Settings")]
    [Tooltip("Висота Box Collider під час присідання")]
    public float crouchColliderHeight = 1.0f;
    [Tooltip("Швидкість пересування під час присідання")]
    public float crouchSpeed = 2.5f;
    [Tooltip("Швидкість переходу між стоянням і присіданням (для колайдера і камери)")]
    public float crouchSmoothSpeed = 10f;
    [Tooltip("Локальна Y-координата камери під час присідання (відносно батьківського об'єкта, стандартна вертикальна вісь).")]
    public float crouchCameraLocalY = -0.2f;

    [Header("Slide Settings")]
    [Tooltip("Дозволити ковзання при спрінті?")]
    public bool allowSlide = true;
    [Tooltip("Додаткова сила, що застосовується на початку ковзання")]
    public float slideForce = 15f;
    [Tooltip("Тривалість ковзання в секундах")]
    public float slideDuration = 0.8f;
    [Tooltip("Висота Box Collider під час ковзання")]
    public float slideColliderHeight = 0.5f;
    [Tooltip("Кульдаун після ковзання в секундах")]
    public float slideCooldown = 1f;

    [Header("Dash Settings")]
    [Tooltip("Кнопка для виконання ривка/перекату.")]
    public KeyCode dashKey = KeyCode.Q;
    [Tooltip("Дозволити ривок/перекат?")]
    public bool allowDash = true;
    [Tooltip("Додаткова сила, що застосовується під час ривка.")]
    public float dashForce = 20f;
    [Tooltip("Тривалість ривка/перекату в секундах.")]
    public float dashDuration = 0.25f;
    [Tooltip("Висота Box Collider під час ривка/перекату.")]
    public float dashColliderHeight = 0.5f;
    [Tooltip("Локальна Y-координата камери під час ривка (зазвичай дуже низько, як при присіданні/ковзанні).")]
    public float dashCameraLocalY = -0.25f;
    [Tooltip("Кульдаун після ривка/перекату в секундах.")]
    public float dashCooldown = 1.5f;
    [Tooltip("Вартість стаміни за один ривок.")]
    public float dashStaminaCost = 20f;
    [Tooltip("Мінімальна стаміна, необхідна для виконання ривка.")]
    public float minStaminaToDash = 20f;
    [Tooltip("Множник швидкості для колайдера та камери під час переходу до/від ривка (для плавності).")]
    public float dashSmoothSpeedMultiplier = 2.0f;
    [Tooltip("Зміна FOV камери під час ривка.")]
    public float dashFOVChange = 10f;

    [Header("Headbob Settings")]
    [Tooltip("Трансформ камери для застосування хитання голови")]
    public Transform cameraTransform;
    [Tooltip("Базова амплітуда (величина) коливання голови")]
    public float headbobAmplitude = 0.05f;
    [Tooltip("Базова частота коливання голови (чим вище, тим швидше)")]
    public float headbobFrequency = 10f;
    [Tooltip("Множник частоти для спрінту")]
    public float headbobSprintFrequencyMultiplier = 1.5f;
    [Tooltip("Множник амплітуди для спрінту")]
    public float headbobSprintAmpMultiplier = 1.5f;
    [Tooltip("Множник частоти для присідання")]
    public float headbobCrouchFrequencyMultiplier = 0.7f;
    [Tooltip("Множник амплітуди для присідання")]
    public float headbobCrouchAmpMultiplier = 0.7f;


    [Header("Footstep Settings")]
    [Tooltip("Аудіо джерело для відтворення звуків кроків")]
    public AudioSource audioSource;
    [Tooltip("Список звукових кліпів для кроків (вибирається випадково)")]
    public List<AudioClip> footstepSounds;
    [Tooltip("Час між кроками під час ходьби")]
    public float walkFootstepInterval = 0.5f;
    [Tooltip("Час між кроками під час спрінту")]
    public float sprintFootstepInterval = 0.3f;
    [Tooltip("Час між кроками під час присідання")]
    public float crouchFootstepInterval = 0.7f;
    [Tooltip("Мінімальний поріг швидкості для відтворення звуків кроків")]
    public float minFootstepSpeed = 0.1f;


    [Header("Ground Check")]
    [Tooltip("Об'єкт Transform, який розташований біля ніг гравця для перевірки землі")]
    public Transform groundCheck;
    [Tooltip("Розмір куба для перевірки землі (половинні розміри). Наприклад, 0.4, 0.1, 0.4 для куба 0.8х0.2х0.8.")]
    public Vector3 groundCheckHalfExtents = new Vector3(0.4f, 0.1f, 0.4f);
    [Tooltip("Шар(и), які вважаються землею")]
    public LayerMask groundLayer;

    // --- Приватні змінні ---
    private Rigidbody rb;
    private Camera playerCamera;
    private BoxCollider boxCollider;
    [SerializeField] private bool isGrounded; // Зроблено SerializeField для налагодження

    // Змінні для стрибків
    [SerializeField] private int jumpsRemaining;
    private float lastGroundedTime;
    private float lastJumpPressTime;

    // Змінні для спрінту
    [SerializeField] private float currentStamina;
    [SerializeField] private bool isSprinting;
    private float lastSprintTime;

    // Змінні для присідання
    [SerializeField] private bool isCrouching;
    private float initialColliderHeight;
    private Vector3 initialColliderCenter;
    private float initialCameraLocalY; // Змінено на Y (стандартна вертикальна вісь)

    // Змінні для ковзання
    [SerializeField] private bool isSliding;
    private float slideStartTime;
    private float lastSlideTime;

    // Змінні для ривка/перекату
    [SerializeField] private bool isDashing;
    private float dashStartTime;
    private float lastDashTime;

    // Змінні для Headbob
    private Vector3 originalCameraLocalPos;
    private float headbobTimer;

    // Змінні для кроків
    private float nextFootstepTime;

    // Змінні для FOV
    private float defaultFOV;

    // Посилання на PlayerHealth (для вимкнення контролю при смерті)
    private PlayerHealth playerHealth;

    // State Machine
    private StateMachine stateMachine;
    [Header("State Machine Debug")]
    [SerializeField] private StateMachineDebugInfo debugInfo = new StateMachineDebugInfo();
    [SerializeField] private bool useStateMachine = true;

    // Публічні властивості для State Machine
    public StateMachine StateMachine => stateMachine;
    public bool IsGrounded => isGrounded;
    public bool CanSprint() => currentStamina >= minStaminaToSprint;
    public void DrainStamina() => HandleStaminaDrain();
    public void SetCrouchState(bool crouching) => SetCrouchingState(crouching);
    public void SetSlideState(bool sliding) => SetSlidingState(sliding);

    void Awake()
    {
        // Валідація критичних компонентів
        rb = GetComponent<Rigidbody>();
        if (!ValidationHelper.ValidateComponentCritical(rb, "Rigidbody", this)) return;
        rb.freezeRotation = true;

        boxCollider = GetComponent<BoxCollider>();
        if (!ValidationHelper.ValidateComponentCritical(boxCollider, "BoxCollider", this)) return;

        if (!ValidationHelper.ValidateComponentCritical(groundCheck, "Ground Check Transform", this)) return;

        // Зберігаємо оригінальні параметри колайдера
        initialColliderHeight = boxCollider.size.y;
        initialColliderCenter = boxCollider.center;

        // Ініціалізуємо стамину та стрибки
        currentStamina = maxStamina;
        jumpsRemaining = allowDoubleJump ? 2 : 1;

        // Знаходимо та валідуємо камеру
        InitializeCamera();

        // Знаходимо аудіо компонент (не критичний)
        InitializeAudio();

        // Знаходимо PlayerHealth (не критичний, але важливий)
        playerHealth = GetComponent<PlayerHealth>();
        if (!ValidationHelper.ValidateComponent(playerHealth, "PlayerHealth", this))
        {
            Debug.LogWarning("PlayerMovement: Перевірки смерті не працюватимуть.", this);
        }

        // Валідуємо параметри
        ValidateParameters();

        // Ініціалізуємо State Machine
        InitializeStateMachine();
    }

    /// <summary>
    /// Ініціалізує камеру та пов'язані компоненти
    /// </summary>
    void InitializeCamera()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (ValidationHelper.ValidateComponent(playerCamera, "Player Camera", this))
        {
            if (cameraTransform == null)
            {
                cameraTransform = playerCamera.transform;
            }
            originalCameraLocalPos = cameraTransform.localPosition;
            initialCameraLocalY = originalCameraLocalPos.y;
            defaultFOV = playerCamera.fieldOfView;
        }
        else
        {
            Debug.LogError("PlayerMovement: Функції камери не працюватимуть!", this);
        }
    }

    /// <summary>
    /// Ініціалізує аудіо компонент
    /// </summary>
    void InitializeAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (!ValidationHelper.ValidateComponent(audioSource, "AudioSource", this))
        {
            Debug.LogWarning("PlayerMovement: Звуки кроків не будуть відтворюватися.", this);
        }
    }

    /// <summary>
    /// Валідує та виправляє параметри компонента
    /// </summary>
    void ValidateParameters()
    {
        walkSpeed = ValidationHelper.ValidateSpeed(walkSpeed, "Walk Speed");
        sprintSpeed = ValidationHelper.ValidateSpeed(sprintSpeed, "Sprint Speed");
        crouchSpeed = ValidationHelper.ValidateSpeed(crouchSpeed, "Crouch Speed");
        
        // Переконуємося, що sprint швидше за walk
        if (sprintSpeed <= walkSpeed)
        {
            Debug.LogWarning("PlayerMovement: Sprint Speed має бути більше Walk Speed. Автоматично виправлено.");
            sprintSpeed = walkSpeed * 1.5f;
        }

        maxStamina = ValidationHelper.ValidateHealth(maxStamina, "Max Stamina");
        currentStamina = Mathf.Min(currentStamina, maxStamina);

        // Валідуємо часові параметри
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);

        // Заміняємо магічні числа константами
        minFootstepSpeed = GameConstants.MIN_FOOTSTEP_SPEED;
        fallMultiplier = GameConstants.DEFAULT_GRAVITY_MULTIPLIER;
        lowJumpMultiplier = GameConstants.LOW_JUMP_MULTIPLIER;
        minSpeedToDrainStamina = GameConstants.MIN_SPEED_TO_DRAIN_STAMINA;
    }

    /// <summary>
    /// Валідація параметрів в Unity Editor (викликається при зміні значень в Inspector)
    /// </summary>
    void OnValidate()
    {
        // Валідуємо швидкості
        walkSpeed = Mathf.Max(GameConstants.MIN_SPEED, walkSpeed);
        sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
        crouchSpeed = Mathf.Max(GameConstants.MIN_SPEED, crouchSpeed);

        // Валідуємо стамину
        maxStamina = Mathf.Max(GameConstants.MIN_HEALTH, maxStamina);
        staminaDrainRate = Mathf.Max(0f, staminaDrainRate);
        staminaRegenRate = Mathf.Max(0f, staminaRegenRate);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
        minStaminaToSprint = Mathf.Max(0f, minStaminaToSprint);

        // Валідуємо стрибки
        jumpForce = Mathf.Max(0f, jumpForce);
        fallMultiplier = Mathf.Max(1f, fallMultiplier);
        lowJumpMultiplier = Mathf.Max(1f, lowJumpMultiplier);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);

        // Валідуємо колайдери
        crouchColliderHeight = Mathf.Max(0.1f, crouchColliderHeight);
        slideColliderHeight = Mathf.Max(0.1f, slideColliderHeight);
        dashColliderHeight = Mathf.Max(0.1f, dashColliderHeight);

        // Валідуємо часові параметри
        slideDuration = Mathf.Max(0.1f, slideDuration);
        slideCooldown = Mathf.Max(0f, slideCooldown);
        dashDuration = Mathf.Max(0.1f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);

        // Валідуємо headbob
        headbobAmplitude = Mathf.Max(0f, headbobAmplitude);
        headbobFrequency = Mathf.Max(0f, headbobFrequency);

        // Валідуємо звуки кроків
        walkFootstepInterval = Mathf.Max(0.1f, walkFootstepInterval);
        sprintFootstepInterval = Mathf.Max(0.1f, sprintFootstepInterval);
        crouchFootstepInterval = Mathf.Max(0.1f, crouchFootstepInterval);
    }

    void OnDestroy()
    {
        // Очищуємо State Machine при знищенні
        if (stateMachine != null)
        {
            stateMachine.Clear();
        }
    }

    /// <summary>
    /// Ініціалізує State Machine з усіма станами
    /// </summary>
    void InitializeStateMachine()
    {
        if (!useStateMachine) return;

        stateMachine = new StateMachine(this);

        // Додаємо всі стани
        stateMachine.AddState(new IdleState(stateMachine, this));
        stateMachine.AddState(new WalkingState(stateMachine, this));
        stateMachine.AddState(new RunningState(stateMachine, this));
        stateMachine.AddState(new JumpingState(stateMachine, this));
        stateMachine.AddState(new FallingState(stateMachine, this));
        stateMachine.AddState(new CrouchingState(stateMachine, this));
        stateMachine.AddState(new SlidingState(stateMachine, this));

        // Запускаємо з початкового стану
        stateMachine.Start<IdleState>();

        Debug.Log("PlayerMovement: State Machine ініціалізовано з 7 станами");
    }

    /// <summary>
    /// Реалізація інтерфейсу IStateMachineOwner
    /// </summary>
    public void OnStateChanged(System.Type previousState, System.Type newState)
    {
        Debug.Log($"PlayerMovement: State changed from {previousState?.Name ?? "None"} to {newState.Name}");
    }

    /// <summary>
    /// Методи для State Machine - управління присіданням
    /// </summary>
    void SetCrouchingState(bool crouching)
    {
        isCrouching = crouching;
        // Логіка зміни колайдера буде в UpdateColliderAndCamera
    }

    /// <summary>
    /// Методи для State Machine - управління ковзанням
    /// </summary>
    void SetSlidingState(bool sliding)
    {
        isSliding = sliding;
        if (sliding)
        {
            lastSlideTime = Time.time;
        }
        // Логіка зміни колайдера буде в UpdateColliderAndCamera
    }

    /// <summary>
    /// Методи для State Machine - обробка стаміни
    /// </summary>
    void HandleStaminaDrain()
    {
        if (currentStamina > 0 && new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude > minSpeedToDrainStamina)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            
            // Відправляємо подію зміни стаміни
            Events.Trigger(new StaminaChangedEvent(currentStamina, maxStamina));
        }
    }

    void Update()
    {
        // Якщо гравець мертвий, вимикаємо всі дії, окрім оновлення колайдера/камери та FOV
        if (playerHealth != null && playerHealth.IsDead())
        {
            HandleDeathState();
            return;
        }

        // Оновлюємо State Machine (якщо використовується)
        if (useStateMachine && stateMachine != null)
        {
            stateMachine.Update();
            debugInfo.UpdateInfo(stateMachine);
        }

        // === Перевірка землі ===
        // Використовуємо Physics.OverlapBox. Quaternion.identity означає без обертання для коробки.
        isGrounded = Physics.OverlapBox(groundCheck.position, groundCheckHalfExtents, Quaternion.identity, groundLayer).Length > 0;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            if (!isSliding && !isDashing) // Скидаємо стрибки, якщо не ковзаємо і не в ривку
            {
                jumpsRemaining = allowDoubleJump ? 2 : 1;
            }
        }

        // === Обробка вхідних даних для стрибка ===
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressTime = Time.time;
        }

        // === Логіка Перекату/Ривка (Dash) ===
        // Ривок має найвищий пріоритет.
        if (allowDash && Input.GetKeyDown(dashKey) && !isDashing && isGrounded && currentStamina >= minStaminaToDash && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
        else if (isDashing)
        {
            UpdateDash();
        }

        // === Логіка Присідання та Ковзання ===
        // Обробляємо тільки якщо гравець не в ривку
        if (!isDashing)
        {
            bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

            // Ковзання: якщо дозволено, спрінтимо, хочемо присісти, на землі, кульдаун пройшов
            if (allowSlide && isSprinting && wantsToCrouch && isGrounded && Time.time >= lastSlideTime + slideCooldown)
            {
                StartSlide();
            }
            else if (isSliding)
            {
                UpdateSlide();
            }
            else // Якщо не ковзаємо, обробляємо звичайне присідання
            {
                // Перемикання присідання за натисканням
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    if (isCrouching) { TryStandUp(); }
                    else { isCrouching = true; }
                }
                // Якщо присідаємо і кнопку відпустили
                // (Примітка: цей блок спрацює, якщо ви хочете, щоб присідання трималося, поки тримаєте кнопку)
                if (isCrouching && !wantsToCrouch)
                {
                    TryStandUp();
                }
            }
        }
        else // Якщо в ривку, примусово вимикаємо присідання, спрінт, ковзання
        {
            isCrouching = false;
            isSprinting = false;
            isSliding = false;
        }

        // === Логіка Спрінту та Стаміни ===
        // Обробляємо тільки якщо не в ривку і не ковзаємо
        if (!isDashing && !isSliding)
        {
            Vector3 currentMoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            float currentHorizontalSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);
            bool canSprint = wantsToSprint && !isCrouching && currentStamina > minStaminaToSprint && currentHorizontalSpeed > minSpeedToDrainStamina;

            if (canSprint)
            {
                isSprinting = true;
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(currentStamina, 0);
                lastSprintTime = Time.time;
            }
            else
            {
                isSprinting = false;
                if (Time.time - lastSprintTime >= staminaRegenDelay && currentStamina < maxStamina)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);
                }
            }
        }
        else
        {
            isSprinting = false; // Виходимо зі спрінту, якщо в ковзанні або ривку
        }

        // === Оновлення розміру BoxCollider та позиції камери ===
        UpdateColliderAndCamera();

        // === Headbobing ===
        HandleHeadbob(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);

        // === Оновлення FOV камери ===
        UpdateCameraFOV();
    }

    void FixedUpdate()
    {
        // Якщо гравець мертвий, зупиняємо рух фізики
        if (playerHealth != null && playerHealth.IsDead())
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // Використовуємо State Machine для фізики (якщо увімкнено)
        if (useStateMachine && stateMachine != null)
        {
            stateMachine.FixedUpdate();
            return; // State Machine керує рухом
        }

        // === Рух гравця ===
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (playerCamera == null) {
            Debug.LogWarning("PlayerMovement: playerCamera не призначена в FixedUpdate. Рух може бути некоректним.", this);
            return;
        }

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0; // Для руху по горизонтальній площині
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredMoveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        float currentTargetSpeed = walkSpeed; // Базова швидкість

        // Визначаємо поточну цільову швидкість на основі стану гравця
        if (isDashing)
        {
            currentTargetSpeed = dashForce;
        }
        else if (isSliding)
        {
            currentTargetSpeed = slideForce;
        }
        else if (isSprinting)
        {
            currentTargetSpeed = sprintSpeed;
        }
        else if (isCrouching)
        {
            currentTargetSpeed = crouchSpeed;
        }
        
        // Обмеження швидкості, якщо стаміна вичерпана
        if (currentStamina <= 0 && isSprinting && !isSliding && !isDashing) {
            currentTargetSpeed = walkSpeed;
        }


        // Враховуємо контроль у повітрі
        float currentAccelerationFactor = isGrounded ? groundAcceleration : airAcceleration; // Змінено на airAcceleration
        float currentDecelerationFactor = isGrounded ? groundDeceleration : airControl; // AirControl використовується як decelerator

        Vector3 targetVelocity = desiredMoveDirection * currentTargetSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 newHorizontalVelocity = currentHorizontalVelocity;

        // --- Покращений контроль у повітрі ---
        if (isDashing || isSliding)
        {
            // Під час ривка або ковзання, основний рух - це імпульс. WASD не сильно впливає.
            // Можна додати невелике "підрулювання" тут.
            // newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, airControl * Time.fixedDeltaTime);
        }
        else if (desiredMoveDirection.magnitude > 0.1f) // Якщо є вхідні дані для руху
        {
            newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, currentAccelerationFactor * Time.fixedDeltaTime);
        }
        else // Якщо немає вхідних даних для руху, поступово зупиняємо
        {
            // На землі зупиняємося швидко, у повітрі - повільніше
            if (isGrounded)
            {
                newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, currentDecelerationFactor * Time.fixedDeltaTime);
            }
            else // У повітрі лише зменшуємо інерцію, якщо немає іншого напрямку
            {
                newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, airControl * Time.fixedDeltaTime * 0.5f); // Зменшуємо швидкість повільніше
            }
        }
        
        rb.velocity = new Vector3(newHorizontalVelocity.x, rb.velocity.y, newHorizontalVelocity.z);


        // === Обробка стрибка у FixedUpdate (для фізики) ===
        bool canJump = (Time.time - lastJumpPressTime < jumpBufferTime) &&
                       ((Time.time - lastGroundedTime < coyoteTime) || (jumpsRemaining > 0));

        if (canJump && !isCrouching && !isSliding && !isDashing) // Не можна стрибати під час присідання, ковзання або ривка
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Обнуляємо вертикальну швидкість
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Додаємо імпульс

            lastJumpPressTime = 0;
            lastGroundedTime = 0; // Скидаємо койот-тайм після стрибка
            
            // Якщо подвійний стрибок або якщо вже в повітрі
            if (!isGrounded && jumpsRemaining > 0)
            {
                jumpsRemaining--;
                Debug.Log("Double Jump! Jumps Remaining: " + jumpsRemaining);
            }
            else if (isGrounded) // Якщо стрибок з землі
            {
                jumpsRemaining = allowDoubleJump ? 1 : 0; // Залишаємо один, якщо подвійний дозволено
                Debug.Log("Ground Jump! Jumps Remaining: " + jumpsRemaining);
            }
        }

        // === Змінна висота стрибка та швидке падіння ===
        // Додаткова гравітація для падіння
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Менша висота стрибка, якщо кнопку відпустили
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // === Обробка звуків кроків ===
        HandleFootsteps(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
    }

    // --- Допоміжні методи ---

    /// <summary>
    /// Обробляє стан, коли гравець мертвий.
    /// </summary>
    private void HandleDeathState()
    {
        // Якщо гравець мертвий, забезпечуємо, що він не рухається і не взаємодіє.
        // Змінюємо лише візуальні аспекти (колайдер, камера, FOV).
        isCrouching = false;
        isSprinting = false;
        isSliding = false;
        isDashing = false;
        
        UpdateColliderAndCamera(); // Оновлюємо, щоб гравець повернувся у стояче положення
        UpdateCameraFOV(); // Оновлюємо, щоб FOV повернувся до стандартного
    }


    /// <summary>
    /// Оновлює висоту BoxCollider та Y-позицію камери при присіданні/стоянні/ковзанні/ривку.
    /// Камера зміщується по локальній осі Y (стандартна вертикальна вісь).
    /// </summary>
    private void UpdateColliderAndCamera()
    {
        if (boxCollider == null || cameraTransform == null) return;

        float targetColliderHeight;
        float targetCameraLocalY;
        float currentSmoothSpeed = crouchSmoothSpeed;

        // Пріоритет станів: Dash > Slide > Crouch > Stand
        if (isDashing)
        {
            targetColliderHeight = dashColliderHeight;
            targetCameraLocalY = dashCameraLocalY;
            currentSmoothSpeed *= dashSmoothSpeedMultiplier;
        }
        else if (isSliding)
        {
            targetColliderHeight = slideColliderHeight;
            targetCameraLocalY = crouchCameraLocalY; // При ковзанні камера на тій же висоті, що й при присіданні
        }
        else if (isCrouching)
        {
            targetColliderHeight = crouchColliderHeight;
            targetCameraLocalY = crouchCameraLocalY;
        }
        else // Стоїмо
        {
            targetColliderHeight = initialColliderHeight;
            targetCameraLocalY = initialCameraLocalY;
        }

        // Обчислюємо цільовий центр для BoxCollider, щоб його нижня частина залишалася на місці.
        // (Припускаємо, що pivot гравця знаходиться біля основи колайдера або на 0,0,0)
        Vector3 targetColliderCenter = new Vector3(initialColliderCenter.x,
                                                   initialColliderCenter.y - (initialColliderHeight - targetColliderHeight) / 2f,
                                                   initialColliderCenter.z);

        // Плавна зміна розміру та центру колайдера
        boxCollider.size = Vector3.Lerp(boxCollider.size, new Vector3(boxCollider.size.x, targetColliderHeight, boxCollider.size.z), currentSmoothSpeed * Time.deltaTime);
        boxCollider.center = Vector3.Lerp(boxCollider.center, targetColliderCenter, currentSmoothSpeed * Time.deltaTime);

        // Плавна зміна позиції камери по локальній осі Y (для "вертикального" присідання/ривка)
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
                                                    new Vector3(cameraTransform.localPosition.x, targetCameraLocalY, cameraTransform.localPosition.z),
                                                    currentSmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Спроба встати з присідання. Перевіряє, чи є достатньо місця над гравцем за допомогою BoxCast.
    /// </summary>
    private void TryStandUp()
    {
        if (boxCollider == null) return;

        // Поточні розміри
        float currentColliderHeight = boxCollider.size.y;
        Vector3 currentColliderCenter = boxCollider.center;

        // Якщо вже стоїмо, то нічого не робимо
        if (Mathf.Abs(currentColliderHeight - initialColliderHeight) < 0.01f)
        {
            isCrouching = false; // Переконаємось, що стан правильний
            return;
        }

        // Обчислюємо початкову точку BoxCast (від поточної верхівки колайдера)
        // та відстань до цільової повної висоти.
        Vector3 currentColliderWorldTop = transform.position + transform.TransformVector(currentColliderCenter + Vector3.up * (currentColliderHeight / 2f));
        float checkDistance = initialColliderHeight - currentColliderHeight;
        
        // Розміри коробки для перевірки (беремо розміри стоячого гравця)
        // Застосовуємо розміри стоячого гравця, щоб перевірити, чи поміститься він повністю.
        Vector3 checkHalfExtents = new Vector3(boxCollider.size.x / 2f, (initialColliderHeight / 2f), boxCollider.size.z / 2f); // Половина висоти стоячої моделі

        // Сміщення, щоб BoxCast починався від центру нової (стоячої) висоти.
        // Це гарантує, що ми перевіряємо простір, який буде зайнятий.
        Vector3 boxCastOrigin = transform.position + transform.TransformVector(initialColliderCenter);

        // Виконуємо BoxCast, ігноруючи власні коллайдери гравця
        // RaycastHit hit; // Для відладки
        if (Physics.BoxCast(boxCastOrigin, checkHalfExtents, Vector3.up, transform.rotation, 0.1f, groundLayer, QueryTriggerInteraction.Ignore)) // Зроблено короткий каст 0.1f
        {
            // Debug.Log($"Cannot stand up: Obstruction detected above player by {hit.collider.name}"); // Відладкове повідомлення
        }
        else
        {
            isCrouching = false;
            Debug.Log("Player stood up.");
        }
    }

    /// <summary>
    /// Запускає механіку ковзання.
    /// </summary>
    private void StartSlide()
    {
        isSliding = true;
        isCrouching = true; // Ковзання автоматично переходить у стан присідання (візуально)
        slideStartTime = Time.time;
        lastSlideTime = Time.time;

        Vector3 slideDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        if (slideDirection.magnitude < 0.1f) // Якщо стоїмо, ковзаємо вперед
        {
            slideDirection = playerCamera.transform.forward;
            slideDirection.y = 0;
            slideDirection.Normalize();
        }
        
        rb.velocity = new Vector3(0, rb.velocity.y, 0); // Скидаємо поточну горизонтальну швидкість
        rb.AddForce(slideDirection * slideForce, ForceMode.Impulse);

        currentStamina = Mathf.Max(0, currentStamina - (staminaDrainRate * slideDuration / 2f)); // Витрачаємо частину стаміни
        Debug.Log("Player Started Sliding!");
    }

    /// <summary>
    /// Оновлює стан ковзання. Зменшує швидкість з часом.
    /// </summary>
    private void UpdateSlide()
    {
        if (Time.time - slideStartTime >= slideDuration)
        {
            EndSlide();
            return;
        }

        // Плавно зменшуємо швидкість ковзання.
        // Забезпечуємо, що гравець має хоч якийсь контроль над рухом, навіть під час ковзання.
        float slideProgress = (Time.time - slideStartTime) / slideDuration;
        float currentTargetHorizontalSpeed = Mathf.Lerp(slideForce, crouchSpeed, slideProgress);

        // Обмежуємо швидкість, щоб не було прискорення, якщо гравець намагається рухатися швидше, ніж ковзання.
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (currentHorizontalVelocity.magnitude > currentTargetHorizontalSpeed + 0.1f) // Якщо швидкість вища за цільову ковзання
        {
            rb.velocity = Vector3.Lerp(currentHorizontalVelocity, currentHorizontalVelocity.normalized * currentTargetHorizontalSpeed, Time.deltaTime * slideForce);
        }
    }

    /// <summary>
    /// Завершує механіку ковзання.
    /// </summary>
    private void EndSlide()
    {
        if (!isSliding) return; // Запобігаємо повторному виклику
        isSliding = false;
        // Після ковзання, якщо кнопку присідання відпустили, спробуємо встати.
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            TryStandUp();
        }
        isSprinting = false; // Виходимо зі спрінту
        Debug.Log("Player Ended Sliding!");
    }

    /// <summary>
    /// Запускає механіку ривка/перекату.
    /// </summary>
    private void StartDash()
    {
        isDashing = true;
        dashStartTime = Time.time;
        lastDashTime = Time.time;

        currentStamina = Mathf.Max(0, currentStamina - dashStaminaCost);

        isCrouching = false;
        isSprinting = false;
        isSliding = false;

        Vector3 dashDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = playerCamera.transform.forward;
            dashDirection.y = 0;
            dashDirection.Normalize();
        }

        // Обнуляємо поточну горизонтальну швидкість для чистого імпульсу ривка
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        Debug.Log("Player Started Dashing!");
    }

    /// <summary>
    /// Оновлює стан ривка/перекату.
    /// </summary>
    private void UpdateDash()
    {
        if (Time.time - dashStartTime >= dashDuration)
        {
            EndDash();
            return;
        }
    }

    /// <summary>
    /// Завершує механіку ривка/перекату.
    /// </summary>
    private void EndDash()
    {
        if (!isDashing) return; // Запобігаємо повторному виклику
        isDashing = false;
        // Після ривка гравець повертається в стояче положення (або присідає, якщо утримується кнопка присідання)
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            TryStandUp();
        }
        isSprinting = false;
        Debug.Log("Player Ended Dashing!");
    }


    /// <summary>
    /// Обробляє ефект хитання голови (headbob).
    /// </summary>
    /// <param name="currentSpeed">Поточна горизонтальна швидкість гравця.</param>
    private void HandleHeadbob(float currentSpeed)
    {
        if (cameraTransform == null) return;

        if (isGrounded && currentSpeed > minFootstepSpeed && !isDashing) // Headbob не працює під час ривка
        {
            float bobFrequency = headbobFrequency;
            float bobAmplitude = headbobAmplitude;

            if (isSprinting)
            {
                bobFrequency *= headbobSprintFrequencyMultiplier;
                bobAmplitude *= headbobSprintAmpMultiplier;
            }
            else if (isCrouching || isSliding)
            {
                bobFrequency *= headbobCrouchFrequencyMultiplier;
                bobAmplitude *= headbobCrouchAmpMultiplier;
            }

            headbobTimer += Time.deltaTime * bobFrequency;

            // Хитання по осі X (бік-в-бік) та Y (вгору-вниз) - стандартні локальні осі камери
            float bobX = Mathf.Cos(headbobTimer * 0.5f) * bobAmplitude * 0.5f; // Горизонтальне хитання (локальна X)
            float bobY = Mathf.Sin(headbobTimer) * bobAmplitude; // Основне вертикальне коливання камери (локальна Y)

            cameraTransform.localPosition = new Vector3(
                originalCameraLocalPos.x + bobX,
                cameraTransform.localPosition.y + bobY, // Додаємо до поточної локальної Y (яка вже враховує присідання/стояння)
                originalCameraLocalPos.z // Локальна Z залишається базовою (це ваша "вперед" вісь для камери)
            );
        }
        else // Якщо не рухаємось, в повітрі або в ривку, повертаємо камеру до початкової позиції
        {
            headbobTimer = 0; // Скидаємо таймер

            // Повертаємо X та Z до початкових, Y (вертикаль) залишається від присідання/стояння
            Vector3 targetBobPos = new Vector3(originalCameraLocalPos.x, cameraTransform.localPosition.y, originalCameraLocalPos.z);
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetBobPos, Time.deltaTime * headbobFrequency);
        }
    }

    /// <summary>
    /// Відтворює звуки кроків гравця.
    /// </summary>
    /// <param name="moveMagnitude">Поточна горизонтальна швидкість гравця.</param>
    private void HandleFootsteps(float moveMagnitude)
    {
        if (isDashing || isSliding) return; // Не відтворюємо звуки під час ривка/ковзання (можна додати окремі звуки для них)

        if (audioSource == null || footstepSounds.Count == 0 || !isGrounded || moveMagnitude < minFootstepSpeed) return;

        float currentFootstepInterval = walkFootstepInterval;
        if (isSprinting)
        {
            currentFootstepInterval = sprintFootstepInterval;
        }
        else if (isCrouching)
        {
            currentFootstepInterval = crouchFootstepInterval;
        }

        if (Time.time >= nextFootstepTime)
        {
            AudioClip clipToPlay = footstepSounds[Random.Range(0, footstepSounds.Count)];
            audioSource.PlayOneShot(clipToPlay);

            nextFootstepTime = Time.time + currentFootstepInterval;
        }
    }

    /// <summary>
    /// Оновлює поле зору (FOV) камери.
    /// </summary>
    private void UpdateCameraFOV()
    {
        if (playerCamera == null) return;

        float targetFOV = defaultFOV;
        
        // FOV при спрінті та ривку
        if (isSprinting)
        {
            targetFOV = defaultFOV + dashFOVChange; // Використовуємо FOV від ривка для спрінту
        }
        if (isDashing)
        {
            targetFOV = defaultFOV + dashFOVChange * 1.5f; // Більший FOV для ривка
        }
        // Якщо є активний WeaponController і він прицілений, його FOV має пріоритет
        // (Для цього WeaponController повинен бути доступний тут або мати статичні поля FOV)
        // Наразі PlayerMovement не знає про стан прицілювання зброї, тому FOV тільки від руху.
        // Якщо ви хочете, щоб FOV при прицілюванні "перебивав" FOV при спрінті,
        // то логіка FOV має бути в WeaponController або централізованому скрипті камери.
        
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * dashSmoothSpeedMultiplier * 2f);
    }


    // --- Публічні методи для доступу з інших скриптів (наприклад, для UI) ---

    public float GetCurrentStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
    
    public bool IsCrouching()
    {
        return isCrouching;
    }

    public bool IsSprinting()
    {
        return isSprinting;
    }

    public bool IsSliding()
    {
        return isSliding;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    // Для візуалізації GroundCheck в редакторі
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckHalfExtents * 2f);
    }
    
}