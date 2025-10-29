using UnityEngine;

public class PlayerShootInput : MonoBehaviour
{
    public PlayerWeaponHandler weaponHandler;
    public KeyCode shootMouse = KeyCode.Mouse1;

    void Awake()
    {
        if (weaponHandler == null) weaponHandler = GetComponent<PlayerWeaponHandler>();
    }

    void Update()
    {
        if (weaponHandler == null || weaponHandler.CurrentGun == null) return;
        if (Input.GetKeyDown(shootMouse)) weaponHandler.CurrentGun.Fire();
    }
}
