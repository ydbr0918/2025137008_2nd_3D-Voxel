// RunFootstepForce.cs  (Unity 2022.3)
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RunFootstepForce : MonoBehaviour
{
    [Header("Input")]
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Clips")]
    public AudioClip[] clips;

    [Header("Playback")]
    [Tooltip("발소리 간격(초)")]
    public float stepInterval = 0.30f;
    [Range(0f, 1f)] public float volume = 0.9f;
    [Range(0.8f, 1.2f)] public float pitchBase = 1.0f;
    [Range(0f, 0.2f)] public float pitchJitter = 0.04f;
    public bool force2D = true;               // 2D로 강제 (거리/리스너 문제 방지)
    public bool ignoreListenerPause = true;   // 전역 일시정지 무시

    AudioSource src;
    Coroutine loopCo;
    bool runningHeld; // 현재 프레임에 달리기 입력 유지중?

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        if (force2D) src.spatialBlend = 0f;   // 2D
        if (ignoreListenerPause) src.ignoreListenerPause = true;
    }

    void Update()
    {
        bool moveHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A)
                      || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool runHeld = Input.GetKey(runKey);

        bool wantRun = runHeld && moveHeld;

        if (wantRun && !runningHeld)
        {
            // 달리기 시작: 즉시 1번 재생 + 루프 시작
            PlayOne();
            loopCo = StartCoroutine(StepLoop());
        }
        else if (!wantRun && runningHeld)
        {
            // 달리기 종료: 루프 정지
            if (loopCo != null) { StopCoroutine(loopCo); loopCo = null; }
        }

        runningHeld = wantRun;
    }

    IEnumerator StepLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(stepInterval);
            PlayOne();
        }
    }

    void PlayOne()
    {
        if (clips == null || clips.Length == 0 || src == null) return;
        int i = clips.Length == 1 ? 0 : Random.Range(0, clips.Length);
        src.pitch = pitchBase + Random.Range(-pitchJitter, pitchJitter);
        src.volume = volume;
        src.PlayOneShot(clips[i]);
    }
}
