using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    static CoroutineRunner _Instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject().AddComponent<CoroutineRunner>();
                _Instance.name = _Instance.GetType().ToString();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }
}
