using System.Drawing;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChainAnim : MonoBehaviour
{
    [SerializeField] private GameObject chain1;
    [SerializeField] private GameObject chain2;

    public async UniTask onChain()
    {
        var size = chain1.GetComponent<SpriteRenderer>().size;
        while (true)
        {
            size.x += 0.8f;
            if (size.x >= 10)
            {
                size.x = 10;
                chain1.GetComponent<SpriteRenderer>().size = size;
                chain2.GetComponent<SpriteRenderer>().size = size;
                break;
            }
            chain1.GetComponent<SpriteRenderer>().size = size;
            chain2.GetComponent<SpriteRenderer>().size = size;
            await UniTask.Yield();
        }
    }
    public async UniTask offChain()
    {
        var size = chain1.GetComponent<SpriteRenderer>().size;
        while (true)
        {
            size.x -= 0.8f;
            if (size.x <= 0)
            {
                size.x = 0;
                chain1.GetComponent<SpriteRenderer>().size = size;
                chain2.GetComponent<SpriteRenderer>().size = size;
                break;
            }
            chain1.GetComponent<SpriteRenderer>().size = size;
            chain2.GetComponent<SpriteRenderer>().size = size;
            await UniTask.Yield();
        }
    }
}
