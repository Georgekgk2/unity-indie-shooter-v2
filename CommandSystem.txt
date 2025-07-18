using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Базовий інтерфейс для всіх команд. Реалізує Command Pattern для гнучкого управління діями.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Виконує команду
    /// </summary>
    void Execute();
    
    /// <summary>
    /// Скасовує команду (для Undo функціональності)
    /// </summary>
    void Undo();
    
    /// <summary>
    /// Чи можна скасувати цю команду?
    /// </summary>
    bool CanUndo { get; }
    
    /// <summary>
    /// Назва команди для логування та налагодження
    /// </summary>
    string CommandName { get; }
    
    /// <summary>
    /// Час виконання команди
    /// </summary>
    float ExecutionTime { get; }
}

/// <summary>
/// Абстрактний базовий клас для команд з загальною функціональністю
/// </summary>
public abstract class BaseCommand : ICommand
{
    public abstract string CommandName { get; }
    public float ExecutionTime { get; private set; }
    public virtual bool CanUndo => false;

    protected bool isExecuted = false;

    public virtual void Execute()
    {
        if (isExecuted) return;
        
        ExecutionTime = Time.time;
        isExecuted = true;
        
        if (GameConstants.logEvents)
        {
            Debug.Log($"Command executed: {CommandName} at {ExecutionTime:F2}");
        }
    }

    public virtual void Undo()
    {
        if (!CanUndo || !isExecuted) return;
        
        isExecuted = false;
        
        if (GameConstants.logEvents)
        {
            Debug.Log($"Command undone: {CommandName}");
        }
    }
}

/// <summary>
/// Команда з параметрами для більшої гнучкості
/// </summary>
/// <typeparam name="T">Тип параметра команди</typeparam>
public abstract class ParameterizedCommand<T> : BaseCommand
{
    protected T parameter;
    
    public ParameterizedCommand(T parameter)
    {
        this.parameter = parameter;
    }
    
    public T Parameter => parameter;
}

/// <summary>
/// Композитна команда для виконання кількох команд одночасно
/// </summary>
public class CompositeCommand : BaseCommand
{
    private List<ICommand> commands = new List<ICommand>();
    public override string CommandName => $"Composite({commands.Count} commands)";
    public override bool CanUndo => true;

    public void AddCommand(ICommand command)
    {
        commands.Add(command);
    }

    public override void Execute()
    {
        base.Execute();
        
        foreach (var command in commands)
        {
            command.Execute();
        }
    }

    public override void Undo()
    {
        if (!CanUndo) return;
        
        // Скасовуємо команди у зворотному порядку
        for (int i = commands.Count - 1; i >= 0; i--)
        {
            if (commands[i].CanUndo)
            {
                commands[i].Undo();
            }
        }
        
        base.Undo();
    }
}

/// <summary>
/// Макро команда для виконання послідовності команд з затримками
/// </summary>
public class MacroCommand : BaseCommand
{
    [System.Serializable]
    public struct CommandStep
    {
        public ICommand command;
        public float delay;
        
        public CommandStep(ICommand command, float delay = 0f)
        {
            this.command = command;
            this.delay = delay;
        }
    }

    private List<CommandStep> steps = new List<CommandStep>();
    private MonoBehaviour executor;
    
    public override string CommandName => $"Macro({steps.Count} steps)";
    public override bool CanUndo => true;

    public MacroCommand(MonoBehaviour executor)
    {
        this.executor = executor;
    }

    public void AddStep(ICommand command, float delay = 0f)
    {
        steps.Add(new CommandStep(command, delay));
    }

    public override void Execute()
    {
        base.Execute();
        
        if (executor != null)
        {
            executor.StartCoroutine(ExecuteMacroCoroutine());
        }
    }

    private System.Collections.IEnumerator ExecuteMacroCoroutine()
    {
        foreach (var step in steps)
        {
            if (step.delay > 0)
            {
                yield return new WaitForSeconds(step.delay);
            }
            
            step.command.Execute();
        }
    }

    public override void Undo()
    {
        if (!CanUndo) return;
        
        // Скасовуємо всі кроки у зворотному порядку
        for (int i = steps.Count - 1; i >= 0; i--)
        {
            if (steps[i].command.CanUndo)
            {
                steps[i].command.Undo();
            }
        }
        
        base.Undo();
    }
}

/// <summary>
/// Invoker - клас, який виконує команди та керує історією
/// </summary>
public class CommandInvoker
{
    private Stack<ICommand> commandHistory = new Stack<ICommand>();
    private Stack<ICommand> undoHistory = new Stack<ICommand>();
    
    [Header("Command History Settings")]
    [SerializeField] private int maxHistorySize = 50;
    [SerializeField] private bool enableUndo = true;
    [SerializeField] private bool logCommands = false;

    public int HistoryCount => commandHistory.Count;
    public int UndoHistoryCount => undoHistory.Count;
    public bool CanUndo => enableUndo && commandHistory.Count > 0;
    public bool CanRedo => enableUndo && undoHistory.Count > 0;

    /// <summary>
    /// Виконує команду та додає її до історії
    /// </summary>
    public void ExecuteCommand(ICommand command)
    {
        if (command == null) return;

        command.Execute();

        if (enableUndo && command.CanUndo)
        {
            commandHistory.Push(command);
            
            // Очищуємо redo історію при новій команді
            undoHistory.Clear();
            
            // Обмежуємо розмір історії
            while (commandHistory.Count > maxHistorySize)
            {
                var oldCommands = new ICommand[commandHistory.Count];
                commandHistory.CopyTo(oldCommands, 0);
                commandHistory.Clear();
                
                for (int i = 0; i < maxHistorySize; i++)
                {
                    commandHistory.Push(oldCommands[i]);
                }
            }
        }

        if (logCommands)
        {
            Debug.Log($"CommandInvoker: Executed {command.CommandName}");
        }
    }

    /// <summary>
    /// Скасовує останню команду
    /// </summary>
    public bool UndoLastCommand()
    {
        if (!CanUndo) return false;

        var command = commandHistory.Pop();
        command.Undo();
        undoHistory.Push(command);

        if (logCommands)
        {
            Debug.Log($"CommandInvoker: Undone {command.CommandName}");
        }

        return true;
    }

    /// <summary>
    /// Повторює скасовану команду
    /// </summary>
    public bool RedoLastCommand()
    {
        if (!CanRedo) return false;

        var command = undoHistory.Pop();
        command.Execute();
        commandHistory.Push(command);

        if (logCommands)
        {
            Debug.Log($"CommandInvoker: Redone {command.CommandName}");
        }

        return true;
    }

    /// <summary>
    /// Очищає всю історію команд
    /// </summary>
    public void ClearHistory()
    {
        commandHistory.Clear();
        undoHistory.Clear();
        
        if (logCommands)
        {
            Debug.Log("CommandInvoker: History cleared");
        }
    }

    /// <summary>
    /// Отримує статистику команд
    /// </summary>
    public void GetCommandStats(out int totalCommands, out int undoableCommands, out int redoableCommands)
    {
        totalCommands = commandHistory.Count;
        undoableCommands = 0;
        redoableCommands = undoHistory.Count;

        foreach (var command in commandHistory)
        {
            if (command.CanUndo) undoableCommands++;
        }
    }

    /// <summary>
    /// Виводить статистику в консоль
    /// </summary>
    public void PrintCommandStats()
    {
        GetCommandStats(out int total, out int undoable, out int redoable);
        Debug.Log($"CommandInvoker Stats - Total: {total}, Undoable: {undoable}, Redoable: {redoable}");
    }
}

/// <summary>
/// Система для реєстрації та виконання команд за назвою (для конфігурації)
/// </summary>
public class CommandRegistry
{
    private static Dictionary<string, Func<ICommand>> commandFactories = new Dictionary<string, Func<ICommand>>();
    
    /// <summary>
    /// Реєструє фабрику команд за назвою
    /// </summary>
    public static void RegisterCommand<T>(string name, Func<T> factory) where T : ICommand
    {
        commandFactories[name] = () => factory();
    }
    
    /// <summary>
    /// Створює команду за назвою
    /// </summary>
    public static ICommand CreateCommand(string name)
    {
        if (commandFactories.ContainsKey(name))
        {
            return commandFactories[name]();
        }
        
        Debug.LogWarning($"CommandRegistry: Command '{name}' not found!");
        return null;
    }
    
    /// <summary>
    /// Отримує всі зареєстровані команди
    /// </summary>
    public static List<string> GetRegisteredCommands()
    {
        return new List<string>(commandFactories.Keys);
    }
    
    /// <summary>
    /// Очищає реєстр команд
    /// </summary>
    public static void Clear()
    {
        commandFactories.Clear();
    }
}

/// <summary>
/// Атрибут для автоматичної реєстрації команд
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class)]
public class RegisterCommandAttribute : System.Attribute
{
    public string CommandName { get; }
    
    public RegisterCommandAttribute(string commandName)
    {
        CommandName = commandName;
    }
}