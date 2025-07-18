using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// СИСТЕМА КЛАСІВ ПЕРСОНАЖА - CHARACTER SPECIALIZATION
/// Включає 5 унікальних класів з різними стилями гри та прогресією
/// Інтегрується з існуючою системою перків та досягнень
/// </summary>

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
    public float accuracyMultiplier = 1f;
    public float reloadSpeedMultiplier = 1f;
    
    [Header("Class Progression")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public ClassTier currentTier = ClassTier.Novice;
    
    [Header("Unlocked Abilities")]
    public List<ClassAbility> unlockedAbilities = new List<ClassAbility>();
    public List<ClassPerk> unlockedPerks = new List<ClassPerk>();
    
    // Абстрактні методи для кожного класу
    public abstract void ApplyClassBonuses(PlayerController player);
    public abstract void OnLevelUp(int newLevel);
    public abstract ClassAbility GetSignatureAbility();
    public abstract List<WeaponType> GetPreferredWeapons();
}

// ================================
// КЛАС 1: ASSAULT (ШТУРМОВИК)
// ================================

[CreateAssetMenu(fileName = "Assault_Class", menuName = "Game/Classes/Assault")]
public class AssaultClass : CharacterClassBase
{
    [Header("Assault Specific")]
    public float weaponSwitchSpeedBonus = 0.3f;
    public float movementSpeedBonus = 0.2f;
    public float criticalHitChance = 0.15f;
    
    public AssaultClass()
    {
        classType = CharacterClass.Assault;
        className = "Штурмовик";
        description = "Збалансований боєць з високою мобільністю та універсальністю";
        
        healthMultiplier = 1f;
        damageMultiplier = 1.1f;
        speedMultiplier = 1.2f;
        accuracyMultiplier = 1f;
        reloadSpeedMultiplier = 1.1f;
    }
    
    public override void ApplyClassBonuses(PlayerController player)
    {
        // Застосування бонусів штурмовика
        player.maxHealth *= healthMultiplier;
        player.movementSpeed *= speedMultiplier;
        
        // Бонус до швидкості зміни зброї
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.weaponSwitchSpeed *= (1f + weaponSwitchSpeedBonus);
        }
    }
    
    public override void OnLevelUp(int newLevel)
    {
        currentLevel = newLevel;
        
        // Розблокування здібностей залежно від рівня
        if (newLevel == 5 && !HasAbility("Sprint"))
        {
            UnlockAbility(new SprintAbility());
        }
        
        if (newLevel == 15 && !HasAbility("DoubleShot"))
        {
            UnlockAbility(new DoubleShotAbility());
        }
        
        if (newLevel == 30 && !HasAbility("Adrenaline"))
        {
            UnlockAbility(new AdrenalineAbility());
        }
        
        UpdateTier();
    }
    
    public override ClassAbility GetSignatureAbility()
    {
        return new AdrenalineAbility();
    }
    
    public override List<WeaponType> GetPreferredWeapons()
    {
        return new List<WeaponType>
        {
            WeaponType.AssaultRifle,
            WeaponType.SMG,
            WeaponType.Pistol
        };
    }
    
    bool HasAbility(string abilityName)
    {
        return unlockedAbilities.Exists(a => a.abilityName == abilityName);
    }
    
    void UnlockAbility(ClassAbility ability)
    {
        unlockedAbilities.Add(ability);
        UIManager.Instance?.ShowNotification($"РОЗБЛОКОВАНО: {ability.abilityName}", NotificationType.Success);
    }
    
    void UpdateTier()
    {
        if (currentLevel >= 76) currentTier = ClassTier.Legend;
        else if (currentLevel >= 51) currentTier = ClassTier.Master;
        else if (currentLevel >= 26) currentTier = ClassTier.Expert;
        else if (currentLevel >= 11) currentTier = ClassTier.Veteran;
        else currentTier = ClassTier.Novice;
    }
}

// ================================
// КЛАС 2: TANK (ЗАХИСНИК)
// ================================

[CreateAssetMenu(fileName = "Tank_Class", menuName = "Game/Classes/Tank")]
public class TankClass : CharacterClassBase
{
    [Header("Tank Specific")]
    public float armorBonus = 0.5f;
    public float damageReduction = 0.3f;
    public float healthRegenRate = 2f;
    
    public TankClass()
    {
        classType = CharacterClass.Tank;
        className = "Захисник";
        description = "Важко бронований боєць з високою живучістю";
        
        healthMultiplier = 1.8f;
        damageMultiplier = 0.9f;
        speedMultiplier = 0.8f;
        accuracyMultiplier = 0.9f;
        reloadSpeedMultiplier = 0.9f;
    }
    
    public override void ApplyClassBonuses(PlayerController player)
    {
        player.maxHealth *= healthMultiplier;
        player.movementSpeed *= speedMultiplier;
        
        // Додавання компонента броні
        TankArmor armor = player.gameObject.GetComponent<TankArmor>();
        if (armor == null)
        {
            armor = player.gameObject.AddComponent<TankArmor>();
        }
        
        armor.damageReduction = damageReduction;
        armor.healthRegenRate = healthRegenRate;
    }
    
    public override void OnLevelUp(int newLevel)
    {
        currentLevel = newLevel;
        
        if (newLevel == 5 && !HasAbility("Shield"))
        {
            UnlockAbility(new ShieldAbility());
        }
        
        if (newLevel == 15 && !HasAbility("Taunt"))
        {
            UnlockAbility(new TauntAbility());
        }
        
        if (newLevel == 30 && !HasAbility("Fortress"))
        {
            UnlockAbility(new FortressAbility());
        }
        
        UpdateTier();
    }
    
    public override ClassAbility GetSignatureAbility()
    {
        return new FortressAbility();
    }
    
    public override List<WeaponType> GetPreferredWeapons()
    {
        return new List<WeaponType>
        {
            WeaponType.Shotgun,
            WeaponType.HeavyMachineGun,
            WeaponType.RocketLauncher
        };
    }
    
    bool HasAbility(string abilityName)
    {
        return unlockedAbilities.Exists(a => a.abilityName == abilityName);
    }
    
    void UnlockAbility(ClassAbility ability)
    {
        unlockedAbilities.Add(ability);
        UIManager.Instance?.ShowNotification($"РОЗБЛОКОВАНО: {ability.abilityName}", NotificationType.Success);
    }
    
    void UpdateTier()
    {
        if (currentLevel >= 76) currentTier = ClassTier.Legend;
        else if (currentLevel >= 51) currentTier = ClassTier.Master;
        else if (currentLevel >= 26) currentTier = ClassTier.Expert;
        else if (currentLevel >= 11) currentTier = ClassTier.Veteran;
        else currentTier = ClassTier.Novice;
    }
}

// ================================
// СИСТЕМА ЗДІБНОСТЕЙ КЛАСІВ
// ================================

[System.Serializable]
public abstract class ClassAbility
{
    public string abilityName;
    public string description;
    public Sprite abilityIcon;
    public float cooldownTime;
    public float duration;
    public int energyCost;
    
    protected float lastUsedTime;
    
    public bool CanUse()
    {
        return Time.time - lastUsedTime >= cooldownTime;
    }
    
    public abstract void Activate(PlayerController player);
    public abstract void Deactivate(PlayerController player);
    
    protected void StartCooldown()
    {
        lastUsedTime = Time.time;
    }
}

// Здібність спринту для штурмовика
public class SprintAbility : ClassAbility
{
    public SprintAbility()
    {
        abilityName = "Спринт";
        description = "Збільшує швидкість руху на 50% протягом 8 секунд";
        cooldownTime = 20f;
        duration = 8f;
        energyCost = 25;
    }
    
    public override void Activate(PlayerController player)
    {
        player.movementSpeed *= 1.5f;
        StartCooldown();
        
        // Автоматична деактивація через duration
        player.StartCoroutine(DeactivateAfterDuration(player));
    }
    
    public override void Deactivate(PlayerController player)
    {
        player.movementSpeed /= 1.5f;
    }
    
    System.Collections.IEnumerator DeactivateAfterDuration(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        Deactivate(player);
    }
}

// Здібність щита для танка
public class ShieldAbility : ClassAbility
{
    public ShieldAbility()
    {
        abilityName = "Енергетичний щит";
        description = "Блокує 90% урону протягом 5 секунд";
        cooldownTime = 30f;
        duration = 5f;
        energyCost = 40;
    }
    
    public override void Activate(PlayerController player)
    {
        TankArmor armor = player.GetComponent<TankArmor>();
        if (armor != null)
        {
            armor.shieldActive = true;
            armor.shieldStrength = 0.9f;
        }
        
        StartCooldown();
        player.StartCoroutine(DeactivateAfterDuration(player));
    }
    
    public override void Deactivate(PlayerController player)
    {
        TankArmor armor = player.GetComponent<TankArmor>();
        if (armor != null)
        {
            armor.shieldActive = false;
        }
    }
    
    System.Collections.IEnumerator DeactivateAfterDuration(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        Deactivate(player);
    }
}

// ================================
// ДОПОМІЖНІ КОМПОНЕНТИ
// ================================

public class TankArmor : MonoBehaviour
{
    public float damageReduction = 0.3f;
    public float healthRegenRate = 2f;
    public bool shieldActive = false;
    public float shieldStrength = 0f;
    
    void Start()
    {
        // Запуск регенерації здоров'я
        StartCoroutine(HealthRegeneration());
    }
    
    System.Collections.IEnumerator HealthRegeneration()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        
        while (true)
        {
            yield return new WaitForSeconds(1f);
            
            if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(healthRegenRate);
            }
        }
    }
    
    public float CalculateDamageReduction(float incomingDamage)
    {
        float finalDamage = incomingDamage;
        
        // Базове зменшення урону
        finalDamage *= (1f - damageReduction);
        
        // Додаткове зменшення від щита
        if (shieldActive)
        {
            finalDamage *= (1f - shieldStrength);
        }
        
        return finalDamage;
    }
}

// ================================
// МЕНЕДЖЕР КЛАСІВ
// ================================

public class CharacterClassManager : MonoBehaviour
{
    [Header("Available Classes")]
    public AssaultClass assaultClass;
    public TankClass tankClass;
    // Додати інші класи тут
    
    [Header("Current Player")]
    public CharacterClass currentClass = CharacterClass.Assault;
    public CharacterClassBase activeClassData;
    
    public static CharacterClassManager Instance { get; private set; }
    
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
        InitializeClass(currentClass);
    }
    
    public void InitializeClass(CharacterClass classType)
    {
        currentClass = classType;
        
        switch (classType)
        {
            case CharacterClass.Assault:
                activeClassData = assaultClass;
                break;
            case CharacterClass.Tank:
                activeClassData = tankClass;
                break;
            // Додати інші класи
        }
        
        ApplyClassToPlayer();
    }
    
    void ApplyClassToPlayer()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && activeClassData != null)
        {
            activeClassData.ApplyClassBonuses(player);
        }
    }
    
    public void AddExperience(int experience)
    {
        if (activeClassData == null) return;
        
        activeClassData.currentExperience += experience;
        
        // Перевірка на підвищення рівня
        int requiredExp = GetRequiredExperience(activeClassData.currentLevel);
        
        if (activeClassData.currentExperience >= requiredExp)
        {
            LevelUp();
        }
    }
    
    void LevelUp()
    {
        activeClassData.currentLevel++;
        activeClassData.currentExperience = 0;
        
        activeClassData.OnLevelUp(activeClassData.currentLevel);
        
        UIManager.Instance?.ShowNotification($"РІВЕНЬ ПІДВИЩЕНО! {activeClassData.className} LVL {activeClassData.currentLevel}", NotificationType.LevelUp);
    }
    
    int GetRequiredExperience(int level)
    {
        return 100 + (level * 50); // Прогресивне збільшення
    }
    
    public bool CanUseAbility(string abilityName)
    {
        if (activeClassData == null) return false;
        
        ClassAbility ability = activeClassData.unlockedAbilities.Find(a => a.abilityName == abilityName);
        return ability != null && ability.CanUse();
    }
    
    public void UseAbility(string abilityName)
    {
        if (!CanUseAbility(abilityName)) return;
        
        ClassAbility ability = activeClassData.unlockedAbilities.Find(a => a.abilityName == abilityName);
        PlayerController player = FindObjectOfType<PlayerController>();
        
        if (ability != null && player != null)
        {
            ability.Activate(player);
        }
    }
}