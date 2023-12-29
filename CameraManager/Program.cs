using System.Drawing;

namespace CameraManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // 将 JSON 配置写入临时文件
            string configFile = "camera.json";

            // 创建 ConfigFileCameraDataSource 实例
            ICameraDataSource cameraDataSource = new ConfigFileCameraDataSource(configFile);

            // 创建 CameraController 实例
            CameraController cameraController = new CameraController(cameraDataSource);

            // 测试打印摄像机详情
            cameraController.PrintCameraDetails();


            // 创建相机对象
            var camera = new Camera
            {
                Position = new Point3F(0, 0, 0),
                FocalLength = 0.0048,
                HorizontalFOV = 0.0,
                VerticalFOV = 0.0,
                HorizontalResolution = 2560,
                VerticalResolution = 1920,
                HorizontalPanAngle = 0.0,
                VerticalTiltAngle = 0.0,
                Pitch = 0.0,
                Yaw = 0.0,
                Roll = 0.0
            };
            camera.CameraRotationMatrix = Matrix3x3.CalculateCameraRotationMatrix(camera.Pitch, camera.Yaw, camera.Roll);
            // 创建物体坐标对象
            var objectCoordinates = new Point3F(10, 10, 5);

            // 计算物体在相机图像中的位置
            var objectPositionInImage = cameraController.CalculateObjectPositionToCamera(objectCoordinates, camera);

            // 计算相机需要水平转动的角度
            var horizontalPanAngle = cameraController.CalculateHorizontalPanAngle(objectPositionInImage, camera);

            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = cameraController.CalculateVerticalTiltAngle(objectPositionInImage, camera);


        }
    }
}
