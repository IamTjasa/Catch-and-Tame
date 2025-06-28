using UnityEngine;
using Oculus.Interaction;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;

    private Grabbable grabbable;
    private bool hasSpawned = false;

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
        if (!hasSpawned && pointerEvent.Type == PointerEventType.Select)
        {
            hasSpawned = true;

            Instantiate(ballPrefab, transform.position, transform.rotation);
        }
    }
}
