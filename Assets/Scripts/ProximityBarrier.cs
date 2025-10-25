using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class ProximityBarrier : MonoBehaviour
{
    [Header("Player")]
    public string playerTag = "Player";
    Transform player;

    [Header("Distances")]
    public float nearDistance = 1.5f;   // �� �Ÿ� �̳��� ���� �Ӱ�
    public float farDistance = 4.0f;   // �� �Ÿ� ���̸� ���� �� ����
    public float smooth = 10f;          // �ε巴�� ����

    [Header("Colors / Alpha")]
    public Color farColor = new Color(1, 1, 1, 0.12f);     // �ָ��� ���� ����
    public Color nearColor = new Color(1, 0.2f, 0.2f, 0.55f); // �����̼� �Ӱ� ���ϰ�

    [Header("Emission (����)")]
    public bool useEmission = true;
    public float farEmission = 0f;
    public float nearEmission = 2.0f;   // �����̼� ��¦ ����

    Renderer rend;
    Material matInstance;     // �� ������ �ν��Ͻ�
    Collider col;
    float lerpT;              // 0(�ָ�)~1(������)

    void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        // �� ������Ʈ�� ���� ��Ƽ���� �ν��Ͻ� ����
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

        // �÷��̾�� ���� '���� ����� ��' �Ÿ�
        Vector3 closest = col.ClosestPoint(player.position);
        float dist = Vector3.Distance(player.position, closest);

        // �Ÿ���0..1�� ��ȯ (farDistance���� 0, nearDistance���� 1)
        float targetT = Mathf.InverseLerp(farDistance, nearDistance, dist);
        lerpT = Mathf.Lerp(lerpT, targetT, 1f - Mathf.Exp(-smooth * Time.deltaTime)); // �ε巯�� ��������

        Apply(lerpT);
    }

    void Apply(float t)
    {
        if (!matInstance) return;

        // ��/���� ����
        Color c = Color.Lerp(farColor, nearColor, t);
        matInstance.SetColor("_BaseColor", c);

        // ���̼� ���� (����)
        if (useEmission)
        {
            float e = Mathf.Lerp(farEmission, nearEmission, t);
            // URP Lit�� Emission Color * Intensity ����
            Color ec = new Color(1f, 0.2f, 0.2f) * e; // ���� ��
            matInstance.SetColor("_EmissionColor", ec);
            if (e > 0.01f) matInstance.EnableKeyword("_EMISSION");
            else matInstance.DisableKeyword("_EMISSION");
        }
    }
}

