using System;
using UnityEngine;

public class CharactorData : MonoBehaviour
{
    public Status status;
}

[Serializable]
public struct Status
{
    public int maxHp;
    public int hp;
    public int atk;
    public int def;
    public int critical;

    public Status(int hp, int atk, int def,int critical) : this()
    {
        this.maxHp = hp;
        this.atk = atk;
        this.def = def;
        this.critical = critical;
    }

    public static Status operator +(Status a, Status b)
    {
        Status status = new Status();
        status.hp = Mathf.Max(1,a.maxHp + b.hp);
        status.atk = Mathf.Max(0, a.atk + b.atk);
        status.def = Mathf.Max(0, a.def + b.def);
        status.critical = Mathf.Max(0, a.critical + b.critical);
        status.maxHp = Mathf.Max(1,a.maxHp + b.hp);
        return status;
    }
        

    public static Status Zero()
    {
        return new Status(0, 0, 0,0);
    }

}
