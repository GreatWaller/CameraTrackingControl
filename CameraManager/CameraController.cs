using CameraManager.OnvifCamera;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
        public CameraInfo FindNearestCamera(GeoLocation shipLocation)
        {
            CameraInfo nearestCamera = null;
            double minDistance = double.MaxValue;

            foreach (var camera in cameras)
            {
                double distance = CalculateDistance(shipLocation, new GeoLocation { Latitude = camera.Latitude, Longitude = camera.Longitude });
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCamera = camera;
                }
            }

            return nearestCamera;
        }

        // 计算两点间距离
        private double CalculateDistance(GeoLocation location1, GeoLocation location2)
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

        #region calculate by pixel，no need for now

        // 计算物体在相机图像中的位置
        public PointF CalculateObjectPositionInImage(Vector3 objectCoordinates, Camera camera)
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
        public double CalculateHorizontalPanAngle(Vector3 objectPositionInImage, Camera camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = Math.Atan2(objectPositionInImage.X, camera.FocalLength) * 180 / Math.PI - camera.HorizontalPanAngle;

            return horizontalPanAngle;
        }


        // 计算相机需要垂直转动的角度
        public double CalculateVerticalTiltAngle(Vector3 objectPositionInImage, Camera camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = Math.Atan2(objectPositionInImage.Y, camera.FocalLength) * 180 / Math.PI - camera.VerticalTiltAngle;

            return verticalTiltAngle;
        }
        #endregion

        #region core

        public Vector3 CalculateObjectPositionToCamera(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 将物体坐标从世界坐标系转换到相机坐标系
            var objectCoordinatesInCameraCoordinates = camera.CameraRotationMatrix * objectCoordinates;
            return objectCoordinatesInCameraCoordinates;
        }
        public float CalculateHorizontalPanAngle(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要水平转动的角度
            var horizontalPanAngle = MathF.Atan2(objectCoordinates.Y, objectCoordinates.X) * 180 / MathF.PI - camera.HomePanToNorth;

            if (horizontalPanAngle < 0)
            {
                horizontalPanAngle = 360 + horizontalPanAngle;
            }
            return horizontalPanAngle;
        }
        public float CalculateVerticalTiltAngle(Vector3 objectCoordinates, CameraInfo camera)
        {
            // 计算相机需要垂直转动的角度
            var verticalTiltAngle = MathF.Atan2(objectCoordinates.Z, objectCoordinates.Y) * 180 / MathF.PI - camera.HomeTiltToHorizon;

            verticalTiltAngle = Math.Clamp(verticalTiltAngle, camera.MinTiltDegree, camera.MaxTiltDegree);
            return verticalTiltAngle;
        }

        private int CalculateZoomLevel(Vector3 objectPositionToCamera, CameraInfo cameraInfo)
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
        public bool PointToTarget(Vector3 objectCoordinates, string deviceId)
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

            var result = Move(deviceId, horizontalPanAngle, verticalTiltAngle, zoomLevel);

            return result;
        }

        public bool PointToTargetByGeo(GeoLocation location)
        {
            /* 1 find the nearest camera
             * 2 point camera at target
             */
            var camera = FindNearestCamera(location);
            var target = GetRelativeCartesianCoordinates(camera.Latitude, camera.Longitude, camera.Altitude, location.Latitude, location.Longitude, location.Altitude);

            var result = PointToTarget(target, camera.DeviceId);
            return true;
        }
        #endregion


        #region helper
        public static Vector3 ConvertLatLngToCartesian(double latitude, double longitude, double altitude)
        {
            // Convert latitude and longitude to radians
            double latRad = latitude * Math.PI / 180;
            double lonRad = longitude * Math.PI / 180;

            // Calculate the Earth's radius at the given latitude
            double radius = 6378137.0; // Earth's mean radius in meters
            double flattening = 1 / 298.257223563; // Earth's flattening
            double e2 = flattening * (2 - flattening);
            double a = radius * (1 - e2);
            double b = radius * Math.Sqrt(1 - e2);
            double c = Math.Sqrt(a * a - b * b);

            // Calculate the x, y, and z coordinates
            double x = (a * Math.Cos(latRad) * Math.Cos(lonRad) + altitude) * 1000; // Convert to meters
            double y = (a * Math.Cos(latRad) * Math.Sin(lonRad)) * 1000; // Convert to meters
            double z = ((b * b / a) * Math.Sin(latRad) + altitude) * 1000; // Convert to meters

            // Return the Cartesian coordinates
            return new Vector3((float)x, (float)y, (float)z);
        }
        public static Vector3 GetRelativeCartesianCoordinates(double latitude1, double longitude1, double altitude1, double latitude2, double longitude2, double altitude2)
        {
            // Convert the two sets of latitude and longitude to Cartesian coordinates
            Vector3 cartesianCoordinates1 = ConvertLatLngToCartesian(latitude1, longitude1, altitude1);
            Vector3 cartesianCoordinates2 = ConvertLatLngToCartesian(latitude2, longitude2, altitude2);

            // Calculate the relative Cartesian coordinates
            Vector3 relativeCartesianCoordinates = cartesianCoordinates2 - cartesianCoordinates1;

            // Return the relative Cartesian coordinates
            return relativeCartesianCoordinates;
        }
        #endregion
    }
}
