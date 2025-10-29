using UnityEngine;

public class IndicatorCube : MonoBehaviour
{
    [Header("Target")]
    public Renderer target;                  // 비우면 GetComponent<Renderer>()
    [Header("Colors")]
    public Color offColor = new Color(0.8f, 0.1f, 0.1f);  // 빨강
    public Color onColor = new Color(0.1f, 0.8f, 0.1f);  // 초록
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
        // material 은 이 오브젝트용 인스턴스를 가져오므로 색상 공유 이슈 없음
        target.material.color = isOn ? onColor : offColor;
    }
}
