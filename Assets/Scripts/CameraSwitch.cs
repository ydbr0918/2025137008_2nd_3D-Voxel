using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Targets")]
    public Transform player;          // �÷��̾� Transform
    public Transform head;            // �÷��̾� �Ӹ�(��) ��ġ(������ eyeHeight ���)

    [Header("Third-Person (Quarter View)")]
    public Vector3 quarterOffset = new Vector3(0f, 6f, -8f);  // ���� ���� ������
    public Vector3 quarterEuler = new Vector3(45f, 45f, 0f);  // ���ͺ� ���� ����

    [Header("First-Person")]
    public float eyeHeight = 1.6f;    // head�� ���� �� ������
    [Range(-89f, 89f)] public float fpPitch = 0f; // 1��Ī ���� ��(���콺�� ����)

    [Header("Common")]
    public float posSmooth = 10f;
    public float rotSmooth = 12f;

    [Header("(Optional) Hide Mesh in FP")]
    public Renderer[] hideInFirstPerson; // 1��Ī���� ������ �ϴ� �Ӹ�/�� �޽�

    bool isFirstPerson = false;
    public bool IsFirstPerson => isFirstPerson;  // �ܺο��� �б�

    Vector3 targetPos;
    Quaternion targetRot;

    void Start()
    {
        // ������ 3��Ī ���ͺ�
        isFirstPerson = false;
        ApplyVisibility();
        // �ʱ� ��ġ/ȸ�� ����
        targetPos = CalcQuarterPos();
        targetRot = Quaternion.Euler(quarterEuler);
        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            ApplyVisibility();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        if (isFirstPerson)
        {
            // 1��Ī: �� ��ġ�� ����
            Vector3 eyePos = head != null ? head.position : (player.position + Vector3.up * eyeHeight);
            targetPos = eyePos;

            // �÷��̾ ���� ����(���� yaw)�� ������, pitch�� fpPitch ���
            Vector3 forward = player.forward;
            forward.y = 0f;
            forward.Normalize();
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;

            float yaw = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
            targetRot = Quaternion.Euler(fpPitch, yaw, 0f);
        }
        else
        {
            // 3��Ī: ���ͺ� ������ ���� + �÷��̾ ���� �̵�
            targetPos = CalcQuarterPos();
            targetRot = Quaternion.Euler(quarterEuler);
        }

        // �ε巴�� ����
        transform.position = Vector3.Lerp(transform.position, targetPos, posSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSmooth * Time.deltaTime);
    }

    Vector3 CalcQuarterPos()
    {
        // quarterOffset�� "���� ����" ���������� ó��(���� ����).
        // �÷��̾� ��ġ�� ���󰡰�, ī�޶� ������ quarterEuler�� ���� ����.
        return player.position + quarterOffset;
    }

    void ApplyVisibility()
    {
        if (hideInFirstPerson == null) return;
        foreach (var r in hideInFirstPerson)
        {
            if (r != null) r.enabled = !isFirstPerson; // 1��Ī�� �� ����
        }
    }
}
