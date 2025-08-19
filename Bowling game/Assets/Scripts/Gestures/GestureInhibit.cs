/*using UnityEngine;

public class GestureInhibit : MonoBehaviour
{
    static GestureInhibit _inst;
    float blockedUntil;

    public static bool IsBlocked => _inst != null && Time.time < _inst.blockedUntil;

    void Awake()
    {
        if (_inst && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void BeginHold()          
    {
        Ensure();                             
        _inst.blockedUntil = Time.time + 9999f;
    }

    public static void EndHold(float tailSeconds)  
    {
        Ensure();
        _inst.blockedUntil = Time.time + Mathf.Max(0f, tailSeconds);
    }

    public static void Block(float seconds)   
    {
        Ensure();
        _inst.blockedUntil = Mathf.Max(_inst.blockedUntil, Time.time + seconds);
    }

    static void Ensure()
    {
        if (_inst) return;
        var go = new GameObject("[GestureInhibit]");
        _inst = go.AddComponent<GestureInhibit>();
        DontDestroyOnLoad(go);
    }
}
*/