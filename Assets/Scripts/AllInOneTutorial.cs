using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// �ٿ��ֱ⸸���� �����ϴ� ���ο� Ʃ�丮��.
/// �� GameObject�� ���̸�:
/// - ���� ȭ�� ���̵�
/// - �ϴ� Ʃ�丮�� ��(����/������) �ڵ� ����
/// - �ܰ躰 �Է� ���� (WASD �� V �� Shift �� F �� LMB)
/// - ������ ���� 3�� �� �����
/// - ESC �Ͻ�����(���/������)
/// </summary>
public class AllInOneTutorial : MonoBehaviour
{
    [Header("Optional: �ѱ� TMP ��Ʈ (��� ����)")]
    public TMP_FontAsset koreanFont;

    [Header("��Ÿ��")]
    public float overlayFade = 1.0f;     // ���� ȭ�� ���̵� �ð�
    public float barFade = 0.35f;        // �� ���̵� �ð�
    public float typeCPS = 40f;          // �ʴ� Ÿ�ڼ�

    // ���� ������ ���۷�����
    Canvas _canvas;
    CanvasScaler _scaler;
    GameObject _eventSystem;

    // Overlay
    CanvasGroup _overlay;

    // Bottom bar
    CanvasGroup _bar;
    Image _icon;
    TMP_Text _txt;

    // Pause
    GameObject _pausePanel;
    bool _paused = false;
    bool _canPause = false;

    // ���� ����
    int _step = -1;
    bool _runningStep = false;

    void Awake()
    {
        EnsureCanvasAndEventSystem();
        BuildOverlay();
        BuildBottomBar();
        BuildPauseUI();
    }

    void Start()
    {
        StartCoroutine(FadeInThenRun());
    }

    void Update()
    {
        if (_canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_paused) Resume(); else Pause();
        }
    }

    // ====== UI ���� ======
    void EnsureCanvasAndEventSystem()
    {
        _canvas = FindObjectOfType<Canvas>();
        if (_canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _scaler = go.GetComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            _scaler = _canvas.GetComponent<CanvasScaler>();
            if (_scaler == null) _scaler = _canvas.gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920, 1080);
            _scaler.matchWidthOrHeight = 0.5f;
        }

        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            _eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }
    }

    void BuildOverlay()
    {
        var go = new GameObject("FullScreenOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(_canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 1);

        _overlay = go.GetComponent<CanvasGroup>();
        _overlay.alpha = 1f;
        _overlay.interactable = true;
        _overlay.blocksRaycasts = true;
    }

    void BuildBottomBar()
    {
        var bar = new GameObject("TutorialBottomBar",
            typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(HorizontalLayoutGroup));
        bar.transform.SetParent(_canvas.transform, false);

        // (�߰�) �׻� �ֻ�� ������
        bar.transform.SetAsLastSibling();

        var rt = bar.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(40, 24);          // Left, Bottom
        rt.offsetMax = new Vector2(-40, 24 + 120);   // Right, Top(=Bottom+Height)

        var img = bar.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.63f);
        img.raycastTarget = false;

        var h = bar.GetComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(16, 16, 12, 12);
        h.spacing = 16;
        h.childAlignment = TextAnchor.MiddleLeft;

        // (�ٽ�) �ؽ�Ʈ�� �������� ����������
        h.childControlWidth = true;         // �� ����: true
        h.childControlHeight = false;
        h.childForceExpandWidth = true;     // �� ����: true
        h.childForceExpandHeight = false;

        _bar = bar.GetComponent<CanvasGroup>();
        _bar.alpha = 0f;
        _bar.interactable = false;
        _bar.blocksRaycasts = false;

        // ������
        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconGO.transform.SetParent(bar.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(96, 96);
        _icon = iconGO.GetComponent<Image>();
        _icon.raycastTarget = false;

        // (�ٽ�) �������� ������, �ؽ�Ʈ�� ������ �Ե���
        var iconLE = iconGO.GetComponent<LayoutElement>();
        iconLE.minWidth = 96; iconLE.preferredWidth = 96;
        iconLE.flexibleWidth = 0;

        // �ؽ�Ʈ
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textGO.transform.SetParent(bar.transform, false);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16; tmp.fontSizeMax = 36;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (koreanFont != null) tmp.font = koreanFont;

        // (�ٽ�) �ٹٲ�/�����÷ο� ����
        tmp.enableWordWrapping = false;                       // �� �� ����
        tmp.overflowMode = TextOverflowModes.Overflow;        // ��ġ�� �״��

        // (�ٽ�) �ؽ�Ʈ�� ���� ���� ���� ����
        var textLE = textGO.GetComponent<LayoutElement>();
        textLE.flexibleWidth = 1f;        // ���� �������� �ؽ�Ʈ��
        textLE.minWidth = 0;
        textLE.preferredWidth = -1;

        _txt = tmp;
    }

    void BuildPauseUI()
    {
        _pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        _pausePanel.transform.SetParent(_canvas.transform, false);
        var rt = _pausePanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 280);
        var img = _pausePanel.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        _pausePanel.SetActive(false);

        // ����
        var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(_pausePanel.transform, false);
        var trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 80);
        trt.sizeDelta = new Vector2(440, 60);
        var ttxt = title.GetComponent<TextMeshProUGUI>();
        ttxt.text = "�Ͻ�����";
        ttxt.alignment = TextAlignmentOptions.Center;
        ttxt.fontSize = 36;
        if (koreanFont != null) ttxt.font = koreanFont;

        // ����ϱ� ��ư
        var cont = CreateUIButton(_pausePanel.transform, "����ϱ�", new Vector2(0, 20), OnClickContinue);
        // ������ ��ư
        var exit = CreateUIButton(_pausePanel.transform, "������", new Vector2(0, -50), OnClickExit);
    }

    Button CreateUIButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(360, 52);
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = new Color(1, 1, 1, 0.08f);

        var b = go.GetComponent<Button>();
        b.onClick.AddListener(onClick);

        var tgo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        var tmp = tgo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        if (koreanFont != null) tmp.font = koreanFont;

        return b;
    }

    // ====== �帧 ======
    IEnumerator FadeInThenRun()
    {
        // ���� ȭ�� ��� ����
        yield return new WaitForSeconds(0.2f);

        // ���̵� �ƿ�
        float t = 0f;
        while (t < overlayFade)
        {
            t += Time.deltaTime;
            _overlay.alpha = Mathf.Lerp(1f, 0f, t / overlayFade);
            yield return null;
        }
        _overlay.alpha = 0f;
        _overlay.interactable = false;
        _overlay.blocksRaycasts = false;

        // ù ���� ����
        NextStep();
    }

    void NextStep()
    {
        _step++;
        StartCoroutine(RunStep(_step));
    }

    IEnumerator RunStep(int s)
    {
        _runningStep = false;

        switch (s)
        {
            case 0: // ȯ��
                yield return FadeBar(1f);
                yield return TypeText("Ʃ�丮�� �°� ȯ����.");
                yield return WaitForClick();
                break;

            case 1: // ���� �ȳ�
                yield return TypeText("������ �÷����ϱ����� �����ϰ� ���۹��� �˾ƺ���.");
                yield return WaitForClick();
                // �������ʹ� �� ����� ���� �Է� ��ٸ�
                yield return FadeBar(0f);
                break;

            case 2: // WASD
                // �����ְ� ���� ���� �Է� ���
                yield return FadeBar(1f);
                yield return TypeText("WASD�� ���� �÷��̾ �����غ���.");
                yield return FadeBar(0f);
                yield return WaitForWASD();
                yield return FadeBar(1f);
                yield return TypeText("���߾�. ���� V�� ���� ������ �����غ���.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 3: // V
                yield return FadeBar(1f);
                yield return TypeText("V�� ���� 1��3��Ī�� �ٲ㺸��.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.V);
                yield return FadeBar(1f);
                yield return TypeText("����! ���� Shift�� �޷�����.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 4: // Shift �޸���
                yield return FadeBar(1f);
                yield return TypeText("Shift�� ���� �޷�����.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.LeftShift);
                yield return FadeBar(1f);
                yield return TypeText("����, ���� F�� ���� �ֿ�����.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 5: // F�� �ݱ�(������ F �Է����� ó��)
                yield return FadeBar(1f);
                yield return TypeText("F�� ���� �� �������� �ֿ�����.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.F);
                yield return FadeBar(1f);
                yield return TypeText("����! ��Ŭ������ ����غ�.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 6: // ���
                yield return FadeBar(1f);
                yield return TypeText("��Ŭ��(LMB)���� ���!");
                yield return FadeBar(0f);
                yield return WaitForLeftClick();
                yield return FadeBar(1f);
                yield return TypeText("�Ϻ���!");
                yield return new WaitForSeconds(0.5f);
                break;

            case 7: // ������ �ȳ� 3��
                yield return TypeText("���� �����ص� �� �� ����.");
                yield return new WaitForSeconds(3f);
                yield return FadeBar(0f);
                _canPause = true; // ���� ESC ����
                break;

            default:
                // ����
                yield break;
        }

        NextStep();
    }

    // ====== ��ƿ ======
    IEnumerator FadeBar(float target)
    {
        // (�߰�) �׻� �ٸ� �� ����
        _bar.transform.SetAsLastSibling();

        float a0 = _bar.alpha;
        _bar.interactable = false; _bar.blocksRaycasts = false;
        float t = 0f;
        while (t < barFade)
        {
            t += Time.deltaTime;
            _bar.alpha = Mathf.Lerp(a0, target, t / barFade);
            yield return null;
        }
        _bar.alpha = target;
        bool on = target > 0.99f;
        _bar.interactable = on; _bar.blocksRaycasts = on;
    }


    IEnumerator TypeText(string line)
    {
        if (_txt == null) yield break;
        if (typeCPS <= 0f)
        {
            _txt.text = line; yield break;
        }

        _txt.text = "";
        float t = 0f; int shown = 0; float cps = Mathf.Max(1f, typeCPS);
        while (shown < line.Length)
        {
            t += Time.deltaTime * cps;
            int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, line.Length);
            if (target != shown)
            {
                shown = target;
                _txt.text = line.Substring(0, shown);
            }
            if (Input.GetMouseButtonDown(0))
            {
                _txt.text = line; break;
            }
            yield return null;
        }
        _txt.text = line;
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
    }

    IEnumerator WaitForWASD()
    {
        while (true)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && (kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
                               kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame))
                yield break;
#else
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                yield break;
#endif
            yield return null;
        }
    }

    IEnumerator WaitForLeftClick()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
    }

    IEnumerator WaitForKeyDown(KeyCode key)
    {
        while (!Input.GetKeyDown(key)) yield return null;
    }

    // ====== �Ͻ����� ======
    void Pause()
    {
        _paused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Resume()
    {
        _paused = false;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnClickContinue() => Resume();

    void OnClickExit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ���θ޴��� ������ �ٲټ���. ������ ����.
        if (HasScene("MainMenu")) SceneManager.LoadScene("MainMenu");
        else Application.Quit();
#endif
    }

    bool HasScene(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var sn = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sn == name) return true;
        }
        return false;
    }
}
