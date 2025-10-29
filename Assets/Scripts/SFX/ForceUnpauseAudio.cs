using UnityEngine;

public class ForceUnpauseAudio : MonoBehaviour
{
    void Awake()
    {
        // 유니티 전역 오디오 일시정지 해제
        AudioListener.pause = false;

        // 씬에 존재하는 모든 오디오소스에 대해 뮤트 해제
        foreach (var s in FindObjectsOfType<AudioSource>(true))
        {
            s.mute = false;
            s.ignoreListenerPause = true;
            if (s.volume <= 0f) s.volume = 0.8f;
        }
    }
}
