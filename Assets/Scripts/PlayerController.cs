using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;          // 이동 속도
    public float rotateSmooth = 10f;      // (3인칭일 때) 이동 방향을 바라보는 회전 부드러움
    public Transform cam;                 // 사용할 카메라(비워두면 MainCamera 자동 사용)
    public CameraSwitch camSwitch;        // 1인칭/3인칭 모드 확인용

    private Rigidbody rb;
    private Vector3 moveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
        if (camSwitch == null && Camera.main != null)
            camSwitch = Camera.main.GetComponent<CameraSwitch>();
    }

    void Update()
    {
        // 입력 (WASD)
        float h = Input.GetAxisRaw("Horizontal"); // A(-1) / D(+1)
        float v = Input.GetAxisRaw("Vertical");   // S(-1) / W(+1)

        // 카메라 기준 축: Y를 제거(바닥에 투영)
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        // 이동 방향(1인칭/3인칭 공통: 카메라 기준으로 전후/좌우 움직임)
        moveDir = (camForward * v + camRight * h).normalized;

        bool isFP = (camSwitch != null && camSwitch.IsFirstPerson);

        // 3인칭일 때만 이동 방향을 바라보도록 자동 회전
        if (!isFP && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }
        // 1인칭일 때 회전은 마우스(FirstPersonMouseLook)가 담당 → 여기선 회전 금지
    }

    void FixedUpdate()
    {
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // 키를 안 누르면 수평속도 0
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }
}
