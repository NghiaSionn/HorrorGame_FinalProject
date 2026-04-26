using UnityEngine;
using Fusion;
using Crest; // Thư viện của sóng biển

public class NetworkBoatSync : NetworkBehaviour
{
    private SimpleFloatingObject _crestBuoyancy;

    private void Awake()
    {
        // Lấy script tính toán lực nổi của Crest
        _crestBuoyancy = GetComponent<SimpleFloatingObject>();
    }

    public override void Spawned()
    {
        if (_crestBuoyancy == null) return;

        if (HasStateAuthority)
        {
            // Nếu mình là Chủ phòng (Host / State Authority): Mình sẽ tự tính toán lực nổi của sóng
            _crestBuoyancy.enabled = true;
        }
        else
        {
            // Nếu mình là Khách (Client): TẮT tính năng tự tính toán sóng đi.
            // Tránh việc máy Khách và máy Chủ cùng tính toán vật lý dẫn đến tàu bị giật lắc giằng co.
            // Lúc này con tàu sẽ hoàn toàn di chuyển dựa trên dữ liệu mạng do máy Chủ gửi về.
            _crestBuoyancy.enabled = false;
        }
    }
}
