using System.Collections;
using System.Collections.Generic;
using UnityEngine; // ��Ҫ��Unity�����AudioSource���
using UnityEngine.UI;

public class PlayAudio : MonoBehaviour
{
    public AudioSource audioSource; // AudioSource���
    public AudioClip audioClip; // ��Ƶ��Դ

    public Slider slider;

    private void Update()
    {
        audioSource.volume = slider.value; // ��������
    }
    public void ClickBtnPlay()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void ClickBtnPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
}
