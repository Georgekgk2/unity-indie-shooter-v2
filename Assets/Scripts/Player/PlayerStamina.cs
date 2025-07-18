using UnityEngine;

namespace IndieShooter.Player
{
    /// <summary>
    /// Система витривалості гравця. Обробляє спринт, витрату та відновлення стаміни.
    /// </summary>
    public class PlayerStamina : MonoBehaviour
    {
        [Header("Stamina Settings")]
        [Tooltip("Максимальна кількість стаміни")]
        public float maxStamina = 100f;
        [Tooltip("Швидкість витрати стаміни за секунду під час спринту")]
        public float staminaDrainRate = 15f;
        [Tooltip("Швидкість відновлення стаміни за секунду, коли не спринтує")]
        public float staminaRegenRate = 10f;
        [Tooltip("Затримка перед початком відновлення стаміни після припинення спринту")]
        public float staminaRegenDelay = 1.5f;
        [Tooltip("Мінімальна кількість стаміни, необхідна для початку спринту")]
        public float minStaminaToSprint = 10f;
        [Tooltip("Мінімальна швидкість руху гравця для витрати стаміни під час спринту")]
        public float minSpeedToDrainStamina = 0.1f;
        
        // Посилання на основний контролер
        private PlayerMovementCore movementCore;
        private Rigidbody rb;
        
        // Стан стаміни
        private float currentStamina;
        private bool isSprinting = false;
        private bool wantsToSprint = false;
        private float lastSprintTime = 0f;
        
        // Події стаміни
        public System.Action<float, float> OnStaminaChanged; // current, max
        public System.Action OnSprintStarted;
        public System.Action OnSprintEnded;
        public System.Action OnStaminaExhausted;
        
        public void Initialize(PlayerMovementCore core)
        {
            movementCore = core;
            rb = core.GetRigidbody();
            currentStamina = maxStamina;
        }
        
        void Start()
        {
            ValidateSetup();
            ValidateParameters();
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (movementCore == null)
            {
                Debug.LogError("PlayerStamina: MovementCore not initialized!", this);
                enabled = false;
                return;
            }
            
            if (rb == null)
            {
                Debug.LogError("PlayerStamina: Rigidbody not found!", this);
                enabled = false;
                return;
            }
        }
        
        /// <summary>
        /// Валідує та виправляє параметри
        /// </summary>
        private void ValidateParameters()
        {
            maxStamina = Mathf.Max(1f, maxStamina);
            staminaDrainRate = Mathf.Max(0.1f, staminaDrainRate);
            staminaRegenRate = Mathf.Max(0.1f, staminaRegenRate);
            staminaRegenDelay = Mathf.Max(0f, staminaRegenDelay);
            minStaminaToSprint = Mathf.Clamp(minStaminaToSprint, 0f, maxStamina * 0.5f);
            minSpeedToDrainStamina = Mathf.Max(0.01f, minSpeedToDrainStamina);
            
            // Переконуємося, що поточна стаміна в межах
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
        
        /// <summary>
        /// Обробляє введення для спринту
        /// </summary>
        public void HandleInput()
        {
            wantsToSprint = Input.GetKey(KeyCode.LeftShift);
        }
        
        /// <summary>
        /// Оновлює систему стаміни
        /// </summary>
        public void UpdateStamina()
        {
            // Перевіряємо умови для спринту
            bool canSprint = CanSprint();
            
            // Оновлюємо стан спринту
            UpdateSprintState(canSprint);
            
            // Оновлюємо стаміну
            if (isSprinting)
            {
                DrainStamina();
            }
            else
            {
                RegenerateStamina();
            }
            
            // Тригеримо події
            TriggerStaminaEvents();
        }
        
        /// <summary>
        /// Перевіряє, чи може гравець спринтувати
        /// </summary>
        private bool CanSprint()
        {
            if (!wantsToSprint) return false;
            if (currentStamina < minStaminaToSprint) return false;
            if (!movementCore.IsGrounded()) return false;
            
            // Перевіряємо швидкість руху
            Vector3 horizontalVelocity = movementCore.GetCurrentVelocity();
            horizontalVelocity.y = 0;
            if (horizontalVelocity.magnitude < minSpeedToDrainStamina) return false;
            
            // Перевіряємо, чи не в спеціальних станах
            var statesSystem = GetComponent<PlayerStates>();
            if (statesSystem != null)
            {
                if (statesSystem.IsCrouching() || statesSystem.IsSliding() || statesSystem.IsDashing())
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Оновлює стан спринту
        /// </summary>
        private void UpdateSprintState(bool canSprint)
        {
            bool wasSprintingBefore = isSprinting;
            isSprinting = canSprint;
            
            // Тригеримо події зміни стану
            if (isSprinting && !wasSprintingBefore)
            {
                OnSprintStarted?.Invoke();
                TriggerSprintEvent(true);
            }
            else if (!isSprinting && wasSprintingBefore)
            {
                lastSprintTime = Time.time;
                OnSprintEnded?.Invoke();
                TriggerSprintEvent(false);
            }
        }
        
        /// <summary>
        /// Витрачає стаміну під час спринту
        /// </summary>
        private void DrainStamina()
        {
            // Витрачаємо стаміну тільки якщо рухаємося
            Vector3 horizontalVelocity = movementCore.GetCurrentVelocity();
            horizontalVelocity.y = 0;
            
            if (horizontalVelocity.magnitude > minSpeedToDrainStamina)
            {
                float previousStamina = currentStamina;
                currentStamina -= staminaDrainRate * Time.deltaTime;
                currentStamina = Mathf.Max(currentStamina, 0f);
                
                // Перевіряємо виснаження
                if (previousStamina > 0 && currentStamina <= 0)
                {
                    OnStaminaExhausted?.Invoke();
                    TriggerStaminaExhaustedEvent();
                }
            }
        }
        
        /// <summary>
        /// Відновлює стаміну
        /// </summary>
        private void RegenerateStamina()
        {
            // Відновлюємо тільки після затримки
            if (Time.time - lastSprintTime >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
        
        /// <summary>
        /// Тригерить події стаміни
        /// </summary>
        private void TriggerStaminaEvents()
        {
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Тригерить події спринту через EventSystem
        /// </summary>
        private void TriggerSprintEvent(bool started)
        {
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                string eventName = started ? "PlayerSprintStarted" : "PlayerSprintEnded";
                eventSystem.TriggerEvent(eventName, new {
                    stamina = currentStamina,
                    maxStamina = maxStamina,
                    staminaPercentage = GetStaminaPercentage()
                });
            }
        }
        
        /// <summary>
        /// Тригерить подію виснаження стаміни
        /// </summary>
        private void TriggerStaminaExhaustedEvent()
        {
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.TriggerEvent("PlayerStaminaExhausted", new {
                    position = transform.position,
                    wasSprintingBefore = true
                });
            }
        }
        
        // === Публічні методи ===
        
        /// <summary>
        /// Перевіряє, чи гравець зараз спринтує
        /// </summary>
        public bool IsSprinting() => isSprinting;
        
        /// <summary>
        /// Отримує поточну стаміну
        /// </summary>
        public float GetCurrentStamina() => currentStamina;
        
        /// <summary>
        /// Отримує максимальну стаміну
        /// </summary>
        public float GetMaxStamina() => maxStamina;
        
        /// <summary>
        /// Отримує відсоток стаміни (0-1)
        /// </summary>
        public float GetStaminaPercentage() => currentStamina / maxStamina;
        
        /// <summary>
        /// Перевіряє, чи стаміна виснажена
        /// </summary>
        public bool IsStaminaExhausted() => currentStamina <= 0f;
        
        /// <summary>
        /// Перевіряє, чи може почати спринт
        /// </summary>
        public bool CanStartSprint() => currentStamina >= minStaminaToSprint;
        
        /// <summary>
        /// Отримує модифікатор швидкості для спринту
        /// </summary>
        public float GetSpeedModifier()
        {
            if (isSprinting) return 1.0f; // Спринт використовує sprintSpeed
            return 1.0f; // Звичайна швидкість
        }
        
        /// <summary>
        /// Примусово витрачає стаміну (для зовнішніх систем)
        /// </summary>
        public bool ConsumeStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                currentStamina = Mathf.Max(currentStamina, 0f);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Примусово відновлює стаміну (для зовнішніх систем)
        /// </summary>
        public void RestoreStamina(float amount)
        {
            currentStamina += amount;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Повністю відновлює стаміну
        /// </summary>
        public void FullRestoreStamina()
        {
            currentStamina = maxStamina;
        }
        
        /// <summary>
        /// Встановлює максимальну стаміну (для прогресії персонажа)
        /// </summary>
        public void SetMaxStamina(float newMaxStamina)
        {
            float ratio = currentStamina / maxStamina;
            maxStamina = Mathf.Max(1f, newMaxStamina);
            currentStamina = maxStamina * ratio; // Зберігаємо відсоток
        }
        
        /// <summary>
        /// Отримує детальну інформацію про стаміну для UI
        /// </summary>
        public StaminaInfo GetStaminaInfo()
        {
            return new StaminaInfo
            {
                current = currentStamina,
                max = maxStamina,
                percentage = GetStaminaPercentage(),
                isSprinting = isSprinting,
                canSprint = CanStartSprint(),
                isExhausted = IsStaminaExhausted(),
                timeUntilRegen = Mathf.Max(0, staminaRegenDelay - (Time.time - lastSprintTime)),
                drainRate = staminaDrainRate,
                regenRate = staminaRegenRate
            };
        }
        
        /// <summary>
        /// Скидає стан стаміни (для респавну або перезапуску рівня)
        /// </summary>
        public void ResetStamina()
        {
            currentStamina = maxStamina;
            isSprinting = false;
            wantsToSprint = false;
            lastSprintTime = 0f;
        }
    }
    
    /// <summary>
    /// Структура з детальною інформацією про стаміну
    /// </summary>
    [System.Serializable]
    public struct StaminaInfo
    {
        public float current;
        public float max;
        public float percentage;
        public bool isSprinting;
        public bool canSprint;
        public bool isExhausted;
        public float timeUntilRegen;
        public float drainRate;
        public float regenRate;
    }
}