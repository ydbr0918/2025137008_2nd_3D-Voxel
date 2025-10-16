using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;          // 플레이어 Transform
    public Transform head;            // 플레이어 머리(눈) 위치(없으면 eyeHeight 사용)

    [Header("Third-Person (Quarter View)")]
    public Vector3 quarterOffset = new Vector3(0f, 6f, -8f);  // 월드 기준 오프셋
    public Vector3 quarterEuler = new Vector3(45f, 45f, 0f);  // 쿼터뷰 고정 각도

    [Header("First-Person")]
    public float eyeHeight = 1.6f;    // head가 없을 때 눈높이
    [Range(-89f, 89f)] public float fpPitch = 0f; // 1인칭 상하 각(마우스가 변경)

    [Header("Common")]
    public float posSmooth = 10f;
    public float rotSmooth = 12f;

    [Header("(Optional) Hide Mesh in FP")]
    public Renderer[] hideInFirstPerson; // 1인칭에서 가려야 하는 머리/몸 메쉬

    bool isFirstPerson = false;
    public bool IsFirstPerson => isFirstPerson;  // 외부에서 읽기

    Vector3 targetPos;
    Quaternion targetRot;

    void Start()
    {
        // 시작은 3인칭 쿼터뷰
        isFirstPerson = false;
        ApplyVisibility();
        // 초기 위치/회전 세팅
        targetPos = CalcQuarterPos();
        targetRot = Quaternion.Euler(quarterEuler);
        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            ApplyVisibility();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (isFirstPerson)
        {
            // 1인칭: 눈 위치에 고정
            Vector3 eyePos = head != null ? head.position : (player.position + Vector3.up * eyeHeight);
            targetPos = eyePos;

            // 플레이어가 보는 방향(수평 yaw)을 따르고, pitch는 fpPitch 사용
            Vector3 forward = player.forward;
            forward.y = 0f;
            forward.Normalize();
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;

            float yaw = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
            targetRot = Quaternion.Euler(fpPitch, yaw, 0f);
        }
        else
        {
            // 3인칭: 쿼터뷰 각도로 고정 + 플레이어를 따라 이동
            targetPos = CalcQuarterPos();
            targetRot = Quaternion.Euler(quarterEuler);
        }

        // 부드럽게 보간
        transform.position = Vector3.Lerp(transform.position, targetPos, posSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSmooth * Time.deltaTime);
    }

    Vector3 CalcQuarterPos()
    {
        // quarterOffset은 "월드 기준" 오프셋으로 처리(각도 고정).
        // 플레이어 위치만 따라가고, 카메라 각도는 quarterEuler로 고정 유지.
        return player.position + quarterOffset;
    }

    void ApplyVisibility()
    {
        if (hideInFirstPerson == null) return;
        foreach (var r in hideInFirstPerson)
        {
            if (r != null) r.enabled = !isFirstPerson; // 1인칭일 때 숨김
        }
    }
}
