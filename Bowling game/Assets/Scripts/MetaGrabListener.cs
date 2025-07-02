using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
public class MetaGrabListener : MonoBehaviour
{
    private Grabbable grabbable;
    private Rigidbody rb;
    private Renderer[] renderers;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();

        // Skrij žogo in izklopi fiziko
        foreach (var r in renderers)
            r.enabled = false;

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void OnEnable()
    {
        grabbable.WhenPointerEventRaised += OnGrabEvent;
    }

    private void OnDisable()
    {
        grabbable.WhenPointerEventRaised -= OnGrabEvent;
    }

    private void OnGrabEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select) // On grab
        {
            // Pokaži žogo, a naj ostane brez fizike dokler je v roki
            foreach (var r in renderers)
                r.enabled = true;
        }
        else if (pointerEvent.Type == PointerEventType.Unselect) // On release
        {
            // Vklopi fiziko šele ko spustimo objekt
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }
}
