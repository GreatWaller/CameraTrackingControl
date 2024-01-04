using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.OnvifCamera
{
    internal class Device
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public string Ipv4Address { get; set; }
        public int GroupId { get; set; }
        public DeviceGroup Group{ get; set; }
        public List<string> Types { get; set; }
        public List<string> Capabilities { get; set; }
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Altitude { get; set; }
        public float HomePanToNorth { get; set; }
        public float HomeTiltToHorizon { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public float AngleToXAxis { get; set; }
        public float AngleToYAxis { get; set; }
        public float AngleToZAxis { get; set; }
        public bool CanPTZ { get; set; }
        public float MinPanDegree { get; set; }
        public float MaxPanDegree { get; set; }
        public float MinTiltDegree { get; set; }
        public float MaxTiltDegree { get; set; }
        public float MinZoomLevel { get; set; }
        public float MaxZoomLevel { get; set; }
        public float FocalLength { get; set; }
        public int Id { get; set; }
    }
}
