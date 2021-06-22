using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TextureResizer.Editor
{
    public class TextureResizer
    {
        public const int minSize = 120;
        
        [MenuItem("Assets/Resizer/UpscaleToX4")]
        private static void UpscaleToX4()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D tex)
                {
                    if (tex.width < minSize || tex.height < minSize)
                    {
                        Debug.Log($"[Texture Resizer] {obj.name} is too small. Skip");
                        continue;
                    }
                    var newWidth = Mathf.CeilToInt(tex.width/4f) * 4;
                    var newHeight = Mathf.CeilToInt(tex.height/4f) * 4;
                    Resize(tex, newWidth, newHeight);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        
        [MenuItem("Assets/Resizer/UpscaleToX4", true)]
        private static bool UpscaleToX4Validation()
        {
            return Selection.objects.All(x=> x is Texture2D);
        }

        
        [MenuItem("Assets/Resizer/DownscaleToX4")]
        private static void DownscaleToX4()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D tex)
                {
                    if (tex.width < minSize || tex.height < minSize)
                    {
                        Debug.Log($"Texture Resizer {obj.name} is too small. Skip");
                        continue;
                    }
                    var newWidth = Mathf.FloorToInt(tex.width/4f) * 4;
                    var newHeight = Mathf.FloorToInt(tex.height/4f) * 4;
                    Resize(tex, newWidth, newHeight);
                    AssetDatabase.Refresh();
                }
            }
        }
        
        [MenuItem("Assets/Resizer/DownscaleToX4", true)]
        private static bool DownscaleToX4Validation()
        {
            return Selection.objects.All(x=> x is Texture2D);
        }

        
        public static void Resize(Texture2D texture2D, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            try
            {
                texture2D.Resize(targetWidth, targetHeight, texture2D.format, texture2D.mipmapCount > 0);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Texture Resizer] {e.Message}");
                RenderTexture.ReleaseTemporary(rt);
                return;
            }

            try
            {
                texture2D.ReadPixels(new Rect(0.0f, 0.0f, targetWidth, targetHeight), 0, 0);
                texture2D.Apply();
                var applicationDataPath = Application.dataPath.Replace("Assets", "");
                var assetPath = AssetDatabase.GetAssetPath(texture2D);
                
                byte[] _bytes = null;
                if (assetPath.EndsWith(".png"))
                    _bytes = texture2D.EncodeToPNG();
                if (assetPath.EndsWith(".jpg") || assetPath.EndsWith(".jpeg"))
                    _bytes = texture2D.EncodeToJPG();
                if (_bytes != null)
                    System.IO.File.WriteAllBytes(applicationDataPath + assetPath, _bytes);
                
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
            catch
            {
                Debug.LogError("[Texture Resizer] Please enable Read/Write on "+ texture2D.name);
            }
            
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}
