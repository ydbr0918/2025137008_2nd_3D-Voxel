using UnityEngine;
using System.Collections;

public class LeverSwitch : MonoBehaviour
{
    [Header("Hookups")]
    public Transform handle;                 // 회전 축이 될 빈 오브젝트(Handle)
    public ElevatorManager elevatorManager;  // 불 색상 제어 담당
    public PowerManager powerManager;        // 전원 완료 카운트 담당

    [Header("Mapping")]
    [Tooltip("0=1번불, 1=2번불, 2=3번불")]
    public int indicatorIndex = 0;           // 이 레버가 켤 불의 인덱스(0-based)
    public string leverId = "Lever1";        // PowerManager 중복 방지를 위한 고유 ID

    [Header("Anim")]
    public float rotateAngle = 60f;          // 위→아래로 숙일 각도(반대면 -60)
    public float rotateSpeed = 6f;           // 회전 속도

    [Header("Input")]
    public float interactDistance = 3f;
    public KeyCode useKey = KeyCode.F;

    bool isPulled = false;
    bool isRotating = false;
    Quaternion targetRot;

    void Update()
    {
        if (isPulled || isRotating) return;

        // 간단한 거리 판정(원하면 레이캐스트/프롬프트로 대체 가능)
        var cam = Camera.main;
        if (!cam) return;
        if (Vector3.Distance(cam.transform.position, transform.position) > interactDistance) return;

        if (Input.GetKeyDown(useKey))
        {
            PullLever();
        }
    }

    void PullLever()
    {
        if (isPulled) return;
        if (!handle) { Debug.LogWarning($"{name}: Handle이 비어 있음"); return; }

        isPulled = true;
        isRotating = true;

        targetRot = Quaternion.Euler(
            handle.localEulerAngles.x + rotateAngle,
            handle.localEulerAngles.y,
            handle.localEulerAngles.z
        );
        StartCoroutine(RotateHandleCo());

        // 🔶 불(Indicator) 1:1 매핑으로 켜기
        elevatorManager?.SetIndicator(indicatorIndex, true);

        // 🔶 전원 카운트 보고(중복 방지)
        powerManager?.ActivateLever(string.IsNullOrEmpty(leverId) ? $"Lever_{indicatorIndex}" : leverId);
    }

    IEnumerator RotateHandleCo()
    {
        while (Quaternion.Angle(handle.localRotation, targetRot) > 0.1f)
        {
            handle.localRotation = Quaternion.Slerp(handle.localRotation, targetRot, Time.deltaTime * rotateSpeed);
            yield return null;
        }
        handle.localRotation = targetRot;
        isRotating = false;
    }
}
