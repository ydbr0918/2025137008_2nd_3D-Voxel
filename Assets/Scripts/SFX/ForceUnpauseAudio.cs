using UnityEngine;

public class ForceUnpauseAudio : MonoBehaviour
{
    void Awake()
    {
        // ����Ƽ ���� ����� �Ͻ����� ����
        AudioListener.pause = false;

        // ���� �����ϴ� ��� ������ҽ��� ���� ��Ʈ ����
        foreach (var s in FindObjectsOfType<AudioSource>(true))
        {
            s.mute = false;
            s.ignoreListenerPause = true;
            if (s.volume <= 0f) s.volume = 0.8f;
        }
    }
}
