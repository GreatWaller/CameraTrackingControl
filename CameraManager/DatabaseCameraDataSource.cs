using CameraManager.OnvifCamera;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager
{
    internal class DatabaseCameraDataSource : ICameraDataSource
    {
        private readonly RestApiClient restApiClient;

        public DatabaseCameraDataSource(string baseUri)
        {
            restApiClient = new RestApiClient(baseUri);

        }
        public List<CameraInfo> LoadCameras()
        {
            var cameraInfos= new List<CameraInfo>();
            // 从数据库加载摄像机信息
            // get all devices
            // find username and password
            // find mainstream profile token

            Task<string> devicesJson = restApiClient.GetAsync("Device/GetAll");
            var deviceRoot = JsonConvert.DeserializeObject<DeviceRoot>(devicesJson.Result);
            foreach (var device in deviceRoot?.Result.Items)
            {
                var cameraInfo = new CameraInfo();
                cameraInfo.DeviceId = device.DeviceId;
                cameraInfo.Ipv4Address = device.Ipv4Address;
                cameraInfo.Longitude = device.Longitude;
                cameraInfo.Latitude = device.Latitude;
                cameraInfo.HomePanToNorth = device.HomePanToNorth;
                cameraInfo.HomeTiltToHorizon = device.HomeTiltToHorizon;
                cameraInfo.MinPanDegree = device.MinPanDegree;
                cameraInfo.MaxPanDegree = device.MaxPanDegree;
                cameraInfo.MinTiltDegree = device.MinTiltDegree;
                cameraInfo.MaxTiltDegree = device.MaxTiltDegree;
                cameraInfo.MinZoomLevel = device.MinZoomLevel;
                cameraInfo.MaxZoomLevel = device.MaxZoomLevel;
                cameraInfo.FocalLength = device.FocalLength;

                try
                {
                    var deviceInfoJson = restApiClient.GetAsync("DeviceCredential/GetCredentialByDeviceId?deviceId=" + device.DeviceId);
                    var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(deviceInfoJson.Result);
                    if (deviceInfo != null)
                    {
                        cameraInfo.UserName = deviceInfo.Username;
                        cameraInfo.Password = deviceInfo.Password;
                    }

                    var profileJson = restApiClient.GetAsync("Media/GetProfilesByDeviceId?deviceId=" + device.DeviceId);
                    var profiles = JsonConvert.DeserializeObject<DeviceProfile>(profileJson.Result);
                    var profile = profiles?.Result.FirstOrDefault(p => p.Name == "mainStream");
                    if (profile != null)
                    {
                        cameraInfo.ProfileToken = profile.Token;
                        cameraInfo.VideoWidth = profile.VideoEncoderConfiguration.VideoWidth;
                        cameraInfo.VideoHeight = profile.VideoEncoderConfiguration.VideoHeight;
                    }
                }
                catch (Exception)
                {
                    Trace.TraceError($"ERROR DeviceId: {device.DeviceId}");
                    continue;
                    //throw;
                }

                cameraInfos.Add(cameraInfo);
            }



            return cameraInfos;
        }


    }
}
