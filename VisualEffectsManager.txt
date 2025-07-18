using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Менеджер візуальних ефектів з підтримкою Object Pooling та Event System.
/// Забезпечує професійні візуальні ефекти для всіх дій в грі.
/// </summary>
public class VisualEffectsManager : MonoBehaviour,
    IEventHandler<WeaponFiredEvent>,
    IEventHandler<PlayerHealthChangedEvent>,
    IEventHandler<PlayerDeathEvent>,
    IEventHandler<CameraShakeEvent>,
    IEventHandler<WeaponReloadStartedEvent>
{
    [Header("Effect Prefabs")]
    [Tooltip("Колекція ефектів зброї")]
    public WeaponEffectsCollection weaponEffects;
    [Tooltip("Колекція ефектів гравця")]
    public PlayerEffectsCollection playerEffects;
    [Tooltip("Колекція ефектів оточення")]
    public EnvironmentEffectsCollection environmentEffects;
    [Tooltip("Колекція ефектів UI")]
    public UIEffectsCollection uiEffects;

    [Header("Pool Settings")]
    [Tooltip("Початковий розмір пулу для кожного ефекту")]
    public int initialPoolSize = 10;
    [Tooltip("Максимальний розмір пулу")]
    public int maxPoolSize = 50;
    [Tooltip("Автоматично розширювати пул?")]
    public bool autoExpandPool = true;

    [Header("Performance Settings")]
    [Tooltip("Максимальна кількість активних ефектів")]
    public int maxActiveEffects = 100;
    [Tooltip("Дистанція відсікання ефектів")]
    public float effectCullingDistance = 100f;
    [Tooltip("Якість ефектів")]
    public EffectQuality effectQuality = EffectQuality.High;

    // Пули ефектів
    private Dictionary<GameObject, Queue<GameObject>> effectPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, List<GameObject>> activeEffects = new Dictionary<GameObject, List<GameObject>>();
    
    // Камера для розрахунку дистанції
    private Camera playerCamera;
    private Transform cameraTransform;
    
    // Лічильники для оптимізації
    private int currentActiveEffects = 0;
    private float lastCullingCheck = 0f;
    private const float cullingCheckInterval = 0.5f;

    // Singleton
    public static VisualEffectsManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEffectsManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Підписуємося на події
        Events.Subscribe<WeaponFiredEvent>(this);
        Events.Subscribe<PlayerHealthChangedEvent>(this);
        Events.Subscribe<PlayerDeathEvent>(this);
        Events.Subscribe<CameraShakeEvent>(this);
        Events.Subscribe<WeaponReloadStartedEvent>(this);

        // Знаходимо камеру
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }
    }

    void Update()
    {
        // Періодично перевіряємо відстань до ефектів для оптимізації
        if (Time.time - lastCullingCheck >= cullingCheckInterval)
        {
            CullDistantEffects();
            lastCullingCheck = Time.time;
        }
    }

    void InitializeEffectsManager()
    {
        // Ініціалізуємо пули для всіх типів ефектів
        InitializeEffectPools();
        
        Debug.Log("VisualEffectsManager ініціалізовано");
    }

    void InitializeEffectPools()
    {
        // Ініціалізуємо пули для ефектів зброї
        if (weaponEffects != null)
        {
            InitializePool(weaponEffects.muzzleFlash);
            InitializePool(weaponEffects.bulletHit);
            InitializePool(weaponEffects.shellEject);
            InitializePool(weaponEffects.bloodSplatter);
            InitializePool(weaponEffects.sparkEffect);
        }

        // Ініціалізуємо пули для ефектів гравця
        if (playerEffects != null)
        {
            InitializePool(playerEffects.damageEffect);
            InitializePool(playerEffects.healEffect);
            InitializePool(playerEffects.deathEffect);
            InitializePool(playerEffects.jumpDust);
            InitializePool(playerEffects.landDust);
        }

        // Ініціалізуємо пули для ефектів оточення
        if (environmentEffects != null)
        {
            InitializePool(environmentEffects.explosion);
            InitializePool(environmentEffects.smoke);
            InitializePool(environmentEffects.debris);
            InitializePool(environmentEffects.fire);
        }
    }

    void InitializePool(GameObject prefab)
    {
        if (prefab == null) return;

        effectPools[prefab] = new Queue<GameObject>();
        activeEffects[prefab] = new List<GameObject>();

        // Створюємо початкові об'єкти в пулі
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject effect = CreatePooledEffect(prefab);
            effectPools[prefab].Enqueue(effect);
        }
    }

    GameObject CreatePooledEffect(GameObject prefab)
    {
        GameObject effect = Instantiate(prefab, transform);
        effect.SetActive(false);
        
        // Додаємо компонент для автоматичного повернення в пул
        PooledEffect pooledComponent = effect.GetComponent<PooledEffect>();
        if (pooledComponent == null)
        {
            pooledComponent = effect.AddComponent<PooledEffect>();
        }
        pooledComponent.Initialize(this, prefab);

        return effect;
    }

    /// <summary>
    /// Відтворює ефект в певній позиції
    /// </summary>
    public GameObject PlayEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation = default, Transform parent = null, float scale = 1f)
    {
        if (effectPrefab == null) return null;

        // Перевіряємо ліміт активних ефектів
        if (currentActiveEffects >= maxActiveEffects)
        {
            return null;
        }

        // Перевіряємо дистанцію до камери
        if (cameraTransform != null)
        {
            float distance = Vector3.Distance(position, cameraTransform.position);
            if (distance > effectCullingDistance)
            {
                return null;
            }
        }

        GameObject effect = GetPooledEffect(effectPrefab);
        if (effect != null)
        {
            effect.transform.position = position;
            effect.transform.rotation = rotation == default ? Quaternion.identity : rotation;
            effect.transform.localScale = Vector3.one * scale;
            
            if (parent != null)
            {
                effect.transform.SetParent(parent);
            }

            effect.SetActive(true);
            currentActiveEffects++;

            // Налаштовуємо якість ефекту
            AdjustEffectQuality(effect);
        }

        return effect;
    }

    /// <summary>
    /// Відтворює ефект з автоматичним знищенням
    /// </summary>
    public GameObject PlayEffectWithDuration(GameObject effectPrefab, Vector3 position, float duration, Quaternion rotation = default, float scale = 1f)
    {
        GameObject effect = PlayEffect(effectPrefab, position, rotation, null, scale);
        if (effect != null)
        {
            StartCoroutine(ReturnEffectToPoolAfterDelay(effect, effectPrefab, duration));
        }
        return effect;
    }

    /// <summary>
    /// Отримує ефект з пулу
    /// </summary>
    GameObject GetPooledEffect(GameObject prefab)
    {
        if (!effectPools.ContainsKey(prefab))
        {
            InitializePool(prefab);
        }

        Queue<GameObject> pool = effectPools[prefab];
        GameObject effect = null;

        if (pool.Count > 0)
        {
            effect = pool.Dequeue();
        }
        else if (autoExpandPool && activeEffects[prefab].Count < maxPoolSize)
        {
            effect = CreatePooledEffect(prefab);
        }

        if (effect != null)
        {
            activeEffects[prefab].Add(effect);
        }

        return effect;
    }

    /// <summary>
    /// Повертає ефект в пул
    /// </summary>
    public void ReturnEffectToPool(GameObject effect, GameObject prefab)
    {
        if (effect == null || !effectPools.ContainsKey(prefab)) return;

        effect.SetActive(false);
        effect.transform.SetParent(transform);
        
        activeEffects[prefab].Remove(effect);
        effectPools[prefab].Enqueue(effect);
        currentActiveEffects--;
    }

    /// <summary>
    /// Повертає ефект в пул після затримки
    /// </summary>
    IEnumerator ReturnEffectToPoolAfterDelay(GameObject effect, GameObject prefab, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnEffectToPool(effect, prefab);
    }

    /// <summary>
    /// Налаштовує якість ефекту
    /// </summary>
    void AdjustEffectQuality(GameObject effect)
    {
        ParticleSystem[] particles = effect.GetComponentsInChildren<ParticleSystem>();
        
        foreach (ParticleSystem ps in particles)
        {
            var main = ps.main;
            var emission = ps.emission;
            
            switch (effectQuality)
            {
                case EffectQuality.Low:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.3f);
                    emission.rateOverTime = emission.rateOverTime.constant * 0.5f;
                    break;
                case EffectQuality.Medium:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.6f);
                    emission.rateOverTime = emission.rateOverTime.constant * 0.75f;
                    break;
                case EffectQuality.High:
                    // Залишаємо оригінальні налаштування
                    break;
                case EffectQuality.Ultra:
                    main.maxParticles = Mathf.RoundToInt(main.maxParticles * 1.5f);
                    emission.rateOverTime = emission.rateOverTime.constant * 1.25f;
                    break;
            }
        }
    }

    /// <summary>
    /// Відключає віддалені ефекти для оптимізації
    /// </summary>
    void CullDistantEffects()
    {
        if (cameraTransform == null) return;

        foreach (var kvp in activeEffects)
        {
            List<GameObject> effects = kvp.Value;
            GameObject prefab = kvp.Key;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                GameObject effect = effects[i];
                if (effect != null && effect.activeInHierarchy)
                {
                    float distance = Vector3.Distance(effect.transform.position, cameraTransform.position);
                    if (distance > effectCullingDistance)
                    {
                        ReturnEffectToPool(effect, prefab);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Очищає всі активні ефекти
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var kvp in activeEffects)
        {
            List<GameObject> effects = kvp.Value;
            GameObject prefab = kvp.Key;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                GameObject effect = effects[i];
                if (effect != null)
                {
                    ReturnEffectToPool(effect, prefab);
                }
            }
        }
    }

    // ================================
    // EVENT HANDLERS
    // ================================

    public void HandleEvent(WeaponFiredEvent eventData)
    {
        if (weaponEffects != null)
        {
            // Muzzle flash
            if (weaponEffects.muzzleFlash != null)
            {
                PlayEffectWithDuration(weaponEffects.muzzleFlash, eventData.FirePosition, 0.1f);
            }

            // Shell ejection
            if (weaponEffects.shellEject != null)
            {
                Vector3 shellPosition = eventData.FirePosition + Random.insideUnitSphere * 0.2f;
                PlayEffectWithDuration(weaponEffects.shellEject, shellPosition, 2f);
            }
        }
    }

    public void HandleEvent(PlayerHealthChangedEvent eventData)
    {
        if (playerEffects != null)
        {
            if (eventData.IsDamage && playerEffects.damageEffect != null)
            {
                // Ефект урону на екрані
                PlayEffectWithDuration(playerEffects.damageEffect, Vector3.zero, 0.5f);
            }
            else if (eventData.IsHealing && playerEffects.healEffect != null)
            {
                // Ефект лікування
                PlayEffectWithDuration(playerEffects.healEffect, Vector3.zero, 1f);
            }
        }
    }

    public void HandleEvent(PlayerDeathEvent eventData)
    {
        if (playerEffects != null && playerEffects.deathEffect != null)
        {
            PlayEffectWithDuration(playerEffects.deathEffect, eventData.DeathPosition, 3f);
        }
    }

    public void HandleEvent(CameraShakeEvent eventData)
    {
        // Можна додати додаткові візуальні ефекти при тряске камери
        if (uiEffects != null && uiEffects.screenDistortion != null)
        {
            PlayEffectWithDuration(uiEffects.screenDistortion, Vector3.zero, eventData.Duration);
        }
    }

    public void HandleEvent(WeaponReloadStartedEvent eventData)
    {
        if (weaponEffects != null && weaponEffects.reloadEffect != null)
        {
            PlayEffectWithDuration(weaponEffects.reloadEffect, Vector3.zero, eventData.ReloadDuration);
        }
    }

    /// <summary>
    /// Отримує статистику ефектів
    /// </summary>
    public void GetEffectStats(out int totalPools, out int activeEffectsCount, out int pooledEffectsCount)
    {
        totalPools = effectPools.Count;
        activeEffectsCount = currentActiveEffects;
        pooledEffectsCount = 0;

        foreach (var pool in effectPools.Values)
        {
            pooledEffectsCount += pool.Count;
        }
    }

    [ContextMenu("Print Effect Stats")]
    public void PrintEffectStats()
    {
        GetEffectStats(out int totalPools, out int active, out int pooled);
        Debug.Log($"VisualEffects Stats - Pools: {totalPools}, Active: {active}, Pooled: {pooled}");
    }

    void OnDestroy()
    {
        Events.Unsubscribe<WeaponFiredEvent>(this);
        Events.Unsubscribe<PlayerHealthChangedEvent>(this);
        Events.Unsubscribe<PlayerDeathEvent>(this);
        Events.Unsubscribe<CameraShakeEvent>(this);
        Events.Unsubscribe<WeaponReloadStartedEvent>(this);
    }

    public enum EffectQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }
}

/// <summary>
/// Компонент для ефектів з пулу
/// </summary>
public class PooledEffect : MonoBehaviour
{
    private VisualEffectsManager effectsManager;
    private GameObject prefab;
    private float autoReturnTime = -1f;

    public void Initialize(VisualEffectsManager manager, GameObject effectPrefab)
    {
        effectsManager = manager;
        prefab = effectPrefab;
    }

    void OnEnable()
    {
        // Автоматично повертаємо в пул, якщо є ParticleSystem
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            autoReturnTime = ps.main.duration + ps.main.startLifetime.constantMax;
            Invoke(nameof(ReturnToPool), autoReturnTime);
        }
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    public void ReturnToPool()
    {
        if (effectsManager != null && prefab != null)
        {
            effectsManager.ReturnEffectToPool(gameObject, prefab);
        }
    }
}

// ================================
// EFFECT COLLECTIONS
// ================================

[System.Serializable]
public class WeaponEffectsCollection
{
    [Header("Weapon Effects")]
    public GameObject muzzleFlash;
    public GameObject bulletHit;
    public GameObject shellEject;
    public GameObject bloodSplatter;
    public GameObject sparkEffect;
    public GameObject reloadEffect;
}

[System.Serializable]
public class PlayerEffectsCollection
{
    [Header("Player Effects")]
    public GameObject damageEffect;
    public GameObject healEffect;
    public GameObject deathEffect;
    public GameObject jumpDust;
    public GameObject landDust;
    public GameObject sprintTrail;
}

[System.Serializable]
public class EnvironmentEffectsCollection
{
    [Header("Environment Effects")]
    public GameObject explosion;
    public GameObject smoke;
    public GameObject debris;
    public GameObject fire;
    public GameObject water;
    public GameObject dust;
}

[System.Serializable]
public class UIEffectsCollection
{
    [Header("UI Effects")]
    public GameObject screenDistortion;
    public GameObject bloodOverlay;
    public GameObject flashEffect;
    public GameObject fadeEffect;
}