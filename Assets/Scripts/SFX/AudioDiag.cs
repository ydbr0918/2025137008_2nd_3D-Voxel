// AudioDiag.cs
using UnityEngine;

public class AudioDiag : MonoBehaviour
{
    void Start()
    {
        var listeners = FindObjectsOfType<AudioListener>(true);
        Debug.Log($"[AudioDiag] listeners={listeners.Length}, pause={AudioListener.pause}");

        var sources = FindObjectsOfType<AudioSource>(true);
        int muted = 0; foreach (var s in sources) if (s.mute || s.volume <= 0f) muted++;
        Debug.Log($"[AudioDiag] sources={sources.Length}, mutedOrZeroVol={muted}");
    }
}
