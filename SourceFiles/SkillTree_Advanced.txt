using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// РОЗГАЛУЖЕНЕ ДЕРЕВО НАВИЧОК - ADVANCED SKILL TREE
/// Комплексна система навичок з 5 гілками спеціалізації
/// Включає синергії, престиж навички та динамічне розблокування
/// </summary>

// ================================
// ТИПИ НАВИЧОК
// ================================

public enum SkillCategory
{
    Combat,         // Бойові навички
    Survival,       // Виживання
    Technology,     // Технології
    Leadership,     // Лідерство
    Stealth        // Стелс
}

public enum SkillTier
{
    Basic,          // Базові (рівень 1-10)
    Advanced,       // Просунуті (рівень 11-25)
    Expert,         // Експертні (рівень 26-50)
    Master,         // Майстерські (рівень 51-75)
    Legendary       // Легендарні (рівень 76-100)
}

public enum SkillType
{
    Passive,        // Пасивна навичка
    Active,         // Активна здібність
    Toggle,         // Перемикач
    Upgrade,        // Покращення
    Synergy        // Синергетична
}

// ================================
// БАЗОВИЙ КЛАС НАВИЧКИ
// ================================

[System.Serializable]
public abstract class Skill
{
    [Header("Basic Information")]
    public string skillId;
    public string skillName;
    public string description;
    public Sprite icon;
    
    [Header("Skill Properties")]
    public SkillCategory category;
    public SkillTier tier;
    public SkillType type;
    public int maxLevel = 5;
    public int currentLevel = 0;
    
    [Header("Requirements")]
    public int requiredPlayerLevel = 1;
    public int requiredSkillPoints = 1;
    public List<string> prerequisiteSkills = new List<string>();
    
    [Header("Effects")]
    public List<SkillEffect> effects = new List<SkillEffect>();
    public float cooldown = 0f;
    public float duration = 0f;
    
    [Header("Synergies")]
    public List<string> synergySkills = new List<string>();
    public float synergyBonus = 0.2f;
    
    protected bool isUnlocked = false;
    protected bool isActive = false;
    protected float lastUsedTime = 0f;
    
    public abstract void ApplySkill(PlayerController player);
    public abstract void RemoveSkill(PlayerController player);
    public abstract bool CanUpgrade();
    
    public virtual void LevelUp()
    {
        if (CanUpgrade())
        {
            currentLevel++;
            OnLevelUp();
        }
    }
    
    protected virtual void OnLevelUp()
    {
        // Логіка при підвищенні рівня навички
    }
    
    public virtual bool CanUse()
    {
        return isUnlocked && (Time.time - lastUsedTime >= cooldown);
    }
    
    public virtual void Use(PlayerController player)
    {
        if (!CanUse()) return;
        
        lastUsedTime = Time.time;
        ApplySkill(player);
        
        if (duration > 0f)
        {
            player.StartCoroutine(DeactivateAfterDuration(player));
        }
    }
    
    protected virtual IEnumerator DeactivateAfterDuration(PlayerController player)
    {
        yield return new WaitForSeconds(duration);
        RemoveSkill(player);
    }
    
    public virtual int GetUpgradeCost()
    {
        return currentLevel + 1;
    }
    
    public virtual bool MeetsRequirements(int playerLevel, List<string> unlockedSkills)
    {
        if (playerLevel < requiredPlayerLevel) return false;
        
        foreach (string prereq in prerequisiteSkills)
        {
            if (!unlockedSkills.Contains(prereq)) return false;
        }
        
        return true;
    }
}

// ================================
// КОНКРЕТНІ НАВИЧКИ - COMBAT
// ================================

[CreateAssetMenu(fileName = "WeaponMastery", menuName = "Game/Skills/Combat/Weapon Mastery")]
public class WeaponMasterySkill : Skill
{
    [Header("Weapon Mastery")]
    public float damageBonus = 0.1f;
    public float accuracyBonus = 0.05f;
    public float reloadSpeedBonus = 0.1f;
    
    public WeaponMasterySkill()
    {
        skillId = "weapon_mastery";
        skillName = "Майстерність зброї";
        description = "Покращує урон, точність та швидкість перезарядки";
        category = SkillCategory.Combat;
        tier = SkillTier.Basic;
        type = SkillType.Passive;
        maxLevel = 5;
    }
    
    public override void ApplySkill(PlayerController player)
    {
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            float levelMultiplier = currentLevel;
            weaponController.damageMultiplier *= (1f + damageBonus * levelMultiplier);
            weaponController.accuracy *= (1f + accuracyBonus * levelMultiplier);
            weaponController.reloadSpeed *= (1f + reloadSpeedBonus * levelMultiplier);
        }
    }
    
    public override void RemoveSkill(PlayerController player)
    {
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            float levelMultiplier = currentLevel;
            weaponController.damageMultiplier /= (1f + damageBonus * levelMultiplier);
            weaponController.accuracy /= (1f + accuracyBonus * levelMultiplier);
            weaponController.reloadSpeed /= (1f + reloadSpeedBonus * levelMultiplier);
        }
    }
    
    public override bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
}

[CreateAssetMenu(fileName = "BerserkerRage", menuName = "Game/Skills/Combat/Berserker Rage")]
public class BerserkerRageSkill : Skill
{
    [Header("Berserker Rage")]
    public float damageMultiplier = 1.5f;
    public float speedMultiplier = 1.3f;
    public float healthThreshold = 0.3f;
    
    public BerserkerRageSkill()
    {
        skillId = "berserker_rage";
        skillName = "Лють берсерка";
        description = "При низькому здоров'ї активується режим люті";
        category = SkillCategory.Combat;
        tier = SkillTier.Advanced;
        type = SkillType.Toggle;
        duration = 10f;
        cooldown = 60f;
        requiredPlayerLevel = 15;
        prerequisiteSkills.Add("weapon_mastery");
    }
    
    public override void ApplySkill(PlayerController player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.GetHealthPercentage() <= healthThreshold)
        {
            isActive = true;
            
            // Збільшення урону та швидкості
            WeaponController weaponController = player.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.damageMultiplier *= damageMultiplier;
            }
            
            player.movementSpeed *= speedMultiplier;
            
            // Візуальний ефект
            CreateBerserkerEffect(player);
        }
    }
    
    public override void RemoveSkill(PlayerController player)
    {
        if (!isActive) return;
        
        isActive = false;
        
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.damageMultiplier /= damageMultiplier;
        }
        
        player.movementSpeed /= speedMultiplier;
    }
    
    void CreateBerserkerEffect(PlayerController player)
    {
        GameObject effect = new GameObject("Berserker Effect");
        effect.transform.SetParent(player.transform);
        effect.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.red;
        main.startLifetime = duration;
        main.startSpeed = 5f;
        main.loop = true;
        
        Object.Destroy(effect, duration);
    }
    
    public override bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
}

// ================================
// НАВИЧКИ ВИЖИВАННЯ
// ================================

[CreateAssetMenu(fileName = "HealthRegeneration", menuName = "Game/Skills/Survival/Health Regeneration")]
public class HealthRegenerationSkill : Skill
{
    [Header("Health Regeneration")]
    public float regenRate = 2f;
    public float regenInterval = 1f;
    
    public HealthRegenerationSkill()
    {
        skillId = "health_regen";
        skillName = "Регенерація здоров'я";
        description = "Поступове відновлення здоров'я";
        category = SkillCategory.Survival;
        tier = SkillTier.Basic;
        type = SkillType.Passive;
        maxLevel = 5;
    }
    
    public override void ApplySkill(PlayerController player)
    {
        HealthRegeneration regenComponent = player.GetComponent<HealthRegeneration>();
        if (regenComponent == null)
        {
            regenComponent = player.gameObject.AddComponent<HealthRegeneration>();
        }
        
        regenComponent.regenRate = regenRate * currentLevel;
        regenComponent.regenInterval = regenInterval;
        regenComponent.enabled = true;
    }
    
    public override void RemoveSkill(PlayerController player)
    {
        HealthRegeneration regenComponent = player.GetComponent<HealthRegeneration>();
        if (regenComponent != null)
        {
            regenComponent.enabled = false;
        }
    }
    
    public override bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
}

// ================================
// ТЕХНОЛОГІЧНІ НАВИЧКИ
// ================================

[CreateAssetMenu(fileName = "HackingExpertise", menuName = "Game/Skills/Technology/Hacking")]
public class HackingExpertiseSkill : Skill
{
    [Header("Hacking")]
    public float hackingSpeed = 1.5f;
    public float hackingRange = 2f;
    
    public HackingExpertiseSkill()
    {
        skillId = "hacking_expertise";
        skillName = "Експертиза хакінгу";
        description = "Швидший хакінг електронних систем";
        category = SkillCategory.Technology;
        tier = SkillTier.Advanced;
        type = SkillType.Passive;
        requiredPlayerLevel = 20;
    }
    
    public override void ApplySkill(PlayerController player)
    {
        HackingSystem hackingSystem = player.GetComponent<HackingSystem>();
        if (hackingSystem == null)
        {
            hackingSystem = player.gameObject.AddComponent<HackingSystem>();
        }
        
        hackingSystem.hackingSpeed = hackingSpeed;
        hackingSystem.hackingRange = hackingRange;
        hackingSystem.enabled = true;
    }
    
    public override void RemoveSkill(PlayerController player)
    {
        HackingSystem hackingSystem = player.GetComponent<HackingSystem>();
        if (hackingSystem != null)
        {
            hackingSystem.enabled = false;
        }
    }
    
    public override bool CanUpgrade()
    {
        return currentLevel < maxLevel;
    }
}

// ================================
// МЕНЕДЖЕР ДЕРЕВА НАВИЧОК
// ================================

public class SkillTreeManager : MonoBehaviour
{
    [Header("Skill Tree Data")]
    public List<Skill> allSkills = new List<Skill>();
    public List<string> unlockedSkills = new List<string>();
    
    [Header("Player Progress")]
    public int availableSkillPoints = 0;
    public int totalSkillPoints = 0;
    public int playerLevel = 1;
    
    [Header("UI References")]
    public SkillTreeUI skillTreeUI;
    
    private Dictionary<SkillCategory, List<Skill>> skillsByCategory = new Dictionary<SkillCategory, List<Skill>>();
    private Dictionary<string, Skill> skillsById = new Dictionary<string, Skill>();
    
    public static SkillTreeManager Instance { get; private set; }
    
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
        InitializeSkillTree();
        LoadSkillProgress();
    }
    
    void InitializeSkillTree()
    {
        // Організація навичок за категоріями
        foreach (var skill in allSkills)
        {
            if (!skillsByCategory.ContainsKey(skill.category))
            {
                skillsByCategory[skill.category] = new List<Skill>();
            }
            
            skillsByCategory[skill.category].Add(skill);
            skillsById[skill.skillId] = skill;
        }
        
        // Ініціалізація UI
        if (skillTreeUI != null)
        {
            skillTreeUI.Initialize(skillsByCategory);
        }
    }
    
    public bool UnlockSkill(string skillId)
    {
        if (!skillsById.ContainsKey(skillId)) return false;
        
        Skill skill = skillsById[skillId];
        
        // Перевірка вимог
        if (!skill.MeetsRequirements(playerLevel, unlockedSkills))
        {
            UIManager.Instance?.ShowNotification("Не виконані вимоги для навички", NotificationType.Warning);
            return false;
        }
        
        // Перевірка очок навичок
        int cost = skill.GetUpgradeCost();
        if (availableSkillPoints < cost)
        {
            UIManager.Instance?.ShowNotification("Недостатньо очок навичок", NotificationType.Warning);
            return false;
        }
        
        // Розблокування навички
        skill.LevelUp();
        availableSkillPoints -= cost;
        
        if (!unlockedSkills.Contains(skillId))
        {
            unlockedSkills.Add(skillId);
        }
        
        // Застосування навички до гравця
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            skill.ApplySkill(player);
        }
        
        // Перевірка синергій
        CheckSkillSynergies(skill);
        
        // Збереження прогресу
        SaveSkillProgress();
        
        UIManager.Instance?.ShowNotification($"Розблоковано: {skill.skillName}", NotificationType.Success);
        return true;
    }
    
    void CheckSkillSynergies(Skill skill)
    {
        foreach (string synergySkillId in skill.synergySkills)
        {
            if (unlockedSkills.Contains(synergySkillId))
            {
                ApplySynergy(skill, skillsById[synergySkillId]);
            }
        }
    }
    
    void ApplySynergy(Skill skill1, Skill skill2)
    {
        float synergyBonus = (skill1.synergyBonus + skill2.synergyBonus) / 2f;
        
        UIManager.Instance?.ShowNotification($"СИНЕРГІЯ: {skill1.skillName} + {skill2.skillName}", NotificationType.Special);
        
        // Додаткові ефекти синергії
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            ApplySynergyEffects(player, skill1, skill2, synergyBonus);
        }
    }
    
    void ApplySynergyEffects(PlayerController player, Skill skill1, Skill skill2, float bonus)
    {
        // Логіка синергетичних ефектів
        if (skill1.category == SkillCategory.Combat && skill2.category == SkillCategory.Technology)
        {
            // Синергія бою та технологій
            WeaponController weaponController = player.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.damageMultiplier *= (1f + bonus);
            }
        }
    }
    
    public void AddSkillPoints(int points)
    {
        availableSkillPoints += points;
        totalSkillPoints += points;
        
        if (skillTreeUI != null)
        {
            skillTreeUI.UpdateSkillPoints(availableSkillPoints);
        }
    }
    
    public void OnPlayerLevelUp(int newLevel)
    {
        playerLevel = newLevel;
        
        // Додавання очок навичок при підвищенні рівня
        int skillPointsGained = 2; // 2 очки за рівень
        AddSkillPoints(skillPointsGained);
        
        // Оновлення доступних навичок
        UpdateAvailableSkills();
    }
    
    void UpdateAvailableSkills()
    {
        if (skillTreeUI != null)
        {
            skillTreeUI.UpdateAvailableSkills(playerLevel, unlockedSkills);
        }
    }
    
    public List<Skill> GetSkillsByCategory(SkillCategory category)
    {
        return skillsByCategory.ContainsKey(category) ? skillsByCategory[category] : new List<Skill>();
    }
    
    public Skill GetSkillById(string skillId)
    {
        return skillsById.ContainsKey(skillId) ? skillsById[skillId] : null;
    }
    
    public bool IsSkillUnlocked(string skillId)
    {
        return unlockedSkills.Contains(skillId);
    }
    
    public int GetSkillLevel(string skillId)
    {
        Skill skill = GetSkillById(skillId);
        return skill != null ? skill.currentLevel : 0;
    }
    
    void SaveSkillProgress()
    {
        // Збереження розблокованих навичок
        string unlockedJson = string.Join(",", unlockedSkills);
        PlayerPrefs.SetString("UnlockedSkills", unlockedJson);
        
        // Збереження рівнів навичок
        foreach (var skill in allSkills)
        {
            PlayerPrefs.SetInt($"Skill_{skill.skillId}_Level", skill.currentLevel);
        }
        
        PlayerPrefs.SetInt("AvailableSkillPoints", availableSkillPoints);
        PlayerPrefs.SetInt("TotalSkillPoints", totalSkillPoints);
        PlayerPrefs.Save();
    }
    
    void LoadSkillProgress()
    {
        // Завантаження розблокованих навичок
        string unlockedJson = PlayerPrefs.GetString("UnlockedSkills", "");
        if (!string.IsNullOrEmpty(unlockedJson))
        {
            unlockedSkills = unlockedJson.Split(',').ToList();
        }
        
        // Завантаження рівнів навичок
        foreach (var skill in allSkills)
        {
            skill.currentLevel = PlayerPrefs.GetInt($"Skill_{skill.skillId}_Level", 0);
            
            // Застосування навички, якщо вона розблокована
            if (unlockedSkills.Contains(skill.skillId))
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    skill.ApplySkill(player);
                }
            }
        }
        
        availableSkillPoints = PlayerPrefs.GetInt("AvailableSkillPoints", 0);
        totalSkillPoints = PlayerPrefs.GetInt("TotalSkillPoints", 0);
    }
    
    public void ResetSkillTree()
    {
        // Скидання всіх навичок
        foreach (var skill in allSkills)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                skill.RemoveSkill(player);
            }
            
            skill.currentLevel = 0;
        }
        
        // Повернення очок навичок
        availableSkillPoints = totalSkillPoints;
        unlockedSkills.Clear();
        
        SaveSkillProgress();
        
        UIManager.Instance?.ShowNotification("Дерево навичок скинуто", NotificationType.Info);
    }
}

// ================================
// ДОПОМІЖНІ КОМПОНЕНТИ
// ================================

public class HealthRegeneration : MonoBehaviour
{
    public float regenRate = 2f;
    public float regenInterval = 1f;
    
    void Start()
    {
        StartCoroutine(RegenerationCoroutine());
    }
    
    IEnumerator RegenerationCoroutine()
    {
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        
        while (enabled)
        {
            yield return new WaitForSeconds(regenInterval);
            
            if (playerHealth != null && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(regenRate);
            }
        }
    }
}

public class HackingSystem : MonoBehaviour
{
    public float hackingSpeed = 1.5f;
    public float hackingRange = 2f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            TryHackNearbyObjects();
        }
    }
    
    void TryHackNearbyObjects()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, hackingRange);
        
        foreach (var obj in nearbyObjects)
        {
            HackableObject hackable = obj.GetComponent<HackableObject>();
            if (hackable != null && !hackable.isHacked)
            {
                StartCoroutine(HackObject(hackable));
                break;
            }
        }
    }
    
    IEnumerator HackObject(HackableObject target)
    {
        float hackTime = target.hackingTime / hackingSpeed;
        float elapsed = 0f;
        
        while (elapsed < hackTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / hackTime;
            
            UIManager.Instance?.UpdateHackingProgress(progress);
            
            yield return null;
        }
        
        target.OnHacked();
        UIManager.Instance?.HideHackingProgress();
    }
}

[System.Serializable]
public class SkillEffect
{
    public string effectName;
    public float value;
    public EffectType type;
}

public enum EffectType
{
    Additive,       // Додавання
    Multiplicative, // Множення
    Override        // Заміна
}