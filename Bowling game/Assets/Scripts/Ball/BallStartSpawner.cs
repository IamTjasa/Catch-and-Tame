using UnityEngine;
using System.Collections;

public class BallStartSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform handSpawnPoint;
    public float verticalOffset = 0.15f;

    private bool firstBallSpawned = false;
    private GameObject currentBall;

    void Start()
    {
        StartCoroutine(SpawnWithDelay(3f));
    }

    void Update()
    {
        if (firstBallSpawned && currentBall == null)
        {
            SpawnBall();
        }
    }

    IEnumerator SpawnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentBall == null)
        {
            SpawnBall();
            firstBallSpawned = true;
        }
    }

    void SpawnBall()
    {
        Vector3 spawnPos = handSpawnPoint.position + handSpawnPoint.up * verticalOffset;
        Quaternion spawnRot = handSpawnPoint.rotation;

        currentBall = Instantiate(ballPrefab, spawnPos, spawnRot, handSpawnPoint);
        currentBall.tag = "Ball";

        Debug.Log("Ball spawned at: " + spawnPos);
    }
}
