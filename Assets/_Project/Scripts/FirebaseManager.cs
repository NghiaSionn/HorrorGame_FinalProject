using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using Fusion; // Thêm thư viện Fusion

public class FirebaseManager : MonoBehaviour
{
    // Tạo Singleton để script này có thể được gọi từ bất kỳ đâu (ví dụ: FirebaseManager.Instance.SignInAnonymously())
    public static FirebaseManager Instance;

    [Header("Firebase State")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser user;

    private void Awake()
    {
        // Cấu hình Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Không bị hủy khi chuyển Scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Bắt đầu khởi tạo Firebase ngay khi game chạy
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        Debug.Log("Đang kiểm tra và khởi tạo Firebase...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase đã sẵn sàng để sử dụng
                InitializeAuth();
            }
            else
            {
                Debug.LogError("Lỗi không thể khởi tạo Firebase: " + dependencyStatus);
            }
        });
    }

    private void InitializeAuth()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        Debug.Log("Firebase Auth đã khởi tạo thành công!");
    }

    // Hàm này tự động chạy mỗi khi trạng thái đăng nhập thay đổi (vừa đăng nhập, hoặc đăng xuất)
    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Đã đăng xuất tài khoản: " + user.UserId);
            }
            
            user = auth.CurrentUser;
            
            if (signedIn)
            {
                Debug.Log("Đã đăng nhập thành công! UserID: " + user.UserId);
            }
        }
    }

    // =========================================================
    // CÁC HÀM XỬ LÝ ĐĂNG NHẬP / ĐĂNG KÝ
    // =========================================================

    // 1. Đăng nhập ẩn danh (Cho khách chơi thử)
    public void SignInAnonymously()
    {
        Debug.Log("Đang thử đăng nhập ẩn danh...");
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Đăng nhập ẩn danh bị hủy.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi đăng nhập ẩn danh: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.Log("Khách đăng nhập thành công! ID: " + result.User.UserId);

            // GỌI PHOTON CHẠY SAU KHI ĐĂNG NHẬP KHÁCH THÀNH CÔNG
            Debug.Log("Chuẩn bị gọi Photon...");
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log("Đang chạy trên luồng chính. Gọi StartGame...");
                if (FusionNetworkManager.Instance == null) {
                    Debug.LogError("LỖI: Chưa kéo script FusionNetworkManager vào Scene!");
                    return;
                }
                FusionNetworkManager.Instance.StartGame(GameMode.Shared);
            });
        });
    }

    // 2. Đăng ký tài khoản bằng Email và Password
    public void RegisterWithEmail(string email, string password)
    {
        Debug.Log($"Đang đăng ký tài khoản cho email: {email}...");
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Đăng ký bị hủy.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi đăng ký: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.Log("Tạo tài khoản thành công! ID: " + result.User.UserId);
        });
    }

    // 3. Đăng nhập bằng Email và Password
    public void SignInWithEmail(string email, string password)
    {
        Debug.Log($"Đang đăng nhập vào email: {email}...");
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Đăng nhập bị hủy.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi đăng nhập: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.Log("Đăng nhập thành công! ID: " + result.User.UserId);

            // GỌI PHOTON CHẠY SAU KHI ĐĂNG NHẬP EMAIL THÀNH CÔNG
            Debug.Log("Chuẩn bị gọi Photon...");
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                Debug.Log("Đang chạy trên luồng chính. Gọi StartGame...");
                if (FusionNetworkManager.Instance == null) {
                    Debug.LogError("LỖI: Chưa kéo script FusionNetworkManager vào Scene!");
                    return;
                }
                FusionNetworkManager.Instance.StartGame(GameMode.Shared);
            });
        });
    }

    // 4. Đăng xuất
    public void SignOut()
    {
        auth.SignOut();
        Debug.Log("Đã đăng xuất tài khoản.");
    }
}
