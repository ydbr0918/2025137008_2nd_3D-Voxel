using UnityEngine;

public class IndicatorLamp : MonoBehaviour
{
    [Header("Renderer to color")]
    public Renderer lampRenderer;          // ���Ǿ� MeshRenderer
    public Color offColor = Color.gray;
    public Color onColor = Color.yellow;

    public bool IsOn { get; private set; } = false;   // �� ���� ����

    void Reset()
    {
        if (!lampRenderer) lampRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        ApplyColor(offColor);
        IsOn = false;
    }

    public void TurnOn()
    {
        ApplyColor(onColor);
        IsOn = true;
    }

    public void TurnOff()
    {
        ApplyColor(offColor);
        IsOn = false;
    }

    void ApplyColor(Color c)
    {
        if (!lampRenderer) return;
        var mat = lampRenderer.material;   // �ν��Ͻ�ȭ
        mat.color = c;
    }
}
