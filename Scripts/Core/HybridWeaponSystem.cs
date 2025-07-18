using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ShooterGame.Core;

/// <summary>
/// HYBRID WEAPON SYSTEM - WEEK 4 INTEGRATION
/// Інтегрує NewWeapons_AdvancedArsenal з Character Classes та GitHub WeaponController
/// Використовує Phase 1 інфраструктуру для оптимальної продуктивності
/// </summary>

namespace ShooterGame.Weapons
{
    // ================================
    // WEAPON TYPE ENUMS
    // ================================
    
    public enum WeaponCategory
    {
        Pistol,
        AssaultRifle,
        Shotgun,
        SniperRifle,
        SMG,
        LMG,
        Special,
        Melee
    }

    public enum WeaponRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum WeaponTier
    {
        Basic,      // Рівень 1-10
        Advanced,   // Рівень 11-25
        Expert,     // Рівень 26-50
        Master,     // Рівень 51-75
        Legendary   // Рівень 76-100
    }

    // ================================
    // HYBRID WEAPON CONFIGURATION
    // ================================

    [System.Serializable]
    public class HybridWeaponConfig
    {
        [Header("Basic Info")]
        public string weaponId;
        public string displayName;
        public string description;
        public WeaponCategory category;
        public WeaponRarity rarity;
        public WeaponTier tier;
        public Sprite weaponIcon;
        public GameObject weaponPrefab;

        [Header("Combat Stats")]
        public float baseDamage = 25f;
        public float fireRate = 8f;
        public float bulletForce = 30f;
        public float bulletSpread = 0.05f;
        public float maxRange = 100f;
        public float headshotMultiplier = 2f;

        [Header("Ammo & Reload")]
        public int magazineSize = 30;
        public float reloadTime = 2f;
        public int maxAmmo = 300;
        public bool isAutomatic = true;

        [Header("Character Class Bonuses")]
        public Dictionary<CharacterClass, WeaponClassBonus> classBonuses = new Dictionary<CharacterClass, WeaponClassBonus>();

        [Header("Special Abilities")]
        public bool hasSpecialAbility = false;
        public string abilityName;
        public string abilityDescription;
        public float abilityCooldown = 30f;
        public float abilityDuration = 5f;

        // Інтеграція з Performance Monitor
        public void LogPerformanceMetric(string operation)
        {
            var monitor = PerformanceMonitor.Instance;
            monitor?.TakeSnapshot($"Weapon_{weaponId}_{operation}");
        }
    }

    [System.Serializable]
    public class WeaponClassBonus
    {
        public float damageMultiplier = 1f;
        public float fireRateMultiplier = 1f;
        public float reloadSpeedMultiplier = 1f;
        public float accuracyMultiplier = 1f;
        public bool hasSpecialEffect = false;
        public string specialEffectDescription;
    }

    // ================================
    // ADVANCED WEAPON IMPLEMENTATIONS
    // ================================

    [CreateAssetMenu(fileName = "PlasmaRifle_Config", menuName = "Game/Weapons/Advanced/Plasma Rifle")]
    public class PlasmaRifleConfig : ScriptableObject
    {
        public HybridWeaponConfig config;

        void Reset()
        {
            config = new HybridWeaponConfig
            {
                weaponId = "plasma_rifle_mk1",
                displayName = "Plasma Rifle MK-I",
                description = "Енергетична зброя з високим пошкодженням та унікальними ефектами",
                category = WeaponCategory.AssaultRifle,
                rarity = WeaponRarity.Epic,
                tier = WeaponTier.Advanced,

                baseDamage = 45f,
                fireRate = 6f,
                bulletForce = 50f,
                bulletSpread = 0.02f,
                maxRange = 150f,
                headshotMultiplier = 2.5f,

                magazineSize = 25,
                reloadTime = 2.5f,
                maxAmmo = 200,
                isAutomatic = true,

                hasSpecialAbility = true,
                abilityName = "Plasma Overcharge",
                abilityDescription = "Збільшує пошкодження на 50% протягом 8 секунд",
                abilityCooldown = 45f,
                abilityDuration = 8f
            };

            // Character Class Bonuses
            config.classBonuses[CharacterClass.Assault] = new WeaponClassBonus
            {
                damageMultiplier = 1.2f,
                fireRateMultiplier = 1.1f,
                reloadSpeedMultiplier = 1.15f,
                accuracyMultiplier = 1.1f,
                hasSpecialEffect = true,
                specialEffectDescription = "Plasma shots have 15% chance to chain to nearby enemies"
            };

            config.classBonuses[CharacterClass.Tank] = new WeaponClassBonus
            {
                damageMultiplier = 1.1f,
                fireRateMultiplier = 0.9f,
                reloadSpeedMultiplier = 1.0f,
                accuracyMultiplier = 1.2f,
                hasSpecialEffect = true,
                specialEffectDescription = "Plasma shots penetrate through armor more effectively"
            };
        }
    }

    [CreateAssetMenu(fileName = "GaussSniper_Config", menuName = "Game/Weapons/Advanced/Gauss Sniper")]
    public class GaussSniperConfig : ScriptableObject
    {
        public HybridWeaponConfig config;

        void Reset()
        {
            config = new HybridWeaponConfig
            {
                weaponId = "gauss_sniper_x7",
                displayName = "Gauss Sniper X-7",
                description = "Електромагнітна снайперська гвинтівка з пробивною здатністю",
                category = WeaponCategory.SniperRifle,
                rarity = WeaponRarity.Legendary,
                tier = WeaponTier.Expert,

                baseDamage = 120f,
                fireRate = 1.2f,
                bulletForce = 100f,
                bulletSpread = 0.001f,
                maxRange = 300f,
                headshotMultiplier = 4f,

                magazineSize = 5,
                reloadTime = 3.5f,
                maxAmmo = 50,
                isAutomatic = false,

                hasSpecialAbility = true,
                abilityName = "Electromagnetic Pulse",
                abilityDescription = "Наступний постріл проходить крізь всі перешкоди",
                abilityCooldown = 60f,
                abilityDuration = 1f
            };

            // Sniper Class має найбільші бонуси
            config.classBonuses[CharacterClass.Sniper] = new WeaponClassBonus
            {
                damageMultiplier = 1.5f,
                fireRateMultiplier = 1.3f,
                reloadSpeedMultiplier = 1.4f,
                accuracyMultiplier = 1.8f,
                hasSpecialEffect = true,
                specialEffectDescription = "Perfect accuracy when scoped + bullet time effect"
            };
        }
    }

    [CreateAssetMenu(fileName = "NanoShotgun_Config", menuName = "Game/Weapons/Advanced/Nano Shotgun")]
    public class NanoShotgunConfig : ScriptableObject
    {
        public HybridWeaponConfig config;

        void Reset()
        {
            config = new HybridWeaponConfig
            {
                weaponId = "nano_shotgun_v3",
                displayName = "Nano Shotgun V-3",
                description = "Нанотехнологічний дробовик з адаптивними снарядами",
                category = WeaponCategory.Shotgun,
                rarity = WeaponRarity.Rare,
                tier = WeaponTier.Advanced,

                baseDamage = 80f,
                fireRate = 2.5f,
                bulletForce = 40f,
                bulletSpread = 0.15f,
                maxRange = 30f,
                headshotMultiplier = 1.8f,

                magazineSize = 8,
                reloadTime = 2.8f,
                maxAmmo = 80,
                isAutomatic = false,

                hasSpecialAbility = true,
                abilityName = "Nano Swarm",
                abilityDescription = "Снаряди самонаводяться на ворогів",
                abilityCooldown = 35f,
                abilityDuration = 6f
            };

            // Tank та Engineer мають бонуси для shotgun
            config.classBonuses[CharacterClass.Tank] = new WeaponClassBonus
            {
                damageMultiplier = 1.3f,
                fireRateMultiplier = 1.1f,
                reloadSpeedMultiplier = 1.2f,
                accuracyMultiplier = 1.0f,
                hasSpecialEffect = true,
                specialEffectDescription = "Shotgun blasts knock back enemies further"
            };

            config.classBonuses[CharacterClass.Engineer] = new WeaponClassBonus
            {
                damageMultiplier = 1.1f,
                fireRateMultiplier = 1.2f,
                reloadSpeedMultiplier = 1.3f,
                accuracyMultiplier = 1.1f,
                hasSpecialEffect = true,
                specialEffectDescription = "Nano pellets repair nearby structures"
            };
        }
    }

    // ================================
    // HYBRID WEAPON MANAGER
    // ================================

    public class HybridWeaponManager : MonoBehaviour
    {
        [Header("Weapon System Configuration")]
        public List<HybridWeaponConfig> availableWeapons = new List<HybridWeaponConfig>();
        public Transform weaponHolder;
        public LayerMask enemyLayers = -1;

        [Header("Integration References")]
        public WeaponController legacyWeaponController; // GitHub version compatibility
        public CharacterClassManager characterClassManager;

        // Current weapon state
        private HybridWeaponConfig currentWeapon;
        private GameObject currentWeaponObject;
        private bool isSpecialAbilityActive = false;
        private float abilityTimer = 0f;
        private float abilityCooldownTimer = 0f;

        // Performance monitoring
        private PerformanceMonitor performanceMonitor;
        private QualityGates qualityGates;
        private UniversalObjectPool objectPool;

        // Events
        public static event System.Action<HybridWeaponConfig> OnWeaponChanged;
        public static event System.Action<string> OnWeaponAbilityActivated;
        public static event System.Action<float> OnAmmoChanged;

        // Singleton
        public static HybridWeaponManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeWeaponSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Інтеграція з існуючими системами
            performanceMonitor = PerformanceMonitor.Instance;
            qualityGates = QualityGates.Instance;
            objectPool = UniversalObjectPool.Instance;
            characterClassManager = CharacterClassManager.Instance;

            // Автоматичне знаходження legacy controller
            if (legacyWeaponController == null)
                legacyWeaponController = FindObjectOfType<WeaponController>();

            // Performance tracking
            performanceMonitor?.TakeSnapshot("HybridWeaponManager_Initialized");

            // Встановлення початкової зброї
            if (availableWeapons.Count > 0)
            {
                EquipWeapon(availableWeapons[0]);
            }
        }

        void Update()
        {
            UpdateWeaponTimers();
            HandleInput();
            
            // Quality validation кожні 3 секунди
            if (Time.time % 3f < Time.deltaTime)
            {
                ValidateWeaponSystemQuality();
            }
        }

        private void InitializeWeaponSystem()
        {
            // Створення advanced weapons якщо список порожній
            if (availableWeapons.Count == 0)
            {
                LoadAdvancedWeapons();
            }

            Debug.Log($"[HybridWeaponManager] Initialized with {availableWeapons.Count} weapons");
        }

        private void LoadAdvancedWeapons()
        {
            // Plasma Rifle
            var plasmaRifle = ScriptableObject.CreateInstance<PlasmaRifleConfig>();
            plasmaRifle.Reset();
            availableWeapons.Add(plasmaRifle.config);

            // Gauss Sniper
            var gaussSniper = ScriptableObject.CreateInstance<GaussSniperConfig>();
            gaussSniper.Reset();
            availableWeapons.Add(gaussSniper.config);

            // Nano Shotgun
            var nanoShotgun = ScriptableObject.CreateInstance<NanoShotgunConfig>();
            nanoShotgun.Reset();
            availableWeapons.Add(nanoShotgun.config);

            Debug.Log("[HybridWeaponManager] Advanced weapons loaded");
        }

        public void EquipWeapon(HybridWeaponConfig weapon)
        {
            if (weapon == null) return;

            // Performance tracking
            performanceMonitor?.TakeSnapshot($"EquipWeapon_{weapon.weaponId}_Start");

            // Видалення поточної зброї
            if (currentWeaponObject != null)
            {
                objectPool?.Return(currentWeaponObject, "Weapon");
            }

            // Встановлення нової зброї
            currentWeapon = weapon;
            
            // Створення weapon object через Object Pool
            if (weapon.weaponPrefab != null)
            {
                currentWeaponObject = objectPool?.Get("Weapon");
                if (currentWeaponObject != null)
                {
                    currentWeaponObject.transform.SetParent(weaponHolder);
                    currentWeaponObject.transform.localPosition = Vector3.zero;
                    currentWeaponObject.transform.localRotation = Quaternion.identity;
                }
            }

            // Застосування class bonuses
            ApplyCharacterClassBonuses();

            // Інтеграція з legacy weapon controller
            SyncWithLegacyController();

            OnWeaponChanged?.Invoke(weapon);
            Debug.Log($"[HybridWeaponManager] Equipped weapon: {weapon.displayName}");

            // Performance tracking
            performanceMonitor?.TakeSnapshot($"EquipWeapon_{weapon.weaponId}_Complete");
        }

        private void ApplyCharacterClassBonuses()
        {
            if (currentWeapon == null || characterClassManager == null) return;

            var currentClass = characterClassManager.GetCurrentClass();
            if (currentClass != null && currentWeapon.classBonuses.ContainsKey(currentClass.classType))
            {
                var bonus = currentWeapon.classBonuses[currentClass.classType];
                
                // Застосування бонусів до legacy controller
                if (legacyWeaponController != null)
                {
                    legacyWeaponController.bulletForce *= bonus.damageMultiplier;
                    legacyWeaponController.fireRate *= bonus.fireRateMultiplier;
                    legacyWeaponController.reloadTime /= bonus.reloadSpeedMultiplier;
                    legacyWeaponController.bulletSpread /= bonus.accuracyMultiplier;
                }

                Debug.Log($"[HybridWeaponManager] Applied {currentClass.classType} bonuses to {currentWeapon.displayName}");
                
                if (bonus.hasSpecialEffect)
                {
                    Debug.Log($"[HybridWeaponManager] Special Effect: {bonus.specialEffectDescription}");
                }
            }
        }

        private void SyncWithLegacyController()
        {
            if (legacyWeaponController == null || currentWeapon == null) return;

            // Синхронізація параметрів з legacy controller
            legacyWeaponController.bulletForce = currentWeapon.bulletForce;
            legacyWeaponController.fireRate = currentWeapon.fireRate;
            legacyWeaponController.magazineSize = currentWeapon.magazineSize;
            legacyWeaponController.reloadTime = currentWeapon.reloadTime;
            legacyWeaponController.bulletSpread = currentWeapon.bulletSpread;
            legacyWeaponController.maxAimDistance = currentWeapon.maxRange;

            Debug.Log($"[HybridWeaponManager] Synced with legacy WeaponController");
        }

        public void ActivateWeaponAbility()
        {
            if (currentWeapon == null || !currentWeapon.hasSpecialAbility) return;
            if (isSpecialAbilityActive || abilityCooldownTimer > 0f) return;

            // Performance tracking
            performanceMonitor?.TakeSnapshot($"WeaponAbility_{currentWeapon.weaponId}_Activated");

            isSpecialAbilityActive = true;
            abilityTimer = currentWeapon.abilityDuration;
            abilityCooldownTimer = currentWeapon.abilityCooldown;

            // Створення ability effect через Object Pool
            var abilityEffect = objectPool?.Get("WeaponAbilityEffect");
            if (abilityEffect != null)
            {
                abilityEffect.transform.position = transform.position;
                abilityEffect.SetActive(true);
            }

            OnWeaponAbilityActivated?.Invoke(currentWeapon.abilityName);
            Debug.Log($"[HybridWeaponManager] Activated ability: {currentWeapon.abilityName}");
        }

        private void UpdateWeaponTimers()
        {
            // Ability timer
            if (abilityTimer > 0f)
            {
                abilityTimer -= Time.deltaTime;
                if (abilityTimer <= 0f)
                {
                    isSpecialAbilityActive = false;
                    Debug.Log($"[HybridWeaponManager] Ability {currentWeapon.abilityName} ended");
                }
            }

            // Cooldown timer
            if (abilityCooldownTimer > 0f)
            {
                abilityCooldownTimer -= Time.deltaTime;
            }
        }

        private void HandleInput()
        {
            // Weapon ability activation
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ActivateWeaponAbility();
            }

            // Weapon switching (1-9 keys)
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int weaponIndex = i - 1;
                    if (weaponIndex < availableWeapons.Count)
                    {
                        EquipWeapon(availableWeapons[weaponIndex]);
                    }
                }
            }
        }

        private void ValidateWeaponSystemQuality()
        {
            if (qualityGates != null)
            {
                bool qualityPassed = qualityGates.ValidateQuality();
                if (!qualityPassed)
                {
                    Debug.LogWarning("[HybridWeaponManager] Quality gates failed! Weapon system performance may be degraded.");
                }
            }
        }

        public HybridWeaponConfig GetCurrentWeapon()
        {
            return currentWeapon;
        }

        public List<HybridWeaponConfig> GetAvailableWeapons()
        {
            return new List<HybridWeaponConfig>(availableWeapons);
        }

        public bool IsAbilityReady()
        {
            return currentWeapon != null && currentWeapon.hasSpecialAbility && abilityCooldownTimer <= 0f;
        }

        public float GetAbilityCooldownProgress()
        {
            if (currentWeapon == null || !currentWeapon.hasSpecialAbility) return 0f;
            return 1f - (abilityCooldownTimer / currentWeapon.abilityCooldown);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}