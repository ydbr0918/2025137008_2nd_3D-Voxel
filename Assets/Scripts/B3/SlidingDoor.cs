using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Target")]
    public Transform doorPanel;                 // 실제로 움직일 패널(없으면 자기 자신)
    [Header("Positions (Local)")]
    public Vector3 openLocalPos;                // 열림 위치 (로컬)
    public Vector3 closedLocalPos;              // 닫힘 위치 (로컬)
    [Header("Motion")]
    public float moveSeconds = 0.6f;            // 닫히는 시간
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Header("State")]
    public bool startOpened = true;

    Transform t;
    Coroutine co;

    void Awake()
    {
        t = doorPanel ? doorPanel : transform;
        t.localPosition = startOpened ? openLocalPos : closedLocalPos;
    }

    public void Open()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(MoveTo(openLocalPos));
    }

    public void Close()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(MoveTo(closedLocalPos));
    }

    public void Toggle()
    {
        var target = Vector3.Distance((doorPanel ? doorPanel : transform).localPosition, closedLocalPos) < 0.001f
            ? openLocalPos : closedLocalPos;
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(MoveTo(target));
    }

    IEnumerator MoveTo(Vector3 targetLocal)
    {
        var tr = doorPanel ? doorPanel : transform;
        Vector3 from = tr.localPosition;
        float t0 = 0f;
        while (t0 < moveSeconds)
        {
            t0 += Time.deltaTime;
            float k = curve.Evaluate(Mathf.Clamp01(t0 / moveSeconds));
            tr.localPosition = Vector3.LerpUnclamped(from, targetLocal, k);
            yield return null;
        }
        tr.localPosition = targetLocal;
        co = null;
    }
}
