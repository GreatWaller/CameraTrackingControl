using CameraManager.OnvifCamera;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager
{
    internal class CameraController
    {
        private List<CameraInfo> cameras; // 摄像机列表
        private ICameraDataSource cameraDataSource;

        private readonly CameraApiService cameraApiService;
        private Dictionary<string,MoveStatus> moveStatus = new Dictionary<string,MoveStatus>();

        public CameraController(ICameraDataSource _cameraDataSource, CameraApiService _cameraApiService)
        {
            cameraApiService = _cameraApiService;
            this.cameraDataSource = _cameraDataSource;

            cameras = _cameraDataSource.LoadCameras();
        }

        // 根据船只坐标找到最近的摄像机
        public CameraInfo FindNearestCamera(ShipLocation shipLocation)
        {
            CameraInfo nearestCamera = null;
            double minDistance = double.MaxValue;

            foreach (var camera in cameras)
            {
                double distance = CalculateDistance(shipLocation, new ShipLocation { Latitude = camera.Latitude, Longitude = camera.Longitude });
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCamera = camera;
                }
            }

            return nearestCamera;
        }

        // 计算两点间距离
        private double CalculateDistance(ShipLocation location1, ShipLocation location2)
        {
            // 使用合适的距离计算公式，比如Haversine公式
            // 这里给出一个简单的计算方式
            return Math.Sqrt(Math.Pow(location1.Latitude - location2.Latitude, 2) + Math.Pow(location1.Longitude - location2.Longitude, 2));
        }

        public void PrintCameraDetails()
        {
            Console.WriteLine("Camera Details:");
            foreach (var camera in cameras)
            {
                Console.WriteLine($"DeviceId: {camera.DeviceId}");
                Console.WriteLine($"Latitude: {camera.Latitude}");
                Console.WriteLine($"Longitude: {camera.Longitude}");
                Console.WriteLine($"Altitude: {camera.Altitude}");
                Console.WriteLine($"MinPan: {camera.MinPanDegree}");
                Console.WriteLine($"MaxPan: {camera.MaxPanDegree}");
                Console.WriteLine($"MinTilt: {camera.MinTiltDegree}");
                Console.WriteLine($"MaxTilt: {camera.MaxTiltDegree}");
                Console.WriteLine($"MinZoomLevel: {camera.MinZoomLevel}");
                Console.WriteLine($"MaxZoomLevel: {camera.MaxZoomLevel}");
                Console.WriteLine($"FocalLength: {camera.FocalLength}");
                Console.WriteLine($"HomePanToNorth: {camera.HomePanToNorth}");
                Console.WriteLine($"HomeTiltToHorizon: {camera.HomeTiltToHorizon}");
                //Console.WriteLine($"AngleToXAxis: {camera.AngleToXAxis}");
                //Console.WriteLine($"AngleToYAxis: {camera.AngleToYAxis}");
                //Console.WriteLine($"AngleToZAxis: {camera.AngleToZAxis}");
                Console.WriteLine($"VideoWidth: {camera.VideoWidth}");
                Console.WriteLine($"VideoHeight: {camera.VideoHeight}");
                Console.WriteLine("-------------------------------------");

            }
        }

        #region calculate by pixel

        // 计算物体在相机图像中的位置
        public PointF CalculateObjectPositionInImage(Point3F objectCoordinates, Camera camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;

            // 计算物体在相机图像中的位置
            var objectPositionInImage = new PointF
            {
                X = (float)((objectCoordinatesInCameraCoordinates.X / objectCoordinatesInCameraCoordinates.Z) * camera.FocalLength),
                Y = (float)((objectCoordinatesInCameraCoordinates.Y / objectCoordinatesInCameraCoordinates.Z) * camera.FocalLength)
            };

            return objectPositionInImage;
        }

        // 计算相机需要水平转动的角度
        public double CalculateHorizontalPanAngle(PointF objectPositionInImage, Camera camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = Math.Atan2(objectPositionInImage.X, camera.FocalLength) * 180 / Math.PI - camera.HorizontalPanAngle;

            return horizontalPanAngle;
        }


        // 计算相机需要垂直转动的角度
        public double CalculateVerticalTiltAngle(PointF objectPositionInImage, Camera camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = Math.Atan2(objectPositionInImage.Y, camera.FocalLength) * 180 / Math.PI - camera.VerticalTiltAngle;

            return verticalTiltAngle;
        }
        #endregion

        #region core

        public Point3F CalculateObjectPositionToCamera(Point3F objectCoordinates, CameraInfo camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;
            return objectCoordinatesInCameraCoordinates;
        }
        public float CalculateHorizontalPanAngle(Point3F objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = MathF.Atan2(objectCoordinates.Y, objectCoordinates.X) * 180 / MathF.PI - camera.HomePanToNorth;

            if (horizontalPanAngle < 0)
            {
                horizontalPanAngle = 360 + horizontalPanAngle;
            }
            return horizontalPanAngle;
        }
        public float CalculateVerticalTiltAngle(Point3F objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = MathF.Atan2(objectCoordinates.Z, objectCoordinates.Y) * 180 / MathF.PI - camera.HomeTiltToHorizon;

            verticalTiltAngle = Math.Clamp(verticalTiltAngle, camera.MinTiltDegree, camera.MaxTiltDegree);
            return verticalTiltAngle;
        }

        private int CalculateZoomLevel(Point3F objectPositionToCamera, CameraInfo cameraInfo)
        {
            var F = cameraInfo.VideoWidth * objectPositionToCamera.X / cameraInfo.Fy / objectPositionToCamera.Z;
            F = Math.Clamp(MathF.Abs(F), 1f, cameraInfo.MaxZoomLevel);
            return (int)F;
        }

        #endregion

        #region Move Controll
        public bool PrepareToMove(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            var status = cameraApiService.GetCurrentStatus(deviceId);
            if (status != null && status.Error== "NO error")
            {
                moveStatus.Add(deviceId, new MoveStatus(status));
            }

            return true;
        }

        public bool Move(string deviceId, float panInDegree, float tiltInDegree, int zoomLevel)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            var status = cameraApiService.MoveToAbsolutePositionInDegree(deviceId, panInDegree, tiltInDegree, zoomLevel);
            if (status != null && status.Error == "NO error")
            {
                moveStatus[deviceId].CameraStatus = status;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectCoordinates">通过经纬计算出来的直角坐标，高度就是安装时设置的参数Altitude</param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public bool PointToTarget(Point3F objectCoordinates, string deviceId)
        {
           CameraInfo cameraInfo = cameras.FirstOrDefault(p=>p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                return false;
            }

            // calculate pan/tilt/zoom
            // 计算物体在相机图像中的位置
            var objectPositionToCamera = CalculateObjectPositionToCamera(objectCoordinates, cameraInfo);

            // 计算相机需要水平转动的角度
            var horizontalPanAngle = CalculateHorizontalPanAngle(objectPositionToCamera, cameraInfo);

            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = CalculateVerticalTiltAngle(objectPositionToCamera, cameraInfo);

            // 计算相机需要变焦的倍数
            var zoomLevel = CalculateZoomLevel(objectPositionToCamera,cameraInfo);

            Move(deviceId, horizontalPanAngle, verticalTiltAngle, zoomLevel);

            return true;
        }


        #endregion

    }
}
