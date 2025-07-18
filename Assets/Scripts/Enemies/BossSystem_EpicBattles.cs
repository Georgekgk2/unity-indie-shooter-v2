using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// СИСТЕМА БОССІВ - EPIC BATTLES
/// Включає 3 унікальних боса з багатофазними боями та кінематографічними елементами
/// Інтегрується з існуючою EnemySystem та додає нові механіки
/// </summary>

// ================================
// ТИПИ БОССІВ
// ================================

public enum BossType
{
    CyberTank,      // Великий мех з кількома фазами
    AIOverlord,     // Босс що хакає системи гравця
    SwarmQueen      // Босс що викликає міньйонів
}

public enum BossPhase
{
    Phase1,         // Перша фаза
    Phase2,         // Друга фаза
    Phase3,         // Третя фаза (фінальна)
    Defeated        // Переможений
}

// ================================
// БАЗОВИЙ КЛАС БОСА
// ================================

public abstract class BossBase : EnemyBase
{
    [Header("Boss Settings")]
    [Tooltip("Тип боса")]
    public BossType bossType;
    [Tooltip("Максимальне здоров'я боса")]
    public float maxBossHealth = 1000f;
    [Tooltip("Поточна фаза боса")]
    public BossPhase currentPhase = BossPhase.Phase1;
    [Tooltip("Здоров'я для переходу до фази 2")]
    public float phase2HealthThreshold = 0.66f;
    [Tooltip("Здоров'я для переходу до фази 3")]
    public float phase3HealthThreshold = 0.33f;
    
    [Header("Boss UI")]
    public BossHealthBar healthBar;
    public string bossName = "Unknown Boss";
    public Sprite bossIcon;
    
    [Header("Cinematics")]
    public GameObject introCinematic;
    public GameObject defeatCinematic;
    public AudioSource bossMusic;
    public AudioSource phaseTransitionSound;
    
    protected bool hasEnteredPhase2 = false;
    protected bool hasEnteredPhase3 = false;
    protected bool isInCinematic = false;
    protected float currentBossHealth;
    
    protected override void Start()
    {
        base.Start();
        currentBossHealth = maxBossHealth;
        health = maxBossHealth;
        
        SetupBossUI();
        StartIntroCinematic();
    }
    
    protected virtual void SetupBossUI()
    {
        if (healthBar != null)
        {
            healthBar.SetBoss(bossName, bossIcon, maxBossHealth);
            healthBar.Show();
        }
        
        // Запуск босової музики
        if (bossMusic != null)
        {
            bossMusic.Play();
        }
    }
    
    protected virtual void StartIntroCinematic()
    {
        if (introCinematic != null)
        {
            isInCinematic = true;
            introCinematic.SetActive(true);
            StartCoroutine(IntroCinematicCoroutine());
        }
    }
    
    protected virtual IEnumerator IntroCinematicCoroutine()
    {
        // Заморозка гравця під час кінематографа
        PlayerController.Instance?.SetMovementEnabled(false);
        
        yield return new WaitForSeconds(3f); // Тривалість intro
        
        introCinematic.SetActive(false);
        isInCinematic = false;
        
        PlayerController.Instance?.SetMovementEnabled(true);
        
        // Початок бою
        StartBossFight();
    }
    
    protected virtual void StartBossFight()
    {
        currentState = EnemyState.Attacking;
        UIManager.Instance?.ShowNotification($"БІЙ З БОСОМ: {bossName.ToUpper()}", NotificationType.Boss);
    }
    
    public override void TakeDamage(float damage, DamageType damageType)
    {
        if (isInCinematic) return;
        
        base.TakeDamage(damage, damageType);
        currentBossHealth = health;
        
        // Оновлення UI
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentBossHealth);
        }
        
        // Перевірка переходів між фазами
        CheckPhaseTransitions();
    }
    
    protected virtual void CheckPhaseTransitions()
    {
        float healthPercent = currentBossHealth / maxBossHealth;
        
        if (!hasEnteredPhase2 && healthPercent <= phase2HealthThreshold)
        {
            TransitionToPhase2();
        }
        else if (!hasEnteredPhase3 && healthPercent <= phase3HealthThreshold)
        {
            TransitionToPhase3();
        }
    }
    
    protected virtual void TransitionToPhase2()
    {
        hasEnteredPhase2 = true;
        currentPhase = BossPhase.Phase2;
        
        StartCoroutine(PhaseTransitionCoroutine("ФАЗА 2: ПІДВИЩЕНА АГРЕСІЯ"));
        OnPhase2Enter();
    }
    
    protected virtual void TransitionToPhase3()
    {
        hasEnteredPhase3 = true;
        currentPhase = BossPhase.Phase3;
        
        StartCoroutine(PhaseTransitionCoroutine("ФАЗА 3: ОСТАННІЙ ШАНС"));
        OnPhase3Enter();
    }
    
    protected virtual IEnumerator PhaseTransitionCoroutine(string phaseText)
    {
        // Короткочасна пауза та ефекти
        isInCinematic = true;
        
        if (phaseTransitionSound != null)
        {
            phaseTransitionSound.Play();
        }
        
        UIManager.Instance?.ShowNotification(phaseText, NotificationType.Boss);
        
        // Ефект переходу фази
        CreatePhaseTransitionEffect();
        
        yield return new WaitForSeconds(2f);
        
        isInCinematic = false;
    }
    
    protected virtual void CreatePhaseTransitionEffect()
    {
        // Створення ефекту переходу фази
        GameObject effect = new GameObject("Phase Transition Effect");
        effect.transform.position = transform.position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.red;
        main.startLifetime = 2f;
        main.startSpeed = 10f;
        main.maxParticles = 100;
        
        Destroy(effect, 3f);
    }
    
    // Абстрактні методи для кожної фази
    protected abstract void OnPhase2Enter();
    protected abstract void OnPhase3Enter();
    
    protected override void Die()
    {
        currentPhase = BossPhase.Defeated;
        StartDefeatSequence();
    }
    
    protected virtual void StartDefeatSequence()
    {
        StartCoroutine(DefeatSequenceCoroutine());
    }
    
    protected virtual IEnumerator DefeatSequenceCoroutine()
    {
        isInCinematic = true;
        
        // Зупинка музики
        if (bossMusic != null)
        {
            bossMusic.Stop();
        }
        
        // Кінематограф поразки
        if (defeatCinematic != null)
        {
            defeatCinematic.SetActive(true);
        }
        
        // Приховування UI боса
        if (healthBar != null)
        {
            healthBar.Hide();
        }
        
        UIManager.Instance?.ShowNotification($"{bossName.ToUpper()} ПЕРЕМОЖЕНИЙ!", NotificationType.Victory);
        
        yield return new WaitForSeconds(5f);
        
        if (defeatCinematic != null)
        {
            defeatCinematic.SetActive(false);
        }
        
        // Нагороди за перемогу
        GiveRewards();
        
        base.Die();
    }
    
    protected virtual void GiveRewards()
    {
        // Великий досвід
        ExperienceManager.Instance?.AddExperience(500);
        
        // Спеціальні нагороди
        LootManager.Instance?.SpawnBossLoot(transform.position, bossType);
        
        // Досягнення
        AchievementManager.Instance?.UnlockAchievement($"DEFEAT_{bossType.ToString().ToUpper()}");
    }
}

// ================================
// БОСС 1: CYBER TANK
// ================================

[CreateAssetMenu(fileName = "CyberTank_Config", menuName = "Game/Bosses/Cyber Tank")]
public class CyberTankConfig : EnemyConfiguration
{
    [Header("Cyber Tank Settings")]
    [Tooltip("Урон основної гармати")]
    public float mainCannonDamage = 80f;
    [Tooltip("Урон ракетної системи")]
    public float missileDamage = 60f;
    [Tooltip("Урон лазерної системи")]
    public float laserDamage = 40f;
    [Tooltip("Швидкість руху танка")]
    public float tankSpeed = 3f;
}

public class CyberTank : BossBase
{
    [Header("Cyber Tank Components")]
    public Transform mainCannon;
    public Transform[] missileLaunchers;
    public Transform[] laserTurrets;
    public Transform[] weakPoints;
    public ParticleSystem engineExhaust;
    public AudioSource cannonSound;
    public AudioSource missileSound;
    public AudioSource laserSound;
    
    [Header("Cyber Tank Settings")]
    public float mainCannonDamage = 80f;
    public float missileDamage = 60f;
    public float laserDamage = 40f;
    public float cannonFireRate = 2f;
    public float missileFireRate = 1f;
    public float laserFireRate = 0.5f;
    
    private float cannonTimer = 0f;
    private float missileTimer = 0f;
    private float laserTimer = 0f;
    private bool[] weakPointsDestroyed;
    private int destroyedWeakPoints = 0;
    
    protected override void Start()
    {
        base.Start();
        bossType = BossType.CyberTank;
        bossName = "CYBER TANK MK-VII";
        
        // Ініціалізація слабких точок
        weakPointsDestroyed = new bool[weakPoints.Length];
        SetupWeakPoints();
    }
    
    void SetupWeakPoints()
    {
        for (int i = 0; i < weakPoints.Length; i++)
        {
            WeakPoint weakPoint = weakPoints[i].GetComponent<WeakPoint>();
            if (weakPoint == null)
            {
                weakPoint = weakPoints[i].gameObject.AddComponent<WeakPoint>();
            }
            
            weakPoint.onDestroyed += OnWeakPointDestroyed;
            weakPoint.weakPointIndex = i;
        }
    }
    
    void OnWeakPointDestroyed(int index)
    {
        if (!weakPointsDestroyed[index])
        {
            weakPointsDestroyed[index] = true;
            destroyedWeakPoints++;
            
            // Додатковий урон при знищенні слабкої точки
            TakeDamage(maxBossHealth * 0.1f, DamageType.Explosion);
            
            UIManager.Instance?.ShowNotification("СЛАБКА ТОЧКА ЗНИЩЕНА!", NotificationType.Success);
        }
    }
    
    protected override void UpdateBehavior()
    {
        if (isInCinematic) return;
        
        base.UpdateBehavior();
        
        if (currentState == EnemyState.Attacking)
        {
            HandleCyberTankBehavior();
        }
    }
    
    void HandleCyberTankBehavior()
    {
        if (target == null) return;
        
        // Поворот до цілі
        Vector3 lookDirection = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(lookDirection), Time.deltaTime);
        
        // Атаки залежно від фази
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                HandlePhase1Attacks();
                break;
            case BossPhase.Phase2:
                HandlePhase2Attacks();
                break;
            case BossPhase.Phase3:
                HandlePhase3Attacks();
                break;
        }
        
        // Рух танка
        MoveTank();
    }
    
    void HandlePhase1Attacks()
    {
        // Тільки основна гармата
        cannonTimer += Time.deltaTime;
        if (cannonTimer >= cannonFireRate)
        {
            FireMainCannon();
            cannonTimer = 0f;
        }
    }
    
    void HandlePhase2Attacks()
    {
        // Основна гармата + ракети
        cannonTimer += Time.deltaTime;
        missileTimer += Time.deltaTime;
        
        if (cannonTimer >= cannonFireRate * 0.8f) // Швидше
        {
            FireMainCannon();
            cannonTimer = 0f;
        }
        
        if (missileTimer >= missileFireRate)
        {
            FireMissiles();
            missileTimer = 0f;
        }
    }
    
    void HandlePhase3Attacks()
    {
        // Всі системи зброї
        cannonTimer += Time.deltaTime;
        missileTimer += Time.deltaTime;
        laserTimer += Time.deltaTime;
        
        if (cannonTimer >= cannonFireRate * 0.6f) // Ще швидше
        {
            FireMainCannon();
            cannonTimer = 0f;
        }
        
        if (missileTimer >= missileFireRate * 0.7f)
        {
            FireMissiles();
            missileTimer = 0f;
        }
        
        if (laserTimer >= laserFireRate)
        {
            FireLasers();
            laserTimer = 0f;
        }
    }
    
    void FireMainCannon()
    {
        if (mainCannon == null || target == null) return;
        
        // Поворот гармати до цілі
        Vector3 cannonDirection = (target.position - mainCannon.position).normalized;
        mainCannon.rotation = Quaternion.LookRotation(cannonDirection);
        
        // Створення снаряда
        GameObject projectile = CreateProjectile(mainCannon.position, cannonDirection, mainCannonDamage);
        
        // Ефекти
        if (cannonSound != null)
        {
            cannonSound.Play();
        }
        
        CreateMuzzleFlash(mainCannon.position);
    }
    
    void FireMissiles()
    {
        foreach (var launcher in missileLaunchers)
        {
            if (launcher == null) continue;
            
            Vector3 missileDirection = (target.position - launcher.position).normalized;
            
            // Додавання невеликого розкиду
            missileDirection += Random.insideUnitSphere * 0.2f;
            missileDirection.Normalize();
            
            GameObject missile = CreateMissile(launcher.position, missileDirection, missileDamage);
            
            if (missileSound != null)
            {
                missileSound.Play();
            }
        }
    }
    
    void FireLasers()
    {
        foreach (var turret in laserTurrets)
        {
            if (turret == null) continue;
            
            Vector3 laserDirection = (target.position - turret.position).normalized;
            
            // Створення лазерного променя
            StartCoroutine(CreateLaserBeam(turret.position, laserDirection));
        }
        
        if (laserSound != null)
        {
            laserSound.Play();
        }
    }
    
    GameObject CreateProjectile(Vector3 position, Vector3 direction, float damage)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.5f;
        
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.velocity = direction * 20f;
        
        TankProjectile projectileScript = projectile.AddComponent<TankProjectile>();
        projectileScript.damage = damage;
        
        Destroy(projectile, 5f);
        return projectile;
    }
    
    GameObject CreateMissile(Vector3 position, Vector3 direction, float damage)
    {
        GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        missile.transform.position = position;
        missile.transform.rotation = Quaternion.LookRotation(direction);
        missile.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
        
        Rigidbody rb = missile.AddComponent<Rigidbody>();
        rb.velocity = direction * 15f;
        
        TankMissile missileScript = missile.AddComponent<TankMissile>();
        missileScript.damage = damage;
        missileScript.explosionRadius = 4f;
        
        Destroy(missile, 8f);
        return missile;
    }
    
    IEnumerator CreateLaserBeam(Vector3 startPosition, Vector3 direction)
    {
        LineRenderer laser = new GameObject("Laser Beam").AddComponent<LineRenderer>();
        laser.material = new Material(Shader.Find("Sprites/Default"));
        laser.color = Color.red;
        laser.startWidth = 0.2f;
        laser.endWidth = 0.1f;
        
        RaycastHit hit;
        Vector3 endPosition;
        
        if (Physics.Raycast(startPosition, direction, out hit, 50f))
        {
            endPosition = hit.point;
            
            // Завдання урону
            if (hit.collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(laserDamage, DamageType.Energy);
                }
            }
        }
        else
        {
            endPosition = startPosition + direction * 50f;
        }
        
        laser.SetPosition(0, startPosition);
        laser.SetPosition(1, endPosition);
        
        yield return new WaitForSeconds(0.5f);
        
        Destroy(laser.gameObject);
    }
    
    void MoveTank()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Підтримання оптимальної дистанції
        if (distanceToTarget < 15f)
        {
            // Відступ
            Vector3 retreatDirection = (transform.position - target.position).normalized;
            navMeshAgent.SetDestination(transform.position + retreatDirection * 5f);
        }
        else if (distanceToTarget > 25f)
        {
            // Наближення
            navMeshAgent.SetDestination(target.position);
        }
        
        // Ефект двигуна
        if (engineExhaust != null && navMeshAgent.velocity.magnitude > 0.1f)
        {
            if (!engineExhaust.isPlaying)
            {
                engineExhaust.Play();
            }
        }
        else if (engineExhaust != null && engineExhaust.isPlaying)
        {
            engineExhaust.Stop();
        }
    }
    
    void CreateMuzzleFlash(Vector3 position)
    {
        GameObject flash = new GameObject("Muzzle Flash");
        flash.transform.position = position;
        
        ParticleSystem particles = flash.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.yellow;
        main.startLifetime = 0.2f;
        main.startSpeed = 10f;
        
        Destroy(flash, 1f);
    }
    
    protected override void OnPhase2Enter()
    {
        UIManager.Instance?.ShowNotification("CYBER TANK АКТИВУВАВ РАКЕТНІ СИСТЕМИ!", NotificationType.Warning);
        
        // Збільшення швидкості
        navMeshAgent.speed *= 1.2f;
    }
    
    protected override void OnPhase3Enter()
    {
        UIManager.Instance?.ShowNotification("CYBER TANK АКТИВУВАВ ВСІ СИСТЕМИ ЗБРОЇ!", NotificationType.Danger);
        
        // Максимальна швидкість та агресія
        navMeshAgent.speed *= 1.5f;
        
        // Активація всіх систем
        foreach (var turret in laserTurrets)
        {
            CreateActivationEffect(turret.position);
        }
    }
    
    void CreateActivationEffect(Vector3 position)
    {
        GameObject effect = new GameObject("Activation Effect");
        effect.transform.position = position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.blue;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        
        Destroy(effect, 2f);
    }
}

// ================================
// ДОПОМІЖНІ КЛАСИ
// ================================

public class WeakPoint : MonoBehaviour
{
    public float health = 100f;
    public int weakPointIndex;
    public System.Action<int> onDestroyed;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            onDestroyed?.Invoke(weakPointIndex);
            
            // Ефект знищення
            CreateDestructionEffect();
            
            gameObject.SetActive(false);
        }
    }
    
    void CreateDestructionEffect()
    {
        GameObject effect = new GameObject("Weak Point Destruction");
        effect.transform.position = transform.position;
        
        ParticleSystem explosion = effect.AddComponent<ParticleSystem>();
        var main = explosion.main;
        main.startColor = Color.orange;
        main.startLifetime = 1f;
        main.startSpeed = 15f;
        
        Destroy(effect, 2f);
    }
}

public class TankProjectile : MonoBehaviour
{
    public float damage = 80f;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, DamageType.Explosion);
            }
        }
        
        // Ефект попадання
        CreateImpactEffect();
        Destroy(gameObject);
    }
    
    void CreateImpactEffect()
    {
        GameObject effect = new GameObject("Tank Projectile Impact");
        effect.transform.position = transform.position;
        
        ParticleSystem explosion = effect.AddComponent<ParticleSystem>();
        var main = explosion.main;
        main.startColor = Color.orange;
        main.startLifetime = 0.5f;
        main.startSpeed = 20f;
        
        Destroy(effect, 1f);
    }
}

public class TankMissile : MonoBehaviour
{
    public float damage = 60f;
    public float explosionRadius = 4f;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Environment"))
        {
            Explode();
        }
    }
    
    void Explode()
    {
        // Зональний урон
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Player"))
            {
                PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    playerHealth.TakeDamage(damage * damageMultiplier, DamageType.Explosion);
                }
            }
        }
        
        // Ефект вибуху
        CreateExplosionEffect();
        Destroy(gameObject);
    }
    
    void CreateExplosionEffect()
    {
        GameObject effect = new GameObject("Missile Explosion");
        effect.transform.position = transform.position;
        
        ParticleSystem explosion = effect.AddComponent<ParticleSystem>();
        var main = explosion.main;
        main.startColor = Color.red;
        main.startLifetime = 1f;
        main.startSpeed = 25f;
        main.maxParticles = 50;
        
        Destroy(effect, 2f);
    }
}

// ================================
// UI КОМПОНЕНТИ ДЛЯ БОССІВ
// ================================

public class BossHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public UnityEngine.UI.Slider healthSlider;
    public UnityEngine.UI.Text bossNameText;
    public UnityEngine.UI.Image bossIconImage;
    public UnityEngine.UI.Text phaseText;
    public GameObject healthBarPanel;
    
    private float maxHealth;
    private string currentBossName;
    
    public void SetBoss(string name, Sprite icon, float health)
    {
        currentBossName = name;
        maxHealth = health;
        
        if (bossNameText != null)
        {
            bossNameText.text = name;
        }
        
        if (bossIconImage != null && icon != null)
        {
            bossIconImage.sprite = icon;
        }
        
        if (healthSlider != null)
        {
            healthSlider.maxValue = health;
            healthSlider.value = health;
        }
    }
    
    public void UpdateHealth(float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
    
    public void UpdatePhase(BossPhase phase)
    {
        if (phaseText != null)
        {
            phaseText.text = $"ФАЗА {(int)phase + 1}";
        }
    }
    
    public void Show()
    {
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(true);
        }
    }
    
    public void Hide()
    {
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
        }
    }
}

// ================================
// МЕНЕДЖЕР БОССІВ
// ================================

public class BossManager : MonoBehaviour
{
    [Header("Boss Configurations")]
    public CyberTankConfig cyberTankConfig;
    
    [Header("Boss Spawning")]
    public Transform[] bossSpawnPoints;
    public BossType[] levelBosses; // Боси для кожного рівня
    
    private BossBase currentBoss;
    private int currentLevelIndex = 0;
    
    public static BossManager Instance { get; private set; }
    
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
    
    public void SpawnBoss(BossType bossType, int spawnPointIndex = 0)
    {
        if (currentBoss != null)
        {
            Debug.LogWarning("Босс вже активний!");
            return;
        }
        
        Transform spawnPoint = bossSpawnPoints[spawnPointIndex];
        
        switch (bossType)
        {
            case BossType.CyberTank:
                SpawnCyberTank(spawnPoint);
                break;
            // Додати інших боссів тут
        }
    }
    
    void SpawnCyberTank(Transform spawnPoint)
    {
        GameObject cyberTankPrefab = Resources.Load<GameObject>("Bosses/CyberTank");
        if (cyberTankPrefab != null)
        {
            GameObject bossObject = Instantiate(cyberTankPrefab, spawnPoint.position, spawnPoint.rotation);
            currentBoss = bossObject.GetComponent<CyberTank>();
        }
    }
    
    public void OnBossDefeated(BossBase boss)
    {
        if (currentBoss == boss)
        {
            currentBoss = null;
            
            // Прогрес до наступного рівня
            LevelManager.Instance?.CompleteLevel();
        }
    }
    
    public bool IsBossActive()
    {
        return currentBoss != null && currentBoss.currentPhase != BossPhase.Defeated;
    }
}