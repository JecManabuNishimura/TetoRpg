using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "SoundMaster", menuName="Master/CreateSoundMaster") ]
public class SoundMaster : ScriptableObject
{
    private static SoundMaster _entity;

    public static SoundMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<SoundMaster>("Master/SoundMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(SoundMaster) + " not found");
                }
            }

            return _entity;
        }
    }
    
    public List<SoundData> SoundData = new();
    private AudioSource _seAudioSource;
    private AudioSource _bgmAudioSource;

    public void SetSEAudio(AudioSource source)
    {
        _seAudioSource = source;
    }
    public void SetBGMAudio(AudioSource source)
    {
        _bgmAudioSource = source;
    }

    public async void FadeOutBGM(PlaceOfSound place)
    {
        while (true)
        {
            _bgmAudioSource.volume -= 0.005f;
            if (_bgmAudioSource.volume <= 0)
            {
                break;
            }

            await UniTask.Yield();
        }
    }
    

    public void PlaySoundSE(PlaceOfSound place)
    {
        var clip = SoundData
            .FirstOrDefault(soundData => soundData.place == place);
        if (clip != null)
        {
            _seAudioSource.clip = clip.clip;
            _seAudioSource.volume = clip.volume;
            _seAudioSource.Play();
        }
    }
    public void PlaySoundBGM(PlaceOfSound place)
    {
        var clip = SoundData
            .FirstOrDefault(soundData => soundData.place == place);
        if (clip != null)
        {
            _bgmAudioSource.clip = clip.clip;
            _bgmAudioSource.volume = clip.volume;
            _bgmAudioSource.Play();
        }
    }
}

[Serializable]
public class SoundData
{
    public PlaceOfSound place;
    public AudioClip clip;
    public float volume = 1;
}

public enum PlaceOfSound
{
    Title,
    Select,
    StageSelect,
    MinoMove,
    Rot,
    Stage1BGM,
    
}
