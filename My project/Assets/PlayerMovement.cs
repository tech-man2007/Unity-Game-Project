using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody rb;

    void Start()
    {
        // Grabs the Rigidbody we just added to your player
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Reads the W, A, S, D or Arrow Keys
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Calculates direction relative to where you are currently looking
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Pushes the player through the world
        rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
    }
}