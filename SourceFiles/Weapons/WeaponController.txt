using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Setup")]
    [Tooltip("Префаб кулі, яка буде спавнитися")]
    public GameObject bulletPrefab;
    [Tooltip("Точка, з якої буде спавнитися і запускатися куля")]
    public Transform bulletSpawnPoint;
    [Tooltip("Сила, з якою куля буде запущена")]
    public float bulletForce = 30f;
    [Tooltip("Множник розкиду кулі (чим вище, тим більше розкид)")]
    public float bulletSpread = 0.05f;

    [Tooltip("Чи має куля летіти прямо в центр екрану (Raycast Aiming)?")]
    public bool useRaycastAiming = true;
    [Tooltip("Максимальна відстань для Raycast Aiming")]
    public float maxAimDistance = 500f;


    [Header("Fire Rate & Ammo")]
    [Tooltip("Кількість пострілів за секунду")]
    public float fireRate = 8f;
    [Tooltip("Розмір магазину (кількість патронів)")]
    public int magazineSize = 30;
    [Tooltip("Час перезарядки (у секундах)")]
    public float reloadTime = 2f;
    [Tooltip("Кнопка для перезарядки")]
    public KeyCode reloadKey = KeyCode.R;
    [Tooltip("Кількість доступних перезарядок для цієї зброї")] // НОВЕ ПОЛЕ
    [SerializeField] private int reloadCharges = 1; // За замовчуванням даємо 1 заряд

    [Header("Visual & Audio Effects")]
    [Tooltip("Система частинок для спалаху дула")]
    public ParticleSystem muzzleFlash;
    [Tooltip("Аудіо джерело для пострілів")]
    public AudioSource audioSource;
    [Tooltip("Звуковий кліп для пострілу")]
    public AudioClip shootSoundClip;
    [Tooltip("Звуковий кліп для перезарядки")]
    public AudioClip reloadSoundClip;
    [Tooltip("Звуковий кліп, коли немає патронів")]
    public AudioClip emptyClipSound;

    [Header("Aim Down Sights (ADS) Settings")]
    [Tooltip("Чи підтримує ця зброя режим прицілювання (ADS)?")]
    public bool canAim = true;
    [Tooltip("Кнопка для входу/виходу з режиму прицілювання.")]
    public KeyCode aimKey = KeyCode.Mouse1;
    [Tooltip("Локальна позиція зброї, коли вона прицілена (ADS).")]
    public Vector3 aimPosition;
    [Tooltip("Швидкість переходу до/від режиму прицілювання.")]
    public float aimSpeed = 10f;
    [Tooltip("Значення FOV камери, коли зброя прицілена.")]
    public float aimFOV = 40f;
    [Tooltip("Множник розкиду кулі, коли зброя прицілена (1.0 = без змін, 0.5 = половина розкиду).")]
    [Range(0f, 1f)]
    public float aimSpreadMultiplier = 0.5f;

    [Header("Recoil Settings")]
    [Tooltip("Вертикальна віддача (як сильно камера підкидається вгору)")]
    public float recoilX = 2f;
    [Tooltip("Горизонтальна віддача (як сильно камера відхиляється вліво/вправо)")]
    public float recoilY = 0.5f;
    [Tooltip("Віддача по Z-осі (нахил)")]
    public float recoilZ = 0.5f;
    [Tooltip("Швидкість, з якою віддача застосовується")]
    public float recoilSnappiness = 10f;
    [Tooltip("Швидкість, з якою камера повертається після віддачі")]
    public float recoilReturnSpeed = 5f;
    [Tooltip("Множник віддачі, коли зброя прицілена (менше 1 = менша віддача).")]
    [Range(0f, 1f)]
    public float aimRecoilMultiplier = 0.5f;

    [Header("Pickup & Drop Settings")]
    [Tooltip("Префаб світського об'єкта цієї зброї для викидання.")]
    public GameObject weaponWorldPrefab;
    [Tooltip("Чи можна викинути цю зброю? (Наприклад, не можна викинути кулаки або базовий ніж)")]
    public bool isDroppable = true;
    [Tooltip("Ім'я зброї для відображення в UI.")]
    public string weaponDisplayName = "Weapon";
    [Tooltip("Іконка зброї для відображення в UI.")]
    public Sprite weaponIcon;

    // Приватні змінні для стану зброї
    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private Camera mainCamera;

    private Vector3 initialWeaponLocalPosition;
    private float initialCameraFOV;
    private bool isAiming = false;

    // Посилання на інші скрипти гравця
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private MouseLook mouseLook;

    // Змінні для керування віддачею
    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;

    // Корутини для безпечного управління
    private Coroutine reloadCoroutine;

    void Awake()
    {
        currentAmmo = magazineSize;
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("WeaponController: Main Camera не знайдена. Функції камери можуть працювати некоректно.", this);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("WeaponController: AudioSource не знайдено на об'єкті зброї. Звуки не будуть відтворюватися.", this);
            }
        }

        initialWeaponLocalPosition = transform.localPosition;
        if (mainCamera != null)
        {
            initialCameraFOV = mainCamera.fieldOfView;
        }

        playerMovement = GetComponentInParent<PlayerMovement>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        mouseLook = GetComponentInParent<MouseLook>();
        if (mouseLook == null)
        {
            Debug.LogWarning("WeaponController: MouseLook не знайдено в батьківських об'єктах. Віддача не працюватиме.", this);
        }
    }

    void OnEnable() // Викликається, коли GameObject зброї активується
    {
        OnEquip(); // Викликаємо OnEquip при активації (екіпіровці)
    }

    void OnDisable() // Викликається, коли GameObject зброї деактивується
    {
        // Безпечно зупиняємо всі корутини
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        OnUnequip(); // Викликаємо OnUnequip при деактивації (знятті)
    }


    void Update()
    {
        // Не дозволяємо стріляти/перезаряджатися/прицілюватися, якщо гравець мертвий
        if (playerHealth != null && playerHealth.IsDead())
        {
            if (isAiming) ResetADS();
            // Повертаємо віддачу, якщо гравець мертвий
            targetRecoilRotation = Vector3.zero;
            currentRecoilRotation = Vector3.zero;
            UpdateRecoil(); // Оновлюємо віддачу, щоб вона згасала
            return;
        }

        // Не дозволяємо стріляти або перезаряджатися, якщо ми вже перезаряджаємося
        if (isReloading)
        {
            if (isAiming) ResetADS(); // Якщо перезаряджаємося, виходимо з ADS
            UpdateRecoil(); // Оновлюємо віддачу, щоб вона згасала
            return;
        }

        // --- Обробка прицілювання (ADS) ---
        if (canAim && playerMovement != null)
        {
            // Не дозволяємо прицілюватися під час спрінту, ковзання або ривка
            if (playerMovement.IsSprinting() || playerMovement.IsSliding() || playerMovement.IsDashing())
            {
                if (isAiming) ResetADS();
            }
            else // Якщо не в цих станах, дозволяємо прицілюватися
            {
                if (Input.GetKey(aimKey)) // "Утримувати" для прицілювання
                {
                    if (!isAiming) isAiming = true;
                }
                else
                {
                    if (isAiming) ResetADS();
                }
            }
        }

        // Плавне переміщення зброї та FOV камери
        UpdateWeaponADS();


        // Обробка пострілу (ЛКМ)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                if (audioSource != null && emptyClipSound != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(emptyClipSound);
                }
                Reload(); 
            }
        }

        // Обробка перезарядки (клавіша R)
        if (Input.GetKeyDown(reloadKey) && currentAmmo < magazineSize)
        {
            Reload();
        }

        // Оновлення віддачі (відбувається кожен кадр)
        UpdateRecoil();
    }
    
    /// <summary>
    /// Плавне переміщення зброї до цільової позиції (ADS або Hip) та зміна FOV.
    /// </summary>
    void UpdateWeaponADS()
    {
        Vector3 targetPosition = isAiming ? aimPosition : initialWeaponLocalPosition;
        float targetFOV = isAiming ? aimFOV : initialCameraFOV;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * aimSpeed);

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);
        }
    }

    /// <summary>
    /// Метод для виконання пострілу.
    /// </summary>
    void Shoot()
    {
        nextFireTime = Time.time + 1f / fireRate;
        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSoundClip != null)
        {
            audioSource.PlayOneShot(shootSoundClip);
        }

        if (bulletPrefab == null || bulletSpawnPoint == null)
        {
            Debug.LogWarning("WeaponController: Bullet Prefab або Bullet Spawn Point не призначено.", this);
            return;
        }

        Vector3 shootDirection;
        if (useRaycastAiming && mainCamera != null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out hit, maxAimDistance))
            {
                shootDirection = (hit.point - bulletSpawnPoint.position).normalized;
            }
            else
            {
                shootDirection = ray.direction;
            }
        }
        else
        {
            shootDirection = bulletSpawnPoint.forward;
        }

        // Застосовуємо розкид, враховуючи множник для ADS
        float currentSpread = bulletSpread;
        if (isAiming)
        {
            currentSpread *= aimSpreadMultiplier;
        }

        shootDirection += new Vector3(
            Random.Range(-currentSpread, currentSpread),
            Random.Range(-currentSpread, currentSpread),
            Random.Range(-currentSpread, currentSpread)
        );
        shootDirection.Normalize();

        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);

        // Отримуємо кулю з пулу замість створення нової
        GameObject bullet = null;
        if (BulletPool.Instance != null)
        {
            bullet = BulletPool.Instance.GetBullet(bulletSpawnPoint.position, bulletRotation);
        }
        else
        {
            // Fallback: створюємо кулю як раніше, якщо пул недоступний
            Debug.LogWarning("WeaponController: BulletPool недоступний, використовується Instantiate.");
            bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);
        }

        if (bullet == null)
        {
            Debug.LogWarning("WeaponController: Не вдалося отримати кулю (пул вичерпано?)");
            return;
        } 
        
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.AddForce(shootDirection * bulletForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("WeaponController: На префабі кулі немає Rigidbody. Куля не буде рухатися з імпульсом.", bullet);
        }

        ApplyRecoil(); // Застосовуємо віддачу після пострілу

        // Перевіряємо влучання для damage numbers (Claude покращення)
        CheckForHitAndShowDamage(shootDirection);

        // Відправляємо подію пострілу
        Events.Trigger(new WeaponFiredEvent(weaponDisplayName, bulletSpawnPoint.position, shootDirection, currentAmmo, bullet));

        // Відправляємо подію зміни патронів
        Events.Trigger(new AmmoChangedEvent(weaponDisplayName, currentAmmo, magazineSize, reloadCharges));
    }

    /// <summary>
    /// Метод для початку перезарядки.
    /// </summary>
    void Reload()
    {
        // Перевіряємо, чи не повний магазин, чи не перезаряджаємося, і чи є перезарядки
        if (currentAmmo == magazineSize || isReloading || reloadCharges <= 0)
        {
            if (reloadCharges <= 0) Debug.Log($"Неможливо перезарядити {weaponDisplayName}: немає перезарядок.");
            return;
        }

        isReloading = true;
        reloadCharges--; // Витрачаємо одну перезарядку
        if (isAiming) ResetADS(); // Виходимо з режиму прицілювання при перезарядці
        // Скидаємо віддачу при перезарядці
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        
        Debug.Log($"Reloading {weaponDisplayName}... Reload charges left: {reloadCharges}");

        // Відправляємо подію початку перезарядки
        Events.Trigger(new WeaponReloadStartedEvent(weaponDisplayName, reloadTime, reloadCharges));

        if (audioSource != null && reloadSoundClip != null)
        {
            audioSource.PlayOneShot(reloadSoundClip);
        }

        // Безпечно запускаємо корутину перезарядки
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    /// <summary>
    /// Корутина, яка виконує затримку перезарядки.
    /// </summary>
    IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        reloadCoroutine = null; // Очищуємо посилання після завершення
        Debug.Log($"{weaponDisplayName} reloaded! Current ammo: {currentAmmo}");

        // Відправляємо події завершення перезарядки
        Events.Trigger(new WeaponReloadCompletedEvent(weaponDisplayName, currentAmmo));
        Events.Trigger(new AmmoChangedEvent(weaponDisplayName, currentAmmo, magazineSize, reloadCharges));
    }

    /// <summary>
    /// Валідація параметрів в Unity Editor
    /// </summary>
    void OnValidate()
    {
        // Валідуємо зброю
        fireRate = Mathf.Max(0.1f, fireRate);
        bulletForce = Mathf.Max(1f, bulletForce);
        bulletSpread = Mathf.Max(0f, bulletSpread);
        maxAimDistance = Mathf.Max(1f, maxAimDistance);

        // Валідуємо магазин та перезарядку
        magazineSize = ValidationHelper.ValidateMagazineSize(magazineSize);
        reloadTime = Mathf.Max(0.1f, reloadTime);
        reloadCharges = Mathf.Max(0, reloadCharges);

        // Валідуємо ADS
        aimSpeed = Mathf.Max(0.1f, aimSpeed);
        aimFOV = Mathf.Clamp(aimFOV, 10f, 120f);
        aimSpreadMultiplier = Mathf.Max(0f, aimSpreadMultiplier);

        // Валідуємо віддачу
        recoilAmount = Mathf.Max(0f, recoilAmount);
        recoilSpeed = Mathf.Max(0.1f, recoilSpeed);
        recoilReturnSpeed = Mathf.Max(0.1f, recoilReturnSpeed);

        // Заміняємо магічні числа константами
        if (maxAimDistance == 500f)
            maxAimDistance = GameConstants.DEFAULT_MAX_AIM_DISTANCE;
    }

    // ================================
    // МЕТОДИ ДЛЯ COMMAND PATTERN
    // ================================

    /// <summary>
    /// Перевіряє, чи можна стріляти (для Command Pattern)
    /// </summary>
    public bool CanFire()
    {
        return Time.time >= nextFireTime && currentAmmo > 0 && !isReloading;
    }

    /// <summary>
    /// Виконує постріл (для Command Pattern)
    /// </summary>
    public void Fire()
    {
        if (CanFire())
        {
            Shoot();
        }
    }

    /// <summary>
    /// Перевіряє, чи можна перезаряджати (для Command Pattern)
    /// </summary>
    public bool CanReload()
    {
        return currentAmmo < magazineSize && !isReloading && reloadCharges > 0;
    }

    /// <summary>
    /// Починає перезарядку (для Command Pattern)
    /// </summary>
    public void StartReload()
    {
        if (CanReload())
        {
            Reload();
        }
    }

    /// <summary>
    /// Починає прицілювання (для Command Pattern)
    /// </summary>
    public void StartAiming()
    {
        if (!isAiming && allowADS)
        {
            isAiming = true;
            Debug.Log($"{weaponDisplayName}: Started aiming");
        }
    }

    /// <summary>
    /// Зупиняє прицілювання (для Command Pattern)
    /// </summary>
    public void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;
            Debug.Log($"{weaponDisplayName}: Stopped aiming");
        }
    }

    /// <summary>
    /// Перемикає режим прицілювання (для Command Pattern)
    /// </summary>
    public void ToggleAiming()
    {
        if (isAiming)
        {
            StopAiming();
        }
        else
        {
            StartAiming();
        }
    }

    /// <summary>
    /// Застосовує віддачу до огляду гравця.
    /// </summary>
    void ApplyRecoil()
    {
        if (mouseLook == null) return;

        float actualRecoilX = recoilX;
        float actualRecoilY = recoilY;
        float actualRecoilZ = recoilZ;

        if (isAiming)
        {
            actualRecoilX *= aimRecoilMultiplier;
            actualRecoilY *= aimRecoilMultiplier;
            actualRecoilZ *= aimRecoilMultiplier;
        }

        targetRecoilRotation += new Vector3(-actualRecoilX, Random.Range(-actualRecoilY, actualRecoilY), Random.Range(-actualRecoilZ, actualRecoilZ));
    }

    /// <summary>
    /// Оновлює стан віддачі, плавно переміщуючи камеру до цільового обертання.
    /// </summary>
    void UpdateRecoil()
    {
        if (mouseLook == null) return;

        // Зберігаємо попередню віддачу для обчислення різниці
        Vector3 previousRecoil = currentRecoilRotation;
        
        currentRecoilRotation = Vector3.Lerp(currentRecoilRotation, targetRecoilRotation, Time.deltaTime * recoilSnappiness);
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
        
        // SCOUT ВИПРАВЛЕННЯ: Застосовуємо різницю віддачі до MouseLook
        Vector3 recoilDelta = currentRecoilRotation - previousRecoil;
        if (recoilDelta.magnitude > 0.001f) // Уникаємо мікроскопічних змін
        {
            mouseLook.ApplyRecoil(recoilDelta.x, recoilDelta.y);
        }
    }

    /// <summary>
    /// Генерує віддачу при пострілі (SCOUT ВИПРАВЛЕННЯ)
    /// </summary>
    void ApplyRecoil()
    {
        // Обчислюємо силу віддачі з урахуванням прицілювання
        float recoilMultiplier = isAiming ? aimRecoilMultiplier : 1f;
        
        // Додаємо випадковість до віддачі
        float randomRecoilX = recoilX * recoilMultiplier * Random.Range(0.8f, 1.2f);
        float randomRecoilY = recoilY * recoilMultiplier * Random.Range(-1f, 1f);
        
        // Додаємо віддачу до цільового значення
        targetRecoilRotation += new Vector3(randomRecoilX, randomRecoilY, 0f);
        
        Debug.Log($"WeaponController: Віддача застосована - X:{randomRecoilX:F2}, Y:{randomRecoilY:F2}");
    }

    /// <summary>
    /// Перевіряє влучання та показує damage numbers (Claude покращення)
    /// </summary>
    void CheckForHitAndShowDamage(Vector3 shootDirection)
    {
        if (mainCamera == null) return;
        
        // Виконуємо raycast для виявлення влучань
        Ray ray = new Ray(bulletSpawnPoint.position, shootDirection);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxAimDistance))
        {
            // Перевіряємо, чи влучили у ворога
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Обчислюємо урон
                float baseDamage = GetWeaponDamage();
                bool isHeadshot = IsHeadshot(hit);
                float finalDamage = isHeadshot ? baseDamage * 2f : baseDamage;
                
                // Показуємо damage number
                if (DamageNumbersManager.Instance != null)
                {
                    Vector3 damagePosition = hit.point + Vector3.up * 0.5f;
                    DamageType damageType = enemy.HasArmor() ? DamageType.Armor : DamageType.Normal;
                    DamageNumbersManager.Instance.ShowDamageNumber(damagePosition, finalDamage, isHeadshot, damageType);
                }
                
                // Застосовуємо урон до ворога
                enemy.TakeDamage(finalDamage, hit.point, hit.normal);
                
                Debug.Log($"WeaponController: Влучання! Урон: {finalDamage}, Headshot: {isHeadshot}");
            }
            else
            {
                // Влучили в оточення - можна додати ефекти пострілу
                CreateHitEffect(hit.point, hit.normal);
            }
        }
    }
    
    /// <summary>
    /// Визначає, чи є влучання хедшотом
    /// </summary>
    bool IsHeadshot(RaycastHit hit)
    {
        // Перевіряємо тег або назву колайдера
        return hit.collider.CompareTag("Head") || hit.collider.name.ToLower().Contains("head");
    }
    
    /// <summary>
    /// Повертає урон зброї (можна розширити для різних типів зброї)
    /// </summary>
    float GetWeaponDamage()
    {
        // Базовий урон залежно від типу зброї
        // Можна розширити через ScriptableObject конфігурації
        return 25f; // Базовий урон
    }
    
    /// <summary>
    /// Створює ефект влучання в оточення
    /// </summary>
    void CreateHitEffect(Vector3 position, Vector3 normal)
    {
        // Тут можна додати ефекти іскор, пилу тощо
        // Поки що просто логуємо
        Debug.Log($"WeaponController: Влучання в оточення на позиції {position}");
    }
    
    /// <summary>
    /// Повертає зброю та FOV камери до стандартного стану.
    /// </summary>
    void ResetADS()
    {
        isAiming = false;
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
    }


    // --- Публічні методи для UI або інших скриптів ---
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMagazineSize()
    {
        return magazineSize;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public Vector3 GetRecoilRotation()
    {
        return currentRecoilRotation;
    }

    // НОВІ ПУБЛІЧНІ МЕТОДИ ДЛЯ ПІДБОРУ/ВИКИДАННЯ
    public bool IsDroppable()
    {
        return isDroppable;
    }

    public GameObject GetWeaponWorldPrefab()
    {
        return weaponWorldPrefab;
    }

    public string GetWeaponDisplayName()
    {
        return weaponDisplayName;
    }

    public Sprite GetWeaponIcon()
    {
        return weaponIcon;
    }

    // НОВІ ПУБЛІЧНІ МЕТОДИ ДЛЯ СИСТЕМИ ПЕРЕЗАРЯДКИ
    public int GetReloadCharges()
    {
        return reloadCharges;
    }

    public void AddReloadCharge()
    {
        reloadCharges++;
        Debug.Log($"Reload charge added to '{weaponDisplayName}'. Total charges: {reloadCharges}");
    }


    // Методи, що викликаються WeaponSwitching при екіпіровці/знятті
    public void OnEquip()
    {
        // Скидаємо всі стани при екіпіровці
        // currentAmmo = magazineSize; // Замість цього, патрони будуть зберігатися між екіпіровками.
                                     // Якщо ви хочете повний магазин при кожній екіпіровці, розкоментуйте.
        isReloading = false;
        ResetADS(); // Виходимо з ADS
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        StopAllCoroutines(); // Зупиняємо будь-які корутини (наприклад, перезарядку)
        
        Debug.Log($"{weaponDisplayName} Equipped.");
    }

    public void OnUnequip()
    {
        ResetADS(); // Виходимо з ADS
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        StopAllCoroutines(); // Зупиняємо будь-які поточні корутини (наприклад, перезарядку)
        
        Debug.Log($"{weaponDisplayName} Unequipped.");
    }
}