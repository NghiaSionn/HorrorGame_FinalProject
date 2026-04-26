using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem; 

public class PlayerController : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    [Header("Cài đặt Camera")]
    public Camera playerCamera;
    public float mouseSensitivity = 15f;
    
    // Lưu trữ góc xoay thực tế để chống lỗi giật/nhảy của NetworkTransform
    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Ghi nhớ góc xoay ban đầu của nhân vật
            yRotation = transform.rotation.eulerAngles.y;
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
            var audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null) audioListener.enabled = false;
        }
    }

    private void Update()
    {
        // Xử lý góc nhìn (Chuột) chạy ở Update để mượt mà nhất
        if (!HasStateAuthority || Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        // 1. Nhìn Lên/Xuống (Xoay Camera)
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 2. Nhìn Trái/Phải (Xoay cơ thể) - Dùng biến yRotation để triệt tiêu lỗi giật giật
        yRotation += mouseDelta.x;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false)
        {
            return;
        }

        // BÍ QUYẾT LÀ ĐÂY: Ép cơ thể nhân vật LUÔN LUÔN xoay theo con chuột.
        // Triệt tiêu hoàn toàn tính năng "tự động xoay người khi đi ngang" của NetworkCharacterController
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Xử lý di chuyển (Bàn phím) độc lập hoàn toàn với chuột
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical = 1f;
            if (Keyboard.current.sKey.isPressed) vertical = -1f;
            if (Keyboard.current.aKey.isPressed) horizontal = -1f;
            if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        }

        // Đi thẳng, đi ngang hoàn toàn dựa trên hướng ĐANG NHÌN của cơ thể
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        _controller.Move(moveDirection * 5f * Runner.DeltaTime);

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            _controller.Jump();
        }
    }
}
