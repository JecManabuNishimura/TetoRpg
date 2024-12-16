using DG.Tweening;
using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyMethods;


public class NextUpGauge : MonoBehaviour
{
    public static NextUpGauge Instance;
    [SerializeField] private GameObject gaugeObj;
    [SerializeField] private GameObject gaugeParent;

    private List<GameObject> gaugeList = new ();
    private int nowIndex;
    private int maxIndex;
    public int GetCount => nowIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CreateGauge(int count)
    {

        gaugeParent.transform.ChildClear ();
        gaugeList.Clear ();
        for (int i = 0; i < count; i++)
        {
            gaugeList.Add(Instantiate(gaugeObj, gaugeParent.transform, true));
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

    public void ReCount(int count)
    {

        if(count != maxIndex)
        {
            if (count < maxIndex)
            {
                
                // ���Ȃ����폜
                for (int i = 0; i < maxIndex - count; i++)
                {
                    // 1より小さくはしない
                    if (gaugeList.Count == 1)
                    {
                        break;
                    }
                    Destroy(gaugeList[^1].gameObject);
                    gaugeList.RemoveAt(gaugeList.Count - 1);
                }

                nowIndex = count-1;
                maxIndex = count;
            }
            else
            {
                for (int i = maxIndex; i < count; i++)
                {
                    gaugeList.Add(Instantiate(gaugeObj, gaugeParent.transform, true));
                }
                maxIndex = count;
            }
        }
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
