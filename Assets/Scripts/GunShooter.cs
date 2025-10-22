using UnityEngine;

public class GunShooter : MonoBehaviour
{
    [Header("총 관련 설정")]
    public Transform muzzle;            // 총구 위치
    public Bullet bulletPrefab;         // 발사할 Bullet 프리팹
    public Transform shootOwner;        // 플레이어 Transform (충돌 무시용)
    public float fireRate = 8f;         // 초당 연사 속도
    public float bulletSpeed = 40f;     // 총알 속도

    [Header("이펙트 (선택사항)")]
    public AudioSource fireSfx;         // 총소리 (없으면 비워둬도 됨)
    public ParticleSystem muzzleFx;     // 총구 섬광 (없으면 비워둬도 됨)

    private float _cooldown = 0f;

    // ⚠️ 핵심: 처음엔 비활성화 상태로 시작 (Inspector에서 체크 끄기)
    // 총을 주운 후에만 코드가 동작함

    void Update()
    {
        _cooldown -= Time.deltaTime;

        // 좌클릭 입력
        if (Input.GetMouseButton(0) && _cooldown <= 0f)
        {
            Fire();
            _cooldown = 1f / fireRate;
        }
    }

    public void Fire()
    {
        if (!bulletPrefab || !muzzle) return;

        // 총알 생성
        var bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        Vector3 dir = GetAimDirection();
        bullet.Fire(dir, bulletSpeed);

        // 플레이어와 충돌 무시 (자기 자신 총알 안 맞게)
        if (shootOwner)
        {
            foreach (var bc in bullet.GetComponentsInChildren<Collider>())
                foreach (var oc in shootOwner.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(bc, oc, true);
        }

        // 이펙트
        if (muzzleFx) muzzleFx.Play();
        if (fireSfx) fireSfx.Play();
    }

    private Vector3 GetAimDirection()
    {
        var cam = Camera.main;
        if (cam)
        {
            // 카메라 중앙에서 레이캐스트 → 조준점 방향으로 발사
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                return (hit.point - muzzle.position).normalized;

            return cam.transform.forward;
        }
        return muzzle.forward;
    }
}
