using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraManager.OnvifCamera;

namespace CameraManager
{
    internal interface ICameraService
    {
        public List<CameraInfo> GetAllDevices();
        public CameraStatus GetCurrentStatus(string deviceId);
        public CameraStatus MoveToAbsolutePositionInDegree(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel
            , float panSpeed = 1, float tiltSpeed = 1, float zoomSpeed = 1);
        public CameraStatus MoveToRelativePositionInDegree(string deviceId, float panInDegree, float tiltInDegree, float zoomLevel
            , float panSpeed = 1, float tiltSpeed = 1, float zoomSpeed = 1);
    }
}
