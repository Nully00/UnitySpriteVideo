using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Nulock.UnitySpriteVideo
{
    public static class UnitySpriteVideoEncoderCore
    {
        /// <summary>
        /// チャンクサイズに分割します。
        /// </summary>
        public static ChunkData[] SplitTexture(Texture2D texture, int chunkSize, int imageId)
        {
            int chunkWidth = texture.width / chunkSize;
            int chunkHeight = texture.height / chunkSize;
            ChunkData[] chunks = new ChunkData[chunkSize * chunkSize];

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Texture2D chunk = new Texture2D(chunkWidth, chunkHeight);
                    chunk.SetPixels(
                        texture.GetPixels(x * chunkWidth, (chunkSize - y - 1) * chunkHeight, chunkWidth, chunkHeight)
                        );
                    chunk.Apply();
                    chunks[y * chunkSize + x] = new ChunkData(chunk, imageId, y * chunkSize + x);
                }
            }
            return chunks;
        }
        /// <summary>
        /// 部分的にでも違う部分があればfalseを返します。(epsilonは誤差の許容値です)
        /// (動画の動きを正確に検知するためです。)
        /// </summary>
        public static bool AreTexturesEqual(Texture2D texA, Texture2D texB, float epsilon = 0)
        {
            Color[] pixelsA = texA.GetPixels();
            Color[] pixelsB = texB.GetPixels();

            if (pixelsA.Length != pixelsB.Length)
                return false;

            for (int i = 0; i < pixelsA.Length; i++)
            {
                if (!AreColorsEqual(pixelsA[i], pixelsB[i], epsilon))
                    return false;
            }
            return true;
        }
        private static bool AreColorsEqual(Color colA, Color colB, float epsilon)
        {
            return Mathf.Abs(colA.r - colB.r) <= epsilon &&
                   Mathf.Abs(colA.g - colB.g) <= epsilon &&
                   Mathf.Abs(colA.b - colB.b) <= epsilon &&
                   Mathf.Abs(colA.a - colB.a) <= epsilon;
        }
        /// <summary>
        /// 異なるピクセルの割合を返します。
        /// </summary>
        public static float CompareTextures(Texture2D tex1, Texture2D tex2)
        {
            if (tex1.width != tex2.width || tex1.height != tex2.height)
            {
                Debug.LogError("Textures should be of the same size");
                return 1.0f;
            }

            float diffCount = 0;
            Color[] pixelsA = tex1.GetPixels();
            Color[] pixelsB = tex2.GetPixels();

            for (int i = 0; i < pixelsA.Length; i++)
            {
                if (pixelsA[i] != pixelsB[i])
                    diffCount++;
            }

            return diffCount / pixelsA.Length;
        }
        /// <summary>
        /// フォルダからテクスチャを読み込みます。
        /// </summary>
        public static Texture2D LoadTexturesFromFolder(string path)
        {
            Texture2D texture = new Texture2D(0, 0);
            byte[] fileData = File.ReadAllBytes(path);
            texture.LoadImage(fileData);
            return texture;
        }

        public readonly struct ChunkData
        {
            public readonly Texture2D Texture;
            public readonly int ImageId;
            public readonly int ChunkId;
            public ChunkData(Texture2D texture, int imageId, int chunkId)
            {
                Texture = texture;
                ImageId = imageId;
                ChunkId = chunkId;
            }
        }
        [System.Serializable]
        public struct UnitySpriteVideoInfo
        {
            public ChunkUpdateData[] updateDatas { get; private set; }
            public int chunkSize;
            public int totalFrame;

            //保存時SOA配列
            [SerializeField]
            private int[] _dataAFrameIds;
            [SerializeField]
            private int[] _dataBChunkIds;
            [SerializeField]
            private int[] _dataCStorageIds;
            public UnitySpriteVideoInfo(List<ChunkUpdateData> referenceImageIds, int chunkSize, int totalFrame)
            {
                updateDatas = referenceImageIds.ToArray();
                this.chunkSize = chunkSize;
                this.totalFrame = totalFrame;

                _dataAFrameIds = referenceImageIds.Select(x => x.frameId).ToArray();
                _dataBChunkIds = referenceImageIds.Select(x => x.chunkId).ToArray();
                _dataCStorageIds = referenceImageIds.Select(x => x.storageId).ToArray();
            }
            public void Init()
            {
                updateDatas = new ChunkUpdateData[_dataAFrameIds.Length];
                for (int i = 0; i < _dataAFrameIds.Length; i++)
                {
                    updateDatas[i] = new ChunkUpdateData(_dataAFrameIds[i], _dataBChunkIds[i], _dataCStorageIds[i]);
                }
            }
        }
        [System.Serializable]
        public struct ChunkUpdateData
        {
            public int frameId;//表示するタイミング
            public int chunkId;//表示するチャンク
            public int storageId;//どこに格納されているか

            public ChunkUpdateData(int frameId, int chunkId, int storageId)
            {
                this.frameId = frameId;
                this.chunkId = chunkId;
                this.storageId = storageId;
            }
        }
    }
}