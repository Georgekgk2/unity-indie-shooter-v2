using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.Visual
{
    [System.Serializable]
    public class MaterialSet
    {
        public string setName;
        public Material[] materials;
        public Texture2D[] textures;
    }
    
    public class MaterialManager : MonoBehaviour
    {
        public static MaterialManager Instance { get; private set; }
        
        [Header("Material Sets")]
        public List<MaterialSet> materialSets = new List<MaterialSet>();
        
        [Header("Runtime Generated Textures")]
        public bool generateTexturesAtRuntime = true;
        public int textureResolution = 512;
        
        private Dictionary<string, MaterialSet> materialDatabase;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMaterials();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void InitializeMaterials()
        {
            materialDatabase = new Dictionary<string, MaterialSet>();
            
            foreach (MaterialSet set in materialSets)
            {
                materialDatabase[set.setName] = set;
            }
            
            if (generateTexturesAtRuntime)
            {
                GenerateRuntimeTextures();
            }
        }
        
        void GenerateRuntimeTextures()
        {
            // Generate basic textures for materials that don't have them
            CreateBasicTextures();
            CreateEnvironmentTextures();
            CreateEffectTextures();
        }
        
        void CreateBasicTextures()
        {
            // Create a basic concrete texture
            Texture2D concreteTexture = TextureGenerator.GenerateNoiseTexture(textureResolution, textureResolution, 0.1f);
            concreteTexture.name = "Generated_Concrete";
            
            // Create a basic metal texture
            Texture2D metalTexture = TextureGenerator.GenerateCheckerboardTexture(textureResolution, textureResolution, 32);
            metalTexture.name = "Generated_Metal";
            
            // Create a basic ground texture
            Texture2D groundTexture = TextureGenerator.GenerateGradientTexture(textureResolution, textureResolution, 
                new Color(0.4f, 0.3f, 0.2f), new Color(0.6f, 0.5f, 0.3f), false);
            groundTexture.name = "Generated_Ground";
            
            // Apply textures to materials
            ApplyTextureToMaterial("ConcreteMaterial", concreteTexture);
            ApplyTextureToMaterial("MetallicMaterial", metalTexture);
            ApplyTextureToMaterial("GroundMaterial", groundTexture);
        }
        
        void CreateEnvironmentTextures()
        {
            // Create environment-specific textures
            Texture2D brickTexture = TextureGenerator.GenerateCheckerboardTexture(textureResolution, textureResolution, 16);
            brickTexture.name = "Generated_Brick";
            
            Texture2D grassTexture = TextureGenerator.GenerateNoiseTexture(textureResolution, textureResolution, 0.05f);
            grassTexture.name = "Generated_Grass";
            
            // Store for later use
            StoreGeneratedTexture("Brick", brickTexture);
            StoreGeneratedTexture("Grass", grassTexture);
        }
        
        void CreateEffectTextures()
        {
            // Create effect textures
            Texture2D glowTexture = TextureGenerator.GenerateCircleTexture(textureResolution, 
                new Color(1f, 1f, 1f, 1f), new Color(0f, 0f, 0f, 0f));
            glowTexture.name = "Generated_Glow";
            
            Texture2D sparkTexture = TextureGenerator.GenerateCircleTexture(64, 
                new Color(1f, 0.8f, 0.2f, 1f), new Color(1f, 0.4f, 0.1f, 0f));
            sparkTexture.name = "Generated_Spark";
            
            StoreGeneratedTexture("Glow", glowTexture);
            StoreGeneratedTexture("Spark", sparkTexture);
        }
        
        void ApplyTextureToMaterial(string materialName, Texture2D texture)
        {
            Material material = FindMaterialByName(materialName);
            if (material != null)
            {
                material.mainTexture = texture;
            }
        }
        
        Material FindMaterialByName(string materialName)
        {
            foreach (MaterialSet set in materialSets)
            {
                foreach (Material mat in set.materials)
                {
                    if (mat != null && mat.name.Contains(materialName))
                    {
                        return mat;
                    }
                }
            }
            return null;
        }
        
        void StoreGeneratedTexture(string textureName, Texture2D texture)
        {
            // Store texture for later retrieval
            // This could be expanded to save to Resources or AssetDatabase in editor
        }
        
        public Material GetMaterial(string setName, int index = 0)
        {
            if (materialDatabase.ContainsKey(setName) && 
                materialDatabase[setName].materials.Length > index)
            {
                return materialDatabase[setName].materials[index];
            }
            return null;
        }
        
        public Texture2D GetTexture(string setName, int index = 0)
        {
            if (materialDatabase.ContainsKey(setName) && 
                materialDatabase[setName].textures.Length > index)
            {
                return materialDatabase[setName].textures[index];
            }
            return null;
        }
        
        public void SwapMaterial(Renderer renderer, string newMaterialSetName, int materialIndex = 0)
        {
            Material newMaterial = GetMaterial(newMaterialSetName, materialIndex);
            if (newMaterial != null && renderer != null)
            {
                renderer.material = newMaterial;
            }
        }
        
        public void SwapMaterials(Renderer renderer, string newMaterialSetName)
        {
            if (materialDatabase.ContainsKey(newMaterialSetName) && renderer != null)
            {
                renderer.materials = materialDatabase[newMaterialSetName].materials;
            }
        }
    }
}