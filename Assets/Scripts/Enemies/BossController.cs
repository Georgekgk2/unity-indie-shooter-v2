using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Основний контролер боса. Координує всі системи боса та управляє фазами бою.
    /// Замінює монолітний BossSystem_EpicBattles.cs (937 рядків).
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Boss Settings")]
        [Tooltip("Назва боса")]
        public string bossName = "Epic Boss";
        [Tooltip("Максимальне здоров'я боса")]
        public float maxHealth = 10000f;
        [Tooltip("Поточне здоров'я боса")]
        public float currentHealth;
        [Tooltip("Рівень боса")]
        public int bossLevel = 1;
        [Tooltip("Тип боса")]
        public BossType bossType = BossType.Standard;
        
        [Header("Phase Settings")]
        [Tooltip("Кількість фаз боса")]
        public int totalPhases = 3;
        [Tooltip("Поточна фаза боса")]
        public int currentPhase = 1;
        [Tooltip("Відсотки здоров'я для переходу між фазами")]
        public float[] phaseThresholds = { 75f, 50f, 25f };
        
        [Header("Combat Settings")]
        [Tooltip("Чи активний бос")]
        public bool isActive = false;
        [Tooltip("Чи в бою")]
        public bool inCombat = false;
        [Tooltip("Радіус агро")]
        public float aggroRange = 20f;
        [Tooltip("Радіус атаки")]
        public float attackRange = 5f;
        
        // Підсистеми боса
        private BossPhases phaseManager;
        private BossAbilities abilityManager;
        private BossAI aiController;
        
        // Компоненти Unity
        private Rigidbody bossRigidbody;
        private Animator bossAnimator;
        private AudioSource audioSource;
        
        // Стан боса
        private Transform targetPlayer;
        private float lastAttackTime = 0f;
        private bool isDead = false;
        private bool isInvulnerable = false;
        
        // Події боса
        public System.Action<int> OnPhaseChanged;
        public System.Action<float> OnHealthChanged;
        public System.Action OnBossDefeated;
        public System.Action OnBossEnraged;
        public System.Action<string> OnAbilityUsed;
        
        void Awake()
        {
            InitializeComponents();
            InitializeSubsystems();
        }
        
        void Start()
        {
            SetupBoss();
        }
        
        void Update()
        {
            if (!isActive || isDead) return;
            
            UpdateBossLogic();
            UpdateCombatState();
            CheckPhaseTransitions();
        }
        
        /// <summary>
        /// Ініціалізує основні компоненти
        /// </summary>
        private void InitializeComponents()
        {
            bossRigidbody = GetComponent<Rigidbody>();
            bossAnimator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            
            if (bossRigidbody == null)
            {
                Debug.LogError("BossController: Rigidbody component not found!", this);
            }
            
            if (bossAnimator == null)
            {
                Debug.LogWarning("BossController: Animator component not found!", this);
            }
        }
        
        /// <summary>
        /// Ініціалізує підсистеми боса
        /// </summary>
        private void InitializeSubsystems()
        {
            // Отримуємо або створюємо підсистеми
            phaseManager = GetComponent<BossPhases>();
            abilityManager = GetComponent<BossAbilities>();
            aiController = GetComponent<BossAI>();
            
            // Якщо компоненти не знайдені, додаємо їх
            if (phaseManager == null) phaseManager = gameObject.AddComponent<BossPhases>();
            if (abilityManager == null) abilityManager = gameObject.AddComponent<BossAbilities>();
            if (aiController == null) aiController = gameObject.AddComponent<BossAI>();
            
            // Ініціалізуємо підсистеми
            phaseManager.Initialize(this);
            abilityManager.Initialize(this);
            aiController.Initialize(this);
        }
        
        /// <summary>
        /// Налаштовує боса
        /// </summary>
        private void SetupBoss()
        {
            currentHealth = maxHealth;
            currentPhase = 1;
            
            // Підписуємося на події підсистем
            if (phaseManager != null)
            {
                phaseManager.OnPhaseTransition += HandlePhaseTransition;
            }
            
            if (abilityManager != null)
            {
                abilityManager.OnAbilityExecuted += HandleAbilityExecuted;
            }
            
            // Інтеграція з EventSystem
            EventSystem.Instance?.Subscribe("PlayerEnteredBossArea", OnPlayerEnteredArea);
            EventSystem.Instance?.Subscribe("PlayerLeftBossArea", OnPlayerLeftArea);
            
            Debug.Log($"Boss {bossName} initialized with {maxHealth} health and {totalPhases} phases");
        }
        
        /// <summary>
        /// Оновлює основну логіку боса
        /// </summary>
        private void UpdateBossLogic()
        {
            // Пошук цілі
            FindTarget();
            
            // Оновлення підсистем
            if (aiController != null) aiController.UpdateAI();
            if (abilityManager != null) abilityManager.UpdateAbilities();
        }
        
        /// <summary>
        /// Оновлює стан бою
        /// </summary>
        private void UpdateCombatState()
        {
            if (targetPlayer == null)
            {
                if (inCombat)
                {
                    ExitCombat();
                }
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
            
            if (distanceToPlayer <= aggroRange && !inCombat)
            {
                EnterCombat();
            }
            else if (distanceToPlayer > aggroRange * 1.5f && inCombat)
            {
                ExitCombat();
            }
        }
        
        /// <summary>
        /// Перевіряє переходи між фазами
        /// </summary>
        private void CheckPhaseTransitions()
        {
            if (phaseManager == null) return;
            
            float healthPercentage = (currentHealth / maxHealth) * 100f;
            
            for (int i = 0; i < phaseThresholds.Length; i++)
            {
                int phaseNumber = i + 2; // Фази починаються з 2
                
                if (healthPercentage <= phaseThresholds[i] && currentPhase < phaseNumber)
                {
                    TransitionToPhase(phaseNumber);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Шукає ціль для атаки
        /// </summary>
        private void FindTarget()
        {
            // Знаходимо найближчого гравця
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float closestDistance = float.MaxValue;
            Transform closestPlayer = null;
            
            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance && distance <= aggroRange)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
            
            targetPlayer = closestPlayer;
        }
        
        /// <summary>
        /// Входить у стан бою
        /// </summary>
        private void EnterCombat()
        {
            if (inCombat) return;
            
            inCombat = true;
            isActive = true;
            
            // Активуємо бойову музику та ефекти
            if (bossAnimator != null)
            {
                bossAnimator.SetBool("InCombat", true);
            }
            
            // Тригеримо події
            EventSystem.Instance?.TriggerEvent("BossEnterCombat", new {
                bossName = bossName,
                bossLevel = bossLevel,
                maxHealth = maxHealth
            });
            
            Debug.Log($"Boss {bossName} entered combat!");
        }
        
        /// <summary>
        /// Виходить зі стану бою
        /// </summary>
        private void ExitCombat()
        {
            if (!inCombat) return;
            
            inCombat = false;
            
            if (bossAnimator != null)
            {
                bossAnimator.SetBool("InCombat", false);
            }
            
            // Тригеримо події
            EventSystem.Instance?.TriggerEvent("BossExitCombat", new {
                bossName = bossName
            });
            
            Debug.Log($"Boss {bossName} exited combat");
        }
        
        /// <summary>
        /// Переходить до нової фази
        /// </summary>
        private void TransitionToPhase(int newPhase)
        {
            if (newPhase == currentPhase || phaseManager == null) return;
            
            int previousPhase = currentPhase;
            currentPhase = newPhase;
            
            // Викликаємо менеджер фаз
            phaseManager.TransitionToPhase(newPhase);
            
            // Тригеримо події
            OnPhaseChanged?.Invoke(newPhase);
            
            EventSystem.Instance?.TriggerEvent("BossPhaseChanged", new {
                bossName = bossName,
                previousPhase = previousPhase,
                newPhase = newPhase,
                healthPercentage = (currentHealth / maxHealth) * 100f
            });
            
            Debug.Log($"Boss {bossName} transitioned to phase {newPhase}");
        }
        
        /// <summary>
        /// Отримує пошкодження
        /// </summary>
        public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
        {
            if (isDead || isInvulnerable) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            // Тригеримо події
            OnHealthChanged?.Invoke(currentHealth);
            
            // Ефекти пошкодження
            ShowDamageEffect(hitPoint, damage);
            
            // Перевіряємо смерть
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Реакція на пошкодження
                ReactToDamage(damage, hitDirection);
            }
            
            EventSystem.Instance?.TriggerEvent("BossTookDamage", new {
                bossName = bossName,
                damage = damage,
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                hitPoint = hitPoint
            });
        }
        
        /// <summary>
        /// Реакція на пошкодження
        /// </summary>
        private void ReactToDamage(float damage, Vector3 hitDirection)
        {
            // Анімація пошкодження
            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger("TakeDamage");
            }
            
            // Можливо активувати лють при великому пошкодженні
            if (damage > maxHealth * 0.1f) // Більше 10% здоров'я
            {
                TriggerEnrage();
            }
        }
        
        /// <summary>
        /// Показує ефект пошкодження
        /// </summary>
        private void ShowDamageEffect(Vector3 hitPoint, float damage)
        {
            // Тут можна додати партикли, звуки, тощо
            
            // Створюємо текст пошкодження
            EventSystem.Instance?.TriggerEvent("ShowDamageNumber", new {
                position = hitPoint,
                damage = damage,
                isCritical = false
            });
        }
        
        /// <summary>
        /// Активує стан люті
        /// </summary>
        private void TriggerEnrage()
        {
            OnBossEnraged?.Invoke();
            
            EventSystem.Instance?.TriggerEvent("BossEnraged", new {
                bossName = bossName,
                currentPhase = currentPhase
            });
        }
        
        /// <summary>
        /// Смерть боса
        /// </summary>
        private void Die()
        {
            if (isDead) return;
            
            isDead = true;
            isActive = false;
            inCombat = false;
            
            // Анімація смерті
            if (bossAnimator != null)
            {
                bossAnimator.SetTrigger("Die");
            }
            
            // Тригеримо події
            OnBossDefeated?.Invoke();
            
            EventSystem.Instance?.TriggerEvent("BossDefeated", new {
                bossName = bossName,
                bossLevel = bossLevel,
                finalPhase = currentPhase
            });
            
            Debug.Log($"Boss {bossName} has been defeated!");
            
            // Запускаємо корутину смерті
            StartCoroutine(DeathSequence());
        }
        
        /// <summary>
        /// Послідовність смерті боса
        /// </summary>
        private System.Collections.IEnumerator DeathSequence()
        {
            // Чекаємо завершення анімації смерті
            yield return new WaitForSeconds(3f);
            
            // Дропаємо лут
            DropLoot();
            
            // Ще трохи чекаємо
            yield return new WaitForSeconds(2f);
            
            // Знищуємо об'єкт боса
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Дропає лут після смерті
        /// </summary>
        private void DropLoot()
        {
            EventSystem.Instance?.TriggerEvent("BossLootDropped", new {
                bossName = bossName,
                bossLevel = bossLevel,
                dropPosition = transform.position
            });
        }
        
        // === Обробники подій ===
        
        private void HandlePhaseTransition(int newPhase)
        {
            Debug.Log($"Boss phase transition handled: {newPhase}");
        }
        
        private void HandleAbilityExecuted(string abilityName)
        {
            OnAbilityUsed?.Invoke(abilityName);
        }
        
        private void OnPlayerEnteredArea(object data)
        {
            // Гравець увійшов в зону боса
        }
        
        private void OnPlayerLeftArea(object data)
        {
            // Гравець покинув зону боса
        }
        
        // === Публічні методи ===
        
        public float GetHealthPercentage() => (currentHealth / maxHealth) * 100f;
        public bool IsInCombat() => inCombat;
        public bool IsDead() => isDead;
        public Transform GetTarget() => targetPlayer;
        public int GetCurrentPhase() => currentPhase;
        public BossType GetBossType() => bossType;
        
        public void SetInvulnerable(bool invulnerable)
        {
            isInvulnerable = invulnerable;
        }
        
        public void ForcePhaseTransition(int phase)
        {
            if (phase > 0 && phase <= totalPhases)
            {
                TransitionToPhase(phase);
            }
        }
        
        void OnDestroy()
        {
            // Відписуємося від подій
            EventSystem.Instance?.Unsubscribe("PlayerEnteredBossArea", OnPlayerEnteredArea);
            EventSystem.Instance?.Unsubscribe("PlayerLeftBossArea", OnPlayerLeftArea);
        }
        
        // Візуалізація в редакторі
        void OnDrawGizmosSelected()
        {
            // Радіус агро
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            
            // Радіус атаки
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
    
    /// <summary>
    /// Типи босів
    /// </summary>
    public enum BossType
    {
        Standard,   // Звичайний бос
        Elite,      // Елітний бос
        Raid,       // Рейдовий бос
        World       // Світовий бос
    }
}