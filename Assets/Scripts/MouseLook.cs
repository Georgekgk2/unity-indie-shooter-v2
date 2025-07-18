using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("��������� ���� ��� ��������������� ��������� ������ (�� ������ �� Y).")]
    public float horizontalSensitivity = 100f;
    [Tooltip("��������� ���� ��� ������������� ��������� ������ (�� �� �������� �� X).")]
    public float verticalSensitivity = 100f;

    [Tooltip("�������� ���������. ���� �������� = ������ ����������.")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Tooltip("�� ����������� �������������� ��� ���� (Mouse X)?")]
    public bool invertMouseX = false;
    [Tooltip("�� ����������� ������������ ��� ���� (Mouse Y)?")]
    public bool invertMouseY = false;
    [Tooltip("������ ���� ����. ��� ���� ����� ����� �������� ���� ������������.")]
    [Range(0f, 0.1f)]
    public float mouseDeadZone = 0.01f;

    [Tooltip("̳�������� ��� ������ ���� ��� ������ (�� �������� �� X).")]
    public float minimumX = -90f;
    [Tooltip("������������ ��� ������ ����� ��� ������ (�� �������� �� X).")]
    public float maximumX = 90f;

    [Tooltip("��������� ��� ������. ���� ���������� �� ������ �� Y (�������������).")]
    public Transform playerBody;
    [Tooltip("��������� ������ ������. ���� ���������� �� �� �������� �� X (�����������).")]
    public Transform playerCamera;

    [Header("Cursor Settings")]
    [Tooltip("�� ������� ��������� ������ ��� ����� ���?")]
    public bool lockCursorOnStart = true;
    [Tooltip("������ ��� ���������� ������� (�������� Escape).")]
    public KeyCode unlockCursorKey = KeyCode.Escape;

    // ������� ����
    private Quaternion initialPlayerBodyRotation;
    private Quaternion initialPlayerCameraLocalRotation;
    private bool isCursorLocked = false;

    private float currentCameraRotationX = 0f; // ³�������� �������� ����������� ��� ��� ������������� ��������� ������
    private float currentBodyRotationY = 0f; // ³�������� �������� ����������� ��� ��� ��������������� ��������� ������ (�� �� Y)

    void Awake()
    {
        // �������� �� ����������� playerBody
        if (playerBody == null)
        {
            playerBody = transform;
            Debug.LogWarning("MouseLook: Player Body Transform �� ����������. ��������������� Transform ����� � GameObject: " + name);
        }
        if (playerBody == null)
        {
            Debug.LogError("MouseLook: ��������� ������ Player Body Transform. ������ ��������.");
            enabled = false;
            return;
        }

        // �������� �� ����������� playerCamera
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
            if (playerCamera == null)
            {
                Debug.LogError("MouseLook: Player Camera Transform �� ���������� � Main Camera �� ��������. ������ ��������.");
                enabled = false;
                return;
            }
        }

        // --- �������� �������� ��������� ��� ����� ��� ---
        initialPlayerBodyRotation = playerBody.rotation;
        initialPlayerCameraLocalRotation = playerCamera.localRotation;

        // ���������� ��������� ���� � �������� ��������, ��� �������� �������� ������������ � ����������.
        // ����� �� �� �� Y ��� ������.
        Vector3 initialBodyEuler = playerBody.rotation.eulerAngles;
        currentBodyRotationY = initialBodyEuler.y;
        if (currentBodyRotationY > 180) currentBodyRotationY -= 360;

        // �� �� X ��� ������.
        Vector3 initialCameraEuler = playerCamera.localRotation.eulerAngles;
        currentCameraRotationX = initialCameraEuler.x;
        if (currentCameraRotationX > 180) currentCameraRotationX -= 360;
    }

    void Start()
    {
        if (lockCursorOnStart)
        {
            SetCursorLock(true);
        }
    }

    void Update()
    {
        // ������� ����������/������������� �������
        if (Input.GetKeyDown(unlockCursorKey))
        {
            SetCursorLock(false);
        }
        else if (!isCursorLocked && Input.GetMouseButtonDown(0))
        {
            SetCursorLock(true);
        }

        if (!isCursorLocked)
        {
            return;
        }

        // --- ��������� ������� ����� ���� ---
        float rawMouseX = Input.GetAxisRaw("Mouse X");
        float rawMouseY = Input.GetAxisRaw("Mouse Y");

        // ������������ ������ ����
        if (Mathf.Abs(rawMouseX) < mouseDeadZone) rawMouseX = 0f;
        if (Mathf.Abs(rawMouseY) < mouseDeadZone) rawMouseY = 0f;

        // ������������ ������
        if (invertMouseX) rawMouseX *= -1f;
        if (invertMouseY) rawMouseY *= -1f;

        // --- ������������� ��������� ������ (�� ������ �� Y) ---
        // ������������� Mouse X ��� ��������� �� Y
        float horizontalAngleChange = rawMouseX * horizontalSensitivity * Time.deltaTime;
        currentBodyRotationY += horizontalAngleChange; // ���������� ��� ��� Y-��
        
        // ��������� �� ����������� ��������� �� playerBody
        // Quaternion.Euler(0, currentBodyRotationY, 0) ������� ��������� ����� �� Y.
        // �� ����������� ���� ������� initialPlayerBodyRotation.
        Quaternion targetBodyRotation = initialPlayerBodyRotation * Quaternion.Euler(0, currentBodyRotationY, 0);
        playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, rotationSmoothness);


        // --- ����������� ��������� ������ (�� �� �������� �� X) ---
        // ������������� Mouse Y ��� ��������� �� X
        float verticalAngleChange = rawMouseY * verticalSensitivity * Time.deltaTime;
        currentCameraRotationX -= verticalAngleChange; // ³������, ��� ��� ���� ����� ������� �����
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX); // �������� ����

        // ��������� �� ����������� ��������� �� playerCamera
        // Quaternion.Euler(currentCameraRotationX, 0, 0) ������� ��������� ����� �� X.
        // �� ����������� ���� ������� initialPlayerCameraLocalRotation.
        Quaternion targetCameraRotation = initialPlayerCameraLocalRotation * Quaternion.Euler(currentCameraRotationX, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, rotationSmoothness);
    }

    /// <summary>
    /// ����� ��� ��������� ������ ���������� �������.
    /// </summary>
    /// <param name="lockState">True, ��� ����������� �� ��������� ������; False, ��� ��������.</param>
    public void SetCursorLock(bool lockState)
    {
        isCursorLocked = lockState;
        if (lockState)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// ����� ��� �������� ��������� ������ �� ������ �� ����������� �����.
    /// ����� ����� ������� ��������� ����.
    /// </summary>
    public void ResetLookToInitial()
    {
        if (playerBody != null)
        {
            playerBody.rotation = initialPlayerBodyRotation;
            Vector3 initialBodyEuler = initialPlayerBodyRotation.eulerAngles;
            currentBodyRotationY = initialBodyEuler.y; // ������� �� ��������� Y
            if (currentBodyRotationY > 180) currentBodyRotationY -= 360;
        }
        if (playerCamera != null)
        {
            playerCamera.localRotation = initialPlayerCameraLocalRotation;
            Vector3 initialCameraEuler = initialPlayerCameraLocalRotation.eulerAngles;
            currentCameraRotationX = initialCameraEuler.x;
            if (currentCameraRotationX > 180) currentCameraRotationX -= 360;
        }
    }

    /// <summary>
    /// ������� �������� �������������� ��� ��������� ������ (����������� �� ������ �� Y).
    /// </summary>
    public float GetCurrentHorizontalRotation()
    {
        return currentBodyRotationY;
    }

    /// <summary>
    /// ������� �������� ������������ ��� ��������� ������ (����������� �� �������� �� X).
    /// </summary>
    public float GetCurrentVerticalRotation()
    {
        return currentCameraRotationX;
    }

    /// <summary>
    /// �������, �� ������ ������������.
    /// </summary>
    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }

    /// <summary>
    /// ��������� ������ ���� �� ������ (SCOUT �����������)
    /// </summary>
    public void ApplyRecoil(float recoilX, float recoilY)
    {
        // ����������� ����������� ������ (�����)
        currentCameraRotationX -= recoilY;
        
        // ����������� ������������� ������ (��������� ���)
        transform.Rotate(Vector3.up * recoilX);
        
        // �������� ������������ ��� ���� ������
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX);
        
        // ����������� ��������� ��� �� ������
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentCameraRotationX, 0f, 0f);
        }
    }
}