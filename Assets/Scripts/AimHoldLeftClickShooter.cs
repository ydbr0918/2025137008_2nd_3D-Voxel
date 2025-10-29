using UnityEngine;

public class AimHoldLeftClickShooter : MonoBehaviour
{
    public Transform muzzle;
    public GameObject bulletPrefab;
    public float bulletSpeed = 30f;
    public float fireCooldown = 0.12f;
    public float bulletLife = 3f;
    public KeyCode rightHold = KeyCode.Mouse1;
    public KeyCode leftFire = KeyCode.Mouse0;

    float lastFire;

    void Update()
    {
        if (muzzle == null || bulletPrefab == null) return;
        if (!Input.GetKey(rightHold)) return;
        if (!Input.GetKeyDown(leftFire)) return;
        if (Time.time < lastFire + fireCooldown) return;

        GameObject b = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        if (!b.TryGetComponent<Rigidbody>(out var rb)) rb = b.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = muzzle.forward * bulletSpeed;
        if (bulletLife > 0f) Destroy(b, bulletLife);

        lastFire = Time.time;
    }
}
