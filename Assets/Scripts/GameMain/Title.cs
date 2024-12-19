using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{

    void Update()
    {
        if (Input.anyKeyDown)
        {
            DOTween.Restart(gameObject);
        }
    }

    public void ChangeScene()
    {
        SceneManager.LoadScene("StageSelect");
    }
}
