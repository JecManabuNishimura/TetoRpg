using DG.Tweening;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMethods;


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
        count += GameManager.player.BelongingsMinoEffect["NextGaugeUp"] * 2;
        count -= GameManager.player.BelongingsMinoEffect["NextGaugeDown"] * 2;
        gaugeParent.transform.ChildClear ();
        gaugeList.Clear ();
        for (int i = 0; i < count; i++)
        {
            gaugeList.Add(Instantiate(gaugeObj, gaugeParent.transform, true));
            var anims =  gaugeList[^1].GetComponents<DOTweenAnimation>();
            var openAnim = anims.First(id => id.id == "open");
        }
        nowIndex = count - 1 ;
        maxIndex = count;
    }

    public void ResetGauge()
    {
        foreach (var anim in gaugeList)
        {
            DOTween.Restart(anim,"open");
        }

        nowIndex = maxIndex - 1;
    }

    public void DownCount()
    {
        DOTween.Restart(gaugeList[nowIndex],"close");
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

    public void Clear()
    {
        gaugeParent.transform.ChildClear();
        gaugeList.Clear();
    }
}
