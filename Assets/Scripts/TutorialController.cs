using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [Header("Overlay (검은 화면)")]
    public CanvasGroup overlay;        // 화면 전체를 덮는 Image + CanvasGroup (Alpha 1로 시작)
    public float overlayFadeSeconds = 3f;

    [Header("Text UI (네가 만든 텍스트 상자)")]
    public TutorialUI ui;              // 위에서 만든 TutorialUI (textBox/textField 연결해둔)
    public float uiFadeSeconds = 0.35f;

    [Header("총 아이템")]
    public PickupItem gunItem;         // 총 아이템(Trigger). 여기 붙어있음
    public Transform rightHand;        // 오른손 Transform(attach용) - PickupItem에도 같은 걸 넣을 것

    [Header("일시정지 UI")]
    public GameObject pausePanel;      // ESC 눌렀을 때 켜질 패널(비활성으로 시작)
    public string mainMenuScene = "MainMenu";

    bool paused = false;
    bool canPause = false;

    // WASD 각 키 1회씩 눌렀는지 추적
    bool w, a, s, d;
    bool viewSwitched, sprinted, shot, picked;

    enum Step
    {
        FadeIn,
        Welcome,
        MoveWASD,
        SwitchView,
        Sprint,
        PickupGun,
        Shoot,
        FinalToast,
        Done
    }
    Step step = Step.FadeIn;

    void Start()
    {
        // 초기화
        if (overlay) { overlay.alpha = 1f; overlay.interactable = true; overlay.blocksRaycasts = true; }
        if (pausePanel) pausePanel.SetActive(false);

        // 총 아이템 완료 콜백 연결
        if (gunItem)
        {
            if (rightHand) { gunItem.attachParent = rightHand; } // 인스펙터에서 이미 넣었으면 생략
            gunItem.SetOnPicked(() => { picked = true; });
        }

        StartCoroutine(Run());
    }

    void Update()
    {
        // ESC 일시정지
        if (canPause && Input.GetKeyDown(KeyCode.Escape))
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
            if (Input.GetKeyDown(KeyCode.V)) viewSwitched = true; // 실제 시점전환 스크립트도 따로 동작하도록
        }
        if (step == Step.Sprint || step > Step.Sprint)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) sprinted = true; // 실제 달리기는 캐릭컨트롤러가 처리
        }
        if (step == Step.Shoot || step > Step.Shoot)
        {
            if (Input.GetMouseButtonDown(0)) shot = true;
        }
    }

    IEnumerator Run()
    {
        // 1) 검은 화면 서서히 사라짐 (3초)
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

        // 2) "튜토리얼에 온걸 환영해."
        step = Step.Welcome;
        yield return ui.ShowText("튜토리얼에 온걸 환영해.", uiFadeSeconds);
        // 클릭으로 즉시 다음(원하면 WaitForClick 코루틴을 만들어 사용 가능)
        yield return WaitForClick();

        // 3) "WASD를 눌러..."
        step = Step.MoveWASD;
        yield return ui.ShowText("WASD를 눌러 플레이어를 조작해보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds); // 문구 숨기고 실제 입력 대기
        w = a = s = d = false;
        while (!(w && a && s && d)) yield return null;

        // 4) "V를 눌러 시점을..."
        step = Step.SwitchView;
        yield return ui.ShowText("V를 눌러 1·3인칭을 바꿔보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        viewSwitched = false;
        while (!viewSwitched) yield return null;

        // 5) "Shift를 눌러 달려보자."
        step = Step.Sprint;
        yield return ui.ShowText("Shift를 눌러 달려보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        sprinted = false;
        while (!sprinted) yield return null;

        // 6) "F를 눌러 총 아이템을 주워보자." + [F] 획득 프롬프트는 PickupItem이 표시
        step = Step.PickupGun;
        yield return ui.ShowText("F를 눌러 총 아이템을 주워보자.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        picked = false;
        while (!picked) yield return null;  // PickupItem에서 picked=true

        // 7) "좌클릭(LMB)으로 사격해봐."
        step = Step.Shoot;
        yield return ui.ShowText("좌클릭(LMB)으로 사격해봐.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        shot = false;
        while (!shot) yield return null;

        // 8) 마지막 토스트 "이제 시작해도 될 것 같아." 3초
        step = Step.FinalToast;
        yield return ui.ShowText("이제 시작해도 될 것 같아.", uiFadeSeconds);
        yield return new WaitForSeconds(3f);
        yield return ui.HideText(uiFadeSeconds);

        // 끝. 이제 ESC로 일시정지 가능
        canPause = true;
        step = Step.Done;
    }

    IEnumerator WaitForClick()
    {
        while (!Input.GetMouseButtonDown(0)) yield return null;
    }

    void Pause()
    {
        paused = true;
        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
    }
    public void Resume()
    {
        paused = false;
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;
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
}
