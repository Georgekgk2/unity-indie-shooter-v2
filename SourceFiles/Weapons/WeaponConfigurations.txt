using UnityEngine;

/// <summary>
/// Конфігурація зброї. Містить всі параметри для налаштування поведінки зброї.
/// </summary>
[Configuration("Game/Weapon Configuration", "Weapons")]
[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Weapons/Weapon Configuration")]
public class WeaponConfiguration : BaseConfiguration
{
    [Header("Basic Weapon Info")]
    [Tooltip("Тип зброї")]
    public WeaponType weaponType = WeaponType.AssaultRifle;
    [Tooltip("Іконка зброї для UI")]
    public Sprite weaponIcon;
    [Tooltip("Префаб зброї")]
    public GameObject weaponPrefab;
    [Tooltip("Префаб кулі")]
    public GameObject bulletPrefab;

    [Header("Combat Stats")]
    [Tooltip("Урон за постріл")]
    [Range(1f, 200f)]
    public float damage = 25f;
    [Tooltip("Швидкість стрільби (пострілів в секунду)")]
    [Range(0.1f, 30f)]
    public float fireRate = 8f;
    [Tooltip("Сила кулі")]
    [Range(100f, 2000f)]
    public float bulletForce = 1000f;
    [Tooltip("Розкид кулі")]
    [Range(0f, 0.2f)]
    public float bulletSpread = 0.02f;
    [Tooltip("Максимальна дистанція прицілювання")]
    [Range(10f, 1000f)]
    public float maxAimDistance = 500f;

    [Header("Magazine and Reload")]
    [Tooltip("Розмір магазину")]
    [Range(1, 200)]
    public int magazineSize = 30;
    [Tooltip("Час перезарядки (секунди)")]
    [Range(0.5f, 10f)]
    public float reloadTime = 2.5f;
    [Tooltip("Кількість зарядів для перезарядки")]
    [Range(0, 20)]
    public int reloadCharges = 5;
    [Tooltip("Початкова кількість патронів")]
    [Range(0, 500)]
    public int startingAmmo = 90;

    [Header("Aiming Down Sights")]
    [Tooltip("Чи дозволено прицілювання?")]
    public bool allowADS = true;
    [Tooltip("Швидкість прицілювання")]
    [Range(1f, 20f)]
    public float aimSpeed = 8f;
    [Tooltip("FOV при прицілюванні")]
    [Range(20f, 80f)]
    public float aimFOV = 40f;
    [Tooltip("Множник розкиду при прицілюванні")]
    [Range(0f, 1f)]
    public float aimSpreadMultiplier = 0.3f;

    [Header("Recoil Settings")]
    [Tooltip("Сила віддачі")]
    [Range(0f, 10f)]
    public float recoilAmount = 2f;
    [Tooltip("Швидкість віддачі")]
    [Range(1f, 20f)]
    public float recoilSpeed = 10f;
    [Tooltip("Швидкість повернення після віддачі")]
    [Range(1f, 20f)]
    public float recoilReturnSpeed = 5f;
    [Tooltip("Випадковість віддачі")]
    [Range(0f, 5f)]
    public float recoilRandomness = 1f;

    [Header("Audio")]
    [Tooltip("Звук пострілу")]
    public AudioClip shootSound;
    [Tooltip("Звук перезарядки")]
    public AudioClip reloadSound;
    [Tooltip("Звук порожнього магазину")]
    public AudioClip emptySound;
    [Tooltip("Гучність звуків")]
    [Range(0f, 1f)]
    public float audioVolume = 1f;

    [Header("Visual Effects")]
    [Tooltip("Ефект спалаху")]
    public GameObject muzzleFlashEffect;
    [Tooltip("Ефект удару кулі")]
    public GameObject bulletHitEffect;
    [Tooltip("Ефект гільз")]
    public GameObject shellEjectEffect;

    [Header("Special Features")]
    [Tooltip("Чи автоматична зброя?")]
    public bool isAutomatic = true;
    [Tooltip("Чи можна стріляти під час бігу?")]
    public bool canFireWhileRunning = true;
    [Tooltip("Чи можна стріляти під час стрибка?")]
    public bool canFireWhileJumping = false;
    [Tooltip("Множник урону в голову")]
    [Range(1f, 10f)]
    public float headshotMultiplier = 2f;

    public enum WeaponType
    {
        Pistol,
        AssaultRifle,
        Shotgun,
        SniperRifle,
        SMG,
        LMG,
        Grenade,
        Melee
    }

    protected override void ValidateConfiguration()
    {
        // Валідація базових параметрів
        damage = Mathf.Max(1f, damage);
        fireRate = Mathf.Max(0.1f, fireRate);
        bulletForce = Mathf.Max(100f, bulletForce);
        bulletSpread = Mathf.Max(0f, bulletSpread);
        
        // Валідація магазину
        magazineSize = Mathf.Max(1, magazineSize);
        reloadTime = Mathf.Max(0.1f, reloadTime);
        reloadCharges = Mathf.Max(0, reloadCharges);
        
        // Валідація прицілювання
        aimSpeed = Mathf.Max(1f, aimSpeed);
        aimFOV = Mathf.Clamp(aimFOV, 10f, 120f);
        aimSpreadMultiplier = Mathf.Clamp01(aimSpreadMultiplier);
        
        // Валідація віддачі
        recoilAmount = Mathf.Max(0f, recoilAmount);
        recoilSpeed = Mathf.Max(1f, recoilSpeed);
        recoilReturnSpeed = Mathf.Max(1f, recoilReturnSpeed);
        
        // Валідація аудіо
        audioVolume = Mathf.Clamp01(audioVolume);
        
        // Валідація спеціальних функцій
        headshotMultiplier = Mathf.Max(1f, headshotMultiplier);

        // Автоматичне присвоєння displayName, якщо порожнє
        if (string.IsNullOrEmpty(displayName) && weaponPrefab != null)
        {
            displayName = weaponPrefab.name;
        }
    }

    /// <summary>
    /// Розраховує DPS (урон в секунду)
    /// </summary>
    public float GetDPS()
    {
        return damage * fireRate;
    }

    /// <summary>
    /// Розраховує час спустошення магазину
    /// </summary>
    public float GetMagazineEmptyTime()
    {
        return magazineSize / fireRate;
    }

    /// <summary>
    /// Розраховує загальний час бою (стрільба + перезарядка)
    /// </summary>
    public float GetTotalCombatTime()
    {
        return GetMagazineEmptyTime() + reloadTime;
    }

    /// <summary>
    /// Розраховує ефективний DPS з урахуванням перезарядки
    /// </summary>
    public float GetEffectiveDPS()
    {
        float totalDamage = damage * magazineSize;
        float totalTime = GetTotalCombatTime();
        return totalDamage / totalTime;
    }

    /// <summary>
    /// Перевіряє, чи підходить зброя для певної дистанції
    /// </summary>
    public bool IsEffectiveAtRange(float range)
    {
        switch (weaponType)
        {
            case WeaponType.Pistol:
                return range <= 25f;
            case WeaponType.SMG:
                return range <= 30f;
            case WeaponType.AssaultRifle:
                return range <= 100f;
            case WeaponType.Shotgun:
                return range <= 15f;
            case WeaponType.SniperRifle:
                return range >= 50f;
            case WeaponType.LMG:
                return range >= 30f && range <= 150f;
            default:
                return true;
        }
    }
}

/// <summary>
/// Конфігурація боєприпасів
/// </summary>
[Configuration("Game/Ammo Configuration", "Weapons")]
[CreateAssetMenu(fileName = "AmmoConfig", menuName = "Game/Weapons/Ammo Configuration")]
public class AmmoConfiguration : BaseConfiguration
{
    [Header("Ammo Info")]
    [Tooltip("Тип боєприпасів")]
    public AmmoType ammoType = AmmoType.Standard;
    [Tooltip("Іконка боєприпасів")]
    public Sprite ammoIcon;
    [Tooltip("Префаб кулі")]
    public GameObject bulletPrefab;

    [Header("Ballistics")]
    [Tooltip("Швидкість кулі")]
    [Range(100f, 2000f)]
    public float bulletSpeed = 1000f;
    [Tooltip("Час життя кулі")]
    [Range(1f, 10f)]
    public float bulletLifetime = 3f;
    [Tooltip("Маса кулі")]
    [Range(0.001f, 0.1f)]
    public float bulletMass = 0.01f;
    [Tooltip("Опір повітря")]
    [Range(0f, 2f)]
    public float airResistance = 0.1f;

    [Header("Damage Properties")]
    [Tooltip("Базовий урон")]
    [Range(1f, 500f)]
    public float baseDamage = 25f;
    [Tooltip("Множник урону на дистанції")]
    public AnimationCurve damageDropoffCurve = AnimationCurve.Linear(0f, 1f, 100f, 0.5f);
    [Tooltip("Пробивна здатність")]
    [Range(0, 10)]
    public int penetrationPower = 1;
    [Tooltip("Множник урону по броні")]
    [Range(0.1f, 2f)]
    public float armorDamageMultiplier = 1f;

    [Header("Special Effects")]
    [Tooltip("Тип спеціального ефекту")]
    public SpecialEffect specialEffect = SpecialEffect.None;
    [Tooltip("Сила спеціального ефекту")]
    [Range(0f, 10f)]
    public float effectStrength = 1f;
    [Tooltip("Тривалість ефекту")]
    [Range(0f, 30f)]
    public float effectDuration = 0f;

    [Header("Visual and Audio")]
    [Tooltip("Ефект удару")]
    public GameObject hitEffect;
    [Tooltip("Звук удару")]
    public AudioClip hitSound;
    [Tooltip("Колір трасера")]
    public Color tracerColor = Color.yellow;
    [Tooltip("Чи показувати трасер?")]
    public bool showTracer = false;

    public enum AmmoType
    {
        Standard,
        ArmorPiercing,
        Explosive,
        Incendiary,
        Tracer,
        Rubber,
        Tranquilizer
    }

    public enum SpecialEffect
    {
        None,
        Fire,
        Poison,
        Electric,
        Freeze,
        Explosive,
        Healing
    }

    protected override void ValidateConfiguration()
    {
        bulletSpeed = Mathf.Max(100f, bulletSpeed);
        bulletLifetime = Mathf.Max(0.1f, bulletLifetime);
        bulletMass = Mathf.Max(0.001f, bulletMass);
        airResistance = Mathf.Max(0f, airResistance);
        baseDamage = Mathf.Max(1f, baseDamage);
        penetrationPower = Mathf.Max(0, penetrationPower);
        armorDamageMultiplier = Mathf.Max(0.1f, armorDamageMultiplier);
        effectStrength = Mathf.Max(0f, effectStrength);
        effectDuration = Mathf.Max(0f, effectDuration);

        // Автоматичне присвоєння displayName
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = $"{ammoType} Ammo";
        }
    }

    /// <summary>
    /// Розраховує урон на певній дистанції
    /// </summary>
    public float GetDamageAtDistance(float distance)
    {
        float multiplier = damageDropoffCurve.Evaluate(distance);
        return baseDamage * multiplier;
    }

    /// <summary>
    /// Розраховує швидкість кулі з урахуванням опору повітря
    /// </summary>
    public float GetBulletSpeedAtDistance(float distance)
    {
        return bulletSpeed * Mathf.Exp(-airResistance * distance / 100f);
    }

    /// <summary>
    /// Перевіряє, чи може куля пробити певну кількість перешкод
    /// </summary>
    public bool CanPenetrate(int obstacles)
    {
        return penetrationPower >= obstacles;
    }
}

/// <summary>
/// Набір зброї - колекція зброї для певного рівня або ситуації
/// </summary>
[Configuration("Game/Weapon Set", "Weapons")]
[CreateAssetMenu(fileName = "WeaponSet", menuName = "Game/Weapons/Weapon Set")]
public class WeaponSet : BaseConfiguration
{
    [Header("Weapon Set Info")]
    [Tooltip("Тип набору зброї")]
    public SetType setType = SetType.Balanced;
    [Tooltip("Рекомендований рівень гравця")]
    [Range(1, 100)]
    public int recommendedLevel = 1;

    [Header("Weapons in Set")]
    [Tooltip("Основна зброя")]
    public WeaponConfiguration primaryWeapon;
    [Tooltip("Додаткова зброя")]
    public WeaponConfiguration secondaryWeapon;
    [Tooltip("Зброя ближнього бою")]
    public WeaponConfiguration meleeWeapon;
    [Tooltip("Гранати або спеціальна зброя")]
    public WeaponConfiguration specialWeapon;

    [Header("Ammo Configuration")]
    [Tooltip("Початкова кількість патронів для основної зброї")]
    public int primaryAmmo = 120;
    [Tooltip("Початкова кількість патронів для додаткової зброї")]
    public int secondaryAmmo = 60;
    [Tooltip("Кількість спеціальних боєприпасів")]
    public int specialAmmo = 3;

    [Header("Set Bonuses")]
    [Tooltip("Бонус до урону набору")]
    [Range(0f, 2f)]
    public float damageBonus = 1f;
    [Tooltip("Бонус до швидкості перезарядки")]
    [Range(0.5f, 2f)]
    public float reloadSpeedBonus = 1f;
    [Tooltip("Бонус до точності")]
    [Range(0.5f, 2f)]
    public float accuracyBonus = 1f;

    public enum SetType
    {
        Assault,
        Sniper,
        CQB,
        Support,
        Stealth,
        Balanced,
        Heavy
    }

    protected override void ValidateConfiguration()
    {
        recommendedLevel = Mathf.Clamp(recommendedLevel, 1, 100);
        primaryAmmo = Mathf.Max(0, primaryAmmo);
        secondaryAmmo = Mathf.Max(0, secondaryAmmo);
        specialAmmo = Mathf.Max(0, specialAmmo);
        damageBonus = Mathf.Max(0.1f, damageBonus);
        reloadSpeedBonus = Mathf.Max(0.1f, reloadSpeedBonus);
        accuracyBonus = Mathf.Max(0.1f, accuracyBonus);

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = $"{setType} Weapon Set";
        }
    }

    /// <summary>
    /// Отримує всі зброї в наборі
    /// </summary>
    public WeaponConfiguration[] GetAllWeapons()
    {
        var weapons = new List<WeaponConfiguration>();
        if (primaryWeapon != null) weapons.Add(primaryWeapon);
        if (secondaryWeapon != null) weapons.Add(secondaryWeapon);
        if (meleeWeapon != null) weapons.Add(meleeWeapon);
        if (specialWeapon != null) weapons.Add(specialWeapon);
        return weapons.ToArray();
    }

    /// <summary>
    /// Розраховує загальний DPS набору
    /// </summary>
    public float GetTotalDPS()
    {
        float totalDPS = 0f;
        var weapons = GetAllWeapons();
        foreach (var weapon in weapons)
        {
            totalDPS += weapon.GetEffectiveDPS() * damageBonus;
        }
        return totalDPS;
    }

    /// <summary>
    /// Перевіряє, чи підходить набір для певного стилю гри
    /// </summary>
    public bool IsSuitableForPlayStyle(string playStyle)
    {
        return setType.ToString().ToLower().Contains(playStyle.ToLower());
    }
}