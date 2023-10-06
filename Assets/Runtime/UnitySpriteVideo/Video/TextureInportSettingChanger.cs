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
                //Œ»ómaxsize‚Ìİ’è‚Íì‚Á‚Ä‚Ü‚¹‚ñB‚È‚Ì‚ÅA2048‚æ‚è¬‚³‚¢‰æ‘œ‚Ì‚İ‘Î‰‰Â”\
                textureImporter.maxTextureSize = 2048;
                textureImporter.textureFormat = TextureImporterFormat.Automatic;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

                AssetDatabase.ImportAsset(assetPath);
            }
#endif
        }
    }
}