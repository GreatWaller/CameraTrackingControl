namespace CameraTrackingControl.Onvif
{
    internal class OnvifCamera : ICamera
    {
        // 假设 JSON 文件位于项目的根目录下
        private string jsonFilePath;

        // 假设 baseUri、host、username、password 和 profileToken 是有效的值
        private string baseUri = "http://192.168.1.220:44311";
        private string host = "192.168.1.151";
        private string username = "admin";
        private string password = "CS@202304";
        private string profileToken = "Profile_1";

        // 创建 ApiService 实例
        private ApiService apiService;

        private bool isMoving = false;

        // 角度转换系数，根据实际需要调整
        private const float DegreesToNormalized = 1.0f / 180.0f;

        
        public OnvifCamera()
        {
            jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Eoss.Onvif.postman_collection.json");
            apiService = new ApiService(baseUri, host, username, password, profileToken, jsonFilePath);
        }
        public CameraStatus GetCurrentStatus(CameraStatus originalStatus)
        {
            var cameraStatus = new CameraStatus();
            var status = apiService.GetPTZStatus().Result;
            if (status != null)
            {
                cameraStatus.Azimuth = ConvertToDegreesPan(status.PanPosition, 0f, 360f);
                cameraStatus.Altitude = ConvertToDegreesTilt(status.TiltPosition, -10f, 80f);
                cameraStatus.Zoom = ConvertToDegreesZoom(status.ZoomPosition, 1f, 33f);
            }
            return cameraStatus;
        }

        public async void Rotate(float panDegrees, float tiltDegrees, float zoom)
        {
            if (isMoving)
            {
                Console.WriteLine($"[#]Camera is Moving...");
                return;
            }
            isMoving = true;

            // 转换角度值
            float pan = NormalizedPan(panDegrees, 0, 360);
            float tilt = NormalizeTilt(tiltDegrees, -10, 80);
            float clampedZoom = NormalizeZoom(zoom, 1, 33); // 确保 zoom 在 (1, 33) 范围内

            await apiService.PTZAbsoluteMove(pan, tilt, clampedZoom);

            //Thread.Sleep(1000);
            isMoving = false;
        }

        // 角度转换方法
        private float NormalizedPan(float degrees, float min, float max)
        {
            float middleDegrees = (max - min) / 2;
            if (degrees <= middleDegrees)
            {
                return degrees * DegreesToNormalized;
            }
            else
            {
                return (degrees - max) * DegreesToNormalized;
            }
        }
        private float NormalizeTilt(float degrees, float min, float max)
        {
            float middleDegrees = (max - min) / 2;
            return (middleDegrees - degrees) * DegreesToNormalized;
        }
        private float NormalizeZoom(float zoom, float min, float max)
        {
            float clampedZoom = Math.Max(1, Math.Min(max, zoom)); // 确保 zoom 在 (1, 33) 范围内

            return clampedZoom / (max - min);
        }

        private float ConvertToDegreesPan(float normalizedAngle, float min, float max)
        {
            float middleDegrees = (max - min) / 2;

            float degrees = normalizedAngle * middleDegrees;

            // 确保角度在有效范围内
            if (degrees < 0)
            {
                degrees += max; // 将负值转换为正值
            }

            return degrees;
        }

        private float ConvertToDegreesTilt(float normalizedAngle, float min, float max)
        {
            float middleDegrees = (max - min) / 2;

            float degrees = normalizedAngle * middleDegrees - min;

            return degrees;
        }

        private float ConvertToDegreesZoom(float normalizedZoom, float min, float max)
        {
            float clampedZoom = Math.Min(1, Math.Min(max, normalizedZoom));
            float degrees = clampedZoom * (max - min);

            return degrees;
        }

    }
}
