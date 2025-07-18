using UnityEngine;

/// <summary>
/// Колекція всіх станів руху гравця. Кожен стан відповідає за конкретну поведінку.
/// </summary>

// ================================
// БАЗОВИЙ СТАН РУХУ
// ================================

/// <summary>
/// Базовий клас для всіх станів руху гравця. Містить загальну логіку та посилання на компоненти.
/// </summary>
public abstract class PlayerMovementState : State
{
    protected PlayerMovement playerMovement;
    protected Rigidbody rb;
    protected Camera playerCamera;

    public PlayerMovementState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement)
    {
        this.playerMovement = playerMovement;
        this.rb = playerMovement.GetComponent<Rigidbody>();
        this.playerCamera = Camera.main;
    }

    /// <summary>
    /// Отримує напрямок руху на основі вводу
    /// </summary>
    protected Vector3 GetMovementDirection()
    {
        if (playerCamera == null) return Vector3.zero;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        return (camForward * verticalInput + camRight * horizontalInput).normalized;
    }

    /// <summary>
    /// Перевіряє, чи гравець на землі
    /// </summary>
    protected bool IsGrounded()
    {
        return playerMovement.IsGrounded; // Припускаємо, що це публічна властивість
    }

    /// <summary>
    /// Застосовує рух до Rigidbody
    /// </summary>
    protected void ApplyMovement(Vector3 direction, float speed, float acceleration)
    {
        Vector3 targetVelocity = direction * speed;
        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
        Vector3 velocityChange = Vector3.MoveTowards(currentVelocity, targetVelocity, 
            acceleration * Time.fixedDeltaTime) - currentVelocity;
        
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}

// ================================
// СТАН БЕЗДІЯЛЬНОСТІ
// ================================

[StateTransition(typeof(IdleState), typeof(WalkingState), "Input detected")]
[StateTransition(typeof(IdleState), typeof(JumpingState), "Jump input")]
[StateTransition(typeof(IdleState), typeof(CrouchingState), "Crouch input")]
public class IdleState : PlayerMovementState
{
    public IdleState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Idle State");
        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Walking, // Previous
            PlayerMovementStateChangedEvent.MovementState.Idle,    // Current
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        // Перевіряємо переходи до інших станів
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        // Застосовуємо гальмування
        ApplyMovement(Vector3.zero, 0f, playerMovement.groundDeceleration);
    }

    private void CheckTransitions()
    {
        Vector3 moveDirection = GetMovementDirection();
        
        // Перехід до ходьби
        if (moveDirection.magnitude > 0.1f)
        {
            stateMachine.ChangeState<WalkingState>();
            return;
        }

        // Перехід до стрибка
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            stateMachine.ChangeState<JumpingState>();
            return;
        }

        // Перехід до присідання
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            stateMachine.ChangeState<CrouchingState>();
            return;
        }
    }
}

// ================================
// СТАН ХОДЬБИ
// ================================

[StateTransition(typeof(WalkingState), typeof(IdleState), "No input")]
[StateTransition(typeof(WalkingState), typeof(RunningState), "Sprint input")]
[StateTransition(typeof(WalkingState), typeof(JumpingState), "Jump input")]
[StateTransition(typeof(WalkingState), typeof(CrouchingState), "Crouch input")]
public class WalkingState : PlayerMovementState
{
    public WalkingState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Walking State");
        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Idle,
            PlayerMovementStateChangedEvent.MovementState.Walking,
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = GetMovementDirection();
        ApplyMovement(moveDirection, playerMovement.walkSpeed, playerMovement.groundAcceleration);
    }

    private void CheckTransitions()
    {
        Vector3 moveDirection = GetMovementDirection();
        
        // Перехід до бездіяльності
        if (moveDirection.magnitude < 0.1f)
        {
            stateMachine.ChangeState<IdleState>();
            return;
        }

        // Перехід до бігу
        if (Input.GetKey(KeyCode.LeftShift) && playerMovement.CanSprint())
        {
            stateMachine.ChangeState<RunningState>();
            return;
        }

        // Перехід до стрибка
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            stateMachine.ChangeState<JumpingState>();
            return;
        }

        // Перехід до присідання
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            stateMachine.ChangeState<CrouchingState>();
            return;
        }
    }
}

// ================================
// СТАН БІГУ
// ================================

[StateTransition(typeof(RunningState), typeof(WalkingState), "Sprint released or stamina depleted")]
[StateTransition(typeof(RunningState), typeof(SlidingState), "Slide input")]
[StateTransition(typeof(RunningState), typeof(JumpingState), "Jump input")]
public class RunningState : PlayerMovementState
{
    public RunningState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Running State");
        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Walking,
            PlayerMovementStateChangedEvent.MovementState.Running,
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        // Витрачаємо стамину
        playerMovement.DrainStamina();
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = GetMovementDirection();
        ApplyMovement(moveDirection, playerMovement.sprintSpeed, playerMovement.groundAcceleration);
    }

    private void CheckTransitions()
    {
        Vector3 moveDirection = GetMovementDirection();
        
        // Перехід до ходьби (відпустили спринт або закінчилась стаміна)
        if (!Input.GetKey(KeyCode.LeftShift) || !playerMovement.CanSprint())
        {
            stateMachine.ChangeState<WalkingState>();
            return;
        }

        // Перехід до ковзання
        if (Input.GetKeyDown(KeyCode.LeftControl) && playerMovement.allowSlide)
        {
            stateMachine.ChangeState<SlidingState>();
            return;
        }

        // Перехід до стрибка
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            stateMachine.ChangeState<JumpingState>();
            return;
        }

        // Якщо немає вводу, переходимо до ходьби
        if (moveDirection.magnitude < 0.1f)
        {
            stateMachine.ChangeState<WalkingState>();
            return;
        }
    }
}

// ================================
// СТАН СТРИБКА
// ================================

[StateTransition(typeof(JumpingState), typeof(FallingState), "Velocity becomes negative")]
[StateTransition(typeof(JumpingState), typeof(IdleState), "Landed")]
public class JumpingState : PlayerMovementState
{
    private bool hasJumped = false;

    public JumpingState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Jumping State");
        
        // Застосовуємо силу стрибка
        rb.AddForce(Vector3.up * playerMovement.jumpForce, ForceMode.Impulse);
        hasJumped = true;

        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Walking,
            PlayerMovementStateChangedEvent.MovementState.Jumping,
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        // Повітряний контроль
        Vector3 moveDirection = GetMovementDirection();
        ApplyMovement(moveDirection, playerMovement.walkSpeed * playerMovement.airControl, 
            playerMovement.airAcceleration);

        // Застосовуємо додаткову гравітацію для кращого відчуття стрибка
        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * playerMovement.fallMultiplier, ForceMode.Acceleration);
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.down * playerMovement.lowJumpMultiplier, ForceMode.Acceleration);
        }
    }

    private void CheckTransitions()
    {
        // Перехід до падіння
        if (rb.velocity.y < 0)
        {
            stateMachine.ChangeState<FallingState>();
            return;
        }
    }

    public override bool CanTransitionTo(System.Type stateType)
    {
        // Можна перейти тільки до падіння
        return stateType == typeof(FallingState);
    }
}

// ================================
// СТАН ПАДІННЯ
// ================================

[StateTransition(typeof(FallingState), typeof(IdleState), "Landed with no input")]
[StateTransition(typeof(FallingState), typeof(WalkingState), "Landed with input")]
public class FallingState : PlayerMovementState
{
    public FallingState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Falling State");
        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Jumping,
            PlayerMovementStateChangedEvent.MovementState.Falling,
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        // Повітряний контроль
        Vector3 moveDirection = GetMovementDirection();
        ApplyMovement(moveDirection, playerMovement.walkSpeed * playerMovement.airControl, 
            playerMovement.airAcceleration);

        // Додаткова гравітація для швидшого падіння
        rb.AddForce(Vector3.down * playerMovement.fallMultiplier, ForceMode.Acceleration);
    }

    private void CheckTransitions()
    {
        // Перехід при приземленні
        if (IsGrounded())
        {
            Vector3 moveDirection = GetMovementDirection();
            
            if (moveDirection.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();
            }
            else
            {
                stateMachine.ChangeState<IdleState>();
            }
            return;
        }
    }
}

// ================================
// СТАН ПРИСІДАННЯ
// ================================

[StateTransition(typeof(CrouchingState), typeof(IdleState), "Crouch released")]
[StateTransition(typeof(CrouchingState), typeof(WalkingState), "Crouch released with input")]
public class CrouchingState : PlayerMovementState
{
    public CrouchingState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Crouching State");
        
        // Змінюємо висоту колайдера
        playerMovement.SetCrouchState(true);

        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Walking,
            PlayerMovementStateChangedEvent.MovementState.Crouching,
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        Vector3 moveDirection = GetMovementDirection();
        ApplyMovement(moveDirection, playerMovement.crouchSpeed, playerMovement.groundAcceleration);
    }

    public override void Exit()
    {
        // Відновлюємо висоту колайдера
        playerMovement.SetCrouchState(false);
    }

    private void CheckTransitions()
    {
        // Перехід при відпусканні присідання
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            Vector3 moveDirection = GetMovementDirection();
            
            if (moveDirection.magnitude > 0.1f)
            {
                stateMachine.ChangeState<WalkingState>();
            }
            else
            {
                stateMachine.ChangeState<IdleState>();
            }
            return;
        }
    }
}

// ================================
// СТАН КОВЗАННЯ
// ================================

[StateTransition(typeof(SlidingState), typeof(WalkingState), "Slide finished")]
[StateTransition(typeof(SlidingState), typeof(CrouchingState), "Slide finished with crouch held")]
public class SlidingState : PlayerMovementState
{
    private float slideStartTime;
    private Vector3 slideDirection;

    public SlidingState(StateMachine stateMachine, PlayerMovement playerMovement) 
        : base(stateMachine, playerMovement) { }

    public override void Enter()
    {
        Debug.Log("PlayerMovement: Entered Sliding State");
        
        slideStartTime = Time.time;
        slideDirection = GetMovementDirection();
        if (slideDirection.magnitude < 0.1f)
        {
            slideDirection = playerMovement.transform.forward;
        }

        // Застосовуємо силу ковзання
        rb.AddForce(slideDirection * playerMovement.slideForce, ForceMode.Impulse);
        
        // Змінюємо висоту колайдера
        playerMovement.SetSlideState(true);

        Events.Trigger(new PlayerMovementStateChangedEvent(
            PlayerMovementStateChangedEvent.MovementState.Running,
            PlayerMovementStateChangedEvent.MovementState.Crouching, // Ковзання як різновид присідання
            playerMovement.transform.position,
            rb.velocity
        ));
    }

    public override void Update()
    {
        CheckTransitions();
    }

    public override void FixedUpdate()
    {
        // Поступово зменшуємо швидкість ковзання
        float slideProgress = (Time.time - slideStartTime) / playerMovement.slideDuration;
        if (slideProgress < 1f)
        {
            float currentSlideForce = Mathf.Lerp(playerMovement.slideForce, 0f, slideProgress);
            ApplyMovement(slideDirection, currentSlideForce, playerMovement.groundDeceleration * 0.5f);
        }
    }

    public override void Exit()
    {
        // Відновлюємо висоту колайдера
        playerMovement.SetSlideState(false);
    }

    private void CheckTransitions()
    {
        // Перехід після закінчення ковзання
        if (Time.time - slideStartTime >= playerMovement.slideDuration)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                stateMachine.ChangeState<CrouchingState>();
            }
            else
            {
                stateMachine.ChangeState<WalkingState>();
            }
            return;
        }
    }

    public override bool CanTransitionTo(System.Type stateType)
    {
        // Під час ковзання можна перейти тільки до присідання або ходьби
        return stateType == typeof(CrouchingState) || stateType == typeof(WalkingState);
    }
}