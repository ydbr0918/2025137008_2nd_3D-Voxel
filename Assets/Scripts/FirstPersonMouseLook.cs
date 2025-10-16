using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMouseLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;        // Yaw(�¿� ȸ��)�� ������ ���(�÷��̾�)
    public CameraSwitch camSwitch;  // 1��Ī ���� & fpPitch ����

    [Header("Mouse")]
    public float mouseSensitivity = 120f; // ����(��/�� ����)
    public float minPitch = -80f;
    public float maxPitch = 80f;

    void Awake()
    {
        if (camSwitch == null && Camera.main != null)
            camSwitch = Camera.main.GetComponent<CameraSwitch>();
    }

    void Update()
    {
        if (camSwitch == null || player == null) return;

        if (camSwitch.IsFirstPerson)
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            // �¿�(Yaw) �� �÷��̾ ����
            float yawDelta = mx * mouseSensitivity * Time.deltaTime;
            player.Rotate(0f, yawDelta, 0f, Space.World);

            // ����(Pitch) �� ī�޶� ��Ʈ�ѷ��� fpPitch�� ����(Ŭ����)
            float pitchDelta = -my * mouseSensitivity * Time.deltaTime;
            camSwitch.fpPitch = Mathf.Clamp(camSwitch.fpPitch + pitchDelta, minPitch, maxPitch);

            // Ŀ�� ���
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            // 3��Ī ���� �� Ŀ�� ����
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
