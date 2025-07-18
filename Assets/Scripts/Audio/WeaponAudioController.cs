using UnityEngine;
using IndieShooter.Core;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    public class WeaponAudioController : MonoBehaviour
    {
        [Header("Weapon Sounds")]
        public string fireSound = "WeaponFire";
        public string reloadSound = "WeaponReload";
        public string reloadCompleteSound = "WeaponReloadComplete";
        public string emptySound = "WeaponEmpty";
        public string weaponSwitchSound = "WeaponSwitch";
        
        [Header("Shell Casing Sounds")]
        public string shellCasingSound = "ShellCasing";
        public float shellCasingDelay = 0.1f;
        
        [Header("Mechanical Sounds")]
        public string boltActionSound = "WeaponBolt";
        public string safetyClickSound = "WeaponSafety";
        public string triggerClickSound = "WeaponTrigger";
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float weaponVolume = 1f;
        public bool randomizePitch = true;
        [Range(0f, 0.3f)]
        public float pitchVariation = 0.1f;
        
        private bool isReloading = false;
        private float lastFireTime = 0f;
        
        void Start()
        {
            // Subscribe to weapon events
            EventSystem.Instance?.Subscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Subscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Subscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Subscribe("WeaponEmpty", OnWeaponEmpty);
            EventSystem.Instance?.Subscribe("WeaponSwitched", OnWeaponSwitched);
        }
        
        void OnWeaponFired(object data)
        {
            PlayWeaponSound(fireSound);
            lastFireTime = Time.time;
            
            // Play shell casing sound with delay
            if (!string.IsNullOrEmpty(shellCasingSound))
            {
                Invoke("PlayShellCasingSound", shellCasingDelay);
            }
        }
        
        void OnWeaponReloading(object data)
        {
            isReloading = true;
            PlayWeaponSound(reloadSound);
        }
        
        void OnWeaponReloaded(object data)
        {
            isReloading = false;
            PlayWeaponSound(reloadCompleteSound);
            
            // Play bolt action sound for certain weapon types
            if (!string.IsNullOrEmpty(boltActionSound))
            {
                Invoke("PlayBoltActionSound", 0.2f);
            }
        }
        
        void OnWeaponEmpty(object data)
        {
            PlayWeaponSound(emptySound);
            PlayWeaponSound(triggerClickSound);
        }
        
        void OnWeaponSwitched(object data)
        {
            PlayWeaponSound(weaponSwitchSound);
        }
        
        void PlayWeaponSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName) || AudioManager.Instance == null)
                return;
                
            AudioManager.Instance.PlaySFXAtPosition(soundName, transform.position);
        }
        
        void PlayShellCasingSound()
        {
            PlayWeaponSound(shellCasingSound);
        }
        
        void PlayBoltActionSound()
        {
            PlayWeaponSound(boltActionSound);
        }
        
        // Public methods for manual triggering
        public void PlayFireSound()
        {
            PlayWeaponSound(fireSound);
        }
        
        public void PlayReloadSound()
        {
            PlayWeaponSound(reloadSound);
        }
        
        public void PlayEmptySound()
        {
            PlayWeaponSound(emptySound);
        }
        
        public void PlayMechanicalSound(string soundName)
        {
            PlayWeaponSound(soundName);
        }
        
        // Configuration methods
        public void SetWeaponSounds(string fire, string reload, string empty)
        {
            fireSound = fire;
            reloadSound = reload;
            emptySound = empty;
        }
        
        public void SetWeaponVolume(float volume)
        {
            weaponVolume = Mathf.Clamp01(volume);
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Unsubscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Unsubscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Unsubscribe("WeaponEmpty", OnWeaponEmpty);
            EventSystem.Instance?.Unsubscribe("WeaponSwitched", OnWeaponSwitched);
        }
    }
}