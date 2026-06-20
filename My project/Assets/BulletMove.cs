using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LevelManager manager = FindObjectOfType<LevelManager>();
            if (manager != null) manager.LoseGame();
            Destroy(gameObject);
        }

        // Safer check: If the name contains "Wall", "Floor", or "Cube", destroy the bullet
        string hitName = collision.gameObject.name.ToLower();
        if (hitName.Contains("wall") || hitName.Contains("floor") || hitName.Contains("cube"))
        {
            Destroy(gameObject);
        }
    }
}