using UnityEngine;
using UnityEngine.UI;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    public class UIAudioController : MonoBehaviour
    {
        [Header("UI Sounds")]
        public string buttonClickSound = "UIButtonClick";
        public string buttonHoverSound = "UIButtonHover";
        public string menuOpenSound = "UIMenuOpen";
        public string menuCloseSound = "UIMenuClose";
        public string errorSound = "UIError";
        public string successSound = "UISuccess";
        public string notificationSound = "UINotification";
        
        [Header("Game UI Sounds")]
        public string healthLowSound = "UIHealthLow";
        public string ammoLowSound = "UIAmmoLow";
        public string pickupSound = "UIPickup";
        public string achievementSound = "UIAchievement";
        
        [Header("Auto-Setup")]
        public bool autoSetupButtons = true;
        public bool autoSetupSliders = true;
        public bool autoSetupToggles = true;
        
        void Start()
        {
            if (autoSetupButtons)
            {
                SetupButtons();
            }
            
            if (autoSetupSliders)
            {
                SetupSliders();
            }
            
            if (autoSetupToggles)
            {
                SetupToggles();
            }
        }
        
        void SetupButtons()
        {
            Button[] buttons = FindObjectsOfType<Button>();
            foreach (Button button in buttons)
            {
                // Add click sound
                button.onClick.AddListener(() => PlayUISound(buttonClickSound));
                
                // Add hover sound using EventTrigger
                var eventTrigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                {
                    eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                }
                
                var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                hoverEntry.callback.AddListener((data) => { PlayUISound(buttonHoverSound); });
                eventTrigger.triggers.Add(hoverEntry);
            }
        }
        
        void SetupSliders()
        {
            Slider[] sliders = FindObjectsOfType<Slider>();
            foreach (Slider slider in sliders)
            {
                slider.onValueChanged.AddListener((value) => PlayUISound(buttonClickSound));
            }
        }
        
        void SetupToggles()
        {
            Toggle[] toggles = FindObjectsOfType<Toggle>();
            foreach (Toggle toggle in toggles)
            {
                toggle.onValueChanged.AddListener((isOn) => PlayUISound(buttonClickSound));
            }
        }
        
        public void PlayUISound(string soundName)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioManager.Instance.PlaySFX(soundName);
            }
        }
        
        // Public methods for specific UI events
        public void PlayButtonClick()
        {
            PlayUISound(buttonClickSound);
        }
        
        public void PlayButtonHover()
        {
            PlayUISound(buttonHoverSound);
        }
        
        public void PlayMenuOpen()
        {
            PlayUISound(menuOpenSound);
        }
        
        public void PlayMenuClose()
        {
            PlayUISound(menuCloseSound);
        }
        
        public void PlayError()
        {
            PlayUISound(errorSound);
        }
        
        public void PlaySuccess()
        {
            PlayUISound(successSound);
        }
        
        public void PlayNotification()
        {
            PlayUISound(notificationSound);
        }
        
        public void PlayHealthLow()
        {
            PlayUISound(healthLowSound);
        }
        
        public void PlayAmmoLow()
        {
            PlayUISound(ammoLowSound);
        }
        
        public void PlayPickup()
        {
            PlayUISound(pickupSound);
        }
        
        public void PlayAchievement()
        {
            PlayUISound(achievementSound);
        }
        
        // Volume control for UI sounds
        public void SetUIVolume(float volume)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }
        }
    }
}