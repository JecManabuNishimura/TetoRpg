using System.Threading.Tasks;
using UnityEngine;

public interface ICharactor
{
    void UpdateHp();
    Task Damage(int damage);
}
