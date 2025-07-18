using UnityEngine;
using System.Collections;
using IndieShooter.Core;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Елітні типи ворогів. Містить складних ворогів з унікальними здібностями.
    /// Розділено з монолітного EnemyTypes.cs (917 рядків).
    /// </summary>
    public class EliteEnemies : MonoBehaviour
    {
        [Header("Elite Enemy Prefabs")]
        [Tooltip("Префаб снайпера")]
        public GameObject sniperEnemyPrefab;
        [Tooltip("Префаб важкого ворога")]
        public GameObject heavyEnemyPrefab;
        [Tooltip("Префаб боса")]
        public GameObject bossEnemyPrefab;
        
        /// <summary>
        /// Створює снайпера
        /// </summary>
        public GameObject CreateSniperEnemy(Vector3 position, Quaternion rotation)
        {
            if (sniperEnemyPrefab == null)
            {
                Debug.LogError("EliteEnemies: Sniper enemy prefab not assigned!");
                return null;
            }
            
            var enemy = Instantiate(sniperEnemyPrefab, position, rotation);
            var sniperController = enemy.GetComponent<SniperEnemy>();
            
            if (sniperController == null)
            {
                sniperController = enemy.AddComponent<SniperEnemy>();
            }
            
            SetupSniperEnemy(sniperController);
            return enemy;
        }
        
        /// <summary>
        /// Налаштовує снайпера
        /// </summary>
        private void SetupSniperEnemy(SniperEnemy sniper)
        {
            sniper.enemyType = EnemyType.Ranged;
            sniper.maxHealth = 150f;
            sniper.moveSpeed = 2f;
            sniper.attackDamage = 80f;
            sniper.attackRange = 25f;
            sniper.detectionRange = 30f;
            sniper.aimTime = 2f;
            sniper.accuracy = 0.9f;
        }
    }
    
    /// <summary>
    /// Снайпер - дальнобійний ворог з високою точністю
    /// </summary>
    public class SniperEnemy : EnemyController
    {
        [Header("Sniper Settings")]
        [Tooltip("Час прицілювання")]
        public float aimTime = 2f;
        [Tooltip("Точність стрільби")]
        [Range(0f, 1f)]
        public float accuracy = 0.9f;
        [Tooltip("Лазерний приціл")]
        public LineRenderer laserSight;
        [Tooltip("Позиції для снайпінгу")]
        public Transform[] snipingPositions;
        
        // Стан снайпера
        private bool isAiming = false;
        private float aimTimer = 0f;
        private Transform currentSnipingPosition;
        private bool hasLineOfSight = false;
        
        protected override void Start()
        {
            base.Start();
            FindBestSnipingPosition();
            
            if (laserSight == null)
            {
                laserSight = GetComponent<LineRenderer>();
            }
        }
        
        protected override void Update()
        {
            if (isDead) return;
            
            base.Update();
            UpdateSniperBehavior();
        }
        
        /// <summary>
        /// Оновлює поведінку снайпера
        /// </summary>
        private void UpdateSniperBehavior()
        {
            if (target == null) return;
            
            CheckLineOfSight();
            
            if (hasLineOfSight)
            {
                if (!isAiming)
                {
                    StartAiming();
                }
                else
                {
                    UpdateAiming();
                }
            }
            else
            {
                if (isAiming)
                {
                    StopAiming();
                }
                FindBetterPosition();
            }
        }
        
        /// <summary>
        /// Перевіряє лінію видимості до цілі
        /// </summary>
        private void CheckLineOfSight()
        {
            if (target == null) return;
            
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToTarget, out hit, distanceToTarget))
            {
                hasLineOfSight = hit.transform == target;
            }
            else
            {
                hasLineOfSight = true;
            }
        }
        
        /// <summary>
        /// Починає прицілювання
        /// </summary>
        private void StartAiming()
        {
            isAiming = true;
            aimTimer = 0f;
            
            if (laserSight != null)
            {
                laserSight.enabled = true;
            }
            
            // Анімація прицілювання
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("IsAiming", true);
            }
        }
        
        /// <summary>
        /// Оновлює прицілювання
        /// </summary>
        private void UpdateAiming()
        {
            aimTimer += Time.deltaTime;
            
            // Оновлюємо лазерний приціл
            if (laserSight != null && target != null)
            {
                laserSight.SetPosition(0, transform.position + Vector3.up * 1.5f);
                laserSight.SetPosition(1, target.position + Vector3.up * 1f);
            }
            
            // Стріляємо після завершення прицілювання
            if (aimTimer >= aimTime)
            {
                FireSniperShot();
                StopAiming();
            }
        }
        
        /// <summary>
        /// Робить снайперський постріл
        /// </summary>
        private void FireSniperShot()
        {
            if (target == null) return;
            
            // Розрахунок точності
            Vector3 targetPosition = target.position + Vector3.up * 1f;
            if (Random.value > accuracy)
            {
                // Промах - додаємо розкид
                Vector3 spread = Random.insideUnitSphere * 2f;
                targetPosition += spread;
            }
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            // Перевіряємо попадання
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out hit, attackRange))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    var playerHealth = hit.transform.GetComponent<MonoBehaviour>(); // PlayerHealth
                    if (playerHealth != null)
                    {
                        // playerHealth.TakeDamage(attackDamage, "Sniper Enemy");
                    }
                    
                    // Ефект попадання
                    ShowHitEffect(hit.point, direction);
                }
            }
            
            // Звук пострілу
            PlayShotSound();
            
            // Анімація пострілу
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Shoot");
            }
            
            Debug.Log($"Sniper fired at target!");
        }
        
        /// <summary>
        /// Зупиняє прицілювання
        /// </summary>
        private void StopAiming()
        {
            isAiming = false;
            aimTimer = 0f;
            
            if (laserSight != null)
            {
                laserSight.enabled = false;
            }
            
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("IsAiming", false);
            }
            
            // Після пострілу снайпер може змінити позицію
            if (Random.value < 0.6f)
            {
                FindBestSnipingPosition();
            }
        }
        
        /// <summary>
        /// Знаходить найкращу позицію для снайпінгу
        /// </summary>
        private void FindBestSnipingPosition()
        {
            if (snipingPositions == null || snipingPositions.Length == 0) return;
            
            Transform bestPosition = null;
            float bestScore = float.MinValue;
            
            foreach (var position in snipingPositions)
            {
                if (position == null) continue;
                
                float score = EvaluateSnipingPosition(position);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPosition = position;
                }
            }
            
            if (bestPosition != null && bestPosition != currentSnipingPosition)
            {
                currentSnipingPosition = bestPosition;
                StartCoroutine(MoveToSnipingPosition(bestPosition));
            }
        }
        
        /// <summary>
        /// Оцінює якість позиції для снайпінгу
        /// </summary>
        private float EvaluateSnipingPosition(Transform position)
        {
            if (target == null) return 0f;
            
            float score = 0f;
            
            // Відстань до цілі (оптимальна відстань)
            float distanceToTarget = Vector3.Distance(position.position, target.position);
            if (distanceToTarget >= attackRange * 0.7f && distanceToTarget <= attackRange)
            {
                score += 50f;
            }
            
            // Висота позиції (вища позиція краща)
            if (position.position.y > target.position.y)
            {
                score += 30f;
            }
            
            // Лінія видимості
            Vector3 directionToTarget = (target.position - position.position).normalized;
            RaycastHit hit;
            if (!Physics.Raycast(position.position + Vector3.up * 1.5f, directionToTarget, out hit, distanceToTarget))
            {
                score += 40f;
            }
            
            return score;
        }
        
        /// <summary>
        /// Переміщується до позиції снайпінгу
        /// </summary>
        private IEnumerator MoveToSnipingPosition(Transform targetPosition)
        {
            float moveTime = Vector3.Distance(transform.position, targetPosition.position) / moveSpeed;
            float elapsedTime = 0f;
            Vector3 startPosition = transform.position;
            
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / moveTime;
                
                transform.position = Vector3.Lerp(startPosition, targetPosition.position, t);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, t);
                
                yield return null;
            }
            
            transform.position = targetPosition.position;
            transform.rotation = targetPosition.rotation;
        }
        
        /// <summary>
        /// Шукає кращу позицію якщо немає лінії видимості
        /// </summary>
        private void FindBetterPosition()
        {
            if (Time.time % 2f < 0.1f) // Перевіряємо кожні 2 секунди
            {
                FindBestSnipingPosition();
            }
        }
        
        /// <summary>
        /// Показує ефект попадання
        /// </summary>
        private void ShowHitEffect(Vector3 hitPoint, Vector3 direction)
        {
            // Тут можна додати партикли попадання
            EventSystem.Instance?.TriggerEvent("SniperHit", new {
                position = hitPoint,
                direction = direction
            });
        }
        
        /// <summary>
        /// Відтворює звук пострілу
        /// </summary>
        private void PlayShotSound()
        {
            // Тут можна додати звук снайперського пострілу
            EventSystem.Instance?.TriggerEvent("SniperShot", new {
                position = transform.position
            });
        }
        
        /// <summary>
        /// Реакція на пошкодження - снайпер переміщується
        /// </summary>
        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            
            // Снайпер переміщується при отриманні урону
            if (!isDead)
            {
                StopAiming();
                FindBestSnipingPosition();
            }
        }
        
        /// <summary>
        /// Знаходить ціль з урахуванням дальності
        /// </summary>
        protected override void FindTarget()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float closestDistance = float.MaxValue;
            Transform closestPlayer = null;
            
            foreach (GameObject player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance && distance <= detectionRange)
                {
                    // Перевіряємо лінію видимості
                    Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                    RaycastHit hit;
                    if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, directionToPlayer, out hit, distance) ||
                        hit.transform == player.transform)
                    {
                        closestDistance = distance;
                        closestPlayer = player.transform;
                    }
                }
            }
            
            target = closestPlayer;
        }
        
        void OnDrawGizmosSelected()
        {
            // Візуалізація радіусу атаки
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Візуалізація радіусу виявлення
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Лінія до цілі
            if (target != null)
            {
                Gizmos.color = hasLineOfSight ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, target.position + Vector3.up * 1f);
            }
        }
    }
}