using CameraManager.OnvifCamera;
using CameraManager.RTSP;
using OpenCvSharp;
using RTSP.RawFramesDecoding;
using RTSP.RawFramesDecoding.DecodedFrames;
using RTSP.RawFramesReceiving;
using RtspClientSharpCore;
using SimpleRtspPlayer.GUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    internal class RtspVideoProcessingService : IVideoProcessingService
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";
        //public string DeviceAddress { get; set; } = "rtsp://192.168.1.151:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1";
        //public string Login { get; set; } = "admin";
        //public string Password { get; set; } = "CS@202304";

        private readonly CameraInfo cameraInfo;

        private readonly RtspVideoProcessingBase rtspVideoProcessingBase;

        public event DetectionEventHandler DetectionEvent;

        private readonly IDetectionAlgorithm _detectionAlgorithm;


        #region transform
        private TransformParameters _transformParameters;
        private int _width = 1920;
        private int _height = 1080;
        private Mat _image;
        private object _lock = new object();
        #endregion

        private bool isCameraMoving = false;
        private Mat? lastTarget = null;


        public RtspVideoProcessingService(CameraInfo cameraInfo, IDetectionAlgorithm detectionAlgorithm)
        {
            this.cameraInfo = cameraInfo;
            _transformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(cameraInfo.VideoWidth, cameraInfo.VideoHeight),
                    ScalingPolicy.Stretch, PixelFormat.Bgr24, ScalingQuality.FastBilinear);
            _image = new Mat(cameraInfo.VideoHeight, cameraInfo.VideoWidth, MatType.CV_8UC3);

            rtspVideoProcessingBase = new RtspVideoProcessingBase();
            rtspVideoProcessingBase.FrameReceived += RtspVideoProcessingBase_FrameReceived;
            _detectionAlgorithm = detectionAlgorithm;
        }

        private void RtspVideoProcessingBase_FrameReceived(object? sender, IDecodedVideoFrame decodedVideoFrame)
        {
            if (isCameraMoving)
            {
                //Console.WriteLine($"**************Moving...************");
                return;
            }
            //Console.WriteLine($"[{DateTime.Now.ToString()}] Frame Received");
            decodedVideoFrame.TransformTo(_image.DataStart, (int)_image.Step(), _transformParameters);
            //Cv2.ImWrite("rtspframe.jpg", _image);

            ThreadPool.QueueUserWorkItem(work =>
            {
                List<Rect2d> detections = _detectionAlgorithm.DetectObjects(_image);

                if (detections.Count > 0)
                {
                    var detection = detections.OrderBy(p => Math.Abs(p.X - _image.Width / 2)).First();

                    //Console.WriteLine($"[Detection: {detection.X}, {detection.Y}]");

                    isCameraMoving = true;
                    DetectionEvent.Invoke(cameraInfo.DeviceId, detection);
                    isCameraMoving = false;
                }
            });

            //Cv2.ImShow("RTSP", _image);
            //Cv2.WaitKey(1);
        }

        public void Start(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                Trace.TraceError("StreamUri is null");
                return;
            }
            string address = videoPath;

            if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                address = RtspPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
            {
                //MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(cameraInfo.UserName, cameraInfo.Password);

            var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) :
                new ConnectionParameters(deviceUri, credential);

            connectionParameters.RtpTransport = RtpTransportProtocol.TCP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);


            rtspVideoProcessingBase.Start(connectionParameters);
            //_mainWindowModel.StatusChanged += MainWindowModelOnStatusChanged;
        }

        public void Stop()
        {
            rtspVideoProcessingBase.Stop();
        }
    }
}
