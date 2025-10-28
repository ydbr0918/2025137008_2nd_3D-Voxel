using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float speedAcceleration = 10f;

    [Header("Rotation (3rd Person only)")]
    [SerializeField] private float rotateSmooth = 10f;

    [Header("Camera / Mode")]
    [SerializeField] private Transform cam;            // 비우면 MainCamera
    [SerializeField] private CameraSwitch camSwitch;   // 선택

    [Header("Input")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Refs")]
    [SerializeField] private PlayerAnimDriver animDriver; // ★ Animator 파라미터 제어용

    private Rigidbody rb;
    private Vector3 moveDir;
    private float currentSpeed;
    public bool IsRunning { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (camSwitch == null && Camera.main != null) camSwitch = Camera.main.GetComponent<CameraSwitch>();
        if (animDriver == null) animDriver = GetComponent<PlayerAnimDriver>();
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        // ── Aiming 중이면 이동/회전/애니메이션 Speed 막기 ───────────────
        bool blockByAiming = (animDriver != null && animDriver.IsAimingActive);
        if (blockByAiming)
        {
            moveDir = Vector3.zero;
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * speedAcceleration);

            // Locomotion BlendTree용 Speed 파라미터도 0으로 (Idle)
            if (animDriver != null) animDriver.SetLocomotionSpeed(0f);

            return; // 회전도 차단
        }
        // ─────────────────────────────────────────────────────────────

        // 입력 (WASD)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Vector3.ProjectOnPlane(cam != null ? cam.forward : Vector3.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam != null ? cam.right : Vector3.right, Vector3.up).normalized;

        moveDir = (camForward * v + camRight * h).normalized;

        // 달리기 여부
        IsRunning = Input.GetKey(runKey);

        // 물리 이동 속도(실제 m/s)
        float targetSpeed = (moveDir.sqrMagnitude > 0.0001f) ? (IsRunning ? runSpeed : walkSpeed) : 0f;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedAcceleration);

        // ★ Animator Locomotion BlendTree용 Speed 파라미터 갱신
        //    0 = Idle, 0.5 = Walk, 1.0 = Run  (네 BlendTree Threshold와 일치!)
        float animTarget = 0f;
        if (moveDir.sqrMagnitude > 0.0001f)
            animTarget = IsRunning ? 1f : 0.5f;
        if (animDriver != null) animDriver.SetLocomotionSpeed(animTarget);

        // 3인칭에서만 자동 회전
        bool isFP = (camSwitch != null && camSwitch.IsFirstPerson);
        if (!isFP && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSmooth * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // Aiming 중이면 완전 정지
        if (animDriver != null && animDriver.IsAimingActive)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            rb.MovePosition(rb.position + moveDir * currentSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.fixedDeltaTime * speedAcceleration);
        }
    }

    // 인스펙터에서 실시간 조절용
    public void SetWalkSpeed(float v) => walkSpeed = Mathf.Max(0f, v);
    public void SetRunSpeed(float v) => runSpeed = Mathf.Max(0f, v);
    public float GetWalkSpeed() => walkSpeed;
    public float GetRunSpeed() => runSpeed;
}
