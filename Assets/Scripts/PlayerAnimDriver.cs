using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimDriver : MonoBehaviour
{
    [Header("BlendTree Thresholds")]
    public float walkSpeedValue = 0.5f;
    public float runSpeedValue = 1.0f;

    [Header("Smoothing")]
    public float speedSmoothing = 10f;

    [Header("Gun / Aim")]
    [Tooltip("시작 시 총을 들고 있다면 true")]
    [SerializeField] private bool hasGun = false;   // 기본: false (총 없을 때 조준 불가)
    [SerializeField] private int aimMouseButton = 1; // 1 = 우클릭

    private Animator anim;
    private float speedParam = 0f;

    // 외부에서 읽을 상태
    public bool IsAimingActive { get; private set; }
    public bool HasGun => hasGun;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        // Animator 파라미터 초기화(안전)
        anim.SetBool("isAiming", false);
        anim.SetBool("HasGun", hasGun);
        anim.ResetTrigger("Hit");
    }

    /// 이동 상태 값을 외부(이동 스크립트)에서 넣어줄 때 사용(선택)
    public void SetLocomotionSpeed(float target, float smoothingMul = 1f)
    {
        speedParam = Mathf.Lerp(speedParam, target, Time.deltaTime * speedSmoothing * Mathf.Max(0.01f, smoothingMul));
        anim.SetFloat("Speed", speedParam);
    }

    private void Update()
    {
        // 총이 있어야만 조준 가능
        bool wantAim = hasGun && Input.GetMouseButton(aimMouseButton);

        // Animator에 반영
        anim.SetBool("isAiming", wantAim);
        anim.SetBool("HasGun", hasGun);

        IsAimingActive = wantAim;

        // (테스트) 피격: H
        if (Input.GetKeyDown(KeyCode.H))
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
        }
    }

    // ===== 총 보유 상태 갱신 API =====
    public void SetHasGun(bool value)
    {
        hasGun = value;
        anim.SetBool("HasGun", hasGun);
        if (!hasGun)
        {
            // 총이 사라지면 조준 즉시 해제
            anim.SetBool("isAiming", false);
            IsAimingActive = false;
        }
    }
}
