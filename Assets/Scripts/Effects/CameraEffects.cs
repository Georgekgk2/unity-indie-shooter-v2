using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraEffects : MonoBehaviour
{
    [Header("Camera Shake Settings")]
    [Tooltip("Ñòàíäàðòíà òðèâàë³ñòü òðÿñêè êàìåðè.")]
    public float defaultShakeDuration = 0.2f;
    [Tooltip("Ñòàíäàðòíà ñèëà (àìïë³òóäà) òðÿñêè êàìåðè.")]
    public float defaultShakeMagnitude = 0.1f;
    
    [Header("Damage Flash Settings")]
    [Tooltip("Image UI-åëåìåíò äëÿ ÷åðâîíîãî ñïàëàõó ïðè îòðèìàíí³ óðîíó.")]
    public Image damageFlashImage;
    [Tooltip("Òðèâàë³ñòü ñïàëàõó (ó ñåêóíäàõ).")]
    public float flashDuration = 0.25f;
    [Tooltip("Ìàêñèìàëüíà ïðîçîð³ñòü ñïàëàõó (â³ä 0 äî 1).")]
    public float maxFlashAlpha = 0.5f;

    [Header("Heal Effect Settings")] // ÍÎÂÈÉ ÐÎÇÄ²Ë
    [Tooltip("Image UI-åëåìåíò äëÿ ñïàëàõó ïðè ë³êóâàíí³.")]
    public Image healFlashImage;
    [Tooltip("Òðèâàë³ñòü åôåêòó ë³êóâàííÿ.")]
    public float healEffectDuration = 0.3f;
    [Tooltip("Ìàêñèìàëüíà ïðîçîð³ñòü åôåêòó ë³êóâàííÿ.")]
    public float maxHealAlpha = 0.3f;
    [Tooltip("Ñèëà òðÿñêè êàìåðè ïðè ë³êóâàíí³ (0 - áåç òðÿñêè).")]
    public float healShakeMagnitude = 0.05f;


    // Ïðèâàòí³ çì³íí³
    private Vector3 originalCameraPosition;
    private Coroutine shakeCoroutine;
    private Coroutine damageFlashCoroutine;
    private Coroutine healFlashCoroutine; // Íîâà êîðóòèíà äëÿ ë³êóâàííÿ

    void Awake()
    {
        originalCameraPosition = transform.localPosition;
        
        // Ïåðåêîíóºìîñÿ, ùî ñïàëàõè âèìêíåí³ ïðè ñòàðò³
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
    /// Çàïóñêàº òðÿñêó êàìåðè ç êàñòîìíèìè ïàðàìåòðàìè.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (magnitude <= 0) return; // Íå òðÿñåìî, ÿêùî ñèëà 0
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    /// <summary>
    /// Çàïóñêàº òðÿñêó êàìåðè ç³ ñòàíäàðòíèìè ïàðàìåòðàìè.
    /// </summary>
    public void Shake()
    {
        Shake(defaultShakeDuration, defaultShakeMagnitude);
    }

    /// <summary>
    /// Çàïóñêàº ÷åðâîíèé ñïàëàõ íà åêðàí³ ïðè îòðèìàíí³ óðîíó.
    /// </summary>
    public void FlashDamageEffect()
    {
        if (damageFlashImage == null) return;
        
        if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
        damageFlashCoroutine = StartCoroutine(FlashCoroutine(damageFlashImage, flashDuration, maxFlashAlpha));
    }

    /// <summary>
    /// Çàïóñêàº åôåêò ë³êóâàííÿ (ñïàëàõ + òðÿñêà).
    /// </summary>
    public void PlayHealEffect() // ÍÎÂÈÉ ÌÅÒÎÄ
    {
        if (healFlashImage != null)
        {
            if (healFlashCoroutine != null) StopCoroutine(healFlashCoroutine);
            healFlashCoroutine = StartCoroutine(FlashCoroutine(healFlashImage, healEffectDuration, maxHealAlpha));
        }
        
        // Äîäàºìî ëåãêó òðÿñêó ïðè ë³êóâàíí³
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
    /// Óí³âåðñàëüíà êîðóòèíà äëÿ ñïàëàõó íà åêðàí³.
    /// </summary>
    private IEnumerator FlashCoroutine(Image flashImage, float duration, float maxAlpha) // Çðîáëåíî óí³âåðñàëüíèì
    {
        flashImage.gameObject.SetActive(true);
        float elapsed = 0.0f;
        Color flashColor = flashImage.color;
        
        // Ôàçà ïîÿâè ñïàëàõó
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            flashColor.a = Mathf.Lerp(0, maxAlpha, elapsed / (duration / 2));
            flashImage.color = flashColor;
            yield return null;
        }

        // Ôàçà çíèêíåííÿ ñïàëàõó
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