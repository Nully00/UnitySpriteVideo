using UnityEngine;
using static Nulock.UnitySpriteVideo.UnitySpriteVideoEncoderCore;

namespace Nulock.UnitySpriteVideo
{
    public static class UnitySpriteVideoToJson
    {
        public static void Save(UnitySpriteVideoInfo info, string path)
        {
            JsonConverter.Save(info, GetFilePath(path));
        }
        public static UnitySpriteVideoInfo Load(TextAsset textAsset)
        {
            var data = JsonConverter.Load<UnitySpriteVideoInfo>(textAsset);
            data.Init();
            return data;
        }

        private static string GetFilePath(string fileName)
        {
            if (!fileName.ToLower().EndsWith(".json"))
            {
                fileName += ".json";
            }
            return fileName;
        }
    }
}