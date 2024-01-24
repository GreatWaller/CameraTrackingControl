using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    internal delegate void DetectionEventHandler(string deviceId, Rect2d detection);
    internal interface IVideoProcessingService
    {
        public event DetectionEventHandler DetectionEvent;
        public void Start(string videoPath);
        public void Stop();
    }
}
