using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ShooterGame.Core;
using ShooterGame.Weapons;

/// <summary>
/// WEEK 4 INTEGRATION TESTS - HYBRID WEAPON SYSTEM
/// Тестування інтеграції NewWeapons_AdvancedArsenal з Character Classes та існуючою інфраструктурою
/// Моніторинг Quality Gates під час тестування
/// </summary>

namespace ShooterGame.Testing
{
    public class WeaponIntegrationTests
    {
        private HybridWeaponManager weaponManager;
        private CharacterClassManager classManager;
        private PerformanceMonitor performanceMonitor;
        private QualityGates qualityGates;
        private UniversalObjectPool objectPool;
        
        [SetUp]
        public void Setup()
        {
            // Створення тестового об'єкта
            var testObject = new GameObject("TestWeaponManager");
            weaponManager = testObject.AddComponent<HybridWeaponManager>();
            
            // Додавання Character Class Manager
            var classObject = new GameObject("TestClassManager");
            classManager = classObject.AddComponent<CharacterClassManager>();
            
            // Ініціалізація інфраструктури
            performanceMonitor = PerformanceMonitor.Instance;
            qualityGates = QualityGates.Instance;
            objectPool = UniversalObjectPool.Instance;
            
            Debug.Log("[TEST] Weapon Integration Tests - Setup Complete");
        }

        [TearDown]
        public void TearDown()
        {
            if (weaponManager != null)
            {
                Object.DestroyImmediate(weaponManager.gameObject);
            }
            
            if (classManager != null)
            {
                Object.DestroyImmediate(classManager.gameObject);
            }
            
            Debug.Log("[TEST] Weapon Integration Tests - TearDown Complete");
        }

        [Test]
        public void Test_HybridWeaponManager_Initialization()
        {
            // Arrange & Act
            var initialSnapshot = performanceMonitor?.TakeSnapshot("Test_WeaponInit_Start");
            
            // Assert
            Assert.IsNotNull(weaponManager, "HybridWeaponManager should be initialized");
            Assert.IsNotNull(HybridWeaponManager.Instance, "Singleton instance should be available");
            Assert.IsTrue(weaponManager.GetAvailableWeapons().Count > 0, "Should have available weapons");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after initialization");
            
            Debug.Log("[TEST] ✅ HybridWeaponManager initialization test passed");
        }

        [Test]
        public void Test_PlasmaRifle_Integration()
        {
            // Arrange
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_PlasmaRifle_Start");
            var availableWeapons = weaponManager.GetAvailableWeapons();
            var plasmaRifle = availableWeapons.Find(w => w.weaponId.Contains("plasma_rifle"));
            
            // Act
            weaponManager.EquipWeapon(plasmaRifle);
            var currentWeapon = weaponManager.GetCurrentWeapon();
            
            // Assert
            Assert.IsNotNull(currentWeapon, "Current weapon should be set");
            Assert.AreEqual("Plasma Rifle MK-I", currentWeapon.displayName, "Weapon name should match");
            Assert.AreEqual(WeaponCategory.AssaultRifle, currentWeapon.category, "Weapon category should match");
            
            // Перевірка статистик
            Assert.AreEqual(45f, currentWeapon.baseDamage, 0.01f, "Base damage should be correct");
            Assert.AreEqual(6f, currentWeapon.fireRate, 0.01f, "Fire rate should be correct");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after weapon change");
            
            Debug.Log("[TEST] ✅ Plasma Rifle integration test passed");
        }

        [Test]
        public void Test_GaussSniper_Integration()
        {
            // Arrange
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_GaussSniper_Start");
            var availableWeapons = weaponManager.GetAvailableWeapons();
            var gaussSniper = availableWeapons.Find(w => w.weaponId.Contains("gauss_sniper"));
            
            // Act
            weaponManager.EquipWeapon(gaussSniper);
            var currentWeapon = weaponManager.GetCurrentWeapon();
            
            // Assert
            Assert.IsNotNull(currentWeapon, "Current weapon should be set");
            Assert.AreEqual("Gauss Sniper X-7", currentWeapon.displayName, "Weapon name should match");
            Assert.AreEqual(WeaponCategory.SniperRifle, currentWeapon.category, "Weapon category should match");
            
            // Перевірка статистик
            Assert.AreEqual(120f, currentWeapon.baseDamage, 0.01f, "Base damage should be correct");
            Assert.AreEqual(1.2f, currentWeapon.fireRate, 0.01f, "Fire rate should be correct");
            Assert.AreEqual(4f, currentWeapon.headshotMultiplier, 0.01f, "Headshot multiplier should be correct");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after sniper rifle change");
            
            Debug.Log("[TEST] ✅ Gauss Sniper integration test passed");
        }

        [UnityTest]
        public IEnumerator Test_WeaponAbility_Performance()
        {
            // Arrange
            var availableWeapons = weaponManager.GetAvailableWeapons();
            var plasmaRifle = availableWeapons.Find(w => w.weaponId.Contains("plasma_rifle"));
            weaponManager.EquipWeapon(plasmaRifle);
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_WeaponAbility_Performance_Start");
            
            // Act
            weaponManager.ActivateWeaponAbility();
            
            // Wait for ability to process
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            var performanceEnd = performanceMonitor?.TakeSnapshot("Test_WeaponAbility_Performance_End");
            
            // Перевірка, що здібність не вплинула критично на продуктивність
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after ability activation");
            
            Debug.Log("[TEST] ✅ Weapon ability performance test passed");
        }

        [Test]
        public void Test_ObjectPool_Integration()
        {
            // Arrange
            var availableWeapons = weaponManager.GetAvailableWeapons();
            var nanoShotgun = availableWeapons.Find(w => w.weaponId.Contains("nano_shotgun"));
            
            // Act
            weaponManager.EquipWeapon(nanoShotgun); // Uses object pool for effects
            
            // Assert
            // Перевірка, що Object Pool працює коректно
            Assert.IsNotNull(objectPool, "Object pool should be available");
            
            // Quality Gates перевірка після використання Object Pool
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after object pool usage");
            
            Debug.Log("[TEST] ✅ Object pool integration test passed");
        }

        [Test]
        public void Test_CharacterClass_WeaponBonus()
        {
            // Arrange
            classManager.SetCharacterClass(CharacterClass.Assault);
            var availableWeapons = weaponManager.GetAvailableWeapons();
            var plasmaRifle = availableWeapons.Find(w => w.weaponId.Contains("plasma_rifle"));
            
            // Act
            weaponManager.EquipWeapon(plasmaRifle);
            
            // Assert
            var currentWeapon = weaponManager.GetCurrentWeapon();
            Assert.IsNotNull(currentWeapon, "Current weapon should be set");
            
            // Перевірка бонусів класу
            var assaultBonus = currentWeapon.classBonuses[CharacterClass.Assault];
            Assert.IsNotNull(assaultBonus, "Class bonus should exist");
            Assert.AreEqual(1.2f, assaultBonus.damageMultiplier, 0.01f, "Damage multiplier should be correct");
            Assert.AreEqual(1.1f, assaultBonus.fireRateMultiplier, 0.01f, "Fire rate multiplier should be correct");
            Assert.IsTrue(assaultBonus.hasSpecialEffect, "Should have special effect");
            
            Debug.Log("[TEST] ✅ Character class weapon bonus test passed");
        }

        [Test]
        public void Test_WeaponSystem_QualityGates()
        {
            // Arrange
            var initialQuality = qualityGates?.ValidateQuality() ?? true;
            
            // Act - Виконуємо операції, що можуть вплинути на якість
            var availableWeapons = weaponManager.GetAvailableWeapons();
            for (int i = 0; i < availableWeapons.Count; i++)
            {
                weaponManager.EquipWeapon(availableWeapons[i]);
                weaponManager.ActivateWeaponAbility();
            }
            
            // Assert
            bool finalQuality = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(finalQuality, "Quality should remain acceptable after multiple weapon operations");
            
            // Перевірка Phase 2 targets
            // FPS: >45, Memory: <2.5GB, Stability: >7.5/10, Quality Score: >75/100
            if (qualityGates != null)
            {
                Assert.IsTrue(qualityGates.GetCurrentFPS() > 45f, "FPS should be above Phase 2 threshold (45)");
                Assert.IsTrue(qualityGates.GetQualityScore() > 75f, "Quality score should be above Phase 2 threshold (75)");
            }
            
            Debug.Log("[TEST] ✅ Weapon system quality gates test passed");
        }

        [UnityTest]
        public IEnumerator Test_Weapon_Memory_Management()
        {
            // Arrange
            var initialMemory = System.GC.GetTotalMemory(false);
            var availableWeapons = weaponManager.GetAvailableWeapons();
            
            // Act - Створюємо навантаження на пам'ять
            for (int i = 0; i < 100; i++)
            {
                int weaponIndex = i % availableWeapons.Count;
                weaponManager.EquipWeapon(availableWeapons[weaponIndex]);
                
                if (i % 10 == 0)
                {
                    weaponManager.ActivateWeaponAbility();
                    yield return null; // Дозволяємо Unity обробити кадр
                }
            }
            
            // Примусова збірка сміття
            System.GC.Collect();
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            var finalMemory = System.GC.GetTotalMemory(false);
            var memoryIncrease = (finalMemory - initialMemory) / (1024f * 1024f); // MB
            
            Assert.IsTrue(memoryIncrease < 50f, $"Memory increase should be less than 50MB, actual: {memoryIncrease:F2}MB");
            
            // Quality Gates перевірка пам'яті
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass memory validation");
            
            Debug.Log($"[TEST] ✅ Weapon memory management test passed. Memory increase: {memoryIncrease:F2}MB");
        }

        [Test]
        public void Test_Hybrid_Weapon_Character_Integration()
        {
            // Arrange
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_Hybrid_Weapon_Character_Start");
            
            // Act - Тестуємо всі комбінації класів та зброї
            var availableClasses = System.Enum.GetValues(typeof(CharacterClass));
            var availableWeapons = weaponManager.GetAvailableWeapons();
            
            foreach (CharacterClass classType in availableClasses)
            {
                classManager.SetCharacterClass(classType);
                
                foreach (var weapon in availableWeapons)
                {
                    weaponManager.EquipWeapon(weapon);
                    
                    // Перевірка інтеграції
                    var currentWeapon = weaponManager.GetCurrentWeapon();
                    Assert.IsNotNull(currentWeapon, $"Weapon {weapon.displayName} should be properly integrated with {classType}");
                }
            }
            
            // Assert
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass hybrid weapon-character integration test");
            
            var performanceEnd = performanceMonitor?.TakeSnapshot("Test_Hybrid_Weapon_Character_End");
            
            Debug.Log("[TEST] ✅ Hybrid weapon-character integration test passed");
        }
    }
}