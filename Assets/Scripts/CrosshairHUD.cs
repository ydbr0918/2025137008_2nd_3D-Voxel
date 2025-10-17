using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairHUD : MonoBehaviour
{
    [Header("Refs")]
    public CameraSwitch camSwitch;              // 1��Ī ���� Ȯ��

    [Header("Appearance")]
    public Vector2 size = new Vector2(6f, 6f);  // �߾� �� ũ��(px)
    public Color color = Color.white;           // ����
    [Range(0f, 1f)] public float opacity = 1f;  // ����

    private GameObject canvasGO;
    private GameObject crosshairGO;

    void Awake()
    {
        if (camSwitch == null && Camera.main != null)
            camSwitch = Camera.main.GetComponent<CameraSwitch>();

        CreateUI();
        SetVisible(false); // ������ �� ���̰�(3��Ī)
    }

    void Update()
    {
        bool show = camSwitch != null && camSwitch.IsFirstPerson;
        SetVisible(show);
    }

    void CreateUI()
    {
        // Canvas ����
        canvasGO = new GameObject("CrosshairCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // �� ��

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Crosshair Image ����
        crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);

        var img = crosshairGO.AddComponent<Image>();
        img.color = new Color(color.r, color.g, color.b, opacity); // �⺻ ��������Ʈ(���׶��/�簢��) ���

        var rt = crosshairGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
    }

    void SetVisible(bool v)
    {
        if (crosshairGO != null && crosshairGO.activeSelf != v)
            crosshairGO.SetActive(v);
    }
}
