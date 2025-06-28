using UnityEngine;

public class ShowOnMetaGrab : MonoBehaviour
{
    private Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;
    }

    public void Show()
    {
        foreach (var r in renderers)
            r.enabled = true;
    }
}
