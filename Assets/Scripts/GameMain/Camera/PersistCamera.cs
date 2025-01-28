using UnityEngine;

public class PersistCamera : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        GameManager.Instance.dontdestoryObj.Add(gameObject);
    }
}
