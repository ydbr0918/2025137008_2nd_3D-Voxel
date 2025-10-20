using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class TutorialController : MonoBehaviour
{
    [Header("Overlay (���� ȭ��)")]
    public CanvasGroup overlay;        // ȭ�� ��ü�� ���� Image + CanvasGroup (Alpha 1�� ����)
    public float overlayFadeSeconds = 3f;

    [Header("Text UI (�װ� ���� �ؽ�Ʈ ����)")]
    public TutorialUI ui;              // ������ ���� TutorialUI (textBox/textField �����ص�)
    public float uiFadeSeconds = 0.35f;

    [Header("�� ������")]
    public PickupItem gunItem;         // �� ������(Trigger). ���� �پ�����
    public Transform rightHand;        // ������ Transform(attach��) - PickupItem���� ���� �� ���� ��

    [Header("�Ͻ����� UI")]
    public GameObject pausePanel;      // ESC ������ �� ���� �г�(��Ȱ������ ����)
    public string mainMenuScene = "MainMenu";

    bool paused = false;
    bool canPause = false;

    // WASD �� Ű 1ȸ�� �������� ����
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
        // �ʱ�ȭ
        if (overlay) { overlay.alpha = 1f; overlay.interactable = true; overlay.blocksRaycasts = true; }
        if (pausePanel) pausePanel.SetActive(false);

        // �� ������ �Ϸ� �ݹ� ����
        if (gunItem)
        {
            if (rightHand) { gunItem.attachParent = rightHand; } // �ν����Ϳ��� �̹� �־����� ����
            gunItem.SetOnPicked(() => { picked = true; });
        }

        StartCoroutine(Run());
    }

    void Update()
    {
        // ESC �Ͻ�����
        if (canPause && Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused) Resume();
            else Pause();
        }

        // �Է� ����
        if (step == Step.MoveWASD || step > Step.MoveWASD)
        {
            if (Input.GetKeyDown(KeyCode.W)) w = true;
            if (Input.GetKeyDown(KeyCode.A)) a = true;
            if (Input.GetKeyDown(KeyCode.S)) s = true;
            if (Input.GetKeyDown(KeyCode.D)) d = true;
        }
        if (step == Step.SwitchView || step > Step.SwitchView)
        {
            if (Input.GetKeyDown(KeyCode.V)) viewSwitched = true; // ���� ������ȯ ��ũ��Ʈ�� ���� �����ϵ���
        }
        if (step == Step.Sprint || step > Step.Sprint)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) sprinted = true; // ���� �޸���� ĳ����Ʈ�ѷ��� ó��
        }
        if (step == Step.Shoot || step > Step.Shoot)
        {
            if (Input.GetMouseButtonDown(0)) shot = true;
        }
    }

    IEnumerator Run()
    {
        // 1) ���� ȭ�� ������ ����� (3��)
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

        // 2) "Ʃ�丮�� �°� ȯ����."
        step = Step.Welcome;
        yield return ui.ShowText("Ʃ�丮�� �°� ȯ����.", uiFadeSeconds);
        // Ŭ������ ��� ����(���ϸ� WaitForClick �ڷ�ƾ�� ����� ��� ����)
        yield return WaitForClick();

        // 3) "WASD�� ����..."
        step = Step.MoveWASD;
        yield return ui.ShowText("WASD�� ���� �÷��̾ �����غ���.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds); // ���� ����� ���� �Է� ���
        w = a = s = d = false;
        while (!(w && a && s && d)) yield return null;

        // 4) "V�� ���� ������..."
        step = Step.SwitchView;
        yield return ui.ShowText("V�� ���� 1��3��Ī�� �ٲ㺸��.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        viewSwitched = false;
        while (!viewSwitched) yield return null;

        // 5) "Shift�� ���� �޷�����."
        step = Step.Sprint;
        yield return ui.ShowText("Shift�� ���� �޷�����.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        sprinted = false;
        while (!sprinted) yield return null;

        // 6) "F�� ���� �� �������� �ֿ�����." + [F] ȹ�� ������Ʈ�� PickupItem�� ǥ��
        step = Step.PickupGun;
        yield return ui.ShowText("F�� ���� �� �������� �ֿ�����.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        picked = false;
        while (!picked) yield return null;  // PickupItem���� picked=true

        // 7) "��Ŭ��(LMB)���� ����غ�."
        step = Step.Shoot;
        yield return ui.ShowText("��Ŭ��(LMB)���� ����غ�.", uiFadeSeconds);
        yield return ui.HideText(uiFadeSeconds);
        shot = false;
        while (!shot) yield return null;

        // 8) ������ �佺Ʈ "���� �����ص� �� �� ����." 3��
        step = Step.FinalToast;
        yield return ui.ShowText("���� �����ص� �� �� ����.", uiFadeSeconds);
        yield return new WaitForSeconds(3f);
        yield return ui.HideText(uiFadeSeconds);

        // ��. ���� ESC�� �Ͻ����� ����
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
