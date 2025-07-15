using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(AudioSource))]
public class MetaGrabListener : MonoBehaviour
{
    private Grabbable grabbable;
    private Rigidbody rb;
    private Renderer[] renderers;
    private AudioSource audioSource;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
        renderers = GetComponentsInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();

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
        if (pointerEvent.Type == PointerEventType.Select)
        {
            foreach (var r in renderers)
                r.enabled = true;
        }
        else if (pointerEvent.Type == PointerEventType.Unselect)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }
}
