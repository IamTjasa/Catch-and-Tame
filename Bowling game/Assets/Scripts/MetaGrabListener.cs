using UnityEngine;
using Meta.WitAi.Utilities;
using Oculus.Interaction;

public class MetaGrabListener : MonoBehaviour
{
    public ShowOnMetaGrab showScript;

    private Grabbable grabbable;

    private void Awake()
    {
        grabbable = GetComponent<Grabbable>();
    }

    private void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += OnGrabEvent;
        }
    }

    private void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }

    private void OnGrabEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            if (showScript != null)
                showScript.Show();
        }
    }
}
