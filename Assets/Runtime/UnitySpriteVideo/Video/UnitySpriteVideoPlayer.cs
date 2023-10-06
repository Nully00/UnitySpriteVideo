using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Nulock.UnitySpriteVideo.UnitySpriteVideoEncoderCore;

namespace Nulock.UnitySpriteVideo
{
    [System.Serializable]
    public class UnitySpriteVideoPlayer
    {
        [SerializeField] private string _folderPath = "UnitySpriteVideo/FolderName";
        [SerializeField] private TextAsset _videoInfoJson;
        [SerializeField] private Image _image;
        [SerializeField] private float _fps = 30f;

        private UnitySpriteVideoInfo _videoInfo;
        private Sprite[] _sprites;

        private Texture2D _currentTexture;
        private int _currentStorageId = 0;


        private bool _isPlaying = false;
        private bool _isPause = false;


        public void LoadVideo()
        {
            _videoInfo = UnitySpriteVideoToJson.Load(_videoInfoJson);
            _sprites = Resources.LoadAll<Sprite>(_folderPath);

            if (_sprites == null || _sprites.Length == 0)
            {
                Debug.LogError("Sprites array is null or empty");
                return;
            }
        }
        public void Play(MonoBehaviour mono)
        {
            if(_sprites == null)
            {
                Debug.LogError("SpriteVideo is not loading");
                return;
            }
            mono.StartCoroutine(PlayVideo());
        }
        public void Pause()
        {
            _isPause = true;
        }
        public void Restart()
        {
            _isPause = false;
        }
        public IEnumerator WaitForPlayComplete()
        {
            if (_sprites == null)
            {
                Debug.LogError("SpriteVideo is not loading");
                yield break;
            }
            yield return PlayVideo();
        }
        private IEnumerator PlayVideo()
        {
            if (_isPlaying)
            {
                Debug.LogError("video playing");
                yield break;
            }

            _isPlaying = true;

            int totalFrames = _videoInfo.totalFrame;
            float frameTime = 1f / _fps;

            _currentTexture = new Texture2D(_sprites[0].texture.width, _sprites[0].texture.height);


            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                UpdateFrameTexture(frameIndex);


                var sprite = Sprite.Create(_currentTexture, new Rect(0, 0, _currentTexture.width, _currentTexture.height), new Vector2(0.5f, 0.5f));
                _image.sprite = sprite;

                while (_isPause)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(frameTime);

                Object.Destroy(sprite);
                sprite = null;
            }

            _isPlaying = false;
        }

        private void UpdateFrameTexture(int frameIndex)
        {
            int chunkSize = _videoInfo.chunkSize;
            int chunkWidth = _currentTexture.width / chunkSize;
            int chunkHeight = _currentTexture.height / chunkSize;

            var chunkUpdateDatas = _videoInfo.updateDatas;

            while (_currentStorageId < chunkUpdateDatas.Length)
            {
                var chunkUpdateData = chunkUpdateDatas[_currentStorageId];
                if (chunkUpdateData.frameId != frameIndex)
                    break;

                int currentX = (chunkUpdateData.chunkId % chunkSize) * chunkWidth;
                int currentY = (chunkUpdateData.chunkId / chunkSize) * chunkHeight;
                int yPos = _currentTexture.height - currentY - chunkHeight;



                Color[] chunkTexture = GetChunkTexture(_currentStorageId, chunkSize, chunkWidth, chunkHeight);
                if (chunkTexture.Length != chunkWidth * chunkHeight)
                {
                    Debug.LogError($"Mismatch in expected chunk texture size. Expected: {chunkWidth * chunkHeight}, Got: {chunkTexture.Length}");
                }

                //いつかColor32版にする
                _currentTexture.SetPixels(currentX, yPos, chunkWidth, chunkHeight, chunkTexture);
                _currentStorageId++;
            }

            _currentTexture.Apply();
        }

        private Color[] GetChunkTexture(int storageId, int chunkSize, int chunkW, int chunkH)
        {
            int imageStrageIndex = storageId / (chunkSize * chunkSize);
            var targetTexture = _sprites[imageStrageIndex].texture;
            int targetTextureStrageId = storageId % (chunkSize * chunkSize);

            int currentX = (targetTextureStrageId % chunkSize) * chunkW;
            int currentY = (targetTextureStrageId / chunkSize) * chunkH;
            int yPos = _currentTexture.height - currentY - chunkH;


            return targetTexture.GetPixels(currentX, yPos, chunkW, chunkH);
        }
    }
}