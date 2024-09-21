using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Editor
{
    public class ClothingBuilder
    {
        private static readonly (string Name, float HueShift, float SaturationScale, float BrightnessPercent)[] colors = {
            ("White", 0, 0.0f, 0),
            ("Black", 0, 0.0f, -84),
            ("Gray", 0, 0.0f, -64),
            ("Red", 0, 0.6f, -40),
            ("Green", 120, 0.30f, -60),
            ("Olive", 78, 0.20f, -45),
            ("Blue", 208, 0.61f, -55),
            ("Navy", 208, 0.45f, -65),
            ("Pink", 306, 0.40f, 0),
            ("Purple", 295, 0.25f, -50),
        };

        private static readonly string[] prefabsToCopy = {
            "Animations.prefab",
            "Character_Mesh_3P_Override_0.prefab",
            "Item.prefab"
        };

        [MenuItem("Assets/ClothingBuilder/Build Shirts", false, 10)]
        private static void AdjustImageColors()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D texture)
                {
                    string path = AssetDatabase.GetAssetPath(texture);
                    AdjustImage(path, true);
                }
            }
        }

        private static void AdjustImage(string path, bool isShirt)
        {
            using (var img = new Bitmap(path))
            {
                string baseFolderName = Path.GetFileNameWithoutExtension(path);
                string baseOutputFolder = Path.Combine(Path.GetDirectoryName(path), baseFolderName);

                Directory.CreateDirectory(baseOutputFolder);

                for (int i = 0; i < colors.Length; i++)
                {
                    var (colorName, hueShift, saturationScale, brightnessPercent) = colors[i];
                    var newImg = AdjustHueSaturationBrightness(img, hueShift, saturationScale, brightnessPercent);

                    string colorOutputFolder = Path.Combine(baseOutputFolder, $"{baseFolderName}_{colorName}");
                    Directory.CreateDirectory(colorOutputFolder);
                    Debug.Log($"Created folder: {colorOutputFolder}");

                    string outputFileName = $"{baseFolderName}_{colorName}.png";
                    string outputPath = Path.Combine(colorOutputFolder, outputFileName);
                    newImg.Save(outputPath, ImageFormat.Png);
                    Debug.Log($"Generated: {outputPath}");

                    SetTextureImportSettings(outputPath);
                    CreateMaterial(colorOutputFolder, outputFileName);

                    CopyPrefabs(Path.GetDirectoryName(path), colorOutputFolder);
                }
            }
        }

        private static void CreateMaterial(string folderPath, string textureFileName)
        {
            string materialName = Path.GetFileNameWithoutExtension(textureFileName) + ".mat";
            string materialPath = Path.Combine(folderPath, materialName);

            Material material = new Material(Shader.Find("Standard"));
            string texturePath = Path.Combine(folderPath, textureFileName);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            material.mainTexture = texture;

            // Set smoothness to 0
            material.SetFloat("_Smoothness", 0);

            // Change rendering mode to cutout
            material.SetFloat("_Mode", 1); // 0 = Opaque, 1 = Cutout, 2 = Fade, 3 = Transparent
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHABLEND");
            material.EnableKeyword("_ALPHATEST");
            material.DisableKeyword("_TRANSPARENT");

            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created material: {materialPath}");
        }


        private static void CopyPrefabs(string sourceFolder, string destinationFolder)
        {
            Debug.Log($"Copying prefabs to: {destinationFolder}");
            foreach (var prefab in prefabsToCopy)
            {
                string sourcePath = Path.Combine(sourceFolder, prefab);
                string destinationPath = Path.Combine(destinationFolder, prefab);

                if (File.Exists(sourcePath))
                {
                    // Copy the prefab
                    File.Copy(sourcePath, destinationPath, true);
                    Debug.Log($"Copied: {prefab} to {destinationFolder}");

                    // Load the prefab and assign the new material
                    ApplyMaterialToPrefab(destinationPath, Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(prefab) + ".mat"));
                }
                else
                {
                    Debug.LogWarning($"Prefab not found: {sourcePath}");
                }
            }
        }

        private static void ApplyMaterialToPrefab(string prefabPath, string materialPath)
        {
            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab could not be loaded: {prefabPath}");
                return;
            }

            // Load the material
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Debug.LogWarning($"Material could not be loaded: {materialPath}");
                return;
            }

            // Find the MeshRenderer and assign the new material
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"No MeshRenderers found in prefab: {prefabPath}");
                return;
            }

            foreach (var renderer in renderers)
            {
                Material[] materials = renderer.sharedMaterials;
                bool materialApplied = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material; // Assign the new material
                    materialApplied = true;
                }

                if (materialApplied)
                {
                    renderer.sharedMaterials = materials; // Update the materials
                    Debug.Log($"Applied material to renderer in prefab: {prefabPath}");
                }
                else
                {
                    Debug.LogWarning($"Material not applied to renderer: {renderer.name}");
                }
            }

            // Save the prefab
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"Saved prefab with new material: {prefabPath}");
        }



        private static void SetTextureImportSettings(string path)
        {
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Bitmap AdjustHueSaturationBrightness(Bitmap image, float hueShift, float saturationScale, float brightnessPercent)
        {
            Bitmap adjustedImg = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    if (pixelColor.A == 0) continue; // Skip fully transparent pixels

                    ColorToHLS(pixelColor, out float h, out float l, out float s);
                    h = (hueShift / 360f + h) % 1f;
                    s = saturationScale;
                    float brightnessAdjust = brightnessPercent / 100f;
                    l = Mathf.Clamp01(l * (1 + brightnessAdjust));

                    UnityEngine.Color newColor = HLSColorToRGB(h, l, s);
                    adjustedImg.SetPixel(x, y, System.Drawing.Color.FromArgb(pixelColor.A,
                        Mathf.Clamp((int)(newColor.r * 255), 0, 255),
                        Mathf.Clamp((int)(newColor.g * 255), 0, 255),
                        Mathf.Clamp((int)(newColor.b * 255), 0, 255)));
                }
            }

            return adjustedImg;
        }

        private static void ColorToHLS(System.Drawing.Color color, out float h, out float l, out float s)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);
            l = (max + min) / 2f;

            if (max == min)
            {
                h = 0f;
                s = 0f; // achromatic
            }
            else
            {
                float delta = max - min;
                s = l < 0.5f ? delta / (max + min) : delta / (2 - max - min);

                if (max == r)
                    h = (g - b) / delta + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / delta + 2;
                else
                    h = (r - g) / delta + 4;

                h /= 6;
            }
        }

        private static UnityEngine.Color HLSColorToRGB(float h, float l, float s)
        {
            if (s == 0)
            {
                int value = Mathf.RoundToInt(l * 255);
                return new UnityEngine.Color(value / 255f, value / 255f, value / 255f); // Grayscale
            }

            float q = l < 0.5f ? l * (1 + s) : l + s - (l * s);
            float p = 2 * l - q;

            float r = HueToRGB(p, q, h + 1f / 3f);
            float g = HueToRGB(p, q, h);
            float b = HueToRGB(p, q, h - 1f / 3f);

            return new UnityEngine.Color(r, g, b);
        }

        private static float HueToRGB(float p, float q, float t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1f / 6f) return p + (q - p) * 6 * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6;
            return p;
        }
    }
}
