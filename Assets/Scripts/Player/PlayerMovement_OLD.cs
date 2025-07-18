using UnityEngine;
using System.Collections.Generic; // Äëÿ List<AudioClip>

public class PlayerMovement : MonoBehaviour, IStateMachineOwner
{
    [Header("Movement Settings")]
    [Tooltip("Øâèäê³ñòü ïåðåñóâàííÿ ãðàâöÿ ï³ä ÷àñ õîäüáè")]
    public float walkSpeed = 5f;
    [Tooltip("Øâèäê³ñòü ïåðåñóâàííÿ ãðàâöÿ ï³ä ÷àñ á³ãó/ñïð³íòó")]
    public float sprintSpeed = 10f;
    [Tooltip("Íàñê³ëüêè øâèäêî ãðàâåöü ðîçãàíÿºòüñÿ äî ìàêñèìàëüíî¿ øâèäêîñò³ íà çåìë³")]
    public float groundAcceleration = 10f;
    [Tooltip("Íàñê³ëüêè øâèäêî ãðàâåöü çóïèíÿºòüñÿ íà çåìë³")]
    public float groundDeceleration = 10f;
    [Tooltip("Êîíòðîëü ãðàâöÿ ó ïîâ³òð³ (0 = íåìàº, 1 = ïîâíèé)")]
    [Range(0f, 1f)]
    public float airControl = 0.5f; // Íàñê³ëüêè ãðàâåöü ìîæå çì³íþâàòè íàïðÿìîê â ïîâ³òð³
    [Tooltip("Øâèäê³ñòü, ç ÿêîþ ãðàâåöü ìîæå çì³íèòè íàïðÿìîê ó ïîâ³òð³.")]
    public float airAcceleration = 5f; // Íîâà çì³ííà: Øâèäê³ñòü çì³íè íàïðÿìêó â ïîâ³òð³

    [Header("Jump Settings")]
    [Tooltip("Ñèëà, ç ÿêîþ ãðàâåöü ñòðèáàº")]
    public float jumpForce = 8f;
    [Tooltip("Äîçâîëèòè ïîäâ³éíèé ñòðèáîê?")]
    public bool allowDoubleJump = false;
    [Tooltip("Äîäàòêîâèé ìíîæíèê ñèëè òÿæ³ííÿ, êîëè ãðàâåöü ïàäàº (äëÿ á³ëüø øâèäêîãî ïàä³ííÿ)")]
    public float fallMultiplier = 2.5f;
    [Tooltip("Ìíîæíèê ñèëè òÿæ³ííÿ, ÿêùî êíîïêà ñòðèáêà â³äïóñêàºòüñÿ ðàí³øå (äëÿ íèçüêèõ ñòðèáê³â)")]
    public float lowJumpMultiplier = 2f;
    [Tooltip("×àñ (ó ñåêóíäàõ), ïðîòÿãîì ÿêîãî ãðàâåöü ìîæå ñòðèáíóòè ï³ñëÿ òîãî, ÿê çàëèøèâ çåìëþ")]
    public float coyoteTime = 0.1f;
    [Tooltip("×àñ (ó ñåêóíäàõ), ïðîòÿãîì ÿêîãî íàòèñêàííÿ êíîïêè ñòðèáêà 'çáåð³ãàºòüñÿ', ùîá ñïðàöþâàòè ïðè ïðèçåìëåíí³")]
    public float jumpBufferTime = 0.15f;

    [Header("Sprint Settings (Stamina)")]
    [Tooltip("Ìàêñèìàëüíà ê³ëüê³ñòü ñòàì³íè")]
    public float maxStamina = 100f;
    [Tooltip("Øâèäê³ñòü âèòðàòè ñòàì³íè çà ñåêóíäó ï³ä ÷àñ ñïð³íòó")]
    public float staminaDrainRate = 15f;
    [Tooltip("Øâèäê³ñòü â³äíîâëåííÿ ñòàì³íè çà ñåêóíäó, êîëè íå ñïð³íòóº")]
    public float staminaRegenRate = 10f;
    [Tooltip("Çàòðèìêà ïåðåä ïî÷àòêîì â³äíîâëåííÿ ñòàì³íè ï³ñëÿ ïðèïèíåííÿ ñïð³íòó")]
    public float staminaRegenDelay = 1.5f;
    [Tooltip("Ì³í³ìàëüíà ê³ëüê³ñòü ñòàì³íè, íåîáõ³äíà äëÿ ïî÷àòêó ñïð³íòó")]
    public float minStaminaToSprint = 10f;
    [Tooltip("Ì³í³ìàëüíà øâèäê³ñòü ðóõó ãðàâöÿ äëÿ âèòðàòè ñòàì³íè ï³ä ÷àñ ñïð³íòó")]
    public float minSpeedToDrainStamina = 0.1f;

    [Header("Crouch Settings")]
    [Tooltip("Âèñîòà Box Collider ï³ä ÷àñ ïðèñ³äàííÿ")]
    public float crouchColliderHeight = 1.0f;
    [Tooltip("Øâèäê³ñòü ïåðåñóâàííÿ ï³ä ÷àñ ïðèñ³äàííÿ")]
    public float crouchSpeed = 2.5f;
    [Tooltip("Øâèäê³ñòü ïåðåõîäó ì³æ ñòîÿííÿì ³ ïðèñ³äàííÿì (äëÿ êîëàéäåðà ³ êàìåðè)")]
    public float crouchSmoothSpeed = 10f;
    [Tooltip("Ëîêàëüíà Y-êîîðäèíàòà êàìåðè ï³ä ÷àñ ïðèñ³äàííÿ (â³äíîñíî áàòüê³âñüêîãî îá'ºêòà, ñòàíäàðòíà âåðòèêàëüíà â³ñü).")]
    public float crouchCameraLocalY = -0.2f;

    [Header("Slide Settings")]
    [Tooltip("Äîçâîëèòè êîâçàííÿ ïðè ñïð³íò³?")]
    public bool allowSlide = true;
    [Tooltip("Äîäàòêîâà ñèëà, ùî çàñòîñîâóºòüñÿ íà ïî÷àòêó êîâçàííÿ")]
    public float slideForce = 15f;
    [Tooltip("Òðèâàë³ñòü êîâçàííÿ â ñåêóíäàõ")]
    public float slideDuration = 0.8f;
    [Tooltip("Âèñîòà Box Collider ï³ä ÷àñ êîâçàííÿ")]
    public float slideColliderHeight = 0.5f;
    [Tooltip("Êóëüäàóí ï³ñëÿ êîâçàííÿ â ñåêóíäàõ")]
    public float slideCooldown = 1f;

    [Header("Dash Settings")]
    [Tooltip("Êíîïêà äëÿ âèêîíàííÿ ðèâêà/ïåðåêàòó.")]
    public KeyCode dashKey = KeyCode.Q;
    [Tooltip("Äîçâîëèòè ðèâîê/ïåðåêàò?")]
    public bool allowDash = true;
    [Tooltip("Äîäàòêîâà ñèëà, ùî çàñòîñîâóºòüñÿ ï³ä ÷àñ ðèâêà.")]
    public float dashForce = 20f;
    [Tooltip("Òðèâàë³ñòü ðèâêà/ïåðåêàòó â ñåêóíäàõ.")]
    public float dashDuration = 0.25f;
    [Tooltip("Âèñîòà Box Collider ï³ä ÷àñ ðèâêà/ïåðåêàòó.")]
    public float dashColliderHeight = 0.5f;
    [Tooltip("Ëîêàëüíà Y-êîîðäèíàòà êàìåðè ï³ä ÷àñ ðèâêà (çàçâè÷àé äóæå íèçüêî, ÿê ïðè ïðèñ³äàíí³/êîâçàíí³).")]
    public float dashCameraLocalY = -0.25f;
    [Tooltip("Êóëüäàóí ï³ñëÿ ðèâêà/ïåðåêàòó â ñåêóíäàõ.")]
    public float dashCooldown = 1.5f;
    [Tooltip("Âàðò³ñòü ñòàì³íè çà îäèí ðèâîê.")]
    public float dashStaminaCost = 20f;
    [Tooltip("Ì³í³ìàëüíà ñòàì³íà, íåîáõ³äíà äëÿ âèêîíàííÿ ðèâêà.")]
    public float minStaminaToDash = 20f;
    [Tooltip("Ìíîæíèê øâèäêîñò³ äëÿ êîëàéäåðà òà êàìåðè ï³ä ÷àñ ïåðåõîäó äî/â³ä ðèâêà (äëÿ ïëàâíîñò³).")]
    public float dashSmoothSpeedMultiplier = 2.0f;
    [Tooltip("Çì³íà FOV êàìåðè ï³ä ÷àñ ðèâêà.")]
    public float dashFOVChange = 10f;

    [Header("Headbob Settings")]
    [Tooltip("Òðàíñôîðì êàìåðè äëÿ çàñòîñóâàííÿ õèòàííÿ ãîëîâè")]
    public Transform cameraTransform;
    [Tooltip("Áàçîâà àìïë³òóäà (âåëè÷èíà) êîëèâàííÿ ãîëîâè")]
    public float headbobAmplitude = 0.05f;
    [Tooltip("Áàçîâà ÷àñòîòà êîëèâàííÿ ãîëîâè (÷èì âèùå, òèì øâèäøå)")]
    public float headbobFrequency = 10f;
    [Tooltip("Ìíîæíèê ÷àñòîòè äëÿ ñïð³íòó")]
    public float headbobSprintFrequencyMultiplier = 1.5f;
    [Tooltip("Ìíîæíèê àìïë³òóäè äëÿ ñïð³íòó")]
    public float headbobSprintAmpMultiplier = 1.5f;
    [Tooltip("Ìíîæíèê ÷àñòîòè äëÿ ïðèñ³äàííÿ")]
    public float headbobCrouchFrequencyMultiplier = 0.7f;
    [Tooltip("Ìíîæíèê àìïë³òóäè äëÿ ïðèñ³äàííÿ")]
    public float headbobCrouchAmpMultiplier = 0.7f;


    [Header("Footstep Settings")]
    [Tooltip("Àóä³î äæåðåëî äëÿ â³äòâîðåííÿ çâóê³â êðîê³â")]
    public AudioSource audioSource;
    [Tooltip("Ñïèñîê çâóêîâèõ êë³ï³â äëÿ êðîê³â (âèáèðàºòüñÿ âèïàäêîâî)")]
    public List<AudioClip> footstepSounds;
    [Tooltip("×àñ ì³æ êðîêàìè ï³ä ÷àñ õîäüáè")]
    public float walkFootstepInterval = 0.5f;
    [Tooltip("×àñ ì³æ êðîêàìè ï³ä ÷àñ ñïð³íòó")]
    public float sprintFootstepInterval = 0.3f;
    [Tooltip("×àñ ì³æ êðîêàìè ï³ä ÷àñ ïðèñ³äàííÿ")]
    public float crouchFootstepInterval = 0.7f;
    [Tooltip("Ì³í³ìàëüíèé ïîð³ã øâèäêîñò³ äëÿ â³äòâîðåííÿ çâóê³â êðîê³â")]
    public float minFootstepSpeed = 0.1f;


    [Header("Ground Check")]
    [Tooltip("Îá'ºêò Transform, ÿêèé ðîçòàøîâàíèé á³ëÿ í³ã ãðàâöÿ äëÿ ïåðåâ³ðêè çåìë³")]
    public Transform groundCheck;
    [Tooltip("Ðîçì³ð êóáà äëÿ ïåðåâ³ðêè çåìë³ (ïîëîâèíí³ ðîçì³ðè). Íàïðèêëàä, 0.4, 0.1, 0.4 äëÿ êóáà 0.8õ0.2õ0.8.")]
    public Vector3 groundCheckHalfExtents = new Vector3(0.4f, 0.1f, 0.4f);
    [Tooltip("Øàð(è), ÿê³ ââàæàþòüñÿ çåìëåþ")]
    public LayerMask groundLayer;

    // --- Ïðèâàòí³ çì³íí³ ---
    private Rigidbody rb;
    private Camera playerCamera;
    private BoxCollider boxCollider;
    [SerializeField] private bool isGrounded; // Çðîáëåíî SerializeField äëÿ íàëàãîäæåííÿ

    // Çì³íí³ äëÿ ñòðèáê³â
    [SerializeField] private int jumpsRemaining;
    private float lastGroundedTime;
    private float lastJumpPressTime;

    // Çì³íí³ äëÿ ñïð³íòó
    [SerializeField] private float currentStamina;
    [SerializeField] private bool isSprinting;
    private float lastSprintTime;

    // Çì³íí³ äëÿ ïðèñ³äàííÿ
    [SerializeField] private bool isCrouching;
    private float initialColliderHeight;
    private Vector3 initialColliderCenter;
    private float initialCameraLocalY; // Çì³íåíî íà Y (ñòàíäàðòíà âåðòèêàëüíà â³ñü)

    // Çì³íí³ äëÿ êîâçàííÿ
    [SerializeField] private bool isSliding;
    private float slideStartTime;
    private float lastSlideTime;

    // Çì³íí³ äëÿ ðèâêà/ïåðåêàòó
    [SerializeField] private bool isDashing;
    private float dashStartTime;
    private float lastDashTime;

    // Çì³íí³ äëÿ Headbob
    private Vector3 originalCameraLocalPos;
    private float headbobTimer;

    // Çì³íí³ äëÿ êðîê³â
    private float nextFootstepTime;

    // Çì³íí³ äëÿ FOV
    private float defaultFOV;

    // Ïîñèëàííÿ íà PlayerHealth (äëÿ âèìêíåííÿ êîíòðîëþ ïðè ñìåðò³)
    private PlayerHealth playerHealth;

    // State Machine
    private StateMachine stateMachine;
    [Header("State Machine Debug")]
    [SerializeField] private StateMachineDebugInfo debugInfo = new StateMachineDebugInfo();
    [SerializeField] private bool useStateMachine = true;

    // Ïóáë³÷í³ âëàñòèâîñò³ äëÿ State Machine
    public StateMachine StateMachine => stateMachine;
    public bool IsGrounded => isGrounded;
    public bool CanSprint() => currentStamina >= minStaminaToSprint;
    public void DrainStamina() => HandleStaminaDrain();
    public void SetCrouchState(bool crouching) => SetCrouchingState(crouching);
    public void SetSlideState(bool sliding) => SetSlidingState(sliding);

    void Awake()
    {
        // Âàë³äàö³ÿ êðèòè÷íèõ êîìïîíåíò³â
        rb = GetComponent<Rigidbody>();
        if (!ValidationHelper.ValidateComponentCritical(rb, "Rigidbody", this)) return;
        rb.freezeRotation = true;

        boxCollider = GetComponent<BoxCollider>();
        if (!ValidationHelper.ValidateComponentCritical(boxCollider, "BoxCollider", this)) return;

        if (!ValidationHelper.ValidateComponentCritical(groundCheck, "Ground Check Transform", this)) return;

        // Çáåð³ãàºìî îðèã³íàëüí³ ïàðàìåòðè êîëàéäåðà
        initialColliderHeight = boxCollider.size.y;
        initialColliderCenter = boxCollider.center;

        // ²í³ö³àë³çóºìî ñòàìèíó òà ñòðèáêè
        currentStamina = maxStamina;
        jumpsRemaining = allowDoubleJump ? 2 : 1;

        // Çíàõîäèìî òà âàë³äóºìî êàìåðó
        InitializeCamera();

        // Çíàõîäèìî àóä³î êîìïîíåíò (íå êðèòè÷íèé)
        InitializeAudio();

        // Çíàõîäèìî PlayerHealth (íå êðèòè÷íèé, àëå âàæëèâèé)
        playerHealth = GetComponent<PlayerHealth>();
        if (!ValidationHelper.ValidateComponent(playerHealth, "PlayerHealth", this))
        {
            Debug.LogWarning("PlayerMovement: Ïåðåâ³ðêè ñìåðò³ íå ïðàöþâàòèìóòü.", this);
        }

        // Âàë³äóºìî ïàðàìåòðè
        ValidateParameters();

        // ²í³ö³àë³çóºìî State Machine
        InitializeStateMachine();
    }

    /// <summary>
    /// ²í³ö³àë³çóº êàìåðó òà ïîâ'ÿçàí³ êîìïîíåíòè
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
            Debug.LogError("PlayerMovement: Ôóíêö³¿ êàìåðè íå ïðàöþâàòèìóòü!", this);
        }
    }

    /// <summary>
    /// ²í³ö³àë³çóº àóä³î êîìïîíåíò
    /// </summary>
    void InitializeAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (!ValidationHelper.ValidateComponent(audioSource, "AudioSource", this))
        {
            Debug.LogWarning("PlayerMovement: Çâóêè êðîê³â íå áóäóòü â³äòâîðþâàòèñÿ.", this);
        }
    }

    /// <summary>
    /// Âàë³äóº òà âèïðàâëÿº ïàðàìåòðè êîìïîíåíòà
    /// </summary>
    void ValidateParameters()
    {
        walkSpeed = ValidationHelper.ValidateSpeed(walkSpeed, "Walk Speed");
        sprintSpeed = ValidationHelper.ValidateSpeed(sprintSpeed, "Sprint Speed");
        crouchSpeed = ValidationHelper.ValidateSpeed(crouchSpeed, "Crouch Speed");
        
        // Ïåðåêîíóºìîñÿ, ùî sprint øâèäøå çà walk
        if (sprintSpeed <= walkSpeed)
        {
            Debug.LogWarning("PlayerMovement: Sprint Speed ìàº áóòè á³ëüøå Walk Speed. Àâòîìàòè÷íî âèïðàâëåíî.");
            sprintSpeed = walkSpeed * 1.5f;
        }

        maxStamina = ValidationHelper.ValidateHealth(maxStamina, "Max Stamina");
        currentStamina = Mathf.Min(currentStamina, maxStamina);

        // Âàë³äóºìî ÷àñîâ³ ïàðàìåòðè
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);

        // Çàì³íÿºìî ìàã³÷í³ ÷èñëà êîíñòàíòàìè
        minFootstepSpeed = GameConstants.MIN_FOOTSTEP_SPEED;
        fallMultiplier = GameConstants.DEFAULT_GRAVITY_MULTIPLIER;
        lowJumpMultiplier = GameConstants.LOW_JUMP_MULTIPLIER;
        minSpeedToDrainStamina = GameConstants.MIN_SPEED_TO_DRAIN_STAMINA;
    }

    /// <summary>
    /// Âàë³äàö³ÿ ïàðàìåòð³â â Unity Editor (âèêëèêàºòüñÿ ïðè çì³í³ çíà÷åíü â Inspector)
    /// </summary>
    void OnValidate()
    {
        // Âàë³äóºìî øâèäêîñò³
        walkSpeed = Mathf.Max(GameConstants.MIN_SPEED, walkSpeed);
        sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
        crouchSpeed = Mathf.Max(GameConstants.MIN_SPEED, crouchSpeed);

        // Âàë³äóºìî ñòàìèíó
        maxStamina = Mathf.Max(GameConstants.MIN_HEALTH, maxStamina);
        staminaDrainRate = Mathf.Max(0f, staminaDrainRate);
        staminaRegenRate = Mathf.Max(0f, staminaRegenRate);
        staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
        minStaminaToSprint = Mathf.Max(0f, minStaminaToSprint);

        // Âàë³äóºìî ñòðèáêè
        jumpForce = Mathf.Max(0f, jumpForce);
        fallMultiplier = Mathf.Max(1f, fallMultiplier);
        lowJumpMultiplier = Mathf.Max(1f, lowJumpMultiplier);
        coyoteTime = Mathf.Max(0f, coyoteTime);
        jumpBufferTime = Mathf.Max(0f, jumpBufferTime);

        // Âàë³äóºìî êîëàéäåðè
        crouchColliderHeight = Mathf.Max(0.1f, crouchColliderHeight);
        slideColliderHeight = Mathf.Max(0.1f, slideColliderHeight);
        dashColliderHeight = Mathf.Max(0.1f, dashColliderHeight);

        // Âàë³äóºìî ÷àñîâ³ ïàðàìåòðè
        slideDuration = Mathf.Max(0.1f, slideDuration);
        slideCooldown = Mathf.Max(0f, slideCooldown);
        dashDuration = Mathf.Max(0.1f, dashDuration);
        dashCooldown = Mathf.Max(0f, dashCooldown);

        // Âàë³äóºìî headbob
        headbobAmplitude = Mathf.Max(0f, headbobAmplitude);
        headbobFrequency = Mathf.Max(0f, headbobFrequency);

        // Âàë³äóºìî çâóêè êðîê³â
        walkFootstepInterval = Mathf.Max(0.1f, walkFootstepInterval);
        sprintFootstepInterval = Mathf.Max(0.1f, sprintFootstepInterval);
        crouchFootstepInterval = Mathf.Max(0.1f, crouchFootstepInterval);
    }

    void OnDestroy()
    {
        // Î÷èùóºìî State Machine ïðè çíèùåíí³
        if (stateMachine != null)
        {
            stateMachine.Clear();
        }
    }

    /// <summary>
    /// ²í³ö³àë³çóº State Machine ç óñ³ìà ñòàíàìè
    /// </summary>
    void InitializeStateMachine()
    {
        if (!useStateMachine) return;

        stateMachine = new StateMachine(this);

        // Äîäàºìî âñ³ ñòàíè
        stateMachine.AddState(new IdleState(stateMachine, this));
        stateMachine.AddState(new WalkingState(stateMachine, this));
        stateMachine.AddState(new RunningState(stateMachine, this));
        stateMachine.AddState(new JumpingState(stateMachine, this));
        stateMachine.AddState(new FallingState(stateMachine, this));
        stateMachine.AddState(new CrouchingState(stateMachine, this));
        stateMachine.AddState(new SlidingState(stateMachine, this));

        // Çàïóñêàºìî ç ïî÷àòêîâîãî ñòàíó
        stateMachine.Start<IdleState>();

        Debug.Log("PlayerMovement: State Machine ³í³ö³àë³çîâàíî ç 7 ñòàíàìè");
    }

    /// <summary>
    /// Ðåàë³çàö³ÿ ³íòåðôåéñó IStateMachineOwner
    /// </summary>
    public void OnStateChanged(System.Type previousState, System.Type newState)
    {
        Debug.Log($"PlayerMovement: State changed from {previousState?.Name ?? "None"} to {newState.Name}");
    }

    /// <summary>
    /// Ìåòîäè äëÿ State Machine - óïðàâë³ííÿ ïðèñ³äàííÿì
    /// </summary>
    void SetCrouchingState(bool crouching)
    {
        isCrouching = crouching;
        // Ëîã³êà çì³íè êîëàéäåðà áóäå â UpdateColliderAndCamera
    }

    /// <summary>
    /// Ìåòîäè äëÿ State Machine - óïðàâë³ííÿ êîâçàííÿì
    /// </summary>
    void SetSlidingState(bool sliding)
    {
        isSliding = sliding;
        if (sliding)
        {
            lastSlideTime = Time.time;
        }
        // Ëîã³êà çì³íè êîëàéäåðà áóäå â UpdateColliderAndCamera
    }

    /// <summary>
    /// Ìåòîäè äëÿ State Machine - îáðîáêà ñòàì³íè
    /// </summary>
    void HandleStaminaDrain()
    {
        if (currentStamina > 0 && new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude > minSpeedToDrainStamina)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            
            // Â³äïðàâëÿºìî ïîä³þ çì³íè ñòàì³íè
            Events.Trigger(new StaminaChangedEvent(currentStamina, maxStamina));
        }
    }

    void Update()
    {
        // ßêùî ãðàâåöü ìåðòâèé, âèìèêàºìî âñ³ ä³¿, îêð³ì îíîâëåííÿ êîëàéäåðà/êàìåðè òà FOV
        if (playerHealth != null && playerHealth.IsDead())
        {
            HandleDeathState();
            return;
        }

        // Îíîâëþºìî State Machine (ÿêùî âèêîðèñòîâóºòüñÿ)
        if (useStateMachine && stateMachine != null)
        {
            stateMachine.Update();
            debugInfo.UpdateInfo(stateMachine);
        }

        // === Ïåðåâ³ðêà çåìë³ ===
        // Âèêîðèñòîâóºìî Physics.OverlapBox. Quaternion.identity îçíà÷àº áåç îáåðòàííÿ äëÿ êîðîáêè.
        isGrounded = Physics.OverlapBox(groundCheck.position, groundCheckHalfExtents, Quaternion.identity, groundLayer).Length > 0;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            if (!isSliding && !isDashing) // Ñêèäàºìî ñòðèáêè, ÿêùî íå êîâçàºìî ³ íå â ðèâêó
            {
                jumpsRemaining = allowDoubleJump ? 2 : 1;
            }
        }

        // === Îáðîáêà âõ³äíèõ äàíèõ äëÿ ñòðèáêà ===
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressTime = Time.time;
        }

        // === Ëîã³êà Ïåðåêàòó/Ðèâêà (Dash) ===
        // Ðèâîê ìàº íàéâèùèé ïð³îðèòåò.
        if (allowDash && Input.GetKeyDown(dashKey) && !isDashing && isGrounded && currentStamina >= minStaminaToDash && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
        else if (isDashing)
        {
            UpdateDash();
        }

        // === Ëîã³êà Ïðèñ³äàííÿ òà Êîâçàííÿ ===
        // Îáðîáëÿºìî ò³ëüêè ÿêùî ãðàâåöü íå â ðèâêó
        if (!isDashing)
        {
            bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

            // Êîâçàííÿ: ÿêùî äîçâîëåíî, ñïð³íòèìî, õî÷åìî ïðèñ³ñòè, íà çåìë³, êóëüäàóí ïðîéøîâ
            if (allowSlide && isSprinting && wantsToCrouch && isGrounded && Time.time >= lastSlideTime + slideCooldown)
            {
                StartSlide();
            }
            else if (isSliding)
            {
                UpdateSlide();
            }
            else // ßêùî íå êîâçàºìî, îáðîáëÿºìî çâè÷àéíå ïðèñ³äàííÿ
            {
                // Ïåðåìèêàííÿ ïðèñ³äàííÿ çà íàòèñêàííÿì
                if (Input.GetKeyDown(KeyCode.LeftControl))
                {
                    if (isCrouching) { TryStandUp(); }
                    else { isCrouching = true; }
                }
                // ßêùî ïðèñ³äàºìî ³ êíîïêó â³äïóñòèëè
                // (Ïðèì³òêà: öåé áëîê ñïðàöþº, ÿêùî âè õî÷åòå, ùîá ïðèñ³äàííÿ òðèìàëîñÿ, ïîêè òðèìàºòå êíîïêó)
                if (isCrouching && !wantsToCrouch)
                {
                    TryStandUp();
                }
            }
        }
        else // ßêùî â ðèâêó, ïðèìóñîâî âèìèêàºìî ïðèñ³äàííÿ, ñïð³íò, êîâçàííÿ
        {
            isCrouching = false;
            isSprinting = false;
            isSliding = false;
        }

        // === Ëîã³êà Ñïð³íòó òà Ñòàì³íè ===
        // Îáðîáëÿºìî ò³ëüêè ÿêùî íå â ðèâêó ³ íå êîâçàºìî
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
            isSprinting = false; // Âèõîäèìî ç³ ñïð³íòó, ÿêùî â êîâçàíí³ àáî ðèâêó
        }

        // === Îíîâëåííÿ ðîçì³ðó BoxCollider òà ïîçèö³¿ êàìåðè ===
        UpdateColliderAndCamera();

        // === Headbobing ===
        HandleHeadbob(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);

        // === Îíîâëåííÿ FOV êàìåðè ===
        UpdateCameraFOV();
    }

    void FixedUpdate()
    {
        // ßêùî ãðàâåöü ìåðòâèé, çóïèíÿºìî ðóõ ô³çèêè
        if (playerHealth != null && playerHealth.IsDead())
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // Âèêîðèñòîâóºìî State Machine äëÿ ô³çèêè (ÿêùî óâ³ìêíåíî)
        if (useStateMachine && stateMachine != null)
        {
            stateMachine.FixedUpdate();
            return; // State Machine êåðóº ðóõîì
        }

        // === Ðóõ ãðàâöÿ ===
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (playerCamera == null) {
            Debug.LogWarning("PlayerMovement: playerCamera íå ïðèçíà÷åíà â FixedUpdate. Ðóõ ìîæå áóòè íåêîðåêòíèì.", this);
            return;
        }

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0; // Äëÿ ðóõó ïî ãîðèçîíòàëüí³é ïëîùèí³
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredMoveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        float currentTargetSpeed = walkSpeed; // Áàçîâà øâèäê³ñòü

        // Âèçíà÷àºìî ïîòî÷íó ö³ëüîâó øâèäê³ñòü íà îñíîâ³ ñòàíó ãðàâöÿ
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
        
        // Îáìåæåííÿ øâèäêîñò³, ÿêùî ñòàì³íà âè÷åðïàíà
        if (currentStamina <= 0 && isSprinting && !isSliding && !isDashing) {
            currentTargetSpeed = walkSpeed;
        }


        // Âðàõîâóºìî êîíòðîëü ó ïîâ³òð³
        float currentAccelerationFactor = isGrounded ? groundAcceleration : airAcceleration; // Çì³íåíî íà airAcceleration
        float currentDecelerationFactor = isGrounded ? groundDeceleration : airControl; // AirControl âèêîðèñòîâóºòüñÿ ÿê decelerator

        Vector3 targetVelocity = desiredMoveDirection * currentTargetSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        Vector3 newHorizontalVelocity = currentHorizontalVelocity;

        // --- Ïîêðàùåíèé êîíòðîëü ó ïîâ³òð³ ---
        if (isDashing || isSliding)
        {
            // Ï³ä ÷àñ ðèâêà àáî êîâçàííÿ, îñíîâíèé ðóõ - öå ³ìïóëüñ. WASD íå ñèëüíî âïëèâàº.
            // Ìîæíà äîäàòè íåâåëèêå "ï³äðóëþâàííÿ" òóò.
            // newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, airControl * Time.fixedDeltaTime);
        }
        else if (desiredMoveDirection.magnitude > 0.1f) // ßêùî º âõ³äí³ äàí³ äëÿ ðóõó
        {
            newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, currentAccelerationFactor * Time.fixedDeltaTime);
        }
        else // ßêùî íåìàº âõ³äíèõ äàíèõ äëÿ ðóõó, ïîñòóïîâî çóïèíÿºìî
        {
            // Íà çåìë³ çóïèíÿºìîñÿ øâèäêî, ó ïîâ³òð³ - ïîâ³ëüí³øå
            if (isGrounded)
            {
                newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, currentDecelerationFactor * Time.fixedDeltaTime);
            }
            else // Ó ïîâ³òð³ ëèøå çìåíøóºìî ³íåðö³þ, ÿêùî íåìàº ³íøîãî íàïðÿìêó
            {
                newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, airControl * Time.fixedDeltaTime * 0.5f); // Çìåíøóºìî øâèäê³ñòü ïîâ³ëüí³øå
            }
        }
        
        rb.velocity = new Vector3(newHorizontalVelocity.x, rb.velocity.y, newHorizontalVelocity.z);


        // === Îáðîáêà ñòðèáêà ó FixedUpdate (äëÿ ô³çèêè) ===
        bool canJump = (Time.time - lastJumpPressTime < jumpBufferTime) &&
                       ((Time.time - lastGroundedTime < coyoteTime) || (jumpsRemaining > 0));

        if (canJump && !isCrouching && !isSliding && !isDashing) // Íå ìîæíà ñòðèáàòè ï³ä ÷àñ ïðèñ³äàííÿ, êîâçàííÿ àáî ðèâêà
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Îáíóëÿºìî âåðòèêàëüíó øâèäê³ñòü
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // Äîäàºìî ³ìïóëüñ

            lastJumpPressTime = 0;
            lastGroundedTime = 0; // Ñêèäàºìî êîéîò-òàéì ï³ñëÿ ñòðèáêà
            
            // ßêùî ïîäâ³éíèé ñòðèáîê àáî ÿêùî âæå â ïîâ³òð³
            if (!isGrounded && jumpsRemaining > 0)
            {
                jumpsRemaining--;
                Debug.Log("Double Jump! Jumps Remaining: " + jumpsRemaining);
            }
            else if (isGrounded) // ßêùî ñòðèáîê ç çåìë³
            {
                jumpsRemaining = allowDoubleJump ? 1 : 0; // Çàëèøàºìî îäèí, ÿêùî ïîäâ³éíèé äîçâîëåíî
                Debug.Log("Ground Jump! Jumps Remaining: " + jumpsRemaining);
            }
        }

        // === Çì³ííà âèñîòà ñòðèáêà òà øâèäêå ïàä³ííÿ ===
        // Äîäàòêîâà ãðàâ³òàö³ÿ äëÿ ïàä³ííÿ
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Ìåíøà âèñîòà ñòðèáêà, ÿêùî êíîïêó â³äïóñòèëè
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // === Îáðîáêà çâóê³â êðîê³â ===
        HandleFootsteps(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude);
    }

    // --- Äîïîì³æí³ ìåòîäè ---

    /// <summary>
    /// Îáðîáëÿº ñòàí, êîëè ãðàâåöü ìåðòâèé.
    /// </summary>
    private void HandleDeathState()
    {
        // ßêùî ãðàâåöü ìåðòâèé, çàáåçïå÷óºìî, ùî â³í íå ðóõàºòüñÿ ³ íå âçàºìîä³º.
        // Çì³íþºìî ëèøå â³çóàëüí³ àñïåêòè (êîëàéäåð, êàìåðà, FOV).
        isCrouching = false;
        isSprinting = false;
        isSliding = false;
        isDashing = false;
        
        UpdateColliderAndCamera(); // Îíîâëþºìî, ùîá ãðàâåöü ïîâåðíóâñÿ ó ñòîÿ÷å ïîëîæåííÿ
        UpdateCameraFOV(); // Îíîâëþºìî, ùîá FOV ïîâåðíóâñÿ äî ñòàíäàðòíîãî
    }


    /// <summary>
    /// Îíîâëþº âèñîòó BoxCollider òà Y-ïîçèö³þ êàìåðè ïðè ïðèñ³äàíí³/ñòîÿíí³/êîâçàíí³/ðèâêó.
    /// Êàìåðà çì³ùóºòüñÿ ïî ëîêàëüí³é îñ³ Y (ñòàíäàðòíà âåðòèêàëüíà â³ñü).
    /// </summary>
    private void UpdateColliderAndCamera()
    {
        if (boxCollider == null || cameraTransform == null) return;

        float targetColliderHeight;
        float targetCameraLocalY;
        float currentSmoothSpeed = crouchSmoothSpeed;

        // Ïð³îðèòåò ñòàí³â: Dash > Slide > Crouch > Stand
        if (isDashing)
        {
            targetColliderHeight = dashColliderHeight;
            targetCameraLocalY = dashCameraLocalY;
            currentSmoothSpeed *= dashSmoothSpeedMultiplier;
        }
        else if (isSliding)
        {
            targetColliderHeight = slideColliderHeight;
            targetCameraLocalY = crouchCameraLocalY; // Ïðè êîâçàíí³ êàìåðà íà ò³é æå âèñîò³, ùî é ïðè ïðèñ³äàíí³
        }
        else if (isCrouching)
        {
            targetColliderHeight = crouchColliderHeight;
            targetCameraLocalY = crouchCameraLocalY;
        }
        else // Ñòî¿ìî
        {
            targetColliderHeight = initialColliderHeight;
            targetCameraLocalY = initialCameraLocalY;
        }

        // Îá÷èñëþºìî ö³ëüîâèé öåíòð äëÿ BoxCollider, ùîá éîãî íèæíÿ ÷àñòèíà çàëèøàëàñÿ íà ì³ñö³.
        // (Ïðèïóñêàºìî, ùî pivot ãðàâöÿ çíàõîäèòüñÿ á³ëÿ îñíîâè êîëàéäåðà àáî íà 0,0,0)
        Vector3 targetColliderCenter = new Vector3(initialColliderCenter.x,
                                                   initialColliderCenter.y - (initialColliderHeight - targetColliderHeight) / 2f,
                                                   initialColliderCenter.z);

        // Ïëàâíà çì³íà ðîçì³ðó òà öåíòðó êîëàéäåðà
        boxCollider.size = Vector3.Lerp(boxCollider.size, new Vector3(boxCollider.size.x, targetColliderHeight, boxCollider.size.z), currentSmoothSpeed * Time.deltaTime);
        boxCollider.center = Vector3.Lerp(boxCollider.center, targetColliderCenter, currentSmoothSpeed * Time.deltaTime);

        // Ïëàâíà çì³íà ïîçèö³¿ êàìåðè ïî ëîêàëüí³é îñ³ Y (äëÿ "âåðòèêàëüíîãî" ïðèñ³äàííÿ/ðèâêà)
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition,
                                                    new Vector3(cameraTransform.localPosition.x, targetCameraLocalY, cameraTransform.localPosition.z),
                                                    currentSmoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Ñïðîáà âñòàòè ç ïðèñ³äàííÿ. Ïåðåâ³ðÿº, ÷è º äîñòàòíüî ì³ñöÿ íàä ãðàâöåì çà äîïîìîãîþ BoxCast.
    /// </summary>
    private void TryStandUp()
    {
        if (boxCollider == null) return;

        // Ïîòî÷í³ ðîçì³ðè
        float currentColliderHeight = boxCollider.size.y;
        Vector3 currentColliderCenter = boxCollider.center;

        // ßêùî âæå ñòî¿ìî, òî í³÷îãî íå ðîáèìî
        if (Mathf.Abs(currentColliderHeight - initialColliderHeight) < 0.01f)
        {
            isCrouching = false; // Ïåðåêîíàºìîñü, ùî ñòàí ïðàâèëüíèé
            return;
        }

        // Îá÷èñëþºìî ïî÷àòêîâó òî÷êó BoxCast (â³ä ïîòî÷íî¿ âåðõ³âêè êîëàéäåðà)
        // òà â³äñòàíü äî ö³ëüîâî¿ ïîâíî¿ âèñîòè.
        Vector3 currentColliderWorldTop = transform.position + transform.TransformVector(currentColliderCenter + Vector3.up * (currentColliderHeight / 2f));
        float checkDistance = initialColliderHeight - currentColliderHeight;
        
        // Ðîçì³ðè êîðîáêè äëÿ ïåðåâ³ðêè (áåðåìî ðîçì³ðè ñòîÿ÷îãî ãðàâöÿ)
        // Çàñòîñîâóºìî ðîçì³ðè ñòîÿ÷îãî ãðàâöÿ, ùîá ïåðåâ³ðèòè, ÷è ïîì³ñòèòüñÿ â³í ïîâí³ñòþ.
        Vector3 checkHalfExtents = new Vector3(boxCollider.size.x / 2f, (initialColliderHeight / 2f), boxCollider.size.z / 2f); // Ïîëîâèíà âèñîòè ñòîÿ÷î¿ ìîäåë³

        // Ñì³ùåííÿ, ùîá BoxCast ïî÷èíàâñÿ â³ä öåíòðó íîâî¿ (ñòîÿ÷î¿) âèñîòè.
        // Öå ãàðàíòóº, ùî ìè ïåðåâ³ðÿºìî ïðîñò³ð, ÿêèé áóäå çàéíÿòèé.
        Vector3 boxCastOrigin = transform.position + transform.TransformVector(initialColliderCenter);

        // Âèêîíóºìî BoxCast, ³ãíîðóþ÷è âëàñí³ êîëëàéäåðè ãðàâöÿ
        // RaycastHit hit; // Äëÿ â³äëàäêè
        if (Physics.BoxCast(boxCastOrigin, checkHalfExtents, Vector3.up, transform.rotation, 0.1f, groundLayer, QueryTriggerInteraction.Ignore)) // Çðîáëåíî êîðîòêèé êàñò 0.1f
        {
            // Debug.Log($"Cannot stand up: Obstruction detected above player by {hit.collider.name}"); // Â³äëàäêîâå ïîâ³äîìëåííÿ
        }
        else
        {
            isCrouching = false;
            Debug.Log("Player stood up.");
        }
    }

    /// <summary>
    /// Çàïóñêàº ìåõàí³êó êîâçàííÿ.
    /// </summary>
    private void StartSlide()
    {
        isSliding = true;
        isCrouching = true; // Êîâçàííÿ àâòîìàòè÷íî ïåðåõîäèòü ó ñòàí ïðèñ³äàííÿ (â³çóàëüíî)
        slideStartTime = Time.time;
        lastSlideTime = Time.time;

        Vector3 slideDirection = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        if (slideDirection.magnitude < 0.1f) // ßêùî ñòî¿ìî, êîâçàºìî âïåðåä
        {
            slideDirection = playerCamera.transform.forward;
            slideDirection.y = 0;
            slideDirection.Normalize();
        }
        
        rb.velocity = new Vector3(0, rb.velocity.y, 0); // Ñêèäàºìî ïîòî÷íó ãîðèçîíòàëüíó øâèäê³ñòü
        rb.AddForce(slideDirection * slideForce, ForceMode.Impulse);

        currentStamina = Mathf.Max(0, currentStamina - (staminaDrainRate * slideDuration / 2f)); // Âèòðà÷àºìî ÷àñòèíó ñòàì³íè
        Debug.Log("Player Started Sliding!");
    }

    /// <summary>
    /// Îíîâëþº ñòàí êîâçàííÿ. Çìåíøóº øâèäê³ñòü ç ÷àñîì.
    /// </summary>
    private void UpdateSlide()
    {
        if (Time.time - slideStartTime >= slideDuration)
        {
            EndSlide();
            return;
        }

        // Ïëàâíî çìåíøóºìî øâèäê³ñòü êîâçàííÿ.
        // Çàáåçïå÷óºìî, ùî ãðàâåöü ìàº õî÷ ÿêèéñü êîíòðîëü íàä ðóõîì, íàâ³òü ï³ä ÷àñ êîâçàííÿ.
        float slideProgress = (Time.time - slideStartTime) / slideDuration;
        float currentTargetHorizontalSpeed = Mathf.Lerp(slideForce, crouchSpeed, slideProgress);

        // Îáìåæóºìî øâèäê³ñòü, ùîá íå áóëî ïðèñêîðåííÿ, ÿêùî ãðàâåöü íàìàãàºòüñÿ ðóõàòèñÿ øâèäøå, í³æ êîâçàííÿ.
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (currentHorizontalVelocity.magnitude > currentTargetHorizontalSpeed + 0.1f) // ßêùî øâèäê³ñòü âèùà çà ö³ëüîâó êîâçàííÿ
        {
            rb.velocity = Vector3.Lerp(currentHorizontalVelocity, currentHorizontalVelocity.normalized * currentTargetHorizontalSpeed, Time.deltaTime * slideForce);
        }
    }

    /// <summary>
    /// Çàâåðøóº ìåõàí³êó êîâçàííÿ.
    /// </summary>
    private void EndSlide()
    {
        if (!isSliding) return; // Çàïîá³ãàºìî ïîâòîðíîìó âèêëèêó
        isSliding = false;
        // Ï³ñëÿ êîâçàííÿ, ÿêùî êíîïêó ïðèñ³äàííÿ â³äïóñòèëè, ñïðîáóºìî âñòàòè.
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            TryStandUp();
        }
        isSprinting = false; // Âèõîäèìî ç³ ñïð³íòó
        Debug.Log("Player Ended Sliding!");
    }

    /// <summary>
    /// Çàïóñêàº ìåõàí³êó ðèâêà/ïåðåêàòó.
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

        // Îáíóëÿºìî ïîòî÷íó ãîðèçîíòàëüíó øâèäê³ñòü äëÿ ÷èñòîãî ³ìïóëüñó ðèâêà
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        Debug.Log("Player Started Dashing!");
    }

    /// <summary>
    /// Îíîâëþº ñòàí ðèâêà/ïåðåêàòó.
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
    /// Çàâåðøóº ìåõàí³êó ðèâêà/ïåðåêàòó.
    /// </summary>
    private void EndDash()
    {
        if (!isDashing) return; // Çàïîá³ãàºìî ïîâòîðíîìó âèêëèêó
        isDashing = false;
        // Ï³ñëÿ ðèâêà ãðàâåöü ïîâåðòàºòüñÿ â ñòîÿ÷å ïîëîæåííÿ (àáî ïðèñ³äàº, ÿêùî óòðèìóºòüñÿ êíîïêà ïðèñ³äàííÿ)
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            TryStandUp();
        }
        isSprinting = false;
        Debug.Log("Player Ended Dashing!");
    }


    /// <summary>
    /// Îáðîáëÿº åôåêò õèòàííÿ ãîëîâè (headbob).
    /// </summary>
    /// <param name="currentSpeed">Ïîòî÷íà ãîðèçîíòàëüíà øâèäê³ñòü ãðàâöÿ.</param>
    private void HandleHeadbob(float currentSpeed)
    {
        if (cameraTransform == null) return;

        if (isGrounded && currentSpeed > minFootstepSpeed && !isDashing) // Headbob íå ïðàöþº ï³ä ÷àñ ðèâêà
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

            // Õèòàííÿ ïî îñ³ X (á³ê-â-á³ê) òà Y (âãîðó-âíèç) - ñòàíäàðòí³ ëîêàëüí³ îñ³ êàìåðè
            float bobX = Mathf.Cos(headbobTimer * 0.5f) * bobAmplitude * 0.5f; // Ãîðèçîíòàëüíå õèòàííÿ (ëîêàëüíà X)
            float bobY = Mathf.Sin(headbobTimer) * bobAmplitude; // Îñíîâíå âåðòèêàëüíå êîëèâàííÿ êàìåðè (ëîêàëüíà Y)

            cameraTransform.localPosition = new Vector3(
                originalCameraLocalPos.x + bobX,
                cameraTransform.localPosition.y + bobY, // Äîäàºìî äî ïîòî÷íî¿ ëîêàëüíî¿ Y (ÿêà âæå âðàõîâóº ïðèñ³äàííÿ/ñòîÿííÿ)
                originalCameraLocalPos.z // Ëîêàëüíà Z çàëèøàºòüñÿ áàçîâîþ (öå âàøà "âïåðåä" â³ñü äëÿ êàìåðè)
            );
        }
        else // ßêùî íå ðóõàºìîñü, â ïîâ³òð³ àáî â ðèâêó, ïîâåðòàºìî êàìåðó äî ïî÷àòêîâî¿ ïîçèö³¿
        {
            headbobTimer = 0; // Ñêèäàºìî òàéìåð

            // Ïîâåðòàºìî X òà Z äî ïî÷àòêîâèõ, Y (âåðòèêàëü) çàëèøàºòüñÿ â³ä ïðèñ³äàííÿ/ñòîÿííÿ
            Vector3 targetBobPos = new Vector3(originalCameraLocalPos.x, cameraTransform.localPosition.y, originalCameraLocalPos.z);
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetBobPos, Time.deltaTime * headbobFrequency);
        }
    }

    /// <summary>
    /// Â³äòâîðþº çâóêè êðîê³â ãðàâöÿ.
    /// </summary>
    /// <param name="moveMagnitude">Ïîòî÷íà ãîðèçîíòàëüíà øâèäê³ñòü ãðàâöÿ.</param>
    private void HandleFootsteps(float moveMagnitude)
    {
        if (isDashing || isSliding) return; // Íå â³äòâîðþºìî çâóêè ï³ä ÷àñ ðèâêà/êîâçàííÿ (ìîæíà äîäàòè îêðåì³ çâóêè äëÿ íèõ)

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
    /// Îíîâëþº ïîëå çîðó (FOV) êàìåðè.
    /// </summary>
    private void UpdateCameraFOV()
    {
        if (playerCamera == null) return;

        float targetFOV = defaultFOV;
        
        // FOV ïðè ñïð³íò³ òà ðèâêó
        if (isSprinting)
        {
            targetFOV = defaultFOV + dashFOVChange; // Âèêîðèñòîâóºìî FOV â³ä ðèâêà äëÿ ñïð³íòó
        }
        if (isDashing)
        {
            targetFOV = defaultFOV + dashFOVChange * 1.5f; // Á³ëüøèé FOV äëÿ ðèâêà
        }
        // ßêùî º àêòèâíèé WeaponController ³ â³í ïðèö³ëåíèé, éîãî FOV ìàº ïð³îðèòåò
        // (Äëÿ öüîãî WeaponController ïîâèíåí áóòè äîñòóïíèé òóò àáî ìàòè ñòàòè÷í³ ïîëÿ FOV)
        // Íàðàç³ PlayerMovement íå çíàº ïðî ñòàí ïðèö³ëþâàííÿ çáðî¿, òîìó FOV ò³ëüêè â³ä ðóõó.
        // ßêùî âè õî÷åòå, ùîá FOV ïðè ïðèö³ëþâàíí³ "ïåðåáèâàâ" FOV ïðè ñïð³íò³,
        // òî ëîã³êà FOV ìàº áóòè â WeaponController àáî öåíòðàë³çîâàíîìó ñêðèïò³ êàìåðè.
        
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * dashSmoothSpeedMultiplier * 2f);
    }


    // --- Ïóáë³÷í³ ìåòîäè äëÿ äîñòóïó ç ³íøèõ ñêðèïò³â (íàïðèêëàä, äëÿ UI) ---

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

    // Äëÿ â³çóàë³çàö³¿ GroundCheck â ðåäàêòîð³
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckHalfExtents * 2f);
    }
    
}