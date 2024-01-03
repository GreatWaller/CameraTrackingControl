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

        public CameraController(ICameraDataSource cameraDataSource)
        {
            this.cameraDataSource = cameraDataSource;
            cameras = cameraDataSource.LoadCameras();
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
            }
        }


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


        public Point3F CalculateObjectPositionToCamera(Point3F objectCoordinates, Camera camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;
            return objectCoordinatesInCameraCoordinates;
        }
        public double CalculateHorizontalPanAngle(Point3F objectCoordinates, Camera camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = Math.Atan2(objectCoordinates.X, objectCoordinates.Y) * 180 / Math.PI - camera.HorizontalPanAngle;

            return horizontalPanAngle;
        }
        public double CalculateVerticalTiltAngle(Point3F objectCoordinates, Camera camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = Math.Atan2(objectCoordinates.Y, objectCoordinates.Z) * 180 / Math.PI - camera.VerticalTiltAngle;

            return verticalTiltAngle;
        }


    }
}
