using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

//----------------------------------------------------//
//                      HUEY - V2.1.0                 //
//            https://github.com/Ceraia/Huey          //
//           Made by Axodouble & contributors.        //
//  I know the code is messy, but in BowieD's style:  //
//     https://www.youtube.com/watch?v=-nQn9UkHS4w    //
//----------------------------------------------------//

namespace Editor
{
    public class Huey
    {
        private static readonly (string Name, float HueShift, float SaturationScale, float LightnessPercent)[] colors; // Just the type, not the actual data

        private static readonly string[] prefabsToCopy = {
            "Animations.prefab",
            "Character_Mesh_3P_Override_0.prefab",
            "Item.prefab"
        };

        [MenuItem("Assets/Huey/Process Clothing", false, 10)]
        private static void ProcessClothing()
        {
            string exportLocation = AssetDatabase.LoadAssetAtPath<HueyColorSettings>("Assets/Huey/ColorSettings.asset").ExportLocation;

            if (Directory.Exists(exportLocation))
            {
                Directory.Delete(exportLocation, true);
            }

            ProcessClothingType("Pants", exportLocation);
            ProcessClothingType("Shirts", exportLocation);
        }

        [MenuItem("Assets/Huey/Create Color Variants", false, 10)]
        private static void ProcessTexture()
        {
            foreach (var obj in Selection.objects)
            {
                string texturePath = AssetDatabase.GetAssetPath(obj);
                if (Path.GetExtension(texturePath) == ".png")
                {
                    ProcessColorVariations(texturePath);
                }
            }
        }

        private static void ProcessClothingType(string clothingType, string exportLocation)
        {
            string inputDir = $"Assets/Huey/Input/{clothingType}";
            string outputDir = $"{exportLocation}/{clothingType}";

            if (!Directory.Exists(inputDir))
            {
                Debug.LogError($"Input directory does not exist: {inputDir}");
                return;
            }

            foreach (var itemDir in Directory.GetDirectories(inputDir))
            {
                string itemName = Path.GetFileName(itemDir);
                string[] baseTextures = Directory.GetFiles(itemDir, "*.png");
                string overlayPath = Path.Combine(itemDir, "Overlay.png");

                foreach (var baseTexturePath in baseTextures)
                {
                    if (Path.GetFileName(baseTexturePath) == "Overlay.png") continue;
                    BuildClothing(itemDir, baseTexturePath, overlayPath, outputDir, clothingType == "Shirts");
                }
            }
        }

        private static void BuildClothing(string itemDir, string baseTexturePath, string overlayPath, string outputDir, bool isShirt)
        {
            var colorSettings = AssetDatabase.LoadAssetAtPath<HueyColorSettings>("Assets/Huey/ColorSettings.asset");

            if (colorSettings == null)
            {
                Debug.LogError("Color settings asset not found!");
                return;
            }

            using (var baseImg = new Bitmap(baseTexturePath))
            {
                Bitmap overlayImg = File.Exists(overlayPath) ? new Bitmap(overlayPath) : null;
                string baseItemName = Path.GetFileNameWithoutExtension(baseTexturePath);
                string itemOutputDir = Path.Combine(outputDir, baseItemName);
                Directory.CreateDirectory(itemOutputDir);

                foreach (var color in colorSettings.colors)
                {
                    using (var adjustedImg = AdjustHueSaturationLightness(baseImg, color.hueShift, color.saturationScale, color.lightnessPercent))
                    {
                        Bitmap finalImg = overlayImg != null ? ApplyOverlay(adjustedImg, overlayImg) : adjustedImg;
                        string colorOutputDir = Path.Combine(itemOutputDir, $"{baseItemName}_{color.name}");
                        Directory.CreateDirectory(colorOutputDir);

                        string outputFileName = isShirt ? "Shirt.png" : "Pants.png";
                        string outputPath = Path.Combine(colorOutputDir, outputFileName);

                        finalImg.Save(outputPath, ImageFormat.Png);
                        finalImg.Save(outputPath, ImageFormat.Png);
                        AssetDatabase.Refresh();

                        finalImg.Save(outputPath, ImageFormat.Png);
                        AssetDatabase.Refresh();

                        SetTextureImportSettings(outputPath);
                        CreateMaterial(itemDir, colorOutputDir, outputFileName);
                        CopyPrefabs(itemDir, colorOutputDir, outputFileName);
                    }
                }
                overlayImg?.Dispose();
            }
            AssetDatabase.Refresh();
        }
        private static Bitmap ApplyOverlay(Bitmap baseImg, Bitmap overlayImg)
        {
            Bitmap finalImg = new Bitmap(baseImg.Width, baseImg.Height);
            for (int y = 0; y < baseImg.Height; y++)
            {
                for (int x = 0; x < baseImg.Width; x++)
                {
                    var baseColor = baseImg.GetPixel(x, y);
                    var overlayColor = overlayImg.GetPixel(x, y);
                    var finalColor = overlayColor.A > 0 ? overlayColor : baseColor;
                    finalImg.SetPixel(x, y, finalColor);
                }
            }
            return finalImg;
        }

        private static void ProcessColorVariations(string texturePath)
        {
            // Load the texture
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning($"Texture could not be loaded: {texturePath}");
                return;
            }

            // Get the texture pixels
            UnityEngine.Color[] pixels = texture.GetPixels();

            // Create a new texture for each color variation
            foreach (var (colorName, hueShift, saturationScale, LightnessPercent) in colors)
            {
                UnityEngine.Color[] newPixels = new UnityEngine.Color[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    UnityEngine.Color pixelColor = pixels[i];
                    if (pixelColor.a == 0) continue; // Skip fully transparent pixels

                    ColorToHLS(ConvertToDrawingColor(pixelColor), out float h, out float l, out float s);
                    h = (hueShift / 360f + h) % 1f;
                    s = saturationScale;
                    float LightnessAdjust = LightnessPercent / 100f;
                    l = Mathf.Clamp01(l * (1 + LightnessAdjust));

                    newPixels[i] = HLSColorToRGB(h, l, s);
                }

                // Create a new texture with the new pixels
                Texture2D newTexture = new Texture2D(texture.width, texture.height);
                newTexture.SetPixels(newPixels);
                newTexture.Apply();

                // Save the new texture as "pngname_colorname.png"
                string newTexturePath = texturePath.Replace(".png", $"_{colorName}.png");
                File.WriteAllBytes(newTexturePath, newTexture.EncodeToPNG());
                AssetDatabase.Refresh();

                // Set the texture import settings
                SetTextureImportSettings(newTexturePath);
            }
        }

        // Add this helper function to convert between System.Drawing.Color and UnityEngine.Color
        private static UnityEngine.Color ConvertToUnityColor(System.Drawing.Color drawingColor)
        {
            return new UnityEngine.Color(drawingColor.R / 255f, drawingColor.G / 255f, drawingColor.B / 255f, drawingColor.A / 255f);
        }

        private static System.Drawing.Color ConvertToDrawingColor(UnityEngine.Color unityColor)
        {
            return System.Drawing.Color.FromArgb(
                Mathf.Clamp((int)(unityColor.a * 255), 0, 255),
                Mathf.Clamp((int)(unityColor.r * 255), 0, 255),
                Mathf.Clamp((int)(unityColor.g * 255), 0, 255),
                Mathf.Clamp((int)(unityColor.b * 255), 0, 255));
        }


        private static Bitmap AdjustHueSaturationLightness(Bitmap image, float hueShift, float saturationScale, float lightnessPercent)
        {
            Bitmap adjustedImg = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(x, y);
                    if (pixelColor.A == 0) continue;

                    float h, s, l;
                    ColorToHLS(pixelColor, out h, out l, out s);
                    h = (hueShift / 360f + h) % 1f;
                    s = saturationScale;
                    l = Mathf.Clamp01(l * (1 + lightnessPercent / 100f));

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

        private static void CreateMaterial(string itemDir, string folderPath, string textureFileName)
        {
            string materialName = Path.GetFileNameWithoutExtension(textureFileName) + ".mat";
            string materialPath = Path.Combine(folderPath, materialName);

            Material material = new Material(Shader.Find("Standard"));
            string texturePath = Path.Combine(folderPath, textureFileName);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            material.mainTexture = texture;

            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_Glossiness", 0f);

            material.SetFloat("_Mode", 1); // 0 = Opaque, 1 = Cutout, 2 = Fade, 3 = Transparent
            material.SetInt("_RenderQueue", 2450);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHABLEND");
            material.EnableKeyword("_ALPHATEST");
            material.DisableKeyword("_TRANSPARENT");

            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
        }

        private static void CopyPrefabs(string sourceFolder, string destinationFolder, string outputFileName)
        {
            foreach (var prefab in prefabsToCopy)
            {
                string sourcePath = Path.Combine(sourceFolder, prefab);
                string destinationPath = Path.Combine(destinationFolder, prefab);

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, true);

                    if (prefab == "Item.prefab")
                    {
                        AssetDatabase.Refresh();
                        EditorApplication.delayCall += () =>
                            ApplyMaterialToPrefab(destinationPath, Path.Combine(destinationFolder, Path.GetFileNameWithoutExtension(outputFileName) + ".mat"));
                    }
                }
            }
        }

        private static void ApplyMaterialToPrefab(string prefabPath, string materialPath)
        {
            AssetDatabase.Refresh();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab could not be loaded: {prefabPath}");
                return;
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Debug.LogWarning($"Material could not be loaded: {materialPath}");
                return;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"No Renderers found in prefab: {prefabPath}");
                return;
            }

            foreach (var renderer in renderers)
            {
                Material[] materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;  // Assign the new material
                }
                renderer.sharedMaterials = materials;  // Update the renderer materials
            }

            // Save the prefab with the updated material
            PrefabUtility.SavePrefabAsset(prefab);
        }

        private static void SetTextureImportSettings(string texturePath)
        {
            if (AssetImporter.GetAtPath(texturePath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.isReadable = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}
