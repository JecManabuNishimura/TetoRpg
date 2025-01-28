using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StageLoader : MonoBehaviour
{
    [SerializeField] private List<StageStatus> status;
    
    public int NextCount => status.First(_ => _.stage == GameManager.Instance.nowStage).nextUpGauge;
    public TreasureDropMaster GetDropData()
    {
        var data = status.First(_ => _.stage == GameManager.Instance.nowStage);
        return data.treasureDropMaster;
    }
    private void Awake()
    {
        GameManager.Instance.stageLoader = this;
        GameManager.Instance.StageStart(Stage.Stage1);
    }

    public void SetStageStatus()
    {
        var data = status.First(_ => _.stage == GameManager.Instance.nowStage);
        GameManager.Instance.stageData = data.parameter;
    }

    //public Vector3 EnemyPos => enemyPos.position;
}

[Serializable]
public class StageStatus
{
    public Stage stage;
    public TreasureDropMaster treasureDropMaster;
    public StageData parameter;
    public int nextUpGauge;
}
[Serializable]
public enum Stage
{
    None,
    Stage1,
    Stage2,
    Stage3,
    Stage4,
    Stage5,
    Stage6,
    Stage7,
    Stage8,
    Stage9,
}


