using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraEffects : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [Tooltip("Стандартна тривалість тряски камери.")]
    public float defaultShakeDuration = 0.2f;
    [Tooltip("Стандартна сила (амплітуда) тряски камери.")]
    public float defaultShakeMagnitude = 0.1f;
    
    [Header("Damage Flash Settings")]
    [Tooltip("Image UI-елемент для червоного спалаху при отриманні урону.")]
    public Image damageFlashImage;
    [Tooltip("Тривалість спалаху (у секундах).")]
    public float flashDuration = 0.25f;
    [Tooltip("Максимальна прозорість спалаху (від 0 до 1).")]
    public float maxFlashAlpha = 0.5f;

    [Header("Heal Effect Settings")] // НОВИЙ РОЗДІЛ
    [Tooltip("Image UI-елемент для спалаху при лікуванні.")]
    public Image healFlashImage;
    [Tooltip("Тривалість ефекту лікування.")]
    public float healEffectDuration = 0.3f;
    [Tooltip("Максимальна прозорість ефекту лікування.")]
    public float maxHealAlpha = 0.3f;
    [Tooltip("Сила тряски камери при лікуванні (0 - без тряски).")]
    public float healShakeMagnitude = 0.05f;


    // Приватні змінні
    private Vector3 originalCameraPosition;
    private Coroutine shakeCoroutine;
    private Coroutine damageFlashCoroutine;
    private Coroutine healFlashCoroutine; // Нова корутина для лікування

    void Awake()
    {
        originalCameraPosition = transform.localPosition;
        
        // Переконуємося, що спалахи вимкнені при старті
        if (damageFlashImage != null)
        {
            damageFlashImage.color = new Color(damageFlashImage.color.r, damageFlashImage.color.g, damageFlashImage.color.b, 0);
            damageFlashImage.gameObject.SetActive(false);
        }
        if (healFlashImage != null)
        {
            healFlashImage.color = new Color(healFlashImage.color.r, healFlashImage.color.g, healFlashImage.color.b, 0);
            healFlashImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Запускає тряску камери з кастомними параметрами.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (magnitude <= 0) return; // Не трясемо, якщо сила 0
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    /// <summary>
    /// Запускає тряску камери зі стандартними параметрами.
    /// </summary>
    public void Shake()
    {
        Shake(defaultShakeDuration, defaultShakeMagnitude);
    }

    /// <summary>
    /// Запускає червоний спалах на екрані при отриманні урону.
    /// </summary>
    public void FlashDamageEffect()
    {
        if (damageFlashImage == null) return;
        
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(FlashCoroutine(damageFlashImage, flashDuration, maxFlashAlpha));
    }

    /// <summary>
    /// Запускає ефект лікування (спалах + тряска).
    /// </summary>
    public void PlayHealEffect() // НОВИЙ МЕТОД
    {
        if (healFlashImage != null)
        {
            if (healFlashCoroutine != null) StopCoroutine(healFlashCoroutine);
            healFlashCoroutine = StartCoroutine(FlashCoroutine(healFlashImage, healEffectDuration, maxHealAlpha));
        }
        
        // Додаємо легку тряску при лікуванні
        Shake(healEffectDuration, healShakeMagnitude);
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = new Vector3(originalCameraPosition.x + x, originalCameraPosition.y + y, originalCameraPosition.z);
            
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        
        transform.localPosition = originalCameraPosition;
    }
    
    /// <summary>
    /// Універсальна корутина для спалаху на екрані.
    /// </summary>
    private IEnumerator FlashCoroutine(Image flashImage, float duration, float maxAlpha) // Зроблено універсальним
    {
        flashImage.gameObject.SetActive(true);
        float elapsed = 0.0f;
        Color flashColor = flashImage.color;
        
        // Фаза появи спалаху
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            flashColor.a = Mathf.Lerp(0, maxAlpha, elapsed / (duration / 2));
            flashImage.color = flashColor;
            yield return null;
        }

        // Фаза зникнення спалаху
        elapsed = 0.0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            flashColor.a = Mathf.Lerp(maxAlpha, 0, elapsed / (duration / 2));
            flashImage.color = flashColor;
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
    }
}