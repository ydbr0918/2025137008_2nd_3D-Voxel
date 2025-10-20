using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// 붙여넣기만으로 동작하는 올인원 튜토리얼.
/// 빈 GameObject에 붙이면:
/// - 검은 화면 페이드
/// - 하단 튜토리얼 바(문구/아이콘) 자동 생성
/// - 단계별 입력 유도 (WASD → V → Shift → F → LMB)
/// - 마지막 문구 3초 후 사라짐
/// - ESC 일시정지(계속/나가기)
/// </summary>
public class AllInOneTutorial : MonoBehaviour
{
    [Header("Optional: 한글 TMP 폰트 (없어도 동작)")]
    public TMP_FontAsset koreanFont;

    [Header("스타일")]
    public float overlayFade = 1.0f;     // 검은 화면 페이드 시간
    public float barFade = 0.35f;        // 바 페이드 시간
    public float typeCPS = 40f;          // 초당 타자수

    // 내부 생성된 레퍼런스들
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

    // 진행 상태
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

    // ====== UI 빌드 ======
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

        // (추가) 항상 최상단 렌더링
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

        // (핵심) 텍스트가 가로폭을 가져가도록
        h.childControlWidth = true;         // ← 변경: true
        h.childControlHeight = false;
        h.childForceExpandWidth = true;     // ← 변경: true
        h.childForceExpandHeight = false;

        _bar = bar.GetComponent<CanvasGroup>();
        _bar.alpha = 0f;
        _bar.interactable = false;
        _bar.blocksRaycasts = false;

        // 아이콘
        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconGO.transform.SetParent(bar.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(96, 96);
        _icon = iconGO.GetComponent<Image>();
        _icon.raycastTarget = false;

        // (핵심) 아이콘은 고정폭, 텍스트가 공간을 먹도록
        var iconLE = iconGO.GetComponent<LayoutElement>();
        iconLE.minWidth = 96; iconLE.preferredWidth = 96;
        iconLE.flexibleWidth = 0;

        // 텍스트
        var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textGO.transform.SetParent(bar.transform, false);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16; tmp.fontSizeMax = 36;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (koreanFont != null) tmp.font = koreanFont;

        // (핵심) 줄바꿈/오버플로우 설정
        tmp.enableWordWrapping = false;                       // 한 줄 유지
        tmp.overflowMode = TextOverflowModes.Overflow;        // 넘치면 그대로

        // (핵심) 텍스트가 남은 폭을 전부 차지
        var textLE = textGO.GetComponent<LayoutElement>();
        textLE.flexibleWidth = 1f;        // 남는 가로폭을 텍스트에
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

        // 제목
        var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(_pausePanel.transform, false);
        var trt = title.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 80);
        trt.sizeDelta = new Vector2(440, 60);
        var ttxt = title.GetComponent<TextMeshProUGUI>();
        ttxt.text = "일시정지";
        ttxt.alignment = TextAlignmentOptions.Center;
        ttxt.fontSize = 36;
        if (koreanFont != null) ttxt.font = koreanFont;

        // 계속하기 버튼
        var cont = CreateUIButton(_pausePanel.transform, "계속하기", new Vector2(0, 20), OnClickContinue);
        // 나가기 버튼
        var exit = CreateUIButton(_pausePanel.transform, "나가기", new Vector2(0, -50), OnClickExit);
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

    // ====== 흐름 ======
    IEnumerator FadeInThenRun()
    {
        // 검은 화면 잠깐 유지
        yield return new WaitForSeconds(0.2f);

        // 페이드 아웃
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

        // 첫 스텝 시작
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
            case 0: // 환영
                yield return FadeBar(1f);
                yield return TypeText("튜토리얼에 온걸 환영해.");
                yield return WaitForClick();
                break;

            case 1: // 사전 안내
                yield return TypeText("게임을 플레이하기전에 간단하게 조작법을 알아보자.");
                yield return WaitForClick();
                // 다음부터는 바 숨기고 실제 입력 기다림
                yield return FadeBar(0f);
                break;

            case 2: // WASD
                // 보여주고 숨긴 다음 입력 대기
                yield return FadeBar(1f);
                yield return TypeText("WASD를 눌러 플레이어를 조작해보자.");
                yield return FadeBar(0f);
                yield return WaitForWASD();
                yield return FadeBar(1f);
                yield return TypeText("잘했어. 이제 V를 눌러 시점을 변경해보자.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 3: // V
                yield return FadeBar(1f);
                yield return TypeText("V를 눌러 1·3인칭을 바꿔보자.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.V);
                yield return FadeBar(1f);
                yield return TypeText("좋아! 이제 Shift로 달려보자.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 4: // Shift 달리기
                yield return FadeBar(1f);
                yield return TypeText("Shift를 눌러 달려보자.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.LeftShift);
                yield return FadeBar(1f);
                yield return TypeText("좋아, 이제 F로 총을 주워보자.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 5: // F로 줍기(간단히 F 입력으로 처리)
                yield return FadeBar(1f);
                yield return TypeText("F를 눌러 총 아이템을 주워보자.");
                yield return FadeBar(0f);
                yield return WaitForKeyDown(KeyCode.F);
                yield return FadeBar(1f);
                yield return TypeText("좋아! 좌클릭으로 사격해봐.");
                yield return new WaitForSeconds(0.5f);
                yield return FadeBar(0f);
                break;

            case 6: // 사격
                yield return FadeBar(1f);
                yield return TypeText("좌클릭(LMB)으로 사격!");
                yield return FadeBar(0f);
                yield return WaitForLeftClick();
                yield return FadeBar(1f);
                yield return TypeText("완벽해!");
                yield return new WaitForSeconds(0.5f);
                break;

            case 7: // 마지막 안내 3초
                yield return TypeText("이제 시작해도 될 것 같아.");
                yield return new WaitForSeconds(3f);
                yield return FadeBar(0f);
                _canPause = true; // 이제 ESC 가능
                break;

            default:
                // 종료
                yield break;
        }

        NextStep();
    }

    // ====== 유틸 ======
    IEnumerator FadeBar(float target)
    {
        // (추가) 항상 바를 맨 위에
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

    // ====== 일시정지 ======
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
        // 메인메뉴가 있으면 바꾸세요. 없으면 종료.
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
