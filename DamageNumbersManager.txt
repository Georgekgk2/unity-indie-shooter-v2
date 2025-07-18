using UnityEngine;
using System.Collections;

/// <summary>
/// Менеджер для відображення чисел урону над ворогами (Claude рекомендація)
/// Створює плаваючі числа з анімаціями як в AAA іграх
/// </summary>
public class DamageNumbersManager : MonoBehaviour
{
    [Header("Damage Numbers")]
    [Tooltip("Префаб для відображення урону")]
    public GameObject damageNumberPrefab;
    [Tooltip("Швидкість руху чисел вгору")]
    public float floatSpeed = 2f;
    [Tooltip("Час зникнення")]
    public float fadeTime = 1.5f;
    [Tooltip("Випадковий розкид позиції")]
    public float randomSpread = 0.5f;
    
    [Header("Colors")]
    [Tooltip("Колір звичайного урону")]
    public Color normalDamageColor = Color.white;
    [Tooltip("Колір критичного урону")]
    public Color criticalDamageColor = Color.red;
    [Tooltip("Колір лікування")]
    public Color healingColor = Color.green;
    [Tooltip("Колір урону по броні")]
    public Color armorDamageColor = Color.cyan;
    
    [Header("Animation Settings")]
    [Tooltip("Масштаб для критичного урону")]
    public float criticalScale = 1.5f;
    [Tooltip("Швидкість пульсації")]
    public float pulseSpeed = 3f;
    [Tooltip("Чи використовувати анімацію появи")]
    public bool useSpawnAnimation = true;
    
    // Singleton
    public static DamageNumbersManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Перевіряємо наявність префабу
        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("DamageNumbersManager: Damage Number Prefab не призначено! Створюємо базовий...");
            CreateDefaultDamageNumberPrefab();
        }
    }
    
    /// <summary>
    /// Показує число урону в заданій позиції
    /// </summary>
    public void ShowDamageNumber(Vector3 position, float damage, bool isCritical = false, DamageType damageType = DamageType.Normal)
    {
        if (damageNumberPrefab == null) return;
        
        // Додаємо випадковий розкид
        Vector3 spawnPosition = position + Random.insideUnitSphere * randomSpread;
        
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        DamageNumber damageNum = numberObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            Color textColor = GetDamageColor(damageType, isCritical);
            float scale = isCritical ? criticalScale : 1f;
            damageNum.Initialize(damage, textColor, isCritical, floatSpeed, fadeTime, "", scale);
        }
        else
        {
            Debug.LogWarning("DamageNumbersManager: Префаб не має компонента DamageNumber!");
        }
    }
    
    /// <summary>
    /// Показує число лікування
    /// </summary>
    public void ShowHealingNumber(Vector3 position, float healAmount)
    {
        if (damageNumberPrefab == null) return;
        
        Vector3 spawnPosition = position + Random.insideUnitSphere * randomSpread;
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        DamageNumber damageNum = numberObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            damageNum.Initialize(healAmount, healingColor, false, floatSpeed, fadeTime, "+", 1.2f);
        }
    }
    
    /// <summary>
    /// Показує число досвіду
    /// </summary>
    public void ShowXPNumber(Vector3 position, int xpAmount)
    {
        if (damageNumberPrefab == null) return;
        
        Vector3 spawnPosition = position + Vector3.up * 0.5f + Random.insideUnitSphere * randomSpread;
        GameObject numberObj = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);
        DamageNumber damageNum = numberObj.GetComponent<DamageNumber>();
        
        if (damageNum != null)
        {
            damageNum.Initialize(xpAmount, Color.yellow, false, floatSpeed * 0.8f, fadeTime * 1.2f, "+", 0.8f, "XP");
        }
    }
    
    /// <summary>
    /// Визначає колір урону залежно від типу
    /// </summary>
    Color GetDamageColor(DamageType damageType, bool isCritical)
    {
        Color baseColor;
        
        switch (damageType)
        {
            case DamageType.Normal:
                baseColor = normalDamageColor;
                break;
            case DamageType.Armor:
                baseColor = armorDamageColor;
                break;
            case DamageType.Healing:
                baseColor = healingColor;
                break;
            default:
                baseColor = normalDamageColor;
                break;
        }
        
        // Якщо критичний урон, робимо колір яскравішим
        if (isCritical)
        {
            baseColor = Color.Lerp(baseColor, criticalDamageColor, 0.6f);
        }
        
        return baseColor;
    }
    
    /// <summary>
    /// Створює базовий префаб для damage numbers, якщо не призначено
    /// </summary>
    void CreateDefaultDamageNumberPrefab()
    {
        GameObject prefab = new GameObject("DamageNumber");
        
        // Додаємо TextMesh компонент
        TextMesh textMesh = prefab.AddComponent<TextMesh>();
        textMesh.text = "0";
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        // Додаємо DamageNumber скрипт
        prefab.AddComponent<DamageNumber>();
        
        // Зберігаємо як префаб (в runtime це буде тимчасовий об'єкт)
        damageNumberPrefab = prefab;
        
        Debug.Log("DamageNumbersManager: Створено базовий damage number prefab");
    }
    
    /// <summary>
    /// Очищає всі активні damage numbers (для оптимізації)
    /// </summary>
    public void ClearAllDamageNumbers()
    {
        DamageNumber[] activeNumbers = FindObjectsOfType<DamageNumber>();
        foreach (var number in activeNumbers)
        {
            if (number != null)
            {
                Destroy(number.gameObject);
            }
        }
    }
}

/// <summary>
/// Типи урону для різних кольорів
/// </summary>
public enum DamageType
{
    Normal,
    Armor,
    Healing
}