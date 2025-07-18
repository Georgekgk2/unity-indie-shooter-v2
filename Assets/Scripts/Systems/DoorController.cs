using UnityEngine;

public class DoorController : Interactable // Óñïàäêîâóºìî â³ä Interactable
{
    [Header("Door Settings")]
    [Tooltip("Àí³ìàòîð, ÿêèé êåðóº àí³ìàö³ÿìè äâåðåé (â³äêðèòòÿ/çàêðèòòÿ).")]
    public Animator animator;
    
    [Tooltip("Ô³çè÷íèé êîëàéäåð, ÿêèé áëîêóº ãðàâöÿ ³ áóäå âèìèêàòèñÿ ïðè â³äêðèòò³.")]
    public Collider physicalCollider; // Çàì³ñòü doorCollider
    
    [Tooltip("Êîëàéäåð äëÿ âèÿâëåííÿ âçàºìîä³¿ (Raycast). Ìàº áóòè çàâæäè óâ³ìêíåíèì.")]
    public Collider interactionCollider; // Íîâå ïîëå
    
    [Header("Animation Parameters")]
    [Tooltip("Íàçâà áóëåâîãî ïàðàìåòðà â àí³ìàòîð³, ùî â³äïîâ³äàº çà ñòàí 'â³äêðèòî'.")]
    public string isOpenParameterName = "isOpen";
    
    [Header("Audio")]
    [Tooltip("Çâóê â³äêðèòòÿ äâåðåé.")]
    public AudioClip openSound;
    [Tooltip("Çâóê çàêðèòòÿ äâåðåé.")]
    public AudioClip closeSound;
    
    private AudioSource audioSource;
    private bool isOpen = false;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError($"DoorController íà îá'ºêò³ '{gameObject.name}' íå ìàº àí³ìàòîðà.", this);
        
        // Ïåðåâ³ðÿºìî, ÷è ïðèçíà÷åí³ îáèäâà êîëàéäåðè
        if (physicalCollider == null)
        {
            Debug.LogError($"DoorController íà îá'ºêò³ '{gameObject.name}' íå ìàº ïðèçíà÷åíîãî 'physicalCollider'.", this);
        }
        if (interactionCollider == null)
        {
            // ßêùî íå ïðèçíà÷åíî, ñïðîáóºìî çíàéòè êîëàéäåð-òðèãåð
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
                Debug.LogError($"DoorController íà îá'ºêò³ '{gameObject.name}' íå ìàº 'interactionCollider' (êîëàéäåð ç Is Trigger).", this);
            }
        }
    }

    /// <summary>
    /// Ðåàë³çàö³ÿ àáñòðàêòíîãî ìåòîäó Interact ç áàçîâîãî êëàñó.
    /// </summary>
    public override bool Interact(GameObject interactor)
    {
        isOpen = !isOpen;
        
        if (animator != null)
        {
            animator.SetBool(isOpenParameterName, isOpen);
        }
        
        // --- ÊËÞ×ÎÂÀ ÇÌ²ÍÀ ---
        // Âèìèêàºìî/âìèêàºìî ëèøå ô³çè÷íèé êîëàéäåð
        if (physicalCollider != null)
        {
            physicalCollider.enabled = !isOpen;
        }

        // Â³äòâîðþºìî çâóê
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