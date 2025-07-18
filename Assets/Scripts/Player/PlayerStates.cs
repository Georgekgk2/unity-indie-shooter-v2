using UnityEngine;

namespace IndieShooter.Player
{
    /// <summary>
    /// Система станів гравця. Обробляє різні стани руху: крадіння, ковзання, 
    /// присідання та переходи між ними.
    /// </summary>
    public class PlayerStates : MonoBehaviour
    {
        [Header("State Settings")]
        [Tooltip("Швидкість руху при присіданні")]
        public float crouchSpeed = 2f;
        [Tooltip("Швидкість руху при крадінні")]
        public float sneakSpeed = 1.5f;
        [Tooltip("Сила ковзання")]
        public float slideForce = 10f;
        [Tooltip("Тривалість ковзання")]
        public float slideDuration = 1f;
        [Tooltip("Швидкість переходу між станами")]
        public float stateTransitionSpeed = 5f;
        
        [Header("Crouch Settings")]
        [Tooltip("Висота капсули при присіданні")]
        public float crouchHeight = 1f;
        [Tooltip("Центр капсули при присіданні")]
        public Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
        
        // Посилання на основний контролер
        private PlayerMovementCore movementCore;
        private CapsuleCollider capsuleCollider;
        private Rigidbody rb;
        
        // Поточний стан
        public enum MovementState
        {
            Normal,
            Crouching,
            Sneaking,
            Sliding
        }
        
        private MovementState currentState = MovementState.Normal;
        private MovementState previousState = MovementState.Normal;
        
        // Параметри стану
        private float originalHeight;
        private Vector3 originalCenter;
        private float slideTimer = 0f;
        private Vector3 slideDirection;
        
        // Введення
        private bool crouchInput = false;
        private bool sneakInput = false;
        private bool slideInput = false;
        
        public void Initialize(PlayerMovementCore core)
        {
            movementCore = core;
            capsuleCollider = core.GetCapsuleCollider();
            rb = core.GetRigidbody();
            
            if (capsuleCollider != null)
            {
                originalHeight = capsuleCollider.height;
                originalCenter = capsuleCollider.center;
            }
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
                Debug.LogError("PlayerStates: MovementCore not initialized!", this);
                enabled = false;
                return;
            }
            
            if (capsuleCollider == null)
            {
                Debug.LogError("PlayerStates: CapsuleCollider not found!", this);
                enabled = false;
                return;
            }
        }
        
        /// <summary>
        /// Обробляє введення для станів
        /// </summary>
        public void HandleInput()
        {
            // Присідання
            crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
            
            // Крадіння (Shift + присідання)
            sneakInput = crouchInput && Input.GetKey(KeyCode.LeftShift);
            
            // Ковзання (присідання під час бігу)
            bool sprintInput = Input.GetKey(KeyCode.LeftShift);
            slideInput = Input.GetKeyDown(KeyCode.LeftControl) && sprintInput && 
                        movementCore.GetMoveInput().magnitude > 0.1f && 
                        movementCore.IsGrounded();
        }
        
        /// <summary>
        /// Оновлює стани
        /// </summary>
        public void UpdateStates()
        {
            // Обробляємо введення ковзання
            if (slideInput && currentState != MovementState.Sliding)
            {
                StartSlide();
            }
            
            // Оновлюємо поточний стан
            switch (currentState)
            {
                case MovementState.Normal:
                    UpdateNormalState();
                    break;
                case MovementState.Crouching:
                    UpdateCrouchState();
                    break;
                case MovementState.Sneaking:
                    UpdateSneakState();
                    break;
                case MovementState.Sliding:
                    UpdateSlideState();
                    break;
            }
            
            // Застосовуємо зміни до колайдера
            ApplyColliderChanges();
        }
        
        /// <summary>
        /// Оновлює нормальний стан
        /// </summary>
        private void UpdateNormalState()
        {
            if (sneakInput)
            {
                ChangeState(MovementState.Sneaking);
            }
            else if (crouchInput)
            {
                ChangeState(MovementState.Crouching);
            }
        }
        
        /// <summary>
        /// Оновлює стан присідання
        /// </summary>
        private void UpdateCrouchState()
        {
            if (sneakInput)
            {
                ChangeState(MovementState.Sneaking);
            }
            else if (!crouchInput)
            {
                // Перевіряємо, чи можемо встати
                if (CanStandUp())
                {
                    ChangeState(MovementState.Normal);
                }
            }
        }
        
        /// <summary>
        /// Оновлює стан крадіння
        /// </summary>
        private void UpdateSneakState()
        {
            if (!crouchInput)
            {
                // Перевіряємо, чи можемо встати
                if (CanStandUp())
                {
                    ChangeState(MovementState.Normal);
                }
                else
                {
                    ChangeState(MovementState.Crouching);
                }
            }
            else if (!sneakInput)
            {
                ChangeState(MovementState.Crouching);
            }
        }
        
        /// <summary>
        /// Оновлює стан ковзання
        /// </summary>
        private void UpdateSlideState()
        {
            slideTimer += Time.deltaTime;
            
            // Застосовуємо силу ковзання
            if (slideTimer < slideDuration)
            {
                float slideStrength = Mathf.Lerp(1f, 0f, slideTimer / slideDuration);
                rb.AddForce(slideDirection * slideForce * slideStrength, ForceMode.Force);
            }
            else
            {
                // Завершуємо ковзання
                EndSlide();
            }
        }
        
        /// <summary>
        /// Починає ковзання
        /// </summary>
        private void StartSlide()
        {
            previousState = currentState;
            currentState = MovementState.Sliding;
            slideTimer = 0f;
            
            // Визначаємо напрямок ковзання
            Vector3 moveInput = movementCore.GetMoveInput();
            if (moveInput.magnitude > 0.1f)
            {
                slideDirection = movementCore.GetPlayerCamera().TransformDirection(moveInput).normalized;
                slideDirection.y = 0;
            }
            else
            {
                slideDirection = transform.forward;
            }
            
            TriggerStateEvent("PlayerStartedSliding");
        }
        
        /// <summary>
        /// Завершує ковзання
        /// </summary>
        private void EndSlide()
        {
            if (crouchInput)
            {
                ChangeState(MovementState.Crouching);
            }
            else if (CanStandUp())
            {
                ChangeState(MovementState.Normal);
            }
            else
            {
                ChangeState(MovementState.Crouching);
            }
            
            TriggerStateEvent("PlayerEndedSliding");
        }
        
        /// <summary>
        /// Змінює стан
        /// </summary>
        private void ChangeState(MovementState newState)
        {
            if (currentState == newState) return;
            
            previousState = currentState;
            currentState = newState;
            
            TriggerStateEvent($"PlayerStateChanged");
        }
        
        /// <summary>
        /// Перевіряє, чи може гравець встати
        /// </summary>
        private bool CanStandUp()
        {
            // Перевіряємо, чи є достатньо місця над головою
            Vector3 checkPosition = transform.position + Vector3.up * (originalHeight * 0.5f);
            float checkRadius = capsuleCollider.radius * 0.9f;
            
            return !Physics.CheckSphere(checkPosition, checkRadius, movementCore.GetGroundMask());
        }
        
        /// <summary>
        /// Застосовує зміни до колайдера
        /// </summary>
        private void ApplyColliderChanges()
        {
            float targetHeight = originalHeight;
            Vector3 targetCenter = originalCenter;
            
            switch (currentState)
            {
                case MovementState.Crouching:
                case MovementState.Sneaking:
                case MovementState.Sliding:
                    targetHeight = crouchHeight;
                    targetCenter = crouchCenter;
                    break;
            }
            
            // Плавно змінюємо розміри колайдера
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, targetHeight, 
                stateTransitionSpeed * Time.deltaTime);
            capsuleCollider.center = Vector3.Lerp(capsuleCollider.center, targetCenter, 
                stateTransitionSpeed * Time.deltaTime);
        }
        
        /// <summary>
        /// Тригерить події стану
        /// </summary>
        private void TriggerStateEvent(string eventName)
        {
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.TriggerEvent(eventName, new {
                    currentState = currentState.ToString(),
                    previousState = previousState.ToString(),
                    position = transform.position
                });
            }
        }
        
        // === Публічні методи ===
        
        /// <summary>
        /// Отримує поточний стан
        /// </summary>
        public MovementState GetCurrentState() => currentState;
        
        /// <summary>
        /// Отримує попередній стан
        /// </summary>
        public MovementState GetPreviousState() => previousState;
        
        /// <summary>
        /// Отримує модифікатор швидкості для поточного стану
        /// </summary>
        public float GetSpeedModifier()
        {
            switch (currentState)
            {
                case MovementState.Crouching:
                    return crouchSpeed / 5f; // Припускаємо базову швидкість 5
                case MovementState.Sneaking:
                    return sneakSpeed / 5f;
                case MovementState.Sliding:
                    return 0f; // Ковзання керується фізикою
                default:
                    return 1f;
            }
        }
        
        /// <summary>
        /// Перевіряє, чи гравець присідає
        /// </summary>
        public bool IsCrouching() => currentState == MovementState.Crouching || 
                                    currentState == MovementState.Sneaking || 
                                    currentState == MovementState.Sliding;
        
        /// <summary>
        /// Перевіряє, чи гравець крадеться
        /// </summary>
        public bool IsSneaking() => currentState == MovementState.Sneaking;
        
        /// <summary>
        /// Перевіряє, чи гравець ковзає
        /// </summary>
        public bool IsSliding() => currentState == MovementState.Sliding;
        
        /// <summary>
        /// Примусово змінює стан (для зовнішніх систем)
        /// </summary>
        public void ForceState(MovementState state)
        {
            ChangeState(state);
        }
        
        /// <summary>
        /// Отримує інформацію про стан для UI
        /// </summary>
        public StateInfo GetStateInfo()
        {
            return new StateInfo
            {
                currentState = currentState,
                previousState = previousState,
                speedModifier = GetSpeedModifier(),
                isCrouching = IsCrouching(),
                isSneaking = IsSneaking(),
                isSliding = IsSliding(),
                slideTimeRemaining = IsSliding() ? Mathf.Max(0, slideDuration - slideTimer) : 0f
            };
        }
    }
    
    /// <summary>
    /// Структура з інформацією про стан гравця
    /// </summary>
    [System.Serializable]
    public struct StateInfo
    {
        public PlayerStates.MovementState currentState;
        public PlayerStates.MovementState previousState;
        public float speedModifier;
        public bool isCrouching;
        public bool isSneaking;
        public bool isSliding;
        public float slideTimeRemaining;
    }
}