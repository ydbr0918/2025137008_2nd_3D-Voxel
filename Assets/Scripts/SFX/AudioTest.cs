using UnityEngine;

public class AudioTest : MonoBehaviour
{
    public AudioSource src;
    void Start()
    {
        if (src != null && src.clip != null)
        {
            Debug.Log("[AudioTest] Play footstep manually");
            src.PlayOneShot(src.clip);
        }
    }
}
