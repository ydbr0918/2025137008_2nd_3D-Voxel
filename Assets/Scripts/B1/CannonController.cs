using System.Collections;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("Fire")]
    public Transform muzzle;                // źȯ�� ���� ��ġ(�� transform)
    public GameObject cannonballPrefab;     // CannonBall ������
    public float projectileSpeed = 20f;
    public float fireIntervalMin = 0.6f;
    public float fireIntervalMax = 2f;

    [Header("Overheat / Visual")]
    public Material normalMaterial;
    public Material overheatMaterial;
    public Renderer visualRenderer;         // ��Ƽ���� �ٲ� ������ (ĳ�� ��)
    public ParticleSystem overheatEffect;   // ������ ��ƼŬ (����)

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

    // �ܺο��� ȣ�� -> ���� ����
    public void StartOverheat()
    {
        if (isOverheated) return;
        isOverheated = true;

        // �ð� ȿ��: ��Ƽ���� ����
        if (visualRenderer && overheatMaterial) visualRenderer.material = overheatMaterial;

        // ��ƼŬ
        if (overheatEffect) overheatEffect.Play();

        // stop firing (FireLoop reads isOverheated)
        if (firing != null) StopCoroutine(firing);
    }
}
