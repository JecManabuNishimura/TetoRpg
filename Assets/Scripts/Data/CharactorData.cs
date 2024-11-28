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

    public Status(int hp, int atk, int def) : this()
    {
        this.maxHp = hp;
        this.atk = atk;
        this.def = def;
    }

    public static Status operator +(Status a, Status b)
        => new Status(a.maxHp + b.hp, a.atk + b.atk, a.def + b.def);

    public static Status Zero()
    {
        return new Status(0, 0, 0);
    }

}
