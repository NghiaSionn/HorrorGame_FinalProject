using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Sử dụng TextMeshPro cho chữ đẹp hơn
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    [Header("Buttons")]
    public Button loginButton;
    public Button registerButton;
    public Button guestButton;

    [Header("Status")]
    public TextMeshProUGUI statusText;

    private void Start()
    {
        // Gán sự kiện cho các nút bấm
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        guestButton.onClick.AddListener(OnGuestClicked);
        
        statusText.text = "Chào mừng kẻ sống sót...";
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "<color=red>Đừng để trống hố chôn...</color>";
            return;
        }

        statusText.text = "Đang kiểm tra danh tính...";
        FirebaseManager.Instance.SignInWithEmail(email, password);
    }

    private void OnRegisterClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || password.Length < 6)
        {
            statusText.text = "<color=red>Email sai hoặc mật khẩu quá ngắn (ít nhất 6 ký tự).</color>";
            return;
        }

        statusText.text = "Đang khắc tên bạn vào sổ tử...";
        FirebaseManager.Instance.RegisterWithEmail(email, password);
    }

    private void OnGuestClicked()
    {
        statusText.text = "Đang đi vào bóng tối...";
        FirebaseManager.Instance.SignInAnonymously();
    }
}
