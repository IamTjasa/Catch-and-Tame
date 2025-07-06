using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShowOnMetaGrab : MonoBehaviour
{
    private Renderer[] renderers;
    private Rigidbody rb;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();
        foreach (var r in renderers)
            r.enabled = false;

        rb.useGravity = false;
        rb.isKinematic = true;

        Debug.Log($"{gameObject.name}: Init with gravity OFF, kinematic ON, renderers OFF");
    }

    public void Show()
    {
        foreach (var r in renderers)
            r.enabled = true;

        rb.useGravity = true;
        rb.isKinematic = false;

        Debug.Log($"{gameObject.name}: Show() called  Gravity ON, Kinematic OFF, renderers ON");
    }
}
