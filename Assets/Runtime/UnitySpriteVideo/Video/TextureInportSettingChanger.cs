#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Nulock.UnitySpriteVideo
{
    public static class TextureInportSettingChanger
    {
        public static void ChangeImportSettings(string assetPath)
        {
#if UNITY_EDITOR

            AssetDatabase.Refresh();

            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.alphaSource = TextureImporterAlphaSource.None;
                textureImporter.isReadable = true;
                textureImporter.mipmapEnabled = false;
                textureImporter.wrapMode = UnityEngine.TextureWrapMode.Clamp;
                textureImporter.filterMode = UnityEngine.FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                //現状maxsizeの設定は作ってません。なので、2048より小さい画像のみ対応可能
                textureImporter.maxTextureSize = 2048;
                textureImporter.textureFormat = TextureImporterFormat.Automatic;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

                AssetDatabase.ImportAsset(assetPath);
            }
#endif
        }
    }
}