using UnityEngine;

public class FloatingTextSpawner : MonoBehaviour
{
    public static void SpawnFloatingText(GameObject prefab, Vector3 position, float duration = 1f)
    {
        if (prefab == null) return;

        GameObject textObj = GameObject.Instantiate(prefab, position, Quaternion.identity);

        GameObject.Destroy(textObj, duration);
    }

}
