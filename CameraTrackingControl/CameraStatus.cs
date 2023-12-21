using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTrackingControl
{
    public class CameraStatus
    {
        public float Azimuth { get; set; }
        public float Altitude { get; set; }
        public float Zoom { get; set; }

        public int Width { get; set; } = 2560;
        public int Height { get; set; } = 1920;

        public float FocalLength { get; set; } = 0.0048f;
        public float Fx { get; set; } = 2573.374067f;
        public float Fy { get; set; } = 2564.107019f;
        public string Msg { get; set; }
    }
}
