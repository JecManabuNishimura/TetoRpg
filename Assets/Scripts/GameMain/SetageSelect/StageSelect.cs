using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelect : MonoBehaviour
{
    [SerializeField] private AudioSource BGM;
    [SerializeField] private AudioSource SE;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundMaster.Entity.SetBGMAudio(BGM);
        SoundMaster.Entity.SetSEAudio(SE);
        SoundMaster.Entity.PlaySoundBGM(PlaceOfSound.StageSelect);
    }

    
    async void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SoundMaster.Entity.PlaySoundSE(PlaceOfSound.Select);
            await UniTask.WaitForSeconds(1);
            SceneManager.LoadScene("Stage1");
        }
    }

}
