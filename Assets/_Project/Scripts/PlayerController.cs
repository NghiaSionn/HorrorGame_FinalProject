using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem; 

public class PlayerController : NetworkBehaviour
{
    private NetworkCharacterController _controller;
    private Animator _animator;

    [Header("Cài đặt Di chuyển")]
    public Camera playerCamera;
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.5f;
    public float mouseSensitivity = 15f;
    
    // Lưu trữ góc xoay thực tế để chống lỗi giật/nhảy của NetworkTransform
    private float xRotation = 0f;
    private float yRotation = 0f;
    
    // Biến lưu trữ trạng thái bấm phím nhảy để không bị "nuốt phím" do lệch nhịp mạng
    private bool _jumpPressed;

    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _animator = GetComponentInChildren<Animator>(); // Tìm Animator ở chính nó hoặc các con
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

        // BẮT PHÍM NHẢY Ở ĐÂY ĐỂ CHẮC CHẮN 100% KHÔNG BỊ TRƯỢT PHÍM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _jumpPressed = true;
        }
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

        // Kiểm tra xem có đang đè phím Shift không
        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        float currentTargetSpeed = isRunning ? runSpeed : walkSpeed;

        // Nếu không bấm phím di chuyển nào thì tốc độ mục tiêu bằng 0
        if (horizontal == 0 && vertical == 0) currentTargetSpeed = 0;

        // Đi thẳng, đi ngang dựa trên hướng ĐANG NHÌN
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        _controller.Move(moveDirection * currentTargetSpeed * Runner.DeltaTime);

        // ĐỒNG BỘ HOẠT ẢNH: Gửi tốc độ thực tế vào Animator
        if (_animator != null)
        {
            _animator.SetFloat("Speed", currentTargetSpeed);
        }

        // 3. Xử lý Nhảy (Thực thi cái nút đã được bấm ở hàm Update)
        if (_jumpPressed)
        {
            _jumpPressed = false; // Reset lại phím
            
            _controller.Jump(); // Lệnh nhảy của Fusion
            
            // Kích hoạt hoạt ảnh nhảy
            if (_animator != null)
            {
                _animator.SetTrigger("Jump");
            }
        }
    }
}

