using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Покращена система прицілювання з динамічним crosshair (Claude рекомендація)
/// Включає адаптивний розмір прицільної сітки та професійний hit detection
/// </summary>
public class AdvancedAimingSystem : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [Tooltip("UI елемент прицільної сітки")]
    public RectTransform crosshairUI;
    [Tooltip("Базовий розмір прицільної сітки")]
    public float baseCrosshairSize = 20f;
    [Tooltip("Максимальний розмір прицільної сітки")]
    public float maxCrosshairSize = 60f;
    [Tooltip("Швидкість розширення прицільної сітки")]
    public float crosshairExpansionRate = 2f;
    [Tooltip("Швидкість повернення прицільної сітки")]
    public float crosshairReturnRate = 5f;
    
    [Header("Crosshair Colors")]
    [Tooltip("Звичайний колір прицільної сітки")]
    public Color normalColor = Color.white;
    [Tooltip("Колір при наведенні на ворога")]
    public Color enemyTargetColor = Color.red;
    [Tooltip("Колір при влученні")]
    public Color hitConfirmColor = Color.green;
    
    [Header("Hit Detection")]
    [Tooltip("Шари для hit detection")]
    public LayerMask hitLayers = -1;
    [Tooltip("Максимальна відстань влучання")]
    public float maxHitDistance = 1000f;
    [Tooltip("Префаб маркера влучання")]
    public GameObject hitMarkerPrefab;
    [Tooltip("Звук влучання")]
    public AudioClip hitSound;
    [Tooltip("Звук хедшота")]
    public AudioClip headshotSound;
    
    [Header("Damage Multipliers")]
    [Tooltip("Множник урону для хедшота")]
    public float headshotMultiplier = 2.5f;
    [Tooltip("Множник урону для тіла")]
    public float bodyMultiplier = 1.0f;
    [Tooltip("Множник урону для кінцівок")]
    public float limbMultiplier = 0.8f;
    
    [Header("Visual Feedback")]
    [Tooltip("Колір звичайного влучання")]
    public Color normalHitColor = Color.white;
    [Tooltip("Колір хедшота")]
    public Color headshotHitColor = Color.red;
    [Tooltip("Колір влучання по кінцівках")]
    public Color limbHitColor = Color.yellow;
    
    [Header("Hit Markers")]
    [Tooltip("Тривалість показу hit marker")]
    public float hitMarkerDuration = 0.3f;
    [Tooltip("Розмір hit marker")]
    public float hitMarkerSize = 30f;
    [Tooltip("Анімація hit marker")]
    public bool animateHitMarker = true;
    
    // Приватні змінні
    private Camera playerCamera;
    private WeaponController weaponController;
    private PlayerMovement playerMovement;
    private Image crosshairImage;
    private float currentCrosshairSize;
    private bool isAiming = false;
    private bool isTargetingEnemy = false;
    private Coroutine hitMarkerCoroutine;
    private Coroutine crosshairColorCoroutine;
    
    // Singleton
    public static AdvancedAimingSystem Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Ініціалізація компонентів
        playerCamera = Camera.main;
        weaponController = GetComponent<WeaponController>();
        playerMovement = GetComponent<PlayerMovement>();
        
        if (crosshairUI != null)
        {
            crosshairImage = crosshairUI.GetComponent<Image>();
        }
        
        currentCrosshairSize = baseCrosshairSize;
        
        // Підписка на події стрільби
        Events.Subscribe<WeaponFiredEvent>(this);
        
        Debug.Log("AdvancedAimingSystem: Ініціалізовано");
    }
    
    void Update()
    {
        UpdateCrosshair();
        HandleAiming();
        CheckTargeting();
    }
    
    /// <summary>
    /// Оновлює розмір та колір прицільної сітки
    /// </summary>
    void UpdateCrosshair()
    {
        if (crosshairUI == null) return;
        
        // Обчислюємо цільовий розмір
        float targetSize = CalculateTargetCrosshairSize();
        
        // Плавно змінюємо розмір
        float changeRate = targetSize > currentCrosshairSize ? crosshairExpansionRate : crosshairReturnRate;
        currentCrosshairSize = Mathf.Lerp(currentCrosshairSize, targetSize, Time.deltaTime * changeRate);
        
        // Застосовуємо розмір
        crosshairUI.sizeDelta = Vector2.one * currentCrosshairSize;
        
        // Оновлюємо колір
        UpdateCrosshairColor();
    }
    
    /// <summary>
    /// Обчислює цільовий розмір прицільної сітки
    /// </summary>
    float CalculateTargetCrosshairSize()
    {
        float targetSize = baseCrosshairSize;
        
        // Збільшуємо при русі
        if (IsPlayerMoving())
        {
            targetSize += 15f;
        }
        
        // Збільшуємо при стрільбі
        if (weaponController != null && weaponController.IsReloading())
        {
            targetSize += 10f;
        }
        
        // Зменшуємо при прицілюванні
        if (isAiming)
        {
            targetSize *= 0.4f;
        }
        
        // Збільшуємо залежно від розкиду зброї
        if (weaponController != null)
        {
            // Можна додати логіку для різних типів зброї
            targetSize += GetWeaponSpreadInfluence();
        }
        
        return Mathf.Clamp(targetSize, baseCrosshairSize * 0.3f, maxCrosshairSize);
    }
    
    /// <summary>
    /// Повертає вплив розкиду зброї на розмір прицільної сітки
    /// </summary>
    float GetWeaponSpreadInfluence()
    {
        // Базова логіка - можна розширити для різних типів зброї
        return 0f;
    }
    
    /// <summary>
    /// Оновлює колір прицільної сітки
    /// </summary>
    void UpdateCrosshairColor()
    {
        if (crosshairImage == null) return;
        
        Color targetColor = normalColor;
        
        if (isTargetingEnemy)
        {
            targetColor = enemyTargetColor;
        }
        
        crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
    }
    
    /// <summary>
    /// Обробляє прицілювання
    /// </summary>
    void HandleAiming()
    {
        isAiming = Input.GetButton("Fire2"); // Права кнопка миші
        
        if (weaponController != null)
        {
            weaponController.SetAiming(isAiming);
        }
    }
    
    /// <summary>
    /// Перевіряє, чи прицілюємося на ворога
    /// </summary>
    void CheckTargeting()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        bool wasTargetingEnemy = isTargetingEnemy;
        isTargetingEnemy = false;
        
        if (Physics.Raycast(ray, out hit, maxHitDistance, hitLayers))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                isTargetingEnemy = true;
            }
        }
        
        // Якщо стан змінився, оновлюємо колір
        if (wasTargetingEnemy != isTargetingEnemy)
        {
            UpdateCrosshairColor();
        }
    }
    
    /// <summary>
    /// Перевіряє, чи рухається гравець
    /// </summary>
    bool IsPlayerMoving()
    {
        if (playerMovement != null)
        {
            return playerMovement.IsMoving();
        }
        
        // Fallback - перевіряємо input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        return Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f;
    }
    
    /// <summary>
    /// Обробляє подію пострілу
    /// </summary>
    public void HandleEvent(WeaponFiredEvent eventData)
    {
        // Розширюємо прицільну сітку після пострілу
        currentCrosshairSize = Mathf.Min(currentCrosshairSize + 15f, maxCrosshairSize);
        
        // Виконуємо покращений hit detection
        PerformAdvancedHitDetection();
    }
    
    /// <summary>
    /// Виконує покращений hit detection
    /// </summary>
    void PerformAdvancedHitDetection()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxHitDistance, hitLayers))
        {
            ProcessAdvancedHit(hit);
        }
    }
    
    /// <summary>
    /// Обробляє покращене влучання
    /// </summary>
    void ProcessAdvancedHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;
        
        // Визначаємо тип влучання
        HitType hitType = DetermineHitType(hit);
        float damageMultiplier = GetDamageMultiplier(hitType);
        
        // Показуємо hit marker
        ShowHitMarker(hitType);
        
        // Візуальний та звуковий фідбек
        ShowAdvancedHitFeedback(hit.point, hit.normal, hitType);
        
        // Публікуємо подію влучання
        Events.Publish(new AdvancedHitEvent(hitObject, damageMultiplier, hitType, hit.point));
    }
    
    /// <summary>
    /// Визначає тип влучання
    /// </summary>
    HitType DetermineHitType(RaycastHit hit)
    {
        string colliderName = hit.collider.name.ToLower();
        
        if (hit.collider.CompareTag("Head") || colliderName.Contains("head"))
            return HitType.Headshot;
        else if (hit.collider.CompareTag("Body") || colliderName.Contains("body") || colliderName.Contains("chest"))
            return HitType.Body;
        else if (hit.collider.CompareTag("Limb") || colliderName.Contains("arm") || colliderName.Contains("leg"))
            return HitType.Limb;
        else
            return HitType.Environment;
    }
    
    /// <summary>
    /// Повертає множник урону для типу влучання
    /// </summary>
    float GetDamageMultiplier(HitType hitType)
    {
        switch (hitType)
        {
            case HitType.Headshot: return headshotMultiplier;
            case HitType.Body: return bodyMultiplier;
            case HitType.Limb: return limbMultiplier;
            default: return 0f; // Оточення не отримує урону
        }
    }
    
    /// <summary>
    /// Показує hit marker в центрі екрану
    /// </summary>
    void ShowHitMarker(HitType hitType)
    {
        if (hitMarkerCoroutine != null)
        {
            StopCoroutine(hitMarkerCoroutine);
        }
        
        hitMarkerCoroutine = StartCoroutine(HitMarkerCoroutine(hitType));
        
        // Змінюємо колір прицільної сітки на момент влучання
        if (crosshairColorCoroutine != null)
        {
            StopCoroutine(crosshairColorCoroutine);
        }
        crosshairColorCoroutine = StartCoroutine(HitConfirmColorCoroutine());
    }
    
    /// <summary>
    /// Корутина для анімації hit marker
    /// </summary>
    IEnumerator HitMarkerCoroutine(HitType hitType)
    {
        // Тут можна створити UI елемент hit marker
        // Поки що просто логуємо
        Debug.Log($"AdvancedAimingSystem: Hit marker - {hitType}");
        
        yield return new WaitForSeconds(hitMarkerDuration);
    }
    
    /// <summary>
    /// Корутина для зміни кольору прицільної сітки при влученні
    /// </summary>
    IEnumerator HitConfirmColorCoroutine()
    {
        if (crosshairImage == null) yield break;
        
        Color originalColor = crosshairImage.color;
        crosshairImage.color = hitConfirmColor;
        
        yield return new WaitForSeconds(0.1f);
        
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            crosshairImage.color = Color.Lerp(hitConfirmColor, originalColor, elapsed / 0.3f);
            yield return null;
        }
        
        crosshairImage.color = originalColor;
    }
    
    /// <summary>
    /// Показує покращений візуальний фідбек
    /// </summary>
    void ShowAdvancedHitFeedback(Vector3 hitPoint, Vector3 hitNormal, HitType hitType)
    {
        // Звуковий фідбек
        AudioClip soundToPlay = hitType == HitType.Headshot ? headshotSound : hitSound;
        if (soundToPlay != null)
        {
            AudioSource.PlayClipAtPoint(soundToPlay, hitPoint);
        }
        
        // Тряска камери
        if (CameraEffects.Instance != null)
        {
            float shakeIntensity = hitType == HitType.Headshot ? 0.3f : 0.1f;
            CameraEffects.Instance.Shake(0.1f, shakeIntensity);
        }
    }
    
    // Публічні методи
    public void SetCrosshairSize(float size)
    {
        currentCrosshairSize = size;
    }
    
    public void SetCrosshairColor(Color color)
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = color;
        }
    }
    
    void OnDestroy()
    {
        Events.Unsubscribe<WeaponFiredEvent>(this);
    }
}

/// <summary>
/// Типи влучань для покращеної системи
/// </summary>
public enum HitType
{
    Environment,
    Body,
    Limb,
    Headshot
}

/// <summary>
/// Подія покращеного влучання
/// </summary>
public class AdvancedHitEvent : GameEvent
{
    public GameObject Target { get; }
    public float DamageMultiplier { get; }
    public HitType HitType { get; }
    public Vector3 HitPoint { get; }
    
    public AdvancedHitEvent(GameObject target, float damageMultiplier, HitType hitType, Vector3 hitPoint)
    {
        Target = target;
        DamageMultiplier = damageMultiplier;
        HitType = hitType;
        HitPoint = hitPoint;
    }
}