using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rod.Editor
{
    /// <summary>仅约束新算筹美术目录，旧资源退役不改变公共字体或SDK资源。</summary>
    public sealed class RodUIImportPolicy : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/AssetRaw/UIRaw/Atlas/Rod/";
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot, System.StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            string name = Path.GetFileNameWithoutExtension(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = name.EndsWith("_bg") ? 2048 : 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            if (name.EndsWith("_9s"))
            {
                // 美术按四边各占1/4制作；例如128x128边距32，64x64边距16。
                importer.spriteBorder = new Vector4(width / 4, height / 4, width / 4, height / 4);
                if (width > 256 || height > 256) Debug.LogWarning($"[RodUI] 九宫底板建议不大于256：{assetPath}");
            }
            else importer.spriteBorder = Vector4.zero;
        }

        [MenuItem("六面算筹/UI/将选中Image设为九宫拉伸")]
        public static void SliceSelected()
        {
            foreach (var go in Selection.gameObjects)
            {
                var image = go.GetComponent<Image>();
                if (image == null || image.sprite == null || image.sprite.border == Vector4.zero)
                { Debug.LogWarning($"{go.name}需要先绑定带Border的Sprite", go); continue; }
                Undo.RecordObject(image, "九宫拉伸");
                image.type = Image.Type.Sliced;
                image.fillCenter = true;
                image.preserveAspect = false;
                EditorUtility.SetDirty(image);
                PrefabUtility.RecordPrefabInstancePropertyModifications(image);
            }
        }

        [MenuItem("六面算筹/UI/检查选中层级的九宫设置")]
        public static void CheckSelected()
        {
            int errors = 0, count = 0;
            foreach (var go in Selection.gameObjects)
                foreach (var image in go.GetComponentsInChildren<Image>(true))
                {
                    if (image.sprite == null) continue;
                    string path = AssetDatabase.GetAssetPath(image.sprite);
                    if (!path.StartsWith(ArtRoot, System.StringComparison.Ordinal)) continue;
                    count++;
                    if (image.sprite.border != Vector4.zero && image.type != Image.Type.Sliced)
                    { errors++; Debug.LogError($"[RodUI] {image.name}有Border但没有设置Sliced", image); }
                    if (image.type == Image.Type.Sliced && image.sprite.border == Vector4.zero)
                    { errors++; Debug.LogError($"[RodUI] {image.name}设置Sliced但缺少Border", image); }
                }
            Debug.Log($"[RodUI] 已检查{count}个新UI Image，发现{errors}项问题。文字、图标、插画不要九宫拉伸。");
        }
    }
}
