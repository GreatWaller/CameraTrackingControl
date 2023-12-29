using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager
{
    internal class Camera
    {
        public string Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public double MinPan { get; set; }
        public double MaxPan { get; set; }
        public double MinTilt { get; set; }
        public double MaxTilt { get; set; }
        public int MinZoomLevel { get; set; } = 1;
        public int MaxZoomLevel { get; set; }
        // 1倍焦距
        public double FocalLength { get; set; }

        // 相机0位的状态: 逆时针为正
        public double HomePanToNorth { get; set; }
        public double HomeTiltToHorizon { get; set; }

        // 相机安装位置
        public double AngleToXAxis { get; set; }
        public double AngleToYAxis { get; set; }
        public double AngleToZAxis { get; set;}


        public Point3F Position { get; set; }
        //public double FocalLength { get; set; }
        public double HorizontalFOV { get; set; }
        public double VerticalFOV { get; set; }
        public int HorizontalResolution { get; set; }
        public int VerticalResolution { get; set; }
        public double HorizontalPanAngle { get; set; }
        public double VerticalTiltAngle { get; set; }
        public double Pitch { get; set; }
        public double Yaw { get; set; }
        public double Roll { get; set; }

        public Matrix3x3? CameraRotationMatrix { get; set; }

    }
}
