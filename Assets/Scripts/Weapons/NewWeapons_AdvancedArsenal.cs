using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// РОЗШИРЕНИЙ АРСЕНАЛ ЗБРОЇ - ADVANCED ARSENAL
/// Включає 7 нових типів зброї з унікальними механіками та ефектами
/// Інтегрується з існуючою WeaponController системою
/// </summary>

// ================================
// НОВІ ТИПИ ЗБРОЇ
// ================================

public enum NewWeaponType
{
    PlasmaRifle,      // Енергетична зброя з перегрівом
    RocketLauncher,   // Зброя зонального ураження
    Railgun,          // Пробивна зброя з зарядкою
    Flamethrower,     // Зброя ближнього бою з DOT
    EMPGrenade,       // Тактична зброя проти електроніки
    LaserCannon,      // Точна енергетична зброя
    GravityGun        // Фізичні маніпуляції
}

// ================================
// ЗБРОЯ 1: PLASMA RIFLE
// ================================

[CreateAssetMenu(fileName = "PlasmaRifle_Config", menuName = "Game/Weapons/Energy/Plasma Rifle")]
public class PlasmaRifleConfig : WeaponConfiguration
{
    [Header("Plasma Rifle Settings")]
    [Tooltip("Урон плазмового пострілу")]
    public float plasmaDamage = 45f;
    [Tooltip("Швидкість плазмового снаряда")]
    public float plasmaSpeed = 30f;
    [Tooltip("Максимальна температура")]
    public float maxHeat = 100f;
    [Tooltip("Нагрівання за постріл")]
    public float heatPerShot = 8f;
    [Tooltip("Швидкість охолодження")]
    public float coolingRate = 15f;
    [Tooltip("Температура перегріву")]
    public float overheatingThreshold = 90f;
}

public class PlasmaRifle : WeaponBase
{
    [Header("Plasma Components")]
    public GameObject plasmaProjectilePrefab;
    public ParticleSystem plasmaChargeEffect;
    public ParticleSystem overheatingEffect;
    public AudioSource plasmaFireSound;
    public AudioSource overheatingSound;
    public Transform plasmaBarrel;
    
    [Header("Plasma Settings")]
    public float plasmaDamage = 45f;
    public float plasmaSpeed = 30f;
    public float maxHeat = 100f;
    public float heatPerShot = 8f;
    public float coolingRate = 15f;
    public float overheatingThreshold = 90f;
    
    private float currentHeat = 0f;
    private bool isOverheated = false;
    private bool isCooling = false;
    
    protected override void Start()
    {
        base.Start();
        weaponType = WeaponType.Energy; // Розширюємо існуючий enum
    }
    
    protected override void Update()
    {
        base.Update();
        UpdateHeatSystem();
    }
    
    void UpdateHeatSystem()
    {
        // Охолодження зброї
        if (currentHeat > 0f && !isFiring)
        {
            currentHeat -= coolingRate * Time.deltaTime;
            currentHeat = Mathf.Max(0f, currentHeat);
            
            if (currentHeat < overheatingThreshold * 0.5f && isOverheated)
            {
                StopOverheating();
            }
        }
        
        // Перевірка перегріву
        if (currentHeat >= maxHeat && !isOverheated)
        {
            StartOverheating();
        }
        
        // Оновлення візуальних ефектів
        UpdateHeatEffects();
    }
    
    public override void Fire()
    {
        if (isOverheated || currentAmmo <= 0) return;
        
        base.Fire();
        
        // Додавання тепла
        currentHeat += heatPerShot;
        currentHeat = Mathf.Min(maxHeat, currentHeat);
        
        // Створення плазмового снаряда
        CreatePlasmaProjectile();
        
        // Ефекти
        PlayFireEffects();
    }
    
    void CreatePlasmaProjectile()
    {
        if (plasmaProjectilePrefab == null) return;
        
        GameObject projectile = Instantiate(plasmaProjectilePrefab, plasmaBarrel.position, plasmaBarrel.rotation);
        PlasmaProjectile plasmaScript = projectile.GetComponent<PlasmaProjectile>();
        
        if (plasmaScript != null)
        {
            plasmaScript.damage = plasmaDamage;
            plasmaScript.speed = plasmaSpeed;
            plasmaScript.Initialize();
        }
    }
    
    void PlayFireEffects()
    {
        // Звуковий ефект
        if (plasmaFireSound != null)
        {
            plasmaFireSound.pitch = Random.Range(0.9f, 1.1f);
            plasmaFireSound.Play();
        }
        
        // Ефект зарядки
        if (plasmaChargeEffect != null)
        {
            plasmaChargeEffect.Play();
        }
    }
    
    void StartOverheating()
    {
        isOverheated = true;
        isCooling = true;
        
        // Ефекти перегріву
        if (overheatingEffect != null)
        {
            overheatingEffect.Play();
        }
        
        if (overheatingSound != null)
        {
            overheatingSound.Play();
        }
        
        // Повідомлення гравцю
        ShowOverheatingWarning();
    }
    
    void StopOverheating()
    {
        isOverheated = false;
        isCooling = false;
        
        if (overheatingEffect != null)
        {
            overheatingEffect.Stop();
        }
        
        if (overheatingSound != null)
        {
            overheatingSound.Stop();
        }
    }
    
    void UpdateHeatEffects()
    {
        // Зміна кольору залежно від температури
        float heatPercent = currentHeat / maxHeat;
        
        if (plasmaChargeEffect != null)
        {
            var main = plasmaChargeEffect.main;
            main.startColor = Color.Lerp(Color.blue, Color.red, heatPercent);
        }
    }
    
    void ShowOverheatingWarning()
    {
        // Показати попередження про перегрів в UI
        UIManager.Instance?.ShowNotification("ЗБРОЯ ПЕРЕГРІТА! ОХОЛОДЖЕННЯ...", NotificationType.Warning);
    }
    
    public float GetHeatPercentage()
    {
        return currentHeat / maxHeat;
    }
    
    public bool IsOverheated()
    {
        return isOverheated;
    }
}

// Клас для плазмового снаряда
public class PlasmaProjectile : MonoBehaviour
{
    public float damage = 45f;
    public float speed = 30f;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public float explosionRadius = 3f;
    
    private Rigidbody rb;
    private bool hasHit = false;
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (other.CompareTag("Enemy") || other.CompareTag("Player") || other.CompareTag("Environment"))
        {
            hasHit = true;
            
            // Завдання урону
            if (other.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, DamageType.Energy);
                }
            }
            else if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, DamageType.Energy);
                }
            }
            
            // Ефект вибуху
            CreateExplosion();
            
            Destroy(gameObject);
        }
    }
    
    void CreateExplosion()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Зональний урон
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    enemyHealth.TakeDamage(damage * damageMultiplier * 0.5f, DamageType.Energy);
                }
            }
        }
    }
}

// ================================
// ЗБРОЯ 2: ROCKET LAUNCHER
// ================================

[CreateAssetMenu(fileName = "RocketLauncher_Config", menuName = "Game/Weapons/Explosives/Rocket Launcher")]
public class RocketLauncherConfig : WeaponConfiguration
{
    [Header("Rocket Launcher Settings")]
    [Tooltip("Урон ракети")]
    public float rocketDamage = 100f;
    [Tooltip("Радіус вибуху")]
    public float explosionRadius = 8f;
    [Tooltip("Швидкість ракети")]
    public float rocketSpeed = 25f;
    [Tooltip("Час перезарядки")]
    public float reloadTime = 3f;
}

public class RocketLauncher : WeaponBase
{
    [Header("Rocket Components")]
    public GameObject rocketPrefab;
    public Transform rocketSpawnPoint;
    public ParticleSystem launchEffect;
    public AudioSource launchSound;
    public AudioSource reloadSound;
    
    [Header("Rocket Settings")]
    public float rocketDamage = 100f;
    public float explosionRadius = 8f;
    public float rocketSpeed = 25f;
    public float reloadTime = 3f;
    
    private bool isReloading = false;
    
    protected override void Start()
    {
        base.Start();
        weaponType = WeaponType.Explosive;
        maxAmmo = 6; // Мало набоїв
        currentAmmo = maxAmmo;
    }
    
    public override void Fire()
    {
        if (isReloading || currentAmmo <= 0) return;
        
        base.Fire();
        
        // Створення ракети
        CreateRocket();
        
        // Ефекти запуску
        PlayLaunchEffects();
        
        // Автоматична перезарядка при закінченні набоїв
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
    
    void CreateRocket()
    {
        if (rocketPrefab == null) return;
        
        GameObject rocket = Instantiate(rocketPrefab, rocketSpawnPoint.position, rocketSpawnPoint.rotation);
        Rocket rocketScript = rocket.GetComponent<Rocket>();
        
        if (rocketScript != null)
        {
            rocketScript.damage = rocketDamage;
            rocketScript.explosionRadius = explosionRadius;
            rocketScript.speed = rocketSpeed;
            rocketScript.Initialize();
        }
    }
    
    void PlayLaunchEffects()
    {
        if (launchEffect != null)
        {
            launchEffect.Play();
        }
        
        if (launchSound != null)
        {
            launchSound.Play();
        }
        
        // Сильна віддача
        ApplyRecoil(2f);
    }
    
    void StartReload()
    {
        if (isReloading) return;
        
        isReloading = true;
        StartCoroutine(ReloadCoroutine());
    }
    
    IEnumerator ReloadCoroutine()
    {
        if (reloadSound != null)
        {
            reloadSound.Play();
        }
        
        yield return new WaitForSeconds(reloadTime);
        
        currentAmmo = maxAmmo;
        isReloading = false;
        
        UIManager.Instance?.ShowNotification("РАКЕТОМЕТ ПЕРЕЗАРЯДЖЕНО", NotificationType.Info);
    }
    
    void ApplyRecoil(float multiplier)
    {
        // Сильна віддача для ракетомета
        if (Camera.main != null)
        {
            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.AddRecoil(Vector3.up * 5f * multiplier);
            }
        }
    }
    
    public bool IsReloading()
    {
        return isReloading;
    }
}

// Клас для ракети
public class Rocket : MonoBehaviour
{
    public float damage = 100f;
    public float explosionRadius = 8f;
    public float speed = 25f;
    public float lifetime = 10f;
    public GameObject explosionEffect;
    public AudioSource explosionSound;
    
    private Rigidbody rb;
    private bool hasExploded = false;
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        if (!other.CompareTag("Player")) // Не вибухає від гравця
        {
            Explode();
        }
    }
    
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Ефект вибуху
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        if (explosionSound != null)
        {
            explosionSound.Play();
        }
        
        // Зональний урон
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var obj in nearbyObjects)
        {
            float distance = Vector3.Distance(transform.position, obj.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius);
            float finalDamage = damage * damageMultiplier;
            
            if (obj.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(finalDamage, DamageType.Explosion);
                }
            }
            else if (obj.CompareTag("Player"))
            {
                PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(finalDamage * 0.5f, DamageType.Explosion); // Менший урон гравцю
                }
            }
            
            // Фізичний вплив
            Rigidbody objRb = obj.GetComponent<Rigidbody>();
            if (objRb != null)
            {
                Vector3 explosionDirection = (obj.transform.position - transform.position).normalized;
                objRb.AddForce(explosionDirection * (500f * damageMultiplier), ForceMode.Impulse);
            }
        }
        
        Destroy(gameObject, 0.1f);
    }
}

// ================================
// ЗБРОЯ 3: RAILGUN
// ================================

[CreateAssetMenu(fileName = "Railgun_Config", menuName = "Game/Weapons/Energy/Railgun")]
public class RailgunConfig : WeaponConfiguration
{
    [Header("Railgun Settings")]
    [Tooltip("Урон рейлганом")]
    public float railgunDamage = 200f;
    [Tooltip("Час зарядки")]
    public float chargeTime = 2f;
    [Tooltip("Максимальна дальність")]
    public float maxRange = 100f;
    [Tooltip("Кількість пробитих цілей")]
    public int penetrationCount = 5;
}

public class Railgun : WeaponBase
{
    [Header("Railgun Components")]
    public LineRenderer railBeam;
    public ParticleSystem chargeEffect;
    public ParticleSystem fireEffect;
    public AudioSource chargeSound;
    public AudioSource fireSound;
    public Transform railBarrel;
    
    [Header("Railgun Settings")]
    public float railgunDamage = 200f;
    public float chargeTime = 2f;
    public float maxRange = 100f;
    public int penetrationCount = 5;
    
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private bool isCharged = false;
    
    protected override void Start()
    {
        base.Start();
        weaponType = WeaponType.Energy;
        SetupRailBeam();
    }
    
    void SetupRailBeam()
    {
        if (railBeam == null)
        {
            GameObject beamObject = new GameObject("Rail Beam");
            beamObject.transform.SetParent(railBarrel);
            railBeam = beamObject.AddComponent<LineRenderer>();
        }
        
        railBeam.material = new Material(Shader.Find("Sprites/Default"));
        railBeam.color = Color.cyan;
        railBeam.startWidth = 0.1f;
        railBeam.endWidth = 0.05f;
        railBeam.enabled = false;
    }
    
    protected override void Update()
    {
        base.Update();
        UpdateCharging();
    }
    
    void UpdateCharging()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            
            // Оновлення ефекту зарядки
            if (chargeEffect != null)
            {
                var main = chargeEffect.main;
                main.startSpeed = Mathf.Lerp(1f, 10f, chargeTimer / chargeTime);
            }
            
            if (chargeTimer >= chargeTime)
            {
                CompleteCharge();
            }
        }
    }
    
    public override void StartFiring()
    {
        if (currentAmmo <= 0 || isCharging) return;
        
        StartCharge();
    }
    
    public override void StopFiring()
    {
        if (isCharged)
        {
            FireRailgun();
        }
        else if (isCharging)
        {
            CancelCharge();
        }
    }
    
    void StartCharge()
    {
        isCharging = true;
        chargeTimer = 0f;
        
        // Ефекти зарядки
        if (chargeEffect != null)
        {
            chargeEffect.Play();
        }
        
        if (chargeSound != null)
        {
            chargeSound.Play();
        }
    }
    
    void CompleteCharge()
    {
        isCharging = false;
        isCharged = true;
        
        // Зміна ефектів при повній зарядці
        if (chargeEffect != null)
        {
            var main = chargeEffect.main;
            main.startColor = Color.white;
        }
        
        UIManager.Instance?.ShowNotification("РЕЙЛГАН ЗАРЯДЖЕНО", NotificationType.Success);
    }
    
    void CancelCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }
        
        if (chargeSound != null)
        {
            chargeSound.Stop();
        }
    }
    
    void FireRailgun()
    {
        if (!isCharged || currentAmmo <= 0) return;
        
        isCharged = false;
        currentAmmo--;
        
        // Створення променя
        CreateRailBeam();
        
        // Ефекти пострілу
        PlayFireEffects();
        
        // Віддача
        ApplyRecoil(3f);
    }
    
    void CreateRailBeam()
    {
        Vector3 startPoint = railBarrel.position;
        Vector3 direction = railBarrel.forward;
        
        List<Vector3> hitPoints = new List<Vector3>();
        hitPoints.Add(startPoint);
        
        int penetrationsLeft = penetrationCount;
        Vector3 currentPosition = startPoint;
        
        while (penetrationsLeft > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(currentPosition, direction, out hit, maxRange))
            {
                hitPoints.Add(hit.point);
                
                // Завдання урону
                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(railgunDamage, DamageType.Energy);
                    }
                    penetrationsLeft--;
                }
                else if (hit.collider.CompareTag("Environment"))
                {
                    // Зупинка на твердих об'єктах
                    break;
                }
                
                currentPosition = hit.point + direction * 0.1f;
            }
            else
            {
                // Промінь досяг максимальної дальності
                hitPoints.Add(startPoint + direction * maxRange);
                break;
            }
        }
        
        // Відображення променя
        StartCoroutine(ShowRailBeam(hitPoints));
    }
    
    IEnumerator ShowRailBeam(List<Vector3> points)
    {
        railBeam.enabled = true;
        railBeam.positionCount = points.Count;
        
        for (int i = 0; i < points.Count; i++)
        {
            railBeam.SetPosition(i, points[i]);
        }
        
        yield return new WaitForSeconds(0.1f);
        
        railBeam.enabled = false;
    }
    
    void PlayFireEffects()
    {
        if (fireEffect != null)
        {
            fireEffect.Play();
        }
        
        if (fireSound != null)
        {
            fireSound.Play();
        }
        
        if (chargeEffect != null)
        {
            chargeEffect.Stop();
        }
        
        if (chargeSound != null)
        {
            chargeSound.Stop();
        }
    }
    
    void ApplyRecoil(float multiplier)
    {
        if (Camera.main != null)
        {
            CameraController cameraController = Camera.main.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.AddRecoil(Vector3.up * 3f * multiplier);
            }
        }
    }
    
    public bool IsCharging()
    {
        return isCharging;
    }
    
    public bool IsCharged()
    {
        return isCharged;
    }
    
    public float GetChargeProgress()
    {
        return chargeTimer / chargeTime;
    }
}