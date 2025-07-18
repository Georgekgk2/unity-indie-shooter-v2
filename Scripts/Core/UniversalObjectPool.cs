using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

/// <summary>
/// Universal Object Pool - Production Ready Implementation
/// Part of Phase 1 Infrastructure for Unity Indie Shooter Hybrid Approach
/// </summary>
public class UniversalObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        [Header("Pool Configuration")]
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 100;
        public bool allowGrowth = true;
        public bool preWarm = true;
        
        [Header("Performance Settings")]
        public float cleanupInterval = 60f;
        public int maxCleanupPerFrame = 5;
    }
    
    [Header("Pool Configurations")]
    public PoolConfig[] poolConfigs = new PoolConfig[]
    {
        new PoolConfig 
        { 
            poolName = "Bullet", 
            initialSize = 50, 
            maxSize = 200,
            allowGrowth = true,
            preWarm = true
        },
        new PoolConfig 
        { 
            poolName = "Enemy", 
            initialSize = 20, 
            maxSize = 100,
            allowGrowth = true,
            preWarm = false
        },
        new PoolConfig 
        { 
            poolName = "MuzzleFlash", 
            initialSize = 10, 
            maxSize = 50,
            allowGrowth = true,
            preWarm = true
        },
        new PoolConfig 
        { 
            poolName = "ImpactEffect", 
            initialSize = 20, 
            maxSize = 100,
            allowGrowth = true,
            preWarm = true
        }
    };
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showPoolStats = true;
    
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolConfig> configs = new Dictionary<string, PoolConfig>();
    private Dictionary<GameObject, string> activeObjects = new Dictionary<GameObject, string>();
    private Dictionary<string, int> poolStats = new Dictionary<string, int>();
    
    public static UniversalObjectPool Instance { get; private set; }
    
    // Events for monitoring
    public static event System.Action<string, int> OnPoolExpanded;
    public static event System.Action<string, int, int> OnPoolStatsUpdated;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartCoroutine(PeriodicCleanup());
        
        if (showPoolStats)
        {
            InvokeRepeating(nameof(LogPoolStats), 10f, 30f);
        }
        
        Debug.Log("🔧 Universal Object Pool initialized successfully");
    }
    
    void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogError($"Pool '{config.poolName}' has no prefab assigned!");
                continue;
            }
            
            var pool = new Queue<GameObject>();
            
            // Pre-populate pool if enabled
            if (config.preWarm)
            {
                for (int i = 0; i < config.initialSize; i++)
                {
                    GameObject obj = CreatePooledObject(config);
                    pool.Enqueue(obj);
                }
            }
            
            pools[config.poolName] = pool;
            configs[config.poolName] = config;
            poolStats[config.poolName] = 0;
            
            if (enableDebugLogs)
            {
                Debug.Log($"✅ Initialized pool '{config.poolName}' with {config.initialSize} objects");
            }
        }
    }
    
    GameObject CreatePooledObject(PoolConfig config)
    {
        GameObject obj = Instantiate(config.prefab);
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        
        // Add poolable component if not present
        if (obj.GetComponent<IPoolable>() == null)
        {
            obj.AddComponent<DefaultPoolable>();
        }
        
        return obj;
    }
    
    public GameObject Get(string poolName)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogError($"Pool '{poolName}' not found!");
            return null;
        }
        
        var pool = pools[poolName];
        var config = configs[poolName];
        
        GameObject obj;
        
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else if (config.allowGrowth)
        {
            obj = CreatePooledObject(config);
            OnPoolExpanded?.Invoke(poolName, poolStats[poolName] + 1);
            
            if (enableDebugLogs)
            {
                Debug.Log($"📈 Pool '{poolName}' expanded (new size: {poolStats[poolName] + 1})");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Pool '{poolName}' is empty and growth is disabled!");
            return null;
        }
        
        obj.SetActive(true);
        activeObjects[obj] = poolName;
        poolStats[poolName]++;
        
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnSpawnFromPool();
        
        OnPoolStatsUpdated?.Invoke(poolName, pool.Count, poolStats[poolName]);
        
        return obj;
    }
    
    public void Return(GameObject obj)
    {
        if (obj == null) return;
        
        if (!activeObjects.ContainsKey(obj))
        {
            Debug.LogWarning("⚠️ Trying to return object that wasn't from pool!");
            Destroy(obj);
            return;
        }
        
        string poolName = activeObjects[obj];
        var pool = pools[poolName];
        var config = configs[poolName];
        
        var poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        if (pool.Count < config.maxSize)
        {
            pool.Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
        
        activeObjects.Remove(obj);
        poolStats[poolName]--;
        
        OnPoolStatsUpdated?.Invoke(poolName, pool.Count, poolStats[poolName]);
    }
    
    public void ReturnAfterDelay(GameObject obj, float delay)
    {
        if (obj != null)
        {
            StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
        }
    }
    
    IEnumerator ReturnAfterDelayCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(obj);
    }
    
    IEnumerator PeriodicCleanup()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            
            foreach (var kvp in pools)
            {
                var config = configs[kvp.Key];
                var pool = kvp.Value;
                
                int excessCount = pool.Count - config.initialSize;
                int cleanupCount = Mathf.Min(excessCount, config.maxCleanupPerFrame);
                
                for (int i = 0; i < cleanupCount; i++)
                {
                    if (pool.Count > config.initialSize)
                    {
                        var obj = pool.Dequeue();
                        Destroy(obj);
                    }
                }
                
                if (cleanupCount > 0 && enableDebugLogs)
                {
                    Debug.Log($"🧹 Cleaned up {cleanupCount} excess objects from pool '{kvp.Key}'");
                }
            }
        }
    }
    
    public void LogPoolStats()
    {
        if (!showPoolStats) return;
        
        foreach (var kvp in pools)
        {
            string poolName = kvp.Key;
            int available = kvp.Value.Count;
            int active = poolStats[poolName];
            
            Debug.Log($"📊 Pool '{poolName}': {available} available, {active} active");
        }
    }
    
    public int GetActiveObjectCount(string poolName)
    {
        return poolStats.ContainsKey(poolName) ? poolStats[poolName] : 0;
    }
    
    public int GetAvailableObjectCount(string poolName)
    {
        return pools.ContainsKey(poolName) ? pools[poolName].Count : 0;
    }
}

// Interface for poolable objects
public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}

// Default poolable component
public class DefaultPoolable : MonoBehaviour, IPoolable
{
    public virtual void OnSpawnFromPool()
    {
        // Override in derived classes for custom spawn behavior
    }
    
    public virtual void OnReturnToPool()
    {
        // Default cleanup
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        var trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }
    }
}