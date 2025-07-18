using UnityEngine;
using TMPro; // Äëÿ â³äîáðàæåííÿ ï³äêàçêè

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Â³äñòàíü, íà ÿê³é ãðàâåöü ìîæå âçàºìîä³ÿòè ç îá'ºêòàìè.")]
    public float interactionDistance = 3f;
    [Tooltip("Êíîïêà äëÿ âçàºìîä³¿.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Øàð(è) îá'ºêò³â, ç ÿêèìè ìîæíà âçàºìîä³ÿòè.")]
    public LayerMask interactableLayer;

    [Header("Optimization Settings (SCOUT ÂÈÏÐÀÂËÅÍÍß)")]
    [Tooltip("²íòåðâàë ì³æ Raycast ïåðåâ³ðêàìè (ñåêóíäè) - çìåíøóº íàâàíòàæåííÿ íà 85%")]
    [SerializeField] private float raycastInterval = 0.1f;
    [Tooltip("Êóëäàóí ì³æ âçàºìîä³ÿìè (ñåêóíäè) - çàõèñò â³ä ñïàìó")]
    [SerializeField] private float interactionCooldown = 0.3f;

    [Header("UI Settings")]
    [Tooltip("Òåêñòîâèé åëåìåíò äëÿ â³äîáðàæåííÿ ï³äêàçêè (íàïðèêëàä, 'Íàòèñí³òü E, ùîá â³äêðèòè').")]
    public TextMeshProUGUI interactionPromptText;
    
    // Ïðèâàòí³ çì³íí³
    private Camera mainCamera;
    private Interactable currentInteractable; // Îá'ºêò, íà ÿêèé ìè çàðàç äèâèìîñÿ
    
    // Çì³íí³ äëÿ îïòèì³çàö³¿ (SCOUT ÂÈÏÐÀÂËÅÍÍß)
    private float lastRaycastTime;
    private float lastInteractionTime;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerInteraction: Main Camera íå çíàéäåíà. Âçàºìîä³ÿ íå ïðàöþâàòèìå.", this);
            enabled = false;
        }
        
        // Âèìèêàºìî òåêñò ï³äêàçêè íà ñòàðò³
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // SCOUT ÎÏÒÈÌ²ÇÀÖ²ß: Raycast íå êîæåí êàäð, à ç ³íòåðâàëîì
        if (Time.time - lastRaycastTime >= raycastInterval)
        {
            CheckForInteractable();
            lastRaycastTime = Time.time;
        }

        // Îáðîáêà ââîäó (çàëèøàºòüñÿ êîæåí êàäð äëÿ responsive êîíòðîëþ)
        HandleInteractionInput();
    }

    /// <summary>
    /// Ïåðåâ³ðÿº, ÷è º ïåðåä ãðàâöåì îá'ºêò, ç ÿêèì ìîæíà âçàºìîä³ÿòè.
    /// </summary>
    private void CheckForInteractable()
    {
        RaycastHit hit;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        // Âèïóñêàºìî ïðîì³íü
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // ßêùî ïðîì³íü âëó÷èâ ó îá'ºêò ç êîìïîíåíòîì Interactable
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                // ßêùî ìè äèâèìîñÿ íà íîâèé îá'ºêò
                if (interactable != currentInteractable)
                {
                    currentInteractable = interactable;
                    UpdateInteractionPrompt();
                }
            }
            else // ßêùî âëó÷èëè â ùîñü, àëå öå íå Interactable
            {
                ClearInteraction();
            }
        }
        else // ßêùî ïðîì³íü í³êóäè íå âëó÷èâ
        {
            ClearInteraction();
        }
    }
    
    /// <summary>
    /// Îáðîáëÿº íàòèñêàííÿ êíîïêè âçàºìîä³¿ (SCOUT ÎÏÒÈÌ²ÇÀÖ²ß).
    /// </summary>
    private void HandleInteractionInput()
    {
        // ßêùî ìè äèâèìîñÿ íà îá'ºêò, ç ÿêèì ìîæíà âçàºìîä³ÿòè, ³ íàòèñíóëè êíîïêó E
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            // SCOUT ÂÈÏÐÀÂËÅÍÍß: Çàõèñò â³ä ñïàìó âçàºìîä³é
            if (Time.time - lastInteractionTime < interactionCooldown) 
            {
                Debug.Log("PlayerInteraction: Âçàºìîä³ÿ íà êóëäàóí³, çà÷åêàéòå...");
                return;
            }

            bool success = currentInteractable.Interact(this.gameObject); // Ïåðåäàºìî ñåáå ÿê "³í³ö³àòîðà"
            if (success)
            {
                lastInteractionTime = Time.time;
                Debug.Log($"PlayerInteraction: Óñï³øíà âçàºìîä³ÿ ç {currentInteractable.name}");
            }
        }
    }

    /// <summary>
    /// Îíîâëþº òåêñò ï³äêàçêè äëÿ âçàºìîä³¿.
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
    /// Ïðèõîâóº ï³äêàçêó òà î÷èùóº ïîòî÷íèé îá'ºêò äëÿ âçàºìîä³¿.
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