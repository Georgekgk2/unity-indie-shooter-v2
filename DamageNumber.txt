using UnityEngine;
using System.Collections;

/// <summary>
/// Компонент для окремого числа урону з анімаціями (Claude рекомендація)
/// Відповідає за рух, зникнення та візуальні ефекти одного числа
/// </summary>
public class DamageNumber : MonoBehaviour
{
    // Компоненти
    private TextMesh textMesh;
    private MeshRenderer meshRenderer;
    
    // Параметри анімації
    private float floatSpeed;
    private float fadeTime;
    private float timer;
    private Vector3 velocity;
    private Vector3 initialScale;
    private bool isCritical;
    
    // Анімація появи
    private bool useSpawnAnimation = true;
    private float spawnAnimationTime = 0.2f;
    
    // Пульсація для критичного урону
    private float pulseTimer;
    private float pulseSpeed = 3f;
    
    /// <summary>
    /// Ініціалізує damage number з параметрами
    /// </summary>
    public void Initialize(float value, Color color, bool critical, float speed, float fade, string prefix = "", float scale = 1f, string suffix = "")
    {
        // Отримуємо або створюємо компоненти
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMesh>();
            
        meshRenderer = GetComponent<MeshRenderer>();
        
        // Зберігаємо параметри
        floatSpeed = speed;
        fadeTime = fade;
        isCritical = critical;
        
        // Налаштовуємо текст
        string displayText = prefix + Mathf.RoundToInt(value).ToString();
        if (!string.IsNullOrEmpty(suffix))
            displayText += " " + suffix;
            
        textMesh.text = displayText;
        textMesh.color = color;
        textMesh.fontSize = critical ? 30 : 20;
        textMesh.fontStyle = critical ? FontStyle.Bold : FontStyle.Normal;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        // Налаштовуємо масштаб
        initialScale = Vector3.one * scale;
        transform.localScale = useSpawnAnimation ? Vector3.zero : initialScale;
        
        // Поворот до камери
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0);
        }
        
        // Випадкова швидкість з більшим рухом вгору
        velocity = Vector3.up * floatSpeed + Random.insideUnitSphere * (floatSpeed * 0.3f);
        velocity.y = Mathf.Abs(velocity.y); // Завжди рухаємося вгору
        
        // Запускаємо анімацію появи
        if (useSpawnAnimation)
        {
            StartCoroutine(SpawnAnimation());
        }
        
        Debug.Log($"DamageNumber: Створено {displayText} (Critical: {critical})");
    }
    
    void Update()
    {
        // Рух
        transform.Translate(velocity * Time.deltaTime, Space.World);
        
        // Зменшення швидкості (гравітаційний ефект)
        velocity *= 0.98f;
        
        // Пульсація для критичного урону
        if (isCritical)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * 0.1f;
            transform.localScale = initialScale * pulse;
        }
        
        // Зникнення
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
        
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = alpha;
            textMesh.color = color;
        }
        
        // Поворот до камери (якщо камера рухається)
        if (Camera.main != null && timer < fadeTime * 0.8f) // Припиняємо поворот перед зникненням
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
        
        // Знищення
        if (timer >= fadeTime)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Анімація появи з масштабуванням
    /// </summary>
    IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;
        Vector3 targetScale = initialScale;
        
        // Анімація з bounce ефектом
        while (elapsed < spawnAnimationTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spawnAnimationTime;
            
            // Bounce easing
            float bounceProgress = BounceEaseOut(progress);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, bounceProgress);
            
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    /// <summary>
    /// Bounce easing функція для плавної анімації
    /// </summary>
    float BounceEaseOut(float t)
    {
        if (t < 1f / 2.75f)
        {
            return 7.5625f * t * t;
        }
        else if (t < 2f / 2.75f)
        {
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        }
        else if (t < 2.5f / 2.75f)
        {
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        }
        else
        {
            return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
        }
    }
    
    /// <summary>
    /// Додає імпульс до damage number (наприклад, від вибуху)
    /// </summary>
    public void AddImpulse(Vector3 force)
    {
        velocity += force;
    }
    
    /// <summary>
    /// Змінює колір damage number (наприклад, для спеціальних ефектів)
    /// </summary>
    public void ChangeColor(Color newColor)
    {
        if (textMesh != null)
        {
            Color color = newColor;
            color.a = textMesh.color.a; // Зберігаємо поточну прозорість
            textMesh.color = color;
        }
    }
    
    /// <summary>
    /// Прискорює зникнення (для очищення екрану)
    /// </summary>
    public void FastFade()
    {
        fadeTime = Mathf.Min(fadeTime, 0.5f);
    }
    
    void OnDestroy()
    {
        // Очищення ресурсів, якщо потрібно
    }
}