using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepRunSfx : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;           // �÷��̾� ��Ʈ�ѷ�
    public PlayerAnimDriver animDriver;       // (����) ���̹� ���ܿ�
    public Rigidbody rb;                      // �ӵ� �б��(������ �ڵ� Ž��)

    [Header("Clips")]
    public AudioClip[] runClips;              // ���½����� ������ �޸��� �߼Ҹ���

    [Header("Playback")]
    public bool useAnimationEvents = false;   // �ִϸ��̼� �̺�Ʈ�� �������
    [Tooltip("�ȱ� �ӵ������� ����(��): ���� Ŭ���� ������ ����")]
    public float stepIntervalAtWalk = 0.50f;  // �ȱ� ���� ���� �ð�
    [Tooltip("�޸��� �ӵ������� ����(��)")]
    public float stepIntervalAtRun = 0.33f;  // �޸��� ���� ���� �ð�
    [Range(0f, 1f)] public float volume = 0.9f;
    [Range(0.8f, 1.2f)] public float pitchBase = 1.0f;
    [Range(0f, 0.2f)] public float pitchJitter = 0.04f;

    [Header("Conditions")]
    public float minSpeedForStep = 0.5f;      // �� �ӵ� �̸��̸� �߼Ҹ� x
    public LayerMask groundMask = ~0;         // �� ���̾�(������ Everything)
    public float groundCheckHeight = 0.2f;    // ���� ����ĳ��Ʈ ����
    public float groundCheckDistance = 0.3f;

    AudioSource src;
    float stepTimer;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        src = GetComponent<AudioSource>();
        if (src)
        {
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 1f;    // 3D ����
            src.rolloffMode = AudioRolloffMode.Linear;
            src.minDistance = 1.5f;
            src.maxDistance = 12f;
            src.volume = volume;
        }
    }

    void Awake()
    {
        if (!player) player = GetComponent<PlayerController>();
        if (!animDriver) animDriver = GetComponent<PlayerAnimDriver>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!src) src = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (useAnimationEvents) return; // �̺�Ʈ ����̸� Ÿ�̸� ���� ��Ȱ��

        if (!CanPlaySteps()) { stepTimer = 0f; return; }

        float speedNorm = GetSpeedNormalized(); // 0(����)~1(�޸���ӵ�)
        float interval = Mathf.Lerp(stepIntervalAtWalk, stepIntervalAtRun, speedNorm);

        stepTimer += Time.deltaTime;
        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayOneStep();
        }
    }

    bool CanPlaySteps()
    {
        if (player == null) return false;

        // �޸� ����
        if (!player.IsRunning) return false;

        // �̵� ������ (���� �ӵ�)
        float hSpeed = GetHorizontalSpeed();
        if (hSpeed < minSpeedForStep) return false;

        // ���̹� ���̸� �߼Ҹ� X (���ϴ� ����)
        if (animDriver != null && animDriver.IsAimingActive) return false;

        // ���� üũ
        Vector3 origin = transform.position + Vector3.up * groundCheckHeight;
        if (!Physics.Raycast(origin, Vector3.down, groundCheckDistance + groundCheckHeight, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }

    float GetHorizontalSpeed()
    {
        if (rb != null)
        {
            Vector3 v = rb.velocity; v.y = 0f;
            return v.magnitude;
        }
        // Rigidbody�� ���ٸ� Transform ��� ����(����)
        // (��Ȯ���� ������ �� �����Ƿ� �������� ����)
        return 0f;
    }

    float GetSpeedNormalized()
    {
        // �޸��� �ְ�� ��� ���� �ӵ� ����
        float runMax = player != null ? player.GetRunSpeed() : 6.5f;
        float hs = Mathf.Clamp(GetHorizontalSpeed(), 0f, runMax);
        return runMax > 0.01f ? (hs / runMax) : 0f;
    }

    void PlayOneStep()
    {
        if (runClips == null || runClips.Length == 0 || src == null) return;

        int i = Random.Range(0, runClips.Length);
        src.pitch = pitchBase + Random.Range(-pitchJitter, pitchJitter);
        src.volume = volume;
        src.PlayOneShot(runClips[i]);
    }

    // ���� �ִϸ��̼� �̺�Ʈ���� ȣ��� ����
    // Run �ִϸ��̼��� �� ������� �����ӿ� �� �Լ��� �̺�Ʈ�� �����ϸ�,
    // �ξ� ��Ȯ�� Ÿ�̹����� ����� �� �־�.
    public void PlayStepEvent()
    {
        if (!useAnimationEvents) return;   // �̺�Ʈ ��ĸ� ���
        if (CanPlaySteps()) PlayOneStep();
    }
}
