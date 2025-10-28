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
    [Tooltip("���� �� ���� ��� �ִٸ� true")]
    [SerializeField] private bool hasGun = false;   // �⺻: false (�� ���� �� ���� �Ұ�)
    [SerializeField] private int aimMouseButton = 1; // 1 = ��Ŭ��

    private Animator anim;
    private float speedParam = 0f;

    // �ܺο��� ���� ����
    public bool IsAimingActive { get; private set; }
    public bool HasGun => hasGun;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        // Animator �Ķ���� �ʱ�ȭ(����)
        anim.SetBool("isAiming", false);
        anim.SetBool("HasGun", hasGun);
        anim.ResetTrigger("Hit");
    }

    /// �̵� ���� ���� �ܺ�(�̵� ��ũ��Ʈ)���� �־��� �� ���(����)
    public void SetLocomotionSpeed(float target, float smoothingMul = 1f)
    {
        speedParam = Mathf.Lerp(speedParam, target, Time.deltaTime * speedSmoothing * Mathf.Max(0.01f, smoothingMul));
        anim.SetFloat("Speed", speedParam);
    }

    private void Update()
    {
        // ���� �־�߸� ���� ����
        bool wantAim = hasGun && Input.GetMouseButton(aimMouseButton);

        // Animator�� �ݿ�
        anim.SetBool("isAiming", wantAim);
        anim.SetBool("HasGun", hasGun);

        IsAimingActive = wantAim;

        // (�׽�Ʈ) �ǰ�: H
        if (Input.GetKeyDown(KeyCode.H))
        {
            anim.ResetTrigger("Hit");
            anim.SetTrigger("Hit");
        }
    }

    // ===== �� ���� ���� ���� API =====
    public void SetHasGun(bool value)
    {
        hasGun = value;
        anim.SetBool("HasGun", hasGun);
        if (!hasGun)
        {
            // ���� ������� ���� ��� ����
            anim.SetBool("isAiming", false);
            IsAimingActive = false;
        }
    }
}
