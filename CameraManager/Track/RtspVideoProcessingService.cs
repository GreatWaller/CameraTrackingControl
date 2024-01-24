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
        public string DeviceAddress { get; set; } = "rtsp://192.168.1.151:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1";
        public string Login { get; set; } = "admin";
        public string Password { get; set; } = "CS@202304";

        private readonly RtspVideoProcessingBase rtspVideoProcessingBase;

        public event DetectionEventHandler DetectionEvent;


        #region transform
        private TransformParameters _transformParameters;
        private int _width = 1920;
        private int _height = 1080;
        private Mat _image;
        private object _lock = new object();
        #endregion

        public RtspVideoProcessingService(string deviecId) 
        {
            _transformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(_width, _height),
                    ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);
            _image = new Mat(_height, _width, MatType.CV_8UC4);

            rtspVideoProcessingBase = new RtspVideoProcessingBase();
            rtspVideoProcessingBase.FrameReceived += RtspVideoProcessingBase_FrameReceived;
            
        }

        private void RtspVideoProcessingBase_FrameReceived(object? sender, IDecodedVideoFrame decodedVideoFrame)
        {
            Console.WriteLine("Frame Received");

            decodedVideoFrame.TransformTo(_image.DataStart, (int)_image.Step(), _transformParameters);
            

            Cv2.ImWrite("rtspframe.jpg",_image);
        }

        public void Start(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                Trace.TraceError("StreamUri is null");
                return;
            }
            string address = DeviceAddress;

            if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                address = RtspPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
            {
                //MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(Login, Password);

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
