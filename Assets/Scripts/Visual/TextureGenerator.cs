using UnityEngine;

namespace IndieShooter.Visual
{
    public static class TextureGenerator
    {
        public static Texture2D GenerateNoiseTexture(int width, int height, float scale = 1f)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float noiseValue = Mathf.PerlinNoise(x * scale / width, y * scale / height);
                    pixels[y * width + x] = new Color(noiseValue, noiseValue, noiseValue, 1f);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GenerateCheckerboardTexture(int width, int height, int checkerSize = 8)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isWhite = ((x / checkerSize) + (y / checkerSize)) % 2 == 0;
                    pixels[y * width + x] = isWhite ? Color.white : Color.gray;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GenerateGradientTexture(int width, int height, Color startColor, Color endColor, bool horizontal = true)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float t = horizontal ? (float)x / width : (float)y / height;
                    pixels[y * width + x] = Color.Lerp(startColor, endColor, t);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GenerateCircleTexture(int size, Color centerColor, Color edgeColor)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDistance = size * 0.5f;
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(distance / maxDistance);
                    pixels[y * size + x] = Color.Lerp(centerColor, edgeColor, t);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GenerateNormalMap(Texture2D heightMap, float strength = 1f)
        {
            int width = heightMap.width;
            int height = heightMap.height;
            Texture2D normalMap = new Texture2D(width, height, TextureFormat.RGB24, false);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float left = GetHeightValue(heightMap, x - 1, y);
                    float right = GetHeightValue(heightMap, x + 1, y);
                    float down = GetHeightValue(heightMap, x, y - 1);
                    float up = GetHeightValue(heightMap, x, y + 1);
                    
                    Vector3 normal = new Vector3((left - right) * strength, (down - up) * strength, 1f).normalized;
                    
                    // Convert from [-1,1] to [0,1] range
                    Color normalColor = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f,
                        1f
                    );
                    
                    normalMap.SetPixel(x, y, normalColor);
                }
            }
            
            normalMap.Apply();
            return normalMap;
        }
        
        private static float GetHeightValue(Texture2D texture, int x, int y)
        {
            x = Mathf.Clamp(x, 0, texture.width - 1);
            y = Mathf.Clamp(y, 0, texture.height - 1);
            return texture.GetPixel(x, y).grayscale;
        }
        
        public static Texture2D GenerateMetallicMap(int width, int height, float metallicValue = 0.5f, float roughnessValue = 0.5f)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                // R = Metallic, G = Occlusion, B = Detail Mask, A = Smoothness (1 - Roughness)
                pixels[i] = new Color(metallicValue, 1f, 0f, 1f - roughnessValue);
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}