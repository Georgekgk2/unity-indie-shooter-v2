using UnityEngine;

/// <summary>
/// Колекція команд для управління гравцем. Кожна команда інкапсулює конкретну дію.
/// </summary>

// ================================
// БАЗОВІ КОМАНДИ РУХУ
// ================================

/// <summary>
/// Базовий клас для команд гравця з посиланням на PlayerMovement
/// </summary>
public abstract class PlayerCommand : BaseCommand
{
    protected PlayerMovement playerMovement;
    protected StateMachine stateMachine;

    public PlayerCommand(PlayerMovement playerMovement)
    {
        this.playerMovement = playerMovement;
        this.stateMachine = playerMovement.StateMachine;
    }
}

// ================================
// КОМАНДИ ЗМІНИ СТАНУ
// ================================

[RegisterCommand("StartWalking")]
public class StartWalkingCommand : PlayerCommand
{
    public override string CommandName => "Start Walking";

    public StartWalkingCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null && !stateMachine.IsInState<WalkingState>())
        {
            stateMachine.ChangeState<WalkingState>();
        }
    }
}

[RegisterCommand("StartRunning")]
public class StartRunningCommand : PlayerCommand
{
    public override string CommandName => "Start Running";

    public StartRunningCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null && playerMovement.CanSprint())
        {
            stateMachine.ChangeState<RunningState>();
        }
    }
}

[RegisterCommand("Jump")]
public class JumpCommand : PlayerCommand
{
    public override string CommandName => "Jump";

    public JumpCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null && playerMovement.IsGrounded)
        {
            stateMachine.ChangeState<JumpingState>();
            
            // Відправляємо подію стрибка
            Events.Trigger(new PlayerMovementStateChangedEvent(
                PlayerMovementStateChangedEvent.MovementState.Walking,
                PlayerMovementStateChangedEvent.MovementState.Jumping,
                playerMovement.transform.position,
                playerMovement.GetComponent<Rigidbody>().velocity
            ));
        }
    }
}

[RegisterCommand("StartCrouching")]
public class StartCrouchingCommand : PlayerCommand
{
    public override string CommandName => "Start Crouching";
    public override bool CanUndo => true;

    public StartCrouchingCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null)
        {
            stateMachine.ChangeState<CrouchingState>();
        }
    }

    public override void Undo()
    {
        if (stateMachine != null)
        {
            stateMachine.ChangeState<IdleState>();
        }
        base.Undo();
    }
}

[RegisterCommand("StartSliding")]
public class StartSlidingCommand : PlayerCommand
{
    public override string CommandName => "Start Sliding";

    public StartSlidingCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null && stateMachine.IsInState<RunningState>() && playerMovement.allowSlide)
        {
            stateMachine.ChangeState<SlidingState>();
        }
    }
}

[RegisterCommand("StopMovement")]
public class StopMovementCommand : PlayerCommand
{
    public override string CommandName => "Stop Movement";

    public StopMovementCommand(PlayerMovement playerMovement) : base(playerMovement) { }

    public override void Execute()
    {
        base.Execute();
        
        if (stateMachine != null)
        {
            stateMachine.ChangeState<IdleState>();
        }
    }
}

// ================================
// КОМАНДИ ЗБРОЇ
// ================================

/// <summary>
/// Базовий клас для команд зброї
/// </summary>
public abstract class WeaponCommand : BaseCommand
{
    protected WeaponController weaponController;

    public WeaponCommand(WeaponController weaponController)
    {
        this.weaponController = weaponController;
    }
}

[RegisterCommand("Fire")]
public class FireWeaponCommand : WeaponCommand
{
    public override string CommandName => "Fire Weapon";

    public FireWeaponCommand(WeaponController weaponController) : base(weaponController) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponController != null && weaponController.CanFire())
        {
            weaponController.Fire();
        }
    }
}

[RegisterCommand("Reload")]
public class ReloadWeaponCommand : WeaponCommand
{
    public override string CommandName => "Reload Weapon";

    public ReloadWeaponCommand(WeaponController weaponController) : base(weaponController) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponController != null && weaponController.CanReload())
        {
            weaponController.StartReload();
        }
    }
}

[RegisterCommand("StartAiming")]
public class StartAimingCommand : WeaponCommand
{
    public override string CommandName => "Start Aiming";
    public override bool CanUndo => true;

    public StartAimingCommand(WeaponController weaponController) : base(weaponController) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponController != null)
        {
            weaponController.StartAiming();
        }
    }

    public override void Undo()
    {
        if (weaponController != null)
        {
            weaponController.StopAiming();
        }
        base.Undo();
    }
}

[RegisterCommand("StopAiming")]
public class StopAimingCommand : WeaponCommand
{
    public override string CommandName => "Stop Aiming";

    public StopAimingCommand(WeaponController weaponController) : base(weaponController) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponController != null)
        {
            weaponController.StopAiming();
        }
    }
}

// ================================
// КОМАНДИ ПЕРЕМИКАННЯ ЗБРОЇ
// ================================

/// <summary>
/// Базовий клас для команд перемикання зброї
/// </summary>
public abstract class WeaponSwitchCommand : BaseCommand
{
    protected WeaponSwitching weaponSwitching;

    public WeaponSwitchCommand(WeaponSwitching weaponSwitching)
    {
        this.weaponSwitching = weaponSwitching;
    }
}

[RegisterCommand("SwitchToWeapon")]
public class SwitchToWeaponCommand : ParameterizedCommand<int>
{
    private WeaponSwitching weaponSwitching;
    private int previousWeaponIndex;

    public override string CommandName => $"Switch to Weapon {parameter}";
    public override bool CanUndo => true;

    public SwitchToWeaponCommand(WeaponSwitching weaponSwitching, int weaponIndex) 
        : base(weaponIndex)
    {
        this.weaponSwitching = weaponSwitching;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponSwitching != null)
        {
            previousWeaponIndex = weaponSwitching.GetCurrentWeaponIndex();
            weaponSwitching.SwitchToWeapon(parameter);
        }
    }

    public override void Undo()
    {
        if (weaponSwitching != null)
        {
            weaponSwitching.SwitchToWeapon(previousWeaponIndex);
        }
        base.Undo();
    }
}

[RegisterCommand("NextWeapon")]
public class NextWeaponCommand : WeaponSwitchCommand
{
    public override string CommandName => "Next Weapon";
    public override bool CanUndo => true;
    
    private int previousWeaponIndex;

    public NextWeaponCommand(WeaponSwitching weaponSwitching) : base(weaponSwitching) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponSwitching != null)
        {
            previousWeaponIndex = weaponSwitching.GetCurrentWeaponIndex();
            weaponSwitching.SwitchToNextWeapon();
        }
    }

    public override void Undo()
    {
        if (weaponSwitching != null)
        {
            weaponSwitching.SwitchToWeapon(previousWeaponIndex);
        }
        base.Undo();
    }
}

[RegisterCommand("PreviousWeapon")]
public class PreviousWeaponCommand : WeaponSwitchCommand
{
    public override string CommandName => "Previous Weapon";
    public override bool CanUndo => true;
    
    private int previousWeaponIndex;

    public PreviousWeaponCommand(WeaponSwitching weaponSwitching) : base(weaponSwitching) { }

    public override void Execute()
    {
        base.Execute();
        
        if (weaponSwitching != null)
        {
            previousWeaponIndex = weaponSwitching.GetCurrentWeaponIndex();
            weaponSwitching.SwitchToPreviousWeapon();
        }
    }

    public override void Undo()
    {
        if (weaponSwitching != null)
        {
            weaponSwitching.SwitchToWeapon(previousWeaponIndex);
        }
        base.Undo();
    }
}

// ================================
// КОМАНДИ ВЗАЄМОДІЇ
// ================================

[RegisterCommand("Interact")]
public class InteractCommand : BaseCommand
{
    private PlayerInteraction playerInteraction;

    public override string CommandName => "Interact";

    public InteractCommand(PlayerInteraction playerInteraction)
    {
        this.playerInteraction = playerInteraction;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (playerInteraction != null)
        {
            playerInteraction.TryInteract();
        }
    }
}

// ================================
// КОМБО КОМАНДИ
// ================================

/// <summary>
/// Команда для виконання комбо рухів
/// </summary>
[RegisterCommand("SlideCombo")]
public class SlideComboCommand : CompositeCommand
{
    public override string CommandName => "Slide Combo";

    public SlideComboCommand(PlayerMovement playerMovement)
    {
        // Комбо: Біг → Присідання → Ковзання
        AddCommand(new StartRunningCommand(playerMovement));
        AddCommand(new StartSlidingCommand(playerMovement));
    }
}

/// <summary>
/// Команда для швидкого перемикання зброї та стрільби
/// </summary>
[RegisterCommand("QuickShot")]
public class QuickShotCommand : MacroCommand
{
    public override string CommandName => "Quick Shot";

    public QuickShotCommand(WeaponSwitching weaponSwitching, WeaponController weaponController, MonoBehaviour executor) 
        : base(executor)
    {
        // Макро: Перемикання → Прицілювання → Стрільба → Зняття прицілу
        AddStep(new SwitchToWeaponCommand(weaponSwitching, 0), 0f);
        AddStep(new StartAimingCommand(weaponController), 0.1f);
        AddStep(new FireWeaponCommand(weaponController), 0.2f);
        AddStep(new StopAimingCommand(weaponController), 0.3f);
    }
}

// ================================
// КОМАНДИ НАЛАГОДЖЕННЯ
// ================================

[RegisterCommand("DebugTeleport")]
public class DebugTeleportCommand : ParameterizedCommand<Vector3>
{
    private Transform playerTransform;
    private Vector3 previousPosition;

    public override string CommandName => $"Debug Teleport to {parameter}";
    public override bool CanUndo => true;

    public DebugTeleportCommand(Transform playerTransform, Vector3 position) : base(position)
    {
        this.playerTransform = playerTransform;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (playerTransform != null)
        {
            previousPosition = playerTransform.position;
            playerTransform.position = parameter;
            
            Debug.Log($"Player teleported to {parameter}");
        }
    }

    public override void Undo()
    {
        if (playerTransform != null)
        {
            playerTransform.position = previousPosition;
            Debug.Log($"Player teleported back to {previousPosition}");
        }
        base.Undo();
    }
}

[RegisterCommand("DebugGodMode")]
public class DebugGodModeCommand : BaseCommand
{
    private PlayerHealth playerHealth;
    private bool wasGodModeEnabled;

    public override string CommandName => "Debug God Mode";
    public override bool CanUndo => true;

    public DebugGodModeCommand(PlayerHealth playerHealth)
    {
        this.playerHealth = playerHealth;
    }

    public override void Execute()
    {
        base.Execute();
        
        if (playerHealth != null)
        {
            wasGodModeEnabled = playerHealth.IsGodModeEnabled();
            playerHealth.SetGodMode(!wasGodModeEnabled);
            
            Debug.Log($"God Mode: {(wasGodModeEnabled ? "Disabled" : "Enabled")}");
        }
    }

    public override void Undo()
    {
        if (playerHealth != null)
        {
            playerHealth.SetGodMode(wasGodModeEnabled);
            Debug.Log($"God Mode restored to: {wasGodModeEnabled}");
        }
        base.Undo();
    }
}