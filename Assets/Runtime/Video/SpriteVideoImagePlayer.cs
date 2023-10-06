using Nulock.UnitySpriteVideo;
using System.Collections;
using UnityEngine;

public class SpriteVideoImagePlayer : MonoBehaviour
{
    [SerializeField]
    private UnitySpriteVideoPlayer _videoPlayer;

    private IEnumerator Start()
    {
        _videoPlayer.LoadVideo();
        yield return _videoPlayer.WaitForPlayComplete();
    }
}
