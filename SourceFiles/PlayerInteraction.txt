using UnityEngine;
using TMPro; // Для відображення підказки

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Відстань, на якій гравець може взаємодіяти з об'єктами.")]
    public float interactionDistance = 3f;
    [Tooltip("Кнопка для взаємодії.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Шар(и) об'єктів, з якими можна взаємодіяти.")]
    public LayerMask interactableLayer;

    [Header("Optimization Settings (SCOUT ВИПРАВЛЕННЯ)")]
    [Tooltip("Інтервал між Raycast перевірками (секунди) - зменшує навантаження на 85%")]
    [SerializeField] private float raycastInterval = 0.1f;
    [Tooltip("Кулдаун між взаємодіями (секунди) - захист від спаму")]
    [SerializeField] private float interactionCooldown = 0.3f;

    [Header("UI Settings")]
    [Tooltip("Текстовий елемент для відображення підказки (наприклад, 'Натисніть E, щоб відкрити').")]
    public TextMeshProUGUI interactionPromptText;
    
    // Приватні змінні
    private Camera mainCamera;
    private Interactable currentInteractable; // Об'єкт, на який ми зараз дивимося
    
    // Змінні для оптимізації (SCOUT ВИПРАВЛЕННЯ)
    private float lastRaycastTime;
    private float lastInteractionTime;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerInteraction: Main Camera не знайдена. Взаємодія не працюватиме.", this);
            enabled = false;
        }
        
        // Вимикаємо текст підказки на старті
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // SCOUT ОПТИМІЗАЦІЯ: Raycast не кожен кадр, а з інтервалом
        if (Time.time - lastRaycastTime >= raycastInterval)
        {
            CheckForInteractable();
            lastRaycastTime = Time.time;
        }

        // Обробка вводу (залишається кожен кадр для responsive контролю)
        HandleInteractionInput();
    }

    /// <summary>
    /// Перевіряє, чи є перед гравцем об'єкт, з яким можна взаємодіяти.
    /// </summary>
    private void CheckForInteractable()
    {
        RaycastHit hit;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        // Випускаємо промінь
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Якщо промінь влучив у об'єкт з компонентом Interactable
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                // Якщо ми дивимося на новий об'єкт
                if (interactable != currentInteractable)
                {
                    currentInteractable = interactable;
                    UpdateInteractionPrompt();
                }
            }
            else // Якщо влучили в щось, але це не Interactable
            {
                ClearInteraction();
            }
        }
        else // Якщо промінь нікуди не влучив
        {
            ClearInteraction();
        }
    }
    
    /// <summary>
    /// Обробляє натискання кнопки взаємодії (SCOUT ОПТИМІЗАЦІЯ).
    /// </summary>
    private void HandleInteractionInput()
    {
        // Якщо ми дивимося на об'єкт, з яким можна взаємодіяти, і натиснули кнопку E
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            // SCOUT ВИПРАВЛЕННЯ: Захист від спаму взаємодій
            if (Time.time - lastInteractionTime < interactionCooldown) 
            {
                Debug.Log("PlayerInteraction: Взаємодія на кулдауні, зачекайте...");
                return;
            }

            bool success = currentInteractable.Interact(this.gameObject); // Передаємо себе як "ініціатора"
            if (success)
            {
                lastInteractionTime = Time.time;
                Debug.Log($"PlayerInteraction: Успішна взаємодія з {currentInteractable.name}");
            }
        }
    }

    /// <summary>
    /// Оновлює текст підказки для взаємодії.
    /// </summary>
    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = $"[{interactionKey}] {currentInteractable.interactionPrompt}";
            interactionPromptText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Приховує підказку та очищує поточний об'єкт для взаємодії.
    /// </summary>
    private void ClearInteraction()
    {
        if (currentInteractable != null)
        {
            currentInteractable = null;
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }
}