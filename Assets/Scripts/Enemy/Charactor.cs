using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class Charactor : CharactorData,ICharactor
    {
        [SerializeField] private GameObject attackArea;
        [SerializeField] private float AttackTime = 3;
        [SerializeField] private int AttackInterval = 3;
        [SerializeField] private Damage dameUi;
        [SerializeField] private ParticleSystem particle;
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private float strength = 30.0f;
        [SerializeField] private int vibrato = 30;
        [SerializeField] private bool shakeFlag = false;
        [SerializeField] private AttackSetting[] attackSettings;
        
        private List<ParticleSystem> parts = new List<ParticleSystem>();
        private List<GameObject> area = new ();
        private GameObject attackObjPare;
        private float timer = 0;
        private float attackTime;
        private bool timerFlag = false;
        private int AttackHeight = 1;
        private Tweener shakeTweener;
        private Vector3 initPosition;

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
                // 攻撃の種類


                int pattern = SelectAttackPattern();
                if(pattern == -1)
                {
                    Debug.LogError("攻撃パターンが設定されていません");
                }
                var data = AttackData.GetAttack(attackSettings[pattern].name);
                foreach (var d in data)
                {
                    GameObject aObj = Instantiate(attackArea, new Vector3(d.x,d.y,0), Quaternion.identity);
                    area.Add(aObj);
                    aObj.transform.parent = attackObjPare.transform;
                }

                //SpecialAttackFlag = true;
                timerFlag = true;
            }
        }

        int SelectAttackPattern ()
        {
            int totalWeight = attackSettings.Sum(data => data.weight);
            
            int rand = Random.Range(1, totalWeight+1);
            int count = 0;
            foreach (var val in attackSettings)
            {
                if(rand <= val.weight)
                {
                    return count;
                }
                rand -= val.weight;
                count++;
            }
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

            if (GameManager.maxPutposFlag)
            {
                //ReadyAttack();
            }
            if (timerFlag)
            {
                timer += Time.deltaTime;
                //　一番上まで積まれたら攻撃開始
                if (timer >= AttackTime || GameManager.maxPutposFlag)
                {
                    GameManager.EnemyAttackFlag = true;
                    timer = 0;
                    
                    timerFlag = false;
                }
            }
            else
            {
                ReadyAttack();
                BoardManager.Instance.CheckMaxPutPos();
            }
        }
        
        async Task Play()
        {
            if (SpecialAttackFlag)
            {
                AttackData.GetSpecialAttack(SpecialAttackName.LastAddLine);
                GameManager.EnemyAttackFlag = false;
                return;
            }
            
            var areaCopy = area.ToArray();  // エラー回避用
            foreach (var val in areaCopy)
            {
                if (val != null)
                {
                    var position = val.transform.position;
                    if (!BoardManager.Instance.HitCheck((int)position.x, (int)position.y))
                    {
                        parts.Add(Instantiate(particle, new Vector3(position.x, position.y, 0), Quaternion.identity));
                        parts[^1].Play();
                        parts[^1].GetComponent<Bullet>().target = GameManager.player.gameObject;
                    }
                }
            }

            await Task.Delay(50);
            int count = 0;
            foreach (var val in areaCopy)
            {
                if (val != null)
                {
                    var position = val.transform.position;
                    if (!BoardManager.Instance.HitCheck((int)position.x, (int)position.y))
                    {
                        parts[count].GetComponent<Bullet>().StartMove();    
                        await BoardManager.Instance.EnemyAttack((int)position.x, (int)position.y);
                        count++;
                    }
                }
            }
            
            // ダメージ処理
            GameManager.player.Damage(count);
            foreach (var val in areaCopy)
            {
                var position = val.transform.position;
                BoardManager.Instance.CheckDeleteLine((int)position.y);
            }
            
            parts.Clear();
            foreach (var val in area)
            {
                Destroy(val);
            }
            area.Clear();
            GameManager.EnemyAttackFlag = false;
            AttackHeight = Random.Range(3, 6);
            attackTime = 0; 
        }

        public void UpdateHp()
        {
            throw new NotImplementedException();
        }

        public void Damage(int damage)
        {
            if(damage != 0)
            {
                int newDamage = damage / 2 - status.def / 4;
                status.hp -= newDamage;
                GetComponent<Animator>().Play("DamageAnim", 0, 0);
                if(status.hp < 0)
                {
                    status.hp = 0;
                    GameManager.EnemyDown = true;
                }
            }
        }
    }

    [Serializable]
    public struct AttackSetting
    {
        public AttackName name;
        public int weight;
    }
}
