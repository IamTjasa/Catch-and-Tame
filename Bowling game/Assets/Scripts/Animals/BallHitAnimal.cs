using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BallHitAnimal : MonoBehaviour
{
    private AudioClip capturedSound;

    public GameObject floatingTextPrefab;
    private Canvas progressCanvas;

    public AnimalUIManager uiManager;

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
                string kind = DetectAnimalKind(collision.gameObject);
                QuestEvents.AnimalCaught(kind);

                StartCoroutine(PlayCapturedSoundAndDestroy(collision.gameObject));
            }
            else
            {
                AnimalReaction reaction = collision.gameObject.GetComponent<AnimalReaction>();
                if (reaction != null) reaction.PlayReaction();
            }
        }
    }

    public void Initialize(Canvas scoringCanvas, AnimalUIManager manager)
    {
        progressCanvas = scoringCanvas;
        uiManager = manager;
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

        string animalName = animal.name.Replace("(Clone)", "").Trim();
        ReplaceAnimalImage(animalName);

        if (capturedSound != null)
            yield return new WaitForSeconds(capturedSound.length);
        else
            yield return null;

        Destroy(animal);
        Destroy(soundObj);
    }

    void ReplaceAnimalImage(string animalName)
    {
        Debug.Log("Calling ReplaceAnimalImage with: " + animalName);
        if (uiManager != null)
        {
            uiManager.ReplaceAnimalImage(animalName);
        }
        else
        {
            Debug.LogWarning("UIManager ni nastavljen!");
        }
    }

    string DetectAnimalKind(GameObject animal)
    {
        
        string n = animal.name.Replace("(Clone)", "").Trim();

        if (n.IndexOf("Cat", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Cat";
        if (n.IndexOf("Dog", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Dog";
        if (n.IndexOf("Dragon", System.StringComparison.OrdinalIgnoreCase) >= 0) return "Dragon";

       
        return "Animal";
    }
}
