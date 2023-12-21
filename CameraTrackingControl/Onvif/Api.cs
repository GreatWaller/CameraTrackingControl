using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTrackingControl.Onvif
{
    public class PostmanCollection
    {
        public List<ApiRequest> Item { get; set; }
    }
    public class ApiRequest
    {
        public string Name { get; set; }
        public ApiRequestInfo Request { get; set; }
    }

    public class ApiRequestInfo
    {
        public string Method { get; set; }
        public ApiRequestUrlInfo Url { get; set; }
        public List<ApiRequestHeaderInfo> Header { get; set; }
    }

    public class ApiRequestUrlInfo
    {
        public string Raw { get; set; }
        public string Protocol { get; set; }
        public List<string> Host { get; set; }
        public string Port { get; set; }
        public List<string> Path { get; set; }
        public List<ApiRequestQueryParamInfo> Query { get; set; }
    }

    public class ApiRequestHeaderInfo
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    public class ApiRequestQueryParamInfo
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class PTZStatus
    {
        public float PanPosition { get; set; }
        public float TiltPosition { get; set; }
        public string PanTiltSpace { get; set; }
        public float ZoomPosition { get; set; }
        public string ZoomSpace { get; set; }
        public string PanTiltStatus { get; set; }
        public string ZoomStatus { get; set; }
        public DateTime UtcDateTime { get; set; }
        public string Error { get; set; }
    }
}
