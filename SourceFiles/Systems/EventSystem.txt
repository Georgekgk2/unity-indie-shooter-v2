using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Базовий клас для всіх подій в грі. Всі події повинні наслідуватися від цього класу.
/// </summary>
public abstract class GameEvent
{
    public float TimeStamp { get; private set; }
    
    protected GameEvent()
    {
        TimeStamp = Time.time;
    }
}

/// <summary>
/// Інтерфейс для обробників подій. Компоненти, які хочуть отримувати події, повинні реалізувати цей інтерфейс.
/// </summary>
/// <typeparam name="T">Тип події</typeparam>
public interface IEventHandler<T> where T : GameEvent
{
    void HandleEvent(T eventData);
}

/// <summary>
/// Центральна система подій. Singleton для глобального доступу.
/// Забезпечує типобезпечну підписку та відправку подій.
/// </summary>
public class EventSystem : MonoBehaviour
{
    public static EventSystem Instance { get; private set; }

    // Словник для зберігання обробників подій по типах
    private Dictionary<Type, List<object>> eventHandlers = new Dictionary<Type, List<object>>();
    
    // Черга подій для обробки в наступному кадрі (опціонально)
    private Queue<GameEvent> eventQueue = new Queue<GameEvent>();
    
    [Header("Event System Settings")]
    [Tooltip("Максимальна кількість подій, що обробляються за один кадр")]
    public int maxEventsPerFrame = 10;
    [Tooltip("Чи логувати всі події для налагодження?")]
    public bool logEvents = false;
    [Tooltip("Чи обробляти події в черзі (false = миттєво)")]
    public bool useEventQueue = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("EventSystem ініціалізовано.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (useEventQueue)
        {
            ProcessEventQueue();
        }
    }

    /// <summary>
    /// Підписує обробник на події певного типу
    /// </summary>
    /// <typeparam name="T">Тип події</typeparam>
    /// <param name="handler">Обробник події</param>
    public void Subscribe<T>(IEventHandler<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        
        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = new List<object>();
        }
        
        if (!eventHandlers[eventType].Contains(handler))
        {
            eventHandlers[eventType].Add(handler);
            
            if (logEvents)
            {
                Debug.Log($"EventSystem: {handler.GetType().Name} підписався на {eventType.Name}");
            }
        }
    }

    /// <summary>
    /// Відписує обробник від подій певного типу
    /// </summary>
    /// <typeparam name="T">Тип події</typeparam>
    /// <param name="handler">Обробник події</param>
    public void Unsubscribe<T>(IEventHandler<T> handler) where T : GameEvent
    {
        Type eventType = typeof(T);
        
        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType].Remove(handler);
            
            if (logEvents)
            {
                Debug.Log($"EventSystem: {handler.GetType().Name} відписався від {eventType.Name}");
            }
            
            // Очищуємо порожні списки
            if (eventHandlers[eventType].Count == 0)
            {
                eventHandlers.Remove(eventType);
            }
        }
    }

    /// <summary>
    /// Відправляє подію всім підписаним обробникам
    /// </summary>
    /// <typeparam name="T">Тип події</typeparam>
    /// <param name="eventData">Дані події</param>
    public void TriggerEvent<T>(T eventData) where T : GameEvent
    {
        if (useEventQueue)
        {
            eventQueue.Enqueue(eventData);
        }
        else
        {
            ProcessEvent(eventData);
        }
    }

    /// <summary>
    /// Обробляє подію миттєво
    /// </summary>
    /// <typeparam name="T">Тип події</typeparam>
    /// <param name="eventData">Дані події</param>
    private void ProcessEvent<T>(T eventData) where T : GameEvent
    {
        Type eventType = typeof(T);
        
        if (eventHandlers.ContainsKey(eventType))
        {
            var handlers = eventHandlers[eventType];
            
            if (logEvents)
            {
                Debug.Log($"EventSystem: Обробка {eventType.Name} для {handlers.Count} обробників");
            }
            
            // Створюємо копію списку для безпечної ітерації
            var handlersCopy = new List<object>(handlers);
            
            foreach (var handler in handlersCopy)
            {
                try
                {
                    if (handler is IEventHandler<T> typedHandler)
                    {
                        typedHandler.HandleEvent(eventData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"EventSystem: Помилка при обробці {eventType.Name}: {e.Message}");
                }
            }
        }
        else if (logEvents)
        {
            Debug.LogWarning($"EventSystem: Немає обробників для {eventType.Name}");
        }
    }

    /// <summary>
    /// Обробляє чергу подій
    /// </summary>
    private void ProcessEventQueue()
    {
        int processedEvents = 0;
        
        while (eventQueue.Count > 0 && processedEvents < maxEventsPerFrame)
        {
            var eventData = eventQueue.Dequeue();
            
            // Використовуємо рефлексію для виклику ProcessEvent з правильним типом
            var eventType = eventData.GetType();
            var method = typeof(EventSystem).GetMethod("ProcessEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(eventType);
            
            try
            {
                genericMethod.Invoke(this, new object[] { eventData });
            }
            catch (Exception e)
            {
                Debug.LogError($"EventSystem: Помилка при обробці події з черги: {e.Message}");
            }
            
            processedEvents++;
        }
    }

    /// <summary>
    /// Очищує всі підписки (корисно при зміні сцени)
    /// </summary>
    public void ClearAllSubscriptions()
    {
        eventHandlers.Clear();
        eventQueue.Clear();
        Debug.Log("EventSystem: Всі підписки очищено");
    }

    /// <summary>
    /// Отримує статистику системи подій
    /// </summary>
    public void GetEventSystemStats(out int totalHandlers, out int queuedEvents, out int eventTypes)
    {
        totalHandlers = 0;
        foreach (var handlers in eventHandlers.Values)
        {
            totalHandlers += handlers.Count;
        }
        
        queuedEvents = eventQueue.Count;
        eventTypes = eventHandlers.Count;
    }

    /// <summary>
    /// Виводить статистику в консоль
    /// </summary>
    [ContextMenu("Print Event System Stats")]
    public void PrintEventSystemStats()
    {
        GetEventSystemStats(out int totalHandlers, out int queuedEvents, out int eventTypes);
        Debug.Log($"EventSystem Stats - Event Types: {eventTypes}, Total Handlers: {totalHandlers}, Queued Events: {queuedEvents}");
        
        foreach (var kvp in eventHandlers)
        {
            Debug.Log($"  {kvp.Key.Name}: {kvp.Value.Count} handlers");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            ClearAllSubscriptions();
        }
    }
}

/// <summary>
/// Статичний helper клас для зручного доступу до EventSystem
/// </summary>
public static class Events
{
    /// <summary>
    /// Підписує обробник на події
    /// </summary>
    public static void Subscribe<T>(IEventHandler<T> handler) where T : GameEvent
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.Subscribe(handler);
        }
        else
        {
            Debug.LogError("Events: EventSystem не ініціалізовано!");
        }
    }

    /// <summary>
    /// Відписує обробник від подій
    /// </summary>
    public static void Unsubscribe<T>(IEventHandler<T> handler) where T : GameEvent
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.Unsubscribe(handler);
        }
    }

    /// <summary>
    /// Відправляє подію
    /// </summary>
    public static void Trigger<T>(T eventData) where T : GameEvent
    {
        if (EventSystem.Instance != null)
        {
            EventSystem.Instance.TriggerEvent(eventData);
        }
        else
        {
            Debug.LogError("Events: EventSystem не ініціалізовано!");
        }
    }
}