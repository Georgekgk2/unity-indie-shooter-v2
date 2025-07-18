using UnityEngine;
using IndieShooter.AI.Behaviors;

namespace IndieShooter.AI.States
{
    // Idle State - Enemy is stationary and alert
    public class IdleState : AIState
    {
        private float idleTimer = 0f;
        private float maxIdleTime = 3f;
        
        public IdleState(AIController controller) : base(controller) { }
        
        public override void Enter()
        {
            aiController.StopMovement();
            aiController.SetMovementSpeed(aiController.walkSpeed);
            idleTimer = 0f;
        }
        
        public override void Update()
        {
            idleTimer += Time.deltaTime;
            
            // Look around occasionally
            if (idleTimer > maxIdleTime)
            {
                // Transition to patrol will be handled by state machine
            }
        }
        
        public override void Exit()
        {
            aiController.ResumeMovement();
        }
    }
    
    // Patrol State - Enemy moves between patrol points
    public class PatrolState : AIState
    {
        private PatrolBehavior patrolBehavior;
        
        public PatrolState(AIController controller) : base(controller) 
        {
            patrolBehavior = controller.GetComponent<PatrolBehavior>();
        }
        
        public override void Enter()
        {
            aiController.SetMovementSpeed(aiController.walkSpeed);
            
            if (patrolBehavior != null && patrolBehavior.HasPatrolPoints)
            {
                patrolBehavior.StartPatrol();
                aiController.SetDestination(patrolBehavior.CurrentPatrolTarget);
            }
        }
        
        public override void Update()
        {
            if (patrolBehavior == null || !patrolBehavior.HasPatrolPoints)
            {
                // No patrol points, just idle
                return;
            }
            
            patrolBehavior.UpdatePatrol();
            
            // Check if reached current patrol point
            if (patrolBehavior.HasReachedCurrentPoint(aiController.transform.position))
            {
                patrolBehavior.OnReachedPatrolPoint();
                aiController.SetDestination(patrolBehavior.CurrentPatrolTarget);
            }
        }
        
        public override void Exit()
        {
            // Nothing specific to clean up
        }
    }
    
    // Chase State - Enemy pursues the player
    public class ChaseState : AIState
    {
        private float lastUpdateTime = 0f;
        private float updateInterval = 0.2f; // Update path every 0.2 seconds
        
        public ChaseState(AIController controller) : base(controller) { }
        
        public override void Enter()
        {
            aiController.SetMovementSpeed(aiController.runSpeed);
            lastUpdateTime = 0f;
        }
        
        public override void Update()
        {
            if (aiController.target == null) return;
            
            // Update destination periodically for better performance
            if (Time.time - lastUpdateTime > updateInterval)
            {
                if (aiController.canSeeTarget)
                {
                    aiController.SetDestination(aiController.target.position);
                    aiController.lastKnownPlayerPosition = aiController.target.position;
                }
                else
                {
                    // Move to last known position
                    aiController.SetDestination(aiController.lastKnownPlayerPosition);
                }
                
                lastUpdateTime = Time.time;
            }
            
            // Look at target if visible
            if (aiController.canSeeTarget)
            {
                aiController.LookAt(aiController.target.position);
            }
        }
        
        public override void Exit()
        {
            // Nothing specific to clean up
        }
    }
    
    // Attack State - Enemy attacks the player
    public class AttackState : AIState
    {
        private float lastAttackTime = 0f;
        
        public AttackState(AIController controller) : base(controller) { }
        
        public override void Enter()
        {
            aiController.StopMovement();
            lastAttackTime = 0f;
        }
        
        public override void Update()
        {
            if (aiController.target == null) return;
            
            // Always look at target when attacking
            aiController.LookAt(aiController.target.position);
            
            // Attack if enough time has passed
            if (Time.time - lastAttackTime >= 1f / aiController.fireRate)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
        }
        
        void PerformAttack()
        {
            if (aiController.target == null || !aiController.canSeeTarget) return;
            
            // Trigger attack animation
            var animator = aiController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            // Calculate accuracy
            Vector3 targetPosition = aiController.target.position;
            if (Random.value > aiController.accuracy)
            {
                // Miss - add random offset
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                randomOffset.y = 0; // Keep on horizontal plane
                targetPosition += randomOffset;
            }
            
            // Perform raycast attack
            Vector3 fireOrigin = aiController.firePoint != null ? 
                aiController.firePoint.position : 
                aiController.transform.position + Vector3.up * 1.5f;
                
            Vector3 fireDirection = (targetPosition - fireOrigin).normalized;
            
            RaycastHit hit;
            if (Physics.Raycast(fireOrigin, fireDirection, out hit, aiController.attackRange))
            {
                // Check if hit player
                if (hit.collider.CompareTag("Player"))
                {
                    // Apply damage to player
                    var playerHealth = hit.collider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(aiController.damage);
                    }
                }
                
                // Trigger bullet hit event for effects
                IndieShooter.Core.EventSystem.Instance?.TriggerEvent("BulletHit", hit);
            }
            
            // Trigger weapon fired event for effects
            IndieShooter.Core.EventSystem.Instance?.TriggerEvent("EnemyWeaponFired", aiController);
        }
        
        public override void Exit()
        {
            aiController.ResumeMovement();
        }
    }
    
    // Search State - Enemy searches for player at last known position
    public class SearchState : AIState
    {
        private float searchTimer = 0f;
        private float maxSearchTime = 10f;
        private Vector3 searchCenter;
        private Vector3 currentSearchPoint;
        private bool hasSearchPoint = false;
        private float searchRadius = 5f;
        
        public SearchState(AIController controller) : base(controller) { }
        
        public override void Enter()
        {
            aiController.SetMovementSpeed(aiController.walkSpeed);
            searchTimer = 0f;
            searchCenter = aiController.lastKnownPlayerPosition;
            hasSearchPoint = false;
            
            // Move to last known position first
            aiController.SetDestination(searchCenter);
        }
        
        public override void Update()
        {
            searchTimer += Time.deltaTime;
            
            // Give up searching after max time
            if (searchTimer > maxSearchTime)
            {
                aiController.hasTarget = false;
                return;
            }
            
            // If reached current destination, pick a new search point
            if (aiController.HasReachedDestination() || !hasSearchPoint)
            {
                PickNewSearchPoint();
            }
        }
        
        void PickNewSearchPoint()
        {
            // Pick random point around search center
            Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
            currentSearchPoint = searchCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            aiController.SetDestination(currentSearchPoint);
            hasSearchPoint = true;
        }
        
        public override void Exit()
        {
            // Nothing specific to clean up
        }
    }
    
    // Dead State - Enemy is dead
    public class DeadState : AIState
    {
        public DeadState(AIController controller) : base(controller) { }
        
        public override void Enter()
        {
            aiController.StopMovement();
            
            // Disable collider
            var collider = aiController.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        
        public override void Update()
        {
            // Dead enemies don't do anything
        }
        
        public override void Exit()
        {
            // Dead enemies don't exit this state
        }
    }
}