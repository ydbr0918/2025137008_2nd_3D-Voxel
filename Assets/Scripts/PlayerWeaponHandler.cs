using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    public Transform rightHand;
    public Vector3 localPositionOffset = new Vector3(0.06f, 0f, 0.02f);
    public Vector3 localEulerOffset = new Vector3(0f, 90f, 0f);

    public SimpleGun CurrentGun { get; private set; }

    public void EquipGun(GameObject gunPrefab)
    {
        if (rightHand == null || gunPrefab == null) return;
        if (CurrentGun != null) Destroy(CurrentGun.gameObject);
        var go = Instantiate(gunPrefab, rightHand);
        go.transform.localPosition = localPositionOffset;
        go.transform.localEulerAngles = localEulerOffset;
        go.transform.localScale = Vector3.one;
        CurrentGun = go.GetComponent<SimpleGun>();
    }

    public void Unequip()
    {
        if (CurrentGun != null) Destroy(CurrentGun.gameObject);
        CurrentGun = null;
    }
}
