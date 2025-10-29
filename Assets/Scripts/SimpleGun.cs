using UnityEngine;

public class SimpleGun : MonoBehaviour
{
    public Transform muzzle;
    public GameObject bulletPrefab;
    public float fireCooldown = 0.15f;
    public float spreadAngle = 0f;
    public float forceMultiplier = 1f;

    float lastFire;

    public bool CanFire()
    {
        return Time.time >= lastFire + fireCooldown && muzzle != null && bulletPrefab != null;
    }

    public void Fire()
    {
        if (!CanFire()) return;
        Vector3 dir = muzzle.forward;
        if (spreadAngle > 0f)
        {
            Quaternion r = Quaternion.Euler(Random.Range(-spreadAngle, spreadAngle), Random.Range(-spreadAngle, spreadAngle), 0f);
            dir = r * dir;
        }
        GameObject b = Object.Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(dir, Vector3.up));
        var proj = b.GetComponent<BulletProjectile>();
        if (proj != null) proj.Fire(dir, forceMultiplier);
        lastFire = Time.time;
    }
}
