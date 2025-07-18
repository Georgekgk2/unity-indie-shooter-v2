using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Система ворогів з AI, різними типами та поведінкою.
/// Включає базовий AI, State Machine для ворогів та систему спавну.
/// </summary>

// ================================
// БАЗОВИЙ ВОРОГ
// ================================

public class Enemy : MonoBehaviour, IEventHandler<PlayerDeathEvent>
{
    [Header("Enemy Info")]
    [Tooltip("Тип ворога")]
    public EnemyType enemyType = EnemyType.Soldier;
    [Tooltip("Назва ворога")]
    public string enemyName = "Enemy";
    [Tooltip("Рівень ворога")]
    [Range(1, 20)]
    public int enemyLevel = 1;

    [Header("Health Settings")]
    [Tooltip("Максимальне здоров'я")]
    public float maxHealth = 100f;
    [Tooltip("Поточне здоров'я")]
    public float currentHealth = 100f;
    [Tooltip("Броня")]
    [Range(0f, 0.9f)]
    public float armor = 0f;

    [Header("Combat Settings")]
    [Tooltip("Урон атаки")]
    public float attackDamage = 25f;
    [Tooltip("Швидкість атаки")]
    public float attackRate = 1f;
    [Tooltip("Дальність атаки")]
    public float attackRange = 10f;
    [Tooltip("Дальність виявлення гравця")]
    public float detectionRange = 15f;
    [Tooltip("Кут огляду")]
    [Range(30f, 360f)]
    public float viewAngle = 90f;

    [Header("Movement Settings")]
    [Tooltip("Швидкість руху")]
    public float moveSpeed = 3.5f;
    [Tooltip("Швидкість бігу")]
    public float runSpeed = 6f;
    [Tooltip("Швидкість обертання")]
    public float rotationSpeed = 120f;

    [Header("AI Settings")]
    [Tooltip("Час патрулювання")]
    public float patrolWaitTime = 3f;
    [Tooltip("Точки патрулювання")]
    public Transform[] patrolPoints;
    [Tooltip("Час пошуку гравця")]
    public float searchTime = 5f;

    [Header("Audio")]
    [Tooltip("Звуки ворога")]
    public EnemyAudioCollection audioCollection;

    // Компоненти
    protected NavMeshAgent navAgent;
    protected Animator animator;
    protected Rigidbody rb;
    protected Collider enemyCollider;

    // AI стан
    protected EnemyStateMachine stateMachine;
    protected Transform player;
    protected float lastAttackTime;
    protected float lastSeenPlayerTime;
    protected Vector3 lastKnownPlayerPosition;
    protected bool isDead = false;

    // Патрулювання
    protected int currentPatrolIndex = 0;
    protected float patrolWaitTimer = 0f;

    public enum EnemyType
    {
        Soldier,
        Heavy,
        Scout,
        Sniper,
        Medic,
        Boss
    }

    void Awake()
    {
        InitializeComponents();
        InitializeStateMachine();
    }

    void Start()
    {
        Events.Subscribe<PlayerDeathEvent>(this);
        
        player = FindObjectOfType<PlayerMovement>()?.transform;
        currentHealth = maxHealth;
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }
    }

    void InitializeComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();

        if (navAgent == null)
        {
            Debug.LogWarning($"Enemy {name}: NavMeshAgent не знайдено!");
        }
    }

    void InitializeStateMachine()
    {
        stateMachine = new EnemyStateMachine(this);
        
        // Додаємо стани
        stateMachine.AddState(new EnemyIdleState(stateMachine, this));
        stateMachine.AddState(new EnemyPatrolState(stateMachine, this));
        stateMachine.AddState(new EnemyChaseState(stateMachine, this));
        stateMachine.AddState(new EnemyAttackState(stateMachine, this));
        stateMachine.AddState(new EnemySearchState(stateMachine, this));
        stateMachine.AddState(new EnemyDeadState(stateMachine, this));
        
        // Починаємо з патрулювання або бездіяльності
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            stateMachine.Start<EnemyPatrolState>();
        }
        else
        {
            stateMachine.Start<EnemyIdleState>();
        }
    }

    void Update()
    {
        if (isDead) return;

        stateMachine?.Update();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        stateMachine?.FixedUpdate();
    }

    /// <summary>
    /// Наносить урон ворогу
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead) return;

        // Застосовуємо броню
        float actualDamage = damage * (1f - armor);
        currentHealth -= actualDamage;

        Debug.Log($"{enemyName} отримав {actualDamage} урону. Здоров'я: {currentHealth}");

        // Ефекти удару
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayBloodEffect(hitPoint, hitDirection);
        }

        // Звук удару
        if (audioCollection.hitSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.hitSound, transform.position);
        }

        // Реакція на урон
        if (currentHealth > 0)
        {
            OnDamageReceived(hitPoint, hitDirection);
        }
        else
        {
            Die();
        }
    }

    /// <summary>
    /// Реакція на отримання урону
    /// </summary>
    protected virtual void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        // Переходимо в стан атаки, якщо не в ньому
        if (!stateMachine.IsInState<EnemyAttackState>() && !stateMachine.IsInState<EnemyChaseState>())
        {
            if (player != null)
            {
                lastKnownPlayerPosition = player.position;
                lastSeenPlayerTime = Time.time;
                stateMachine.ChangeState<EnemyChaseState>();
            }
        }
    }

    /// <summary>
    /// Смерть ворога
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0f;

        Debug.Log($"{enemyName} загинув!");

        // Переходимо в стан смерті
        stateMachine.ChangeState<EnemyDeadState>();

        // Перевіряємо, чи закінчився бій (Claude покращення)
        CheckCombatStatus();

        // Звук смерті
        if (audioCollection.deathSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.deathSound, transform.position);
        }

        // Ефект смерті
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayDeathEffect(transform.position);
        }

        // Відключаємо компоненти
        if (navAgent != null) navAgent.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;

        // Знищуємо через деякий час
        StartCoroutine(DestroyAfterDelay(5f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    /// <summary>
    /// Перевіряє стан бою після смерті ворога (Claude покращення)
    /// </summary>
    void CheckCombatStatus()
    {
        // Рахуємо живих ворогів поблизу
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        int aliveEnemiesCount = 0;
        
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != this && !enemy.isDead)
            {
                // Перевіряємо відстань до гравця
                if (player != null)
                {
                    float distanceToPlayer = Vector3.Distance(enemy.transform.position, player.position);
                    if (distanceToPlayer <= 30f) // Радіус бою
                    {
                        aliveEnemiesCount++;
                    }
                }
            }
        }
        
        // Якщо ворогів не залишилося, бій закінчився
        if (aliveEnemiesCount == 0)
        {
            Events.Publish(new CombatEndedEvent(true, Time.time, 1));
            Debug.Log("Enemy: Бій закінчено - всі вороги знищені");
        }
    }

    /// <summary>
    /// Додає метод для перевірки наявності броні (для damage numbers)
    /// </summary>
    public bool HasArmor()
    {
        return armor > 0f;
    }

    /// <summary>
    /// Перевіряє, чи може ворог бачити гравця
    /// </summary>
    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Перевіряємо дистанцію
        if (distanceToPlayer > detectionRange) return false;

        // Перевіряємо кут огляду
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2f) return false;

        // Перевіряємо перешкоди
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.transform != player)
            {
                return false; // Є перешкода
            }
        }

        return true;
    }

    /// <summary>
    /// Атакує гравця
    /// </summary>
    public virtual void AttackPlayer()
    {
        if (player == null || Time.time - lastAttackTime < 1f / attackRate) return;

        lastAttackTime = Time.time;

        // Звук атаки
        if (audioCollection.attackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.attackSound, transform.position);
        }

        // Наносимо урон гравцю
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        Debug.Log($"{enemyName} атакував гравця на {attackDamage} урону!");
    }

    /// <summary>
    /// Оновлює аніматор
    /// </summary>
    void UpdateAnimator()
    {
        if (animator == null) return;

        // Швидкість руху
        float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        animator.SetFloat("Speed", speed);

        // Стан
        animator.SetBool("IsChasing", stateMachine.IsInState<EnemyChaseState>());
        animator.SetBool("IsAttacking", stateMachine.IsInState<EnemyAttackState>());
        animator.SetBool("IsDead", isDead);
    }

    public void HandleEvent(PlayerDeathEvent eventData)
    {
        // Коли гравець помирає, переходимо в режим патрулювання
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            stateMachine.ChangeState<EnemyPatrolState>();
        }
        else
        {
            stateMachine.ChangeState<EnemyIdleState>();
        }
    }

    void OnDestroy()
    {
        Events.Unsubscribe<PlayerDeathEvent>(this);
    }

    // Публічні властивості для AI станів
    public NavMeshAgent NavAgent => navAgent;
    public Transform Player => player;
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
    public float LastSeenPlayerTime => lastSeenPlayerTime;
    public Transform[] PatrolPoints => patrolPoints;
    public int CurrentPatrolIndex { get => currentPatrolIndex; set => currentPatrolIndex = value; }
    public float PatrolWaitTimer { get => patrolWaitTimer; set => patrolWaitTimer = value; }
    public bool IsDead => isDead;

    public void SetLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        lastSeenPlayerTime = Time.time;
    }
}

// ================================
// STATE MACHINE ДЛЯ ВОРОГІВ
// ================================

public class EnemyStateMachine : StateMachine
{
    private Enemy enemy;

    public EnemyStateMachine(Enemy enemy) : base(enemy)
    {
        this.enemy = enemy;
    }

    public Enemy Enemy => enemy;
}

// ================================
// СТАНИ ВОРОГІВ
// ================================

public abstract class EnemyState : State
{
    protected Enemy enemy;
    protected EnemyStateMachine enemyStateMachine;

    public EnemyState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy)
    {
        this.enemy = enemy;
        this.enemyStateMachine = stateMachine;
    }
}

public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = true;
        }
    }

    public override void Update()
    {
        // Перевіряємо, чи бачимо гравця
        if (enemy.CanSeePlayer())
        {
            enemy.SetLastKnownPlayerPosition(enemy.Player.position);
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        // Якщо є точки патрулювання, переходимо до патрулювання
        if (enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
        {
            stateMachine.ChangeState<EnemyPatrolState>();
        }
    }
}

public class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = false;
            enemy.NavAgent.speed = enemy.moveSpeed;
        }
        MoveToNextPatrolPoint();
    }

    public override void Update()
    {
        // Перевіряємо, чи бачимо гравця
        if (enemy.CanSeePlayer())
        {
            enemy.SetLastKnownPlayerPosition(enemy.Player.position);
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        // Перевіряємо, чи дійшли до точки патрулювання
        if (enemy.NavAgent != null && !enemy.NavAgent.pathPending && enemy.NavAgent.remainingDistance < 0.5f)
        {
            enemy.PatrolWaitTimer += Time.deltaTime;
            
            if (enemy.PatrolWaitTimer >= enemy.patrolWaitTime)
            {
                enemy.PatrolWaitTimer = 0f;
                MoveToNextPatrolPoint();
            }
        }
    }

    void MoveToNextPatrolPoint()
    {
        if (enemy.PatrolPoints == null || enemy.PatrolPoints.Length == 0) return;

        enemy.CurrentPatrolIndex = (enemy.CurrentPatrolIndex + 1) % enemy.PatrolPoints.Length;
        
        if (enemy.NavAgent != null && enemy.PatrolPoints[enemy.CurrentPatrolIndex] != null)
        {
            enemy.NavAgent.SetDestination(enemy.PatrolPoints[enemy.CurrentPatrolIndex].position);
        }
    }
}

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = false;
            enemy.NavAgent.speed = enemy.runSpeed;
        }
    }

    public override void Update()
    {
        if (enemy.Player == null) return;

        // Оновлюємо позицію гравця, якщо бачимо його
        if (enemy.CanSeePlayer())
        {
            enemy.SetLastKnownPlayerPosition(enemy.Player.position);
            
            // Перевіряємо дистанцію для атаки
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);
            if (distanceToPlayer <= enemy.attackRange)
            {
                stateMachine.ChangeState<EnemyAttackState>();
                return;
            }
        }
        else
        {
            // Якщо не бачимо гравця довго, переходимо до пошуку
            if (Time.time - enemy.LastSeenPlayerTime > 2f)
            {
                stateMachine.ChangeState<EnemySearchState>();
                return;
            }
        }

        // Рухаємося до останньої відомої позиції гравця
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.SetDestination(enemy.LastKnownPlayerPosition);
        }
    }
}

public class EnemyAttackState : EnemyState
{
    private float attackTimer = 0f;

    public EnemyAttackState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = true;
        }
        attackTimer = 0f;
    }

    public override void Update()
    {
        if (enemy.Player == null) return;

        // Повертаємося до гравця
        Vector3 directionToPlayer = (enemy.Player.position - enemy.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, enemy.rotationSpeed * Time.deltaTime);

        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.position);

        // Якщо гравець далеко, переслідуємо
        if (distanceToPlayer > enemy.attackRange)
        {
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        // Якщо не бачимо гравця, шукаємо
        if (!enemy.CanSeePlayer())
        {
            stateMachine.ChangeState<EnemySearchState>();
            return;
        }

        // Атакуємо
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / enemy.attackRate)
        {
            enemy.AttackPlayer();
            attackTimer = 0f;
        }
    }
}

public class EnemySearchState : EnemyState
{
    private float searchTimer = 0f;

    public EnemySearchState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        searchTimer = 0f;
        
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = false;
            enemy.NavAgent.speed = enemy.moveSpeed;
            enemy.NavAgent.SetDestination(enemy.LastKnownPlayerPosition);
        }
    }

    public override void Update()
    {
        // Перевіряємо, чи знайшли гравця
        if (enemy.CanSeePlayer())
        {
            enemy.SetLastKnownPlayerPosition(enemy.Player.position);
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        searchTimer += Time.deltaTime;

        // Якщо шукали довго, повертаємося до патрулювання
        if (searchTimer >= enemy.searchTime)
        {
            if (enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
            {
                stateMachine.ChangeState<EnemyPatrolState>();
            }
            else
            {
                stateMachine.ChangeState<EnemyIdleState>();
            }
        }
    }
}

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(EnemyStateMachine stateMachine, Enemy enemy) : base(stateMachine, enemy) { }

    public override void Enter()
    {
        if (enemy.NavAgent != null)
        {
            enemy.NavAgent.isStopped = true;
        }
    }

    public override void Update()
    {
        // Мертвий ворог нічого не робить
    }
}

// ================================
// АУДІО КОЛЕКЦІЯ ДЛЯ ВОРОГІВ
// ================================

[System.Serializable]
public class EnemyAudioCollection
{
    [Header("Enemy Sounds")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip alertSound;
    public AudioClip[] footstepSounds;
    public AudioClip[] voiceSounds;
}