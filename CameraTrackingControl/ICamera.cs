using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace CameraTrackingControl
{
    public class PTZAbsoluteEx
    {
        public float elevation { get; set; }
        public float azimuth { get; set; }
        public float absoluteZoom { get; set; }
    }
    internal interface ICamera
    {
        public CameraStatus GetCurrentStatus(CameraStatus originalStatus);

        public void Rotate(float azi, float alt, float zoom);
        
    }
}
