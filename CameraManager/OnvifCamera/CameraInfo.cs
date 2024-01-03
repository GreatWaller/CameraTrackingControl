using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.OnvifCamera
{
    internal class CameraInfo
    {
        public string DeviceId { get; set; }
        public string Ipv4Address { get; set; }
        public bool CanPTZ { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public double HomePanToNorth { get; set; }
        public double HomeTiltToHorizon { get; set; }

        public double MinPanDegree { get; set; }
        public double MaxPanDegree { get; set; }
        public double MinTiltDegree { get; set; }
        public double MaxTiltDegree { get; set; }
        public double MinZoomLevel { get; set; }
        public double MaxZoomLevel { get; set; }
        public double FocalLength { get; set; }

        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public Matrix3x3 CameraRotationMatrix { get; set; }


        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // 默认取主码流
        public string ProfileToken { get; set; } = string.Empty;
        public int VideoWidth { get; set; } = 1920;
        public int VideoHeight { get; set; } = 1080;

    }
}
