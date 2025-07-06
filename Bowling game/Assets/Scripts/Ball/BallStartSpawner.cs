using UnityEngine;
using System.Collections;

public class BallStartSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform handSpawnPoint;
    public float verticalOffset = 0.15f;

    private bool firstBallSpawned = false;

    void Start()
    {
        StartCoroutine(SpawnWithDelay(3f));
    }

    void Update()
    {
        if (firstBallSpawned && GameObject.FindWithTag("Ball") == null)
        {
            SpawnBall();
        }
    }

    IEnumerator SpawnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameObject.FindWithTag("Ball") == null)
        {
            SpawnBall();
            firstBallSpawned = true;
        }
    }

    void SpawnBall()
    {
        Vector3 spawnPos = handSpawnPoint.position + handSpawnPoint.up * verticalOffset;
        Quaternion spawnRot = handSpawnPoint.rotation;

        GameObject ball = Instantiate(ballPrefab, spawnPos, spawnRot, handSpawnPoint);
        ball.tag = "Ball";
    }
}
