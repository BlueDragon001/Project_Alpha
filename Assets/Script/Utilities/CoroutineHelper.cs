using UnityEngine;

public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper instance;
    
    public static CoroutineHelper Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CoroutineHelper");
                instance = go.AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
}
