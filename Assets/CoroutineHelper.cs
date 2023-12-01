using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;

    public static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("CoroutineHelper");
                _instance = obj.AddComponent<CoroutineHelper>();
            }
            return _instance;
        }
    }

    public void StartHelperCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}
