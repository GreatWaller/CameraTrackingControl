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
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
        public double HomePanToNorth { get; set; }
        public double HomeTiltToHorizon { get; set; }
        public double Roll { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double AngleToXAxis { get; set; }
        public double AngleToYAxis { get; set; }
        public double AngleToZAxis { get; set; }
        public bool CanPTZ { get; set; }
        public double MinPanDegree { get; set; }
        public double MaxPanDegree { get; set; }
        public double MinTiltDegree { get; set; }
        public double MaxTiltDegree { get; set; }
        public double MinZoomLevel { get; set; }
        public double MaxZoomLevel { get; set; }
        public double FocalLength { get; set; }
        public int Id { get; set; }
    }
}
