using UnityEngine;

public class ElevatorManager : MonoBehaviour
{
    [Header("Indicator Cubes (1:1 ����)")]
    public GameObject[] indicatorCubes;   // size=3, 0:1����, 1:2����, 2:3����

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

        // ���� �� ���� OFF������ �ʱ�ȭ
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

        // ��Ƽ���� �ν��Ͻ��� �����Ǿ� �ִٸ� ���� ���� �ٲ�Ƿ�, �� ť�꿡 ���� ��Ƽ���� ��� ����
        r.material.color = on ? onColor : offColor;
    }
}
