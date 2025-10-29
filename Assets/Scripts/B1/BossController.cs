using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Explosion")]
    public ParticleSystem explodeEffect;
    public AudioClip explodeClip;
    public float delayBeforeDestroy = 0.5f;
    public GameObject bossVisual; // mesh root to disable

    AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 호출되면 폭발 연출
    public void ExplodeAndClear()
    {
        // visual 숨기고 파티클 재생, 사운드
        if (bossVisual) bossVisual.SetActive(false);
        if (explodeEffect) explodeEffect.Play();
        if (explodeClip) audioSource.PlayOneShot(explodeClip);

        // 게임 클리어 콜백은 ArenaManager가 호출하도록
    }
}
