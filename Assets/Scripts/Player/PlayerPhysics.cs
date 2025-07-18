using UnityEngine;

namespace IndieShooter.Player
{
    /// <summary>
    /// Система фізики гравця. Обробляє перевірку землі, колізії,
    /// гравітацію та інші фізичні взаємодії.
    /// </summary>
    public class PlayerPhysics : MonoBehaviour
    {
        [Header("Ground Detection")]
        [Tooltip("Розмір області перевірки землі")]
        public Vector3 groundCheckHalfExtents = new Vector3(0.4f, 0.1f, 0.4f);
        [Tooltip("Відстань від центру гравця до точки перевірки землі")]
        public float groundCheckDistance = 0.1f;
        [Tooltip("Додатковий час після залишення землі, коли гравець ще вважається на землі")]
        public float groundedGracePeriod = 0.1f;
        
        [Header("Physics Settings")]
        [Tooltip("Множник гравітації для гравця")]
        public float gravityMultiplier = 1f;
        [Tooltip("Максимальна швидкість падіння")]
        public float maxFallSpeed = 20f;
        [Tooltip("Сила, що притискає гравця до землі для кращого контролю")]
        public float groundStickForce = 5f;
        
        [Header("Collision Settings")]
        [Tooltip("Висота кроку, який гравець може подолати")]
        public float stepHeight = 0.3f;
        [Tooltip("Кут нахилу, по якому гравець може ходити")]
        public float maxSlopeAngle = 45f;
        
        // Посилання на компоненти
        private PlayerMovementCore movementCore;
        private Rigidbody rb;
        private CapsuleCollider capsuleCollider;
        
        // Стан фізики
        private bool isGrounded = false;
        private bool wasGroundedLastFrame = false;
        private float lastGroundedTime = 0f;
        private Vector3 groundNormal = Vector3.up;
        private float currentSlopeAngle = 0f;
        
        // Кеш для оптимізації
        private readonly Collider[] groundOverlapResults = new Collider[10];
        
        public void Initialize(PlayerMovementCore core)
        {
            movementCore = core;
            rb = core.GetRigidbody();
            capsuleCollider = core.GetCapsuleCollider();
        }
        
        void Start()
        {
            ValidateSetup();
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (movementCore == null)
            {
                Debug.LogError("PlayerPhysics: MovementCore not initialized!", this);
                enabled = false;
                return;
            }
            
            if (rb == null)
            {
                Debug.LogError("PlayerPhysics: Rigidbody not found!", this);
                enabled = false;
                return;
            }
            
            if (capsuleCollider == null)
            {
                Debug.LogWarning("PlayerPhysics: CapsuleCollider not found!", this);
            }
        }
        
        /// <summary>
        /// Оновлює фізичні розрахунки
        /// </summary>
        public void UpdatePhysics()
        {
            ApplyGravity();
            ApplyGroundStick();
            LimitFallSpeed();
        }
        
        /// <summary>
        /// Перевіряє, чи гравець на землі
        /// </summary>
        public bool CheckGrounded()
        {
            wasGroundedLastFrame = isGrounded;
            
            Transform groundCheck = movementCore.GetGroundCheck();
            LayerMask groundMask = movementCore.GetGroundMask();
            
            if (groundCheck == null)
            {
                // Fallback - використовуємо позицію гравця
                groundCheck = transform;
            }
            
            // Виконуємо перевірку overlap
            Vector3 checkPosition = groundCheck.position;
            int hitCount = Physics.OverlapBoxNonAlloc(
                checkPosition,
                groundCheckHalfExtents,
                groundOverlapResults,
                groundCheck.rotation,
                groundMask
            );
            
            bool foundGround = false;
            groundNormal = Vector3.up;
            currentSlopeAngle = 0f;
            
            // Перевіряємо всі знайдені колайдери
            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = groundOverlapResults[i];
                
                // Ігноруємо власний колайдер
                if (hitCollider == capsuleCollider) continue;
                
                // Виконуємо raycast для отримання нормалі поверхні
                RaycastHit hit;
                Vector3 rayStart = checkPosition + Vector3.up * 0.1f;
                
                if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.2f, groundMask))
                {
                    if (hit.collider == hitCollider)
                    {
                        groundNormal = hit.normal;
                        currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
                        
                        // Перевіряємо, чи можна ходити по цьому нахилу
                        if (currentSlopeAngle <= maxSlopeAngle)
                        {
                            foundGround = true;
                            break;
                        }
                    }
                }
            }
            
            // Оновлюємо стан землі
            if (foundGround)
            {
                isGrounded = true;
                lastGroundedTime = Time.time;
            }
            else
            {
                // Використовуємо grace period
                isGrounded = (Time.time - lastGroundedTime) <= groundedGracePeriod;
            }
            
            // Тригеримо події при зміні стану
            if (isGrounded != wasGroundedLastFrame)
            {
                TriggerGroundStateEvents();
            }
            
            return isGrounded;
        }
        
        /// <summary>
        /// Застосовує модифіковану гравітацію
        /// </summary>
        private void ApplyGravity()
        {
            if (rb == null || isGrounded) return;
            
            Vector3 gravity = Physics.gravity * gravityMultiplier;
            rb.AddForce(gravity, ForceMode.Acceleration);
        }
        
        /// <summary>
        /// Притискає гравця до землі для кращого контролю
        /// </summary>
        private void ApplyGroundStick()
        {
            if (rb == null || !isGrounded) return;
            
            // Притискаємо гравця до землі
            Vector3 stickForce = -groundNormal * groundStickForce;
            rb.AddForce(stickForce, ForceMode.Force);
        }
        
        /// <summary>
        /// Обмежує максимальну швидкість падіння
        /// </summary>
        private void LimitFallSpeed()
        {
            if (rb == null) return;
            
            Vector3 velocity = rb.velocity;
            
            if (velocity.y < -maxFallSpeed)
            {
                velocity.y = -maxFallSpeed;
                rb.velocity = velocity;
            }
        }
        
        /// <summary>
        /// Тригерить події зміни стану землі
        /// </summary>
        private void TriggerGroundStateEvents()
        {
            // Інтеграція з EventSystem
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                if (isGrounded && !wasGroundedLastFrame)
                {
                    // Приземлення
                    eventSystem.TriggerEvent("PlayerLanded", new {
                        position = transform.position,
                        velocity = rb.velocity,
                        groundNormal = groundNormal,
                        slopeAngle = currentSlopeAngle
                    });
                }
                else if (!isGrounded && wasGroundedLastFrame)
                {
                    // Залишення землі
                    eventSystem.TriggerEvent("PlayerLeftGround", new {
                        position = transform.position,
                        velocity = rb.velocity
                    });
                }
            }
        }
        
        /// <summary>
        /// Перевіряє, чи може гравець подолати крок
        /// </summary>
        public bool CanStepUp(Vector3 moveDirection)
        {
            if (!isGrounded || moveDirection.magnitude < 0.1f) return false;
            
            // Виконуємо raycast вперед на рівні кроку
            Vector3 rayStart = transform.position + Vector3.up * stepHeight;
            float rayDistance = capsuleCollider != null ? capsuleCollider.radius + 0.1f : 0.6f;
            
            RaycastHit hit;
            if (Physics.Raycast(rayStart, moveDirection, out hit, rayDistance))
            {
                // Перевіряємо, чи є місце для кроку
                Vector3 stepCheckStart = hit.point + Vector3.up * 0.1f;
                
                if (!Physics.Raycast(stepCheckStart, Vector3.down, stepHeight + 0.2f))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Виконує крок вгору
        /// </summary>
        public void PerformStepUp(Vector3 moveDirection)
        {
            if (rb == null) return;
            
            Vector3 stepForce = Vector3.up * stepHeight * 10f;
            rb.AddForce(stepForce, ForceMode.Impulse);
        }
        
        // === Публічні методи ===
        
        /// <summary>
        /// Отримує нормаль поверхні під гравцем
        /// </summary>
        public Vector3 GetGroundNormal() => groundNormal;
        
        /// <summary>
        /// Отримує кут нахилу поверхні
        /// </summary>
        public float GetSlopeAngle() => currentSlopeAngle;
        
        /// <summary>
        /// Перевіряє, чи на нахилі гравець
        /// </summary>
        public bool IsOnSlope() => currentSlopeAngle > 1f && currentSlopeAngle <= maxSlopeAngle;
        
        /// <summary>
        /// Отримує напрямок руху по нахилу
        /// </summary>
        public Vector3 GetSlopeMovementDirection(Vector3 inputDirection)
        {
            return Vector3.ProjectOnPlane(inputDirection, groundNormal).normalized;
        }
        
        /// <summary>
        /// Встановлює множник гравітації
        /// </summary>
        public void SetGravityMultiplier(float multiplier)
        {
            gravityMultiplier = multiplier;
        }
        
        /// <summary>
        /// Отримує інформацію про фізичний стан
        /// </summary>
        public PhysicsInfo GetPhysicsInfo()
        {
            return new PhysicsInfo
            {
                isGrounded = isGrounded,
                groundNormal = groundNormal,
                slopeAngle = currentSlopeAngle,
                isOnSlope = IsOnSlope(),
                velocity = rb != null ? rb.velocity : Vector3.zero,
                lastGroundedTime = lastGroundedTime
            };
        }
        
        /// <summary>
        /// Візуалізація для редактора
        /// </summary>
        void OnDrawGizmos()
        {
            if (movementCore == null) return;
            
            Transform groundCheck = movementCore.GetGroundCheck();
            if (groundCheck == null) groundCheck = transform;
            
            // Візуалізація області перевірки землі
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.matrix = Matrix4x4.TRS(groundCheck.position, groundCheck.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, groundCheckHalfExtents * 2f);
            
            // Візуалізація нормалі поверхні
            if (isGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawRay(transform.position, groundNormal * 2f);
            }
        }
    }
    
    /// <summary>
    /// Структура з інформацією про фізичний стан
    /// </summary>
    [System.Serializable]
    public struct PhysicsInfo
    {
        public bool isGrounded;
        public Vector3 groundNormal;
        public float slopeAngle;
        public bool isOnSlope;
        public Vector3 velocity;
        public float lastGroundedTime;
    }
}