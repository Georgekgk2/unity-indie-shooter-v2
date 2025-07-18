using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    [Tooltip("Чутливість миші для горизонтального обертання гравця (по світовій осі Y).")]
    public float horizontalSensitivity = 100f;
    [Tooltip("Чутливість миші для вертикального обертання камери (по її локальній осі X).")]
    public float verticalSensitivity = 100f;

    [Tooltip("Плавність обертання. Вище значення = швидше реагування.")]
    [Range(0.01f, 1f)]
    public float rotationSmoothness = 0.1f;

    [Tooltip("Чи інвертувати горизонтальний рух миші (Mouse X)?")]
    public bool invertMouseX = false;
    [Tooltip("Чи інвертувати вертикальний рух миші (Mouse Y)?")]
    public bool invertMouseY = false;
    [Tooltip("Мертва зона миші. Рух миші менше цього значення буде ігноруватися.")]
    [Range(0f, 0.1f)]
    public float mouseDeadZone = 0.01f;

    [Tooltip("Мінімальний кут огляду вниз для камери (по локальній осі X).")]
    public float minimumX = -90f;
    [Tooltip("Максимальний кут огляду вгору для камери (по локальній осі X).")]
    public float maximumX = 90f;

    [Tooltip("Трансформ тіла гравця. Буде обертатися по світовій осі Y (горизонтально).")]
    public Transform playerBody;
    [Tooltip("Трансформ камери гравця. Буде обертатися по її локальній осі X (вертикально).")]
    public Transform playerCamera;

    [Header("Cursor Settings")]
    [Tooltip("Чи потрібно блокувати курсор при старті гри?")]
    public bool lockCursorOnStart = true;
    [Tooltip("Кнопка для вивільнення курсора (зазвичай Escape).")]
    public KeyCode unlockCursorKey = KeyCode.Escape;

    // Приватні змінні
    private Quaternion initialPlayerBodyRotation;
    private Quaternion initialPlayerCameraLocalRotation;
    private bool isCursorLocked = false;

    private float currentCameraRotationX = 0f; // Відстежуємо поточний накопичений кут для вертикального обертання камери
    private float currentBodyRotationY = 0f; // Відстежуємо поточний накопичений кут для горизонтального обертання гравця (по осі Y)

    void Awake()
    {
        // Перевірка та призначення playerBody
        if (playerBody == null)
        {
            playerBody = transform;
            Debug.LogWarning("MouseLook: Player Body Transform не призначено. Використовується Transform цього ж GameObject: " + name);
        }
        if (playerBody == null)
        {
            Debug.LogError("MouseLook: Невдалося знайти Player Body Transform. Скрипт вимкнено.");
            enabled = false;
            return;
        }

        // Перевірка та призначення playerCamera
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
            if (playerCamera == null)
            {
                Debug.LogError("MouseLook: Player Camera Transform не призначено і Main Camera не знайдено. Скрипт вимкнено.");
                enabled = false;
                return;
            }
        }

        // --- Зберігаємо початкові обертання при старті гри ---
        initialPlayerBodyRotation = playerBody.rotation;
        initialPlayerCameraLocalRotation = playerCamera.localRotation;

        // Ініціалізуємо накопичені кути з поточних обертань, щоб зберегти початкові налаштування з інспектора.
        // Тепер це по осі Y для гравця.
        Vector3 initialBodyEuler = playerBody.rotation.eulerAngles;
        currentBodyRotationY = initialBodyEuler.y;
        if (currentBodyRotationY > 180) currentBodyRotationY -= 360;

        // По осі X для камери.
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
        // Обробка блокування/розблокування курсора
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

        // --- Отримання вхідних даних миші ---
        float rawMouseX = Input.GetAxisRaw("Mouse X");
        float rawMouseY = Input.GetAxisRaw("Mouse Y");

        // Застосування мертвої зони
        if (Mathf.Abs(rawMouseX) < mouseDeadZone) rawMouseX = 0f;
        if (Mathf.Abs(rawMouseY) < mouseDeadZone) rawMouseY = 0f;

        // Застосування інверсії
        if (invertMouseX) rawMouseX *= -1f;
        if (invertMouseY) rawMouseY *= -1f;

        // --- Горизонтальне обертання гравця (по світовій осі Y) ---
        // Використовуємо Mouse X для обертання по Y
        float horizontalAngleChange = rawMouseX * horizontalSensitivity * Time.deltaTime;
        currentBodyRotationY += horizontalAngleChange; // Накопичуємо кут для Y-осі
        
        // Згладжуємо та застосовуємо обертання до playerBody
        // Quaternion.Euler(0, currentBodyRotationY, 0) створює обертання тільки по Y.
        // Ми застосовуємо його відносно initialPlayerBodyRotation.
        Quaternion targetBodyRotation = initialPlayerBodyRotation * Quaternion.Euler(0, currentBodyRotationY, 0);
        playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRotation, rotationSmoothness);


        // --- Вертикальне обертання камери (по її локальній осі X) ---
        // Використовуємо Mouse Y для обертання по X
        float verticalAngleChange = rawMouseY * verticalSensitivity * Time.deltaTime;
        currentCameraRotationX -= verticalAngleChange; // Віднімаємо, щоб рух миші вгору дивився вгору
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX); // Обмежуємо кути

        // Згладжуємо та застосовуємо обертання до playerCamera
        // Quaternion.Euler(currentCameraRotationX, 0, 0) створює обертання тільки по X.
        // Ми застосовуємо його відносно initialPlayerCameraLocalRotation.
        Quaternion targetCameraRotation = initialPlayerCameraLocalRotation * Quaternion.Euler(currentCameraRotationX, 0, 0);
        playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, targetCameraRotation, rotationSmoothness);
    }

    /// <summary>
    /// Метод для керування станом блокування курсора.
    /// </summary>
    /// <param name="lockState">True, щоб заблокувати та приховати курсор; False, щоб показати.</param>
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
    /// Метод для скидання обертання гравця та камери до початкового стану.
    /// Також скидає внутрішні накопичені кути.
    /// </summary>
    public void ResetLookToInitial()
    {
        if (playerBody != null)
        {
            playerBody.rotation = initialPlayerBodyRotation;
            Vector3 initialBodyEuler = initialPlayerBodyRotation.eulerAngles;
            currentBodyRotationY = initialBodyEuler.y; // Скидаємо до початкової Y
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
    /// Повертає поточний горизонтальний кут обертання гравця (накопичений по світовій осі Y).
    /// </summary>
    public float GetCurrentHorizontalRotation()
    {
        return currentBodyRotationY;
    }

    /// <summary>
    /// Повертає поточний вертикальний кут обертання камери (накопичений по локальній осі X).
    /// </summary>
    public float GetCurrentVerticalRotation()
    {
        return currentCameraRotationX;
    }

    /// <summary>
    /// Повертає, чи курсор заблокований.
    /// </summary>
    public bool IsCursorLocked()
    {
        return isCursorLocked;
    }

    /// <summary>
    /// Застосовує віддачу зброї до камери (SCOUT ВИПРАВЛЕННЯ)
    /// </summary>
    public void ApplyRecoil(float recoilX, float recoilY)
    {
        // Застосовуємо вертикальну віддачу (вгору)
        currentCameraRotationX -= recoilY;
        
        // Застосовуємо горизонтальну віддачу (обертання тіла)
        transform.Rotate(Vector3.up * recoilX);
        
        // Обмежуємо вертикальний кут після віддачі
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, minimumX, maximumX);
        
        // Застосовуємо обмежений кут до камери
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentCameraRotationX, 0f, 0f);
        }
    }
}