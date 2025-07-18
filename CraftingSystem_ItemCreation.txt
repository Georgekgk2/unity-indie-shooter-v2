using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// СИСТЕМА КРАФТИНГУ ПРЕДМЕТІВ - ITEM CREATION SYSTEM
/// Комплексна система створення предметів, зброї та модифікацій
/// Включає рецепти, матеріали, якість предметів та майстерність
/// </summary>

// ================================
// ТИПИ КРАФТИНГУ
// ================================

public enum CraftingCategory
{
    Weapons,        // Зброя
    Armor,          // Броня
    Consumables,    // Витратні предмети
    Modifications,  // Модифікації
    Tools,          // Інструменти
    Ammunition,     // Боєприпаси
    Electronics,    // Електроніка
    Materials       // Матеріали
}

public enum CraftingQuality
{
    Poor,           // Погана якість
    Common,         // Звичайна
    Uncommon,       // Незвичайна
    Rare,           // Рідкісна
    Epic,           // Епічна
    Legendary,      // Легендарна
    Perfect         // Досконала
}

public enum CraftingDifficulty
{
    Novice,         // Новачок
    Apprentice,     // Учень
    Journeyman,     // Підмайстер
    Expert,         // Експерт
    Master,         // Майстер
    Grandmaster     // Грандмайстер
}

// ================================
// БАЗОВІ КЛАСИ
// ================================

[System.Serializable]
public class CraftingMaterial
{
    public string materialId;
    public string materialName;
    public string description;
    public Sprite icon;
    public CraftingQuality quality;
    public int stackSize = 100;
    public float weight = 1f;
    public int value = 10;
    public bool isRare = false;
    
    [Header("Properties")]
    public Dictionary<string, float> properties = new Dictionary<string, float>();
    
    public CraftingMaterial(string id, string name)
    {
        materialId = id;
        materialName = name;
    }
}

[System.Serializable]
public class CraftingRecipe
{
    public string recipeId;
    public string recipeName;
    public string description;
    public CraftingCategory category;
    public CraftingDifficulty difficulty;
    
    [Header("Requirements")]
    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();
    public List<string> requiredTools = new List<string>();
    public int requiredLevel = 1;
    public float craftingTime = 10f;
    
    [Header("Results")]
    public CraftingResult primaryResult;
    public List<CraftingResult> possibleResults = new List<CraftingResult>();
    public List<CraftingResult> byproducts = new List<CraftingResult>();
    
    [Header("Success Rates")]
    public float baseSuccessRate = 0.8f;
    public float criticalSuccessRate = 0.1f;
    public float failureRate = 0.1f;
    
    [Header("Experience")]
    public int experienceReward = 50;
    public CraftingCategory experienceCategory;
    
    public bool CanCraft(CraftingInventory inventory, int playerLevel)
    {
        if (playerLevel < requiredLevel) return false;
        
        foreach (var requirement in requiredMaterials)
        {
            if (!inventory.HasMaterial(requirement.materialId, requirement.quantity))
            {
                return false;
            }
        }
        
        foreach (var tool in requiredTools)
        {
            if (!inventory.HasTool(tool))
            {
                return false;
            }
        }
        
        return true;
    }
}

[System.Serializable]
public class MaterialRequirement
{
    public string materialId;
    public int quantity;
    public CraftingQuality minQuality = CraftingQuality.Common;
    
    public MaterialRequirement(string id, int qty)
    {
        materialId = id;
        quantity = qty;
    }
}

[System.Serializable]
public class CraftingResult
{
    public string itemId;
    public int quantity = 1;
    public float probability = 1f;
    public CraftingQuality quality = CraftingQuality.Common;
    public Dictionary<string, float> bonusProperties = new Dictionary<string, float>();
}

// ================================
// МЕНЕДЖЕР КРАФТИНГУ
// ================================

public class CraftingManager : MonoBehaviour
{
    [Header("Crafting Data")]
    public List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    public List<CraftingMaterial> allMaterials = new List<CraftingMaterial>();
    public List<CraftingStation> craftingStations = new List<CraftingStation>();
    
    [Header("Player Progress")]
    public Dictionary<CraftingCategory, int> craftingLevels = new Dictionary<CraftingCategory, int>();
    public Dictionary<CraftingCategory, int> craftingExperience = new Dictionary<CraftingCategory, int>();
    public List<string> knownRecipes = new List<string>();
    
    [Header("UI References")]
    public CraftingUI craftingUI;
    public CraftingInventory playerInventory;
    
    private Dictionary<string, CraftingRecipe> recipeDatabase = new Dictionary<string, CraftingRecipe>();
    private Dictionary<string, CraftingMaterial> materialDatabase = new Dictionary<string, CraftingMaterial>();
    private CraftingStation currentStation;
    
    public static CraftingManager Instance { get; private set; }
    
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
        InitializeCrafting();
        LoadCraftingProgress();
    }
    
    void InitializeCrafting()
    {
        // Ініціалізація баз даних
        foreach (var recipe in allRecipes)
        {
            recipeDatabase[recipe.recipeId] = recipe;
        }
        
        foreach (var material in allMaterials)
        {
            materialDatabase[material.materialId] = material;
        }
        
        // Ініціалізація рівнів крафтингу
        foreach (CraftingCategory category in System.Enum.GetValues(typeof(CraftingCategory)))
        {
            if (!craftingLevels.ContainsKey(category))
            {
                craftingLevels[category] = 1;
                craftingExperience[category] = 0;
            }
        }
        
        // Ініціалізація інвентарю
        if (playerInventory == null)
        {
            GameObject inventoryObject = new GameObject("CraftingInventory");
            playerInventory = inventoryObject.AddComponent<CraftingInventory>();
        }
        
        playerInventory.Initialize();
    }
    
    public void OpenCraftingStation(CraftingStation station)
    {
        currentStation = station;
        
        if (craftingUI != null)
        {
            List<CraftingRecipe> availableRecipes = GetAvailableRecipes(station);
            craftingUI.OpenCrafting(station, availableRecipes, playerInventory);
        }
    }
    
    List<CraftingRecipe> GetAvailableRecipes(CraftingStation station)
    {
        List<CraftingRecipe> available = new List<CraftingRecipe>();
        
        foreach (var recipe in allRecipes)
        {
            if (station.CanCraftCategory(recipe.category) && 
                knownRecipes.Contains(recipe.recipeId))
            {
                available.Add(recipe);
            }
        }
        
        return available;
    }
    
    public void StartCrafting(string recipeId, int quantity = 1)
    {
        if (!recipeDatabase.ContainsKey(recipeId))
        {
            UIManager.Instance?.ShowNotification("Рецепт не знайдено", NotificationType.Warning);
            return;
        }
        
        CraftingRecipe recipe = recipeDatabase[recipeId];
        
        // Перевірка можливості крафтингу
        if (!CanCraftRecipe(recipe, quantity))
        {
            return;
        }
        
        // Початок процесу крафтингу
        StartCoroutine(CraftingProcess(recipe, quantity));
    }
    
    bool CanCraftRecipe(CraftingRecipe recipe, int quantity)
    {
        int playerLevel = craftingLevels[recipe.category];
        
        if (!recipe.CanCraft(playerInventory, playerLevel))
        {
            UIManager.Instance?.ShowNotification("Недостатньо матеріалів або рівня", NotificationType.Warning);
            return false;
        }
        
        if (currentStation != null && !currentStation.CanCraftCategory(recipe.category))
        {
            UIManager.Instance?.ShowNotification("Ця станція не підтримує цей тип крафтингу", NotificationType.Warning);
            return false;
        }
        
        return true;
    }
    
    IEnumerator CraftingProcess(CraftingRecipe recipe, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            // Споживання матеріалів
            ConsumeMaterials(recipe);
            
            // Показати прогрес крафтингу
            if (craftingUI != null)
            {
                craftingUI.StartCraftingProgress(recipe.craftingTime);
            }
            
            // Очікування завершення крафтингу
            yield return new WaitForSeconds(recipe.craftingTime);
            
            // Визначення результату
            CraftingOutcome outcome = DetermineCraftingOutcome(recipe);
            
            // Створення предмета
            ProcessCraftingOutcome(recipe, outcome);
            
            // Додавання досвіду
            AddCraftingExperience(recipe.experienceCategory, recipe.experienceReward);
            
            // Короткочасна пауза між крафтингами
            if (i < quantity - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        if (craftingUI != null)
        {
            craftingUI.FinishCrafting();
        }
    }
    
    void ConsumeMaterials(CraftingRecipe recipe)
    {
        foreach (var requirement in recipe.requiredMaterials)
        {
            playerInventory.RemoveMaterial(requirement.materialId, requirement.quantity);
        }
    }
    
    CraftingOutcome DetermineCraftingOutcome(CraftingRecipe recipe)
    {
        float playerSkill = GetCraftingSkillBonus(recipe.category);
        float stationBonus = currentStation != null ? currentStation.GetQualityBonus() : 0f;
        float totalBonus = playerSkill + stationBonus;
        
        float roll = Random.Range(0f, 1f);
        
        if (roll <= recipe.criticalSuccessRate + totalBonus * 0.1f)
        {
            return CraftingOutcome.CriticalSuccess;
        }
        else if (roll <= recipe.baseSuccessRate + totalBonus * 0.2f)
        {
            return CraftingOutcome.Success;
        }
        else if (roll <= 1f - recipe.failureRate + totalBonus * 0.1f)
        {
            return CraftingOutcome.PartialSuccess;
        }
        else
        {
            return CraftingOutcome.Failure;
        }
    }
    
    float GetCraftingSkillBonus(CraftingCategory category)
    {
        int level = craftingLevels[category];
        return level * 0.02f; // 2% бонус за рівень
    }
    
    void ProcessCraftingOutcome(CraftingRecipe recipe, CraftingOutcome outcome)
    {
        switch (outcome)
        {
            case CraftingOutcome.CriticalSuccess:
                CreateCriticalSuccessItem(recipe);
                UIManager.Instance?.ShowNotification("КРИТИЧНИЙ УСПІХ!", NotificationType.Success);
                break;
                
            case CraftingOutcome.Success:
                CreateSuccessItem(recipe);
                UIManager.Instance?.ShowNotification("Предмет створено успішно", NotificationType.Success);
                break;
                
            case CraftingOutcome.PartialSuccess:
                CreatePartialSuccessItem(recipe);
                UIManager.Instance?.ShowNotification("Частковий успіх", NotificationType.Warning);
                break;
                
            case CraftingOutcome.Failure:
                CreateFailureResult(recipe);
                UIManager.Instance?.ShowNotification("Крафтинг провалився", NotificationType.Danger);
                break;
        }
        
        // Створення побічних продуктів
        CreateByproducts(recipe);
    }
    
    void CreateCriticalSuccessItem(CraftingRecipe recipe)
    {
        // Створення предмета з підвищеною якістю
        CraftingResult result = recipe.primaryResult;
        CraftingQuality enhancedQuality = EnhanceQuality(result.quality, 2);
        
        CreateCraftedItem(result.itemId, result.quantity + 1, enhancedQuality, true);
    }
    
    void CreateSuccessItem(CraftingRecipe recipe)
    {
        CraftingResult result = recipe.primaryResult;
        CreateCraftedItem(result.itemId, result.quantity, result.quality, false);
    }
    
    void CreatePartialSuccessItem(CraftingRecipe recipe)
    {
        CraftingResult result = recipe.primaryResult;
        CraftingQuality reducedQuality = ReduceQuality(result.quality, 1);
        
        CreateCraftedItem(result.itemId, result.quantity, reducedQuality, false);
    }
    
    void CreateFailureResult(CraftingRecipe recipe)
    {
        // При провалі можна отримати частину матеріалів назад
        foreach (var requirement in recipe.requiredMaterials)
        {
            int returnAmount = Mathf.RoundToInt(requirement.quantity * 0.3f);
            if (returnAmount > 0)
            {
                playerInventory.AddMaterial(requirement.materialId, returnAmount);
            }
        }
    }
    
    void CreateByproducts(CraftingRecipe recipe)
    {
        foreach (var byproduct in recipe.byproducts)
        {
            if (Random.value <= byproduct.probability)
            {
                CreateCraftedItem(byproduct.itemId, byproduct.quantity, byproduct.quality, false);
            }
        }
    }
    
    void CreateCraftedItem(string itemId, int quantity, CraftingQuality quality, bool isCritical)
    {
        // Створення предмета в інвентарі
        CraftedItem item = new CraftedItem
        {
            itemId = itemId,
            quantity = quantity,
            quality = quality,
            isCriticalCraft = isCritical,
            craftedBy = "Player",
            craftedAt = System.DateTime.Now
        };
        
        // Додавання бонусних властивостей залежно від якості
        ApplyQualityBonuses(item);
        
        playerInventory.AddCraftedItem(item);
        
        // Візуальний ефект створення
        CreateCraftingEffect(quality, isCritical);
    }
    
    void ApplyQualityBonuses(CraftedItem item)
    {
        float qualityMultiplier = GetQualityMultiplier(item.quality);
        
        // Застосування бонусів залежно від типу предмета
        if (item.itemId.Contains("weapon"))
        {
            item.bonusProperties["damage"] = qualityMultiplier;
            item.bonusProperties["durability"] = qualityMultiplier;
        }
        else if (item.itemId.Contains("armor"))
        {
            item.bonusProperties["protection"] = qualityMultiplier;
            item.bonusProperties["durability"] = qualityMultiplier;
        }
    }
    
    float GetQualityMultiplier(CraftingQuality quality)
    {
        switch (quality)
        {
            case CraftingQuality.Poor: return 0.7f;
            case CraftingQuality.Common: return 1f;
            case CraftingQuality.Uncommon: return 1.2f;
            case CraftingQuality.Rare: return 1.5f;
            case CraftingQuality.Epic: return 2f;
            case CraftingQuality.Legendary: return 3f;
            case CraftingQuality.Perfect: return 5f;
            default: return 1f;
        }
    }
    
    CraftingQuality EnhanceQuality(CraftingQuality current, int levels)
    {
        int newLevel = (int)current + levels;
        newLevel = Mathf.Clamp(newLevel, 0, (int)CraftingQuality.Perfect);
        return (CraftingQuality)newLevel;
    }
    
    CraftingQuality ReduceQuality(CraftingQuality current, int levels)
    {
        int newLevel = (int)current - levels;
        newLevel = Mathf.Clamp(newLevel, 0, (int)CraftingQuality.Perfect);
        return (CraftingQuality)newLevel;
    }
    
    void CreateCraftingEffect(CraftingQuality quality, bool isCritical)
    {
        GameObject effect = new GameObject("Crafting Effect");
        effect.transform.position = currentStation != null ? currentStation.transform.position : transform.position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        
        if (isCritical)
        {
            main.startColor = Color.gold;
            main.startSpeed = 15f;
        }
        else
        {
            main.startColor = GetQualityColor(quality);
            main.startSpeed = 10f;
        }
        
        main.startLifetime = 2f;
        main.maxParticles = 50;
        
        Destroy(effect, 3f);
    }
    
    Color GetQualityColor(CraftingQuality quality)
    {
        switch (quality)
        {
            case CraftingQuality.Poor: return Color.gray;
            case CraftingQuality.Common: return Color.white;
            case CraftingQuality.Uncommon: return Color.green;
            case CraftingQuality.Rare: return Color.blue;
            case CraftingQuality.Epic: return Color.magenta;
            case CraftingQuality.Legendary: return Color.yellow;
            case CraftingQuality.Perfect: return Color.cyan;
            default: return Color.white;
        }
    }
    
    void AddCraftingExperience(CraftingCategory category, int experience)
    {
        craftingExperience[category] += experience;
        
        // Перевірка підвищення рівня
        int requiredExp = GetRequiredExperience(craftingLevels[category]);
        
        if (craftingExperience[category] >= requiredExp)
        {
            LevelUpCrafting(category);
        }
    }
    
    int GetRequiredExperience(int level)
    {
        return 100 + (level * 50); // Прогресивне збільшення
    }
    
    void LevelUpCrafting(CraftingCategory category)
    {
        craftingLevels[category]++;
        craftingExperience[category] = 0;
        
        UIManager.Instance?.ShowNotification($"Рівень {category} підвищено до {craftingLevels[category]}!", NotificationType.LevelUp);
        
        // Розблокування нових рецептів
        UnlockNewRecipes(category, craftingLevels[category]);
    }
    
    void UnlockNewRecipes(CraftingCategory category, int level)
    {
        foreach (var recipe in allRecipes)
        {
            if (recipe.category == category && 
                recipe.requiredLevel == level && 
                !knownRecipes.Contains(recipe.recipeId))
            {
                LearnRecipe(recipe.recipeId);
            }
        }
    }
    
    public void LearnRecipe(string recipeId)
    {
        if (!knownRecipes.Contains(recipeId))
        {
            knownRecipes.Add(recipeId);
            
            CraftingRecipe recipe = recipeDatabase[recipeId];
            UIManager.Instance?.ShowNotification($"Вивчено рецепт: {recipe.recipeName}", NotificationType.Unlock);
        }
    }
    
    public List<CraftingRecipe> GetKnownRecipes()
    {
        return knownRecipes.Select(id => recipeDatabase[id]).ToList();
    }
    
    public int GetCraftingLevel(CraftingCategory category)
    {
        return craftingLevels.ContainsKey(category) ? craftingLevels[category] : 1;
    }
    
    void LoadCraftingProgress()
    {
        // Завантаження рівнів крафтингу
        foreach (CraftingCategory category in System.Enum.GetValues(typeof(CraftingCategory)))
        {
            craftingLevels[category] = PlayerPrefs.GetInt($"CraftingLevel_{category}", 1);
            craftingExperience[category] = PlayerPrefs.GetInt($"CraftingExp_{category}", 0);
        }
        
        // Завантаження відомих рецептів
        string recipesJson = PlayerPrefs.GetString("KnownRecipes", "");
        if (!string.IsNullOrEmpty(recipesJson))
        {
            knownRecipes = recipesJson.Split(',').ToList();
        }
    }
    
    void SaveCraftingProgress()
    {
        // Збереження рівнів крафтингу
        foreach (var kvp in craftingLevels)
        {
            PlayerPrefs.SetInt($"CraftingLevel_{kvp.Key}", kvp.Value);
        }
        
        foreach (var kvp in craftingExperience)
        {
            PlayerPrefs.SetInt($"CraftingExp_{kvp.Key}", kvp.Value);
        }
        
        // Збереження відомих рецептів
        string recipesJson = string.Join(",", knownRecipes);
        PlayerPrefs.SetString("KnownRecipes", recipesJson);
        
        PlayerPrefs.Save();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCraftingProgress();
        }
    }
}

// ================================
// ДОПОМІЖНІ КЛАСИ
// ================================

public enum CraftingOutcome
{
    CriticalSuccess,    // Критичний успіх
    Success,            // Успіх
    PartialSuccess,     // Частковий успіх
    Failure            // Провал
}

[System.Serializable]
public class CraftedItem
{
    public string itemId;
    public int quantity;
    public CraftingQuality quality;
    public bool isCriticalCraft;
    public string craftedBy;
    public System.DateTime craftedAt;
    public Dictionary<string, float> bonusProperties = new Dictionary<string, float>();
}

public class CraftingStation : MonoBehaviour
{
    [Header("Station Info")]
    public string stationName;
    public List<CraftingCategory> supportedCategories = new List<CraftingCategory>();
    public float qualityBonus = 0.1f;
    public float speedBonus = 0.2f;
    
    [Header("Requirements")]
    public bool requiresPower = false;
    public bool isActive = true;
    
    public bool CanCraftCategory(CraftingCategory category)
    {
        return isActive && supportedCategories.Contains(category);
    }
    
    public float GetQualityBonus()
    {
        return isActive ? qualityBonus : 0f;
    }
    
    public float GetSpeedBonus()
    {
        return isActive ? speedBonus : 0f;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance?.ShowInteractionPrompt($"Використати {stationName}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.Instance?.HideInteractionPrompt();
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            CraftingManager.Instance?.OpenCraftingStation(this);
        }
    }
}