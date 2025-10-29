using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public int maxHearts = 2;
    public int currentHearts = 2;
    public float height = 2f;
    public float spacing = 0.35f;
    public bool billboard = true;

    readonly List<GameObject> hearts = new List<GameObject>();
    Transform cam;

    void Awake()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        Build();
        ApplyCount();
    }

    void LateUpdate()
    {
        Vector3 p = transform.parent != null ? transform.parent.position : transform.position;
        transform.position = p + Vector3.up * height;
        if (billboard && cam != null)
        {
            Vector3 dir = transform.position - cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    void Build()
    {
        Clear();
        if (heartPrefab == null) return;
        float total = (maxHearts - 1) * spacing;
        float start = -total * 0.5f;
        for (int i = 0; i < maxHearts; i++)
        {
            Vector3 local = new Vector3(start + spacing * i, 0f, 0f);
            GameObject h = Instantiate(heartPrefab, transform);
            h.transform.localPosition = local;
            h.transform.localRotation = Quaternion.identity;
            hearts.Add(h);
        }
    }

    void Clear()
    {
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i] != null) Destroy(hearts[i]);
        }
        hearts.Clear();
    }

    void ApplyCount()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            bool on = i < Mathf.Clamp(currentHearts, 0, maxHearts);
            if (hearts[i] != null) hearts[i].SetActive(on);
        }
    }

    public void SetHearts(int count)
    {
        currentHearts = Mathf.Clamp(count, 0, maxHearts);
        ApplyCount();
    }

    public void SetMaxAndCurrent(int max, int current)
    {
        maxHearts = Mathf.Max(1, max);
        currentHearts = Mathf.Clamp(current, 0, maxHearts);
        Build();
        ApplyCount();
    }

    public void Decrease(int amount = 1)
    {
        SetHearts(currentHearts - Mathf.Max(1, amount));
    }
}
