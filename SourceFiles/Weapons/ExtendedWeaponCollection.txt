using UnityEngine;

/// <summary>
/// Розширена колекція зброї з різноманітними типами та характеристиками.
/// Включає 20+ варіантів зброї для різних стилів гри.
/// </summary>

// ================================
// ПІСТОЛЕТИ
// ================================

[CreateAssetMenu(fileName = "Glock17_Config", menuName = "Game/Weapons/Pistols/Glock 17")]
public class Glock17Configuration : WeaponConfiguration
{
    void Reset()
    {
        // Базові параметри
        weaponType = WeaponType.Pistol;
        displayName = "Glock 17";
        description = "Надійний службовий пістолет з високою точністю";
        
        // Бойові характеристики
        damage = 35f;
        fireRate = 6f;
        bulletForce = 800f;
        bulletSpread = 0.01f;
        maxAimDistance = 50f;
        
        // Магазин та перезарядка
        magazineSize = 17;
        reloadTime = 1.8f;
        reloadCharges = 8;
        startingAmmo = 68;
        
        // Прицілювання
        allowADS = true;
        aimSpeed = 12f;
        aimFOV = 50f;
        aimSpreadMultiplier = 0.2f;
        
        // Віддача
        recoilAmount = 1.5f;
        recoilSpeed = 15f;
        recoilReturnSpeed = 8f;
        recoilRandomness = 0.8f;
        
        // Спеціальні функції
        isAutomatic = false;
        canFireWhileRunning = true;
        canFireWhileJumping = true;
        headshotMultiplier = 2.5f;
    }
}

[CreateAssetMenu(fileName = "DesertEagle_Config", menuName = "Game/Weapons/Pistols/Desert Eagle")]
public class DesertEagleConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.Pistol;
        displayName = "Desert Eagle";
        description = "Потужний пістолет з високим уроном та сильною віддачею";
        
        damage = 75f;
        fireRate = 3f;
        bulletForce = 1200f;
        bulletSpread = 0.02f;
        maxAimDistance = 75f;
        
        magazineSize = 7;
        reloadTime = 2.2f;
        reloadCharges = 6;
        startingAmmo = 35;
        
        allowADS = true;
        aimSpeed = 8f;
        aimFOV = 45f;
        aimSpreadMultiplier = 0.15f;
        
        recoilAmount = 3.5f;
        recoilSpeed = 20f;
        recoilReturnSpeed = 6f;
        recoilRandomness = 1.5f;
        
        isAutomatic = false;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 3f;
    }
}

// ================================
// ШТУРМОВІ ГВИНТІВКИ
// ================================

[CreateAssetMenu(fileName = "AK47_Config", menuName = "Game/Weapons/Assault Rifles/AK-47")]
public class AK47Configuration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.AssaultRifle;
        displayName = "AK-47";
        description = "Легендарна штурмова гвинтівка з високим уроном та помірною віддачею";
        
        damage = 42f;
        fireRate = 10f;
        bulletForce = 1100f;
        bulletSpread = 0.025f;
        maxAimDistance = 200f;
        
        magazineSize = 30;
        reloadTime = 2.8f;
        reloadCharges = 6;
        startingAmmo = 120;
        
        allowADS = true;
        aimSpeed = 6f;
        aimFOV = 35f;
        aimSpreadMultiplier = 0.4f;
        
        recoilAmount = 2.8f;
        recoilSpeed = 12f;
        recoilReturnSpeed = 5f;
        recoilRandomness = 1.2f;
        
        isAutomatic = true;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 2.2f;
    }
}

[CreateAssetMenu(fileName = "M4A1_Config", menuName = "Game/Weapons/Assault Rifles/M4A1")]
public class M4A1Configuration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.AssaultRifle;
        displayName = "M4A1";
        description = "Точна штурмова гвинтівка з низькою віддачею та високою швидкістю стрільби";
        
        damage = 38f;
        fireRate = 12f;
        bulletForce = 1000f;
        bulletSpread = 0.018f;
        maxAimDistance = 250f;
        
        magazineSize = 30;
        reloadTime = 2.5f;
        reloadCharges = 7;
        startingAmmo = 150;
        
        allowADS = true;
        aimSpeed = 8f;
        aimFOV = 40f;
        aimSpreadMultiplier = 0.3f;
        
        recoilAmount = 2.2f;
        recoilSpeed = 14f;
        recoilReturnSpeed = 7f;
        recoilRandomness = 0.8f;
        
        isAutomatic = true;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 2f;
    }
}

// ================================
// СНАЙПЕРСЬКІ ГВИНТІВКИ
// ================================

[CreateAssetMenu(fileName = "AWP_Config", menuName = "Game/Weapons/Sniper Rifles/AWP")]
public class AWPConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.SniperRifle;
        displayName = "AWP";
        description = "Потужна снайперська гвинтівка з можливістю вбивства одним пострілом";
        
        damage = 150f;
        fireRate = 1.2f;
        bulletForce = 2000f;
        bulletSpread = 0.005f;
        maxAimDistance = 800f;
        
        magazineSize = 5;
        reloadTime = 3.5f;
        reloadCharges = 4;
        startingAmmo = 25;
        
        allowADS = true;
        aimSpeed = 3f;
        aimFOV = 15f;
        aimSpreadMultiplier = 0.1f;
        
        recoilAmount = 4.5f;
        recoilSpeed = 8f;
        recoilReturnSpeed = 4f;
        recoilRandomness = 0.5f;
        
        isAutomatic = false;
        canFireWhileRunning = false;
        canFireWhileJumping = false;
        headshotMultiplier = 5f;
    }
}

[CreateAssetMenu(fileName = "Scout_Config", menuName = "Game/Weapons/Sniper Rifles/Scout")]
public class ScoutConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.SniperRifle;
        displayName = "Scout";
        description = "Легка снайперська гвинтівка з високою мобільністю";
        
        damage = 85f;
        fireRate = 2f;
        bulletForce = 1500f;
        bulletSpread = 0.008f;
        maxAimDistance = 600f;
        
        magazineSize = 8;
        reloadTime = 2.8f;
        reloadCharges = 5;
        startingAmmo = 40;
        
        allowADS = true;
        aimSpeed = 5f;
        aimFOV = 20f;
        aimSpreadMultiplier = 0.15f;
        
        recoilAmount = 3.2f;
        recoilSpeed = 10f;
        recoilReturnSpeed = 6f;
        recoilRandomness = 0.7f;
        
        isAutomatic = false;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 3.5f;
    }
}

// ================================
// ДРОБОВИКИ
// ================================

[CreateAssetMenu(fileName = "Shotgun_Config", menuName = "Game/Weapons/Shotguns/Pump Shotgun")]
public class PumpShotgunConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.Shotgun;
        displayName = "Pump Shotgun";
        description = "Потужний дробовик для ближнього бою";
        
        damage = 120f; // Урон за всі кульки
        fireRate = 1.5f;
        bulletForce = 800f;
        bulletSpread = 0.15f; // Великий розкид
        maxAimDistance = 25f;
        
        magazineSize = 6;
        reloadTime = 4f; // Довга перезарядка по одному патрону
        reloadCharges = 4;
        startingAmmo = 30;
        
        allowADS = true;
        aimSpeed = 4f;
        aimFOV = 45f;
        aimSpreadMultiplier = 0.6f;
        
        recoilAmount = 4f;
        recoilSpeed = 8f;
        recoilReturnSpeed = 5f;
        recoilRandomness = 1.8f;
        
        isAutomatic = false;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 1.5f; // Менший множник через розкид
    }
}

[CreateAssetMenu(fileName = "AutoShotgun_Config", menuName = "Game/Weapons/Shotguns/Auto Shotgun")]
public class AutoShotgunConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.Shotgun;
        displayName = "Auto Shotgun";
        description = "Автоматичний дробовик з високою швидкістю стрільби";
        
        damage = 80f;
        fireRate = 4f;
        bulletForce = 700f;
        bulletSpread = 0.12f;
        maxAimDistance = 30f;
        
        magazineSize = 8;
        reloadTime = 3.2f;
        reloadCharges = 5;
        startingAmmo = 40;
        
        allowADS = true;
        aimSpeed = 5f;
        aimFOV = 50f;
        aimSpreadMultiplier = 0.7f;
        
        recoilAmount = 3.5f;
        recoilSpeed = 10f;
        recoilReturnSpeed = 6f;
        recoilRandomness = 1.5f;
        
        isAutomatic = true;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 1.3f;
    }
}

// ================================
// ПІСТОЛЕТИ-КУЛЕМЕТИ
// ================================

[CreateAssetMenu(fileName = "MP5_Config", menuName = "Game/Weapons/SMGs/MP5")]
public class MP5Configuration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.SMG;
        displayName = "MP5";
        description = "Компактний пістолет-кулемет з високою точністю";
        
        damage = 28f;
        fireRate = 15f;
        bulletForce = 600f;
        bulletSpread = 0.022f;
        maxAimDistance = 80f;
        
        magazineSize = 25;
        reloadTime = 2.2f;
        reloadCharges = 8;
        startingAmmo = 125;
        
        allowADS = true;
        aimSpeed = 10f;
        aimFOV = 45f;
        aimSpreadMultiplier = 0.4f;
        
        recoilAmount = 1.8f;
        recoilSpeed = 16f;
        recoilReturnSpeed = 9f;
        recoilRandomness = 1f;
        
        isAutomatic = true;
        canFireWhileRunning = true;
        canFireWhileJumping = true;
        headshotMultiplier = 2f;
    }
}

[CreateAssetMenu(fileName = "UZI_Config", menuName = "Game/Weapons/SMGs/UZI")]
public class UZIConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.SMG;
        displayName = "UZI";
        description = "Швидкострільний пістолет-кулемет для ближнього бою";
        
        damage = 24f;
        fireRate = 18f;
        bulletForce = 500f;
        bulletSpread = 0.035f;
        maxAimDistance = 60f;
        
        magazineSize = 32;
        reloadTime = 2f;
        reloadCharges = 10;
        startingAmmo = 160;
        
        allowADS = true;
        aimSpeed = 12f;
        aimFOV = 50f;
        aimSpreadMultiplier = 0.5f;
        
        recoilAmount = 2.2f;
        recoilSpeed = 18f;
        recoilReturnSpeed = 10f;
        recoilRandomness = 1.3f;
        
        isAutomatic = true;
        canFireWhileRunning = true;
        canFireWhileJumping = true;
        headshotMultiplier = 1.8f;
    }
}

// ================================
// КУЛЕМЕТИ
// ================================

[CreateAssetMenu(fileName = "M249_Config", menuName = "Game/Weapons/LMGs/M249")]
public class M249Configuration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.LMG;
        displayName = "M249 SAW";
        description = "Важкий кулемет з великим магазином та високим уроном";
        
        damage = 45f;
        fireRate = 8f;
        bulletForce = 1200f;
        bulletSpread = 0.04f;
        maxAimDistance = 300f;
        
        magazineSize = 100;
        reloadTime = 5f;
        reloadCharges = 3;
        startingAmmo = 200;
        
        allowADS = true;
        aimSpeed = 2f;
        aimFOV = 30f;
        aimSpreadMultiplier = 0.6f;
        
        recoilAmount = 3.8f;
        recoilSpeed = 8f;
        recoilReturnSpeed = 3f;
        recoilRandomness = 2f;
        
        isAutomatic = true;
        canFireWhileRunning = false;
        canFireWhileJumping = false;
        headshotMultiplier = 2.5f;
    }
}

// ================================
// СПЕЦІАЛЬНА ЗБРОЯ
// ================================

[CreateAssetMenu(fileName = "GrenadeLauncher_Config", menuName = "Game/Weapons/Special/Grenade Launcher")]
public class GrenadeLauncherConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.Grenade;
        displayName = "Grenade Launcher";
        description = "Гранатомет з вибуховими снарядами";
        
        damage = 200f; // Урон від вибуху
        fireRate = 1f;
        bulletForce = 400f; // Повільні снаряди
        bulletSpread = 0.01f;
        maxAimDistance = 150f;
        
        magazineSize = 1;
        reloadTime = 3f;
        reloadCharges = 6;
        startingAmmo = 12;
        
        allowADS = true;
        aimSpeed = 3f;
        aimFOV = 40f;
        aimSpreadMultiplier = 0.5f;
        
        recoilAmount = 5f;
        recoilSpeed = 5f;
        recoilReturnSpeed = 3f;
        recoilRandomness = 1f;
        
        isAutomatic = false;
        canFireWhileRunning = false;
        canFireWhileJumping = false;
        headshotMultiplier = 1f; // Вибухівка не має headshot
    }
}

[CreateAssetMenu(fileName = "Crossbow_Config", menuName = "Game/Weapons/Special/Crossbow")]
public class CrossbowConfiguration : WeaponConfiguration
{
    void Reset()
    {
        weaponType = WeaponType.SniperRifle;
        displayName = "Crossbow";
        description = "Безшумний арбалет з високим уроном";
        
        damage = 100f;
        fireRate = 0.8f;
        bulletForce = 800f;
        bulletSpread = 0.005f;
        maxAimDistance = 200f;
        
        magazineSize = 1;
        reloadTime = 2.5f;
        reloadCharges = 15;
        startingAmmo = 30;
        
        allowADS = true;
        aimSpeed = 4f;
        aimFOV = 25f;
        aimSpreadMultiplier = 0.1f;
        
        recoilAmount = 1f; // Мінімальна віддача
        recoilSpeed = 5f;
        recoilReturnSpeed = 8f;
        recoilRandomness = 0.2f;
        
        isAutomatic = false;
        canFireWhileRunning = true;
        canFireWhileJumping = false;
        headshotMultiplier = 4f;
    }
}

// ================================
// НАБОРИ ЗБРОЇ
// ================================

[CreateAssetMenu(fileName = "AssaultSet_Config", menuName = "Game/Weapon Sets/Assault Set")]
public class AssaultWeaponSet : WeaponSet
{
    void Reset()
    {
        setType = SetType.Assault;
        displayName = "Assault Weapon Set";
        description = "Збалансований набір для штурмових операцій";
        recommendedLevel = 5;
        
        // Тут можна призначити конкретні конфігурації зброї
        primaryAmmo = 150;
        secondaryAmmo = 68;
        specialAmmo = 6;
        
        damageBonus = 1.1f;
        reloadSpeedBonus = 1.2f;
        accuracyBonus = 1.1f;
    }
}

[CreateAssetMenu(fileName = "SniperSet_Config", menuName = "Game/Weapon Sets/Sniper Set")]
public class SniperWeaponSet : WeaponSet
{
    void Reset()
    {
        setType = SetType.Sniper;
        displayName = "Sniper Weapon Set";
        description = "Набір для дальнього бою та точної стрільби";
        recommendedLevel = 8;
        
        primaryAmmo = 40;
        secondaryAmmo = 51;
        specialAmmo = 3;
        
        damageBonus = 1.3f;
        reloadSpeedBonus = 0.9f;
        accuracyBonus = 1.5f;
    }
}

[CreateAssetMenu(fileName = "CQBSet_Config", menuName = "Game/Weapon Sets/CQB Set")]
public class CQBWeaponSet : WeaponSet
{
    void Reset()
    {
        setType = SetType.CQB;
        displayName = "Close Quarters Battle Set";
        description = "Набір для ближнього бою в обмеженому просторі";
        recommendedLevel = 3;
        
        primaryAmmo = 48;
        secondaryAmmo = 85;
        specialAmmo = 8;
        
        damageBonus = 1.2f;
        reloadSpeedBonus = 1.4f;
        accuracyBonus = 0.9f;
    }
}

[CreateAssetMenu(fileName = "SupportSet_Config", menuName = "Game/Weapon Sets/Support Set")]
public class SupportWeaponSet : WeaponSet
{
    void Reset()
    {
        setType = SetType.Support;
        displayName = "Support Weapon Set";
        description = "Важкий набір для підтримки команди";
        recommendedLevel = 10;
        
        primaryAmmo = 300;
        secondaryAmmo = 34;
        specialAmmo = 12;
        
        damageBonus = 1.4f;
        reloadSpeedBonus = 0.8f;
        accuracyBonus = 0.8f;
    }
}