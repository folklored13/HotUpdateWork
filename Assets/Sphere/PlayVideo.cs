using UnityEngine;
using UnityEngine.UI; // ��Ҫ��Unity�����Video Player ���
using UnityEngine.Video;

public class PlayVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public void ClickBtnPlay()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play(); // ������Ƶ
        }
    }

    public void ClickBtnPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();  // ��ͣ��Ƶ
        }
    }
}
