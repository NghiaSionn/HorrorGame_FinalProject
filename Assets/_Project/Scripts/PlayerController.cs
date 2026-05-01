using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    private NetworkCharacterController _controller;
    private CharacterController _characterController;
    private Animator _animator;
    private ChangeDetector _changeDetector;

    [Header("Cài đặt Di chuyển")]
    public Camera playerCamera;
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.5f;
    public float mouseSensitivity = 15f;

    [Header("UI Tên người chơi")]
    public TextMeshProUGUI nameTagText;  // Kéo TMP Text vào đây
    public Transform nameBillboard;      // Kéo Canvas chứa tên vào đây

    // Biến mạng: Tự động đồng bộ cho TẤT CẢ người chơi, kể cả người vào sau
    [Networked]
    public NetworkString<_32> PlayerName { get; set; }

    private float xRotation = 0f;
    private float yRotation = 0f;
    private bool _jumpPressed;

    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    public override void Spawned()
    {
        // Khởi tạo bộ theo dõi thay đổi biến mạng (Fusion 2)
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            // --- PLAYER CỦA CHÍNH MÌNH ---
            playerCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yRotation = transform.rotation.eulerAngles.y;

            // Ẩn name tag của chính mình
            if (nameBillboard != null)
                nameBillboard.gameObject.SetActive(false);

            // Ghi tên vào biến mạng → tự động đồng bộ cho tất cả, kể cả người vào sau
            PlayerName = GetDisplayName();
        }
        else
        {
            // --- PLAYER CỦA NGƯỜI KHÁC ---
            playerCamera.gameObject.SetActive(false);
            var audioListener = playerCamera.GetComponent<AudioListener>();
            if (audioListener != null) audioListener.enabled = false;

            // Hiển thị tên ngay khi vừa thấy player này (cho người vào sau)
            UpdateNameTag();
        }
    }

    // Lấy tên hiển thị từ tài khoản Firebase
    private string GetDisplayName()
    {
        if (FirebaseManager.Instance?.user?.Email != null)
        {
            string email = FirebaseManager.Instance.user.Email;
            int atIndex = email.IndexOf('@');
            if (atIndex > 0)
                return email.Substring(0, atIndex);
            return email;
        }

        if (FirebaseManager.Instance?.user?.UserId != null)
        {
            string uid = FirebaseManager.Instance.user.UserId;
            return "Khach_" + uid.Substring(Mathf.Max(0, uid.Length - 4));
        }

        return "Nguoi choi";
    }

    // Fusion 2: Chạy mỗi frame để phát hiện thay đổi biến mạng và cập nhật UI
    public override void Render()
    {
        if (_changeDetector == null) return;

        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(PlayerName):
                    UpdateNameTag();
                    break;
            }
        }
    }

    private void UpdateNameTag()
    {
        if (nameTagText != null)
        {
            nameTagText.text = PlayerName.ToString();
        }
    }

    private void Update()
    {
        if (!HasStateAuthority || Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        yRotation += mouseDelta.x;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Chỉ nhảy khi đang chạm đất
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_characterController != null && _characterController.isGrounded)
            {
                _jumpPressed = true;
            }
        }

        // Billboard: Tên luôn quay mặt đúng về phía Camera
        if (nameBillboard != null && Camera.main != null)
        {
            Vector3 dirToCamera = Camera.main.transform.position - nameBillboard.position;
            nameBillboard.rotation = Quaternion.LookRotation(dirToCamera);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false) return;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical = 1f;
            if (Keyboard.current.sKey.isPressed) vertical = -1f;
            if (Keyboard.current.aKey.isPressed) horizontal = -1f;
            if (Keyboard.current.dKey.isPressed) horizontal = 1f;
        }

        bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        float currentTargetSpeed = isRunning ? runSpeed : walkSpeed;

        if (horizontal == 0 && vertical == 0) currentTargetSpeed = 0;

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        moveDirection.Normalize();

        _controller.Move(moveDirection * currentTargetSpeed * Runner.DeltaTime);

        if (_animator != null)
        {
            _animator.SetFloat("Speed", currentTargetSpeed);
        }

        if (_jumpPressed)
        {
            _jumpPressed = false;
            _controller.Jump();

            if (_animator != null)
            {
                _animator.SetTrigger("Jump");
            }
        }
    }
}
