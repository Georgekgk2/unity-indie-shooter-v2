using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Система фаз боса. Управляє переходами між фазами та їх унікальними характеристиками.
    /// </summary>
    public class BossPhases : MonoBehaviour
    {
        [Header("Phase Configuration")]
        [Tooltip("Налаштування для кожної фази")]
        public BossPhaseData[] phases;
        [Tooltip("Час переходу між фазами")]
        public float transitionDuration = 2f;
        [Tooltip("Ефект переходу між фазами")]
        public GameObject phaseTransitionEffect;
        
        // Посилання на основний контролер
        private BossController bossController;
        
        // Поточний стан фаз
        private int currentPhaseIndex = 0;
        private bool isTransitioning = false;
        private float transitionTimer = 0f;
        
        // Події фаз
        public System.Action<int> OnPhaseTransition;
        public System.Action<int> OnPhaseStarted;
        public System.Action<int> OnPhaseEnded;
        
        public void Initialize(BossController controller)
        {
            bossController = controller;
            SetupPhases();
        }
        
        void Start()
        {
            ValidatePhases();
            StartPhase(0);
        }
        
        void Update()
        {
            if (isTransitioning)
            {
                UpdateTransition();
            }
        }
        
        /// <summary>
        /// Налаштовує фази боса
        /// </summary>
        private void SetupPhases()
        {
            if (phases == null || phases.Length == 0)
            {
                CreateDefaultPhases();
            }
            
            // Ініціалізуємо кожну фазу
            for (int i = 0; i < phases.Length; i++)
            {
                phases[i].phaseNumber = i + 1;
                phases[i].isActive = false;
            }
        }
        
        /// <summary>
        /// Створює дефолтні фази якщо не налаштовані
        /// </summary>
        private void CreateDefaultPhases()
        {
            phases = new BossPhaseData[3];
            
            // Фаза 1: Нормальна
            phases[0] = new BossPhaseData
            {
                phaseName = "Normal Phase",
                healthThreshold = 100f,
                damageMultiplier = 1f,
                speedMultiplier = 1f,
                abilitySet = new string[] { "BasicAttack", "Charge" }
            };
            
            // Фаза 2: Агресивна
            phases[1] = new BossPhaseData
            {
                phaseName = "Aggressive Phase",
                healthThreshold = 50f,
                damageMultiplier = 1.5f,
                speedMultiplier = 1.2f,
                abilitySet = new string[] { "BasicAttack", "Charge", "AreaAttack" }
            };
            
            // Фаза 3: Берсерк
            phases[2] = new BossPhaseData
            {
                phaseName = "Berserk Phase",
                healthThreshold = 25f,
                damageMultiplier = 2f,
                speedMultiplier = 1.5f,
                abilitySet = new string[] { "BasicAttack", "Charge", "AreaAttack", "UltimateAbility" }
            };
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування фаз
        /// </summary>
        private void ValidatePhases()
        {
            if (phases == null || phases.Length == 0)
            {
                Debug.LogError("BossPhases: No phases configured!", this);
                enabled = false;
                return;
            }
            
            // Перевіряємо що пороги здоров'я йдуть по спаданню
            for (int i = 1; i < phases.Length; i++)
            {
                if (phases[i].healthThreshold >= phases[i-1].healthThreshold)
                {
                    Debug.LogWarning($"BossPhases: Phase {i+1} health threshold should be lower than phase {i}", this);
                }
            }
        }
        
        /// <summary>
        /// Переходить до нової фази
        /// </summary>
        public void TransitionToPhase(int phaseNumber)
        {
            int phaseIndex = phaseNumber - 1;
            
            if (phaseIndex < 0 || phaseIndex >= phases.Length)
            {
                Debug.LogError($"BossPhases: Invalid phase number {phaseNumber}", this);
                return;
            }
            
            if (phaseIndex == currentPhaseIndex)
            {
                Debug.LogWarning($"BossPhases: Already in phase {phaseNumber}", this);
                return;
            }
            
            if (isTransitioning)
            {
                Debug.LogWarning("BossPhases: Already transitioning between phases", this);
                return;
            }
            
            StartTransition(phaseIndex);
        }
        
        /// <summary>
        /// Починає перехід до нової фази
        /// </summary>
        private void StartTransition(int newPhaseIndex)
        {
            isTransitioning = true;
            transitionTimer = transitionDuration;
            
            // Завершуємо поточну фазу
            EndCurrentPhase();
            
            // Показуємо ефект переходу
            ShowTransitionEffect();
            
            // Тригеримо подію переходу
            OnPhaseTransition?.Invoke(newPhaseIndex + 1);
            
            Debug.Log($"Boss transitioning from phase {currentPhaseIndex + 1} to phase {newPhaseIndex + 1}");
            
            // Запускаємо корутину переходу
            StartCoroutine(TransitionCoroutine(newPhaseIndex));
        }
        
        /// <summary>
        /// Корутина переходу між фазами
        /// </summary>
        private System.Collections.IEnumerator TransitionCoroutine(int newPhaseIndex)
        {
            // Робимо боса невразливим під час переходу
            if (bossController != null)
            {
                bossController.SetInvulnerable(true);
            }
            
            // Чекаємо завершення переходу
            yield return new WaitForSeconds(transitionDuration);
            
            // Активуємо нову фазу
            currentPhaseIndex = newPhaseIndex;
            StartPhase(currentPhaseIndex);
            
            // Відновлюємо вразливість
            if (bossController != null)
            {
                bossController.SetInvulnerable(false);
            }
            
            isTransitioning = false;
        }
        
        /// <summary>
        /// Оновлює перехід між фазами
        /// </summary>
        private void UpdateTransition()
        {
            transitionTimer -= Time.deltaTime;
            
            // Можна додати ефекти переходу тут
            float progress = 1f - (transitionTimer / transitionDuration);
            UpdateTransitionEffects(progress);
        }
        
        /// <summary>
        /// Оновлює ефекти переходу
        /// </summary>
        private void UpdateTransitionEffects(float progress)
        {
            // Тут можна додати візуальні ефекти переходу
            // Наприклад, зміна кольору, розміру, тощо
        }
        
        /// <summary>
        /// Показує ефект переходу
        /// </summary>
        private void ShowTransitionEffect()
        {
            if (phaseTransitionEffect != null)
            {
                Instantiate(phaseTransitionEffect, transform.position, transform.rotation);
            }
        }
        
        /// <summary>
        /// Починає нову фазу
        /// </summary>
        private void StartPhase(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Length) return;
            
            var phase = phases[phaseIndex];
            phase.isActive = true;
            
            // Застосовуємо налаштування фази
            ApplyPhaseSettings(phase);
            
            // Тригеримо події
            OnPhaseStarted?.Invoke(phase.phaseNumber);
            
            Debug.Log($"Boss started phase {phase.phaseNumber}: {phase.phaseName}");
        }
        
        /// <summary>
        /// Завершує поточну фазу
        /// </summary>
        private void EndCurrentPhase()
        {
            if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Length)
            {
                var phase = phases[currentPhaseIndex];
                phase.isActive = false;
                
                OnPhaseEnded?.Invoke(phase.phaseNumber);
                
                Debug.Log($"Boss ended phase {phase.phaseNumber}: {phase.phaseName}");
            }
        }
        
        /// <summary>
        /// Застосовує налаштування фази
        /// </summary>
        private void ApplyPhaseSettings(BossPhaseData phase)
        {
            // Інтеграція з EventSystem для повідомлення інших систем
            var eventSystem = FindObjectOfType<IndieShooter.Core.EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.TriggerEvent("BossPhaseSettingsChanged", new {
                    phaseNumber = phase.phaseNumber,
                    phaseName = phase.phaseName,
                    damageMultiplier = phase.damageMultiplier,
                    speedMultiplier = phase.speedMultiplier,
                    abilitySet = phase.abilitySet
                });
            }
        }
        
        /// <summary>
        /// Перевіряє чи потрібен перехід фази на основі здоров'я
        /// </summary>
        public bool ShouldTransitionBasedOnHealth(float healthPercentage)
        {
            // Перевіряємо чи є наступна фаза
            int nextPhaseIndex = currentPhaseIndex + 1;
            if (nextPhaseIndex >= phases.Length) return false;
            
            // Перевіряємо поріг здоров'я
            return healthPercentage <= phases[nextPhaseIndex].healthThreshold;
        }
        
        /// <summary>
        /// Отримує наступну фазу на основі здоров'я
        /// </summary>
        public int GetNextPhaseForHealth(float healthPercentage)
        {
            for (int i = phases.Length - 1; i >= 0; i--)
            {
                if (healthPercentage <= phases[i].healthThreshold)
                {
                    return i + 1; // Повертаємо номер фази (1-based)
                }
            }
            return 1; // Дефолтна перша фаза
        }
        
        // === Публічні методи ===
        
        public BossPhaseData GetCurrentPhase()
        {
            if (currentPhaseIndex >= 0 && currentPhaseIndex < phases.Length)
            {
                return phases[currentPhaseIndex];
            }
            return null;
        }
        
        public int GetCurrentPhaseNumber() => currentPhaseIndex + 1;
        public bool IsTransitioning() => isTransitioning;
        public int GetTotalPhases() => phases?.Length ?? 0;
        
        public BossPhaseData GetPhase(int phaseNumber)
        {
            int index = phaseNumber - 1;
            if (index >= 0 && index < phases.Length)
            {
                return phases[index];
            }
            return null;
        }
        
        public string[] GetCurrentAbilitySet()
        {
            var currentPhase = GetCurrentPhase();
            return currentPhase?.abilitySet ?? new string[0];
        }
        
        public float GetCurrentDamageMultiplier()
        {
            var currentPhase = GetCurrentPhase();
            return currentPhase?.damageMultiplier ?? 1f;
        }
        
        public float GetCurrentSpeedMultiplier()
        {
            var currentPhase = GetCurrentPhase();
            return currentPhase?.speedMultiplier ?? 1f;
        }
    }
    
    /// <summary>
    /// Дані фази боса
    /// </summary>
    [System.Serializable]
    public class BossPhaseData
    {
        [Header("Phase Info")]
        public int phaseNumber = 1;
        public string phaseName = "Phase 1";
        public string phaseDescription = "";
        
        [Header("Activation")]
        public float healthThreshold = 100f; // Відсоток здоров'я для активації
        public bool isActive = false;
        
        [Header("Modifiers")]
        [Range(0.1f, 5f)]
        public float damageMultiplier = 1f;
        [Range(0.1f, 3f)]
        public float speedMultiplier = 1f;
        [Range(0.1f, 2f)]
        public float defenseMultiplier = 1f;
        
        [Header("Abilities")]
        public string[] abilitySet = new string[0];
        public float abilityCooldownMultiplier = 1f;
        
        [Header("Visual Effects")]
        public Color phaseColor = Color.white;
        public GameObject phaseEffect;
        public AudioClip phaseMusic;
        
        [Header("Behavior")]
        public BossAIBehavior aiBehavior = BossAIBehavior.Aggressive;
        public float aggroRange = 20f;
        public float attackRange = 5f;
    }
    
    /// <summary>
    /// Типи поведінки боса
    /// </summary>
    public enum BossAIBehavior
    {
        Passive,     // Пасивний
        Defensive,   // Оборонний
        Aggressive,  // Агресивний
        Berserk,     // Берсерк
        Tactical     // Тактичний
    }
}