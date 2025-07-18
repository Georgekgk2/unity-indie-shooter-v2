using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quality Gates - Automated quality validation and monitoring
/// Part of Phase 1 Infrastructure for Unity Indie Shooter Hybrid Approach
/// </summary>
public class QualityGates : MonoBehaviour
{
    [Header("Quality Thresholds")]
    public float minimumFPS = 45f;
    public float maximumMemoryMB = 2048f;
    public float minimumTestCoverage = 80f;
    public float maximumCrashRate = 1f;
    public float minimumStabilityScore = 7.5f;
    
    [Header("Validation Settings")]
    public bool enableContinuousValidation = true;
    public float validationInterval = 30f;
    public bool enableAlerts = true;
    public bool enableAutoRollback = false;
    
    [Header("Phase Targets")]
    public PhaseTarget currentPhaseTarget = PhaseTarget.Phase1_Foundation;
    
    private float lastValidationTime = 0f;
    private List<QualityValidationResult> validationHistory = new List<QualityValidationResult>();
    private int consecutiveFailures = 0;
    private const int maxConsecutiveFailures = 3;
    
    public static QualityGates Instance { get; private set; }
    
    // Events
    public static event System.Action<QualityValidationResult> OnQualityValidation;
    public static event System.Action<QualityValidationResult> OnQualityFailure;
    public static event System.Action OnRollbackTriggered;
    
    public enum PhaseTarget
    {
        Phase1_Foundation,
        Phase2_Integration,
        Phase3_Advanced
    }
    
    [System.Serializable]
    public class QualityMetrics
    {
        public float averageFPS;
        public long memoryUsageMB;
        public float testCoverage;
        public float crashRate;
        public float stabilityScore;
        public int activeGameObjects;
        public float sessionDuration;
        public PhaseTarget currentPhase;
    }
    
    [System.Serializable]
    public class QualityValidationResult
    {
        public float timestamp;
        public QualityMetrics metrics;
        public bool overallPassed;
        public List<string> passedChecks;
        public List<string> failedChecks;
        public float qualityScore;
        public QualityGrade grade;
    }
    
    public enum QualityGrade
    {
        Excellent,      // 90-100: Ready for next phase
        Good,           // 80-89: Minor improvements needed
        Acceptable,     // 70-79: Some improvements needed
        NeedsWork,      // 60-69: Significant improvements needed
        Failing         // <60: Major issues, rollback consideration
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
        if (enableContinuousValidation)
        {
            InvokeRepeating(nameof(PerformQualityValidation), validationInterval, validationInterval);
        }
        
        Debug.Log($"🛡️ Quality Gates initialized - monitoring {currentPhaseTarget} targets");
    }
    
    void Update()
    {
        if (enableContinuousValidation && Time.time - lastValidationTime > validationInterval)
        {
            PerformQualityValidation();
            lastValidationTime = Time.time;
        }
    }
    
    public bool ValidateQuality()
    {
        var result = PerformDetailedValidation();
        return result.overallPassed;
    }
    
    void PerformQualityValidation()
    {
        var result = PerformDetailedValidation();
        
        validationHistory.Add(result);
        
        // Maintain history limit
        if (validationHistory.Count > 100)
        {
            validationHistory.RemoveAt(0);
        }
        
        // Handle validation result
        if (result.overallPassed)
        {
            consecutiveFailures = 0;
            if (enableAlerts)
            {
                Debug.Log($"✅ Quality validation passed - Score: {result.qualityScore:F1}/100 ({result.grade})");
            }
        }
        else
        {
            consecutiveFailures++;
            if (enableAlerts)
            {
                Debug.LogWarning($"⚠️ Quality validation failed - Score: {result.qualityScore:F1}/100 ({result.grade})");
                Debug.LogWarning($"Failed checks: {string.Join(", ", result.failedChecks)}");
            }
            
            OnQualityFailure?.Invoke(result);
            
            // Check for rollback trigger
            if (enableAutoRollback && consecutiveFailures >= maxConsecutiveFailures)
            {
                TriggerRollback(result);
            }
        }
        
        OnQualityValidation?.Invoke(result);
    }
    
    QualityValidationResult PerformDetailedValidation()
    {
        var metrics = CollectCurrentMetrics();
        var result = new QualityValidationResult
        {
            timestamp = Time.time,
            metrics = metrics,
            passedChecks = new List<string>(),
            failedChecks = new List<string>(),
            currentPhase = currentPhaseTarget
        };
        
        // Get phase-specific thresholds
        var thresholds = GetPhaseThresholds(currentPhaseTarget);
        
        // Perform individual checks
        bool fpsValid = ValidateFPS(metrics.averageFPS, thresholds.minimumFPS, result);
        bool memoryValid = ValidateMemory(metrics.memoryUsageMB, thresholds.maximumMemoryMB, result);
        bool stabilityValid = ValidateStability(metrics.stabilityScore, thresholds.minimumStabilityScore, result);
        bool performanceValid = ValidateOverallPerformance(metrics, result);
        
        // Calculate overall result
        result.overallPassed = fpsValid && memoryValid && stabilityValid && performanceValid;
        
        // Calculate quality score
        result.qualityScore = CalculateQualityScore(metrics, thresholds);
        result.grade = DetermineQualityGrade(result.qualityScore);
        
        return result;
    }
    
    QualityMetrics CollectCurrentMetrics()
    {
        var performanceMonitor = PerformanceMonitor.Instance;
        var memoryManager = MemoryManager.Instance;
        
        return new QualityMetrics
        {
            averageFPS = performanceMonitor?.GetAverageFPS() ?? 0f,
            memoryUsageMB = memoryManager?.GetCurrentMemoryUsage() / 1024 / 1024 ?? 0,
            testCoverage = GetTestCoverage(),
            crashRate = GetCrashRate(),
            stabilityScore = CalculateStabilityScore(),
            activeGameObjects = FindObjectsOfType<GameObject>().Length,
            sessionDuration = Time.time,
            currentPhase = currentPhaseTarget
        };
    }
    
    PhaseThresholds GetPhaseThresholds(PhaseTarget phase)
    {
        switch (phase)
        {
            case PhaseTarget.Phase1_Foundation:
                return new PhaseThresholds
                {
                    minimumFPS = 45f,
                    maximumMemoryMB = 2048f,
                    minimumStabilityScore = 8.0f,
                    minimumTestCoverage = 80f
                };
            case PhaseTarget.Phase2_Integration:
                return new PhaseThresholds
                {
                    minimumFPS = 40f,
                    maximumMemoryMB = 2500f,
                    minimumStabilityScore = 7.5f,
                    minimumTestCoverage = 85f
                };
            case PhaseTarget.Phase3_Advanced:
                return new PhaseThresholds
                {
                    minimumFPS = 50f,
                    maximumMemoryMB = 2048f,
                    minimumStabilityScore = 8.5f,
                    minimumTestCoverage = 95f
                };
            default:
                return new PhaseThresholds();
        }
    }
    
    struct PhaseThresholds
    {
        public float minimumFPS;
        public float maximumMemoryMB;
        public float minimumStabilityScore;
        public float minimumTestCoverage;
    }
    
    bool ValidateFPS(float currentFPS, float threshold, QualityValidationResult result)
    {
        if (currentFPS >= threshold)
        {
            result.passedChecks.Add($"FPS: {currentFPS:F1} >= {threshold}");
            return true;
        }
        else
        {
            result.failedChecks.Add($"FPS: {currentFPS:F1} < {threshold}");
            return false;
        }
    }
    
    bool ValidateMemory(long currentMemory, float threshold, QualityValidationResult result)
    {
        if (currentMemory <= threshold)
        {
            result.passedChecks.Add($"Memory: {currentMemory}MB <= {threshold}MB");
            return true;
        }
        else
        {
            result.failedChecks.Add($"Memory: {currentMemory}MB > {threshold}MB");
            return false;
        }
    }
    
    bool ValidateStability(float stabilityScore, float threshold, QualityValidationResult result)
    {
        if (stabilityScore >= threshold)
        {
            result.passedChecks.Add($"Stability: {stabilityScore:F1} >= {threshold}");
            return true;
        }
        else
        {
            result.failedChecks.Add($"Stability: {stabilityScore:F1} < {threshold}");
            return false;
        }
    }
    
    bool ValidateOverallPerformance(QualityMetrics metrics, QualityValidationResult result)
    {
        // Additional performance checks
        bool gameObjectCountOK = metrics.activeGameObjects < 10000;
        bool sessionStabilityOK = metrics.sessionDuration > 60f; // At least 1 minute stable
        
        if (gameObjectCountOK)
            result.passedChecks.Add($"GameObject count: {metrics.activeGameObjects} < 10000");
        else
            result.failedChecks.Add($"GameObject count: {metrics.activeGameObjects} >= 10000");
            
        if (sessionStabilityOK)
            result.passedChecks.Add($"Session stability: {metrics.sessionDuration:F0}s >= 60s");
        else
            result.failedChecks.Add($"Session stability: {metrics.sessionDuration:F0}s < 60s");
        
        return gameObjectCountOK && sessionStabilityOK;
    }
    
    float CalculateQualityScore(QualityMetrics metrics, PhaseThresholds thresholds)
    {
        float score = 0f;
        
        // FPS score (25% weight)
        float fpsScore = Mathf.Clamp01(metrics.averageFPS / thresholds.minimumFPS) * 25f;
        
        // Memory score (25% weight)
        float memoryScore = Mathf.Clamp01(thresholds.maximumMemoryMB / metrics.memoryUsageMB) * 25f;
        
        // Stability score (30% weight)
        float stabilityScore = Mathf.Clamp01(metrics.stabilityScore / 10f) * 30f;
        
        // Test coverage score (20% weight)
        float coverageScore = Mathf.Clamp01(metrics.testCoverage / 100f) * 20f;
        
        score = fpsScore + memoryScore + stabilityScore + coverageScore;
        
        return Mathf.Clamp(score, 0f, 100f);
    }
    
    QualityGrade DetermineQualityGrade(float score)
    {
        if (score >= 90f) return QualityGrade.Excellent;
        if (score >= 80f) return QualityGrade.Good;
        if (score >= 70f) return QualityGrade.Acceptable;
        if (score >= 60f) return QualityGrade.NeedsWork;
        return QualityGrade.Failing;
    }
    
    float GetTestCoverage()
    {
        // Placeholder - in real implementation, integrate with test framework
        return 85f; // Assume 85% coverage for Phase 1
    }
    
    float GetCrashRate()
    {
        // Placeholder - in real implementation, track actual crashes
        return 0.5f; // Assume 0.5% crash rate
    }
    
    float CalculateStabilityScore()
    {
        var performanceMonitor = PerformanceMonitor.Instance;
        if (performanceMonitor == null) return 5f;
        
        // Calculate stability based on performance consistency
        float avgFPS = performanceMonitor.GetAverageFPS();
        float minFPS = performanceMonitor.GetMinimumFPS();
        
        if (avgFPS == 0) return 5f;
        
        float fpsStability = minFPS / avgFPS; // Ratio of min to avg FPS
        float stabilityScore = fpsStability * 10f; // Scale to 0-10
        
        return Mathf.Clamp(stabilityScore, 0f, 10f);
    }
    
    void TriggerRollback(QualityValidationResult result)
    {
        Debug.LogError($"🚨 QUALITY GATE FAILURE - Triggering rollback after {consecutiveFailures} consecutive failures");
        Debug.LogError($"Failed checks: {string.Join(", ", result.failedChecks)}");
        
        OnRollbackTriggered?.Invoke();
        
        // In a real implementation, this would trigger actual rollback procedures
        // For now, we'll just log the event
        Debug.LogError("🔄 Rollback procedures would be initiated here");
    }
    
    // Public API methods
    public void SetPhaseTarget(PhaseTarget newPhase)
    {
        currentPhaseTarget = newPhase;
        Debug.Log($"🎯 Quality Gates target updated to {newPhase}");
    }
    
    public QualityValidationResult GetLatestValidation()
    {
        return validationHistory.Count > 0 ? validationHistory[validationHistory.Count - 1] : null;
    }
    
    public List<QualityValidationResult> GetValidationHistory()
    {
        return new List<QualityValidationResult>(validationHistory);
    }
    
    public bool CanProceedToNextPhase()
    {
        var latest = GetLatestValidation();
        return latest != null && latest.overallPassed && latest.qualityScore >= 80f;
    }
    
    // Debug methods
    [ContextMenu("Force Quality Validation")]
    public void ForceValidation()
    {
        PerformQualityValidation();
    }
    
    [ContextMenu("Generate Quality Report")]
    public void GenerateQualityReport()
    {
        var latest = GetLatestValidation();
        if (latest != null)
        {
            Debug.Log($"📊 Quality Report - Score: {latest.qualityScore:F1}/100, Grade: {latest.grade}");
            Debug.Log($"✅ Passed: {string.Join(", ", latest.passedChecks)}");
            if (latest.failedChecks.Count > 0)
            {
                Debug.Log($"❌ Failed: {string.Join(", ", latest.failedChecks)}");
            }
        }
    }
}