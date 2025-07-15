using UnityEngine;
using System.Collections;

public class BallHitAnimal : MonoBehaviour
{
    private AudioClip capturedSound;

    public GameObject floatingTextPrefab; 

    void Start()
    {
        capturedSound = Resources.Load<AudioClip>("Sounds/Captured");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Animal"))
        {
            float chance = Random.value;

            if (chance < 0.5f)
            {
                StartCoroutine(PlayCapturedSoundAndDestroy(collision.gameObject));
            }
            else
            {
                AnimalReaction reaction = collision.gameObject.GetComponent<AnimalReaction>();
                if (reaction != null) reaction.PlayReaction();
            }
        }
    }

    IEnumerator PlayCapturedSoundAndDestroy(GameObject animal)
    {
        GameObject soundObj = new GameObject("CapturedSoundPlayer");
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = capturedSound;
        audioSource.Play();

        if (floatingTextPrefab != null)
        {
            Vector3 spawnPos = animal.transform.position + Vector3.up * 1.5f;
            FloatingTextSpawner.SpawnFloatingText(floatingTextPrefab, spawnPos, 1f);
        }

        yield return new WaitForSeconds(capturedSound.length);

        Destroy(animal);
        Destroy(soundObj);
    }
}
