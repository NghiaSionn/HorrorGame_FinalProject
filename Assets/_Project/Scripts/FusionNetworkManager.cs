using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class FusionNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static FusionNetworkManager Instance;

    [Header("Player Settings")]
    public NetworkObject playerPrefab; // Kéo thả Prefab nhân vật vào đây

    private NetworkRunner _runner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    public async void StartGame(GameMode mode)
    {
        Debug.Log("===> FusionNetworkManager: Đang khởi động Photon Fusion...");
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        // Lấy vị trí của Scene TestMutiPlayer trong Build Settings
        int targetSceneIndex = SceneUtility.GetBuildIndexByScenePath("TestMutiPlayer");
        if (targetSceneIndex == -1)
        {
            Debug.LogError("LỖI CỰC KỲ QUAN TRỌNG: Không tìm thấy Scene 'TestMutiPlayer'! Bạn phải vào File -> Build Settings -> Cầm Scene TestMutiPlayer thả vào danh sách 'Scenes In Build' thì Photon mới biết đường Load.");
            return;
        }

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "HorrorRoom_01",
            Scene = SceneRef.FromIndex(targetSceneIndex), // Load Scene TestMutiPlayer
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("Đã vào phòng Photon Fusion thành công! Đang chuyển Scene...");
        }
        else
        {
            Debug.LogError($"Lỗi vào phòng: {result.ShutdownReason}");
        }
    }

    // --- CÁC HÀM CALLBACK CỦA PHOTON FUSION 2 (Đã cập nhật chuẩn) ---
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Người chơi {player} đã tham gia!");

        // Nếu là chính mình vừa vào phòng, hãy sinh ra nhân vật của mình
        if (player == runner.LocalPlayer && playerPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(0, 5, 0); // Rơi từ độ cao 5m xuống
            runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
