#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForceAudioUnmuteOnPlay
{
    static ForceAudioUnmuteOnPlay()
    {
        // �÷��� ��� ��ȯ �� �ڵ����� ���Ұ� ����
        EditorApplication.playModeStateChanged += _ =>
        {
            EditorUtility.audioMasterMute = false;
        };

        // Ȥ�� �ٸ� ��ũ��Ʈ�� ��� �ѹ��� ��� �ֱ������� ����
        EditorApplication.update += () =>
        {
            if (EditorUtility.audioMasterMute)
                EditorUtility.audioMasterMute = false;
        };
    }
}
#endif
