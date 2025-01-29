using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Transition : MonoBehaviour
{
    public void StartEnd()
    {
        GetComponent<PlayableDirector>().Pause();
    }

    public void Restart()
    {
        GetComponent<PlayableDirector>().Play();
    }
}
