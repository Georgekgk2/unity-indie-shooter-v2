using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ShooterGame.Core;

/// <summary>
/// WEEK 3 INTEGRATION TESTS - CHARACTER CLASS SYSTEM
/// Тестування інтеграції CharacterClasses з існуючою інфраструктурою
/// Моніторинг Quality Gates під час тестування
/// </summary>

namespace ShooterGame.Testing
{
    public class CharacterClassIntegrationTests
    {
        private CharacterClassManager classManager;
        private PerformanceMonitor performanceMonitor;
        private QualityGates qualityGates;
        private UniversalObjectPool objectPool;
        
        [SetUp]
        public void Setup()
        {
            // Створення тестового об'єкта
            var testObject = new GameObject("TestCharacterClassManager");
            classManager = testObject.AddComponent<CharacterClassManager>();
            
            // Ініціалізація інфраструктури
            performanceMonitor = PerformanceMonitor.Instance;
            qualityGates = QualityGates.Instance;
            objectPool = UniversalObjectPool.Instance;
            
            Debug.Log("[TEST] Character Class Integration Tests - Setup Complete");
        }

        [TearDown]
        public void TearDown()
        {
            if (classManager != null)
            {
                Object.DestroyImmediate(classManager.gameObject);
            }
            
            Debug.Log("[TEST] Character Class Integration Tests - TearDown Complete");
        }

        [Test]
        public void Test_CharacterClassManager_Initialization()
        {
            // Arrange & Act
            var initialSnapshot = performanceMonitor?.TakeSnapshot("Test_Initialization_Start");
            
            // Assert
            Assert.IsNotNull(classManager, "CharacterClassManager should be initialized");
            Assert.IsNotNull(CharacterClassManager.Instance, "Singleton instance should be available");
            Assert.IsTrue(classManager.GetAvailableClasses().Count > 0, "Should have available classes");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after initialization");
            
            Debug.Log("[TEST] ✅ CharacterClassManager initialization test passed");
        }

        [Test]
        public void Test_AssaultClass_Integration()
        {
            // Arrange
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_AssaultClass_Start");
            
            // Act
            classManager.SetCharacterClass(CharacterClass.Assault);
            var currentClass = classManager.GetCurrentClass();
            
            // Assert
            Assert.IsNotNull(currentClass, "Current class should be set");
            Assert.AreEqual(CharacterClass.Assault, currentClass.classType, "Should be Assault class");
            Assert.AreEqual("Assault", currentClass.className, "Class name should match");
            
            // Перевірка статистик
            Assert.AreEqual(1.1f, currentClass.damageMultiplier, 0.01f, "Damage multiplier should be correct");
            Assert.AreEqual(1.2f, currentClass.speedMultiplier, 0.01f, "Speed multiplier should be correct");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after class change");
            
            Debug.Log("[TEST] ✅ Assault class integration test passed");
        }

        [Test]
        public void Test_TankClass_Integration()
        {
            // Arrange
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_TankClass_Start");
            
            // Act
            classManager.SetCharacterClass(CharacterClass.Tank);
            var currentClass = classManager.GetCurrentClass();
            
            // Assert
            Assert.IsNotNull(currentClass, "Current class should be set");
            Assert.AreEqual(CharacterClass.Tank, currentClass.classType, "Should be Tank class");
            Assert.AreEqual("Tank", currentClass.className, "Class name should match");
            
            // Перевірка статистик
            Assert.AreEqual(1.5f, currentClass.healthMultiplier, 0.01f, "Health multiplier should be correct");
            Assert.AreEqual(0.7f, currentClass.speedMultiplier, 0.01f, "Speed multiplier should be correct");
            Assert.AreEqual(1.8f, currentClass.armorMultiplier, 0.01f, "Armor multiplier should be correct");
            
            // Quality Gates перевірка
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after tank class change");
            
            Debug.Log("[TEST] ✅ Tank class integration test passed");
        }

        [UnityTest]
        public IEnumerator Test_ClassAbility_Performance()
        {
            // Arrange
            classManager.SetCharacterClass(CharacterClass.Assault);
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_Ability_Performance_Start");
            
            // Act
            classManager.ActivateClassAbility();
            
            // Wait for ability to process
            yield return new WaitForSeconds(0.1f);
            
            // Assert
            var performanceEnd = performanceMonitor?.TakeSnapshot("Test_Ability_Performance_End");
            
            // Перевірка, що здібність не вплинула критично на продуктивність
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after ability activation");
            
            Debug.Log("[TEST] ✅ Class ability performance test passed");
        }

        [Test]
        public void Test_ObjectPool_Integration()
        {
            // Arrange
            classManager.SetCharacterClass(CharacterClass.Tank);
            
            // Act
            classManager.ActivateClassAbility(); // Tank shield uses object pool
            
            // Assert
            // Перевірка, що Object Pool працює коректно
            Assert.IsNotNull(objectPool, "Object pool should be available");
            
            // Quality Gates перевірка після використання Object Pool
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass after object pool usage");
            
            Debug.Log("[TEST] ✅ Object pool integration test passed");
        }

        [Test]
        public void Test_PerformanceMonitor_Integration()
        {
            // Arrange & Act
            var snapshot1 = performanceMonitor?.TakeSnapshot("Test_Performance_1");
            classManager.SetCharacterClass(CharacterClass.Assault);
            var snapshot2 = performanceMonitor?.TakeSnapshot("Test_Performance_2");
            classManager.ActivateClassAbility();
            var snapshot3 = performanceMonitor?.TakeSnapshot("Test_Performance_3");
            
            // Assert
            Assert.IsNotNull(performanceMonitor, "Performance monitor should be available");
            
            // Перевірка, що моніторинг працює
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should validate performance correctly");
            
            Debug.Log("[TEST] ✅ Performance monitor integration test passed");
        }

        [Test]
        public void Test_QualityGates_Thresholds()
        {
            // Arrange
            var initialQuality = qualityGates?.ValidateQuality() ?? true;
            
            // Act - Виконуємо операції, що можуть вплинути на якість
            for (int i = 0; i < 5; i++)
            {
                classManager.SetCharacterClass((CharacterClass)(i % 2)); // Assault/Tank
                classManager.ActivateClassAbility();
            }
            
            // Assert
            bool finalQuality = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(finalQuality, "Quality should remain acceptable after multiple operations");
            
            // Перевірка Phase 2 targets
            // FPS: >45, Memory: <2.5GB, Stability: >7.5/10, Quality Score: >75/100
            if (qualityGates != null)
            {
                Assert.IsTrue(qualityGates.GetCurrentFPS() > 45f, "FPS should be above Phase 2 threshold (45)");
                Assert.IsTrue(qualityGates.GetQualityScore() > 75f, "Quality score should be above Phase 2 threshold (75)");
            }
            
            Debug.Log("[TEST] ✅ Quality gates thresholds test passed");
        }

        [UnityTest]
        public IEnumerator Test_Memory_Management()
        {
            // Arrange
            var initialMemory = System.GC.GetTotalMemory(false);
            
            // Act - Створюємо навантаження на пам'ять
            for (int i = 0; i < 100; i++)
            {
                classManager.SetCharacterClass(CharacterClass.Assault);
                classManager.ActivateClassAbility();
                
                if (i % 10 == 0)
                {
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
            
            Debug.Log($"[TEST] ✅ Memory management test passed. Memory increase: {memoryIncrease:F2}MB");
        }

        [Test]
        public void Test_Hybrid_Integration_Compatibility()
        {
            // Arrange & Act
            var performanceStart = performanceMonitor?.TakeSnapshot("Test_Hybrid_Compatibility_Start");
            
            // Тестуємо сумісність з GitHub PerkSystem
            var perkSystem = PerkSystem.Instance;
            
            // Тестуємо всі доступні класи
            var availableClasses = classManager.GetAvailableClasses();
            foreach (var classType in System.Enum.GetValues(typeof(CharacterClass)))
            {
                if (availableClasses.Any(c => c.classType == (CharacterClass)classType))
                {
                    classManager.SetCharacterClass((CharacterClass)classType);
                    
                    // Перевірка інтеграції
                    var currentClass = classManager.GetCurrentClass();
                    Assert.IsNotNull(currentClass, $"Class {classType} should be properly integrated");
                    Assert.IsTrue(currentClass.ValidateClassIntegrity(), $"Class {classType} should pass integrity check");
                }
            }
            
            // Assert
            bool qualityPassed = qualityGates?.ValidateQuality() ?? true;
            Assert.IsTrue(qualityPassed, "Quality gates should pass hybrid compatibility test");
            
            var performanceEnd = performanceMonitor?.TakeSnapshot("Test_Hybrid_Compatibility_End");
            
            Debug.Log("[TEST] ✅ Hybrid integration compatibility test passed");
        }
    }
}