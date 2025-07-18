using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// СИСТЕМА ОПТИМІЗАЦІЇ ПРОДУКТИВНОСТІ - PERFORMANCE OPTIMIZATION
/// Автоматична та ручна оптимізація для підтримки 60+ FPS
/// Включає адаптивну якість, LOD системи та профілювання
/// </summary>

// ================================
// РІВНІ ЯКОСТІ
// ================================

public enum QualityLevel
{
    Ultra,          // Ультра якість
    High,           // Висока якість
    Medium,         // Середня якість
    Low,            // Низька якість
    Potato,         // Мінімальна якість
    Auto            // Автоматична
}

public enum PerformanceTarget
{
    FPS_30,         // 30 FPS
    FPS_60,         // 60 FPS
    FPS_120,        // 120 FPS
    FPS_144,        // 144 FPS
    Unlimited       // Без обмежень
}

// ================================
// МЕНЕДЖЕР ПРОДУКТИВНОСТІ
// ================================

public class PerformanceManager : MonoBehaviour
{
    [Header("Performance Settings")]
    public PerformanceTarget targetFPS = PerformanceTarget.FPS_60;
    public QualityLevel currentQuality = QualityLevel.Auto;
    public bool enableAdaptiveQuality = true;
    public bool enableProfiling = true;
    
    [Header("Monitoring")]
    public float fpsUpdateInterval = 1f;
    public int frameCountSample = 60;
    public float performanceCheckInterval = 5f;
    
    [Header("Optimization Thresholds")]
    public float lowFPSThreshold = 45f;
    public float highFPSThreshold = 75f;
    public float memoryWarningThreshold = 0.8f; // 80% від доступної пам'яті
    
    [Header("Quality Settings")]
    public QualitySettings[] qualityPresets;
    
    // Performance metrics
    private float currentFPS = 60f;
    private float averageFPS = 60f;
    private float minFPS = 60f;
    private float maxFPS = 60f;
    private long currentMemoryUsage = 0;
    private long maxMemoryUsage = 0;
    
    // Frame timing
    private Queue<float> frameTimes = new Queue<float>();
    private float lastFPSUpdate = 0f;
    private int frameCount = 0;
    
    // Optimization components
    private LODManager lodManager;
    private CullingManager cullingManager;
    private EffectsManager effectsManager;
    private AudioOptimizer audioOptimizer;
    
    // Auto-quality adjustment
    private float lastQualityAdjustment = 0f;
    private float qualityAdjustmentCooldown = 10f;
    
    public static PerformanceManager Instance { get; private set; }
    
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
        InitializePerformanceSystem();
        StartPerformanceMonitoring();
    }
    
    void InitializePerformanceSystem()
    {
        // Ініціалізація компонентів оптимізації
        lodManager = GetComponent<LODManager>() ?? gameObject.AddComponent<LODManager>();
        cullingManager = GetComponent<CullingManager>() ?? gameObject.AddComponent<CullingManager>();
        effectsManager = GetComponent<EffectsManager>() ?? gameObject.AddComponent<EffectsManager>();
        audioOptimizer = GetComponent<AudioOptimizer>() ?? gameObject.AddComponent<AudioOptimizer>();
        
        // Налаштування цільового FPS
        SetTargetFrameRate();
        
        // Застосування початкових налаштувань якості
        if (currentQuality != QualityLevel.Auto)
        {
            ApplyQualitySettings(currentQuality);
        }
        
        // Ініціалізація профілювання
        if (enableProfiling)
        {
            InitializeProfiling();
        }
    }
    
    void SetTargetFrameRate()
    {
        switch (targetFPS)
        {
            case PerformanceTarget.FPS_30:
                Application.targetFrameRate = 30;
                break;
            case PerformanceTarget.FPS_60:
                Application.targetFrameRate = 60;
                break;
            case PerformanceTarget.FPS_120:
                Application.targetFrameRate = 120;
                break;
            case PerformanceTarget.FPS_144:
                Application.targetFrameRate = 144;
                break;
            case PerformanceTarget.Unlimited:
                Application.targetFrameRate = -1;
                break;
        }
    }
    
    void InitializeProfiling()
    {
        Profiler.enabled = true;
        Profiler.enableBinaryLog = false;
        Profiler.logFile = "";
    }
    
    void StartPerformanceMonitoring()
    {
        StartCoroutine(MonitorPerformance());
        StartCoroutine(UpdateFPSMetrics());
    }
    
    void Update()
    {
        UpdateFrameMetrics();
        
        if (enableAdaptiveQuality && currentQuality == QualityLevel.Auto)
        {
            CheckAdaptiveQuality();
        }
    }
    
    void UpdateFrameMetrics()
    {
        float deltaTime = Time.unscaledDeltaTime;
        
        if (deltaTime > 0f)
        {
            float fps = 1f / deltaTime;
            frameTimes.Enqueue(fps);
            
            if (frameTimes.Count > frameCountSample)
            {
                frameTimes.Dequeue();
            }
            
            currentFPS = fps;
            frameCount++;
        }
    }
    
    IEnumerator UpdateFPSMetrics()
    {
        while (true)
        {
            yield return new WaitForSeconds(fpsUpdateInterval);
            
            if (frameTimes.Count > 0)
            {
                averageFPS = frameTimes.Average();
                minFPS = frameTimes.Min();
                maxFPS = frameTimes.Max();
            }
            
            // Оновлення метрик пам'яті
            UpdateMemoryMetrics();
            
            // Логування метрик
            if (enableProfiling)
            {
                LogPerformanceMetrics();
            }
        }
    }
    
    void UpdateMemoryMetrics()
    {
        currentMemoryUsage = Profiler.GetTotalAllocatedMemory(false);
        
        if (currentMemoryUsage > maxMemoryUsage)
        {
            maxMemoryUsage = currentMemoryUsage;
        }
        
        // Перевірка на критичне використання пам'яті
        long totalMemory = SystemInfo.systemMemorySize * 1024L * 1024L; // MB to bytes
        float memoryUsagePercent = (float)currentMemoryUsage / totalMemory;
        
        if (memoryUsagePercent > memoryWarningThreshold)
        {
            TriggerMemoryOptimization();
        }
    }
    
    void TriggerMemoryOptimization()
    {
        Debug.LogWarning("High memory usage detected. Triggering optimization...");
        
        // Очищення пам'яті
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        
        // Зменшення якості ефектів
        effectsManager?.ReduceEffectQuality();
        
        // Оптимізація аудіо
        audioOptimizer?.OptimizeAudioMemory();
        
        UIManager.Instance?.ShowNotification("Оптимізація пам'яті виконана", NotificationType.Info);
    }
    
    IEnumerator MonitorPerformance()
    {
        while (true)
        {
            yield return new WaitForSeconds(performanceCheckInterval);
            
            AnalyzePerformance();
            
            if (enableAdaptiveQuality)
            {
                AdjustQualityIfNeeded();
            }
        }
    }
    
    void AnalyzePerformance()
    {
        // Аналіз стабільності FPS
        float fpsVariance = maxFPS - minFPS;
        bool isStable = fpsVariance < 10f;
        
        // Аналіз середнього FPS
        bool isPerformanceGood = averageFPS >= GetTargetFPSValue() * 0.9f;
        bool isPerformancePoor = averageFPS < GetTargetFPSValue() * 0.75f;
        
        // Логування результатів аналізу
        if (enableProfiling)
        {
            Debug.Log($"Performance Analysis - Avg FPS: {averageFPS:F1}, Stable: {isStable}, Good: {isPerformanceGood}");
        }
        
        // Тригери для оптимізації
        if (isPerformancePoor)
        {
            TriggerPerformanceOptimization();
        }
    }
    
    float GetTargetFPSValue()
    {
        switch (targetFPS)
        {
            case PerformanceTarget.FPS_30: return 30f;
            case PerformanceTarget.FPS_60: return 60f;
            case PerformanceTarget.FPS_120: return 120f;
            case PerformanceTarget.FPS_144: return 144f;
            default: return 60f;
        }
    }
    
    void TriggerPerformanceOptimization()
    {
        Debug.Log("Poor performance detected. Applying optimizations...");
        
        // Оптимізація LOD
        lodManager?.IncreaseLODBias();
        
        // Оптимізація кулінгу
        cullingManager?.ReduceCullingDistance();
        
        // Зменшення якості ефектів
        effectsManager?.ReduceEffectQuality();
        
        // Оптимізація аудіо
        audioOptimizer?.ReduceAudioQuality();
        
        // Зменшення якості освітлення
        OptimizeLighting();
        
        // Оптимізація тіней
        OptimizeShadows();
    }
    
    void CheckAdaptiveQuality()
    {
        if (Time.time - lastQualityAdjustment < qualityAdjustmentCooldown)
        {
            return;
        }
        
        if (averageFPS < lowFPSThreshold)
        {
            // Зменшення якості
            QualityLevel newQuality = GetLowerQuality(GetCurrentEffectiveQuality());
            if (newQuality != GetCurrentEffectiveQuality())
            {
                ApplyQualitySettings(newQuality);
                lastQualityAdjustment = Time.time;
                
                UIManager.Instance?.ShowNotification($"Якість знижена до {newQuality}", NotificationType.Info);
            }
        }
        else if (averageFPS > highFPSThreshold)
        {
            // Підвищення якості
            QualityLevel newQuality = GetHigherQuality(GetCurrentEffectiveQuality());
            if (newQuality != GetCurrentEffectiveQuality())
            {
                ApplyQualitySettings(newQuality);
                lastQualityAdjustment = Time.time;
                
                UIManager.Instance?.ShowNotification($"Якість підвищена до {newQuality}", NotificationType.Success);
            }
        }
    }
    
    void AdjustQualityIfNeeded()
    {
        // Більш агресивна адаптація якості
        if (averageFPS < GetTargetFPSValue() * 0.8f)
        {
            // Критично низький FPS - негайне зниження якості
            QualityLevel newQuality = GetLowerQuality(GetCurrentEffectiveQuality());
            ApplyQualitySettings(newQuality);
            
            Debug.LogWarning($"Critical FPS drop detected. Quality reduced to {newQuality}");
        }
    }
    
    QualityLevel GetCurrentEffectiveQuality()
    {
        if (currentQuality == QualityLevel.Auto)
        {
            // Визначення поточної ефективної якості на основі налаштувань
            return DetermineCurrentQuality();
        }
        return currentQuality;
    }
    
    QualityLevel DetermineCurrentQuality()
    {
        // Логіка визначення поточної якості на основі налаштувань Unity
        int unityQualityLevel = QualitySettings.GetQualityLevel();
        
        switch (unityQualityLevel)
        {
            case 0: return QualityLevel.Potato;
            case 1: return QualityLevel.Low;
            case 2: return QualityLevel.Medium;
            case 3: return QualityLevel.High;
            case 4: return QualityLevel.Ultra;
            default: return QualityLevel.Medium;
        }
    }
    
    QualityLevel GetLowerQuality(QualityLevel current)
    {
        switch (current)
        {
            case QualityLevel.Ultra: return QualityLevel.High;
            case QualityLevel.High: return QualityLevel.Medium;
            case QualityLevel.Medium: return QualityLevel.Low;
            case QualityLevel.Low: return QualityLevel.Potato;
            case QualityLevel.Potato: return QualityLevel.Potato;
            default: return QualityLevel.Medium;
        }
    }
    
    QualityLevel GetHigherQuality(QualityLevel current)
    {
        switch (current)
        {
            case QualityLevel.Potato: return QualityLevel.Low;
            case QualityLevel.Low: return QualityLevel.Medium;
            case QualityLevel.Medium: return QualityLevel.High;
            case QualityLevel.High: return QualityLevel.Ultra;
            case QualityLevel.Ultra: return QualityLevel.Ultra;
            default: return QualityLevel.Medium;
        }
    }
    
    public void ApplyQualitySettings(QualityLevel quality)
    {
        currentQuality = quality;
        
        switch (quality)
        {
            case QualityLevel.Ultra:
                ApplyUltraQuality();
                break;
            case QualityLevel.High:
                ApplyHighQuality();
                break;
            case QualityLevel.Medium:
                ApplyMediumQuality();
                break;
            case QualityLevel.Low:
                ApplyLowQuality();
                break;
            case QualityLevel.Potato:
                ApplyPotatoQuality();
                break;
        }
        
        // Оновлення компонентів оптимізації
        lodManager?.UpdateLODSettings(quality);
        cullingManager?.UpdateCullingSettings(quality);
        effectsManager?.UpdateEffectSettings(quality);
        audioOptimizer?.UpdateAudioSettings(quality);
    }
    
    void ApplyUltraQuality()
    {
        QualitySettings.SetQualityLevel(4, true);
        
        // Максимальні налаштування
        QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        QualitySettings.shadowDistance = 150f;
        QualitySettings.shadowCascades = 4;
        QualitySettings.antiAliasing = 8;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        QualitySettings.vSyncCount = 1;
        
        // Налаштування рендерингу
        SetRenderScale(1.0f);
        SetTextureQuality(0);
    }
    
    void ApplyHighQuality()
    {
        QualitySettings.SetQualityLevel(3, true);
        
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.shadowDistance = 100f;
        QualitySettings.shadowCascades = 4;
        QualitySettings.antiAliasing = 4;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.vSyncCount = 1;
        
        SetRenderScale(1.0f);
        SetTextureQuality(0);
    }
    
    void ApplyMediumQuality()
    {
        QualitySettings.SetQualityLevel(2, true);
        
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowDistance = 75f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.antiAliasing = 2;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        QualitySettings.vSyncCount = 0;
        
        SetRenderScale(0.9f);
        SetTextureQuality(1);
    }
    
    void ApplyLowQuality()
    {
        QualitySettings.SetQualityLevel(1, true);
        
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowDistance = 50f;
        QualitySettings.shadowCascades = 1;
        QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.vSyncCount = 0;
        
        SetRenderScale(0.8f);
        SetTextureQuality(2);
    }
    
    void ApplyPotatoQuality()
    {
        QualitySettings.SetQualityLevel(0, true);
        
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.vSyncCount = 0;
        
        SetRenderScale(0.7f);
        SetTextureQuality(3);
        
        // Додаткові оптимізації для слабких систем
        OptimizeForLowEnd();
    }
    
    void SetRenderScale(float scale)
    {
        // Налаштування масштабу рендерингу для URP/HDRP
        var pipeline = GraphicsSettings.renderPipelineAsset;
        if (pipeline != null)
        {
            // Логіка для різних render pipeline
        }
    }
    
    void SetTextureQuality(int level)
    {
        QualitySettings.globalTextureMipmapLimit = level;
    }
    
    void OptimizeForLowEnd()
    {
        // Вимкнення дорогих ефектів
        effectsManager?.DisableExpensiveEffects();
        
        // Зменшення дистанції рендерингу
        cullingManager?.SetMinimumCullingDistance();
        
        // Оптимізація частинок
        OptimizeParticles();
        
        // Зменшення якості аудіо
        audioOptimizer?.SetMinimumAudioQuality();
    }
    
    void OptimizeLighting()
    {
        // Зменшення кількості реалтайм світла
        Light[] lights = FindObjectsOfType<Light>();
        int realtimeLights = 0;
        int maxRealtimeLights = GetMaxRealtimeLights();
        
        foreach (var light in lights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Realtime)
            {
                if (realtimeLights >= maxRealtimeLights)
                {
                    light.lightmapBakeType = LightmapBakeType.Baked;
                }
                else
                {
                    realtimeLights++;
                }
            }
        }
    }
    
    int GetMaxRealtimeLights()
    {
        switch (GetCurrentEffectiveQuality())
        {
            case QualityLevel.Ultra: return 8;
            case QualityLevel.High: return 6;
            case QualityLevel.Medium: return 4;
            case QualityLevel.Low: return 2;
            case QualityLevel.Potato: return 1;
            default: return 4;
        }
    }
    
    void OptimizeShadows()
    {
        QualityLevel quality = GetCurrentEffectiveQuality();
        
        switch (quality)
        {
            case QualityLevel.Potato:
                QualitySettings.shadows = ShadowQuality.Disable;
                break;
            case QualityLevel.Low:
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                break;
            case QualityLevel.Medium:
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                break;
        }
    }
    
    void OptimizeParticles()
    {
        ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
        
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.5f);
            
            var emission = particle.emission;
            emission.rateOverTime = emission.rateOverTime.constant * 0.7f;
        }
    }
    
    void LogPerformanceMetrics()
    {
        if (!enableProfiling) return;
        
        string logMessage = $"Performance Metrics - " +
                          $"FPS: {currentFPS:F1} (Avg: {averageFPS:F1}, Min: {minFPS:F1}, Max: {maxFPS:F1}) | " +
                          $"Memory: {currentMemoryUsage / (1024 * 1024)}MB | " +
                          $"Quality: {GetCurrentEffectiveQuality()}";
        
        Debug.Log(logMessage);
    }
    
    // Публічні методи для зовнішнього використання
    public float GetCurrentFPS() => currentFPS;
    public float GetAverageFPS() => averageFPS;
    public long GetMemoryUsage() => currentMemoryUsage;
    public QualityLevel GetQuality() => GetCurrentEffectiveQuality();
    
    public void SetQuality(QualityLevel quality)
    {
        ApplyQualitySettings(quality);
    }
    
    public void ToggleAdaptiveQuality()
    {
        enableAdaptiveQuality = !enableAdaptiveQuality;
        UIManager.Instance?.ShowNotification($"Адаптивна якість: {(enableAdaptiveQuality ? "Увімкнена" : "Вимкнена")}", NotificationType.Info);
    }
    
    public PerformanceMetrics GetPerformanceMetrics()
    {
        return new PerformanceMetrics
        {
            currentFPS = currentFPS,
            averageFPS = averageFPS,
            minFPS = minFPS,
            maxFPS = maxFPS,
            memoryUsage = currentMemoryUsage,
            quality = GetCurrentEffectiveQuality(),
            isStable = (maxFPS - minFPS) < 10f
        };
    }
}

// ================================
// ДОПОМІЖНІ КЛАСИ
// ================================

[System.Serializable]
public class PerformanceMetrics
{
    public float currentFPS;
    public float averageFPS;
    public float minFPS;
    public float maxFPS;
    public long memoryUsage;
    public QualityLevel quality;
    public bool isStable;
}

[System.Serializable]
public class QualitySettings
{
    public string name;
    public int shadowDistance;
    public ShadowResolution shadowResolution;
    public int antiAliasing;
    public float renderScale;
    public int textureQuality;
}

// Спеціалізовані менеджери оптимізації
public class LODManager : MonoBehaviour
{
    public void UpdateLODSettings(QualityLevel quality) { /* Реалізація LOD оптимізації */ }
    public void IncreaseLODBias() { /* Збільшення LOD bias для кращої продуктивності */ }
}

public class CullingManager : MonoBehaviour
{
    public void UpdateCullingSettings(QualityLevel quality) { /* Реалізація culling оптимізації */ }
    public void ReduceCullingDistance() { /* Зменшення дистанції culling */ }
    public void SetMinimumCullingDistance() { /* Мінімальна дистанція для слабких систем */ }
}

public class EffectsManager : MonoBehaviour
{
    public void UpdateEffectSettings(QualityLevel quality) { /* Реалізація ефектів оптимізації */ }
    public void ReduceEffectQuality() { /* Зменшення якості ефектів */ }
    public void DisableExpensiveEffects() { /* Вимкнення дорогих ефектів */ }
}

public class AudioOptimizer : MonoBehaviour
{
    public void UpdateAudioSettings(QualityLevel quality) { /* Реалізація аудіо оптимізації */ }
    public void ReduceAudioQuality() { /* Зменшення якості аудіо */ }
    public void OptimizeAudioMemory() { /* Оптимізація пам'яті аудіо */ }
    public void SetMinimumAudioQuality() { /* Мінімальна якість аудіо */ }
}