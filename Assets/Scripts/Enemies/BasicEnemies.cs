using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Базові типи ворогів. Містить простих ворогів для початкових рівнів.
    /// Розділено з монолітного EnemyTypes.cs (917 рядків).
    /// </summary>
    public class BasicEnemies : MonoBehaviour
    {
        [Header("Basic Enemy Settings")]
        [Tooltip("Префаб базового ворога")]
        public GameObject basicEnemyPrefab;
        [Tooltip("Префаб швидкого ворога")]
        public GameObject fastEnemyPrefab;
        [Tooltip("Префаб важкого ворога")]
        public GameObject heavyEnemyPrefab;
        
        /// <summary>
        /// Створює базового ворога
        /// </summary>
        public GameObject CreateBasicEnemy(Vector3 position, Quaternion rotation)
        {
            if (basicEnemyPrefab == null)
            {
                Debug.LogError("BasicEnemies: Basic enemy prefab not assigned!");
                return null;
            }
            
            var enemy = Instantiate(basicEnemyPrefab, position, rotation);
            var enemyController = enemy.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                SetupBasicEnemy(enemyController);
            }
            
            return enemy;
        }
        
        /// <summary>
        /// Налаштовує базового ворога
        /// </summary>
        private void SetupBasicEnemy(EnemyController controller)
        {
            controller.enemyType = EnemyType.Basic;
            controller.maxHealth = 100f;
            controller.moveSpeed = 3f;
            controller.attackDamage = 20f;
            controller.attackRange = 2f;
            controller.detectionRange = 10f;
        }
        
        /// <summary>
        /// Створює швидкого ворога
        /// </summary>
        public GameObject CreateFastEnemy(Vector3 position, Quaternion rotation)
        {
            if (fastEnemyPrefab == null)
            {
                Debug.LogError("BasicEnemies: Fast enemy prefab not assigned!");
                return null;
            }
            
            var enemy = Instantiate(fastEnemyPrefab, position, rotation);
            var enemyController = enemy.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                SetupFastEnemy(enemyController);
            }
            
            return enemy;
        }
        
        /// <summary>
        /// Налаштовує швидкого ворога
        /// </summary>
        private void SetupFastEnemy(EnemyController controller)
        {
            controller.enemyType = EnemyType.Fast;
            controller.maxHealth = 60f;
            controller.moveSpeed = 6f;
            controller.attackDamage = 15f;
            controller.attackRange = 1.5f;
            controller.detectionRange = 12f;
        }
        
        /// <summary>
        /// Створює важкого ворога
        /// </summary>
        public GameObject CreateHeavyEnemy(Vector3 position, Quaternion rotation)
        {
            if (heavyEnemyPrefab == null)
            {
                Debug.LogError("BasicEnemies: Heavy enemy prefab not assigned!");
                return null;
            }
            
            var enemy = Instantiate(heavyEnemyPrefab, position, rotation);
            var enemyController = enemy.GetComponent<EnemyController>();
            
            if (enemyController != null)
            {
                SetupHeavyEnemy(enemyController);
            }
            
            return enemy;
        }
        
        /// <summary>
        /// Налаштовує важкого ворога
        /// </summary>
        private void SetupHeavyEnemy(EnemyController controller)
        {
            controller.enemyType = EnemyType.Heavy;
            controller.maxHealth = 300f;
            controller.moveSpeed = 1.5f;
            controller.attackDamage = 40f;
            controller.attackRange = 3f;
            controller.detectionRange = 8f;
        }
    }
    
    /// <summary>
    /// Базовий контролер ворога
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        public EnemyType enemyType = EnemyType.Basic;
        public float maxHealth = 100f;
        public float currentHealth;
        public float moveSpeed = 3f;
        public float attackDamage = 20f;
        public float attackRange = 2f;
        public float detectionRange = 10f;
        
        [Header("Behavior")]
        public bool isAggressive = true;
        public bool canPatrol = true;
        public Transform[] patrolPoints;
        
        // Стан ворога
        private Transform target;
        private bool isDead = false;
        private float lastAttackTime = 0f;
        private float attackCooldown = 1f;
        
        // Компоненти
        private Rigidbody enemyRigidbody;
        private Animator enemyAnimator;
        
        void Awake()
        {
            enemyRigidbody = GetComponent<Rigidbody>();
            enemyAnimator = GetComponent<Animator>();
        }
        
        void Start()
        {
            currentHealth = maxHealth;
        }
        
        void Update()
        {
            if (isDead) return;
            
            FindTarget();
            UpdateBehavior();
        }
        
        /// <summary>
        /// Шукає ціль для атаки
        /// </summary>
        private void FindTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= detectionRange)
                {
                    target = player.transform;
                }
                else if (distance > detectionRange * 1.5f)
                {
                    target = null;
                }
            }
        }
        
        /// <summary>
        /// Оновлює поведінку ворога
        /// </summary>
        private void UpdateBehavior()
        {
            if (target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                
                if (distanceToTarget <= attackRange)
                {
                    AttackTarget();
                }
                else
                {
                    MoveTowardsTarget();
                }
            }
            else if (canPatrol)
            {
                Patrol();
            }
        }
        
        /// <summary>
        /// Рухається до цілі
        /// </summary>
        private void MoveTowardsTarget()
        {
            if (target == null) return;
            
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(target);
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("IsMoving", true);
            }
        }
        
        /// <summary>
        /// Атакує ціль
        /// </summary>
        private void AttackTarget()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;
            
            lastAttackTime = Time.time;
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Attack");
            }
            
            // Завдаємо пошкодження цілі
            var playerHealth = target.GetComponent<MonoBehaviour>(); // PlayerHealth
            if (playerHealth != null)
            {
                // playerHealth.TakeDamage(attackDamage);
            }
        }
        
        /// <summary>
        /// Патрулювання
        /// </summary>
        private void Patrol()
        {
            // Логіка патрулювання між точками
        }
        
        /// <summary>
        /// Отримує пошкодження
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Реакція на пошкодження
                if (enemyAnimator != null)
                {
                    enemyAnimator.SetTrigger("TakeDamage");
                }
            }
        }
        
        /// <summary>
        /// Смерть ворога
        /// </summary>
        private void Die()
        {
            isDead = true;
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Die");
            }
            
            // Тригеримо подію смерті
            EventSystem.Instance?.TriggerEvent("EnemyDied", new {
                enemyType = enemyType,
                position = transform.position
            });
            
            // Знищуємо через деякий час
            Destroy(gameObject, 3f);
        }
    }
    
    /// <summary>
    /// Типи ворогів
    /// </summary>
    public enum EnemyType
    {
        Basic,      // Базовий ворог
        Fast,       // Швидкий ворог
        Heavy,      // Важкий ворог
        Ranged,     // Дальнобійний ворог
        Elite,      // Елітний ворог
        Boss        // Бос
    }
}