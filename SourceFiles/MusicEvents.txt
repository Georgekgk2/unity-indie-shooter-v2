using UnityEngine;

/// <summary>
/// Додаткові події для динамічної музичної системи (Claude рекомендація)
/// Розширює існуючу Event System для музичних потреб
/// </summary>

/// <summary>
/// Подія початку бою
/// </summary>
public class CombatStartedEvent : GameEvent
{
    public int EnemyCount { get; }
    public Vector3 CombatLocation { get; }
    public float IntensityLevel { get; }
    
    public CombatStartedEvent(int enemyCount, Vector3 location, float intensity = 1f)
    {
        EnemyCount = enemyCount;
        CombatLocation = location;
        IntensityLevel = intensity;
    }
}

/// <summary>
/// Подія закінчення бою
/// </summary>
public class CombatEndedEvent : GameEvent
{
    public bool PlayerWon { get; }
    public float CombatDuration { get; }
    public int EnemiesKilled { get; }
    
    public CombatEndedEvent(bool playerWon, float duration, int enemiesKilled = 0)
    {
        PlayerWon = playerWon;
        CombatDuration = duration;
        EnemiesKilled = enemiesKilled;
    }
}

/// <summary>
/// Подія зміни музичного стану
/// </summary>
public class MusicStateChangedEvent : GameEvent
{
    public DynamicMusicManager.MusicState PreviousState { get; }
    public DynamicMusicManager.MusicState NewState { get; }
    public float TransitionTime { get; }
    
    public MusicStateChangedEvent(DynamicMusicManager.MusicState previousState, 
                                  DynamicMusicManager.MusicState newState, 
                                  float transitionTime)
    {
        PreviousState = previousState;
        NewState = newState;
        TransitionTime = transitionTime;
    }
}

/// <summary>
/// Подія зміни інтенсивності музики
/// </summary>
public class MusicIntensityChangedEvent : GameEvent
{
    public float IntensityLevel { get; }
    public string Reason { get; }
    
    public MusicIntensityChangedEvent(float intensity, string reason = "")
    {
        IntensityLevel = intensity;
        Reason = reason;
    }
}

/// <summary>
/// Подія для запиту зміни музики
/// </summary>
public class MusicChangeRequestEvent : GameEvent
{
    public DynamicMusicManager.MusicState RequestedState { get; }
    public bool ForceChange { get; }
    public float CustomTransitionTime { get; }
    
    public MusicChangeRequestEvent(DynamicMusicManager.MusicState requestedState, 
                                   bool forceChange = false, 
                                   float customTransitionTime = -1f)
    {
        RequestedState = requestedState;
        ForceChange = forceChange;
        CustomTransitionTime = customTransitionTime;
    }
}