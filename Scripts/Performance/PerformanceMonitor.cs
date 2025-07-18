using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

/// <summary>
/// Performance Monitor - Real-time performance tracking and alerting
/// Part of Phase 1 Infrastructure for Unity Indie Shooter Hybrid Approach
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("Monitoring Settings")]
    public bool enableRealTimeMonitoring = true;
    public float reportInterval = 5f;
    public bool enableFileLogging = true;
    public bool enableConsoleLogging = true;
    
    [Header("Performance Thresholds")]
    public float lowFPSThreshold = 45f;
    public float criticalFPSThreshold = 30f;
    public float highMemoryThreshold = 2048f; // MB
    public float criticalMemoryThreshold = 3072f; // MB
    
    [Header("History Settings")]
    public int maxHistoryEntries = 1000;
    public bool enableTrendAnalysis = true;
    
    private float lastReportTime = 0f;
    private List<PerformanceSnapshot> snapshots = new List<PerformanceSnapshot>();
    private Queue<float> fpsHistory = new Queue<float>();
    
    // Performance statistics
    private float averageFPS = 0f;
    private float minimumFPS = float.MaxValue;
    private float maximumFPS = 0f;
    private long averageMemory = 0;
    private long peakMemory = 0;
    
    public static PerformanceMonitor Instance { get; private set; }
    
    // Events for alerts
    public static event System.Action<float> OnLowFPSDetected;
    public static event System.Action<float> OnCriticalFPSDetected;
    public static event System.Action<long> OnHighMemoryDetected;
    public static event System.Action<long> OnCriticalMemoryDetected;
    public static event System.Action<PerformanceSnapshot> OnPerformanceSnapshot;
    
    [System.Serializable]
    public class PerformanceSnapshot
    {
        public float timestamp;
        public float fps;
        public long memoryMB;
        public int activeGameObjects;
        public float cpuTime;
        
        // Trend indicators
        public float fpsChange;
        public long memoryChange;
        public PerformanceLevel performanceLevel;
    }
    
    public enum PerformanceLevel
    {
        Excellent,  // 60+ FPS, <1GB RAM
        Good,       // 45-60 FPS, 1-1.5GB RAM
        Fair,       // 30-45 FPS, 1.5-2GB RAM
        Poor,       // 15-30 FPS, 2-3GB RAM
        Critical    // <15 FPS, >3GB RAM
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (enableFileLogging)
        {
            InitializeFileLogging();
        }
        
        Debug.Log("🔍 Performance Monitor started - tracking FPS, Memory, and System health");
    }
    
    void Update()
    {
        if (enableRealTimeMonitoring && Time.time - lastReportTime > reportInterval)
        {
            TakePerformanceSnapshot();
            lastReportTime = Time.time;
        }
        
        // Update real-time FPS tracking
        float currentFPS = 1f / Time.deltaTime;
        fpsHistory.Enqueue(currentFPS);
        
        if (fpsHistory.Count > 60) // Keep last 60 frames
        {
            fpsHistory.Dequeue();
        }
    }
    
    void TakePerformanceSnapshot()
    {
        var snapshot = new PerformanceSnapshot
        {
            timestamp = Time.time,
            fps = GetCurrentFPS(),
            memoryMB = System.GC.GetTotalMemory(false) / 1024 / 1024,
            activeGameObjects = FindObjectsOfType<GameObject>().Length,
            cpuTime = Time.deltaTime * 1000f, // ms
        };
        
        // Calculate trends
        if (snapshots.Count > 0)
        {
            var lastSnapshot = snapshots[snapshots.Count - 1];
            snapshot.fpsChange = snapshot.fps - lastSnapshot.fps;
            snapshot.memoryChange = snapshot.memoryMB - lastSnapshot.memoryMB;
        }
        
        // Determine performance level
        snapshot.performanceLevel = DeterminePerformanceLevel(snapshot);
        
        snapshots.Add(snapshot);
        
        // Maintain history limit
        if (snapshots.Count > maxHistoryEntries)
        {
            snapshots.RemoveAt(0);
        }
        
        // Update statistics
        UpdateStatistics(snapshot);
        
        // Check thresholds and trigger alerts
        CheckPerformanceThresholds(snapshot);
        
        // Log performance data
        LogPerformanceData(snapshot);
        
        // Trigger event
        OnPerformanceSnapshot?.Invoke(snapshot);
    }
    
    float GetCurrentFPS()
    {
        if (fpsHistory.Count == 0) return 0f;
        return fpsHistory.Average();
    }
    
    PerformanceLevel DeterminePerformanceLevel(PerformanceSnapshot snapshot)
    {
        if (snapshot.fps >= 60f && snapshot.memoryMB < 1024)
            return PerformanceLevel.Excellent;
        else if (snapshot.fps >= 45f && snapshot.memoryMB < 1536)
            return PerformanceLevel.Good;
        else if (snapshot.fps >= 30f && snapshot.memoryMB < 2048)
            return PerformanceLevel.Fair;
        else if (snapshot.fps >= 15f && snapshot.memoryMB < 3072)
            return PerformanceLevel.Poor;
        else
            return PerformanceLevel.Critical;
    }
    
    void UpdateStatistics(PerformanceSnapshot snapshot)
    {
        // FPS statistics
        if (snapshot.fps < minimumFPS) minimumFPS = snapshot.fps;
        if (snapshot.fps > maximumFPS) maximumFPS = snapshot.fps;
        
        // Memory statistics
        if (snapshot.memoryMB > peakMemory) peakMemory = snapshot.memoryMB;
        
        // Calculate averages
        if (snapshots.Count > 0)
        {
            averageFPS = snapshots.Average(s => s.fps);
            averageMemory = (long)snapshots.Average(s => s.memoryMB);
        }
    }
    
    void CheckPerformanceThresholds(PerformanceSnapshot snapshot)
    {
        // FPS threshold checks
        if (snapshot.fps <= criticalFPSThreshold)
        {
            OnCriticalFPSDetected?.Invoke(snapshot.fps);
            if (enableConsoleLogging)
            {
                Debug.LogError($"🚨 CRITICAL FPS: {snapshot.fps:F1} (threshold: {criticalFPSThreshold})");
            }
        }
        else if (snapshot.fps <= lowFPSThreshold)
        {
            OnLowFPSDetected?.Invoke(snapshot.fps);
            if (enableConsoleLogging)
            {
                Debug.LogWarning($"⚠️ Low FPS: {snapshot.fps:F1} (threshold: {lowFPSThreshold})");
            }
        }
        
        // Memory threshold checks
        if (snapshot.memoryMB >= criticalMemoryThreshold)
        {
            OnCriticalMemoryDetected?.Invoke(snapshot.memoryMB);
            if (enableConsoleLogging)
            {
                Debug.LogError($"🚨 CRITICAL MEMORY: {snapshot.memoryMB}MB (threshold: {criticalMemoryThreshold}MB)");
            }
        }
        else if (snapshot.memoryMB >= highMemoryThreshold)
        {
            OnHighMemoryDetected?.Invoke(snapshot.memoryMB);
            if (enableConsoleLogging)
            {
                Debug.LogWarning($"⚠️ High Memory: {snapshot.memoryMB}MB (threshold: {highMemoryThreshold}MB)");
            }
        }
    }
    
    void LogPerformanceData(PerformanceSnapshot snapshot)
    {
        if (enableConsoleLogging)
        {
            string trendIndicator = GetTrendIndicator(snapshot);
            string levelIcon = GetPerformanceLevelIcon(snapshot.performanceLevel);
            Debug.Log($"📊 Performance {levelIcon}: {snapshot.fps:F1} FPS, {snapshot.memoryMB}MB RAM, {snapshot.activeGameObjects} GameObjects {trendIndicator}");
        }
        
        if (enableFileLogging)
        {
            LogToFile(snapshot);
        }
    }
    
    string GetTrendIndicator(PerformanceSnapshot snapshot)
    {
        string fpsTrend = snapshot.fpsChange > 1f ? "📈" : snapshot.fpsChange < -1f ? "📉" : "➡️";
        string memoryTrend = snapshot.memoryChange > 50 ? "📈" : snapshot.memoryChange < -50 ? "📉" : "➡️";
        return $"(FPS {fpsTrend}, RAM {memoryTrend})";
    }
    
    string GetPerformanceLevelIcon(PerformanceLevel level)
    {
        switch (level)
        {
            case PerformanceLevel.Excellent: return "🟢";
            case PerformanceLevel.Good: return "🔵";
            case PerformanceLevel.Fair: return "🟡";
            case PerformanceLevel.Poor: return "🟠";
            case PerformanceLevel.Critical: return "🔴";
            default: return "⚪";
        }
    }
    
    void InitializeFileLogging()
    {
        string logPath = Path.Combine(Application.persistentDataPath, "PerformanceLogs");
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }
        
        string logFile = Path.Combine(logPath, $"performance_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        
        // Write CSV header
        string header = "Timestamp,FPS,MemoryMB,GameObjects,CPUTime,PerformanceLevel,FPSChange,MemoryChange";
        File.WriteAllText(logFile, header + "\n");
        
        Debug.Log($"📝 Performance logging initialized: {logFile}");
    }
    
    void LogToFile(PerformanceSnapshot snapshot)
    {
        string logPath = Path.Combine(Application.persistentDataPath, "PerformanceLogs");
        string logFile = Path.Combine(logPath, $"performance_{System.DateTime.Now:yyyyMMdd}.csv");
        
        string logEntry = $"{snapshot.timestamp:F2},{snapshot.fps:F2},{snapshot.memoryMB},{snapshot.activeGameObjects},{snapshot.cpuTime:F2},{snapshot.performanceLevel},{snapshot.fpsChange:F2},{snapshot.memoryChange}";
        
        File.AppendAllText(logFile, logEntry + "\n");
    }
    
    // Public API methods
    public float GetAverageFPS() => averageFPS;
    public float GetMinimumFPS() => minimumFPS;
    public float GetMaximumFPS() => maximumFPS;
    public long GetAverageMemory() => averageMemory;
    public long GetPeakMemory() => peakMemory;
    public PerformanceLevel GetCurrentPerformanceLevel() => snapshots.Count > 0 ? snapshots[snapshots.Count - 1].performanceLevel : PerformanceLevel.Good;
    
    public List<PerformanceSnapshot> GetPerformanceHistory() => new List<PerformanceSnapshot>(snapshots);
    
    public void ResetStatistics()
    {
        snapshots.Clear();
        fpsHistory.Clear();
        averageFPS = 0f;
        minimumFPS = float.MaxValue;
        maximumFPS = 0f;
        averageMemory = 0;
        peakMemory = 0;
        
        Debug.Log("📊 Performance statistics reset");
    }
    
    // Debug methods
    [ContextMenu("Generate Performance Report")]
    public void GeneratePerformanceReport()
    {
        var report = new
        {
            timestamp = System.DateTime.UtcNow,
            sessionDuration = Time.time,
            averageFPS = averageFPS,
            minimumFPS = minimumFPS,
            maximumFPS = maximumFPS,
            averageMemoryMB = averageMemory,
            peakMemoryMB = peakMemory,
            currentPerformanceLevel = GetCurrentPerformanceLevel(),
            totalSnapshots = snapshots.Count
        };
        
        string reportJson = JsonUtility.ToJson(report, true);
        string reportPath = Path.Combine(Application.persistentDataPath, $"performance_report_{System.DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(reportPath, reportJson);
        
        Debug.Log($"📊 Performance report generated: {reportPath}");
    }
}