using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;          // 이동 속도
    public float rotateSmooth = 10f;      // 회전 부드러움
    public Transform cam;                 // 사용할 카메라(비워두면 MainCamera 자동 사용)

    private Rigidbody rb;
    private Vector3 moveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }

    void Update()
    {
        // 입력 (WASD)
        float h = Input.GetAxisRaw("Horizontal"); // A(-1) / D(+1)
        float v = Input.GetAxisRaw("Vertical");   // S(-1) / W(+1)

        // 카메라 기준 축: Y를 제거(바닥에 투영)
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        // 이동 방향
        moveDir = (camForward * v + camRight * h).normalized;

        // 이동 방향이 있을 때 캐릭터가 그쪽을 바라보게
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // 키를 안 누르면 멈춤
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
}

