using UnityEngine;

public class IndicatorCube : MonoBehaviour
{
    [Header("Target")]
    public Renderer target;                  // ���� GetComponent<Renderer>()
    [Header("Colors")]
    public Color offColor = new Color(0.8f, 0.1f, 0.1f);  // ����
    public Color onColor = new Color(0.1f, 0.8f, 0.1f);  // �ʷ�
    [Header("State")]
    public bool isOn = false;

    void Awake()
    {
        if (!target) target = GetComponent<Renderer>();
        Apply();
    }

    public void TurnOn() { isOn = true; Apply(); }
    public void TurnOff() { isOn = false; Apply(); }
    public void Toggle() { isOn = !isOn; Apply(); }

    void Apply()
    {
        if (!target) return;
        // material �� �� ������Ʈ�� �ν��Ͻ��� �������Ƿ� ���� ���� �̽� ����
        target.material.color = isOn ? onColor : offColor;
    }
}
