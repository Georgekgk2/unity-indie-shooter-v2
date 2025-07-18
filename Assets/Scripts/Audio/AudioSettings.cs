using UnityEngine;
using UnityEngine.UI;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    public class AudioSettings : MonoBehaviour
    {
        [Header("UI References")]
        public Slider masterVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider ambientVolumeSlider;
        
        [Header("Volume Labels")]
        public Text masterVolumeLabel;
        public Text sfxVolumeLabel;
        public Text ambientVolumeLabel;
        
        [Header("Test Buttons")]
        public Button testSFXButton;
        public Button testAmbientButton;
        
        [Header("Test Sounds")]
        public string testSFXSound = "WeaponFire";
        public string testAmbientSound = "AmbientWind";
        
        void Start()
        {
            InitializeSliders();
            SetupEventListeners();
            UpdateLabels();
        }
        
        void InitializeSliders()
        {
            if (AudioManager.Instance != null)
            {
                if (masterVolumeSlider != null)
                {
                    masterVolumeSlider.value = AudioManager.Instance.masterVolume;
                }
                
                if (sfxVolumeSlider != null)
                {
                    sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;
                }
                
                if (ambientVolumeSlider != null)
                {
                    ambientVolumeSlider.value = AudioManager.Instance.ambientVolume;
                }
            }
        }
        
        void SetupEventListeners()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            
            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            }
            
            if (testSFXButton != null)
            {
                testSFXButton.onClick.AddListener(TestSFX);
            }
            
            if (testAmbientButton != null)
            {
                testAmbientButton.onClick.AddListener(TestAmbient);
            }
        }
        
        void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
            UpdateLabels();
        }
        
        void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
            UpdateLabels();
        }
        
        void OnAmbientVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetAmbientVolume(value);
            }
            UpdateLabels();
        }
        
        void UpdateLabels()
        {
            if (masterVolumeLabel != null && masterVolumeSlider != null)
            {
                masterVolumeLabel.text = $"Master: {(masterVolumeSlider.value * 100):F0}%";
            }
            
            if (sfxVolumeLabel != null && sfxVolumeSlider != null)
            {
                sfxVolumeLabel.text = $"SFX: {(sfxVolumeSlider.value * 100):F0}%";
            }
            
            if (ambientVolumeLabel != null && ambientVolumeSlider != null)
            {
                ambientVolumeLabel.text = $"Ambient: {(ambientVolumeSlider.value * 100):F0}%";
            }
        }
        
        void TestSFX()
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(testSFXSound))
            {
                AudioManager.Instance.PlaySFX(testSFXSound);
            }
        }
        
        void TestAmbient()
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(testAmbientSound))
            {
                AudioManager.Instance.PlaySFX(testAmbientSound);
            }
        }
        
        // Public methods for external control
        public void ResetToDefaults()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = 1f;
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = 0.8f;
            }
            
            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.value = 0.6f;
            }
        }
        
        public void MuteAll()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = 0f;
            }
        }
        
        public void UnmuteAll()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = 1f;
            }
        }
    }
}