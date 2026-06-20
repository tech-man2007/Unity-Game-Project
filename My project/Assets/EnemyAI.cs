using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Transform player;

    void Start()
    {
        // Automatically find the Player object in the scene
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            // 1. Rotate to look at the player
            Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(targetPosition);

            // 2. Move forward toward the player
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }
}