using UnityEngine;

public class BallHitAnimal : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Ball collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Animal"))
        {
            Debug.Log("Animal was hit! Destroying it.");
            Destroy(collision.gameObject);
        }
    }
}
