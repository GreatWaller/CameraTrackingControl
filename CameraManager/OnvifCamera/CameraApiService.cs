using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.OnvifCamera
{
    internal class CameraApiService
    {
        private string baseUri;
        private readonly RestApiClient restApiClient;
        private List<CameraInfo> cameraInfos;

        public CameraApiService(string baseUri)
        {
            this.baseUri = baseUri;
            restApiClient = new RestApiClient(baseUri);
            cameraInfos = new List<CameraInfo>();
        }

        public List<CameraInfo> GetAllDevices()
        {
            cameraInfos.Clear();
            //var cameraInfos = new List<CameraInfo>();

            // 从数据库加载摄像机信息
            // get all devices
            // find username and password
            // find mainstream profile token
            Task<string> devicesJson = restApiClient.GetAsync("Device/GetAll");
            var deviceRoot = JsonConvert.DeserializeObject<DeviceRoot>(devicesJson.Result);
            if (deviceRoot == null)
            {
                return cameraInfos;
            }
            foreach (var device in deviceRoot?.Result?.Items)
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

        public CameraStatus GetCurrentStatus(string deviceId)
        {
            var cameraStatus = new CameraStatus();
            // host username pw profile
            if (cameraInfos.Count == 0)
            {
                cameraInfos = GetAllDevices();
            }

            var cameraInfo = cameraInfos.Find(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                cameraStatus.Error = $"Can not find the Device: {deviceId}";
                return cameraStatus;
            }
            var parameters = new Dictionary<string, string>()
            {
                { "host", cameraInfo.Ipv4Address },
                {"username", cameraInfo.UserName },
                {"password", cameraInfo.Password },
                {"profileToken", cameraInfo.ProfileToken }
            };

            var uri = restApiClient.BuildUri("PTZ/GetStatusInDegree", parameters);

            try
            {
                var jsonString = restApiClient.GetAsync(uri).Result;
                // 将JSON字符串反序列化为ApiResponse对象
                var cameraStatusResponse = JsonConvert.DeserializeObject<CameraStatusResponse>(jsonString);
                cameraStatus = cameraStatusResponse?.Result;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to GetStatusInDegree");
                //throw;
                cameraStatus.Error = $"Failed to GetStatusInDegree. [DeviceId: {deviceId}]";
                return cameraStatus;
            }

            return cameraStatus;
        }

        public CameraStatus MoveToAbsolutePositionInDegree(string deviceId, float panInDegree, float tiltInDegree, int zoomLevel
            , float panSpeed=1, float tiltSpeed =1, float zoomSpeed=1)
        {
            var cameraStatus = new CameraStatus();

            if (cameraInfos.Count == 0)
            {
                cameraInfos = GetAllDevices();
            }

            var cameraInfo = cameraInfos.Find(p => p.DeviceId == deviceId);
            if (cameraInfo == null)
            {
                cameraStatus.Error = $"Can not find the Device: {deviceId}";
                return cameraStatus;
            }
            var parameters = new Dictionary<string, string>()
            {
                { "host", cameraInfo.Ipv4Address },
                {"username", cameraInfo.UserName },
                {"password", cameraInfo.Password },
                {"profileToken", cameraInfo.ProfileToken },
                {"panInDegree", panInDegree.ToString()}, 
                {"tiltInDegree", tiltInDegree.ToString()}, 
                {"zoomInLevel",zoomLevel.ToString()},
                {"panSpeed",panSpeed.ToString()},
                {"tiltSpeed",tiltSpeed.ToString()},
                {"zoomSpeed",zoomSpeed.ToString()}
            };

            var uri = restApiClient.BuildUri("PTZ/AbsoluteMoveWithDegree", parameters);

            try
            {
                var jsonString = restApiClient.PostAsync(uri,string.Empty).Result;
                // 将JSON字符串反序列化为ApiResponse对象
                cameraStatus = JsonConvert.DeserializeObject<CameraStatus>(jsonString);

            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to GetStatusInDegree");
                //throw;
                cameraStatus.Error = $"Failed to Move. [DeviceId: {deviceId}]";
            }

            return cameraStatus;

        }
    }
}