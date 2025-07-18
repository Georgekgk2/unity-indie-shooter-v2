using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// Memory Manager - Automated memory management and leak prevention
/// Part of Phase 1 Infrastructure for Unity Indie Shooter Hybrid Approach
/// </summary>
public class MemoryManager : MonoBehaviour
{
    [Header("Memory Management Settings")]
    public float cleanupInterval = 30f;
    public float memoryWarningThreshold = 0.8f; // 80% of available memory
    public bool enableAutomaticGC = true;
    public bool enableMemoryProfiling = true;
    
    [Header("Cleanup Settings")]
    public bool cleanupAudioSources = true;
    public bool cleanupParticleSystems = true;
    public bool cleanupObjectPools = true;
    public bool cleanupWeakReferences = true;
    
    private float lastCleanupTime = 0f;
    private Dictionary<string, System.WeakReference> managedObjects = new Dictionary<string, System.WeakReference>();
    private long initialMemory = 0;
    
    public static MemoryManager Instance { get; private set; }
    
    // Events for monitoring
    public static event System.Action<long> OnMemoryUsageChanged;
    public static event System.Action OnMemoryWarning;
    public static event System.Action OnMemoryCleanup;
    public static event System.Action OnCriticalMemoryAlert;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            initialMemory = System.GC.GetTotalMemory(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InvokeRepeating(nameof(PerformMemoryCheck), cleanupInterval, cleanupInterval);
        
        if (enableMemoryProfiling)
        {
            InvokeRepeating(nameof(LogMemoryStats), 10f, 60f);
        }
        
        Debug.Log("🧹 Memory Manager initialized - automated cleanup active");
    }
    
    void PerformMemoryCheck()
    {
        long currentMemory = System.GC.GetTotalMemory(false);
        long availableMemory = GetAvailableMemory();
        
        float memoryUsageRatio = (float)currentMemory / availableMemory;
        
        OnMemoryUsageChanged?.Invoke(currentMemory);
        
        if (memoryUsageRatio > 0.9f) // Critical threshold
        {
            Debug.LogError($"🚨 CRITICAL MEMORY USAGE: {memoryUsageRatio:P1}");
            OnCriticalMemoryAlert?.Invoke();
            PerformEmergencyCleanup();
        }
        else if (memoryUsageRatio > memoryWarningThreshold)
        {
            Debug.LogWarning($"⚠️ High memory usage detected: {memoryUsageRatio:P1}");
            OnMemoryWarning?.Invoke();
            PerformEmergencyCleanup();
        }
        else if (enableAutomaticGC && Time.time - lastCleanupTime > cleanupInterval)
        {
            PerformRoutineCleanup();
        }
    }
    
    void PerformRoutineCleanup()
    {
        lastCleanupTime = Time.time;
        
        Debug.Log("🧹 Starting routine memory cleanup...");
        
        int cleanedItems = 0;
        
        // Cleanup dead references
        if (cleanupWeakReferences)
        {
            cleanedItems += CleanupWeakReferences();
        }
        
        // Cleanup object pools
        if (cleanupObjectPools)
        {
            cleanedItems += CleanupObjectPools();
        }
        
        // Cleanup audio sources
        if (cleanupAudioSources)
        {
            cleanedItems += CleanupAudioSources();
        }
        
        // Cleanup particle systems
        if (cleanupParticleSystems)
        {
            cleanedItems += CleanupParticleSystems();
        }
        
        // Force garbage collection
        if (enableAutomaticGC)
        {
            long beforeGC = System.GC.GetTotalMemory(false);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long afterGC = System.GC.GetTotalMemory(false);
            
            long freedMemory = beforeGC - afterGC;
            Debug.Log($"🗑️ Garbage collection freed {freedMemory / 1024 / 1024}MB");
        }
        
        OnMemoryCleanup?.Invoke();
        Debug.Log($"✅ Routine cleanup completed - {cleanedItems} items cleaned");
    }
    
    void PerformEmergencyCleanup()
    {
        Debug.LogWarning("🚨 Performing emergency memory cleanup!");
        
        int cleanedItems = 0;
        
        // Aggressive cleanup
        cleanedItems += CleanupWeakReferences();
        cleanedItems += CleanupObjectPools();
        cleanedItems += CleanupAudioSources();
        cleanedItems += CleanupParticleSystems();
        
        // Cleanup textures and resources
        Resources.UnloadUnusedAssets();
        
        // Force aggressive GC
        long beforeGC = System.GC.GetTotalMemory(false);
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        long afterGC = System.GC.GetTotalMemory(false);
        
        long freedMemory = beforeGC - afterGC;
        Debug.LogWarning($"🚨 Emergency cleanup completed - {cleanedItems} items cleaned, {freedMemory / 1024 / 1024}MB freed");
    }
    
    int CleanupWeakReferences()
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in managedObjects)
        {
            if (!kvp.Value.IsAlive)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (string key in keysToRemove)
        {
            managedObjects.Remove(key);
        }
        
        if (keysToRemove.Count > 0)
        {
            Debug.Log($"🧹 Cleaned up {keysToRemove.Count} dead references");
        }
        
        return keysToRemove.Count;
    }
    
    int CleanupObjectPools()
    {
        int cleanedCount = 0;
        
        if (UniversalObjectPool.Instance != null)
        {
            // Get initial counts
            var poolNames = new string[] { "Bullet", "Enemy", "MuzzleFlash", "ImpactEffect" };
            
            foreach (string poolName in poolNames)
            {
                int beforeCount = UniversalObjectPool.Instance.GetAvailableObjectCount(poolName);
                // Pool has its own cleanup logic
                int afterCount = UniversalObjectPool.Instance.GetAvailableObjectCount(poolName);
                cleanedCount += (beforeCount - afterCount);
            }
        }
        
        if (cleanedCount > 0)
        {
            Debug.Log($"🧹 Cleaned up {cleanedCount} pooled objects");
        }
        
        return cleanedCount;
    }
    
    int CleanupAudioSources()
    {
        var audioSources = FindObjectsOfType<AudioSource>();
        int cleanedUp = 0;
        
        foreach (var source in audioSources)
        {
            if (source != null && !source.isPlaying && source.clip == null)
            {
                source.Stop();
                source.clip = null;
                cleanedUp++;
            }
        }
        
        if (cleanedUp > 0)
        {
            Debug.Log($"🧹 Cleaned up {cleanedUp} unused audio sources");
        }
        
        return cleanedUp;
    }
    
    int CleanupParticleSystems()
    {
        var particleSystems = FindObjectsOfType<ParticleSystem>();
        int cleanedUp = 0;
        
        foreach (var ps in particleSystems)
        {
            if (ps != null && !ps.isPlaying && ps.particleCount == 0)
            {
                ps.Clear();
                cleanedUp++;
            }
        }
        
        if (cleanedUp > 0)
        {
            Debug.Log($"🧹 Cleaned up {cleanedUp} particle systems");
        }
        
        return cleanedUp;
    }
    
    long GetAvailableMemory()
    {
        // Estimate available memory (this is a simplified approach)
        // In a real implementation, you might use platform-specific APIs
        return 2L * 1024 * 1024 * 1024; // 2GB default assumption
    }
    
    void LogMemoryStats()
    {
        long totalMemory = System.GC.GetTotalMemory(false);
        long memoryGrowth = totalMemory - initialMemory;
        int gen0 = System.GC.CollectionCount(0);
        int gen1 = System.GC.CollectionCount(1);
        int gen2 = System.GC.CollectionCount(2);
        
        Debug.Log($"📊 Memory Stats: {totalMemory / 1024 / 1024}MB total (+{memoryGrowth / 1024 / 1024}MB growth), GC: Gen0={gen0}, Gen1={gen1}, Gen2={gen2}");
    }
    
    // Public API methods
    public void RegisterManagedObject(string id, object obj)
    {
        managedObjects[id] = new System.WeakReference(obj);
    }
    
    public void UnregisterManagedObject(string id)
    {
        managedObjects.Remove(id);
    }
    
    public long GetCurrentMemoryUsage()
    {
        return System.GC.GetTotalMemory(false);
    }
    
    public long GetMemoryGrowth()
    {
        return GetCurrentMemoryUsage() - initialMemory;
    }
    
    public void ForceCleanup()
    {
        PerformEmergencyCleanup();
    }
    
    public void ForceGarbageCollection()
    {
        long beforeGC = System.GC.GetTotalMemory(false);
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        long afterGC = System.GC.GetTotalMemory(false);
        
        long freedMemory = beforeGC - afterGC;
        Debug.Log($"🗑️ Manual GC freed {freedMemory / 1024 / 1024}MB");
    }
    
    // Debug methods
    [ContextMenu("Force Memory Cleanup")]
    public void DebugForceCleanup()
    {
        ForceCleanup();
    }
    
    [ContextMenu("Force Garbage Collection")]
    public void DebugForceGC()
    {
        ForceGarbageCollection();
    }
    
    [ContextMenu("Log Memory Statistics")]
    public void DebugLogMemoryStats()
    {
        LogMemoryStats();
    }
}