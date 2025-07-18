using UnityEngine;

namespace IndieShooter.Player
{
    /// <summary>
    /// Основний контролер руху гравця. Координує роботу всіх підсистем руху.
    /// Замінює монолітний PlayerMovement.cs (1,066 рядків) на модульну архітектуру.
    /// </summary>
    public class PlayerMovementCore : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Швидкість пересування гравця під час ходьби")]
        public float walkSpeed = 5f;
        [Tooltip("Швидкість пересування гравця під час бігу/спринту")]
        public float sprintSpeed = 10f;
        [Tooltip("Наскільки швидко гравець розганяється до максимальної швидкості на землі")]
        public float groundAcceleration = 10f;
        [Tooltip("Наскільки швидко гравець зупиняється на землі")]
        public float groundDeceleration = 10f;
        [Tooltip("Контроль гравця у повітрі (0 = немає, 1 = повний)")]
        [Range(0f, 1f)]
        public float airControl = 0.5f;
        [Tooltip("Швидкість, з якою гравець може змінити напрямок у повітрі")]
        public float airAcceleration = 5f;

        [Header("Component References")]
        public Transform playerCamera;
        public Transform groundCheck;
        public LayerMask groundMask = 1;
        
        // Компоненти підсистем
        private PlayerJumping jumpingSystem;
        private PlayerStamina staminaSystem;
        private PlayerFootsteps footstepsSystem;
        private PlayerStates statesSystem;
        private PlayerPhysics physicsSystem;
        
        // Основні компоненти Unity
        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        
        // Поточний стан руху
        private Vector3 moveInput;
        private Vector3 currentVelocity;
        private bool isGrounded;
        
        void Awake()
        {
            InitializeComponents();
            InitializeSubsystems();
        }
        
        void Start()
        {
            ValidateSetup();
        }
        
        void Update()
        {
            HandleInput();
            UpdateSubsystems();
        }
        
        void FixedUpdate()
        {
            UpdatePhysics();
        }
        
        /// <summary>
        /// Ініціалізує основні Unity компоненти
        /// </summary>
        private void InitializeComponents()
        {
            rb = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            
            if (rb == null)
            {
                Debug.LogError("PlayerMovementCore: Rigidbody component not found!", this);
                enabled = false;
                return;
            }
            
            // Налаштування Rigidbody
            rb.freezeRotation = true;
            rb.useGravity = true;
        }
        
        /// <summary>
        /// Ініціалізує підсистеми руху
        /// </summary>
        private void InitializeSubsystems()
        {
            // Отримуємо або створюємо підсистеми
            jumpingSystem = GetComponent<PlayerJumping>();
            staminaSystem = GetComponent<PlayerStamina>();
            footstepsSystem = GetComponent<PlayerFootsteps>();
            statesSystem = GetComponent<PlayerStates>();
            physicsSystem = GetComponent<PlayerPhysics>();
            
            // Ініціалізуємо підсистеми з посиланням на основний контролер
            if (jumpingSystem != null) jumpingSystem.Initialize(this);
            if (staminaSystem != null) staminaSystem.Initialize(this);
            if (footstepsSystem != null) footstepsSystem.Initialize(this);
            if (statesSystem != null) statesSystem.Initialize(this);
            if (physicsSystem != null) physicsSystem.Initialize(this);
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (playerCamera == null)
                Debug.LogWarning("PlayerMovementCore: Player camera not assigned!", this);
                
            if (groundCheck == null)
                Debug.LogWarning("PlayerMovementCore: Ground check transform not assigned!", this);
        }
        
        /// <summary>
        /// Обробляє введення від гравця
        /// </summary>
        private void HandleInput()
        {
            // Отримуємо введення руху
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector3(horizontal, 0, vertical).normalized;
            
            // Передаємо введення підсистемам
            if (jumpingSystem != null) jumpingSystem.HandleInput();
            if (staminaSystem != null) staminaSystem.HandleInput();
            if (statesSystem != null) statesSystem.HandleInput();
        }
        
        /// <summary>
        /// Оновлює всі підсистеми
        /// </summary>
        private void UpdateSubsystems()
        {
            // Оновлюємо стан землі
            if (physicsSystem != null)
            {
                isGrounded = physicsSystem.CheckGrounded();
            }
            
            // Оновлюємо підсистеми
            if (staminaSystem != null) staminaSystem.UpdateStamina();
            if (statesSystem != null) statesSystem.UpdateStates();
            if (footstepsSystem != null) footstepsSystem.UpdateFootsteps();
        }
        
        /// <summary>
        /// Оновлює фізику руху
        /// </summary>
        private void UpdatePhysics()
        {
            if (rb == null) return;
            
            // Отримуємо поточну швидкість руху
            float currentSpeed = GetCurrentMoveSpeed();
            
            // Застосовуємо рух
            ApplyMovement(currentSpeed);
            
            // Оновлюємо фізичні ефекти
            if (physicsSystem != null) physicsSystem.UpdatePhysics();
        }
        
        /// <summary>
        /// Отримує поточну швидкість руху з урахуванням всіх модифікаторів
        /// </summary>
        private float GetCurrentMoveSpeed()
        {
            float baseSpeed = walkSpeed;
            
            // Модифікатори швидкості від підсистем
            if (staminaSystem != null && staminaSystem.IsSprinting())
            {
                baseSpeed = sprintSpeed;
            }
            
            if (statesSystem != null)
            {
                baseSpeed *= statesSystem.GetSpeedModifier();
            }
            
            return baseSpeed;
        }
        
        /// <summary>
        /// Застосовує рух до Rigidbody
        /// </summary>
        private void ApplyMovement(float speed)
        {
            if (moveInput.magnitude < 0.1f)
            {
                // Зупинка
                ApplyDeceleration();
                return;
            }
            
            // Обчислюємо напрямок руху відносно камери
            Vector3 moveDirection = CalculateMoveDirection();
            
            // Застосовуємо рух
            if (isGrounded)
            {
                ApplyGroundMovement(moveDirection, speed);
            }
            else
            {
                ApplyAirMovement(moveDirection, speed);
            }
        }
        
        /// <summary>
        /// Обчислює напрямок руху відносно камери
        /// </summary>
        private Vector3 CalculateMoveDirection()
        {
            if (playerCamera == null) return transform.TransformDirection(moveInput);
            
            Vector3 forward = playerCamera.forward;
            Vector3 right = playerCamera.right;
            
            // Проектуємо на горизонтальну площину
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            return forward * moveInput.z + right * moveInput.x;
        }
        
        /// <summary>
        /// Застосовує рух на землі
        /// </summary>
        private void ApplyGroundMovement(Vector3 direction, float speed)
        {
            Vector3 targetVelocity = direction * speed;
            Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            
            Vector3 velocityChange = Vector3.MoveTowards(
                currentHorizontalVelocity, 
                targetVelocity, 
                groundAcceleration * Time.fixedDeltaTime
            );
            
            rb.velocity = new Vector3(velocityChange.x, rb.velocity.y, velocityChange.z);
        }
        
        /// <summary>
        /// Застосовує рух у повітрі
        /// </summary>
        private void ApplyAirMovement(Vector3 direction, float speed)
        {
            Vector3 targetVelocity = direction * speed * airControl;
            Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            
            Vector3 velocityChange = Vector3.MoveTowards(
                currentHorizontalVelocity, 
                targetVelocity, 
                airAcceleration * Time.fixedDeltaTime
            );
            
            rb.velocity = new Vector3(velocityChange.x, rb.velocity.y, velocityChange.z);
        }
        
        /// <summary>
        /// Застосовує уповільнення при відсутності введення
        /// </summary>
        private void ApplyDeceleration()
        {
            Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            float deceleration = isGrounded ? groundDeceleration : airAcceleration * 0.5f;
            
            Vector3 newVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity, 
                Vector3.zero, 
                deceleration * Time.fixedDeltaTime
            );
            
            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
        }
        
        // === Публічні методи для доступу з підсистем ===
        
        public Rigidbody GetRigidbody() => rb;
        public CapsuleCollider GetCapsuleCollider() => capsuleCollider;
        public Transform GetPlayerCamera() => playerCamera;
        public Transform GetGroundCheck() => groundCheck;
        public LayerMask GetGroundMask() => groundMask;
        public Vector3 GetMoveInput() => moveInput;
        public bool IsGrounded() => isGrounded;
        public Vector3 GetCurrentVelocity() => rb != null ? rb.velocity : Vector3.zero;
        
        // === Методи для підсистем ===
        
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (rb != null) rb.AddForce(force, mode);
        }
        
        public void SetVelocity(Vector3 velocity)
        {
            if (rb != null) rb.velocity = velocity;
        }
        
        public void SetVelocityY(float y)
        {
            if (rb != null) rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
        }
    }
}