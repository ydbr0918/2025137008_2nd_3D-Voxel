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

    // ---------- Camera Collision (3인칭 전용) ----------
    [Header("Camera Collision (3rd only)")]
    public LayerMask wallMask;        // ★ 여기엔 'Wall'만 체크 (Barrier 제외)
    public float camRadius = 0.3f;    // 스피어캐스트 반지름(카메라 반경)
    public float minCamDist = 0.5f;   // 너무 붙지 않도록 최소 거리
    public float collisionSmooth = 20f; // 충돌시 위치 보정 스무딩
    public float focusHeightBias = 1.4f; // 캐스트 시작 높이(가슴~머리 사이)

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

        // 클리핑 줄이기(선택)
        var cam = GetComponent<Camera>();
        if (cam) cam.nearClipPlane = 0.03f;
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
            // 3인칭: 기본 쿼터뷰 목표 위치/회전
            Vector3 desiredPos = CalcQuarterPos();
            Quaternion desiredRot = Quaternion.Euler(quarterEuler);

            // -------- 카메라 충돌 보정 --------
            Vector3 focus = head ? head.position : player.position + Vector3.up * focusHeightBias;
            Vector3 dir = desiredPos - focus;
            float dist = dir.magnitude;

            Vector3 fixedPos = desiredPos;

            if (dist > 0.001f)
            {
                dir /= dist; // 정규화

                // Wall 레이어만 맞는 SphereCast
                if (Physics.SphereCast(focus, camRadius, dir, out RaycastHit hit, dist, wallMask, QueryTriggerInteraction.Ignore))
                {
                    float safeDist = Mathf.Clamp(hit.distance - camRadius, minCamDist, dist);
                    fixedPos = focus + dir * safeDist;
                }
            }

            // 스무딩해서 적용
            targetPos = Vector3.Lerp(transform.position, fixedPos, collisionSmooth * Time.deltaTime);
            targetRot = Quaternion.Slerp(transform.rotation, desiredRot, rotSmooth * Time.deltaTime);
        }

        // 최종 스무딩(기존)
        transform.position = Vector3.Lerp(transform.position, targetPos, posSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSmooth * Time.deltaTime);
    }

    Vector3 CalcQuarterPos()
    {
        // quarterOffset은 "월드 기준" 오프셋
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
