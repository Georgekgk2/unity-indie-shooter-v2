using UnityEngine;

namespace IndieShooter.Player
{
    /// <summary>
    /// Система стрибків гравця. Обробляє всю логіку стрибків, включаючи подвійні стрибки,
    /// coyote time, jump buffer та модифікатори гравітації.
    /// </summary>
    public class PlayerJumping : MonoBehaviour
    {
        [Header("Jump Settings")]
        [Tooltip("Сила, з якою гравець стрибає")]
        public float jumpForce = 8f;
        [Tooltip("Дозволити подвійний стрибок?")]
        public bool allowDoubleJump = false;
        [Tooltip("Додатковий множник сили тяжіння, коли гравець падає")]
        public float fallMultiplier = 2.5f;
        [Tooltip("Множник сили тяжіння, якщо кнопка стрибка відпускається раніше")]
        public float lowJumpMultiplier = 2f;
        [Tooltip("Час (у секундах), протягом якого гравець може стрибнути після того, як залишив землю")]
        public float coyoteTime = 0.1f;
        [Tooltip("Час (у секундах), протягом якого натискання кнопки стрибка 'зберігається'")]
        public float jumpBufferTime = 0.15f;
        
        // Посилання на основний контролер
        private PlayerMovementCore movementCore;
        private Rigidbody rb;
        
        // Стан стрибків
        private bool hasDoubleJumped = false;
        private float lastGroundedTime = 0f;
        private float lastJumpInputTime = 0f;
        private bool isJumping = false;
        private bool jumpInputHeld = false;
        
        // Кеш для оптимізації
        private float originalGravity;
        
        public void Initialize(PlayerMovementCore core)
        {
            movementCore = core;
            rb = core.GetRigidbody();
            
            if (rb != null)
            {
                originalGravity = Physics.gravity.y;
            }
        }
        
        void Start()
        {
            ValidateSetup();
        }
        
        void Update()
        {
            UpdateJumpTimers();
            ApplyGravityModifiers();
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (movementCore == null)
            {
                Debug.LogError("PlayerJumping: MovementCore not initialized!", this);
                enabled = false;
                return;
            }
            
            if (rb == null)
            {
                Debug.LogError("PlayerJumping: Rigidbody not found!", this);
                enabled = false;
                return;
            }
        }
        
        /// <summary>
        /// Обробляє введення стрибка
        /// </summary>
        public void HandleInput()
        {
            // Обробка введення стрибка
            if (Input.GetKeyDown(KeyCode.Space))
            {
                lastJumpInputTime = Time.time;
                TryJump();
            }
            
            // Відстеження утримання кнопки стрибка
            jumpInputHeld = Input.GetKey(KeyCode.Space);
            
            // Відпускання кнопки стрибка для низьких стрибків
            if (Input.GetKeyUp(KeyCode.Space) && isJumping && rb.velocity.y > 0)
            {
                ApplyLowJumpModifier();
            }
        }
        
        /// <summary>
        /// Оновлює таймери для coyote time та jump buffer
        /// </summary>
        private void UpdateJumpTimers()
        {
            // Оновлюємо час останнього контакту з землею
            if (movementCore.IsGrounded())
            {
                lastGroundedTime = Time.time;
                hasDoubleJumped = false;
                isJumping = false;
            }
            
            // Перевіряємо jump buffer
            if (Time.time - lastJumpInputTime <= jumpBufferTime && CanJump())
            {
                PerformJump();
            }
        }
        
        /// <summary>
        /// Спробує виконати стрибок
        /// </summary>
        private void TryJump()
        {
            if (CanJump())
            {
                PerformJump();
            }
        }
        
        /// <summary>
        /// Перевіряє, чи може гравець стрибнути
        /// </summary>
        private bool CanJump()
        {
            // Звичайний стрибок (coyote time)
            if (Time.time - lastGroundedTime <= coyoteTime)
            {
                return true;
            }
            
            // Подвійний стрибок
            if (allowDoubleJump && !hasDoubleJumped && !movementCore.IsGrounded())
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Виконує стрибок
        /// </summary>
        private void PerformJump()
        {
            // Визначаємо тип стрибка
            bool isDoubleJump = !movementCore.IsGrounded() && Time.time - lastGroundedTime > coyoteTime;
            
            if (isDoubleJump)
            {
                hasDoubleJumped = true;
            }
            
            // Скидаємо вертикальну швидкість та додаємо силу стрибка
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Встановлюємо стан стрибка
            isJumping = true;
            
            // Скидаємо jump buffer
            lastJumpInputTime = 0f;
            
            // Тригеримо події
            TriggerJumpEvents(isDoubleJump);
        }
        
        /// <summary>
        /// Застосовує модифікатор для низьких стрибків
        /// </summary>
        private void ApplyLowJumpModifier()
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.5f, rb.velocity.z);
            }
        }
        
        /// <summary>
        /// Застосовує модифікатори гравітації
        /// </summary>
        private void ApplyGravityModifiers()
        {
            if (rb == null) return;
            
            Vector3 currentVelocity = rb.velocity;
            
            // Застосовуємо модифікатори гравітації
            if (currentVelocity.y < 0)
            {
                // Падіння - збільшуємо гравітацію
                rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (currentVelocity.y > 0 && !jumpInputHeld)
            {
                // Низький стрибок - збільшуємо гравітацію
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
        }
        
        /// <summary>
        /// Тригерить події стрибка
        /// </summary>
        private void TriggerJumpEvents(bool isDoubleJump)
        {
            // Тут можна додати інтеграцію з EventSystem
            string eventName = isDoubleJump ? "PlayerDoubleJumped" : "PlayerJumped";
            
            // Якщо EventSystem доступна
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.TriggerEvent(eventName, new { 
                    position = transform.position,
                    velocity = rb.velocity,
                    isDoubleJump = isDoubleJump
                });
            }
        }
        
        // === Публічні методи для доступу ===
        
        /// <summary>
        /// Перевіряє, чи гравець зараз стрибає
        /// </summary>
        public bool IsJumping() => isJumping;
        
        /// <summary>
        /// Перевіряє, чи використав гравець подвійний стрибок
        /// </summary>
        public bool HasUsedDoubleJump() => hasDoubleJumped;
        
        /// <summary>
        /// Перевіряє, чи може гравець зараз стрибнути
        /// </summary>
        public bool CanJumpNow() => CanJump();
        
        /// <summary>
        /// Примусово виконує стрибок (для зовнішніх систем)
        /// </summary>
        public void ForceJump(float customForce = -1f)
        {
            float force = customForce > 0 ? customForce : jumpForce;
            
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            
            isJumping = true;
            TriggerJumpEvents(false);
        }
        
        /// <summary>
        /// Скидає стан стрибків (для зовнішніх систем)
        /// </summary>
        public void ResetJumpState()
        {
            hasDoubleJumped = false;
            isJumping = false;
            lastJumpInputTime = 0f;
        }
        
        /// <summary>
        /// Отримує інформацію про стан стрибків для UI
        /// </summary>
        public JumpInfo GetJumpInfo()
        {
            return new JumpInfo
            {
                canJump = CanJump(),
                isJumping = isJumping,
                hasDoubleJumped = hasDoubleJumped,
                coyoteTimeRemaining = Mathf.Max(0, coyoteTime - (Time.time - lastGroundedTime)),
                jumpBufferTimeRemaining = Mathf.Max(0, jumpBufferTime - (Time.time - lastJumpInputTime))
            };
        }
    }
    
    /// <summary>
    /// Структура з інформацією про стан стрибків
    /// </summary>
    [System.Serializable]
    public struct JumpInfo
    {
        public bool canJump;
        public bool isJumping;
        public bool hasDoubleJumped;
        public float coyoteTimeRemaining;
        public float jumpBufferTimeRemaining;
    }
}