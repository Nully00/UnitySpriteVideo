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
                //����maxsize�̐ݒ�͍���Ă܂���B�Ȃ̂ŁA2048��菬�����摜�̂ݑΉ��\
                textureImporter.maxTextureSize = 2048;
                textureImporter.textureFormat = TextureImporterFormat.Automatic;
                textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

                AssetDatabase.ImportAsset(assetPath);
            }
#endif
        }
    }
}