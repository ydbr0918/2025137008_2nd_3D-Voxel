using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; 
    private Rigidbody rb;

    private Vector3 forwardDir; 
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        forwardDir = transform.forward;
    }

    void Update()
    {
        float z = Input.GetAxisRaw("Horizontal"); 
        float x = Input.GetAxisRaw("Vertical");   
        Vector3 rightDir = Vector3.Cross(Vector3.up, -forwardDir);
        moveInput = (forwardDir * z + rightDir * x).normalized;
    }

    void FixedUpdate()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // 이동
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

            // 이동 방향 바라보기 (회전)
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 0.15f);
        }
    }
}

