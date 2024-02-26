using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    public delegate void DetectionEventHandler(string deviceId, Rect2d detection);
    public delegate void ImageChangeEventHandler(string deviceId, Bitmap image, Rect2d detection);
    internal interface IVideoProcessingService
    {
        public event DetectionEventHandler? DetectionEvent;
        public event ImageChangeEventHandler? ImageChangeEvent;
        void LookTo(Rect2d box);
        public void Start(string videoPath);
        public void Stop();
    }
}
