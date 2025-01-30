using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    [SerializeField] private  AudioSource GMBaudio;
    [SerializeField] private  AudioSource SEaudio;

    private void Start()
    {
        SoundMaster.Entity.SetBGMAudio(GMBaudio);
        SoundMaster.Entity.SetSEAudio(SEaudio);
        SoundMaster.Entity.PlaySoundBGM(PlaceOfSound.Title);
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SoundMaster.Entity.PlaySoundSE(PlaceOfSound.Select);
            SoundMaster.Entity.FadeOutBGM(PlaceOfSound.Title);
            DOTween.Restart(gameObject);
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("StageSelect");
    }
}
