using UnityEngine;

public class GunPickup : MonoBehaviour
{
    public GameObject equippedGunPrefab;
    public string playerTag = "Player";
    public bool autoPickup = true;
    public KeyCode pickupKey = KeyCode.E;

    bool inside;
    PlayerWeaponHandler handler;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        handler = other.GetComponent<PlayerWeaponHandler>();
        inside = true;
        if (autoPickup) TryPickup();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        inside = false;
        handler = null;
    }

    void Update()
    {
        if (!autoPickup && inside && Input.GetKeyDown(pickupKey)) TryPickup();
    }

    void TryPickup()
    {
        if (handler == null || equippedGunPrefab == null) return;
        handler.EquipGun(equippedGunPrefab);
        Destroy(gameObject);
    }
}
