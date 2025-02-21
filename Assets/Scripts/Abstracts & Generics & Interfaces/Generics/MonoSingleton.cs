using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {

#if UNITY_6000_0_OR_NEWER
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
                _instance = FindObjectOfType<T>();
#endif
            }
            return _instance;
        }
    }

    protected virtual void Awake() => CheckInstance();

    protected void CheckInstance()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
    }

}
