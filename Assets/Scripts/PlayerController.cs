using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem; // Bắt buộc phải có để dùng New Input System

public class PlayerController : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    private void Awake()
    {
        // Lấy thành phần NetworkCharacterController mà bạn đã gắn trên Player
        _controller = GetComponent<NetworkCharacterController>();
    }

    // FixedUpdateNetwork là hàm "Update" đặc biệt của Photon Fusion (giúp đồng bộ mạng mượt mà)
    public override void FixedUpdateNetwork()
    {
        // Chỉ người nào tạo ra nhân vật này (HasStateAuthority) mới được phép điều khiển nó
        if (HasStateAuthority == false)
        {
            return;
        }

        // 1. Nhận phím bấm bằng New Input System
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical = 1f;
            if (Keyboard.current.sKey.isPressed) vertical = -1f;
            if (Keyboard.current.aKey.isPressed) horizontal = -1f;
            if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        }

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        moveDirection.Normalize();

        // 2. Di chuyển nhân vật (Áp dụng trọng lực nếu không có di chuyển)
        _controller.Move(moveDirection * 5f * Runner.DeltaTime);

        // 3. Nhảy (Phím Space)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _controller.Jump();
        }
    }
}
