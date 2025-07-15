using UnityEngine;

public class BallHitAnimal : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Animal"))
        {
            float chance = Random.value; 
            Debug.Log("Chance roll: " + chance);

            if (chance < 0.5f)
            {
                Debug.Log("Animal caught! Destroying.");
                Destroy(collision.gameObject);
            }
            else
            {
                Debug.Log("Animal escaped! Playing reaction animation.");

                AnimalReaction reaction = collision.gameObject.GetComponent<AnimalReaction>();
                if (reaction != null)
                {
                    reaction.PlayReaction();
                }
                else
                {
                    Debug.LogWarning("AnimalReaction script missing on: " + collision.gameObject.name);
                }
            }
        }
    }
}
