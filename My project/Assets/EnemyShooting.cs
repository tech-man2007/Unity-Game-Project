using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint; // Where the bullet spawns
    public float fireRate = 2f;
    private float nextFireTime;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Instantiate(bulletPrefab, firePoint.position, transform.rotation);
            nextFireTime = Time.time + fireRate;
        }
    }
}