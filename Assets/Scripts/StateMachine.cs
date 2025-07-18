using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Базовий клас для всіх станів. Кожен стан має методи входу, виходу, оновлення та обробки подій.
/// </summary>
public abstract class State
{
    protected StateMachine stateMachine;
    protected MonoBehaviour owner;

    public State(StateMachine stateMachine, MonoBehaviour owner)
    {
        this.stateMachine = stateMachine;
        this.owner = owner;
    }

    /// <summary>
    /// Викликається при вході в стан
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// Викликається кожен кадр, поки стан активний
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Викликається в FixedUpdate, поки стан активний
    /// </summary>
    public virtual void FixedUpdate() { }

    /// <summary>
    /// Викликається при виході зі стану
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// Перевіряє, чи можна перейти до іншого стану
    /// </summary>
    public virtual bool CanTransitionTo(Type stateType) { return true; }

    /// <summary>
    /// Обробляє події в поточному стані
    /// </summary>
    public virtual void HandleEvent(GameEvent gameEvent) { }
}

/// <summary>
/// Універсальна машина станів. Може використовуватися для будь-якого компонента.
/// </summary>
public class StateMachine
{
    private Dictionary<Type, State> states = new Dictionary<Type, State>();
    private State currentState;
    private MonoBehaviour owner;

    [Header("State Machine Debug")]
    [SerializeField] private bool logStateChanges = true;
    [SerializeField] private string currentStateName = "None";

    public State CurrentState => currentState;
    public Type CurrentStateType => currentState?.GetType();
    public string CurrentStateName => currentState?.GetType().Name ?? "None";

    public StateMachine(MonoBehaviour owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Додає стан до машини станів
    /// </summary>
    public void AddState<T>(T state) where T : State
    {
        Type stateType = typeof(T);
        if (!states.ContainsKey(stateType))
        {
            states[stateType] = state;
        }
        else
        {
            Debug.LogWarning($"StateMachine: Стан {stateType.Name} вже існує!");
        }
    }

    /// <summary>
    /// Переходить до вказаного стану
    /// </summary>
    public bool ChangeState<T>() where T : State
    {
        return ChangeState(typeof(T));
    }

    /// <summary>
    /// Переходить до вказаного стану за типом
    /// </summary>
    public bool ChangeState(Type stateType)
    {
        // Перевіряємо, чи існує стан
        if (!states.ContainsKey(stateType))
        {
            Debug.LogError($"StateMachine: Стан {stateType.Name} не знайдено!");
            return false;
        }

        // Перевіряємо, чи не той самий стан
        if (currentState != null && currentState.GetType() == stateType)
        {
            return false; // Вже в цьому стані
        }

        // Перевіряємо, чи можна перейти до нового стану
        if (currentState != null && !currentState.CanTransitionTo(stateType))
        {
            if (logStateChanges)
            {
                Debug.LogWarning($"StateMachine: Неможливо перейти з {currentState.GetType().Name} до {stateType.Name}");
            }
            return false;
        }

        State newState = states[stateType];

        // Виходимо з поточного стану
        if (currentState != null)
        {
            currentState.Exit();
            if (logStateChanges)
            {
                Debug.Log($"StateMachine: Вихід зі стану {currentState.GetType().Name}");
            }
        }

        // Входимо в новий стан
        currentState = newState;
        currentStateName = currentState.GetType().Name;
        currentState.Enter();

        if (logStateChanges)
        {
            Debug.Log($"StateMachine: Вхід в стан {currentState.GetType().Name}");
        }

        return true;
    }

    /// <summary>
    /// Запускає машину станів з початковим станом
    /// </summary>
    public void Start<T>() where T : State
    {
        ChangeState<T>();
    }

    /// <summary>
    /// Оновлює поточний стан
    /// </summary>
    public void Update()
    {
        currentState?.Update();
    }

    /// <summary>
    /// Оновлює поточний стан в FixedUpdate
    /// </summary>
    public void FixedUpdate()
    {
        currentState?.FixedUpdate();
    }

    /// <summary>
    /// Передає подію поточному стану
    /// </summary>
    public void HandleEvent(GameEvent gameEvent)
    {
        currentState?.HandleEvent(gameEvent);
    }

    /// <summary>
    /// Перевіряє, чи поточний стан відповідає вказаному типу
    /// </summary>
    public bool IsInState<T>() where T : State
    {
        return currentState != null && currentState.GetType() == typeof(T);
    }

    /// <summary>
    /// Отримує стан за типом
    /// </summary>
    public T GetState<T>() where T : State
    {
        Type stateType = typeof(T);
        if (states.ContainsKey(stateType))
        {
            return states[stateType] as T;
        }
        return null;
    }

    /// <summary>
    /// Отримує всі доступні стани
    /// </summary>
    public List<Type> GetAllStateTypes()
    {
        return new List<Type>(states.Keys);
    }

    /// <summary>
    /// Очищає всі стани (корисно при знищенні об'єкта)
    /// </summary>
    public void Clear()
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState = null;
        }
        states.Clear();
        currentStateName = "None";
    }

    /// <summary>
    /// Отримує статистику машини станів
    /// </summary>
    public void GetStateMachineStats(out int totalStates, out string currentState, out bool isActive)
    {
        totalStates = states.Count;
        currentState = CurrentStateName;
        isActive = this.currentState != null;
    }

    /// <summary>
    /// Виводить статистику в консоль
    /// </summary>
    public void PrintStateMachineStats()
    {
        GetStateMachineStats(out int totalStates, out string currentState, out bool isActive);
        Debug.Log($"StateMachine Stats - Total States: {totalStates}, Current: {currentState}, Active: {isActive}");
        
        foreach (var state in states)
        {
            Debug.Log($"  Available State: {state.Key.Name}");
        }
    }
}

/// <summary>
/// Компонент для відображення поточного стану в Inspector (для налагодження)
/// </summary>
[System.Serializable]
public class StateMachineDebugInfo
{
    [SerializeField] private string currentState = "None";
    [SerializeField] private int totalStates = 0;
    [SerializeField] private bool isActive = false;

    public void UpdateInfo(StateMachine stateMachine)
    {
        if (stateMachine != null)
        {
            stateMachine.GetStateMachineStats(out totalStates, out currentState, out isActive);
        }
    }
}

/// <summary>
/// Атрибут для позначення переходів між станами (для документації)
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
public class StateTransitionAttribute : System.Attribute
{
    public Type FromState { get; }
    public Type ToState { get; }
    public string Condition { get; }

    public StateTransitionAttribute(Type fromState, Type toState, string condition = "")
    {
        FromState = fromState;
        ToState = toState;
        Condition = condition;
    }
}

/// <summary>
/// Інтерфейс для компонентів, які використовують State Machine
/// </summary>
public interface IStateMachineOwner
{
    StateMachine StateMachine { get; }
    void OnStateChanged(Type previousState, Type newState);
}