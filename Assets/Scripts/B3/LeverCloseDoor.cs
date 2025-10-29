// LeverCloseDoor.cs
// - ī�޶� �߾� ������ ������ ���� [F] ������Ʈ�� ����
//   ������ �����̸� ������, ���� �ݴ´�(1ȸ��).
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class LeverCloseDoor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;                       // ���� Camera.main
    public float maxDistance = 3.0f;
    public LayerMask hitMask = ~0;           // Interactable ���̾ ����
    public float aimRadius = 0.1f;           // ���Ǿ�ĳ��Ʈ �ݰ�(���� ��ȭ)
    public float holdGrace = 0.12f;          // �� ������ ������ ����

    [Header("Prompt")]
    public TMP_Text promptText;              // [F] ���� ������ (�� ������Ʈ)
    public string promptLine = "[F] ���� ������";

    [Header("Lever Visual")]
    public Transform handle;                 // ������(�ڽ� Transform)
    public Vector3 upLocalEuler = new Vector3(-45, 0, 0);
    public Vector3 downLocalEuler = new Vector3(45, 0, 0);
    public float pullSeconds = 0.25f;
    public AnimationCurve pullCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Door To Close")]
    public SlidingDoor door;                 // ���� ��

    [Header("SFX (optional)")]
    public AudioSource sfx;
    public AudioClip pullClip;
    public AudioClip doorCloseClip;

    [Header("Misc")]
    public KeyCode useKey = KeyCode.F;
    public bool oneShot = true;              // �� ���� �۵�

    bool used = false;
    bool looking = false;
    float lastSeen = -999f;

    public bool Activated { get; private set; }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (promptText) promptText.gameObject.SetActive(false);
        if (handle) handle.localEulerAngles = upLocalEuler;
    }

    void Update()
    {
        UpdateAim();

        if (!used && looking && Input.GetKeyDown(useKey))
            StartCoroutine(DoPull());
    }

    void UpdateAim()
    {
        bool seen = false;

        if (cam)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.SphereCast(ray, aimRadius, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Collide))
            {
                var me = hit.collider.GetComponentInParent<LeverCloseDoor>();
                if (me == this)
                {
                    seen = true;
                    lastSeen = Time.time;
                }
            }
        }

        bool shouldShow = (seen || Time.time - lastSeen <= holdGrace) && !used;

        looking = shouldShow;

        if (promptText)
        {
            if (shouldShow)
            {
                if (!promptText.gameObject.activeSelf) promptText.gameObject.SetActive(true);
                promptText.text = promptLine;
            }
            else
            {
                if (promptText.gameObject.activeSelf) promptText.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator DoPull()
    {
        Activated = true;
        used = oneShot ? true : used;
        if (promptText) promptText.gameObject.SetActive(false);
        if (sfx && pullClip) sfx.PlayOneShot(pullClip);
        if (handle)
        {
            Quaternion from = Quaternion.Euler(upLocalEuler);
            Quaternion to = Quaternion.Euler(downLocalEuler);
            float t = 0f;
            while (t < pullSeconds)
            {
                t += Time.deltaTime;
                float k = pullCurve.Evaluate(Mathf.Clamp01(t / pullSeconds));
                handle.localRotation = Quaternion.SlerpUnclamped(from, to, k);
                yield return null;
            }
            handle.localRotation = to;
        }
        if (door) door.Close();
        if (sfx && doorCloseClip) sfx.PlayOneShot(doorCloseClip);
    }
}
