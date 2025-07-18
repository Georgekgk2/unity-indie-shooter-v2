using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// СИСТЕМА МОДИФІКАЦІЙ ЗБРОЇ - WEAPON MODIFICATION SYSTEM
/// Дозволяє гравцям кастомізувати зброю з різними модифікаціями
/// Включає систему крафтингу, рідкісність модів та синергії
/// </summary>

// ================================
// ТИПИ МОДИФІКАЦІЙ
// ================================

public enum ModificationType
{
    Barrel,         // Ствол (точність, дальність)
    Scope,          // Приціл (zoom, точність)
    Grip,           // Рукоятка (стабільність, швидкість)
    Magazine,       // Магазин (ємність, перезарядка)
    Stock,          // Приклад (віддача, стабільність)
    Muzzle,         // Дульний пристрій (урон, звук)
    Trigger,        // Спусковий механізм (швидкість стрільби)
    Ammunition,     // Боєприпаси (урон, ефекти)
    Coating,        // Покриття (спеціальні ефекти)
    Core           // Ядро (фундаментальні зміни)
}

public enum ModificationRarity
{
    Common,         // Звичайна (біла)
    Uncommon,       // Незвичайна (зелена)
    Rare,           // Рідкісна (синя)
    Epic,           // Епічна (фіолетова)
    Legendary,      // Легендарна (помаранчева)
    Mythic          // Міфічна (червона)
}

public enum ModificationEffect
{
    DamageIncrease,     // Збільшення урону
    AccuracyBoost,      // Покращення точності
    RangeExtension,     // Збільшення дальності
    ReloadSpeedUp,      // Прискорення перезарядки
    FireRateBoost,      // Збільшення швидкості стрільби
    RecoilReduction,    // Зменшення віддачі
    MagazineExpansion,  // Збільшення магазину
    CriticalChance,     // Шанс критичного урону
    ElementalDamage,    // Елементальний урон
    ArmorPenetration,   // Пробиття броні
    SilentShots,        // Безшумні постріли
    ExplosiveRounds,    // Вибухові снаряди
    PoisonCoating,      // Отруйне покриття
    EnergyAmplifier,    // Енергетичний підсилювач
    TimeDistortion      // Спотворення часу
}

// ================================
// БАЗОВИЙ КЛАС МОДИФІКАЦІЇ
// ================================

[System.Serializable]
public abstract class WeaponModification
{
    [Header("Basic Information")]
    public string modificationName;
    public string description;
    public ModificationType type;
    public ModificationRarity rarity;
    public Sprite icon;
    
    [Header("Effects")]
    public List<ModificationEffect> effects = new List<ModificationEffect>();
    public Dictionary<string, float> statModifiers = new Dictionary<string, float>();
    
    [Header("Requirements")]
    public List<WeaponType> compatibleWeapons = new List<WeaponType>();
    public int requiredLevel = 1;
    public List<string> requiredMaterials = new List<string>();
    
    [Header("Synergies")]
    public List<string> synergyMods = new List<string>();
    public float synergyBonus = 0.2f;
    
    [Header("Visual")]
    public GameObject visualPrefab;
    public Material weaponMaterial;
    public ParticleSystem effectParticles;
    
    protected bool isInstalled = false;
    protected WeaponBase attachedWeapon;
    
    public abstract void ApplyModification(WeaponBase weapon);
    public abstract void RemoveModification(WeaponBase weapon);
    public abstract bool IsCompatible(WeaponBase weapon);
    
    public virtual float GetRarityMultiplier()
    {
        switch (rarity)
        {
            case ModificationRarity.Common: return 1f;
            case ModificationRarity.Uncommon: return 1.2f;
            case ModificationRarity.Rare: return 1.5f;
            case ModificationRarity.Epic: return 2f;
            case ModificationRarity.Legendary: return 3f;
            case ModificationRarity.Mythic: return 5f;
            default: return 1f;
        }
    }
    
    public virtual Color GetRarityColor()
    {
        switch (rarity)
        {
            case ModificationRarity.Common: return Color.white;
            case ModificationRarity.Uncommon: return Color.green;
            case ModificationRarity.Rare: return Color.blue;
            case ModificationRarity.Epic: return Color.magenta;
            case ModificationRarity.Legendary: return Color.yellow;
            case ModificationRarity.Mythic: return Color.red;
            default: return Color.white;
        }
    }
}

// ================================
// КОНКРЕТНІ МОДИФІКАЦІЇ
// ================================

[CreateAssetMenu(fileName = "ExtendedBarrel", menuName = "Game/Modifications/Barrel/Extended")]
public class ExtendedBarrelMod : WeaponModification
{
    [Header("Barrel Specific")]
    public float rangeIncrease = 0.3f;
    public float accuracyBonus = 0.2f;
    public float damageBonus = 0.15f;
    
    public ExtendedBarrelMod()
    {
        modificationName = "Подовжений ствол";
        description = "Збільшує дальність та точність зброї";
        type = ModificationType.Barrel;
        rarity = ModificationRarity.Uncommon;
        
        effects.Add(ModificationEffect.RangeExtension);
        effects.Add(ModificationEffect.AccuracyBoost);
        effects.Add(ModificationEffect.DamageIncrease);
        
        compatibleWeapons.Add(WeaponType.AssaultRifle);
        compatibleWeapons.Add(WeaponType.SniperRifle);
        compatibleWeapons.Add(WeaponType.DMR);
    }
    
    public override void ApplyModification(WeaponBase weapon)
    {
        if (!IsCompatible(weapon)) return;
        
        attachedWeapon = weapon;
        isInstalled = true;
        
        // Застосування модифікацій
        weapon.range *= (1f + rangeIncrease * GetRarityMultiplier());
        weapon.accuracy *= (1f + accuracyBonus * GetRarityMultiplier());
        weapon.damage *= (1f + damageBonus * GetRarityMultiplier());
        
        // Візуальні зміни
        ApplyVisualChanges(weapon);
        
        // Лог для дебагу
        Debug.Log($"Встановлено {modificationName} на {weapon.weaponName}");
    }
    
    public override void RemoveModification(WeaponBase weapon)
    {
        if (!isInstalled || attachedWeapon != weapon) return;
        
        // Видалення модифікацій
        weapon.range /= (1f + rangeIncrease * GetRarityMultiplier());
        weapon.accuracy /= (1f + accuracyBonus * GetRarityMultiplier());
        weapon.damage /= (1f + damageBonus * GetRarityMultiplier());
        
        // Видалення візуальних змін
        RemoveVisualChanges(weapon);
        
        isInstalled = false;
        attachedWeapon = null;
        
        Debug.Log($"Видалено {modificationName} з {weapon.weaponName}");
    }
    
    public override bool IsCompatible(WeaponBase weapon)
    {
        return compatibleWeapons.Contains(weapon.weaponType);
    }
    
    void ApplyVisualChanges(WeaponBase weapon)
    {
        if (visualPrefab != null)
        {
            GameObject visual = Object.Instantiate(visualPrefab, weapon.transform);
            visual.name = $"{modificationName}_Visual";
        }
        
        if (weaponMaterial != null)
        {
            Renderer weaponRenderer = weapon.GetComponent<Renderer>();
            if (weaponRenderer != null)
            {
                weaponRenderer.material = weaponMaterial;
            }
        }
    }
    
    void RemoveVisualChanges(WeaponBase weapon)
    {
        Transform visual = weapon.transform.Find($"{modificationName}_Visual");
        if (visual != null)
        {
            Object.Destroy(visual.gameObject);
        }
    }
}

[CreateAssetMenu(fileName = "HolographicScope", menuName = "Game/Modifications/Scope/Holographic")]
public class HolographicScopeMod : WeaponModification
{
    [Header("Scope Specific")]
    public float zoomLevel = 2f;
    public float accuracyBonus = 0.4f;
    public float aimSpeedBonus = 0.2f;
    public bool hasNightVision = false;
    
    public HolographicScopeMod()
    {
        modificationName = "Голографічний приціл";
        description = "Покращує точність та швидкість прицілювання";
        type = ModificationType.Scope;
        rarity = ModificationRarity.Rare;
        
        effects.Add(ModificationEffect.AccuracyBoost);
        
        compatibleWeapons.Add(WeaponType.AssaultRifle);
        compatibleWeapons.Add(WeaponType.DMR);
        compatibleWeapons.Add(WeaponType.LMG);
    }
    
    public override void ApplyModification(WeaponBase weapon)
    {
        if (!IsCompatible(weapon)) return;
        
        attachedWeapon = weapon;
        isInstalled = true;
        
        weapon.accuracy *= (1f + accuracyBonus * GetRarityMultiplier());
        
        // Додавання компонента прицілу
        HolographicSight sight = weapon.gameObject.GetComponent<HolographicSight>();
        if (sight == null)
        {
            sight = weapon.gameObject.AddComponent<HolographicSight>();
        }
        
        sight.zoomLevel = zoomLevel;
        sight.aimSpeedMultiplier = 1f + aimSpeedBonus;
        sight.nightVisionEnabled = hasNightVision;
        
        ApplyVisualChanges(weapon);
    }
    
    public override void RemoveModification(WeaponBase weapon)
    {
        if (!isInstalled || attachedWeapon != weapon) return;
        
        weapon.accuracy /= (1f + accuracyBonus * GetRarityMultiplier());
        
        HolographicSight sight = weapon.gameObject.GetComponent<HolographicSight>();
        if (sight != null)
        {
            Object.Destroy(sight);
        }
        
        RemoveVisualChanges(weapon);
        
        isInstalled = false;
        attachedWeapon = null;
    }
    
    public override bool IsCompatible(WeaponBase weapon)
    {
        return compatibleWeapons.Contains(weapon.weaponType);
    }
    
    void ApplyVisualChanges(WeaponBase weapon)
    {
        if (visualPrefab != null)
        {
            GameObject visual = Object.Instantiate(visualPrefab, weapon.transform);
            visual.name = $"{modificationName}_Visual";
            
            // Налаштування позиції прицілу
            visual.transform.localPosition = new Vector3(0, 0.1f, 0.3f);
        }
    }
    
    void RemoveVisualChanges(WeaponBase weapon)
    {
        Transform visual = weapon.transform.Find($"{modificationName}_Visual");
        if (visual != null)
        {
            Object.Destroy(visual.gameObject);
        }
    }
}

[CreateAssetMenu(fileName = "ExplosiveAmmo", menuName = "Game/Modifications/Ammunition/Explosive")]
public class ExplosiveAmmoMod : WeaponModification
{
    [Header("Explosive Ammo Specific")]
    public float explosionRadius = 3f;
    public float explosionDamage = 50f;
    public float damageBonus = 0.3f;
    public GameObject explosionEffect;
    
    public ExplosiveAmmoMod()
    {
        modificationName = "Вибухові набої";
        description = "Набої вибухають при попаданні, завдаючи зональний урон";
        type = ModificationType.Ammunition;
        rarity = ModificationRarity.Epic;
        
        effects.Add(ModificationEffect.ExplosiveRounds);
        effects.Add(ModificationEffect.DamageIncrease);
        
        compatibleWeapons.Add(WeaponType.AssaultRifle);
        compatibleWeapons.Add(WeaponType.LMG);
        compatibleWeapons.Add(WeaponType.SniperRifle);
    }
    
    public override void ApplyModification(WeaponBase weapon)
    {
        if (!IsCompatible(weapon)) return;
        
        attachedWeapon = weapon;
        isInstalled = true;
        
        weapon.damage *= (1f + damageBonus * GetRarityMultiplier());
        
        // Додавання компонента вибухових набоїв
        ExplosiveAmmo ammoComponent = weapon.gameObject.GetComponent<ExplosiveAmmo>();
        if (ammoComponent == null)
        {
            ammoComponent = weapon.gameObject.AddComponent<ExplosiveAmmo>();
        }
        
        ammoComponent.explosionRadius = explosionRadius;
        ammoComponent.explosionDamage = explosionDamage * GetRarityMultiplier();
        ammoComponent.explosionEffect = explosionEffect;
        ammoComponent.enabled = true;
        
        ApplyVisualChanges(weapon);
    }
    
    public override void RemoveModification(WeaponBase weapon)
    {
        if (!isInstalled || attachedWeapon != weapon) return;
        
        weapon.damage /= (1f + damageBonus * GetRarityMultiplier());
        
        ExplosiveAmmo ammoComponent = weapon.gameObject.GetComponent<ExplosiveAmmo>();
        if (ammoComponent != null)
        {
            ammoComponent.enabled = false;
        }
        
        RemoveVisualChanges(weapon);
        
        isInstalled = false;
        attachedWeapon = null;
    }
    
    public override bool IsCompatible(WeaponBase weapon)
    {
        return compatibleWeapons.Contains(weapon.weaponType);
    }
    
    void ApplyVisualChanges(WeaponBase weapon)
    {
        // Зміна кольору дульного спалаху
        ParticleSystem muzzleFlash = weapon.GetComponentInChildren<ParticleSystem>();
        if (muzzleFlash != null)
        {
            var main = muzzleFlash.main;
            main.startColor = Color.orange;
        }
        
        // Додавання ефекту на зброю
        if (effectParticles != null)
        {
            GameObject effect = Object.Instantiate(effectParticles.gameObject, weapon.transform);
            effect.name = $"{modificationName}_Effect";
        }
    }
    
    void RemoveVisualChanges(WeaponBase weapon)
    {
        Transform effect = weapon.transform.Find($"{modificationName}_Effect");
        if (effect != null)
        {
            Object.Destroy(effect.gameObject);
        }
        
        // Відновлення оригінального кольору
        ParticleSystem muzzleFlash = weapon.GetComponentInChildren<ParticleSystem>();
        if (muzzleFlash != null)
        {
            var main = muzzleFlash.main;
            main.startColor = Color.yellow;
        }
    }
}

// ================================
// ДОПОМІЖНІ КОМПОНЕНТИ
// ================================

public class HolographicSight : MonoBehaviour
{
    public float zoomLevel = 2f;
    public float aimSpeedMultiplier = 1.2f;
    public bool nightVisionEnabled = false;
    
    private Camera playerCamera;
    private float originalFOV;
    private bool isAiming = false;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }
    }
    
    void Update()
    {
        HandleAiming();
    }
    
    void HandleAiming()
    {
        if (Input.GetMouseButtonDown(1)) // Right click to aim
        {
            StartAiming();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            StopAiming();
        }
    }
    
    void StartAiming()
    {
        isAiming = true;
        
        if (playerCamera != null)
        {
            float targetFOV = originalFOV / zoomLevel;
            StartCoroutine(SmoothZoom(playerCamera.fieldOfView, targetFOV));
        }
        
        if (nightVisionEnabled)
        {
            EnableNightVision();
        }
    }
    
    void StopAiming()
    {
        isAiming = false;
        
        if (playerCamera != null)
        {
            StartCoroutine(SmoothZoom(playerCamera.fieldOfView, originalFOV));
        }
        
        if (nightVisionEnabled)
        {
            DisableNightVision();
        }
    }
    
    IEnumerator SmoothZoom(float fromFOV, float toFOV)
    {
        float elapsed = 0f;
        float duration = 0.2f / aimSpeedMultiplier;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = Mathf.Lerp(fromFOV, toFOV, t);
            }
            
            yield return null;
        }
        
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = toFOV;
        }
    }
    
    void EnableNightVision()
    {
        // Зміна освітлення для нічного бачення
        RenderSettings.ambientIntensity *= 2f;
        
        // Зміна кольорової схеми
        if (playerCamera != null)
        {
            playerCamera.backgroundColor = Color.green * 0.1f;
        }
    }
    
    void DisableNightVision()
    {
        // Відновлення нормального освітлення
        RenderSettings.ambientIntensity /= 2f;
        
        if (playerCamera != null)
        {
            playerCamera.backgroundColor = Color.black;
        }
    }
}

public class ExplosiveAmmo : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float explosionDamage = 50f;
    public GameObject explosionEffect;
    
    void OnEnable()
    {
        // Підписка на події пострілу
        WeaponBase weapon = GetComponent<WeaponBase>();
        if (weapon != null)
        {
            weapon.onBulletHit += OnBulletHit;
        }
    }
    
    void OnDisable()
    {
        WeaponBase weapon = GetComponent<WeaponBase>();
        if (weapon != null)
        {
            weapon.onBulletHit -= OnBulletHit;
        }
    }
    
    void OnBulletHit(Vector3 hitPosition, Collider hitCollider)
    {
        CreateExplosion(hitPosition);
    }
    
    void CreateExplosion(Vector3 position)
    {
        // Візуальний ефект
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, position, Quaternion.identity);
        }
        
        // Зональний урон
        Collider[] nearbyObjects = Physics.OverlapSphere(position, explosionRadius);
        
        foreach (var obj in nearbyObjects)
        {
            float distance = Vector3.Distance(position, obj.transform.position);
            float damageMultiplier = 1f - (distance / explosionRadius);
            float finalDamage = explosionDamage * damageMultiplier;
            
            // Завдання урону ворогам
            EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(finalDamage, DamageType.Explosion);
            }
            
            // Фізичний вплив
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 explosionDirection = (obj.transform.position - position).normalized;
                rb.AddForce(explosionDirection * (300f * damageMultiplier), ForceMode.Impulse);
            }
        }
    }
}

// ================================
// МЕНЕДЖЕР МОДИФІКАЦІЙ
// ================================

public class WeaponModificationManager : MonoBehaviour
{
    [Header("Available Modifications")]
    public List<WeaponModification> availableModifications = new List<WeaponModification>();
    
    [Header("Player Inventory")]
    public List<WeaponModification> playerModifications = new List<WeaponModification>();
    
    [Header("Installed Modifications")]
    public Dictionary<WeaponBase, List<WeaponModification>> installedMods = new Dictionary<WeaponBase, List<WeaponModification>>();
    
    public static WeaponModificationManager Instance { get; private set; }
    
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
    
    public bool InstallModification(WeaponBase weapon, WeaponModification modification)
    {
        if (!modification.IsCompatible(weapon))
        {
            UIManager.Instance?.ShowNotification("Модифікація несумісна з цією зброєю", NotificationType.Warning);
            return false;
        }
        
        if (!playerModifications.Contains(modification))
        {
            UIManager.Instance?.ShowNotification("У вас немає цієї модифікації", NotificationType.Warning);
            return false;
        }
        
        // Перевірка на конфлікти
        if (HasConflictingModification(weapon, modification))
        {
            UIManager.Instance?.ShowNotification("Конфлікт з встановленою модифікацією", NotificationType.Warning);
            return false;
        }
        
        // Встановлення модифікації
        modification.ApplyModification(weapon);
        
        if (!installedMods.ContainsKey(weapon))
        {
            installedMods[weapon] = new List<WeaponModification>();
        }
        
        installedMods[weapon].Add(modification);
        playerModifications.Remove(modification);
        
        // Перевірка синергій
        CheckSynergies(weapon);
        
        UIManager.Instance?.ShowNotification($"Встановлено: {modification.modificationName}", NotificationType.Success);
        return true;
    }
    
    public bool RemoveModification(WeaponBase weapon, WeaponModification modification)
    {
        if (!installedMods.ContainsKey(weapon) || !installedMods[weapon].Contains(modification))
        {
            return false;
        }
        
        modification.RemoveModification(weapon);
        installedMods[weapon].Remove(modification);
        playerModifications.Add(modification);
        
        UIManager.Instance?.ShowNotification($"Видалено: {modification.modificationName}", NotificationType.Info);
        return true;
    }
    
    bool HasConflictingModification(WeaponBase weapon, WeaponModification newMod)
    {
        if (!installedMods.ContainsKey(weapon)) return false;
        
        foreach (var installedMod in installedMods[weapon])
        {
            if (installedMod.type == newMod.type)
            {
                return true; // Один тип модифікації на слот
            }
        }
        
        return false;
    }
    
    void CheckSynergies(WeaponBase weapon)
    {
        if (!installedMods.ContainsKey(weapon)) return;
        
        var mods = installedMods[weapon];
        
        foreach (var mod1 in mods)
        {
            foreach (var mod2 in mods)
            {
                if (mod1 != mod2 && mod1.synergyMods.Contains(mod2.modificationName))
                {
                    ApplySynergy(weapon, mod1, mod2);
                }
            }
        }
    }
    
    void ApplySynergy(WeaponBase weapon, WeaponModification mod1, WeaponModification mod2)
    {
        float synergyBonus = (mod1.synergyBonus + mod2.synergyBonus) / 2f;
        
        // Застосування синергетичного бонусу
        weapon.damage *= (1f + synergyBonus);
        
        UIManager.Instance?.ShowNotification($"СИНЕРГІЯ: {mod1.modificationName} + {mod2.modificationName}", NotificationType.Special);
        
        // Візуальний ефект синергії
        CreateSynergyEffect(weapon);
    }
    
    void CreateSynergyEffect(WeaponBase weapon)
    {
        GameObject synergyEffect = new GameObject("Synergy Effect");
        synergyEffect.transform.SetParent(weapon.transform);
        synergyEffect.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = synergyEffect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.cyan;
        main.startLifetime = 2f;
        main.startSpeed = 5f;
        main.maxParticles = 20;
        
        Destroy(synergyEffect, 3f);
    }
    
    public List<WeaponModification> GetCompatibleModifications(WeaponBase weapon)
    {
        return playerModifications.Where(mod => mod.IsCompatible(weapon)).ToList();
    }
    
    public List<WeaponModification> GetInstalledModifications(WeaponBase weapon)
    {
        return installedMods.ContainsKey(weapon) ? installedMods[weapon] : new List<WeaponModification>();
    }
    
    public void AddModificationToInventory(WeaponModification modification)
    {
        playerModifications.Add(modification);
        UIManager.Instance?.ShowNotification($"Отримано модифікацію: {modification.modificationName}", NotificationType.Reward);
    }
}