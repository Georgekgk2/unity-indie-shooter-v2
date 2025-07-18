using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// РОЗШИРЕНИЙ ПАКЕТ ВОРОГІВ - ELITE EXPANSION
/// Включає 5 нових типів ворогів з унікальними здібностями та тактиками
/// Інтегрується з існуючою EnemySystem та підтримує всі базові механіки
/// </summary>

// ================================
// НОВІ ТИПИ ВОРОГІВ
// ================================

public enum NewEnemyType
{
    EliteSniper,      // Снайпер з лазерним прицілом
    HeavyGunner,      // Бронований з мініганом
    StealthAssassin,  // Невидимий асасин
    ShieldBearer,     // Носій енергетичного щита
    DroneController   // Контролер дронів
}

// ================================
// ВОРОГ 1: ELITE SNIPER
// ================================

[CreateAssetMenu(fileName = "EliteSniper_Config", menuName = "Game/Enemies/Elite/Sniper")]
public class EliteSniperConfig : EnemyConfiguration
{
    [Header("Sniper Specific Settings")]
    [Tooltip("Дальність снайперського пострілу")]
    public float sniperRange = 50f;
    [Tooltip("Час прицілювання")]
    public float aimingTime = 2.5f;
    [Tooltip("Урон снайперського пострілу")]
    public float sniperDamage = 80f;
    [Tooltip("Точність пострілу")]
    public float accuracy = 0.95f;
    
    [Header("Laser Sight")]
    [Tooltip("Використовувати лазерний приціл")]
    public bool useLaserSight = true;
    [Tooltip("Колір лазера")]
    public Color laserColor = Color.red;
    [Tooltip("Товщина лазерного променя")]
    public float laserWidth = 0.1f;
}

public class EliteSniper : EnemyBase
{
    [Header("Sniper Components")]
    public LineRenderer laserSight;
    public Transform sniperRifle;
    public ParticleSystem muzzleFlash;
    public AudioSource sniperShotSound;
    
    [Header("Sniper Settings")]
    public float sniperRange = 50f;
    public float aimingTime = 2.5f;
    public float sniperDamage = 80f;
    public LayerMask obstacleLayer;
    public LayerMask playerLayer;
    
    private bool isAiming = false;
    private bool hasLineOfSight = false;
    private float aimingTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    
    protected override void Start()
    {
        base.Start();
        SetupLaserSight();
        enemyType = EnemyType.Sniper; // Розширюємо існуючий enum
    }
    
    void SetupLaserSight()
    {
        if (laserSight == null)
        {
            GameObject laserObject = new GameObject("Laser Sight");
            laserObject.transform.SetParent(sniperRifle);
            laserSight = laserObject.AddComponent<LineRenderer>();
        }
        
        laserSight.material = new Material(Shader.Find("Sprites/Default"));
        laserSight.color = Color.red;
        laserSight.startWidth = 0.05f;
        laserSight.endWidth = 0.02f;
        laserSight.enabled = false;
    }
    
    protected override void UpdateBehavior()
    {
        base.UpdateBehavior();
        
        if (currentState == EnemyState.Attacking)
        {
            HandleSniperBehavior();
        }
    }
    
    void HandleSniperBehavior()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= sniperRange)
        {
            CheckLineOfSight();
            
            if (hasLineOfSight)
            {
                StartAiming();
            }
            else
            {
                StopAiming();
                SearchForTarget();
            }
        }
        else
        {
            StopAiming();
            MoveToOptimalPosition();
        }
    }
    
    void CheckLineOfSight()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, directionToTarget, out hit, sniperRange, obstacleLayer | playerLayer))
        {
            hasLineOfSight = hit.collider.CompareTag("Player");
        }
        else
        {
            hasLineOfSight = false;
        }
    }
    
    void StartAiming()
    {
        if (!isAiming)
        {
            isAiming = true;
            aimingTimer = 0f;
            EnableLaserSight();
        }
        
        // Поворот до цілі
        Vector3 lookDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Оновлення лазерного прицілу
        UpdateLaserSight();
        
        aimingTimer += Time.deltaTime;
        
        if (aimingTimer >= aimingTime)
        {
            FireSniperShot();
            isAiming = false;
            aimingTimer = 0f;
            DisableLaserSight();
        }
    }
    
    void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;
            aimingTimer = 0f;
            DisableLaserSight();
        }
    }
    
    void EnableLaserSight()
    {
        if (laserSight != null)
        {
            laserSight.enabled = true;
        }
    }
    
    void DisableLaserSight()
    {
        if (laserSight != null)
        {
            laserSight.enabled = false;
        }
    }
    
    void UpdateLaserSight()
    {
        if (laserSight != null && target != null)
        {
            laserSight.SetPosition(0, sniperRifle.position);
            laserSight.SetPosition(1, target.position);
        }
    }
    
    void FireSniperShot()
    {
        if (target == null) return;
        
        // Звуковий ефект
        if (sniperShotSound != null)
        {
            sniperShotSound.Play();
        }
        
        // Візуальний ефект
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        
        // Перевірка попадання
        RaycastHit hit;
        Vector3 shootDirection = (target.position - sniperRifle.position).normalized;
        
        if (Physics.Raycast(sniperRifle.position, shootDirection, out hit, sniperRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Завдання урону гравцю
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(sniperDamage, DamageType.Bullet);
                }
                
                // Ефект попадання
                CreateImpactEffect(hit.point);
            }
        }
        
        // Пауза після пострілу
        StartCoroutine(PostShotCooldown());
    }
    
    IEnumerator PostShotCooldown()
    {
        yield return new WaitForSeconds(3f); // Пауза між пострілами
        
        // Можливість змінити позицію після пострілу
        if (Random.value < 0.3f)
        {
            ChangePosition();
        }
    }
    
    void MoveToOptimalPosition()
    {
        // Пошук оптимальної позиції для снайпінгу
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 optimalPosition = target.position - directionToTarget * (sniperRange * 0.8f);
        
        navMeshAgent.SetDestination(optimalPosition);
    }
    
    void SearchForTarget()
    {
        // Пошук цілі, якщо втратили лінію зору
        if (lastKnownPlayerPosition != Vector3.zero)
        {
            navMeshAgent.SetDestination(lastKnownPlayerPosition);
        }
    }
    
    void ChangePosition()
    {
        // Зміна позиції після пострілу для уникнення контратаки
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection.y = 0;
        Vector3 newPosition = transform.position + randomDirection;
        
        navMeshAgent.SetDestination(newPosition);
    }
    
    void CreateImpactEffect(Vector3 position)
    {
        // Створення ефекту попадання
        GameObject impactEffect = Instantiate(bulletImpactEffect, position, Quaternion.identity);
        Destroy(impactEffect, 2f);
    }
    
    public override void OnPlayerSpotted(Transform playerTransform)
    {
        base.OnPlayerSpotted(playerTransform);
        lastKnownPlayerPosition = playerTransform.position;
    }
}

// ================================
// ВОРОГ 2: HEAVY GUNNER
// ================================

[CreateAssetMenu(fileName = "HeavyGunner_Config", menuName = "Game/Enemies/Elite/Heavy Gunner")]
public class HeavyGunnerConfig : EnemyConfiguration
{
    [Header("Heavy Gunner Settings")]
    [Tooltip("Здоров'я важкого стрільця")]
    public float heavyHealth = 300f;
    [Tooltip("Швидкість стрільби мініганом")]
    public float minigunFireRate = 0.1f;
    [Tooltip("Час розкручування мініганом")]
    public float spinUpTime = 2f;
    [Tooltip("Час перегріву")]
    public float overheatingTime = 10f;
    [Tooltip("Час охолодження")]
    public float cooldownTime = 5f;
}

public class HeavyGunner : EnemyBase
{
    [Header("Heavy Gunner Components")]
    public Transform minigun;
    public ParticleSystem minigunMuzzleFlash;
    public AudioSource minigunSound;
    public AudioSource spinUpSound;
    public GameObject armorPlating;
    
    [Header("Heavy Gunner Settings")]
    public float minigunFireRate = 0.1f;
    public float spinUpTime = 2f;
    public float overheatingTime = 10f;
    public float cooldownTime = 5f;
    public float minigunDamage = 25f;
    public float armorReduction = 0.5f;
    
    private bool isFiring = false;
    private bool isSpinningUp = false;
    private bool isOverheated = false;
    private float fireTimer = 0f;
    private float spinUpTimer = 0f;
    private float overheatingTimer = 0f;
    private float cooldownTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        health = 300f; // Більше здоров'я
        enemyType = EnemyType.Heavy;
        SetupArmor();
    }
    
    void SetupArmor()
    {
        // Налаштування броні
        if (armorPlating != null)
        {
            armorPlating.SetActive(true);
        }
    }
    
    protected override void UpdateBehavior()
    {
        base.UpdateBehavior();
        
        if (currentState == EnemyState.Attacking)
        {
            HandleHeavyGunnerBehavior();
        }
        
        UpdateMinigunState();
    }
    
    void HandleHeavyGunnerBehavior()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= attackRange)
        {
            // Поворот до цілі
            Vector3 lookDirection = (target.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, 
                Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
            
            // Початок стрільби
            if (!isOverheated && !isSpinningUp)
            {
                StartFiring();
            }
        }
        else
        {
            StopFiring();
            // Наближення до цілі
            navMeshAgent.SetDestination(target.position);
        }
    }
    
    void UpdateMinigunState()
    {
        // Обробка стану мініганом
        if (isSpinningUp)
        {
            spinUpTimer += Time.deltaTime;
            if (spinUpTimer >= spinUpTime)
            {
                isSpinningUp = false;
                isFiring = true;
                spinUpTimer = 0f;
                
                if (minigunSound != null)
                {
                    minigunSound.Play();
                }
            }
        }
        
        if (isFiring && !isOverheated)
        {
            overheatingTimer += Time.deltaTime;
            
            // Стрільба
            fireTimer += Time.deltaTime;
            if (fireTimer >= minigunFireRate)
            {
                FireMinigunBullet();
                fireTimer = 0f;
            }
            
            // Перевірка перегріву
            if (overheatingTimer >= overheatingTime)
            {
                StartOverheating();
            }
        }
        
        if (isOverheated)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldownTime)
            {
                StopOverheating();
            }
        }
    }
    
    void StartFiring()
    {
        if (!isFiring && !isSpinningUp && !isOverheated)
        {
            isSpinningUp = true;
            spinUpTimer = 0f;
            
            if (spinUpSound != null)
            {
                spinUpSound.Play();
            }
        }
    }
    
    void StopFiring()
    {
        if (isFiring || isSpinningUp)
        {
            isFiring = false;
            isSpinningUp = false;
            spinUpTimer = 0f;
            
            if (minigunSound != null)
            {
                minigunSound.Stop();
            }
            
            if (spinUpSound != null)
            {
                spinUpSound.Stop();
            }
        }
    }
    
    void FireMinigunBullet()
    {
        if (target == null) return;
        
        // Візуальний ефект
        if (minigunMuzzleFlash != null)
        {
            minigunMuzzleFlash.Play();
        }
        
        // Створення кулі
        Vector3 shootDirection = (target.position - minigun.position).normalized;
        
        // Додавання невеликого розкиду
        shootDirection += Random.insideUnitSphere * 0.1f;
        shootDirection.Normalize();
        
        RaycastHit hit;
        if (Physics.Raycast(minigun.position, shootDirection, out hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(minigunDamage, DamageType.Bullet);
                }
            }
            
            // Ефект попадання
            CreateImpactEffect(hit.point);
        }
    }
    
    void StartOverheating()
    {
        isOverheated = true;
        isFiring = false;
        overheatingTimer = 0f;
        cooldownTimer = 0f;
        
        if (minigunSound != null)
        {
            minigunSound.Stop();
        }
        
        // Ефект перегріву
        CreateOverheatEffect();
    }
    
    void StopOverheating()
    {
        isOverheated = false;
        cooldownTimer = 0f;
    }
    
    void CreateOverheatEffect()
    {
        // Створення ефекту перегріву (пар, іскри тощо)
        GameObject overheatEffect = new GameObject("Overheat Effect");
        overheatEffect.transform.position = minigun.position;
        
        ParticleSystem steam = overheatEffect.AddComponent<ParticleSystem>();
        var main = steam.main;
        main.startColor = Color.white;
        main.startLifetime = 2f;
        main.startSpeed = 5f;
        
        Destroy(overheatEffect, 3f);
    }
    
    void CreateImpactEffect(Vector3 position)
    {
        GameObject impactEffect = Instantiate(bulletImpactEffect, position, Quaternion.identity);
        Destroy(impactEffect, 1f);
    }
    
    public override void TakeDamage(float damage, DamageType damageType)
    {
        // Зменшення урону завдяки броні
        float reducedDamage = damage * (1f - armorReduction);
        base.TakeDamage(reducedDamage, damageType);
    }
    
    protected override void Die()
    {
        // Вибух при смерті
        CreateDeathExplosion();
        base.Die();
    }
    
    void CreateDeathExplosion()
    {
        // Створення вибуху при смерті
        GameObject explosion = new GameObject("Heavy Gunner Explosion");
        explosion.transform.position = transform.position;
        
        // Додавання ефектів вибуху
        ParticleSystem explosionEffect = explosion.AddComponent<ParticleSystem>();
        var main = explosionEffect.main;
        main.startColor = Color.orange;
        main.startLifetime = 1f;
        main.startSpeed = 10f;
        
        // Урон від вибуху
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 5f);
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(50f, DamageType.Explosion);
                }
            }
        }
        
        Destroy(explosion, 3f);
    }
}

// ================================
// ВОРОГ 3: STEALTH ASSASSIN
// ================================

[CreateAssetMenu(fileName = "StealthAssassin_Config", menuName = "Game/Enemies/Elite/Stealth Assassin")]
public class StealthAssassinConfig : EnemyConfiguration
{
    [Header("Stealth Settings")]
    [Tooltip("Тривалість невидимості")]
    public float stealthDuration = 5f;
    [Tooltip("Час перезарядки невидимості")]
    public float stealthCooldown = 15f;
    [Tooltip("Швидкість в режимі стелс")]
    public float stealthSpeed = 8f;
    [Tooltip("Урон атаки з засідки")]
    public float backstabDamage = 120f;
}

public class StealthAssassin : EnemyBase
{
    [Header("Stealth Components")]
    public Renderer[] renderers;
    public ParticleSystem stealthActivationEffect;
    public ParticleSystem stealthDeactivationEffect;
    public AudioSource stealthSound;
    
    [Header("Stealth Settings")]
    public float stealthDuration = 5f;
    public float stealthCooldown = 15f;
    public float stealthSpeed = 8f;
    public float backstabDamage = 120f;
    public float detectionRadius = 3f;
    
    private bool isStealthed = false;
    private bool canUseSteath = true;
    private float stealthTimer = 0f;
    private float cooldownTimer = 0f;
    private float originalSpeed;
    private Material[] originalMaterials;
    private Material stealthMaterial;
    
    protected override void Start()
    {
        base.Start();
        originalSpeed = navMeshAgent.speed;
        enemyType = EnemyType.Assassin;
        SetupStealthMaterials();
    }
    
    void SetupStealthMaterials()
    {
        // Збереження оригінальних матеріалів
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].material;
        }
        
        // Створення матеріалу для невидимості
        stealthMaterial = new Material(Shader.Find("Standard"));
        stealthMaterial.SetFloat("_Mode", 3); // Transparent mode
        stealthMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        stealthMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        stealthMaterial.SetInt("_ZWrite", 0);
        stealthMaterial.DisableKeyword("_ALPHATEST_ON");
        stealthMaterial.EnableKeyword("_ALPHABLEND_ON");
        stealthMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        stealthMaterial.renderQueue = 3000;
        stealthMaterial.color = new Color(1f, 1f, 1f, 0.2f);
    }
    
    protected override void UpdateBehavior()
    {
        base.UpdateBehavior();
        
        UpdateStealthState();
        
        if (currentState == EnemyState.Attacking)
        {
            HandleAssassinBehavior();
        }
    }
    
    void UpdateStealthState()
    {
        if (isStealthed)
        {
            stealthTimer += Time.deltaTime;
            if (stealthTimer >= stealthDuration)
            {
                DeactivateStealth();
            }
        }
        
        if (!canUseSteath)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= stealthCooldown)
            {
                canUseSteath = true;
                cooldownTimer = 0f;
            }
        }
    }
    
    void HandleAssassinBehavior()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Активація стелс при наближенні
        if (distanceToTarget <= 15f && canUseSteath && !isStealthed)
        {
            ActivateStealth();
        }
        
        if (distanceToTarget <= detectionRadius)
        {
            // Атака з близької відстані
            PerformBackstabAttack();
        }
        else
        {
            // Наближення до цілі
            navMeshAgent.SetDestination(target.position);
        }
    }
    
    void ActivateStealth()
    {
        if (!canUseSteath) return;
        
        isStealthed = true;
        canUseSteath = false;
        stealthTimer = 0f;
        
        // Збільшення швидкості
        navMeshAgent.speed = stealthSpeed;
        
        // Зміна матеріалів для невидимості
        foreach (var renderer in renderers)
        {
            renderer.material = stealthMaterial;
        }
        
        // Ефекти
        if (stealthActivationEffect != null)
        {
            stealthActivationEffect.Play();
        }
        
        if (stealthSound != null)
        {
            stealthSound.Play();
        }
        
        // Зменшення радіусу виявлення ворогами
        GetComponent<Collider>().enabled = false;
    }
    
    void DeactivateStealth()
    {
        if (!isStealthed) return;
        
        isStealthed = false;
        stealthTimer = 0f;
        
        // Відновлення швидкості
        navMeshAgent.speed = originalSpeed;
        
        // Відновлення оригінальних матеріалів
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material = originalMaterials[i];
        }
        
        // Ефекти
        if (stealthDeactivationEffect != null)
        {
            stealthDeactivationEffect.Play();
        }
        
        // Відновлення колайдера
        GetComponent<Collider>().enabled = true;
    }
    
    void PerformBackstabAttack()
    {
        if (target == null) return;
        
        // Деактивація стелс при атаці
        if (isStealthed)
        {
            DeactivateStealth();
        }
        
        // Поворот до цілі
        Vector3 lookDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Атака
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float damage = isStealthed ? backstabDamage : attackDamage;
            playerHealth.TakeDamage(damage, DamageType.Melee);
        }
        
        // Ефект атаки
        CreateAttackEffect();
        
        // Пауза після атаки
        StartCoroutine(PostAttackCooldown());
    }
    
    void CreateAttackEffect()
    {
        // Створення ефекту атаки
        GameObject attackEffect = new GameObject("Assassin Attack Effect");
        attackEffect.transform.position = transform.position;
        
        ParticleSystem slashEffect = attackEffect.AddComponent<ParticleSystem>();
        var main = slashEffect.main;
        main.startColor = Color.red;
        main.startLifetime = 0.5f;
        main.startSpeed = 15f;
        
        Destroy(attackEffect, 1f);
    }
    
    IEnumerator PostAttackCooldown()
    {
        yield return new WaitForSeconds(2f);
        
        // Можливість відступу після атаки
        if (Random.value < 0.4f)
        {
            RetreatFromTarget();
        }
    }
    
    void RetreatFromTarget()
    {
        if (target == null) return;
        
        // Відступ від цілі
        Vector3 retreatDirection = (transform.position - target.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * 10f;
        
        navMeshAgent.SetDestination(retreatPosition);
    }
    
    public override void OnPlayerSpotted(Transform playerTransform)
    {
        base.OnPlayerSpotted(playerTransform);
        
        // Можливість активації стелс при виявленні
        if (Random.value < 0.6f && canUseSteath)
        {
            ActivateStealth();
        }
    }
}