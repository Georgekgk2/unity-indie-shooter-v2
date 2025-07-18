using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Центральний менеджер вводу, який використовує Command Pattern для обробки всіх дій гравця.
/// Забезпечує гнучке налаштування клавіш та можливість запису/відтворення геймплею.
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Чи увімкнений Input Manager?")]
    public bool enableInputManager = true;
    [Tooltip("Чи дозволити переназначення клавіш?")]
    public bool allowKeyRebinding = true;
    [Tooltip("Чи логувати команди для налагодження?")]
    public bool logCommands = false;

    [Header("Undo/Redo Settings")]
    [Tooltip("Чи увімкнути функціональність Undo/Redo?")]
    public bool enableUndoRedo = true;
    [Tooltip("Максимальна кількість команд в історії")]
    [Range(10, 100)]
    public int maxCommandHistory = 50;

    [Header("Component References")]
    [Tooltip("Посилання на PlayerMovement")]
    public PlayerMovement playerMovement;
    [Tooltip("Посилання на WeaponController")]
    public WeaponController weaponController;
    [Tooltip("Посилання на WeaponSwitching")]
    public WeaponSwitching weaponSwitching;
    [Tooltip("Посилання на PlayerInteraction")]
    public PlayerInteraction playerInteraction;
    [Tooltip("Посилання на PlayerHealth")]
    public PlayerHealth playerHealth;

    // Command System
    private CommandInvoker commandInvoker;
    private Dictionary<KeyCode, ICommand> keyBindings = new Dictionary<KeyCode, ICommand>();
    private Dictionary<string, ICommand> axisBindings = new Dictionary<string, ICommand>();
    
    // Input State
    private Dictionary<KeyCode, bool> previousKeyStates = new Dictionary<KeyCode, bool>();
    private Dictionary<string, float> previousAxisValues = new Dictionary<string, float>();
    
    // Replay System
    [Header("Replay System")]
    [Tooltip("Чи записувати команди для replay?")]
    public bool enableReplayRecording = false;
    private List<TimedCommand> recordedCommands = new List<TimedCommand>();
    private bool isReplaying = false;
    private float replayStartTime;

    [System.Serializable]
    public struct TimedCommand
    {
        public ICommand command;
        public float timestamp;
        
        public TimedCommand(ICommand command, float timestamp)
        {
            this.command = command;
            this.timestamp = timestamp;
        }
    }

    // Singleton для глобального доступу
    public static InputManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInputManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupDefaultKeyBindings();
        RegisterCommands();
    }

    void Update()
    {
        if (!enableInputManager || isReplaying) return;

        ProcessInput();
        
        // Обробка Undo/Redo
        if (enableUndoRedo)
        {
            ProcessUndoRedo();
        }
    }

    /// <summary>
    /// Ініціалізує Input Manager
    /// </summary>
    void InitializeInputManager()
    {
        commandInvoker = new CommandInvoker();
        
        // Автоматично знаходимо компоненти, якщо не призначені
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        if (weaponController == null) weaponController = FindObjectOfType<WeaponController>();
        if (weaponSwitching == null) weaponSwitching = FindObjectOfType<WeaponSwitching>();
        if (playerInteraction == null) playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();

        Debug.Log("InputManager ініціалізовано");
    }

    /// <summary>
    /// Реєструє всі команди в CommandRegistry
    /// </summary>
    void RegisterCommands()
    {
        if (playerMovement != null)
        {
            CommandRegistry.RegisterCommand("StartWalking", () => new StartWalkingCommand(playerMovement));
            CommandRegistry.RegisterCommand("StartRunning", () => new StartRunningCommand(playerMovement));
            CommandRegistry.RegisterCommand("Jump", () => new JumpCommand(playerMovement));
            CommandRegistry.RegisterCommand("StartCrouching", () => new StartCrouchingCommand(playerMovement));
            CommandRegistry.RegisterCommand("StartSliding", () => new StartSlidingCommand(playerMovement));
            CommandRegistry.RegisterCommand("StopMovement", () => new StopMovementCommand(playerMovement));
        }

        if (weaponController != null)
        {
            CommandRegistry.RegisterCommand("Fire", () => new FireWeaponCommand(weaponController));
            CommandRegistry.RegisterCommand("Reload", () => new ReloadWeaponCommand(weaponController));
            CommandRegistry.RegisterCommand("StartAiming", () => new StartAimingCommand(weaponController));
            CommandRegistry.RegisterCommand("StopAiming", () => new StopAimingCommand(weaponController));
        }

        if (weaponSwitching != null)
        {
            CommandRegistry.RegisterCommand("NextWeapon", () => new NextWeaponCommand(weaponSwitching));
            CommandRegistry.RegisterCommand("PreviousWeapon", () => new PreviousWeaponCommand(weaponSwitching));
        }

        if (playerInteraction != null)
        {
            CommandRegistry.RegisterCommand("Interact", () => new InteractCommand(playerInteraction));
        }

        Debug.Log($"InputManager: Зареєстровано {CommandRegistry.GetRegisteredCommands().Count} команд");
    }

    /// <summary>
    /// Налаштовує стандартні прив'язки клавіш
    /// </summary>
    void SetupDefaultKeyBindings()
    {
        // Рух
        BindKey(KeyCode.Space, "Jump");
        BindKey(KeyCode.LeftControl, "StartCrouching");
        BindKey(KeyCode.LeftShift, "StartRunning");
        
        // Зброя
        BindKey(KeyCode.Mouse0, "Fire");
        BindKey(KeyCode.Mouse1, "StartAiming");
        BindKey(KeyCode.R, "Reload");
        
        // Перемикання зброї
        BindKey(KeyCode.Q, "PreviousWeapon");
        BindKey(KeyCode.E, "NextWeapon");
        BindKey(KeyCode.Alpha1, () => new SwitchToWeaponCommand(weaponSwitching, 0));
        BindKey(KeyCode.Alpha2, () => new SwitchToWeaponCommand(weaponSwitching, 1));
        BindKey(KeyCode.Alpha3, () => new SwitchToWeaponCommand(weaponSwitching, 2));
        
        // Взаємодія
        BindKey(KeyCode.F, "Interact");
        
        // Undo/Redo
        if (enableUndoRedo)
        {
            BindKey(KeyCode.Z, () => new UndoCommand(commandInvoker));
            BindKey(KeyCode.Y, () => new RedoCommand(commandInvoker));
        }

        Debug.Log($"InputManager: Налаштовано {keyBindings.Count} прив'язок клавіш");
    }

    /// <summary>
    /// Прив'язує клавішу до команди за назвою
    /// </summary>
    public void BindKey(KeyCode key, string commandName)
    {
        var command = CommandRegistry.CreateCommand(commandName);
        if (command != null)
        {
            keyBindings[key] = command;
        }
    }

    /// <summary>
    /// Прив'язує клавішу до команди через фабрику
    /// </summary>
    public void BindKey(KeyCode key, Func<ICommand> commandFactory)
    {
        if (commandFactory != null)
        {
            keyBindings[key] = commandFactory();
        }
    }

    /// <summary>
    /// Видаляє прив'язку клавіші
    /// </summary>
    public void UnbindKey(KeyCode key)
    {
        keyBindings.Remove(key);
    }

    /// <summary>
    /// Обробляє весь ввід
    /// </summary>
    void ProcessInput()
    {
        ProcessKeyInput();
        ProcessAxisInput();
        ProcessMouseInput();
    }

    /// <summary>
    /// Обробляє ввід з клавіатури
    /// </summary>
    void ProcessKeyInput()
    {
        foreach (var binding in keyBindings)
        {
            KeyCode key = binding.Key;
            ICommand command = binding.Value;
            
            bool currentKeyState = Input.GetKey(key);
            bool previousKeyState = previousKeyStates.ContainsKey(key) ? previousKeyStates[key] : false;
            
            // Виконуємо команду при натисканні клавіші
            if (Input.GetKeyDown(key))
            {
                ExecuteCommand(command);
            }
            
            // Для команд, які потребують утримання клавіші
            else if (currentKeyState && command is StartRunningCommand)
            {
                ExecuteCommand(command);
            }
            
            // Для команд, які потребують відпускання клавіші
            else if (Input.GetKeyUp(key))
            {
                if (command is StartCrouchingCommand)
                {
                    ExecuteCommand(CommandRegistry.CreateCommand("StopMovement"));
                }
                else if (command is StartAimingCommand)
                {
                    ExecuteCommand(CommandRegistry.CreateCommand("StopAiming"));
                }
            }
            
            previousKeyStates[key] = currentKeyState;
        }
    }

    /// <summary>
    /// Обробляє ввід з осей (WASD, стіки геймпада)
    /// </summary>
    void ProcessAxisInput()
    {
        // Горизонтальний та вертикальний рух
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector2 moveInput = new Vector2(horizontal, vertical);
        
        // Визначаємо тип руху на основі вводу
        if (moveInput.magnitude > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift) && playerMovement.CanSprint())
            {
                ExecuteCommand(CommandRegistry.CreateCommand("StartRunning"));
            }
            else
            {
                ExecuteCommand(CommandRegistry.CreateCommand("StartWalking"));
            }
        }
        else
        {
            ExecuteCommand(CommandRegistry.CreateCommand("StopMovement"));
        }
    }

    /// <summary>
    /// Обробляє ввід миші
    /// </summary>
    void ProcessMouseInput()
    {
        // Колесо миші для перемикання зброї
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            ExecuteCommand(CommandRegistry.CreateCommand("NextWeapon"));
        }
        else if (scroll < 0f)
        {
            ExecuteCommand(CommandRegistry.CreateCommand("PreviousWeapon"));
        }
    }

    /// <summary>
    /// Обробляє Undo/Redo команди
    /// </summary>
    void ProcessUndoRedo()
    {
        if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            commandInvoker.UndoLastCommand();
        }
        
        if (Input.GetKeyDown(KeyCode.Y) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            commandInvoker.RedoLastCommand();
        }
    }

    /// <summary>
    /// Виконує команду через CommandInvoker
    /// </summary>
    public void ExecuteCommand(ICommand command)
    {
        if (command == null) return;

        commandInvoker.ExecuteCommand(command);

        // Записуємо для replay
        if (enableReplayRecording && !isReplaying)
        {
            recordedCommands.Add(new TimedCommand(command, Time.time));
        }

        if (logCommands)
        {
            Debug.Log($"InputManager: Executed {command.CommandName}");
        }
    }

    /// <summary>
    /// Виконує команду за назвою
    /// </summary>
    public void ExecuteCommand(string commandName)
    {
        var command = CommandRegistry.CreateCommand(commandName);
        ExecuteCommand(command);
    }

    /// <summary>
    /// Починає запис команд для replay
    /// </summary>
    public void StartRecording()
    {
        recordedCommands.Clear();
        enableReplayRecording = true;
        Debug.Log("InputManager: Розпочато запис команд");
    }

    /// <summary>
    /// Зупиняє запис команд
    /// </summary>
    public void StopRecording()
    {
        enableReplayRecording = false;
        Debug.Log($"InputManager: Зупинено запис. Записано {recordedCommands.Count} команд");
    }

    /// <summary>
    /// Відтворює записані команди
    /// </summary>
    public void StartReplay()
    {
        if (recordedCommands.Count == 0)
        {
            Debug.LogWarning("InputManager: Немає записаних команд для відтворення");
            return;
        }

        isReplaying = true;
        replayStartTime = Time.time;
        StartCoroutine(ReplayCoroutine());
        Debug.Log($"InputManager: Розпочато відтворення {recordedCommands.Count} команд");
    }

    /// <summary>
    /// Корутина для відтворення команд
    /// </summary>
    System.Collections.IEnumerator ReplayCoroutine()
    {
        float originalTimestamp = recordedCommands[0].timestamp;
        
        foreach (var timedCommand in recordedCommands)
        {
            float relativeTime = timedCommand.timestamp - originalTimestamp;
            float targetTime = replayStartTime + relativeTime;
            
            // Чекаємо до потрібного часу
            while (Time.time < targetTime)
            {
                yield return null;
            }
            
            // Виконуємо команду
            timedCommand.command.Execute();
        }
        
        isReplaying = false;
        Debug.Log("InputManager: Відтворення завершено");
    }

    /// <summary>
    /// Зберігає налаштування клавіш
    /// </summary>
    public void SaveKeyBindings()
    {
        // Тут можна реалізувати збереження в PlayerPrefs або файл
        Debug.Log("InputManager: Налаштування клавіш збережено");
    }

    /// <summary>
    /// Завантажує налаштування клавіш
    /// </summary>
    public void LoadKeyBindings()
    {
        // Тут можна реалізувати завантаження з PlayerPrefs або файлу
        Debug.Log("InputManager: Налаштування клавіш завантажено");
    }

    /// <summary>
    /// Отримує статистику Input Manager
    /// </summary>
    public void GetInputStats(out int keyBindings, out int commandHistory, out int recordedCommands)
    {
        keyBindings = this.keyBindings.Count;
        commandInvoker.GetCommandStats(out commandHistory, out _, out _);
        recordedCommands = this.recordedCommands.Count;
    }

    /// <summary>
    /// Виводить статистику в консоль
    /// </summary>
    [ContextMenu("Print Input Stats")]
    public void PrintInputStats()
    {
        GetInputStats(out int keyBindings, out int commandHistory, out int recordedCommands);
        Debug.Log($"InputManager Stats - Key Bindings: {keyBindings}, Command History: {commandHistory}, Recorded Commands: {recordedCommands}");
        
        commandInvoker.PrintCommandStats();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            CommandRegistry.Clear();
        }
    }
}

// ================================
// ДОПОМІЖНІ КОМАНДИ
// ================================

/// <summary>
/// Команда для Undo функціональності
/// </summary>
public class UndoCommand : BaseCommand
{
    private CommandInvoker invoker;
    public override string CommandName => "Undo";

    public UndoCommand(CommandInvoker invoker)
    {
        this.invoker = invoker;
    }

    public override void Execute()
    {
        base.Execute();
        invoker.UndoLastCommand();
    }
}

/// <summary>
/// Команда для Redo функціональності
/// </summary>
public class RedoCommand : BaseCommand
{
    private CommandInvoker invoker;
    public override string CommandName => "Redo";

    public RedoCommand(CommandInvoker invoker)
    {
        this.invoker = invoker;
    }

    public override void Execute()
    {
        base.Execute();
        invoker.RedoLastCommand();
    }
}