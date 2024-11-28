using UnityEngine;

[CreateAssetMenu(fileName = "EffectMaster", menuName = "Scriptable Objects/EffectMaster")]
public class EffectMaster : ScriptableObject
{
    public GameObject Explotion;

    private static EffectMaster _entity;

    public static EffectMaster Entity
    {
        get
        {
            //初アクセス時にロードする
            if (_entity == null)
            {
                _entity = Resources.Load<EffectMaster>("Master/EffectMaster");

                //ロード出来なかった場合はエラーログを表示
                if (_entity == null)
                {
                    Debug.LogError(nameof(EffectMaster) + " not found");
                }
            }

            return _entity;
        }
    }

    public void PlayEffect(EffectType type, Vector2 pos)
    {
        switch (type)
        {
            case EffectType.Explotion:
                var effect = Instantiate(Explotion);
                Destroy(effect,1);
                effect.transform.position = pos;
                break;
        }
    }
}
public enum EffectType
{
    Explotion,
}