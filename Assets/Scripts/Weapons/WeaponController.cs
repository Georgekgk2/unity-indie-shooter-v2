using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Setup")]
    [Tooltip("Ïðåôàá êóë³, ÿêà áóäå ñïàâíèòèñÿ")]
    public GameObject bulletPrefab;
    [Tooltip("Òî÷êà, ç ÿêî¿ áóäå ñïàâíèòèñÿ ³ çàïóñêàòèñÿ êóëÿ")]
    public Transform bulletSpawnPoint;
    [Tooltip("Ñèëà, ç ÿêîþ êóëÿ áóäå çàïóùåíà")]
    public float bulletForce = 30f;
    [Tooltip("Ìíîæíèê ðîçêèäó êóë³ (÷èì âèùå, òèì á³ëüøå ðîçêèä)")]
    public float bulletSpread = 0.05f;

    [Tooltip("×è ìàº êóëÿ ëåò³òè ïðÿìî â öåíòð åêðàíó (Raycast Aiming)?")]
    public bool useRaycastAiming = true;
    [Tooltip("Ìàêñèìàëüíà â³äñòàíü äëÿ Raycast Aiming")]
    public float maxAimDistance = 500f;


    [Header("Fire Rate & Ammo")]
    [Tooltip("Ê³ëüê³ñòü ïîñòð³ë³â çà ñåêóíäó")]
    public float fireRate = 8f;
    [Tooltip("Ðîçì³ð ìàãàçèíó (ê³ëüê³ñòü ïàòðîí³â)")]
    public int magazineSize = 30;
    [Tooltip("×àñ ïåðåçàðÿäêè (ó ñåêóíäàõ)")]
    public float reloadTime = 2f;
    [Tooltip("Êíîïêà äëÿ ïåðåçàðÿäêè")]
    public KeyCode reloadKey = KeyCode.R;
    [Tooltip("Ê³ëüê³ñòü äîñòóïíèõ ïåðåçàðÿäîê äëÿ ö³º¿ çáðî¿")] // ÍÎÂÅ ÏÎËÅ
    [SerializeField] private int reloadCharges = 1; // Çà çàìîâ÷óâàííÿì äàºìî 1 çàðÿä

    [Header("Visual & Audio Effects")]
    [Tooltip("Ñèñòåìà ÷àñòèíîê äëÿ ñïàëàõó äóëà")]
    public ParticleSystem muzzleFlash;
    [Tooltip("Àóä³î äæåðåëî äëÿ ïîñòð³ë³â")]
    public AudioSource audioSource;
    [Tooltip("Çâóêîâèé êë³ï äëÿ ïîñòð³ëó")]
    public AudioClip shootSoundClip;
    [Tooltip("Çâóêîâèé êë³ï äëÿ ïåðåçàðÿäêè")]
    public AudioClip reloadSoundClip;
    [Tooltip("Çâóêîâèé êë³ï, êîëè íåìàº ïàòðîí³â")]
    public AudioClip emptyClipSound;

    [Header("Aim Down Sights (ADS) Settings")]
    [Tooltip("×è ï³äòðèìóº öÿ çáðîÿ ðåæèì ïðèö³ëþâàííÿ (ADS)?")]
    public bool canAim = true;
    [Tooltip("Êíîïêà äëÿ âõîäó/âèõîäó ç ðåæèìó ïðèö³ëþâàííÿ.")]
    public KeyCode aimKey = KeyCode.Mouse1;
    [Tooltip("Ëîêàëüíà ïîçèö³ÿ çáðî¿, êîëè âîíà ïðèö³ëåíà (ADS).")]
    public Vector3 aimPosition;
    [Tooltip("Øâèäê³ñòü ïåðåõîäó äî/â³ä ðåæèìó ïðèö³ëþâàííÿ.")]
    public float aimSpeed = 10f;
    [Tooltip("Çíà÷åííÿ FOV êàìåðè, êîëè çáðîÿ ïðèö³ëåíà.")]
    public float aimFOV = 40f;
    [Tooltip("Ìíîæíèê ðîçêèäó êóë³, êîëè çáðîÿ ïðèö³ëåíà (1.0 = áåç çì³í, 0.5 = ïîëîâèíà ðîçêèäó).")]
    [Range(0f, 1f)]
    public float aimSpreadMultiplier = 0.5f;

    [Header("Recoil Settings")]
    [Tooltip("Âåðòèêàëüíà â³ääà÷à (ÿê ñèëüíî êàìåðà ï³äêèäàºòüñÿ âãîðó)")]
    public float recoilX = 2f;
    [Tooltip("Ãîðèçîíòàëüíà â³ääà÷à (ÿê ñèëüíî êàìåðà â³äõèëÿºòüñÿ âë³âî/âïðàâî)")]
    public float recoilY = 0.5f;
    [Tooltip("Â³ääà÷à ïî Z-îñ³ (íàõèë)")]
    public float recoilZ = 0.5f;
    [Tooltip("Øâèäê³ñòü, ç ÿêîþ â³ääà÷à çàñòîñîâóºòüñÿ")]
    public float recoilSnappiness = 10f;
    [Tooltip("Øâèäê³ñòü, ç ÿêîþ êàìåðà ïîâåðòàºòüñÿ ï³ñëÿ â³ääà÷³")]
    public float recoilReturnSpeed = 5f;
    [Tooltip("Ìíîæíèê â³ääà÷³, êîëè çáðîÿ ïðèö³ëåíà (ìåíøå 1 = ìåíøà â³ääà÷à).")]
    [Range(0f, 1f)]
    public float aimRecoilMultiplier = 0.5f;

    [Header("Pickup & Drop Settings")]
    [Tooltip("Ïðåôàá ñâ³òñüêîãî îá'ºêòà ö³º¿ çáðî¿ äëÿ âèêèäàííÿ.")]
    public GameObject weaponWorldPrefab;
    [Tooltip("×è ìîæíà âèêèíóòè öþ çáðîþ? (Íàïðèêëàä, íå ìîæíà âèêèíóòè êóëàêè àáî áàçîâèé í³æ)")]
    public bool isDroppable = true;
    [Tooltip("²ì'ÿ çáðî¿ äëÿ â³äîáðàæåííÿ â UI.")]
    public string weaponDisplayName = "Weapon";
    [Tooltip("²êîíêà çáðî¿ äëÿ â³äîáðàæåííÿ â UI.")]
    public Sprite weaponIcon;

    // Ïðèâàòí³ çì³íí³ äëÿ ñòàíó çáðî¿
    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private Camera mainCamera;

    private Vector3 initialWeaponLocalPosition;
    private float initialCameraFOV;
    private bool isAiming = false;

    // Ïîñèëàííÿ íà ³íø³ ñêðèïòè ãðàâöÿ
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private MouseLook mouseLook;

    // Çì³íí³ äëÿ êåðóâàííÿ â³ääà÷åþ
    private Vector3 currentRecoilRotation;
    private Vector3 targetRecoilRotation;

    // Êîðóòèíè äëÿ áåçïå÷íîãî óïðàâë³ííÿ
    private Coroutine reloadCoroutine;

    void Awake()
    {
        currentAmmo = magazineSize;
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("WeaponController: Main Camera íå çíàéäåíà. Ôóíêö³¿ êàìåðè ìîæóòü ïðàöþâàòè íåêîðåêòíî.", this);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("WeaponController: AudioSource íå çíàéäåíî íà îá'ºêò³ çáðî¿. Çâóêè íå áóäóòü â³äòâîðþâàòèñÿ.", this);
            }
        }

        initialWeaponLocalPosition = transform.localPosition;
        if (mainCamera != null)
        {
            initialCameraFOV = mainCamera.fieldOfView;
        }

        playerMovement = GetComponentInParent<PlayerMovement>();
        playerHealth = GetComponentInParent<PlayerHealth>();
        mouseLook = GetComponentInParent<MouseLook>();
        if (mouseLook == null)
        {
            Debug.LogWarning("WeaponController: MouseLook íå çíàéäåíî â áàòüê³âñüêèõ îá'ºêòàõ. Â³ääà÷à íå ïðàöþâàòèìå.", this);
        }
    }

    void OnEnable() // Âèêëèêàºòüñÿ, êîëè GameObject çáðî¿ àêòèâóºòüñÿ
    {
        OnEquip(); // Âèêëèêàºìî OnEquip ïðè àêòèâàö³¿ (åê³ï³ðîâö³)
    }

    void OnDisable() // Âèêëèêàºòüñÿ, êîëè GameObject çáðî¿ äåàêòèâóºòüñÿ
    {
        // Áåçïå÷íî çóïèíÿºìî âñ³ êîðóòèíè
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        
        OnUnequip(); // Âèêëèêàºìî OnUnequip ïðè äåàêòèâàö³¿ (çíÿòò³)
    }


    void Update()
    {
        // Íå äîçâîëÿºìî ñòð³ëÿòè/ïåðåçàðÿäæàòèñÿ/ïðèö³ëþâàòèñÿ, ÿêùî ãðàâåöü ìåðòâèé
        if (playerHealth != null && playerHealth.IsDead())
        {
            if (isAiming) ResetADS();
            // Ïîâåðòàºìî â³ääà÷ó, ÿêùî ãðàâåöü ìåðòâèé
            targetRecoilRotation = Vector3.zero;
            currentRecoilRotation = Vector3.zero;
            UpdateRecoil(); // Îíîâëþºìî â³ääà÷ó, ùîá âîíà çãàñàëà
            return;
        }

        // Íå äîçâîëÿºìî ñòð³ëÿòè àáî ïåðåçàðÿäæàòèñÿ, ÿêùî ìè âæå ïåðåçàðÿäæàºìîñÿ
        if (isReloading)
        {
            if (isAiming) ResetADS(); // ßêùî ïåðåçàðÿäæàºìîñÿ, âèõîäèìî ç ADS
            UpdateRecoil(); // Îíîâëþºìî â³ääà÷ó, ùîá âîíà çãàñàëà
            return;
        }

        // --- Îáðîáêà ïðèö³ëþâàííÿ (ADS) ---
        if (canAim && playerMovement != null)
        {
            // Íå äîçâîëÿºìî ïðèö³ëþâàòèñÿ ï³ä ÷àñ ñïð³íòó, êîâçàííÿ àáî ðèâêà
            if (playerMovement.IsSprinting() || playerMovement.IsSliding() || playerMovement.IsDashing())
            {
                if (isAiming) ResetADS();
            }
            else // ßêùî íå â öèõ ñòàíàõ, äîçâîëÿºìî ïðèö³ëþâàòèñÿ
            {
                if (Input.GetKey(aimKey)) // "Óòðèìóâàòè" äëÿ ïðèö³ëþâàííÿ
                {
                    if (!isAiming) isAiming = true;
                }
                else
                {
                    if (isAiming) ResetADS();
                }
            }
        }

        // Ïëàâíå ïåðåì³ùåííÿ çáðî¿ òà FOV êàìåðè
        UpdateWeaponADS();


        // Îáðîáêà ïîñòð³ëó (ËÊÌ)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                if (audioSource != null && emptyClipSound != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(emptyClipSound);
                }
                Reload(); 
            }
        }

        // Îáðîáêà ïåðåçàðÿäêè (êëàâ³øà R)
        if (Input.GetKeyDown(reloadKey) && currentAmmo < magazineSize)
        {
            Reload();
        }

        // Îíîâëåííÿ â³ääà÷³ (â³äáóâàºòüñÿ êîæåí êàäð)
        UpdateRecoil();
    }
    
    /// <summary>
    /// Ïëàâíå ïåðåì³ùåííÿ çáðî¿ äî ö³ëüîâî¿ ïîçèö³¿ (ADS àáî Hip) òà çì³íà FOV.
    /// </summary>
    void UpdateWeaponADS()
    {
        Vector3 targetPosition = isAiming ? aimPosition : initialWeaponLocalPosition;
        float targetFOV = isAiming ? aimFOV : initialCameraFOV;

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * aimSpeed);

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);
        }
    }

    /// <summary>
    /// Ìåòîä äëÿ âèêîíàííÿ ïîñòð³ëó.
    /// </summary>
    void Shoot()
    {
        nextFireTime = Time.time + 1f / fireRate;
        currentAmmo--;

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSoundClip != null)
        {
            audioSource.PlayOneShot(shootSoundClip);
        }

        if (bulletPrefab == null || bulletSpawnPoint == null)
        {
            Debug.LogWarning("WeaponController: Bullet Prefab àáî Bullet Spawn Point íå ïðèçíà÷åíî.", this);
            return;
        }

        Vector3 shootDirection;
        if (useRaycastAiming && mainCamera != null)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out hit, maxAimDistance))
            {
                shootDirection = (hit.point - bulletSpawnPoint.position).normalized;
            }
            else
            {
                shootDirection = ray.direction;
            }
        }
        else
        {
            shootDirection = bulletSpawnPoint.forward;
        }

        // Çàñòîñîâóºìî ðîçêèä, âðàõîâóþ÷è ìíîæíèê äëÿ ADS
        float currentSpread = bulletSpread;
        if (isAiming)
        {
            currentSpread *= aimSpreadMultiplier;
        }

        shootDirection += new Vector3(
            Random.Range(-currentSpread, currentSpread),
            Random.Range(-currentSpread, currentSpread),
            Random.Range(-currentSpread, currentSpread)
        );
        shootDirection.Normalize();

        Quaternion bulletRotation = Quaternion.LookRotation(shootDirection);

        // Îòðèìóºìî êóëþ ç ïóëó çàì³ñòü ñòâîðåííÿ íîâî¿
        GameObject bullet = null;
        if (BulletPool.Instance != null)
        {
            bullet = BulletPool.Instance.GetBullet(bulletSpawnPoint.position, bulletRotation);
        }
        else
        {
            // Fallback: ñòâîðþºìî êóëþ ÿê ðàí³øå, ÿêùî ïóë íåäîñòóïíèé
            Debug.LogWarning("WeaponController: BulletPool íåäîñòóïíèé, âèêîðèñòîâóºòüñÿ Instantiate.");
            bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);
        }

        if (bullet == null)
        {
            Debug.LogWarning("WeaponController: Íå âäàëîñÿ îòðèìàòè êóëþ (ïóë âè÷åðïàíî?)");
            return;
        } 
        
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.AddForce(shootDirection * bulletForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("WeaponController: Íà ïðåôàá³ êóë³ íåìàº Rigidbody. Êóëÿ íå áóäå ðóõàòèñÿ ç ³ìïóëüñîì.", bullet);
        }

        ApplyRecoil(); // Çàñòîñîâóºìî â³ääà÷ó ï³ñëÿ ïîñòð³ëó

        // Ïåðåâ³ðÿºìî âëó÷àííÿ äëÿ damage numbers (Claude ïîêðàùåííÿ)
        CheckForHitAndShowDamage(shootDirection);

        // Â³äïðàâëÿºìî ïîä³þ ïîñòð³ëó
        Events.Trigger(new WeaponFiredEvent(weaponDisplayName, bulletSpawnPoint.position, shootDirection, currentAmmo, bullet));

        // Â³äïðàâëÿºìî ïîä³þ çì³íè ïàòðîí³â
        Events.Trigger(new AmmoChangedEvent(weaponDisplayName, currentAmmo, magazineSize, reloadCharges));
    }

    /// <summary>
    /// Ìåòîä äëÿ ïî÷àòêó ïåðåçàðÿäêè.
    /// </summary>
    void Reload()
    {
        // Ïåðåâ³ðÿºìî, ÷è íå ïîâíèé ìàãàçèí, ÷è íå ïåðåçàðÿäæàºìîñÿ, ³ ÷è º ïåðåçàðÿäêè
        if (currentAmmo == magazineSize || isReloading || reloadCharges <= 0)
        {
            if (reloadCharges <= 0) Debug.Log($"Íåìîæëèâî ïåðåçàðÿäèòè {weaponDisplayName}: íåìàº ïåðåçàðÿäîê.");
            return;
        }

        isReloading = true;
        reloadCharges--; // Âèòðà÷àºìî îäíó ïåðåçàðÿäêó
        if (isAiming) ResetADS(); // Âèõîäèìî ç ðåæèìó ïðèö³ëþâàííÿ ïðè ïåðåçàðÿäö³
        // Ñêèäàºìî â³ääà÷ó ïðè ïåðåçàðÿäö³
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        
        Debug.Log($"Reloading {weaponDisplayName}... Reload charges left: {reloadCharges}");

        // Â³äïðàâëÿºìî ïîä³þ ïî÷àòêó ïåðåçàðÿäêè
        Events.Trigger(new WeaponReloadStartedEvent(weaponDisplayName, reloadTime, reloadCharges));

        if (audioSource != null && reloadSoundClip != null)
        {
            audioSource.PlayOneShot(reloadSoundClip);
        }

        // Áåçïå÷íî çàïóñêàºìî êîðóòèíó ïåðåçàðÿäêè
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
        }
        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    /// <summary>
    /// Êîðóòèíà, ÿêà âèêîíóº çàòðèìêó ïåðåçàðÿäêè.
    /// </summary>
    IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        reloadCoroutine = null; // Î÷èùóºìî ïîñèëàííÿ ï³ñëÿ çàâåðøåííÿ
        Debug.Log($"{weaponDisplayName} reloaded! Current ammo: {currentAmmo}");

        // Â³äïðàâëÿºìî ïîä³¿ çàâåðøåííÿ ïåðåçàðÿäêè
        Events.Trigger(new WeaponReloadCompletedEvent(weaponDisplayName, currentAmmo));
        Events.Trigger(new AmmoChangedEvent(weaponDisplayName, currentAmmo, magazineSize, reloadCharges));
    }

    /// <summary>
    /// Âàë³äàö³ÿ ïàðàìåòð³â â Unity Editor
    /// </summary>
    void OnValidate()
    {
        // Âàë³äóºìî çáðîþ
        fireRate = Mathf.Max(0.1f, fireRate);
        bulletForce = Mathf.Max(1f, bulletForce);
        bulletSpread = Mathf.Max(0f, bulletSpread);
        maxAimDistance = Mathf.Max(1f, maxAimDistance);

        // Âàë³äóºìî ìàãàçèí òà ïåðåçàðÿäêó
        magazineSize = ValidationHelper.ValidateMagazineSize(magazineSize);
        reloadTime = Mathf.Max(0.1f, reloadTime);
        reloadCharges = Mathf.Max(0, reloadCharges);

        // Âàë³äóºìî ADS
        aimSpeed = Mathf.Max(0.1f, aimSpeed);
        aimFOV = Mathf.Clamp(aimFOV, 10f, 120f);
        aimSpreadMultiplier = Mathf.Max(0f, aimSpreadMultiplier);

        // Âàë³äóºìî â³ääà÷ó
        recoilAmount = Mathf.Max(0f, recoilAmount);
        recoilSpeed = Mathf.Max(0.1f, recoilSpeed);
        recoilReturnSpeed = Mathf.Max(0.1f, recoilReturnSpeed);

        // Çàì³íÿºìî ìàã³÷í³ ÷èñëà êîíñòàíòàìè
        if (maxAimDistance == 500f)
            maxAimDistance = GameConstants.DEFAULT_MAX_AIM_DISTANCE;
    }

    // ================================
    // ÌÅÒÎÄÈ ÄËß COMMAND PATTERN
    // ================================

    /// <summary>
    /// Ïåðåâ³ðÿº, ÷è ìîæíà ñòð³ëÿòè (äëÿ Command Pattern)
    /// </summary>
    public bool CanFire()
    {
        return Time.time >= nextFireTime && currentAmmo > 0 && !isReloading;
    }

    /// <summary>
    /// Âèêîíóº ïîñòð³ë (äëÿ Command Pattern)
    /// </summary>
    public void Fire()
    {
        if (CanFire())
        {
            Shoot();
        }
    }

    /// <summary>
    /// Ïåðåâ³ðÿº, ÷è ìîæíà ïåðåçàðÿäæàòè (äëÿ Command Pattern)
    /// </summary>
    public bool CanReload()
    {
        return currentAmmo < magazineSize && !isReloading && reloadCharges > 0;
    }

    /// <summary>
    /// Ïî÷èíàº ïåðåçàðÿäêó (äëÿ Command Pattern)
    /// </summary>
    public void StartReload()
    {
        if (CanReload())
        {
            Reload();
        }
    }

    /// <summary>
    /// Ïî÷èíàº ïðèö³ëþâàííÿ (äëÿ Command Pattern)
    /// </summary>
    public void StartAiming()
    {
        if (!isAiming && allowADS)
        {
            isAiming = true;
            Debug.Log($"{weaponDisplayName}: Started aiming");
        }
    }

    /// <summary>
    /// Çóïèíÿº ïðèö³ëþâàííÿ (äëÿ Command Pattern)
    /// </summary>
    public void StopAiming()
    {
        if (isAiming)
        {
            isAiming = false;
            Debug.Log($"{weaponDisplayName}: Stopped aiming");
        }
    }

    /// <summary>
    /// Ïåðåìèêàº ðåæèì ïðèö³ëþâàííÿ (äëÿ Command Pattern)
    /// </summary>
    public void ToggleAiming()
    {
        if (isAiming)
        {
            StopAiming();
        }
        else
        {
            StartAiming();
        }
    }

    /// <summary>
    /// Çàñòîñîâóº â³ääà÷ó äî îãëÿäó ãðàâöÿ.
    /// </summary>
    void ApplyRecoil()
    {
        if (mouseLook == null) return;

        float actualRecoilX = recoilX;
        float actualRecoilY = recoilY;
        float actualRecoilZ = recoilZ;

        if (isAiming)
        {
            actualRecoilX *= aimRecoilMultiplier;
            actualRecoilY *= aimRecoilMultiplier;
            actualRecoilZ *= aimRecoilMultiplier;
        }

        targetRecoilRotation += new Vector3(-actualRecoilX, Random.Range(-actualRecoilY, actualRecoilY), Random.Range(-actualRecoilZ, actualRecoilZ));
    }

    /// <summary>
    /// Îíîâëþº ñòàí â³ääà÷³, ïëàâíî ïåðåì³ùóþ÷è êàìåðó äî ö³ëüîâîãî îáåðòàííÿ.
    /// </summary>
    void UpdateRecoil()
    {
        if (mouseLook == null) return;

        // Çáåð³ãàºìî ïîïåðåäíþ â³ääà÷ó äëÿ îá÷èñëåííÿ ð³çíèö³
        Vector3 previousRecoil = currentRecoilRotation;
        
        currentRecoilRotation = Vector3.Lerp(currentRecoilRotation, targetRecoilRotation, Time.deltaTime * recoilSnappiness);
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
        
        // SCOUT ÂÈÏÐÀÂËÅÍÍß: Çàñòîñîâóºìî ð³çíèöþ â³ääà÷³ äî MouseLook
        Vector3 recoilDelta = currentRecoilRotation - previousRecoil;
        if (recoilDelta.magnitude > 0.001f) // Óíèêàºìî ì³êðîñêîï³÷íèõ çì³í
        {
            mouseLook.ApplyRecoil(recoilDelta.x, recoilDelta.y);
        }
    }

    /// <summary>
    /// Ãåíåðóº â³ääà÷ó ïðè ïîñòð³ë³ (SCOUT ÂÈÏÐÀÂËÅÍÍß)
    /// </summary>
    void ApplyRecoil()
    {
        // Îá÷èñëþºìî ñèëó â³ääà÷³ ç óðàõóâàííÿì ïðèö³ëþâàííÿ
        float recoilMultiplier = isAiming ? aimRecoilMultiplier : 1f;
        
        // Äîäàºìî âèïàäêîâ³ñòü äî â³ääà÷³
        float randomRecoilX = recoilX * recoilMultiplier * Random.Range(0.8f, 1.2f);
        float randomRecoilY = recoilY * recoilMultiplier * Random.Range(-1f, 1f);
        
        // Äîäàºìî â³ääà÷ó äî ö³ëüîâîãî çíà÷åííÿ
        targetRecoilRotation += new Vector3(randomRecoilX, randomRecoilY, 0f);
        
        Debug.Log($"WeaponController: Â³ääà÷à çàñòîñîâàíà - X:{randomRecoilX:F2}, Y:{randomRecoilY:F2}");
    }

    /// <summary>
    /// Ïåðåâ³ðÿº âëó÷àííÿ òà ïîêàçóº damage numbers (Claude ïîêðàùåííÿ)
    /// </summary>
    void CheckForHitAndShowDamage(Vector3 shootDirection)
    {
        if (mainCamera == null) return;
        
        // Âèêîíóºìî raycast äëÿ âèÿâëåííÿ âëó÷àíü
        Ray ray = new Ray(bulletSpawnPoint.position, shootDirection);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxAimDistance))
        {
            // Ïåðåâ³ðÿºìî, ÷è âëó÷èëè ó âîðîãà
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Îá÷èñëþºìî óðîí
                float baseDamage = GetWeaponDamage();
                bool isHeadshot = IsHeadshot(hit);
                float finalDamage = isHeadshot ? baseDamage * 2f : baseDamage;
                
                // Ïîêàçóºìî damage number
                if (DamageNumbersManager.Instance != null)
                {
                    Vector3 damagePosition = hit.point + Vector3.up * 0.5f;
                    DamageType damageType = enemy.HasArmor() ? DamageType.Armor : DamageType.Normal;
                    DamageNumbersManager.Instance.ShowDamageNumber(damagePosition, finalDamage, isHeadshot, damageType);
                }
                
                // Çàñòîñîâóºìî óðîí äî âîðîãà
                enemy.TakeDamage(finalDamage, hit.point, hit.normal);
                
                Debug.Log($"WeaponController: Âëó÷àííÿ! Óðîí: {finalDamage}, Headshot: {isHeadshot}");
            }
            else
            {
                // Âëó÷èëè â îòî÷åííÿ - ìîæíà äîäàòè åôåêòè ïîñòð³ëó
                CreateHitEffect(hit.point, hit.normal);
            }
        }
    }
    
    /// <summary>
    /// Âèçíà÷àº, ÷è º âëó÷àííÿ õåäøîòîì
    /// </summary>
    bool IsHeadshot(RaycastHit hit)
    {
        // Ïåðåâ³ðÿºìî òåã àáî íàçâó êîëàéäåðà
        return hit.collider.CompareTag("Head") || hit.collider.name.ToLower().Contains("head");
    }
    
    /// <summary>
    /// Ïîâåðòàº óðîí çáðî¿ (ìîæíà ðîçøèðèòè äëÿ ð³çíèõ òèï³â çáðî¿)
    /// </summary>
    float GetWeaponDamage()
    {
        // Áàçîâèé óðîí çàëåæíî â³ä òèïó çáðî¿
        // Ìîæíà ðîçøèðèòè ÷åðåç ScriptableObject êîíô³ãóðàö³¿
        return 25f; // Áàçîâèé óðîí
    }
    
    /// <summary>
    /// Ñòâîðþº åôåêò âëó÷àííÿ â îòî÷åííÿ
    /// </summary>
    void CreateHitEffect(Vector3 position, Vector3 normal)
    {
        // Òóò ìîæíà äîäàòè åôåêòè ³ñêîð, ïèëó òîùî
        // Ïîêè ùî ïðîñòî ëîãóºìî
        Debug.Log($"WeaponController: Âëó÷àííÿ â îòî÷åííÿ íà ïîçèö³¿ {position}");
    }
    
    /// <summary>
    /// Ïîâåðòàº çáðîþ òà FOV êàìåðè äî ñòàíäàðòíîãî ñòàíó.
    /// </summary>
    void ResetADS()
    {
        isAiming = false;
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
    }


    // --- Ïóáë³÷í³ ìåòîäè äëÿ UI àáî ³íøèõ ñêðèïò³â ---
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMagazineSize()
    {
        return magazineSize;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public Vector3 GetRecoilRotation()
    {
        return currentRecoilRotation;
    }

    // ÍÎÂ² ÏÓÁË²×Í² ÌÅÒÎÄÈ ÄËß Ï²ÄÁÎÐÓ/ÂÈÊÈÄÀÍÍß
    public bool IsDroppable()
    {
        return isDroppable;
    }

    public GameObject GetWeaponWorldPrefab()
    {
        return weaponWorldPrefab;
    }

    public string GetWeaponDisplayName()
    {
        return weaponDisplayName;
    }

    public Sprite GetWeaponIcon()
    {
        return weaponIcon;
    }

    // ÍÎÂ² ÏÓÁË²×Í² ÌÅÒÎÄÈ ÄËß ÑÈÑÒÅÌÈ ÏÅÐÅÇÀÐßÄÊÈ
    public int GetReloadCharges()
    {
        return reloadCharges;
    }

    public void AddReloadCharge()
    {
        reloadCharges++;
        Debug.Log($"Reload charge added to '{weaponDisplayName}'. Total charges: {reloadCharges}");
    }


    // Ìåòîäè, ùî âèêëèêàþòüñÿ WeaponSwitching ïðè åê³ï³ðîâö³/çíÿòò³
    public void OnEquip()
    {
        // Ñêèäàºìî âñ³ ñòàíè ïðè åê³ï³ðîâö³
        // currentAmmo = magazineSize; // Çàì³ñòü öüîãî, ïàòðîíè áóäóòü çáåð³ãàòèñÿ ì³æ åê³ï³ðîâêàìè.
                                     // ßêùî âè õî÷åòå ïîâíèé ìàãàçèí ïðè êîæí³é åê³ï³ðîâö³, ðîçêîìåíòóéòå.
        isReloading = false;
        ResetADS(); // Âèõîäèìî ç ADS
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        StopAllCoroutines(); // Çóïèíÿºìî áóäü-ÿê³ êîðóòèíè (íàïðèêëàä, ïåðåçàðÿäêó)
        
        Debug.Log($"{weaponDisplayName} Equipped.");
    }

    public void OnUnequip()
    {
        ResetADS(); // Âèõîäèìî ç ADS
        targetRecoilRotation = Vector3.zero;
        currentRecoilRotation = Vector3.zero;
        StopAllCoroutines(); // Çóïèíÿºìî áóäü-ÿê³ ïîòî÷í³ êîðóòèíè (íàïðèêëàä, ïåðåçàðÿäêó)
        
        Debug.Log($"{weaponDisplayName} Unequipped.");
    }
}