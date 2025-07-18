using UnityEngine;

public class DoorController : Interactable // Успадковуємо від Interactable
{
    [Header("Door Settings")]
    [Tooltip("Аніматор, який керує анімаціями дверей (відкриття/закриття).")]
    public Animator animator;
    
    [Tooltip("Фізичний колайдер, який блокує гравця і буде вимикатися при відкритті.")]
    public Collider physicalCollider; // Замість doorCollider
    
    [Tooltip("Колайдер для виявлення взаємодії (Raycast). Має бути завжди увімкненим.")]
    public Collider interactionCollider; // Нове поле
    
    [Header("Animation Parameters")]
    [Tooltip("Назва булевого параметра в аніматорі, що відповідає за стан 'відкрито'.")]
    public string isOpenParameterName = "isOpen";
    
    [Header("Audio")]
    [Tooltip("Звук відкриття дверей.")]
    public AudioClip openSound;
    [Tooltip("Звук закриття дверей.")]
    public AudioClip closeSound;
    
    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError($"DoorController на об'єкті '{gameObject.name}' не має аніматора.", this);
        
        // Перевіряємо, чи призначені обидва колайдери
        if (physicalCollider == null)
        {
            Debug.LogError($"DoorController на об'єкті '{gameObject.name}' не має призначеного 'physicalCollider'.", this);
        }
        if (interactionCollider == null)
        {
            // Якщо не призначено, спробуємо знайти колайдер-тригер
            Collider[] colliders = GetComponents<Collider>();
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    interactionCollider = col;
                    break;
                }
            }
            if (interactionCollider == null)
            {
                Debug.LogError($"DoorController на об'єкті '{gameObject.name}' не має 'interactionCollider' (колайдер з Is Trigger).", this);
            }
        }
    }

    /// <summary>
    /// Реалізація абстрактного методу Interact з базового класу.
    /// </summary>
    public override bool Interact(GameObject interactor)
    {
        isOpen = !isOpen;
        
        if (animator != null)
        {
            animator.SetBool(isOpenParameterName, isOpen);
        }
        
        // --- КЛЮЧОВА ЗМІНА ---
        // Вимикаємо/вмикаємо лише фізичний колайдер
        if (physicalCollider != null)
        {
            physicalCollider.enabled = !isOpen;
        }

        // Відтворюємо звук
        if (isOpen && openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
        else if (!isOpen && closeSound != null)
        {
            AudioSource.PlayClipAtPoint(closeSound, transform.position);
        }
        
        Debug.Log($"Door '{gameObject.name}' state changed to: {(isOpen ? "Open" : "Closed")}");
        
        return true;
    }
}