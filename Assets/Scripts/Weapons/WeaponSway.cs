using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings (Mouse Input)")]
    [Tooltip("�������� ������ ����� ���� �������� �� ���������� (�� ���� ���� X)")]
    public float swayAmountX = 0.5f;
    [Tooltip("�������� ������ ����� ���� �������� �� �������� (�� ���� ���� Y)")]
    public float swayAmountY = 0.5f;
    [Tooltip("�������� ���������� ���� �� ��������� �������")]
    public float smoothAmount = 6f;
    [Tooltip("̳�������� ��� ���� ��� ��������� sway (������ ����)")]
    public float mouseDeadZone = 0.001f;
    [Tooltip("����������� ��������� ���� �� ������ (� ��������)")]
    public float maxSwayAngle = 5f;


    [Header("Bob Settings (Walk/Run Input)")]
    [Tooltip("�������� (��������) ������� ���� �� X �� Y ���� �� ��� ������.")]
    public Vector2 walkBobAmount = new Vector2(0.1f, 0.1f);
    [Tooltip("������� ������� ���� �� ��� ������.")]
    public float walkBobFrequency = 10f;
    [Tooltip("������� �������� ������� ���� ��� ���.")]
    public float sprintBobAmountMultiplier = 2f;
    [Tooltip("������� ������� ������� ���� ��� ���.")]
    public float sprintBobFrequencyMultiplier = 1.5f;
    [Tooltip("�������� ������� ���� ��� ��������.")]
    public Vector2 crouchBobAmount = new Vector2(0.006f, 0.006f);
    [Tooltip("������� ������� ���� ��� ��������.")]
    public float crouchBobFrequency = 7f;
    [Tooltip("̳������� �������� ���� ������ ��� ��������� bob.")]
    public float minBobSpeed = 0.1f;

    [Header("Vertical Speed Tilt Settings")]
    [Tooltip("�������� ������ ����� ����������� ���� ��� ������� (�� 0 �� 1).")]
    [Range(0f, 1f)]
    public float jumpTiltAmount = 0.3f;
    [Tooltip("�������� ������ ����� ����������� ����� ��� ����� (�� 0 �� 1).")]
    [Range(0f, 1f)]
    public float fallTiltAmount = 0.3f;
    [Tooltip("������������ ��� ������ ���� �� ����������� ��������.")]
    public float maxTiltAngle = 10f;
    [Tooltip("�������� ������ ���� �� ����������� ��������.")]
    public float tiltSmoothSpeed = 5f;


    // ������� ����
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalPosition;

    private float bobTimer;
    
    // ��������� �� PlayerMovement �� Rigidbody ������
    private PlayerMovement playerMovement;
    private Rigidbody playerRb; 

    // ������ ��������� ��� �����������
    private Quaternion calculatedSwayRotation; // ��������� �� ���� ����
    private Quaternion calculatedTiltRotation; // ��������� �� ����������� ��������


    void Awake() // ������������� Awake ��� �����������
    {
        initialLocalRotation = transform.localRotation;
        initialLocalPosition = transform.localPosition;

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("WeaponSway: PlayerMovement ������ �� �������� � ����������� ��'�����. ���� ������� WeaponSway �� �������������.", this);
        }
        else
        {
            playerRb = playerMovement.GetComponent<Rigidbody>();
            if (playerRb == null)
            {
                Debug.LogWarning("WeaponSway: Rigidbody �� �������� �� ��'��� PlayerMovement. Vertical Speed Tilt �� �����������.", this);
            }
        }
    }

    void Update()
    {
        // 1. ���������� Sway (��� ����)
        calculatedSwayRotation = CalculateWeaponSway();

        // 2. ���������� Bob (��� ������ WASD)
        HandleWeaponBob(); // ��� ����� ����� transform.localPosition

        // 3. ���������� Vertical Speed Tilt
        calculatedTiltRotation = CalculateVerticalSpeedTilt();

        // 4. �������� �� ��������� �� ����������� �� ����
        // ������� ��������: initialLocalRotation * (sway) * (tilt)
        // (x * y) - �� �������� y, ���� x
        // ���� ���� �� ������, ��� Tilt ��� "���" Sway, �� Tilt ��� ������� � �������,
        // ��� ������, ���� ������� ������ ����� (Quaternion.Euler(tilt) * Quaternion.Euler(sway))
        // ��������: initial (������ ���������) * tilt * sway.
        // ��������, sway/tilt �������������� ������� �������� ���������.
        
        // Final Rotation = ������ ��������� * (Sway Rotation) * (Tilt Rotation)
        Quaternion finalRotation = initialLocalRotation * calculatedSwayRotation * calculatedTiltRotation;

        // ������ ����������� ������ ���������
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * smoothAmount);
    }

    /// <summary>
    /// �������� ��������� ���� �� ���� ���� (Sway).
    /// </summary>
    /// <returns>Quaternion, �� ����������� ��������� Sway.</returns>
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
    /// �������� ��������� ������� ���� �� ���� ������ (Bob).
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
    /// �������� ��������� ���� �� ����������� �������� ������ (Tilt).
    /// </summary>
    /// <returns>Quaternion, �� ����������� ��������� Tilt.</returns>
    Quaternion CalculateVerticalSpeedTilt()
    {
        if (playerRb == null) return Quaternion.identity;

        float verticalVelocity = playerRb.velocity.y;
        float targetTiltAngle = 0f;

        if (verticalVelocity > 0.1f) // �������� (�������� �����)
        {
            targetTiltAngle = -verticalVelocity * jumpTiltAmount; // ̳���, ��� �������� ����
        }
        else if (verticalVelocity < -0.1f) // ������ (�������� ����)
        {
            targetTiltAngle = -verticalVelocity * fallTiltAmount; // ̳���, ��� �������� ����� (��'���� �������� * ��'����� ���������� = ���������� ���)
        }

        targetTiltAngle = Mathf.Clamp(targetTiltAngle, -maxTiltAngle, maxTiltAngle);

        // ������ ���������� �� ��������� ����
        // �� �������� ���� Lerp'����� � Update() ��� ����������� finalRotation.
        // ���� ��� ������ ��������� ���������, �� ����������� ��� �����.
        return Quaternion.Euler(targetTiltAngle, 0, 0);
    }
}