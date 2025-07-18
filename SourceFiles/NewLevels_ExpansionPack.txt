using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// РОЗШИРЕНИЙ ПАКЕТ РІВНІВ - НОВІ ЛОКАЦІЇ ТА ГЕЙМПЛЕЙНІ МЕХАНІКИ
/// Включає 8 нових рівнів з унікальними особливостями та челенджами
/// </summary>

// ================================
// НОВІ ТИПИ РІВНІВ
// ================================

public enum NewLevelType
{
    UrbanWarfare,      // Міський бій
    IndustrialComplex, // Промисловий комплекс
    UndergroundBase,   // Підземна база
    SkyPlatform,       // Небесна платформа
    AbandonedCity,     // Покинуте місто
    CyberLab,          // Кібер-лабораторія
    DesertOutpost,     // Пустельний форпост
    SpaceStation       // Космічна станція
}

// ================================
// РІВЕНЬ 1: МІСЬКИЙ БІЙ
// ================================

[CreateAssetMenu(fileName = "UrbanWarfare_Config", menuName = "Game/Levels/Urban Warfare")]
public class UrbanWarfareLevelConfig : LevelConfiguration
{
    [Header("Urban Warfare Settings")]
    [Tooltip("Кількість будівель для укриття")]
    public int buildingCount = 15;
    [Tooltip("Щільність цивільних об'єктів")]
    public float civilianDensity = 0.3f;
    [Tooltip("Використовувати вертикальний геймплей")]
    public bool useVerticalGameplay = true;
    [Tooltip("Снайперські позиції")]
    public Transform[] sniperPositions;

    [Header("Environmental Hazards")]
    [Tooltip("Вибухонебезпечні автомобілі")]
    public GameObject[] explosiveCars;
    [Tooltip("Руйнівні елементи")]
    public DestructibleBuilding[] destructibleBuildings;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        SetupUrbanEnvironment();
        SpawnCivilianObjects();
        SetupSniperPositions();
    }

    void SetupUrbanEnvironment()
    {
        // Налаштування міського середовища
        foreach (var car in explosiveCars)
        {
            var explosive = car.GetComponent<ExplosiveObject>();
            if (explosive != null)
            {
                explosive.explosionRadius = 8f;
                explosive.explosionDamage = 150f;
            }
        }
    }

    void SpawnCivilianObjects()
    {
        // Спавн цивільних об'єктів для реалізму
        for (int i = 0; i < buildingCount * civilianDensity; i++)
        {
            // Логіка спавну цивільних об'єктів
        }
    }

    void SetupSniperPositions()
    {
        // Налаштування снайперських позицій
        foreach (var position in sniperPositions)
        {
            var sniperSpawn = position.GetComponent<EnemySpawnPoint>();
            if (sniperSpawn != null)
            {
                sniperSpawn.enemyType = EnemyType.EliteSniper;
                sniperSpawn.spawnDelay = Random.Range(30f, 60f);
            }
        }
    }
}

// ================================
// РІВЕНЬ 2: ПРОМИСЛОВИЙ КОМПЛЕКС
// ================================

[CreateAssetMenu(fileName = "Industrial_Config", menuName = "Game/Levels/Industrial Complex")]
public class IndustrialComplexConfig : LevelConfiguration
{
    [Header("Industrial Settings")]
    [Tooltip("Конвеєрні лінії")]
    public ConveyorBelt[] conveyorBelts;
    [Tooltip("Промислові роботи")]
    public IndustrialRobot[] robots;
    [Tooltip("Токсичні зони")]
    public ToxicZone[] toxicZones;
    [Tooltip("Енергетичні генератори")]
    public PowerGenerator[] generators;

    [Header("Factory Mechanics")]
    [Tooltip("Швидкість конвеєра")]
    public float conveyorSpeed = 2f;
    [Tooltip("Урон від токсичних зон")]
    public float toxicDamage = 10f;
    [Tooltip("Час між токсичними атаками")]
    public float toxicInterval = 2f;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        ActivateConveyorBelts();
        SetupToxicZones();
        InitializeRobots();
        SetupPowerGrid();
    }

    void ActivateConveyorBelts()
    {
        foreach (var belt in conveyorBelts)
        {
            belt.speed = conveyorSpeed;
            belt.Activate();
        }
    }

    void SetupToxicZones()
    {
        foreach (var zone in toxicZones)
        {
            zone.damagePerSecond = toxicDamage;
            zone.damageInterval = toxicInterval;
            zone.EnableToxicEffect();
        }
    }

    void InitializeRobots()
    {
        foreach (var robot in robots)
        {
            robot.patrolSpeed = 3f;
            robot.detectionRange = 15f;
            robot.StartPatrol();
        }
    }

    void SetupPowerGrid()
    {
        foreach (var generator in generators)
        {
            generator.powerOutput = 100f;
            generator.canBeDestroyed = true;
            generator.onDestroyed += OnGeneratorDestroyed;
        }
    }

    void OnGeneratorDestroyed(PowerGenerator generator)
    {
        // Вимкнення частини освітлення та систем
        DisablePowerInArea(generator.powerRadius);
    }

    void DisablePowerInArea(float radius)
    {
        // Логіка вимкнення електроенергії в радіусі
    }
}

// ================================
// РІВЕНЬ 3: ПІДЗЕМНА БАЗА
// ================================

[CreateAssetMenu(fileName = "Underground_Config", menuName = "Game/Levels/Underground Base")]
public class UndergroundBaseConfig : LevelConfiguration
{
    [Header("Underground Settings")]
    [Tooltip("Рівень освітлення")]
    public float lightLevel = 0.3f;
    [Tooltip("Лабіринтні коридори")]
    public CorridorSection[] corridors;
    [Tooltip("Системи вентиляції")]
    public VentilationShaft[] vents;
    [Tooltip("Системи безпеки")]
    public SecuritySystem[] securitySystems;

    [Header("Stealth Mechanics")]
    [Tooltip("Використовувати стелс механіки")]
    public bool useStealthMechanics = true;
    [Tooltip("Радіус виявлення в темряві")]
    public float darkDetectionRadius = 5f;
    [Tooltip("Радіус виявлення при освітленні")]
    public float lightDetectionRadius = 12f;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        SetupLighting();
        InitializeVentilationSystem();
        ActivateSecuritySystems();
        ConfigureStealthMechanics();
    }

    void SetupLighting()
    {
        // Налаштування освітлення для атмосфери
        RenderSettings.ambientIntensity = lightLevel;
        
        var lights = FindObjectsOfType<Light>();
        foreach (var light in lights)
        {
            light.intensity *= lightLevel;
            light.shadows = LightShadows.Hard;
        }
    }

    void InitializeVentilationSystem()
    {
        foreach (var vent in vents)
        {
            vent.allowPlayerTraversal = true;
            vent.noiseLevel = 0.2f; // Тихий рух через вентиляцію
            vent.SetupVentPath();
        }
    }

    void ActivateSecuritySystems()
    {
        foreach (var security in securitySystems)
        {
            security.detectionRange = lightDetectionRadius;
            security.alertLevel = SecurityAlertLevel.High;
            security.Activate();
        }
    }

    void ConfigureStealthMechanics()
    {
        if (useStealthMechanics)
        {
            var stealthManager = FindObjectOfType<StealthManager>();
            if (stealthManager != null)
            {
                stealthManager.darkDetectionRadius = darkDetectionRadius;
                stealthManager.lightDetectionRadius = lightDetectionRadius;
                stealthManager.EnableStealthMode();
            }
        }
    }
}

// ================================
// РІВЕНЬ 4: НЕБЕСНА ПЛАТФОРМА
// ================================

[CreateAssetMenu(fileName = "SkyPlatform_Config", menuName = "Game/Levels/Sky Platform")]
public class SkyPlatformConfig : LevelConfiguration
{
    [Header("Sky Platform Settings")]
    [Tooltip("Висота платформи")]
    public float platformHeight = 1000f;
    [Tooltip("Швидкість вітру")]
    public float windSpeed = 15f;
    [Tooltip("Напрямок вітру")]
    public Vector3 windDirection = Vector3.right;
    [Tooltip("Рухомі платформи")]
    public MovingPlatform[] movingPlatforms;

    [Header("Weather Effects")]
    [Tooltip("Використовувати погодні ефекти")]
    public bool useWeatherEffects = true;
    [Tooltip("Інтенсивність туману")]
    public float fogDensity = 0.02f;
    [Tooltip("Ймовірність блискавки")]
    public float lightningChance = 0.1f;

    [Header("Aerial Combat")]
    [Tooltip("Літаючі вороги")]
    public FlyingEnemy[] flyingEnemies;
    [Tooltip("Повітряні транспорти")]
    public AerialVehicle[] aerialVehicles;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        SetupWindEffects();
        InitializeMovingPlatforms();
        ConfigureWeatherSystem();
        SpawnAerialEnemies();
    }

    void SetupWindEffects()
    {
        var windZone = FindObjectOfType<WindZone>();
        if (windZone == null)
        {
            var windObject = new GameObject("Wind Zone");
            windZone = windObject.AddComponent<WindZone>();
        }

        windZone.mode = WindZoneMode.Directional;
        windZone.windMain = windSpeed;
        windZone.windTurbulence = windSpeed * 0.3f;
        windZone.transform.rotation = Quaternion.LookRotation(windDirection);
    }

    void InitializeMovingPlatforms()
    {
        foreach (var platform in movingPlatforms)
        {
            platform.movementSpeed = 3f;
            platform.waitTime = 2f;
            platform.StartMovement();
        }
    }

    void ConfigureWeatherSystem()
    {
        if (useWeatherEffects)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogColor = Color.gray;

            StartCoroutine(WeatherEffectsCoroutine());
        }
    }

    IEnumerator WeatherEffectsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10f, 30f));
            
            if (Random.value < lightningChance)
            {
                TriggerLightningEffect();
            }
        }
    }

    void TriggerLightningEffect()
    {
        // Ефект блискавки
        var lightning = FindObjectOfType<LightningEffect>();
        if (lightning != null)
        {
            lightning.TriggerLightning();
        }
    }

    void SpawnAerialEnemies()
    {
        foreach (var enemy in flyingEnemies)
        {
            enemy.flightHeight = platformHeight + Random.Range(-50f, 50f);
            enemy.patrolRadius = 100f;
            enemy.StartFlying();
        }
    }
}

// ================================
// РІВЕНЬ 5: ПОКИНУТЕ МІСТО
// ================================

[CreateAssetMenu(fileName = "AbandonedCity_Config", menuName = "Game/Levels/Abandoned City")]
public class AbandonedCityConfig : LevelConfiguration
{
    [Header("Abandoned City Settings")]
    [Tooltip("Рівень руйнування")]
    public float destructionLevel = 0.7f;
    [Tooltip("Радіаційні зони")]
    public RadiationZone[] radiationZones;
    [Tooltip("Покинуті транспортні засоби")]
    public AbandonedVehicle[] vehicles;
    [Tooltip("Мутанти")]
    public MutantEnemy[] mutants;

    [Header("Survival Elements")]
    [Tooltip("Використовувати елементи виживання")]
    public bool useSurvivalElements = true;
    [Tooltip("Радіаційний урон")]
    public float radiationDamage = 5f;
    [Tooltip("Час між радіаційними атаками")]
    public float radiationInterval = 3f;

    [Header("Scavenging")]
    [Tooltip("Точки збору ресурсів")]
    public ScavengePoint[] scavengePoints;
    [Tooltip("Рідкісні предмети")]
    public RareItem[] rareItems;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        SetupDestructionLevel();
        InitializeRadiationZones();
        SpawnMutants();
        SetupScavenging();
    }

    void SetupDestructionLevel()
    {
        var buildings = FindObjectsOfType<Building>();
        foreach (var building in buildings)
        {
            if (Random.value < destructionLevel)
            {
                building.ApplyDestruction(Random.Range(0.3f, 0.9f));
            }
        }
    }

    void InitializeRadiationZones()
    {
        foreach (var zone in radiationZones)
        {
            zone.damagePerSecond = radiationDamage;
            zone.damageInterval = radiationInterval;
            zone.EnableRadiation();
        }
    }

    void SpawnMutants()
    {
        foreach (var mutant in mutants)
        {
            mutant.aggressionLevel = 0.8f;
            mutant.packBehavior = true;
            mutant.StartHunting();
        }
    }

    void SetupScavenging()
    {
        foreach (var point in scavengePoints)
        {
            point.respawnTime = Random.Range(300f, 600f); // 5-10 хвилин
            point.lootTable = GenerateAbandonedCityLoot();
            point.Initialize();
        }
    }

    LootTable GenerateAbandonedCityLoot()
    {
        var lootTable = ScriptableObject.CreateInstance<LootTable>();
        // Налаштування таблиці лута для покинутого міста
        return lootTable;
    }
}

// ================================
// РІВЕНЬ 6: КІБЕР-ЛАБОРАТОРІЯ
// ================================

[CreateAssetMenu(fileName = "CyberLab_Config", menuName = "Game/Levels/Cyber Laboratory")]
public class CyberLabConfig : LevelConfiguration
{
    [Header("Cyber Lab Settings")]
    [Tooltip("Рівень технологій")]
    public int techLevel = 5;
    [Tooltip("Голографічні проекції")]
    public HolographicProjection[] holograms;
    [Tooltip("Кібер-вороги")]
    public CyberEnemy[] cyberEnemies;
    [Tooltip("Енергетичні бар'єри")]
    public EnergyBarrier[] energyBarriers;

    [Header("Hacking Mechanics")]
    [Tooltip("Використовувати механіки хакінгу")]
    public bool useHackingMechanics = true;
    [Tooltip("Термінали для хакінгу")]
    public HackableTerminal[] terminals;
    [Tooltip("Системи безпеки")]
    public CyberSecuritySystem[] securitySystems;

    [Header("Digital Environment")]
    [Tooltip("Цифрові ефекти")]
    public DigitalEffect[] digitalEffects;
    [Tooltip("Матричні переходи")]
    public MatrixTransition[] matrixTransitions;

    public override void InitializeLevel()
    {
        base.InitializeLevel();
        SetupCyberEnvironment();
        InitializeHackingSystems();
        ActivateDigitalEffects();
        SpawnCyberEnemies();
    }

    void SetupCyberEnvironment()
    {
        foreach (var hologram in holograms)
        {
            hologram.transparency = 0.7f;
            hologram.flickerRate = Random.Range(0.1f, 0.5f);
            hologram.Activate();
        }

        foreach (var barrier in energyBarriers)
        {
            barrier.energyLevel = 100f;
            barrier.rechargeRate = 10f;
            barrier.Activate();
        }
    }

    void InitializeHackingSystems()
    {
        if (useHackingMechanics)
        {
            foreach (var terminal in terminals)
            {
                terminal.securityLevel = Random.Range(1, techLevel);
                terminal.hackingTime = terminal.securityLevel * 5f;
                terminal.onHackSuccess += OnTerminalHacked;
            }
        }
    }

    void OnTerminalHacked(HackableTerminal terminal)
    {
        // Логіка після успішного хакінгу
        DisableSecurityInArea(terminal.controlRadius);
    }

    void DisableSecurityInArea(float radius)
    {
        // Вимкнення систем безпеки в радіусі
    }

    void ActivateDigitalEffects()
    {
        foreach (var effect in digitalEffects)
        {
            effect.intensity = Random.Range(0.5f, 1f);
            effect.Activate();
        }
    }

    void SpawnCyberEnemies()
    {
        foreach (var enemy in cyberEnemies)
        {
            enemy.digitalCamouflage = true;
            enemy.hackingAbility = true;
            enemy.techLevel = techLevel;
            enemy.Initialize();
        }
    }
}

// ================================
// ДОПОМІЖНІ КЛАСИ ДЛЯ НОВИХ РІВНІВ
// ================================

[System.Serializable]
public class DestructibleBuilding : MonoBehaviour
{
    public float maxHealth = 1000f;
    public float currentHealth;
    public GameObject[] destructionStages;
    public ParticleSystem destructionEffect;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDestruction(float destructionPercent)
    {
        currentHealth = maxHealth * (1f - destructionPercent);
        UpdateDestructionVisual();
    }

    void UpdateDestructionVisual()
    {
        float healthPercent = currentHealth / maxHealth;
        int stageIndex = Mathf.FloorToInt((1f - healthPercent) * destructionStages.Length);
        
        for (int i = 0; i < destructionStages.Length; i++)
        {
            destructionStages[i].SetActive(i == stageIndex);
        }
    }
}

[System.Serializable]
public class ConveyorBelt : MonoBehaviour
{
    public float speed = 2f;
    public Vector3 direction = Vector3.forward;
    public bool isActive = false;

    void Update()
    {
        if (isActive)
        {
            MoveObjectsOnBelt();
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    void MoveObjectsOnBelt()
    {
        // Логіка переміщення об'єктів на конвеєрі
        Collider[] objects = Physics.OverlapBox(transform.position, transform.localScale / 2);
        foreach (var obj in objects)
        {
            if (obj.GetComponent<Rigidbody>() != null)
            {
                obj.GetComponent<Rigidbody>().velocity += direction * speed;
            }
        }
    }
}

[System.Serializable]
public class ToxicZone : MonoBehaviour
{
    public float damagePerSecond = 10f;
    public float damageInterval = 1f;
    public ParticleSystem toxicEffect;
    public bool isActive = false;

    void Start()
    {
        if (isActive)
        {
            EnableToxicEffect();
        }
    }

    public void EnableToxicEffect()
    {
        isActive = true;
        if (toxicEffect != null)
        {
            toxicEffect.Play();
        }
        StartCoroutine(ToxicDamageCoroutine());
    }

    IEnumerator ToxicDamageCoroutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(damageInterval);
            DamagePlayersInZone();
        }
    }

    void DamagePlayersInZone()
    {
        Collider[] players = Physics.OverlapBox(transform.position, transform.localScale / 2, 
            transform.rotation, LayerMask.GetMask("Player"));
        
        foreach (var player in players)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damagePerSecond, DamageType.Toxic);
            }
        }
    }
}

// ================================
// МЕНЕДЖЕР НОВИХ РІВНІВ
// ================================

public class NewLevelManager : MonoBehaviour
{
    [Header("New Level Configurations")]
    public UrbanWarfareLevelConfig urbanWarfare;
    public IndustrialComplexConfig industrialComplex;
    public UndergroundBaseConfig undergroundBase;
    public SkyPlatformConfig skyPlatform;
    public AbandonedCityConfig abandonedCity;
    public CyberLabConfig cyberLab;

    [Header("Level Progression")]
    public int currentNewLevelIndex = 0;
    public NewLevelType[] levelProgression;

    public void LoadNewLevel(NewLevelType levelType)
    {
        switch (levelType)
        {
            case NewLevelType.UrbanWarfare:
                LoadLevel(urbanWarfare);
                break;
            case NewLevelType.IndustrialComplex:
                LoadLevel(industrialComplex);
                break;
            case NewLevelType.UndergroundBase:
                LoadLevel(undergroundBase);
                break;
            case NewLevelType.SkyPlatform:
                LoadLevel(skyPlatform);
                break;
            case NewLevelType.AbandonedCity:
                LoadLevel(abandonedCity);
                break;
            case NewLevelType.CyberLab:
                LoadLevel(cyberLab);
                break;
        }
    }

    void LoadLevel(LevelConfiguration config)
    {
        if (config != null)
        {
            config.InitializeLevel();
            Debug.Log($"Завантажено новий рівень: {config.name}");
        }
    }

    public void ProgressToNextNewLevel()
    {
        if (currentNewLevelIndex < levelProgression.Length - 1)
        {
            currentNewLevelIndex++;
            LoadNewLevel(levelProgression[currentNewLevelIndex]);
        }
    }
}