#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForceAudioUnmuteOnPlay
{
    static ForceAudioUnmuteOnPlay()
    {
        // 플레이 모드 전환 시 자동으로 음소거 해제
        EditorApplication.playModeStateChanged += _ =>
        {
            EditorUtility.audioMasterMute = false;
        };

        // 혹시 다른 스크립트가 계속 켜버릴 경우 주기적으로 해제
        EditorApplication.update += () =>
        {
            if (EditorUtility.audioMasterMute)
                EditorUtility.audioMasterMute = false;
        };
    }
}
#endif
