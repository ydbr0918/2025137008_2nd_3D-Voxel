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

    // ȣ��Ǹ� ���� ����
    public void ExplodeAndClear()
    {
        // visual ����� ��ƼŬ ���, ����
        if (bossVisual) bossVisual.SetActive(false);
        if (explodeEffect) explodeEffect.Play();
        if (explodeClip) audioSource.PlayOneShot(explodeClip);

        // ���� Ŭ���� �ݹ��� ArenaManager�� ȣ���ϵ���
    }
}
