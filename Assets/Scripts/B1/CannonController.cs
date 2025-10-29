using System.Collections;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("Fire")]
    public Transform muzzle;                // 탄환이 나올 위치(빈 transform)
    public GameObject cannonballPrefab;     // CannonBall 프리팹
    public float projectileSpeed = 20f;
    public float fireIntervalMin = 0.6f;
    public float fireIntervalMax = 2f;

    [Header("Overheat / Visual")]
    public Material normalMaterial;
    public Material overheatMaterial;
    public Renderer visualRenderer;         // 머티리얼 바꿀 렌더러 (캐논 모델)
    public ParticleSystem overheatEffect;   // 과열시 파티클 (선택)

    bool isOverheated = false;
    Coroutine firing;

    void Start()
    {
        if (muzzle == null) Debug.LogWarning($"{name} muzzle not set");
        firing = StartCoroutine(FireLoop());
    }

    IEnumerator FireLoop()
    {
        while (!isOverheated)
        {
            float wait = Random.Range(fireIntervalMin, fireIntervalMax);
            yield return new WaitForSeconds(wait);
            if (!isOverheated) FireOnce();
        }
    }

    void FireOnce()
    {
        if (cannonballPrefab == null || muzzle == null) return;
        var go = Instantiate(cannonballPrefab, muzzle.position, muzzle.rotation);
        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.velocity = muzzle.forward * projectileSpeed;
        // optionally: add some spread
    }

    // 외부에서 호출 -> 과열 시작
    public void StartOverheat()
    {
        if (isOverheated) return;
        isOverheated = true;

        // 시각 효과: 머티리얼 변경
        if (visualRenderer && overheatMaterial) visualRenderer.material = overheatMaterial;

        // 파티클
        if (overheatEffect) overheatEffect.Play();

        // stop firing (FireLoop reads isOverheated)
        if (firing != null) StopCoroutine(firing);
    }
}
