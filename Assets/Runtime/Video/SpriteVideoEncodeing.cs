using System.Threading.Tasks;
using UnityEngine;
namespace Nulock.UnitySpriteVideo
{
    public class SpriteVideoEncodeing : MonoBehaviour
    {
        [SerializeField]
        private string _path;
        [SerializeField]
        private string _videoName = "VideoName";
        [SerializeField]
        private int _chunkSize = 50;
        async void Start()
        {
            UnitySpriteVideoEncorder unitySpriteVideo = new UnitySpriteVideoEncorder(_path, _videoName, _chunkSize);
            var info = await unitySpriteVideo.Encode();
            UnitySpriteVideoToJson.Save(info, unitySpriteVideo.SavePath + "/info");
        }
    }
}