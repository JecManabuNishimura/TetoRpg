using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Transition : MonoBehaviour
{
    public TimelineAsset startTran;
    public TimelineAsset endTran;
    
    public void StartTran()
    {
        GetComponent<PlayableDirector>().playableAsset = startTran;
        GetComponent<PlayableDirector>().Play();
    }
    public void EndTran()
    {
        GetComponent<PlayableDirector>().playableAsset = endTran;
        GetComponent<PlayableDirector>().Play();
    }
}
