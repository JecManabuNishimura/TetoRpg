using DG.Tweening;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;


public class NextUpGauge : MonoBehaviour
{
    [SerializeField] private GameObject gaugeObj;
    [SerializeField] private GameObject gaugeParent;

    private List<GameObject> gaugeList = new ();
    private int nowIndex;
    private int maxIndex;
    public int GetCount => nowIndex;

    public void CreateGauge(int count)
    {
        for (int i = 0; i < count; i++)
        {
            gaugeList.Add(Instantiate(gaugeObj, gaugeParent.transform, true));
            var anims =  gaugeList[^1].GetComponents<DOTweenAnimation>();
            var openAnim = anims.First(id => id.id == "open");
        }
        nowIndex = count - 1 ;
        maxIndex = count;
    }

    public async void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Play();

        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            
        }
    }

    public void ResetGauge()
    {
        foreach (var anim in gaugeList)
        {
            DOTween.Restart(anim,"open");
        }

        nowIndex = maxIndex;
    }

    public void DownCount()
    {
        DOTween.Play(gaugeList[nowIndex],"close");
        nowIndex--;
    }

    public async Task Play()
    {
        foreach (var anim in gaugeList)
        {
            DOTween.Restart(anim,"open");
            await Task.Delay(100);
        }
    }
}
