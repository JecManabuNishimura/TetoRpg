using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class Charactor : CharactorData,ICharactor
    {
        [SerializeField] private GameObject attackArea;
        [SerializeField] private int AttackInterval = 3;
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private float strength = 30.0f;
        [SerializeField] private int vibrato = 30;
        [SerializeField] private bool shakeFlag = false;
        [SerializeField] private AttackOrder[] attackOrders;
        [SerializeField] private AttackSetting[] attackSettings;
        [SerializeField] private SpecialAttackSetting[] spAttackSettings;
        [SerializeField] private int attackTarnCount;
        [SerializeField] private GameObject attackTarn;
        [SerializeField] private Transform attackTarnSpoonPoint;

        [SerializeField] private BombAttack bombAttack;
        
        private List<ParticleSystem> parts = new List<ParticleSystem>();
        private List<GameObject> area = new ();
        private GameObject attackObjPare;
        private float timer = 0;
        private float attackTime;
        private bool timerFlag = false;
        private Tweener shakeTweener;
        private Vector3 initPosition;
        private int attackCount;
        private GameObject countObj;
        private int BombAttackPosX;

        private int attackPattern;
        private int orderNumber = 0;

        private bool SpecialAttackFlag = false;
        
        private void Awake()
        {
            attackObjPare = new GameObject
            {
                name = "AttackArea"
            };
            GameManager.EnemyAttack += Play;
            initPosition = transform.position;

            GameManager.enemy = this;
        }

        private void Start()
        {
            status.hp = status.maxHp;
            attackCount = attackTarnCount;
            
        }

        private void ReadyAttack()
        {
            attackTime += Time.deltaTime;
            if((attackTime >= AttackInterval || GameManager.maxPutposFlag) && !GameManager.EnemyAttackFlag) 
            {
                foreach (var val in area)
                {
                    Destroy(val);
                }
                area.Clear();
                int pattern = 0;
                // 攻撃の種類
                switch (attackOrders[orderNumber])
                {
                    case AttackOrder.Normal:
                    {
                        pattern = SelectAttackPattern(false);
                        if (pattern == -1)
                        {
                            Debug.LogError("攻撃パターンが設定されていません");
                        }
                        attackPattern = pattern;
                        switch (attackSettings[pattern].name)
                        {
                            // ボムの場合は落下までエリアを出さない（危険表示は出す予定）
                            case AttackName.BombAttack:
                            case AttackName.BombMultiAttack:
                                GameManager.EnemyAttackFlag = true;
                                return;
                        }

                        var data = AttackData.GetAttack(attackSettings[pattern].name);
                        foreach (var d in data)
                        {
                            GameObject aObj = Instantiate(attackArea, new Vector3(d.x, d.y, 0), Quaternion.identity);
                            area.Add(aObj);
                            aObj.transform.parent = attackObjPare.transform;
                        }

                        break;
                    }
                    case AttackOrder.Special:
                    {
                        pattern = SelectAttackPattern(true);
                        if (pattern == -1)
                        {
                            Debug.LogError("攻撃パターンが設定されていません");
                        }
                        attackPattern = pattern;
                        var data = AttackData.GetSpecialAttack(spAttackSettings[pattern].name);
                        foreach (var d in data)
                        {
                            GameObject aObj = Instantiate(attackArea, new Vector3(d.x, d.y, 0), Quaternion.identity);
                            area.Add(aObj);
                            aObj.transform.parent = attackObjPare.transform;
                        }
                        break;
                    }
                }
                Debug.Log("抽選");
                GameManager.EnemyAttackFlag = true;
            }
        }

        public void CheckputPos()
        {
            //　一番上まで積まれたら攻撃開始
            if (GameManager.maxPutposFlag)
            {
                //GameManager.EnemyAttackFlag = true;
                attackCount = 0;
            }
        }

        int SelectAttackPattern(bool isSpecial)
        {
            int totalWeight;
            // 対象リストを選択
            var targetSettings = isSpecial ? 
                spAttackSettings.Cast<IWeighted>().ToList()
                : attackSettings.Cast<IWeighted>().ToList();

            // 総重みを計算
            totalWeight = targetSettings.Sum(data => data.weight);

            // ランダム値を生成（1からtotalWeightの間）
            int rand = Random.Range(1, totalWeight + 1);

            int count = 0;
            // 重みに基づいて選択
            foreach (var val in targetSettings)
            {
                if (rand <= val.weight)
                {
                    return count;
                }
                rand -= val.weight;
                count++;
            }

            // 正常なケースでは到達しない
            return -1;
        }
        

        private void StartShake(float duration, float strength, int vibrato, float randomness, bool fadeOut)
        {
            // 前回の処理が残っていれば停止して初期位置に戻す
            if (shakeTweener != null)
            {
                shakeTweener.Kill();
                gameObject.transform.position = initPosition;
            }
            // 揺れ開始
            shakeTweener = gameObject.transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut);
        }

        private void Update()
        {
            if (GameManager.menuFlag) return;
            
            if (shakeFlag)
            {
                StartShake(duration, strength, vibrato, 90, false);
                shakeFlag = false;
            }
            

            if(attackCount <= 0)
            {
                ReadyAttack();
                BoardManager.Instance.CheckMaxPutPos();
            }
        }

        public void CountDown()
        {
            if (attackCount == 0) return;
            var obj = Instantiate(attackTarn);
            obj.GetComponent<TextMeshPro>().text = attackCount.ToString();
            obj.transform.position = attackTarnSpoonPoint.position;
            attackCount--;
        }
        
        async UniTask Play()
        {
            Debug.Log("攻撃" );
            // 座標を保存するリスト
            var positions = new List<Vector3>();
            foreach (var obj in area)
            {
                if (obj != null) // nullチェック
                {
                    positions.Add(obj.transform.position);
                }
            }

            switch (attackOrders[orderNumber])
            {
                case AttackOrder.Normal:
                {
                    switch (attackSettings[attackPattern].name)
                    {
                        case AttackName.BombAttack:
                        {
                            await AttackBomb();
                            break;
                        }
                        case AttackName.BombMultiAttack:
                        {
                            for (int i = 0; i < Random.Range(2, 4); i++)
                            {
                                await AttackBomb();
                                var areaCopy = area.ToArray();  // エラー回避用
                                CreateAttackParticle(positions);

                                await Task.Delay(50);
                                var count = await StartAttack(positions) * status.atk;

                                // ダメージ処理
                                GameManager.player.Damage(count);
                                foreach (var val in positions)
                                {
                                    BoardManager.Instance.CheckDeleteLine((int)val.y);
                                }
                                foreach (var val in area)
                                {
                                    Destroy(val);
                                }
                                area.Clear();
                                parts.Clear();
                            }
                            break;
                        }
                        default:
                        {
                            CreateAttackParticle(positions);
                            Debug.Log("開始");
                            await Task.Delay(50);
                            var count = await StartAttack(positions) * status.atk;

                            // ダメージ処理
                            GameManager.player.Damage(count);
                            foreach (var val in positions)
                            {
                                BoardManager.Instance.CheckDeleteLine((int)val.y);
                            }
                            break;
                        }
                    }
                    break;
                }
                case AttackOrder.Special:
                {
                    CreateAttackParticle(positions);

                    await Task.Delay(50);
                    var count = (await StartAttack(positions) * status.atk) * 1.5f;

                    // ダメージ処理
                    GameManager.player.Damage((int)count);
                    foreach (var val in positions)
                    {
                        BoardManager.Instance.CheckDeleteLine((int)val.y);
                    }
                    break;
                }
            }
            
            Initialize();
        }

        private void Initialize()
        {
            foreach (var p in parts)
            {
                if (p != null)
                {
                    Destroy(p.gameObject);    
                }
            }
            parts.Clear();
            foreach (var val in area)
            {
                Destroy(val);
            }

            area.Clear();
            GameManager.EnemyAttackFlag = false;
            attackTime = 0;
            attackCount = attackTarnCount;
        }

        private async Task AttackBomb()
        {
            BombAttackPosX = Random.Range(0, GameManager.boardWidth);
            await bombAttack.CreateBomb(BombAttackPosX);
            Vector2Int hitPos = bombAttack.hitPos;
            for (int y = hitPos.y + 1; y >= hitPos.y - 1; y--)
            {
                for (int x = hitPos.x - 1; x <= hitPos.x + 1; x++)
                {
                    if (y > 0 && y < GameManager.boardHeight - 1)
                    {
                        if (x >= 0 && x < GameManager.boardWidth)
                        {
                            GameObject aObj = Instantiate(attackArea, new Vector3(x, y, 0),
                                Quaternion.identity);
                            area.Add(aObj);
                            aObj.transform.parent = attackObjPare.transform;
                        }
                    }
                }
            }
        }

        private async Task<int> StartAttack(List<Vector3> areaCopy)
        {
            int count = 0;
            foreach (var val in areaCopy)
            {
                if (val != null)
                {
                    var position = val;
                    if (!BoardManager.Instance.HitCheck((int)position.x, (int)position.y))
                    {
                        parts[count].GetComponent<Bullet>().StartMove();
                        await BoardManager.Instance.EnemyAttack((int)position.x, (int)position.y);
                        count++;
                    }
                }
            }

            return count;
        }

        private void CreateAttackParticle(List<Vector3> areaCopy)
        {
            foreach (var val in areaCopy)
            {
                if (val != null)
                {
                    var position = val;
                    if (!BoardManager.Instance.HitCheck((int)position.x, (int)position.y))
                    {
                        parts.Add(Instantiate(particle, new Vector3(position.x, position.y, 0), Quaternion.identity));
                        parts[^1].Play();
                        parts[^1].GetComponent<Bullet>().target = GameManager.player.gameObject;
                    }
                }
            }
        }

        public void UpdateHp()
        {
            throw new NotImplementedException();
        }

        public async Task Damage(int damage)
        {
            if(damage != 0)
            {
                int newDamage = damage / 2 - status.def / 4;
                if (newDamage < 0)
                {
                    newDamage = 0;
                }
                status.hp -= newDamage;
                
                
                if(status.hp <= 0)
                {
                    status.hp = 0;
                    await Task.Delay(500);
                    
                    gameObject.GetComponent<Animator>().enabled = false;
                    SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
                    await sr.DOFade(0, 1).AsyncWaitForCompletion();
                    GameManager.EnemyDown = true;
                }
            }
        }

        public void EnemyDeath()
        {
            GameManager.EnemyAttack -= Play;
            Initialize();
            Destroy(attackObjPare);
            Destroy(this.gameObject);
        }
    }

    [Serializable]
    public class AttackSetting : IWeighted
    {
        public AttackName name;
        [field: SerializeField] public int weight { get; set; }
    }
    [Serializable]
    public class SpecialAttackSetting : IWeighted
    {
        public SpecialAttackName name;
        [field: SerializeField] public int weight {  get; set;}
    }
    
    public interface IWeighted
    {
        int weight { get; set; }
    }

    [Serializable]
    public enum AttackOrder
    {
        Normal,
        Special,
    }
}
