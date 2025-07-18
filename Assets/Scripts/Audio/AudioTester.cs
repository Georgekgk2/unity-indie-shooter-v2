using UnityEngine;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    public class AudioTester : MonoBehaviour
    {
        [Header("Test Controls")]
        public KeyCode testWeaponFireKey = KeyCode.Alpha1;
        public KeyCode testWeaponReloadKey = KeyCode.Alpha2;
        public KeyCode testFootstepKey = KeyCode.Alpha3;
        public KeyCode testImpactKey = KeyCode.Alpha4;
        public KeyCode testUIKey = KeyCode.Alpha5;
        
        [Header("Test Settings")]
        public bool enableTesting = true;
        public bool showDebugInfo = true;
        
        void Update()
        {
            if (!enableTesting || AudioManager.Instance == null)
                return;
                
            HandleTestInput();
        }
        
        void HandleTestInput()
        {
            if (Input.GetKeyDown(testWeaponFireKey))
            {
                TestWeaponSounds();
            }
            
            if (Input.GetKeyDown(testWeaponReloadKey))
            {
                TestReloadSounds();
            }
            
            if (Input.GetKeyDown(testFootstepKey))
            {
                TestFootstepSounds();
            }
            
            if (Input.GetKeyDown(testImpactKey))
            {
                TestImpactSounds();
            }
            
            if (Input.GetKeyDown(testUIKey))
            {
                TestUISounds();
            }
        }
        
        void TestWeaponSounds()
        {
            AudioManager.Instance.PlaySFX("WeaponFire");
            
            if (showDebugInfo)
            {
                Debug.Log("Testing Weapon Fire Sound");
            }
        }
        
        void TestReloadSounds()
        {
            AudioManager.Instance.PlaySFX("WeaponReload");
            
            // Play reload complete after delay
            Invoke("PlayReloadComplete", 1.5f);
            
            if (showDebugInfo)
            {
                Debug.Log("Testing Weapon Reload Sounds");
            }
        }
        
        void PlayReloadComplete()
        {
            AudioManager.Instance.PlaySFX("WeaponReloadComplete");
        }
        
        void TestFootstepSounds()
        {
            string[] footsteps = { "FootstepWalk1", "FootstepWalk2", "FootstepWalk3" };
            string randomFootstep = footsteps[Random.Range(0, footsteps.Length)];
            AudioManager.Instance.PlaySFX(randomFootstep);
            
            if (showDebugInfo)
            {
                Debug.Log($"Testing Footstep Sound: {randomFootstep}");
            }
        }
        
        void TestImpactSounds()
        {
            string[] impacts = { "ImpactDirt", "ImpactConcrete", "ImpactFlesh", "ImpactDefault" };
            string randomImpact = impacts[Random.Range(0, impacts.Length)];
            AudioManager.Instance.PlaySFX(randomImpact);
            
            if (showDebugInfo)
            {
                Debug.Log($"Testing Impact Sound: {randomImpact}");
            }
        }
        
        void TestUISounds()
        {
            AudioManager.Instance.PlaySFX("UIButtonClick");
            
            if (showDebugInfo)
            {
                Debug.Log("Testing UI Button Click Sound");
            }
        }
        
        void OnGUI()
        {
            if (!enableTesting || !showDebugInfo)
                return;
                
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Audio System Tester", GUI.skin.box);
            GUILayout.Label($"Press {testWeaponFireKey} - Test Weapon Fire");
            GUILayout.Label($"Press {testWeaponReloadKey} - Test Weapon Reload");
            GUILayout.Label($"Press {testFootstepKey} - Test Footsteps");
            GUILayout.Label($"Press {testImpactKey} - Test Impact Sounds");
            GUILayout.Label($"Press {testUIKey} - Test UI Sounds");
            
            if (AudioManager.Instance != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Master Volume: {AudioManager.Instance.masterVolume:F2}");
                GUILayout.Label($"SFX Volume: {AudioManager.Instance.sfxVolume:F2}");
                GUILayout.Label($"Ambient Volume: {AudioManager.Instance.ambientVolume:F2}");
            }
            
            GUILayout.EndArea();
        }
        
        // Public methods for external testing
        public void TestAllSounds()
        {
            StartCoroutine(TestAllSoundsCoroutine());
        }
        
        System.Collections.IEnumerator TestAllSoundsCoroutine()
        {
            TestWeaponSounds();
            yield return new WaitForSeconds(0.5f);
            
            TestFootstepSounds();
            yield return new WaitForSeconds(0.5f);
            
            TestImpactSounds();
            yield return new WaitForSeconds(0.5f);
            
            TestUISounds();
            yield return new WaitForSeconds(0.5f);
            
            TestReloadSounds();
        }
        
        public void TestSpecificSound(string soundName)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(soundName);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Testing Sound: {soundName}");
                }
            }
        }
    }
}