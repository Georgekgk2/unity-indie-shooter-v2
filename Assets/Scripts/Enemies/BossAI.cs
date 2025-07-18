using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Система штучного інтелекту боса. Управляє поведінкою, прийняттям рішень та тактикою.
    /// </summary>
    public class BossAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [Tooltip("Тип поведінки боса")]
        public BossAIBehavior currentBehavior = BossAIBehavior.Aggressive;
        [Tooltip("Інтервал прийняття рішень (секунди)")]
        public float decisionInterval = 1f;
        [Tooltip("Радіус пошуку укриття")]
        public float coverSearchRadius = 15f;
        [Tooltip("Мінімальна відстань до гравця")]
        public float minDistanceToPlayer = 3f;
        [Tooltip("Максимальна відстань до гравця")]
        public float maxDistanceToPlayer = 15f;
        
        [Header("Combat AI")]
        [Tooltip("Ймовірність використання здібностей")]
        [Range(0f, 1f)]
        public float abilityUseProbability = 0.7f;
        [Tooltip("Ймовірність пошуку укриття при низькому здоров'ї")]
        [Range(0f, 1f)]
        public float seekCoverProbability = 0.8f;
        [Tooltip("Поріг здоров'я для пошуку укриття")]
        [Range(0f, 1f)]
        public float lowHealthThreshold = 0.3f;
        
        // Посилання на компоненти
        private BossController bossController;
        private BossAbilities abilityManager;
        private BossPhases phaseManager;
        
        // Стан AI
        private AIState currentState = AIState.Idle;
        private Vector3 targetPosition;
        private float lastDecisionTime = 0f;
        private bool isExecutingAction = false;
        private Queue<AIAction> actionQueue = new Queue<AIAction>();
        
        // Навігація та рух
        private Vector3 lastKnownPlayerPosition;
        private float playerLostTime = 0f;
        private bool hasLineOfSight = false;
        
        public void Initialize(BossController controller)
        {
            bossController = controller;
            abilityManager = controller.GetComponent<BossAbilities>();
            phaseManager = controller.GetComponent<BossPhases>();
        }
        
        void Start()
        {
            ValidateSetup();
            ChangeState(AIState.Idle);
        }
        
        /// <summary>
        /// Оновлює AI логіку
        /// </summary>
        public void UpdateAI()
        {
            if (bossController == null || bossController.IsDead()) return;
            
            UpdatePerception();
            UpdateDecisionMaking();
            UpdateCurrentState();
            ExecuteQueuedActions();
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (bossController == null)
            {
                Debug.LogError("BossAI: BossController not initialized!", this);
                enabled = false;
                return;
            }
        }
        
        /// <summary>
        /// Оновлює сприйняття навколишнього середовища
        /// </summary>
        private void UpdatePerception()
        {
            var target = bossController.GetTarget();
            if (target != null)
            {
                lastKnownPlayerPosition = target.position;
                playerLostTime = 0f;
                hasLineOfSight = CheckLineOfSight(target);
            }
            else
            {
                playerLostTime += Time.deltaTime;
                hasLineOfSight = false;
            }
        }
        
        /// <summary>
        /// Перевіряє лінію видимості до цілі
        /// </summary>
        private bool CheckLineOfSight(Transform target)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToTarget, out hit, distanceToTarget))
            {
                return hit.transform == target;
            }
            
            return true;
        }
        
        /// <summary>
        /// Оновлює процес прийняття рішень
        /// </summary>
        private void UpdateDecisionMaking()
        {
            if (Time.time - lastDecisionTime < decisionInterval) return;
            if (isExecutingAction) return;
            
            lastDecisionTime = Time.time;
            
            // Аналізуємо ситуацію та приймаємо рішення
            AnalyzeSituationAndDecide();
        }
        
        /// <summary>
        /// Аналізує ситуацію та приймає рішення
        /// </summary>
        private void AnalyzeSituationAndDecide()
        {
            var target = bossController.GetTarget();
            if (target == null)
            {
                HandleNoTarget();
                return;
            }
            
            float healthPercentage = bossController.GetHealthPercentage() / 100f;
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            
            // Визначаємо пріоритети на основі ситуації
            List<AIDecision> possibleDecisions = EvaluatePossibleDecisions(target, healthPercentage, distanceToPlayer);
            
            // Вибираємо найкраще рішення
            AIDecision bestDecision = SelectBestDecision(possibleDecisions);
            
            if (bestDecision != null)
            {
                ExecuteDecision(bestDecision);
            }
        }
        
        /// <summary>
        /// Оцінює можливі рішення
        /// </summary>
        private List<AIDecision> EvaluatePossibleDecisions(Transform target, float healthPercentage, float distanceToPlayer)
        {
            var decisions = new List<AIDecision>();
            
            // Рішення про атаку
            if (hasLineOfSight && distanceToPlayer <= bossController.attackRange)
            {
                decisions.Add(new AIDecision
                {
                    type = AIDecisionType.Attack,
                    priority = CalculateAttackPriority(distanceToPlayer, healthPercentage),
                    target = target.position
                });
            }
            
            // Рішення про використання здібностей
            if (abilityManager != null && Random.value < abilityUseProbability)
            {
                var availableAbilities = abilityManager.GetAvailableAbilities();
                foreach (var ability in availableAbilities)
                {
                    decisions.Add(new AIDecision
                    {
                        type = AIDecisionType.UseAbility,
                        priority = CalculateAbilityPriority(ability, distanceToPlayer, healthPercentage),
                        abilityName = ability,
                        target = target.position
                    });
                }
            }
            
            // Рішення про рух
            Vector3 optimalPosition = CalculateOptimalPosition(target, distanceToPlayer);
            if (Vector3.Distance(transform.position, optimalPosition) > 1f)
            {
                decisions.Add(new AIDecision
                {
                    type = AIDecisionType.Move,
                    priority = CalculateMovePriority(distanceToPlayer, healthPercentage),
                    target = optimalPosition
                });
            }
            
            // Рішення про пошук укриття при низькому здоров'ї
            if (healthPercentage < lowHealthThreshold && Random.value < seekCoverProbability)
            {
                Vector3 coverPosition = FindCoverPosition(target);
                if (coverPosition != Vector3.zero)
                {
                    decisions.Add(new AIDecision
                    {
                        type = AIDecisionType.SeekCover,
                        priority = CalculateCoverPriority(healthPercentage),
                        target = coverPosition
                    });
                }
            }
            
            return decisions;
        }
        
        /// <summary>
        /// Розраховує пріоритет атаки
        /// </summary>
        private float CalculateAttackPriority(float distance, float healthPercentage)
        {
            float priority = 50f;
            
            // Чим ближче гравець, тим вищий пріоритет атаки
            priority += (bossController.attackRange - distance) * 10f;
            
            // При низькому здоров'ї пріоритет атаки знижується
            if (healthPercentage < 0.3f)
            {
                priority *= 0.7f;
            }
            
            return priority;
        }
        
        /// <summary>
        /// Розраховує пріоритет використання здібності
        /// </summary>
        private float CalculateAbilityPriority(string abilityName, float distance, float healthPercentage)
        {
            float priority = 30f;
            
            // Різні здібності мають різні пріоритети залежно від ситуації
            switch (abilityName.ToLower())
            {
                case "charge":
                    priority += distance > 5f ? 20f : -10f; // Краще на відстані
                    break;
                case "areaattack":
                    priority += distance < 3f ? 25f : -5f; // Краще вблизи
                    break;
                case "heal":
                    priority += healthPercentage < 0.5f ? 40f : -20f; // При низькому здоров'ї
                    break;
                case "ultimateability":
                    priority += healthPercentage < 0.3f ? 50f : 10f; // Ультимейт при критичному здоров'ї
                    break;
            }
            
            return priority;
        }
        
        /// <summary>
        /// Розраховує пріоритет руху
        /// </summary>
        private float CalculateMovePriority(float distance, float healthPercentage)
        {
            float priority = 20f;
            
            // Якщо занадто близько або далеко
            if (distance < minDistanceToPlayer || distance > maxDistanceToPlayer)
            {
                priority += 30f;
            }
            
            // При низькому здоров'ї рух важливіший
            if (healthPercentage < 0.4f)
            {
                priority += 20f;
            }
            
            return priority;
        }
        
        /// <summary>
        /// Розраховує пріоритет пошуку укриття
        /// </summary>
        private float CalculateCoverPriority(float healthPercentage)
        {
            return (1f - healthPercentage) * 100f; // Чим менше здоров'я, тим вищий пріоритет
        }
        
        /// <summary>
        /// Вибирає найкраще рішення
        /// </summary>
        private AIDecision SelectBestDecision(List<AIDecision> decisions)
        {
            if (decisions.Count == 0) return null;
            
            AIDecision bestDecision = decisions[0];
            foreach (var decision in decisions)
            {
                if (decision.priority > bestDecision.priority)
                {
                    bestDecision = decision;
                }
            }
            
            return bestDecision;
        }
        
        /// <summary>
        /// Виконує прийняте рішення
        /// </summary>
        private void ExecuteDecision(AIDecision decision)
        {
            var action = new AIAction
            {
                type = decision.type,
                target = decision.target,
                abilityName = decision.abilityName,
                startTime = Time.time
            };
            
            actionQueue.Enqueue(action);
        }
        
        /// <summary>
        /// Виконує дії з черги
        /// </summary>
        private void ExecuteQueuedActions()
        {
            if (actionQueue.Count == 0 || isExecutingAction) return;
            
            var action = actionQueue.Dequeue();
            StartCoroutine(ExecuteAction(action));
        }
        
        /// <summary>
        /// Виконує конкретну дію
        /// </summary>
        private IEnumerator ExecuteAction(AIAction action)
        {
            isExecutingAction = true;
            
            switch (action.type)
            {
                case AIDecisionType.Attack:
                    yield return StartCoroutine(ExecuteAttack(action));
                    break;
                    
                case AIDecisionType.UseAbility:
                    yield return StartCoroutine(ExecuteAbility(action));
                    break;
                    
                case AIDecisionType.Move:
                    yield return StartCoroutine(ExecuteMove(action));
                    break;
                    
                case AIDecisionType.SeekCover:
                    yield return StartCoroutine(ExecuteSeekCover(action));
                    break;
            }
            
            isExecutingAction = false;
        }
        
        /// <summary>
        /// Виконує атаку
        /// </summary>
        private IEnumerator ExecuteAttack(AIAction action)
        {
            ChangeState(AIState.Attacking);
            
            // Поворот до цілі
            Vector3 directionToTarget = (action.target - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            
            yield return new WaitForSeconds(0.5f); // Час підготовки до атаки
            
            // Виконання атаки через BossController або BossAbilities
            if (abilityManager != null)
            {
                abilityManager.UseAbility("BasicAttack");
            }
            
            yield return new WaitForSeconds(1f); // Час відновлення після атаки
        }
        
        /// <summary>
        /// Виконує здібність
        /// </summary>
        private IEnumerator ExecuteAbility(AIAction action)
        {
            ChangeState(AIState.UsingAbility);
            
            if (abilityManager != null && !string.IsNullOrEmpty(action.abilityName))
            {
                yield return StartCoroutine(abilityManager.UseAbilityCoroutine(action.abilityName));
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Виконує рух
        /// </summary>
        private IEnumerator ExecuteMove(AIAction action)
        {
            ChangeState(AIState.Moving);
            
            Vector3 startPosition = transform.position;
            float moveDistance = Vector3.Distance(startPosition, action.target);
            float moveTime = moveDistance / (bossController.moveSpeed * GetCurrentSpeedMultiplier());
            float elapsedTime = 0f;
            
            while (elapsedTime < moveTime && Vector3.Distance(transform.position, action.target) > 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveTime;
                
                transform.position = Vector3.Lerp(startPosition, action.target, t);
                
                // Поворот під час руху
                Vector3 moveDirection = (action.target - transform.position).normalized;
                if (moveDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(moveDirection);
                }
                
                yield return null;
            }
            
            transform.position = action.target;
        }
        
        /// <summary>
        /// Виконує пошук укриття
        /// </summary>
        private IEnumerator ExecuteSeekCover(AIAction action)
        {
            ChangeState(AIState.SeekingCover);
            
            // Рухаємося до укриття
            yield return StartCoroutine(ExecuteMove(action));
            
            // Чекаємо в укритті
            yield return new WaitForSeconds(2f);
        }
        
        /// <summary>
        /// Розраховує оптимальну позицію відносно гравця
        /// </summary>
        private Vector3 CalculateOptimalPosition(Transform target, float currentDistance)
        {
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            float optimalDistance = (minDistanceToPlayer + maxDistanceToPlayer) / 2f;
            
            Vector3 optimalPosition;
            
            if (currentDistance < minDistanceToPlayer)
            {
                // Відходимо від гравця
                optimalPosition = transform.position - directionToPlayer * (minDistanceToPlayer - currentDistance + 1f);
            }
            else if (currentDistance > maxDistanceToPlayer)
            {
                // Наближаємося до гравця
                optimalPosition = transform.position + directionToPlayer * (currentDistance - maxDistanceToPlayer + 1f);
            }
            else
            {
                // Рухаємося по колу навколо гравця
                Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
                optimalPosition = target.position + perpendicular * optimalDistance;
            }
            
            return optimalPosition;
        }
        
        /// <summary>
        /// Знаходить позицію укриття
        /// </summary>
        private Vector3 FindCoverPosition(Transform target)
        {
            Vector3 directionFromPlayer = (transform.position - target.position).normalized;
            Vector3 coverPosition = transform.position + directionFromPlayer * coverSearchRadius;
            
            // Перевіряємо чи є перешкоди для укриття
            RaycastHit hit;
            if (Physics.Raycast(coverPosition, target.position - coverPosition, out hit, Vector3.Distance(coverPosition, target.position)))
            {
                return hit.point + hit.normal * 2f; // Позиція за перешкодою
            }
            
            return Vector3.zero; // Укриття не знайдено
        }
        
        /// <summary>
        /// Обробляє відсутність цілі
        /// </summary>
        private void HandleNoTarget()
        {
            if (playerLostTime > 5f)
            {
                ChangeState(AIState.Patrolling);
                // Можна додати логіку патрулювання
            }
            else
            {
                // Йдемо до останньої відомої позиції гравця
                if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 1f)
                {
                    var moveAction = new AIAction
                    {
                        type = AIDecisionType.Move,
                        target = lastKnownPlayerPosition,
                        startTime = Time.time
                    };
                    actionQueue.Enqueue(moveAction);
                }
            }
        }
        
        /// <summary>
        /// Оновлює поточний стан AI
        /// </summary>
        private void UpdateCurrentState()
        {
            // Логіка оновлення стану залежно від поточної ситуації
            switch (currentState)
            {
                case AIState.Idle:
                    if (bossController.GetTarget() != null)
                    {
                        ChangeState(AIState.Engaging);
                    }
                    break;
                    
                case AIState.Engaging:
                    if (bossController.GetTarget() == null)
                    {
                        ChangeState(AIState.Idle);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Змінює стан AI
        /// </summary>
        private void ChangeState(AIState newState)
        {
            if (currentState == newState) return;
            
            AIState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"Boss AI state changed from {previousState} to {newState}");
        }
        
        /// <summary>
        /// Отримує поточний множник швидкості
        /// </summary>
        private float GetCurrentSpeedMultiplier()
        {
            if (phaseManager != null)
            {
                return phaseManager.GetCurrentSpeedMultiplier();
            }
            return 1f;
        }
        
        // === Публічні методи ===
        
        public AIState GetCurrentState() => currentState;
        public bool IsExecutingAction() => isExecutingAction;
        public bool HasLineOfSight() => hasLineOfSight;
        
        public void SetBehavior(BossAIBehavior behavior)
        {
            currentBehavior = behavior;
            // Можна додати логіку зміни поведінки
        }
        
        public void ForceAction(AIDecisionType actionType, Vector3 target, string abilityName = "")
        {
            var action = new AIAction
            {
                type = actionType,
                target = target,
                abilityName = abilityName,
                startTime = Time.time
            };
            
            actionQueue.Enqueue(action);
        }
        
        void OnDrawGizmosSelected()
        {
            // Візуалізація AI стану
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, coverSearchRadius);
            
            // Лінія до цілі
            if (bossController != null && bossController.GetTarget() != null)
            {
                Gizmos.color = hasLineOfSight ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, bossController.GetTarget().position);
            }
            
            // Оптимальна зона
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(transform.position, maxDistanceToPlayer);
        }
    }
    
    /// <summary>
    /// Стани AI боса
    /// </summary>
    public enum AIState
    {
        Idle,           // Бездіяльність
        Patrolling,     // Патрулювання
        Engaging,       // Вступ в бій
        Attacking,      // Атака
        UsingAbility,   // Використання здібності
        Moving,         // Рух
        SeekingCover,   // Пошук укриття
        Stunned         // Приголомшений
    }
    
    /// <summary>
    /// Типи рішень AI
    /// </summary>
    public enum AIDecisionType
    {
        Attack,         // Атакувати
        UseAbility,     // Використати здібність
        Move,           // Рухатися
        SeekCover,      // Шукати укриття
        Patrol,         // Патрулювати
        Wait            // Чекати
    }
    
    /// <summary>
    /// Рішення AI
    /// </summary>
    public class AIDecision
    {
        public AIDecisionType type;
        public float priority;
        public Vector3 target;
        public string abilityName;
    }
    
    /// <summary>
    /// Дія AI
    /// </summary>
    public class AIAction
    {
        public AIDecisionType type;
        public Vector3 target;
        public string abilityName;
        public float startTime;
    }
}