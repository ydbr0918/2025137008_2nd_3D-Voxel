using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairHUD : MonoBehaviour
{
    [Header("Refs")]
    public CameraSwitch camSwitch;              // 1인칭 여부 확인

    [Header("Appearance")]
    public Vector2 size = new Vector2(6f, 6f);  // 중앙 점 크기(px)
    public Color color = Color.white;           // 색상
    [Range(0f, 1f)] public float opacity = 1f;  // 투명도

    private GameObject canvasGO;
    private GameObject crosshairGO;

    void Awake()
    {
        if (camSwitch == null && Camera.main != null)
            camSwitch = Camera.main.GetComponent<CameraSwitch>();

        CreateUI();
        SetVisible(false); // 시작은 안 보이게(3인칭)
    }

    void Update()
    {
        bool show = camSwitch != null && camSwitch.IsFirstPerson;
        SetVisible(show);
    }

    void CreateUI()
    {
        // Canvas 생성
        canvasGO = new GameObject("CrosshairCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // 맨 위

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Crosshair Image 생성
        crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvasGO.transform, false);

        var img = crosshairGO.AddComponent<Image>();
        img.color = new Color(color.r, color.g, color.b, opacity); // 기본 스프라이트(동그라미/사각형) 사용

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
