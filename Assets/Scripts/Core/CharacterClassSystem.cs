using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HYBRID CHARACTER CLASS SYSTEM - WEEK 3 INTEGRATION
/// Інтегрує CharacterClasses_Specialization з GitHub PerkSystem
/// Використовує створену інфраструктуру (Object Pool, Performance Monitor, Quality Gates)
/// </summary>

namespace ShooterGame.Core
{
    // ================================
    // ТИПИ КЛАСІВ ПЕРСОНАЖА
    // ================================
    
    public enum CharacterClass
    {
        Assault,        // Збалансований штурмовик
        Tank,           // Важкий захисник
        Sniper,         // Далекобійний стрілець
        Engineer,       // Технічний спеціаліст
        Medic           // Підтримка та лікування
    }

    public enum ClassTier
    {
        Novice,         // Початківець (рівень 1-10)
        Veteran,        // Ветеран (рівень 11-25)
        Expert,         // Експерт (рівень 26-50)
        Master,         // Майстер (рівень 51-75)
        Legend          // Легенда (рівень 76-100)
    }

    // ================================
    // БАЗОВИЙ КЛАС ПЕРСОНАЖА
    // ================================

    [System.Serializable]
    public abstract class CharacterClassBase
    {
        [Header("Class Information")]
        public CharacterClass classType;
        public string className;
        public string description;
        public Sprite classIcon;
        
        [Header("Base Stats")]
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float armorMultiplier = 1f;
        public float reloadSpeedMultiplier = 1f;
        
        [Header("Special Abilities")]
        public float abilityCooldown = 30f;
        public float abilityDuration = 10f;
        public string abilityName;
        public string abilityDescription;
        
        // Інтеграція з Performance Monitor
        protected PerformanceMonitor performanceMonitor;
        
        public virtual void Initialize()
        {
            performanceMonitor = PerformanceMonitor.Instance;
            if (performanceMonitor != null)
            {
                performanceMonitor.TakeSnapshot($"CharacterClass_{classType}_Initialize");
            }
        }
        
        public abstract void ActivateSpecialAbility();
        public abstract void UpdateClassStats(PlayerHealth playerHealth, PlayerMovement playerMovement);
        
        // Інтеграція з Quality Gates
        public virtual bool ValidateClassIntegrity()
        {
            var qualityGates = QualityGates.Instance;
            if (qualityGates != null)
            {
                return qualityGates.ValidateQuality();
            }
            return true;
        }
    }

    // ================================
    // ASSAULT CLASS - ЗБАЛАНСОВАНИЙ ШТУРМОВИК
    // ================================

    [CreateAssetMenu(fileName = "Assault_Class", menuName = "Game/Classes/Assault")]
    public class AssaultClass : CharacterClassBase
    {
        [Header("Assault Specific")]
        public float sprintSpeedBonus = 1.5f;
        public float weaponSwapSpeedBonus = 1.3f;
        public float criticalHitChance = 0.15f;
        
        private bool isSprintAbilityActive = false;
        private float sprintAbilityTimer = 0f;

        public override void Initialize()
        {
            base.Initialize();
            classType = CharacterClass.Assault;
            className = "Assault";
            description = "Збалансований боєць з високою мобільністю та універсальністю";
            
            healthMultiplier = 1.0f;
            damageMultiplier = 1.1f;
            speedMultiplier = 1.2f;
            armorMultiplier = 0.9f;
            reloadSpeedMultiplier = 1.15f;
            
            abilityCooldown = 25f;
            abilityDuration = 8f;
            abilityName = "Combat Sprint";
            abilityDescription = "Тимчасово збільшує швидкість руху та шанс критичного удару";
        }

        public override void ActivateSpecialAbility()
        {
            if (!isSprintAbilityActive && sprintAbilityTimer <= 0f)
            {
                isSprintAbilityActive = true;
                sprintAbilityTimer = abilityDuration;
                
                // Використовуємо Object Pool для ефектів
                var effectPool = UniversalObjectPool.Instance;
                if (effectPool != null)
                {
                    var sprintEffect = effectPool.Get("SprintEffect");
                    if (sprintEffect != null)
                    {
                        // Налаштування ефекту
                        sprintEffect.transform.position = transform.position;
                        sprintEffect.SetActive(true);
                    }
                }
                
                Debug.Log($"[{className}] Combat Sprint activated!");
                
                // Performance tracking
                performanceMonitor?.TakeSnapshot("AssaultClass_SpecialAbility_Activated");
            }
        }

        public override void UpdateClassStats(PlayerHealth playerHealth, PlayerMovement playerMovement)
        {
            if (playerHealth != null)
            {
                playerHealth.maxHealth = Mathf.RoundToInt(playerHealth.maxHealth * healthMultiplier);
            }
            
            if (playerMovement != null)
            {
                float currentSpeedMultiplier = speedMultiplier;
                if (isSprintAbilityActive)
                {
                    currentSpeedMultiplier *= sprintSpeedBonus;
                }
                playerMovement.walkSpeed *= currentSpeedMultiplier;
                playerMovement.runSpeed *= currentSpeedMultiplier;
            }
            
            // Оновлення таймера здібності
            if (sprintAbilityTimer > 0f)
            {
                sprintAbilityTimer -= Time.deltaTime;
                if (sprintAbilityTimer <= 0f)
                {
                    isSprintAbilityActive = false;
                    Debug.Log($"[{className}] Combat Sprint ended");
                }
            }
        }
    }

    // ================================
    // TANK CLASS - ВАЖКИЙ ЗАХИСНИК
    // ================================

    [CreateAssetMenu(fileName = "Tank_Class", menuName = "Game/Classes/Tank")]
    public class TankClass : CharacterClassBase
    {
        [Header("Tank Specific")]
        public float damageReduction = 0.3f;
        public float healthRegenRate = 2f;
        public float shieldCapacity = 100f;
        
        private bool isShieldActive = false;
        private float currentShield = 0f;
        private float shieldTimer = 0f;

        public override void Initialize()
        {
            base.Initialize();
            classType = CharacterClass.Tank;
            className = "Tank";
            description = "Важкий захисник з високою живучістю та захисними здібностями";
            
            healthMultiplier = 1.5f;
            damageMultiplier = 0.8f;
            speedMultiplier = 0.7f;
            armorMultiplier = 1.8f;
            reloadSpeedMultiplier = 0.9f;
            
            abilityCooldown = 35f;
            abilityDuration = 12f;
            abilityName = "Energy Shield";
            abilityDescription = "Активує енергетичний щит, що поглинає пошкодження";
            
            currentShield = shieldCapacity;
        }

        public override void ActivateSpecialAbility()
        {
            if (!isShieldActive && shieldTimer <= 0f)
            {
                isShieldActive = true;
                shieldTimer = abilityDuration;
                currentShield = shieldCapacity;
                
                // Використовуємо Object Pool для щита
                var effectPool = UniversalObjectPool.Instance;
                if (effectPool != null)
                {
                    var shieldEffect = effectPool.Get("ShieldEffect");
                    if (shieldEffect != null)
                    {
                        shieldEffect.transform.position = transform.position;
                        shieldEffect.SetActive(true);
                    }
                }
                
                Debug.Log($"[{className}] Energy Shield activated! Shield: {currentShield}");
                
                // Performance tracking
                performanceMonitor?.TakeSnapshot("TankClass_SpecialAbility_Activated");
            }
        }

        public override void UpdateClassStats(PlayerHealth playerHealth, PlayerMovement playerMovement)
        {
            if (playerHealth != null)
            {
                playerHealth.maxHealth = Mathf.RoundToInt(playerHealth.maxHealth * healthMultiplier);
                
                // Регенерація здоров'я
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    playerHealth.currentHealth += healthRegenRate * Time.deltaTime;
                    playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth, playerHealth.maxHealth);
                }
            }
            
            if (playerMovement != null)
            {
                playerMovement.walkSpeed *= speedMultiplier;
                playerMovement.runSpeed *= speedMultiplier;
            }
            
            // Оновлення щита
            if (shieldTimer > 0f)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    isShieldActive = false;
                    Debug.Log($"[{className}] Energy Shield deactivated");
                }
            }
        }
        
        public float AbsorbDamage(float incomingDamage)
        {
            if (isShieldActive && currentShield > 0f)
            {
                float absorbedDamage = Mathf.Min(incomingDamage, currentShield);
                currentShield -= absorbedDamage;
                float remainingDamage = incomingDamage - absorbedDamage;
                
                Debug.Log($"[{className}] Shield absorbed {absorbedDamage} damage. Remaining shield: {currentShield}");
                return remainingDamage * (1f - damageReduction);
            }
            
            return incomingDamage * (1f - damageReduction);
        }
    }

    // ================================
    // CHARACTER CLASS MANAGER - HYBRID INTEGRATION
    // ================================

    public class CharacterClassManager : MonoBehaviour
    {
        [Header("Class Configuration")]
        public CharacterClass selectedClass = CharacterClass.Assault;
        public List<CharacterClassBase> availableClasses = new List<CharacterClassBase>();
        
        [Header("Integration References")]
        public PlayerHealth playerHealth;
        public PlayerMovement playerMovement;
        
        // Інтеграція з існуючими системами
        private PerkSystem perkSystem;
        private PerformanceMonitor performanceMonitor;
        private QualityGates qualityGates;
        private UniversalObjectPool objectPool;
        
        // Поточний активний клас
        private CharacterClassBase currentClass;
        
        // Singleton
        public static CharacterClassManager Instance { get; private set; }
        
        // Події для інтеграції
        public static event System.Action<CharacterClass> OnClassChanged;
        public static event System.Action<string> OnAbilityActivated;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeClassSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // Інтеграція з існуючими системами
            perkSystem = PerkSystem.Instance;
            performanceMonitor = PerformanceMonitor.Instance;
            qualityGates = QualityGates.Instance;
            objectPool = UniversalObjectPool.Instance;
            
            // Автоматичне знаходження компонентів
            if (playerHealth == null)
                playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerMovement == null)
                playerMovement = FindObjectOfType<PlayerMovement>();
                
            // Встановлення початкового класу
            SetCharacterClass(selectedClass);
            
            // Performance tracking
            performanceMonitor?.TakeSnapshot("CharacterClassManager_Initialized");
        }

        void Update()
        {
            if (currentClass != null)
            {
                currentClass.UpdateClassStats(playerHealth, playerMovement);
            }
            
            // Перевірка якості кожні 5 секунд
            if (Time.time % 5f < Time.deltaTime)
            {
                ValidateSystemQuality();
            }
        }

        private void InitializeClassSystem()
        {
            // Створення доступних класів
            if (availableClasses.Count == 0)
            {
                // Assault Class
                var assaultClass = ScriptableObject.CreateInstance<AssaultClass>();
                assaultClass.Initialize();
                availableClasses.Add(assaultClass);
                
                // Tank Class
                var tankClass = ScriptableObject.CreateInstance<TankClass>();
                tankClass.Initialize();
                availableClasses.Add(tankClass);
                
                // TODO: Додати інші класи (Sniper, Engineer, Medic) в наступних ітераціях
            }
            
            Debug.Log($"[CharacterClassManager] Initialized with {availableClasses.Count} classes");
        }

        public void SetCharacterClass(CharacterClass newClass)
        {
            var classToSet = availableClasses.FirstOrDefault(c => c.classType == newClass);
            if (classToSet != null)
            {
                currentClass = classToSet;
                selectedClass = newClass;
                
                // Оновлення статистик
                if (currentClass != null)
                {
                    currentClass.UpdateClassStats(playerHealth, playerMovement);
                }
                
                OnClassChanged?.Invoke(newClass);
                Debug.Log($"[CharacterClassManager] Class changed to: {newClass}");
                
                // Performance tracking
                performanceMonitor?.TakeSnapshot($"CharacterClass_Changed_To_{newClass}");
            }
            else
            {
                Debug.LogWarning($"[CharacterClassManager] Class {newClass} not found in available classes!");
            }
        }

        public void ActivateClassAbility()
        {
            if (currentClass != null)
            {
                currentClass.ActivateSpecialAbility();
                OnAbilityActivated?.Invoke(currentClass.abilityName);
                
                // Performance tracking
                performanceMonitor?.TakeSnapshot("ClassAbility_Activated");
            }
        }

        public CharacterClassBase GetCurrentClass()
        {
            return currentClass;
        }

        public List<CharacterClassBase> GetAvailableClasses()
        {
            return new List<CharacterClassBase>(availableClasses);
        }

        private void ValidateSystemQuality()
        {
            if (qualityGates != null)
            {
                bool qualityPassed = qualityGates.ValidateQuality();
                if (!qualityPassed)
                {
                    Debug.LogWarning("[CharacterClassManager] Quality gates failed! Performance may be degraded.");
                }
            }
        }

        // Інтеграція з PerkSystem
        public void ApplyClassPerks()
        {
            if (perkSystem != null && currentClass != null)
            {
                // Логіка застосування перків для поточного класу
                var classSpecificPerks = perkSystem.GetUnlockedPerks()
                    .Where(p => p.name.Contains(currentClass.classType.ToString()))
                    .ToList();
                    
                foreach (var perk in classSpecificPerks)
                {
                    // Застосування перків до класу
                    ApplyPerkToClass(perk);
                }
            }
        }

        private void ApplyPerkToClass(Perk perk)
        {
            // Логіка застосування конкретного перка до поточного класу
            Debug.Log($"[CharacterClassManager] Applying perk {perk.name} to class {currentClass.classType}");
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