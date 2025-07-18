using UnityEngine;
using UnityEngine.AI;
using IndieShooter.Core;

namespace IndieShooter.AI
{
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Search,
        Dead,
        Alert
    }
    
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class AIController : MonoBehaviour
    {
        [Header("AI Settings")]
        public AIState currentState = AIState.Patrol;
        public float health = 100f;
        public float maxHealth = 100f;
        
        [Header("Detection")]
        public float detectionRange = 15f;
        public float attackRange = 8f;
        public float fieldOfView = 120f;
        public LayerMask playerLayerMask = 1 << 8; // Player layer
        public LayerMask obstacleLayerMask = 1 << 13; // Wall layer
        
        [Header("Movement")]
        public float walkSpeed = 2f;
        public float runSpeed = 5f;
        public float rotationSpeed = 5f;
        
        [Header("Combat")]
        public float damage = 20f;
        public float fireRate = 1f;
        public float accuracy = 0.8f;
        public Transform firePoint;
        
        [Header("Patrol")]
        public Transform[] patrolPoints;
        public float waitTime = 2f;
        public bool randomPatrol = false;
        
        [Header("Search")]
        public float searchTime = 10f;
        public float searchRadius = 5f;
        
        // Components
        private NavMeshAgent navAgent;
        private Animator animator;
        private Transform player;
        
        // State variables
        private int currentPatrolIndex = 0;
        private float lastAttackTime = 0f;
        private float stateTimer = 0f;
        private Vector3 lastKnownPlayerPosition;
        private bool playerDetected = false;
        private float alertTimer = 0f;
        
        // Animation hashes
        private int speedHash;
        private int attackHash;
        private int deadHash;
        private int alertHash;
        
        void Start()
        {
            InitializeComponents();
            InitializeAnimationHashes();
            SetupInitialState();
            
            // Find player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        void InitializeComponents()
        {
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            
            navAgent.speed = walkSpeed;
            navAgent.angularSpeed = rotationSpeed * 50f; // Convert to degrees per second
        }
        
        void InitializeAnimationHashes()
        {
            speedHash = Animator.StringToHash("Speed");
            attackHash = Animator.StringToHash("Attack");
            deadHash = Animator.StringToHash("Dead");
            alertHash = Animator.StringToHash("Alert");
        }
        
        void SetupInitialState()
        {
            if (patrolPoints.Length > 0)
            {
                currentState = AIState.Patrol;
                SetDestination(patrolPoints[0].position);
            }
            else
            {
                currentState = AIState.Idle;
            }
        }
        
        void Update()
        {
            if (currentState == AIState.Dead) return;
            
            UpdateDetection();
            UpdateStateMachine();
            UpdateAnimation();
        }
        
        void UpdateDetection()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Check if player is in detection range
            if (distanceToPlayer <= detectionRange)
            {
                // Check if player is in field of view
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angle <= fieldOfView * 0.5f)
                {
                    // Check line of sight
                    if (HasLineOfSight(player.position))
                    {
                        OnPlayerDetected();
                        lastKnownPlayerPosition = player.position;
                        playerDetected = true;
                        alertTimer = 0f;
                    }
                }
            }
            
            // Lose player if too far or no line of sight for too long
            if (playerDetected)
            {
                alertTimer += Time.deltaTime;
                if (distanceToPlayer > detectionRange * 1.5f || alertTimer > 3f)
                {
                    if (!HasLineOfSight(player.position))
                    {
                        OnPlayerLost();
                    }
                }
            }
        }
        
        bool HasLineOfSight(Vector3 targetPosition)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
            Vector3 rayDirection = (targetPosition - rayOrigin).normalized;
            float rayDistance = Vector3.Distance(rayOrigin, targetPosition);
            
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance, obstacleLayerMask))
            {
                return false; // Obstacle in the way
            }
            
            return true; // Clear line of sight
        }
        
        void UpdateStateMachine()
        {
            stateTimer += Time.deltaTime;
            
            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;
                case AIState.Patrol:
                    HandlePatrolState();
                    break;
                case AIState.Chase:
                    HandleChaseState();
                    break;
                case AIState.Attack:
                    HandleAttackState();
                    break;
                case AIState.Search:
                    HandleSearchState();
                    break;
                case AIState.Alert:
                    HandleAlertState();
                    break;
            }
        }
        
        void HandleIdleState()
        {
            navAgent.speed = 0f;
            
            if (patrolPoints.Length > 0 && stateTimer > waitTime)
            {
                ChangeState(AIState.Patrol);
            }
        }
        
        void HandlePatrolState()
        {
            navAgent.speed = walkSpeed;
            
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                // Reached patrol point
                if (randomPatrol)
                {
                    currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                }
                else
                {
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                }
                
                SetDestination(patrolPoints[currentPatrolIndex].position);
                ChangeState(AIState.Idle);
            }
        }
        
        void HandleChaseState()
        {
            navAgent.speed = runSpeed;
            
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(AIState.Attack);
                }
                else if (playerDetected)
                {
                    SetDestination(player.position);
                }
                else
                {
                    // Lost player, start searching
                    ChangeState(AIState.Search);
                }
            }
        }
        
        void HandleAttackState()
        {
            navAgent.speed = 0f;
            
            if (player != null)
            {
                // Face the player
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                
                if (distanceToPlayer > attackRange * 1.2f)
                {
                    ChangeState(AIState.Chase);
                }
                else if (Time.time - lastAttackTime >= 1f / fireRate)
                {
                    Attack();
                }
            }
        }
        
        void HandleSearchState()
        {
            navAgent.speed = walkSpeed;
            
            if (stateTimer > searchTime)
            {
                // Give up searching, return to patrol
                ChangeState(AIState.Patrol);
                return;
            }
            
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                // Pick a random point around last known position
                Vector3 randomPoint = lastKnownPlayerPosition + Random.insideUnitSphere * searchRadius;
                randomPoint.y = transform.position.y;
                
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
                {
                    SetDestination(hit.position);
                }
            }
        }
        
        void HandleAlertState()
        {
            navAgent.speed = 0f;
            
            if (stateTimer > 2f)
            {
                ChangeState(AIState.Search);
            }
        }
        
        void Attack()
        {
            lastAttackTime = Time.time;
            
            // Trigger attack animation
            animator.SetTrigger(attackHash);
            
            // Perform raycast attack
            if (firePoint != null && player != null)
            {
                Vector3 directionToPlayer = (player.position - firePoint.position).normalized;
                
                // Add some inaccuracy
                float inaccuracy = 1f - accuracy;
                directionToPlayer += Random.insideUnitSphere * inaccuracy;
                directionToPlayer.Normalize();
                
                RaycastHit hit;
                if (Physics.Raycast(firePoint.position, directionToPlayer, out hit, attackRange * 2f))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        // Damage player
                        var playerHealth = hit.collider.GetComponent<PlayerHealth>();
                        if (playerHealth != null)
                        {
                            playerHealth.TakeDamage(damage);
                        }
                        
                        // Trigger hit event
                        EventSystem.Instance?.TriggerEvent("PlayerHit", damage);
                    }
                }
                
                // Trigger weapon fired event for effects
                EventSystem.Instance?.TriggerEvent("EnemyWeaponFired", firePoint.position);
            }
        }
        
        void OnPlayerDetected()
        {
            if (currentState != AIState.Chase && currentState != AIState.Attack)
            {
                ChangeState(AIState.Alert);
                EventSystem.Instance?.TriggerEvent("EnemyAlerted", gameObject);
            }
        }
        
        void OnPlayerLost()
        {
            playerDetected = false;
            if (currentState == AIState.Chase || currentState == AIState.Attack)
            {
                ChangeState(AIState.Search);
            }
        }
        
        void ChangeState(AIState newState)
        {
            currentState = newState;
            stateTimer = 0f;
            
            // State-specific initialization
            switch (newState)
            {
                case AIState.Chase:
                    navAgent.speed = runSpeed;
                    break;
                case AIState.Alert:
                    navAgent.speed = 0f;
                    break;
                case AIState.Search:
                    SetDestination(lastKnownPlayerPosition);
                    break;
            }
        }
        
        void SetDestination(Vector3 destination)
        {
            if (navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(destination);
            }
        }
        
        void UpdateAnimation()
        {
            float speed = navAgent.velocity.magnitude;
            animator.SetFloat(speedHash, speed);
            animator.SetBool(alertHash, currentState == AIState.Alert || currentState == AIState.Attack);
        }
        
        public void TakeDamage(float damageAmount)
        {
            health -= damageAmount;
            
            if (health <= 0)
            {
                Die();
            }
            else
            {
                // React to damage
                if (currentState == AIState.Patrol || currentState == AIState.Idle)
                {
                    ChangeState(AIState.Alert);
                }
            }
            
            EventSystem.Instance?.TriggerEvent("EnemyDamaged", gameObject);
        }
        
        void Die()
        {
            currentState = AIState.Dead;
            navAgent.enabled = false;
            animator.SetBool(deadHash, true);
            
            // Disable collider
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            EventSystem.Instance?.TriggerEvent("EnemyDied", gameObject);
            
            // Destroy after delay
            Destroy(gameObject, 5f);
        }
        
        // Debug visualization
        void OnDrawGizmosSelected()
        {
            // Detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Field of view
            Gizmos.color = Color.blue;
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward * detectionRange;
            
            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);
            
            // Patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (Transform point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                    }
                }
            }
        }
    }
}