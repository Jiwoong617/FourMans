using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject();
                go.name = typeof(T).Name;
                _instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
