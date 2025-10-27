using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LeverSwitch : MonoBehaviour
{
    public string leverId = "lever_01";
    public PowerManager powerManager;
    public KeyCode key = KeyCode.F;

    [Header("Visual (선택)")]
    public Transform handle;          // 레버 손잡이(회전용)
    public Vector3 onEuler = new Vector3(-30, 0, 0);
    public Vector3 offEuler = Vector3.zero;
    public AudioSource sfx;
    public AudioClip clickOn;

    bool inRange = false;
    bool isOn = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) inRange = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) inRange = false;
    }

    void Update()
    {
        if (!inRange || isOn) return;
        if (Input.GetKeyDown(key))
        {
            isOn = true;
            if (handle) handle.localEulerAngles = onEuler;
            if (sfx && clickOn) sfx.PlayOneShot(clickOn);
            powerManager?.ActivateLever(leverId);
        }
    }

    void OnValidate()
    {
        if (handle) handle.localEulerAngles = isOn ? onEuler : offEuler;
    }
}
