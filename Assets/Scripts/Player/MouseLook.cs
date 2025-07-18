using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("×óòëèâ³ñòü ìèø³ äëÿ ãîðèçîíòàëüíîãî îáåðòàííÿ ãðàâöÿ (ïî ñâ³òîâ³é îñ³ Y).")]
    public float horizontalSensitivity = 100f;
    [Tooltip("×óòëèâ³ñòü ìèø³ äëÿ âåðòèêàëüíîãî îáåðòàííÿ êàìåðè (ïî ¿¿ ëîêàëüí³é îñ³ X).")]
    public float verticalSensitivity = 100f;

    [Tooltip("Ïëàâí³ñòü îáåðòàííÿ. Âèùå çíà÷åííÿ = øâèäøå ðåàãóâàííÿ.")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Tooltip("×è ³íâåðòóâàòè ãîðèçîíòàëüíèé ðóõ ìèø³ (Mouse X)?")]
    public bool invertMouseX = false;
    [Tooltip("×è ³íâåðòóâàòè âåðòèêàëüíèé ðóõ ìèø³ (Mouse Y)?")]
    public bool invertMouseY = false;
    [Tooltip("Ìåðòâà çîíà ìèø³. Ðóõ ìèø³ ìåíøå öüîãî çíà÷åííÿ áóäå ³ãíîðóâàòèñÿ.")]
    [Range(0f, 0.1f)]
    public float mouseDeadZone = 0.01f;

    [Tooltip("Ì³í³ìàëüíèé êóò îãëÿäó âíèç äëÿ êàìåðè (ïî ëîêàëüí³é îñ³ X).")]
    public float minimumX = -90f;
    [Tooltip("Ìàêñèìàëüíèé êóò îãëÿäó âãîðó äëÿ êàìåðè (ïî ëîêàëüí³é îñ³ X).")]
    public float maximumX = 90f;

    [Tooltip("Òðàíñôîðì ò³ëà ãðàâöÿ. Áóäå îáåðòàòèñÿ ïî ñâ³òîâ³é îñ³ Y (ãîðèçîíòàëüíî).")]
    public Transform playerBody;
    [Tooltip("Òðàíñôîðì êàìåðè ãðàâöÿ. Áóäå îáåðòàòèñÿ ïî ¿¿ ëîêàëüí³é îñ³ X (âåðòèêàëüíî).")]
    public Transform playerCamera;

    [Header("Cursor Settings")]
    [Tooltip("×è ïîòð³áíî áëîêóâàòè êóðñîð ïðè ñòàðò³ ãðè?")]
    public bool lockCursorOnStart = true;
    [Tooltip("Êíîïêà äëÿ âèâ³ëüíåííÿ êóðñîðà (çàçâè÷àé Escape).")]
    public KeyCode unlockCursorKey = KeyCode.Escape;

    // Ïðèâàòí³ çì³íí³
    private Quaternion initialPlayerBodyRotation;
    private Quaternion initialPlayerCameraLocalRotation;
    private bool isCursorLocked = false;

    private float currentCameraRotationX = 0f; // Â³äñòåæóºìî ïîòî÷íèé íàêîïè÷åíèé êóò äëÿ âåðòèêàëüíîãî îáåðòàííÿ êàìåðè
    private float currentBodyRotationY = 0f; // Â³äñòåæóºìî ïîòî÷íèé íàêîïè÷åíèé êóò äëÿ ãîðèçîíòàëüíîãî îáåðòàííÿ ãðàâöÿ (ïî îñ³ Y)

    void Awake()
    {
        // Ïåðåâ³ðêà òà ïðèçíà÷åííÿ playerBody
        if (playerBody == null)
        {
            playerBody = transform;
            Debug.LogWarning("MouseLook: Player Body Transform íå ïðèçíà÷åíî. Âèêîðèñòîâóºòüñÿ Transform öüîãî æ GameObject: " + name);
        }
        if (playerBody == null)
        {
            Debug.LogError("MouseLook: Íåâäàëîñÿ çíàéòè Player Body Transform. Ñêðèïò âèìêíåíî.");
            enabled = false;
            return;
        }

        // Ïåðåâ³ðêà òà ïðèçíà÷åííÿ playerCamera
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
            if (playerCamera == null)
            {
                Debug.LogError("MouseLook: Player Camera Transform íå ïðèçíà÷åíî ³ Main Camera íå çíàéäåíî. Ñêðèïò âèìêíåíî.");
                enabled = false;
                return;
            }
        }

        // --- Çáåð³ãàºìî ïî÷àòêîâ³ îáåðòàííÿ ïðè ñòàðò³ ãðè ---
        initialPlayerBodyRotation = playerBody.rotation;
        initialPlayerCameraLocalRotation = playerCamera.localRotation;

        // ²í³ö³àë³çóºìî íàêîïè÷åí³ êóòè ç ïîòî÷íèõ îáåðòàíü, ùîá çáåðåãòè ïî÷àòêîâ³ íàëàøòóâàííÿ ç ³íñïåêòîðà.
        // Òåïåð öå ïî îñ³ Y äëÿ ãðàâöÿ.
        Vector3 initialBodyEuler = playerBody.rotation.eulerAngles;
        currentBodyRotationY = initialBodyEuler.y;
        if (currentBodyRotationY > 180) currentBodyRotationY -= 360;

        // Ïî îñ³ X äëÿ êàìåðè.
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
        // Îáðîáêà áëîêóâàííÿ/ðîçáëîêóâàííÿ êóðñîðà
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

        // --- Îòðèìàííÿ âõ³äíèõ äàíèõ ìèø³ ---
        float rawMouseX = Input.GetAxisRaw("Mouse X");
        float rawMouseY = Input.GetAxisRaw("Mouse Y");

        // Çàñòîñóâàííÿ ìåðòâî¿ çîíè
        if (Mathf.Abs(rawMouseX) < mouseDeadZone) rawMouseX = 0f;
        if (Mathf.Abs(rawMouseY) < mouseDeadZone) rawMouseY = 0f;

        // Çàñòîñóâàííÿ ³íâåðñ³¿
        if (invertMouseX) rawMouseX *= -1f;
        if (invertMouseY) rawMouseY *= -1f;

        // --- Ãîðèçîíòàëüíå îáåðòàííÿ ãðàâöÿ (ïî ñâ³òîâ³é îñ³ Y) ---
        // Âèêîðèñòîâóºìî Mouse X äëÿ îáåðòàííÿ ïî Y
        float horizontalAngleChange = rawMouseX * horizontalSensitivity * Time.deltaTime;
        currentBodyRotationY += horizontalAngleChange; // Íàêîïè÷óºìî êóò äëÿ Y-îñ³
        
        // Çãëàäæóºìî òà çàñòîñîâóºìî îáåðòàííÿ äî playerBody
        // Quaternion.Euler(0, currentBodyRotationY, 0) ñòâîðþº îáåðòàííÿ ò³ëüêè ïî Y.
        // Ìè çàñòîñîâóºìî éîãî â³äíîñíî initialPlayerBodyRotation.
        Quaternion targetBodyRotation = initialPlayerBodyRotation * Quaternion.Euler(0, currentBodyRotationY, 0);
        playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, rotationSmoothness);


        // --- Âåðòèêàëüíå îáåðòàííÿ êàìåðè (ïî ¿¿ ëîêàëüí³é îñ³ X) ---
        // Âèêîðèñòîâóºìî Mouse Y äëÿ îáåðòàííÿ ïî X
        float verticalAngleChange = rawMouseY * verticalSensitivity * Time.deltaTime;
        currentCameraRotationX -= verticalAngleChange; // Â³äí³ìàºìî, ùîá ðóõ ìèø³ âãîðó äèâèâñÿ âãîðó
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX); // Îáìåæóºìî êóòè

        // Çãëàäæóºìî òà çàñòîñîâóºìî îáåðòàííÿ äî playerCamera
        // Quaternion.Euler(currentCameraRotationX, 0, 0) ñòâîðþº îáåðòàííÿ ò³ëüêè ïî X.
        // Ìè çàñòîñîâóºìî éîãî â³äíîñíî initialPlayerCameraLocalRotation.
        Quaternion targetCameraRotation = initialPlayerCameraLocalRotation * Quaternion.Euler(currentCameraRotationX, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, rotationSmoothness);
    }

    /// <summary>
    /// Ìåòîä äëÿ êåðóâàííÿ ñòàíîì áëîêóâàííÿ êóðñîðà.
    /// </summary>
    /// <param name="lockState">True, ùîá çàáëîêóâàòè òà ïðèõîâàòè êóðñîð; False, ùîá ïîêàçàòè.</param>
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
    /// Ìåòîä äëÿ ñêèäàííÿ îáåðòàííÿ ãðàâöÿ òà êàìåðè äî ïî÷àòêîâîãî ñòàíó.
    /// Òàêîæ ñêèäàº âíóòð³øí³ íàêîïè÷åí³ êóòè.
    /// </summary>
    public void ResetLookToInitial()
    {
        if (playerBody != null)
        {
            playerBody.rotation = initialPlayerBodyRotation;
            Vector3 initialBodyEuler = initialPlayerBodyRotation.eulerAngles;
            currentBodyRotationY = initialBodyEuler.y; // Ñêèäàºìî äî ïî÷àòêîâî¿ Y
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
    /// Ïîâåðòàº ïîòî÷íèé ãîðèçîíòàëüíèé êóò îáåðòàííÿ ãðàâöÿ (íàêîïè÷åíèé ïî ñâ³òîâ³é îñ³ Y).
    /// </summary>
    public float GetCurrentHorizontalRotation()
    {
        return currentBodyRotationY;
    }

    /// <summary>
    /// Ïîâåðòàº ïîòî÷íèé âåðòèêàëüíèé êóò îáåðòàííÿ êàìåðè (íàêîïè÷åíèé ïî ëîêàëüí³é îñ³ X).
    /// </summary>
    public float GetCurrentVerticalRotation()
    {
        return currentCameraRotationX;
    }

    /// <summary>
    /// Ïîâåðòàº, ÷è êóðñîð çàáëîêîâàíèé.
    /// </summary>
    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }

    /// <summary>
    /// Çàñòîñîâóº â³ääà÷ó çáðî¿ äî êàìåðè (SCOUT ÂÈÏÐÀÂËÅÍÍß)
    /// </summary>
    public void ApplyRecoil(float recoilX, float recoilY)
    {
        // Çàñòîñîâóºìî âåðòèêàëüíó â³ääà÷ó (âãîðó)
        currentCameraRotationX -= recoilY;
        
        // Çàñòîñîâóºìî ãîðèçîíòàëüíó â³ääà÷ó (îáåðòàííÿ ò³ëà)
        transform.Rotate(Vector3.up * recoilX);
        
        // Îáìåæóºìî âåðòèêàëüíèé êóò ï³ñëÿ â³ääà÷³
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX);
        
        // Çàñòîñîâóºìî îáìåæåíèé êóò äî êàìåðè
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentCameraRotationX, 0f, 0f);
        }
    }
}