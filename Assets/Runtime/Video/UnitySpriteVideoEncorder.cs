using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static Nulock.UnitySpriteVideo.UnitySpriteVideoEncoderCore;

namespace Nulock.UnitySpriteVideo
{
    public class UnitySpriteVideoEncorder
    {
        //private Sprite[] _sprites { get; }
        private string[] _pngFilePaths;
        private int _chunkSize { get; }
        private string _videoName { get; }

        public string SavePath => $"Assets/Resources/UnitySpriteVideo/{_videoName}";
        public int TotalFrame => _pngFilePaths.Length;
        public int width { get; private set; }
        public int height { get; private set; }

        public UnitySpriteVideoEncorder(string inputPngFolderPath, string videoName, int chunkSize)
        {
            _pngFilePaths = Directory.GetFiles(inputPngFolderPath, "*.png");
            SortFileName(_pngFilePaths);
            Debug.Log(string.Join(", ", _pngFilePaths));

            if (_pngFilePaths == null || _pngFilePaths.Length == 0)
                throw new ArgumentException("image is null or empty");

            _videoName = videoName;
            SetImageSize();

            _chunkSize = CheckChunkSize(chunkSize);
        }

        private void SetImageSize()
        {
            var loadTexture = LoadTexturesFromFolder(_pngFilePaths[0]);
            width = loadTexture.width;
            height = loadTexture.height;
            ReleaseTextureData(loadTexture);
        }
        private int CheckChunkSize(int chunkSize)
        {
            while (width % chunkSize != 0 || height % chunkSize != 0)
            {
                chunkSize--;
            }
            return chunkSize;
        }

        public async Task<UnitySpriteVideoInfo> Encode()
        {
            ClearPath(_videoName);

            var splitedFirstImage = LoadAndReleaseSplitTexture(0);
            var referenceSplitedImage = new ChunkData[splitedFirstImage.Length];
            var referenceImageIds = new List<ChunkUpdateData>();


            Texture2D aggregatedTexture = InitAggregatedTexture();

            long totalFileSize = 0;
            int currentX = 0;
            int currentY = 0;
            int saveCount = 0;
            int updateCount = 0;

            for (int i = 0; i < referenceSplitedImage.Length; i++)
            {
                UpdateReferenceImage(splitedFirstImage[i], i, 0);
            }

            for (int i = 1; i < TotalFrame; i++)
            {
                var splitedImage = LoadAndReleaseSplitTexture(i);

                ProcessSplitedImage(splitedImage, i);

                System.GC.Collect();
                Debug.Log($"Progress : {i + 1} / {TotalFrame}");
                await Task.Delay(1);
            }

            FinalizeSavingAggregatedTexture(aggregatedTexture);


            Debug.Log($"File count: {CountFilesInFolder(SavePath) / 2}");
            Debug.Log($"Chunk size: {_chunkSize}");
            double fileSizeInKilobytes = totalFileSize / 1024.0;
            double fileSizeInMegabytes = fileSizeInKilobytes / 1024.0;
            Debug.Log($"File size: {totalFileSize} bytes, {fileSizeInKilobytes:F2} KB, {fileSizeInMegabytes:F2} MB");

            return new UnitySpriteVideoInfo(referenceImageIds, _chunkSize, TotalFrame);
            //-----------------------------------------------------------------

            ChunkData[] LoadAndReleaseSplitTexture(int index)
            {
                var loadTexture = LoadTexturesFromFolder(_pngFilePaths[index]);
                width = loadTexture.width;
                height = loadTexture.height;

                var splitedFirstImage = SplitTexture(loadTexture, _chunkSize, 0);
                ReleaseTextureData(loadTexture);
                _pngFilePaths[index] = null;

                return splitedFirstImage;
            }

            Texture2D InitAggregatedTexture()
            {
                Texture2D aggregatedTexture;
                aggregatedTexture = new Texture2D(width, height);
                return aggregatedTexture;
            }

            void ProcessSplitedImage(ChunkData[] splitedImages, int frameId)
            {
                for (int j = 0; j < splitedImages.Length; j++)
                {
                    if (ShouldUpdateReferenceImage(splitedImages[j], referenceSplitedImage[j]))
                    {
                        UpdateReferenceImage(splitedImages[j], j, frameId);
                    }
                    else
                    {
                        ReleaseTextureData(splitedImages[j].Texture);
                    }
                }
            }

            bool ShouldUpdateReferenceImage(ChunkData newImage, ChunkData referenceImage)
            {
                return
                    //部分的に結構違う
                    !AreTexturesEqual(newImage.Texture, referenceImage.Texture, 0.05f)
                    //全体的にうっすら違う
                    || CompareTextures(newImage.Texture, referenceImage.Texture) > 0.3;
            }

            void UpdateReferenceImage(ChunkData newImage, int chunkId, int frameId)
            {
                SaveAggregatedTexture(newImage);
                ReleaseTextureData(referenceSplitedImage[chunkId].Texture);
                referenceSplitedImage[chunkId] = newImage;
                referenceImageIds.Add(new ChunkUpdateData(frameId, chunkId, updateCount++));
            }

            void SaveAggregatedTexture(ChunkData chunkData)
            {
                Texture2D chunkTexture = chunkData.Texture;
                int yPos = aggregatedTexture.height - currentY - chunkTexture.height;
                aggregatedTexture.SetPixels(currentX, yPos,
                    chunkTexture.width, chunkTexture.height, chunkTexture.GetPixels());

                currentX += chunkTexture.width;
                if (currentX >= aggregatedTexture.width)
                {
                    currentX = 0;
                    currentY += chunkTexture.height;
                }

                if (currentY >= aggregatedTexture.height)
                {
                    FinalizeSavingAggregatedTexture(aggregatedTexture);
                    currentX = 0;
                    currentY = 0;
                    aggregatedTexture = InitAggregatedTexture();
                }
            }
            void FinalizeSavingAggregatedTexture(Texture2D newImage)
            {
                string savePath = GetSavePath(saveCount++, "png");

                var pngData = ToPng(newImage);
                //var pngData = ToJpg(newImage);
                Save(pngData, savePath);
                totalFileSize += GetFileSize(savePath);
            }
        }

        private void ClearPath(string videoName)
        {
            string directoryPath = SavePath;

            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException e)
                    {
                        Debug.LogError($"Failed to delete file: {file}. Error: {e.Message}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Directory does not exist: {directoryPath}");
            }
        }
        private void Save(byte[] pngData, string savePath)
        {
            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllBytes(savePath, pngData);
            TextureInportSettingChanger.ChangeImportSettings(savePath);
        }
        private string GetSavePath(int id, string last = "png")
        {
            return $"{SavePath}/{id}.{last}";
        }
        private byte[] ToPng(Texture2D chunkData)
        {
            return chunkData.EncodeToPNG();
        }
        private byte[] ToJpg(Texture2D chunkData)
        {
            return chunkData.EncodeToJPG(100);
        }
        private long GetFileSize(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }
        private int CountFilesInFolder(string path)
        {
            try
            {
                string[] files = Directory.GetFiles(path);
                return files.Length;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Cannot count files in {path}. Error: {e.Message}");
                return 0;
            }
        }

        private void ReleaseTextureData(Texture2D[] chunkDatas)
        {
            for (int i = 0; i < chunkDatas.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(chunkDatas[i]);
            }
            System.GC.Collect();
        }
        private void ReleaseTextureData(Texture2D chunkDatas)
        {
            UnityEngine.Object.DestroyImmediate(chunkDatas);
        }

        private void SortFileName(string[] names)
        {
            Array.Sort(names, (x, y) =>
            {
                string fileNameX = Path.GetFileNameWithoutExtension(x);
                string fileNameY = Path.GetFileNameWithoutExtension(y);

                int result = fileNameX.Length.CompareTo(fileNameY.Length);
                if (result != 0) return result;

                int numberX = int.Parse(fileNameX.Substring(fileNameX.IndexOf("_") + 1));
                int numberY = int.Parse(fileNameY.Substring(fileNameY.IndexOf("_") + 1));

                return numberX.CompareTo(numberY);
            });
        }
    }
}