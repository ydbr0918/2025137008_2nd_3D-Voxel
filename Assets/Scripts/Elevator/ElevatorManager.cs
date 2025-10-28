using UnityEngine;

public class ElevatorManager : MonoBehaviour
{
    [Header("Indicator Cubes (1:1 매핑)")]
    public GameObject[] indicatorCubes;   // size=3, 0:1번불, 1:2번불, 2:3번불

    [Header("Colors")]
    public Color offColor = Color.gray;
    public Color onColor = Color.yellow;

    private bool[] onStates;

    public bool AllOn
    {
        get
        {
            if (indicatorCubes == null || indicatorCubes.Length == 0) return false;
            for (int i = 0; i < indicatorCubes.Length; i++)
                if (i >= onStates.Length || !onStates[i]) return false;
            return true;
        }
    }

    void Awake()
    {
        if (indicatorCubes == null) indicatorCubes = new GameObject[0];
        onStates = new bool[indicatorCubes.Length];

        // 시작 시 전부 OFF색으로 초기화
        for (int i = 0; i < indicatorCubes.Length; i++)
            Paint(i, false);
    }

    public void SetIndicator(int indexZeroBased, bool on)
    {
        if (indexZeroBased < 0 || indexZeroBased >= indicatorCubes.Length) return;
        onStates[indexZeroBased] = on;
        Paint(indexZeroBased, on);
    }

    private void Paint(int idx, bool on)
    {
        var go = indicatorCubes[idx];
        if (!go) return;
        var r = go.GetComponent<MeshRenderer>();
        if (!r) return;

        // 머티리얼 인스턴스가 공유되어 있다면 색이 같이 바뀌므로, 각 큐브에 개별 머티리얼 사용 권장
        r.material.color = on ? onColor : offColor;
    }
}
