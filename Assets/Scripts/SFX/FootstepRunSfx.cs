using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepRunSfx : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;           // 플레이어 컨트롤러
    public PlayerAnimDriver animDriver;       // (선택) 에이밍 차단용
    public Rigidbody rb;                      // 속도 읽기용(없으면 자동 탐색)

    [Header("Clips")]
    public AudioClip[] runClips;              // 에셋스토어에서 가져온 달리기 발소리들

    [Header("Playback")]
    public bool useAnimationEvents = false;   // 애니메이션 이벤트로 재생할지
    [Tooltip("걷기 속도에서의 보폭(초): 값이 클수록 템포가 느림")]
    public float stepIntervalAtWalk = 0.50f;  // 걷기 기준 보폭 시간
    [Tooltip("달리기 속도에서의 보폭(초)")]
    public float stepIntervalAtRun = 0.33f;  // 달리기 기준 보폭 시간
    [Range(0f, 1f)] public float volume = 0.9f;
    [Range(0.8f, 1.2f)] public float pitchBase = 1.0f;
    [Range(0f, 0.2f)] public float pitchJitter = 0.04f;

    [Header("Conditions")]
    public float minSpeedForStep = 0.5f;      // 이 속도 미만이면 발소리 x
    public LayerMask groundMask = ~0;         // 땅 레이어(없으면 Everything)
    public float groundCheckHeight = 0.2f;    // 지면 레이캐스트 높이
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
            src.spatialBlend = 1f;    // 3D 사운드
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
        if (useAnimationEvents) return; // 이벤트 방식이면 타이머 로직 비활성

        if (!CanPlaySteps()) { stepTimer = 0f; return; }

        float speedNorm = GetSpeedNormalized(); // 0(정지)~1(달리기속도)
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

        // 달릴 때만
        if (!player.IsRunning) return false;

        // 이동 중인지 (수평 속도)
        float hSpeed = GetHorizontalSpeed();
        if (hSpeed < minSpeedForStep) return false;

        // 에이밍 중이면 발소리 X (원하는 동작)
        if (animDriver != null && animDriver.IsAimingActive) return false;

        // 지면 체크
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
        // Rigidbody가 없다면 Transform 기반 추정(간단)
        // (정확도가 떨어질 수 있으므로 권장하진 않음)
        return 0f;
    }

    float GetSpeedNormalized()
    {
        // 달리기 최고속 대비 현재 속도 비율
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

    // ── 애니메이션 이벤트에서 호출용 ──
    // Run 애니메이션의 발 내려찍는 프레임에 이 함수를 이벤트로 연결하면,
    // 훨씬 정확한 타이밍으로 재생할 수 있어.
    public void PlayStepEvent()
    {
        if (!useAnimationEvents) return;   // 이벤트 방식만 허용
        if (CanPlaySteps()) PlayOneStep();
    }
}
