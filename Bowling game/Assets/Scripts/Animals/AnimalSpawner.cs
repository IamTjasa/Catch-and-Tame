using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float minDistance = 2f;
    public float maxDistance = 6f;

    [Header("Spawn Timing (seconds)")]
    public float minSpawnDelay = 5f;
    public float maxSpawnDelay = 10f;

    [Header("Resources/Prefabs Folder")]
    public string resourcesFolder = "Prefabs";

    private List<GameObject> animalPrefabs = new List<GameObject>();

    void Start()
    {
        LoadAnimalPrefabs();
        StartCoroutine(SpawnAnimalLoop());
    }

    void LoadAnimalPrefabs()
    {
        Object[] loaded = Resources.LoadAll(resourcesFolder, typeof(GameObject));
        foreach (Object obj in loaded)
        {
            GameObject prefab = obj as GameObject;
            if (prefab != null)
            {
                animalPrefabs.Add(prefab);
                Debug.Log("Loaded animal prefab: " + prefab.name);
            }
        }

        if (animalPrefabs.Count == 0)
            Debug.LogWarning("No animal prefabs found in Resources/" + resourcesFolder);
    }

    IEnumerator SpawnAnimalLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnDelay, maxSpawnDelay);
            Debug.Log("Waiting " + waitTime + " seconds to spawn...");
            yield return new WaitForSeconds(waitTime);

            if (GameObject.FindGameObjectWithTag("Animal") != null)
            {
                Debug.Log("Animal already exists, skipping spawn.");
                continue;
            }

            SpawnRandomAnimal();
        }
    }

    void SpawnRandomAnimal()
    {
        if (animalPrefabs.Count == 0)
        {
            Debug.LogWarning("No animal prefabs to spawn.");
            return;
        }

        GameObject randomAnimal = animalPrefabs[Random.Range(0, animalPrefabs.Count)];

        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
        Vector3 spawnPos = new Vector3(randomCircle.x, 0f, randomCircle.y);

        GameObject spawnedAnimal = Instantiate(randomAnimal, spawnPos, Quaternion.identity);
        Debug.Log("Spawned animal: " + spawnedAnimal.name + " at position: " + spawnPos);
    }
}
