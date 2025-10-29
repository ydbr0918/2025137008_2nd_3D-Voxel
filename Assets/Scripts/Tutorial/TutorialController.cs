using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [Header("Overlay (검은 화면)  — 비워두면 자동 탐색")]
    public CanvasGroup overlay;              // 화면을 덮는 Image+CanvasGroup (Alpha 1로 시작 권장)
    public float overlayFadeSeconds = 3f;

    [Header("Text UI (텍스트 상자) — 비워두면 자동 탐색")]
    public TutorialUI ui;                    // TutorialUI(textBox/textField 연결되어 있어야 함)
    public float uiFadeSeconds = 0.35f;

    [Header("총 아이템 (선택) — 없으면 해당 단계 건너뜀")]
    public PickupItem gunItem;               // 총 아이템(Trigger)
    public Transform rightHand;              // 붙일 손(없으면 attachParent는 기존 설정 사용)

    [Header("일시정지 UI (선택)")]
    public GameObject pausePanel;            // ESC 시 열릴 패널(비활성 시작)
    public string mainMenuScene = "MainMenu";

    bool paused = false;
    bool canPause = false;

    // 입력 플래그
    bool w, a, s, d;
    bool viewSwitched, sprinted;

    enum Step { FadeIn, Welcome, MoveWASD, SwitchView, Sprint, PickupGun, Shoot, FinalToast, Done }
    Step step = Step.FadeIn;

    void Awake()
    {
        // 자동 연결 (비어있으면 찾아보기)
        if (!overlay)
        {
            // 이름으로 먼저 찾고, 없으면 Canvas 하위의 가장 큰 CanvasGroup 중 Image가 붙은 것을 사용
            var found = GameObject.Find("Overlay");
            if (found) overlay = found.GetComponent<CanvasGroup>();
            if (!overlay)
            {
                CanvasGroup best = null;
                foreach (var cg in FindObjectsOfType<CanvasGroup>(true))
                {
                    if (!cg.GetComponent<UnityEngine.UI.Image>()) continue;
                    if (!best || Area(cg.GetComponent<RectTransform>()) > Area(best.GetComponent<RectTransform>()))
                        best = cg;
                }
                overlay = best;
            }
        }

        if (!ui) ui = FindObjectOfType<TutorialUI>(true);
        if (!gunItem) gunItem = FindObjectOfType<PickupItem>(true);
        if (!pausePanel)
        {
            var go = GameObject.Find("PausePanel");
            if (go) pausePanel = go;
        }
    }

    void Start()
    {
        // 초기 상태 셋업(있으면 쓰고, 없으면 건너뛰도록)
        if (overlay) { overlay.alpha = 1f; overlay.interactable = true; overlay.blocksRaycasts = true; }
        if (pausePanel) pausePanel.SetActive(false);

      
        StartCoroutine(Run());
    }

    void Update()
    {
        // ESC 일시정지 (패널이 있어야만 동작)
        if (pausePanel && canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }

        // 입력 추적
        if (step == Step.MoveWASD || step > Step.MoveWASD)
        {
            if (Input.GetKeyDown(KeyCode.W)) w = true;
            if (Input.GetKeyDown(KeyCode.A)) a = true;
            if (Input.GetKeyDown(KeyCode.S)) s = true;
            if (Input.GetKeyDown(KeyCode.D)) d = true;
        }
        if (step == Step.SwitchView || step > Step.SwitchView)
        {
            if (Input.GetKeyDown(KeyCode.V)) viewSwitched = true;
        }
        if (step == Step.Sprint || step > Step.Sprint)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) sprinted = true;
        }
       
    }

    IEnumerator Run()
    {
        // 필수 확인: ui 없으면 진행 불가 → 친절 로그 후 중단
        if (!ui)
        {
            Debug.LogError("[TutorialController] TutorialUI가 연결되지 않았습니다. Canvas에 TutorialUI를 추가하고 textBox/textField를 연결하세요.");
            yield break;
        }

        // 1) 검은 화면 페이드인(overlay가 없으면 스킵)
        step = Step.FadeIn;
        if (overlay)
        {
            float t = 0f;
            while (t < overlayFadeSeconds)
            {
                t += Time.deltaTime;
                overlay.alpha = Mathf.Lerp(1f, 0f, t / overlayFadeSeconds);
                yield return null;
            }
            overlay.alpha = 0f; overlay.interactable = false; overlay.blocksRaycasts = false;
        }

        // 2) 환영 문구
        step = Step.Welcome;
        yield return ui.ShowText("튜토리얼에 온걸 환영해.", uiFadeSeconds);
        yield return WaitForClick();

        // 3) WASD
        step = Step.MoveWASD;
        yield return ui.ShowText("WASD를 눌러 플레이어를 조작해보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        w = a = s = d = false;
        while (!(w && a && s && d)) yield return null;

        // 4) V 전환
        step = Step.SwitchView;
        yield return ui.ShowText("V를 눌러 1·3인칭을 바꿔보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        viewSwitched = false;
        while (!viewSwitched) yield return null;

        // 5) Shift 달리기
        step = Step.Sprint;
        yield return ui.ShowText("Shift를 눌러 달려보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        sprinted = false;
        while (!sprinted) yield return null;

     

        // 8) 마지막 토스트
        step = Step.FinalToast;
        yield return ui.ShowText("이제 시작해도 될 것 같아.", uiFadeSeconds);
        yield return new WaitForSeconds(3f);
        yield return ui.HideText(uiFadeSeconds);

        // 끝 — 이제 ESC 허용
        canPause = pausePanel != null;
        step = Step.Done;
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
    }

    // ===== 일시정지 =====
    void Pause()
    {
        if (!pausePanel) return;
        paused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void Resume()
    {
        if (!pausePanel) return;
        paused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OnClickContinue() => Resume();

    public void OnClickExit()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuScene) && HasScene(mainMenuScene))
            SceneManager.LoadScene(mainMenuScene);
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
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

    // 화면 크기 추정용
    float Area(RectTransform rt)
    {
        if (!rt) return 0f;
        var s = rt.rect.size;
        return Mathf.Abs(s.x * s.y);
    }
}
