using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings (Mouse Input)")]
    [Tooltip("Íàñê³ëüêè ñèëüíî çáðîÿ áóäå ðóõàòèñÿ ïî ãîðèçîíòàë³ (â³ä ðóõó ìèø³ X)")]
    public float swayAmountX = 0.5f;
    [Tooltip("Íàñê³ëüêè ñèëüíî çáðîÿ áóäå ðóõàòèñÿ ïî âåðòèêàë³ (â³ä ðóõó ìèø³ Y)")]
    public float swayAmountY = 0.5f;
    [Tooltip("Ïëàâí³ñòü ïîâåðíåííÿ çáðî¿ äî ïî÷àòêîâî¿ ïîçèö³¿")]
    public float smoothAmount = 6f;
    [Tooltip("Ì³í³ìàëüíèé ðóõ ìèø³ äëÿ àêòèâàö³¿ sway (ìåðòâà çîíà)")]
    public float mouseDeadZone = 0.001f;
    [Tooltip("Ìàêñèìàëüíå â³äõèëåííÿ çáðî¿ â³ä öåíòðó (ó ãðàäóñàõ)")]
    public float maxSwayAngle = 5f;


    [Header("Bob Settings (Walk/Run Input)")]
    [Tooltip("Àìïë³òóäà (âåëè÷èíà) õèòàííÿ çáðî¿ ïî X òà Y îñÿõ ï³ä ÷àñ õîäüáè.")]
    public Vector2 walkBobAmount = new Vector2(0.1f, 0.1f);
    [Tooltip("×àñòîòà õèòàííÿ çáðî¿ ï³ä ÷àñ õîäüáè.")]
    public float walkBobFrequency = 10f;
    [Tooltip("Ìíîæíèê àìïë³òóäè õèòàííÿ çáðî¿ ïðè á³ãó.")]
    public float sprintBobAmountMultiplier = 2f;
    [Tooltip("Ìíîæíèê ÷àñòîòè õèòàííÿ çáðî¿ ïðè á³ãó.")]
    public float sprintBobFrequencyMultiplier = 1.5f;
    [Tooltip("Àìïë³òóäà õèòàííÿ çáðî¿ ïðè ïðèñ³äàíí³.")]
    public Vector2 crouchBobAmount = new Vector2(0.006f, 0.006f);
    [Tooltip("×àñòîòà õèòàííÿ çáðî¿ ïðè ïðèñ³äàíí³.")]
    public float crouchBobFrequency = 7f;
    [Tooltip("Ì³í³ìàëüíà øâèäê³ñòü ðóõó ãðàâöÿ äëÿ àêòèâàö³¿ bob.")]
    public float minBobSpeed = 0.1f;

    [Header("Vertical Speed Tilt Settings")]
    [Tooltip("Íàñê³ëüêè ñèëüíî çáðîÿ íàõèëÿºòüñÿ âíèç ïðè ñòðèáêó (â³ä 0 äî 1).")]
    [Range(0f, 1f)]
    public float jumpTiltAmount = 0.3f;
    [Tooltip("Íàñê³ëüêè ñèëüíî çáðîÿ íàõèëÿºòüñÿ âãîðó ïðè ïàä³íí³ (â³ä 0 äî 1).")]
    [Range(0f, 1f)]
    public float fallTiltAmount = 0.3f;
    [Tooltip("Ìàêñèìàëüíèé êóò íàõèëó çáðî¿ â³ä âåðòèêàëüíî¿ øâèäêîñò³.")]
    public float maxTiltAngle = 10f;
    [Tooltip("Ïëàâí³ñòü íàõèëó çáðî¿ â³ä âåðòèêàëüíî¿ øâèäêîñò³.")]
    public float tiltSmoothSpeed = 5f;


    // Ïðèâàòí³ çì³íí³
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    private float bobTimer;
    
    // Ïîñèëàííÿ íà PlayerMovement òà Rigidbody ãðàâöÿ
    private PlayerMovement playerMovement;
    private Rigidbody playerRb; 

    // Ïðîì³æí³ îáåðòàííÿ äëÿ êîìá³íóâàííÿ
    private Quaternion calculatedSwayRotation; // Îáåðòàííÿ â³ä ðóõó ìèø³
    private Quaternion calculatedTiltRotation; // Îáåðòàííÿ â³ä âåðòèêàëüíî¿ øâèäêîñò³


    void Awake() // Âèêîðèñòîâóºìî Awake äëÿ ³í³ö³àë³çàö³¿
    {
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("WeaponSway: PlayerMovement ñêðèïò íå çíàéäåíî â áàòüê³âñüêèõ îá'ºêòàõ. Äåÿê³ ôóíêö³¿ WeaponSway íå ïðàöþâàòèìóòü.", this);
        }
        else
        {
            playerRb = playerMovement.GetComponent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogWarning("WeaponSway: Rigidbody íå çíàéäåíî íà îá'ºêò³ PlayerMovement. Vertical Speed Tilt íå ïðàöþâàòèìå.", this);
            }
        }
    }

    void Update()
    {
        // 1. Îá÷èñëþºìî Sway (ðóõ ìèø³)
        calculatedSwayRotation = CalculateWeaponSway();

        // 2. Îá÷èñëþºìî Bob (ðóõ ãðàâöÿ WASD)
        HandleWeaponBob(); // Öåé ìåòîä çì³íþº transform.localPosition

        // 3. Îá÷èñëþºìî Vertical Speed Tilt
        calculatedTiltRotation = CalculateVerticalSpeedTilt();

        // 4. Êîìá³íóºìî âñ³ îáåðòàííÿ òà çàñòîñîâóºìî äî çáðî¿
        // Ïîðÿäîê ìíîæåííÿ: initialLocalRotation * (sway) * (tilt)
        // (x * y) - öå ñïî÷àòêó y, ïîò³ì x
        // Òîìó ÿêùî ìè õî÷åìî, ùîá Tilt áóâ "íàä" Sway, òî Tilt éäå îñòàíí³ì ó ìíîæåíí³,
        // àáî ïåðøèì, ÿêùî ìíîæèìî ñïðàâà íàë³âî (Quaternion.Euler(tilt) * Quaternion.Euler(sway))
        // Ñïðîáóºìî: initial (áàçîâå îáåðòàííÿ) * tilt * sway.
        // Çàçâè÷àé, sway/tilt çàñòîñîâóþòüñÿ â³äíîñíî áàçîâîãî îáåðòàííÿ.
        
        // Final Rotation = Áàçîâå îáåðòàííÿ * (Sway Rotation) * (Tilt Rotation)
        Quaternion finalRotation = initialLocalRotation * calculatedSwayRotation * calculatedTiltRotation;

        // Ïëàâíî çàñòîñîâóºìî ê³íöåâå îáåðòàííÿ
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * smoothAmount);
    }

    /// <summary>
    /// Îá÷èñëþº îáåðòàííÿ çáðî¿ â³ä ðóõó ìèø³ (Sway).
    /// </summary>
    /// <returns>Quaternion, ùî ïðåäñòàâëÿº îáåðòàííÿ Sway.</returns>
    Quaternion CalculateWeaponSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) < mouseDeadZone) mouseX = 0f;
        if (Mathf.Abs(mouseY) < mouseDeadZone) mouseY = 0f;
        
        float rotationAmountX = -mouseY * swayAmountY;
        float rotationAmountY = mouseX * swayAmountX;

        rotationAmountX = Mathf.Clamp(rotationAmountX, -maxSwayAngle, maxSwayAngle);
        rotationAmountY = Mathf.Clamp(rotationAmountY, -maxSwayAngle, maxSwayAngle);
        
        return Quaternion.Euler(rotationAmountX, rotationAmountY, 0);
    }

    /// <summary>
    /// Îáðîáëÿº ïîçèö³éíå õèòàííÿ çáðî¿ â³ä ðóõó ãðàâöÿ (Bob).
    /// </summary>
    void HandleWeaponBob()
    {
        if (playerMovement == null || playerRb == null) return;

        float currentHorizontalSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
        
        if (currentHorizontalSpeed < minBobSpeed) {
             bobTimer = 0;
             transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * walkBobFrequency);
             return;
        }

        float currentBobFrequency = walkBobFrequency;
        Vector2 currentBobAmount = walkBobAmount;

        if (playerMovement.IsSprinting())
        {
            currentBobFrequency *= sprintBobFrequencyMultiplier;
            currentBobAmount *= sprintBobAmountMultiplier;
        }
        else if (playerMovement.IsCrouching() || playerMovement.IsSliding() || playerMovement.IsDashing())
        {
            currentBobFrequency = crouchBobFrequency;
            currentBobAmount = crouchBobAmount;
        }

        bobTimer += Time.deltaTime * currentBobFrequency;

        float bobOffsetX = Mathf.Cos(bobTimer / 2f) * currentBobAmount.x;
        float bobOffsetY = Mathf.Sin(bobTimer) * currentBobAmount.y;

        transform.localPosition = new Vector3(
            initialLocalPosition.x + bobOffsetX,
            initialLocalPosition.y + bobOffsetY,
            initialLocalPosition.z
        );
    }

    /// <summary>
    /// Îá÷èñëþº îáåðòàííÿ çáðî¿ â³ä âåðòèêàëüíî¿ øâèäêîñò³ ãðàâöÿ (Tilt).
    /// </summary>
    /// <returns>Quaternion, ùî ïðåäñòàâëÿº îáåðòàííÿ Tilt.</returns>
    Quaternion CalculateVerticalSpeedTilt()
    {
        if (playerRb == null) return Quaternion.identity;

        float verticalVelocity = playerRb.velocity.y;
        float targetTiltAngle = 0f;

        if (verticalVelocity > 0.1f) // Ñòðèáàºìî (ðóõàºìîñÿ âãîðó)
        {
            targetTiltAngle = -verticalVelocity * jumpTiltAmount; // Ì³íóñ, ùîá íàõèëÿëî âíèç
        }
        else if (verticalVelocity < -0.1f) // Ïàäàºìî (ðóõàºìîñÿ âíèç)
        {
            targetTiltAngle = -verticalVelocity * fallTiltAmount; // Ì³íóñ, ùîá íàõèëÿëî âãîðó (â³ä'ºìíà øâèäê³ñòü * â³ä'ºìíèé êîåô³ö³ºíò = ïîçèòèâíèé êóò)
        }

        targetTiltAngle = Mathf.Clamp(targetTiltAngle, -maxTiltAngle, maxTiltAngle);

        // Ïëàâíî ïåðåõîäèìî äî ö³ëüîâîãî êóòà
        // Öå çíà÷åííÿ áóäå Lerp'àòèñÿ â Update() ïðè çàñòîñóâàíí³ finalRotation.
        // Òîìó òóò ïðîñòî ïîâåðòàºìî îáåðòàííÿ, ùî ïðåäñòàâëÿº öåé íàõèë.
        return Quaternion.Euler(targetTiltAngle, 0, 0);
    }
}