using UnityEngine;
using Oculus.Interaction;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;

    private Grabbable grabbable;
    private bool hasSpawned = false;

    void Awake()
    {
        grabbable = GetComponent<Grabbable>();
    }

    void OnEnable()
    {
        grabbable.WhenPointerEventRaised += OnGrabEvent;
    }

    void OnDisable()
    {
        grabbable.WhenPointerEventRaised -= OnGrabEvent;
    }

    private void OnGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && !hasSpawned)
        {
            hasSpawned = true;

            GameObject newBall = Instantiate(ballPrefab, transform.position, transform.rotation);
            Debug.Log($"Spawned new ball: {newBall.name}");

            if (!newBall.TryGetComponent(out MetaGrabListener _))
                Debug.LogWarning("New ball is missing MetaGrabListener!");

            if (!newBall.TryGetComponent(out ShowOnMetaGrab _))
                Debug.LogWarning("New ball is missing ShowOnMetaGrab!");
        }
    }
}
