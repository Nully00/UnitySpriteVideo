using System.IO;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Nulock.UnitySpriteVideo
{
    public static class JsonConverter
    {
        public static void Save<T>(T obj, string path)
        {
            string json = JsonUtility.ToJson(obj);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        public static T Load<T>(TextAsset textAsset)
        {
            if (textAsset != null)
            {
                return JsonUtility.FromJson<T>(textAsset.text);
            }
            throw new ArgumentException("TextAsset is not assigned.");
        }
        public static T Load<T>(string path)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                T data = JsonUtility.FromJson<T>(json);

                return data;
            }

            throw new FileNotFoundException("File not found: " + path);
        }
    }
}