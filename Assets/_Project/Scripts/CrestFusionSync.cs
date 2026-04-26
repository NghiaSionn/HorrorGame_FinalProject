using UnityEngine;
using Crest;
using Fusion;
using System.Linq;

public class CrestFusionSync : MonoBehaviour, ITimeProvider
{
    private NetworkRunner _runner;

    // Yêu cầu của Crest: Thời gian hiện tại
    public float CurrentTime 
    {
        get 
        {
            if (_runner == null || !_runner.IsRunning) return UnityEngine.Time.time;
            return (float)_runner.SimulationTime; // Đồng bộ thời gian sóng biển với máy chủ Photon
        }
    }

    // Yêu cầu của Crest: Thời gian giữa 2 khung hình
    public float DeltaTime 
    {
        get 
        {
            if (_runner == null || !_runner.IsRunning) return UnityEngine.Time.deltaTime;
            return _runner.DeltaTime;
        }
    }

    // Yêu cầu của Crest: Dành cho tính toán động lực học
    public float DeltaTimeDynamics => DeltaTime;

    private void Start()
    {
        // Khai báo cho biển biết là: Từ giờ hãy tính sóng theo thời gian của script này
        if (OceanRenderer.Instance != null)
        {
            OceanRenderer.Instance.PushTimeProvider(this);
        }
        else
        {
            Debug.LogError("CrestFusionSync: Không tìm thấy OceanRenderer trong Scene!");
        }
    }

    private void Update()
    {
        // Liên tục kiểm tra xem Photon đã chạy chưa để lấy dữ liệu đồng bộ
        if (_runner == null && NetworkRunner.Instances.Count > 0)
        {
            _runner = NetworkRunner.Instances.FirstOrDefault(r => r.IsRunning);
            if (_runner != null)
            {
                Debug.Log("Đã đồng bộ Sóng biển Crest với máy chủ Photon thành công!");
            }
        }
    }

    private void OnDestroy()
    {
        // Trả lại thời gian bình thường nếu script này bị xóa
        if (OceanRenderer.Instance != null)
        {
            OceanRenderer.Instance.PopTimeProvider(this);
        }
    }
}
