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
    [SerializeField] private GameObject nextObj;
    [SerializeField] private GameObject blockObj;
    [SerializeField] private GameObject blockBackObj;
    [SerializeField] private GameObject arrowObj;
    [SerializeField] private GameObject chainAnim;

    [SerializeField] private GameObject UICanvas;
    [SerializeField] private GameObject ObstacleCount;
    
    private List<GameObject> gaugeList = new ();
    private GameObject gauge;
    private GameObject chain;
    private GameObject obstacleCount;
    
    public int GetCount => gaugeList.Count;

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

    public void CreateGauge()
    {
        var obj = Instantiate(nextObj);
        obstacleCount = Instantiate(ObstacleCount, UICanvas.transform);
        obj.GetComponent<SpriteRenderer>().size = new Vector2(GameManager.Instance.boardWidth + 0.3f, 1.35f);
        obj.transform.position = new Vector3(GameManager.Instance.boardWidth / 2.0f - 0.5f, -1.7f, 0);
        for (int i = 0; i < GameManager.Instance.boardWidth; i++)
        {
            var back = Instantiate(blockBackObj, obj.transform);
            back.transform.localPosition = new Vector3(i - (GameManager.Instance.boardWidth / 2.0f - 0.5f), 0f, 0);
            //�@���I�u�W�F
            var arrow = Instantiate(arrowObj, obj.transform);
            arrow.transform.localPosition = new Vector3(i - (GameManager.Instance.boardWidth / 2.0f - 0.5f), 1.2f, 0);
        }
        gauge = obj;
    }

    public void ResetGauge()
    {
        foreach (var list in gaugeList)
        {
            Destroy(list);
        }
        gaugeList.Clear();
    }

    public async void StopCount()
    {
        chain = Instantiate(chainAnim);
        chain.transform.position = gauge.transform.position - Vector3.forward;
        await chain.GetComponent<ChainAnim>().onChain();
    }
    public async void ReCount()
    {
        await chain.GetComponent<ChainAnim>().offChain();
        Destroy(chain);
    }
    
    public void CountUp()
    {
        for (int i = 0; i < GameManager.Instance.NextUpCountAmount; i++)
        {
            if (gaugeList.Count < GameManager.Instance.boardWidth)
            {
                var obj = Instantiate(blockObj, gauge.transform);
                obj.transform.localPosition =
                    new Vector3(gaugeList.Count - (GameManager.Instance.boardWidth / 2.0f - 0.5f), 0f, 0);
                gaugeList.Add(obj);
            }
        }
    }
    
    public void Clear()
    {
        foreach (var list in gaugeList)
        {
            Destroy(list);
        }
        gaugeList.Clear();
        Destroy(gauge);
        Destroy(obstacleCount);
    }
}
