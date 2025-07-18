using UnityEngine;
using UnityEngine.SceneManagement;
using IndieShooter.Core;

namespace IndieShooter.Core
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        
        [Header("Loading Settings")]
        public GameObject loadingScreen;
        public UnityEngine.UI.Slider progressBar;
        public UnityEngine.UI.Text loadingText;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        
        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }
        
        private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);
                
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            
            while (!operation.isDone)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                
                if (progressBar != null)
                    progressBar.value = progress;
                    
                if (loadingText != null)
                    loadingText.text = $"Loading... {progress * 100:F0}%";
                
                if (operation.progress >= 0.9f)
                {
                    if (loadingText != null)
                        loadingText.text = "Press any key to continue...";
                        
                    if (Input.anyKeyDown)
                    {
                        operation.allowSceneActivation = true;
                    }
                }
                
                yield return null;
            }
            
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        
        private System.Collections.IEnumerator LoadSceneAsync(int sceneIndex)
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(true);
                
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
            
            while (!operation.isDone)
            {
                float progress = operation.progress;
                
                if (progressBar != null)
                    progressBar.value = progress;
                    
                if (loadingText != null)
                    loadingText.text = $"Loading... {progress * 100:F0}%";
                
                yield return null;
            }
            
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
        
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }
        
        public void LoadGameScene()
        {
            LoadScene("MainScene");
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}