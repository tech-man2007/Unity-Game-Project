using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Gun Settings")]
    public float range = 100f;
    public Camera fpsCam;

    [Header("Effects")]
    public GameObject shatteredPrefab; 
    public float explosionForce = 600f;
    public float explosionRadius = 3f;

    void Update()
    {
        // Fire1 is Left Mouse Click
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hitInfo;

        // Shoots an invisible laser from the center of the camera
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hitInfo, range))
        {
            // Check if we hit an enemy
            if (hitInfo.transform.CompareTag("Enemy"))
            {
                // 1. Tell the ScoreManager to add a point
                if (ScoreManager.instance != null)
                {
                    ScoreManager.instance.AddPoint();
                }

                // 2. Spawn the shattered pieces (red blocks)
                GameObject shards = Instantiate(shatteredPrefab, hitInfo.transform.position, hitInfo.transform.rotation);
                
                // 3. Make the shards fly outward
                Rigidbody[] rbs = shards.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rbs)
                {
                    rb.AddExplosionForce(explosionForce, hitInfo.transform.position, explosionRadius);
                }

                // 4. Delete the solid red enemy
                Destroy(hitInfo.transform.gameObject);
            }
        }
    }
}