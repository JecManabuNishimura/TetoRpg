using System.Threading.Tasks;
using UnityEngine;

public class BombAttack : MonoBehaviour
{
    [SerializeField] private GameObject bomb;

    private GameObject obj;
    public Vector2Int hitPos;
    public async Task CreateBomb(int posX)
    {
         obj = Instantiate(bomb);
         obj.transform.position = new Vector3(posX, GameManager.Instance.boardHeight - 1);
         await Move();
    }

    async Task Move()
    {
        // ぶつかっていない場合は下に移動
        while(BoardManager.Instance.HitCheck((int)obj.transform.position.x, (int)obj.transform.position.y))
        {
            obj.transform.position += Vector3.down / 4.0f;
            if (obj.transform.position.y <= 1)
            {
                break;
            }
            await Task.Delay(10);
        }
        hitPos = new Vector2Int((int)obj.transform.position.x, (int)obj.transform.position.y);
        Destroy(obj);
    }
}
