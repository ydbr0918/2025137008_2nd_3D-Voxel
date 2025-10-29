using UnityEngine;

public class LeftClickShoot : MonoBehaviour
{
    public PlayerWeaponHandler weaponHandler;
    public bool autoFire = true;
    public KeyCode shootMouse = KeyCode.Mouse0;

    void Awake()
    {
        if (weaponHandler == null) weaponHandler = GetComponent<PlayerWeaponHandler>();
    }

    void Update()
    {
        if (weaponHandler == null || weaponHandler.CurrentGun == null) return;

        if (autoFire)
        {
            if (Input.GetKey(shootMouse)) weaponHandler.CurrentGun.Fire();
        }
        else
        {
            if (Input.GetKeyDown(shootMouse)) weaponHandler.CurrentGun.Fire();
        }
    }
}
