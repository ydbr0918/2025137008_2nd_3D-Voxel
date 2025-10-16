using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonMouseLook : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;        // Yaw(좌우 회전)를 적용할 대상(플레이어)
    public CameraSwitch camSwitch;  // 1인칭 여부 & fpPitch 제어

    [Header("Mouse")]
    public float mouseSensitivity = 120f; // 감도(도/초 느낌)
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

            // 좌우(Yaw) → 플레이어에 적용
            float yawDelta = mx * mouseSensitivity * Time.deltaTime;
            player.Rotate(0f, yawDelta, 0f, Space.World);

            // 상하(Pitch) → 카메라 컨트롤러의 fpPitch에 적용(클램프)
            float pitchDelta = -my * mouseSensitivity * Time.deltaTime;
            camSwitch.fpPitch = Mathf.Clamp(camSwitch.fpPitch + pitchDelta, minPitch, maxPitch);

            // 커서 잠금
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else
        {
            // 3인칭 복귀 시 커서 해제
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
