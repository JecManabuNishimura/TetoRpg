using UnityEngine;

public class PersistCamera : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameManager.dontdestoryObj.Add(gameObject);
    }
}
