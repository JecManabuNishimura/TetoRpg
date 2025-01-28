using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.Instance.menuFlag = true;
            MenuManager.Instance.OpenMenu();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuManager.Instance.CloseMenu();
        }
    }
}
