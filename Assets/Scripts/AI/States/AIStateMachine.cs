using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.AI.States
{
    public abstract class AIState
    {
        protected AIStateMachine stateMachine;
        protected AIController aiController;
        
        public AIState(AIStateMachine stateMachine, AIController aiController)
        {
            this.stateMachine = stateMachine;
            this.aiController = aiController;
        }
        
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
        public virtual void OnPlayerDetected() { }
        public virtual void OnPlayerLost() { }
        public virtual void OnDamageReceived() { }
    }
    
    public class AIStateMachine : MonoBehaviour
    {
        private AIState currentState;
        private Dictionary<System.Type, AIState> states;
        private AIController aiController;
        
        void Awake()
        {
            aiController = GetComponent<AIController>();
            InitializeStates();
        }
        
        void InitializeStates()
        {
            states = new Dictionary<System.Type, AIState>
            {
                { typeof(IdleState), new IdleState(this, aiController) },
                { typeof(PatrolState), new PatrolState(this, aiController) },
                { typeof(ChaseState), new ChaseState(this, aiController) },
                { typeof(AttackState), new AttackState(this, aiController) },
                { typeof(SearchState), new SearchState(this, aiController) },
                { typeof(AlertState), new AlertState(this, aiController) },
                { typeof(DeadState), new DeadState(this, aiController) }
            };
            
            // Start with idle state
            ChangeState<IdleState>();
        }
        
        void Update()
        {
            currentState?.Update();
        }
        
        public void ChangeState<T>() where T : AIState
        {
            var newStateType = typeof(T);
            
            if (states.ContainsKey(newStateType))
            {
                currentState?.Exit();
                currentState = states[newStateType];
                currentState.Enter();
            }
        }
        
        public T GetState<T>() where T : AIState
        {
            var stateType = typeof(T);
            return states.ContainsKey(stateType) ? states[stateType] as T : null;
        }
        
        public void OnPlayerDetected()
        {
            currentState?.OnPlayerDetected();
        }
        
        public void OnPlayerLost()
        {
            currentState?.OnPlayerLost();
        }
        
        public void OnDamageReceived()
        {
            currentState?.OnDamageReceived();
        }
    }
    
    // Individual State Classes
    public class IdleState : AIState
    {
        private float idleTimer;
        
        public IdleState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            idleTimer = 0f;
            aiController.GetComponent<UnityEngine.AI.NavMeshAgent>().speed = 0f;
        }
        
        public override void Update()
        {
            idleTimer += Time.deltaTime;
            
            if (idleTimer >= aiController.waitTime)
            {
                stateMachine.ChangeState<PatrolState>();
            }
        }
        
        public override void OnPlayerDetected()
        {
            stateMachine.ChangeState<AlertState>();
        }
        
        public override void OnDamageReceived()
        {
            stateMachine.ChangeState<AlertState>();
        }
    }
    
    public class PatrolState : AIState
    {
        public PatrolState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = aiController.walkSpeed;
            
            // Set destination to next patrol point
            if (aiController.patrolPoints.Length > 0)
            {
                navAgent.SetDestination(aiController.patrolPoints[0].position);
            }
        }
        
        public override void Update()
        {
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                stateMachine.ChangeState<IdleState>();
            }
        }
        
        public override void OnPlayerDetected()
        {
            stateMachine.ChangeState<AlertState>();
        }
        
        public override void OnDamageReceived()
        {
            stateMachine.ChangeState<AlertState>();
        }
    }
    
    public class ChaseState : AIState
    {
        private Transform player;
        
        public ChaseState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = aiController.runSpeed;
            
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        public override void Update()
        {
            if (player == null) return;
            
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            float distanceToPlayer = Vector3.Distance(aiController.transform.position, player.position);
            
            if (distanceToPlayer <= aiController.attackRange)
            {
                stateMachine.ChangeState<AttackState>();
            }
            else
            {
                navAgent.SetDestination(player.position);
            }
        }
        
        public override void OnPlayerLost()
        {
            stateMachine.ChangeState<SearchState>();
        }
    }
    
    public class AttackState : AIState
    {
        private Transform player;
        private float lastAttackTime;
        
        public AttackState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = 0f;
            
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        public override void Update()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(aiController.transform.position, player.position);
            
            if (distanceToPlayer > aiController.attackRange * 1.2f)
            {
                stateMachine.ChangeState<ChaseState>();
                return;
            }
            
            // Face the player
            Vector3 directionToPlayer = (player.position - aiController.transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            aiController.transform.rotation = Quaternion.Slerp(aiController.transform.rotation, lookRotation, Time.deltaTime * aiController.rotationSpeed);
            
            // Attack
            if (Time.time - lastAttackTime >= 1f / aiController.fireRate)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        
        void Attack()
        {
            // Attack logic here
            var animator = aiController.GetComponent<Animator>();
            animator.SetTrigger("Attack");
        }
        
        public override void OnPlayerLost()
        {
            stateMachine.ChangeState<SearchState>();
        }
    }
    
    public class SearchState : AIState
    {
        private float searchTimer;
        private Vector3 lastKnownPosition;
        
        public SearchState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            searchTimer = 0f;
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = aiController.walkSpeed;
        }
        
        public override void Update()
        {
            searchTimer += Time.deltaTime;
            
            if (searchTimer >= aiController.searchTime)
            {
                stateMachine.ChangeState<PatrolState>();
            }
        }
        
        public override void OnPlayerDetected()
        {
            stateMachine.ChangeState<ChaseState>();
        }
    }
    
    public class AlertState : AIState
    {
        private float alertTimer;
        
        public AlertState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            alertTimer = 0f;
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.speed = 0f;
        }
        
        public override void Update()
        {
            alertTimer += Time.deltaTime;
            
            if (alertTimer >= 2f)
            {
                stateMachine.ChangeState<SearchState>();
            }
        }
        
        public override void OnPlayerDetected()
        {
            stateMachine.ChangeState<ChaseState>();
        }
    }
    
    public class DeadState : AIState
    {
        public DeadState(AIStateMachine stateMachine, AIController aiController) : base(stateMachine, aiController) { }
        
        public override void Enter()
        {
            var navAgent = aiController.GetComponent<UnityEngine.AI.NavMeshAgent>();
            navAgent.enabled = false;
            
            var animator = aiController.GetComponent<Animator>();
            animator.SetBool("Dead", true);
            
            var collider = aiController.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }
}