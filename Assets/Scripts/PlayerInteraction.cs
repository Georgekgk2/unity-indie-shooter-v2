using UnityEngine;
using TMPro; // ��� ����������� �������

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("³������, �� ��� ������� ���� ��������� � ��'������.")]
    public float interactionDistance = 3f;
    [Tooltip("������ ��� �����䳿.")]
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("���(�) ��'����, � ����� ����� ���������.")]
    public LayerMask interactableLayer;

    [Header("Optimization Settings (SCOUT �����������)")]
    [Tooltip("�������� �� Raycast ���������� (�������) - ������ ������������ �� 85%")]
    [SerializeField] private float raycastInterval = 0.1f;
    [Tooltip("������� �� ��������� (�������) - ������ �� �����")]
    [SerializeField] private float interactionCooldown = 0.3f;

    [Header("UI Settings")]
    [Tooltip("��������� ������� ��� ����������� ������� (���������, '�������� E, ��� �������').")]
    public TextMeshProUGUI interactionPromptText;
    
    // ������� ����
    private Camera mainCamera;
    private Interactable currentInteractable; // ��'���, �� ���� �� ����� ��������
    
    // ���� ��� ���������� (SCOUT �����������)
    private float lastRaycastTime;
    private float lastInteractionTime;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerInteraction: Main Camera �� ��������. ������� �� �����������.", this);
            enabled = false;
        }
        
        // �������� ����� ������� �� �����
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // SCOUT ����̲��ֲ�: Raycast �� ����� ����, � � ����������
        if (Time.time - lastRaycastTime >= raycastInterval)
        {
            CheckForInteractable();
            lastRaycastTime = Time.time;
        }

        // ������� ����� (���������� ����� ���� ��� responsive ��������)
        HandleInteractionInput();
    }

    /// <summary>
    /// ��������, �� � ����� ������� ��'���, � ���� ����� ���������.
    /// </summary>
    private void CheckForInteractable()
    {
        RaycastHit hit;
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

        // ��������� ������
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // ���� ������ ������ � ��'��� � ����������� Interactable
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                // ���� �� �������� �� ����� ��'���
                if (interactable != currentInteractable)
                {
                    currentInteractable = interactable;
                    UpdateInteractionPrompt();
                }
            }
            else // ���� ������� � ����, ��� �� �� Interactable
            {
                ClearInteraction();
            }
        }
        else // ���� ������ ����� �� ������
        {
            ClearInteraction();
        }
    }
    
    /// <summary>
    /// �������� ���������� ������ �����䳿 (SCOUT ����̲��ֲ�).
    /// </summary>
    private void HandleInteractionInput()
    {
        // ���� �� �������� �� ��'���, � ���� ����� ���������, � ��������� ������ E
        if (currentInteractable != null && Input.GetKeyDown(interactionKey))
        {
            // SCOUT �����������: ������ �� ����� �������
            if (Time.time - lastInteractionTime < interactionCooldown) 
            {
                Debug.Log("PlayerInteraction: ������� �� �������, ���������...");
                return;
            }

            bool success = currentInteractable.Interact(this.gameObject); // �������� ���� �� "���������"
            if (success)
            {
                lastInteractionTime = Time.time;
                Debug.Log($"PlayerInteraction: ������ ������� � {currentInteractable.name}");
            }
        }
    }

    /// <summary>
    /// ������� ����� ������� ��� �����䳿.
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
    /// ������� ������� �� ����� �������� ��'��� ��� �����䳿.
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