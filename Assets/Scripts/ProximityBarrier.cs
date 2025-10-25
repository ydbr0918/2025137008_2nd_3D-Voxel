using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ProximityBarrier : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";
    Transform player;

    [Header("Distances")]
    public float nearDistance = 1.5f;   // 이 거리 이내면 가장 붉게
    public float farDistance = 4.0f;   // 이 거리 밖이면 거의 안 보임
    public float smooth = 10f;          // 부드럽게 보간

    [Header("Colors / Alpha")]
    public Color farColor = new Color(1, 1, 1, 0.12f);     // 멀리서 거의 투명
    public Color nearColor = new Color(1, 0.2f, 0.2f, 0.55f); // 가까이서 붉고 진하게

    [Header("Emission (선택)")]
    public bool useEmission = true;
    public float farEmission = 0f;
    public float nearEmission = 2.0f;   // 가까이서 살짝 빛남

    Renderer rend;
    Material matInstance;     // 이 벽만의 인스턴스
    Collider col;
    float lerpT;              // 0(멀리)~1(가까이)

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        // 이 오브젝트만 쓰는 머티리얼 인스턴스 생성
        matInstance = rend.material;
        Apply(0f);
    }

    void OnDestroy()
    {
        if (Application.isPlaying && matInstance) Destroy(matInstance);
    }

    void Update()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p) player = p.transform; else return;
        }

        // 플레이어와 벽의 '가장 가까운 점' 거리
        Vector3 closest = col.ClosestPoint(player.position);
        float dist = Vector3.Distance(player.position, closest);

        // 거리→0..1로 변환 (farDistance에서 0, nearDistance에서 1)
        float targetT = Mathf.InverseLerp(farDistance, nearDistance, dist);
        lerpT = Mathf.Lerp(lerpT, targetT, 1f - Mathf.Exp(-smooth * Time.deltaTime)); // 부드러운 지수보간

        Apply(lerpT);
    }

    void Apply(float t)
    {
        if (!matInstance) return;

        // 색/투명도 보간
        Color c = Color.Lerp(farColor, nearColor, t);
        matInstance.SetColor("_BaseColor", c);

        // 에미션 보간 (선택)
        if (useEmission)
        {
            float e = Mathf.Lerp(farEmission, nearEmission, t);
            // URP Lit는 Emission Color * Intensity 형태
            Color ec = new Color(1f, 0.2f, 0.2f) * e; // 붉은 빛
            matInstance.SetColor("_EmissionColor", ec);
            if (e > 0.01f) matInstance.EnableKeyword("_EMISSION");
            else matInstance.DisableKeyword("_EMISSION");
        }
    }
}

