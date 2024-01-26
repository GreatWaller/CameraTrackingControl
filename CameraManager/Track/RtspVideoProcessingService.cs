using CameraManager.OnvifCamera;
using CameraManager.RTSP;
using RTSP.RawFramesDecoding;
using RTSP.RawFramesDecoding.DecodedFrames;
using RtspClientSharpCore;
using System.Diagnostics;
using System.Drawing;
using System.Net;



namespace CameraManager.Track
{
    internal class RtspVideoProcessingService : IVideoProcessingService
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";

        private readonly CameraInfo cameraInfo;

        private readonly RtspVideoProcessingBase rtspVideoProcessingBase;

        public event DetectionEventHandler DetectionEvent;

        #region transform
        private TransformParameters _transformParameters;

        private Bitmap _bitmap;
        private System.Drawing.Imaging.BitmapData _bitmapData;

        #endregion

        private bool isCameraMoving = false;

        private Tracker tracker;

        public RtspVideoProcessingService(CameraInfo cameraInfo, IDetectionAlgorithm detectionAlgorithm)
        {
            this.cameraInfo = cameraInfo;
            _transformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(cameraInfo.VideoWidth, cameraInfo.VideoHeight),
                    ScalingPolicy.Stretch, PixelFormat.Bgr24, ScalingQuality.FastBilinear);

            _bitmap = new Bitmap(cameraInfo.VideoWidth, cameraInfo.VideoHeight,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //_bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, _bitmap.PixelFormat);

            rtspVideoProcessingBase = new RtspVideoProcessingBase();
            rtspVideoProcessingBase.FrameReceived += RtspVideoProcessingBase_FrameReceived;

            tracker = new Tracker();
        }

        private void RtspVideoProcessingBase_FrameReceived(object? sender, IDecodedVideoFrame decodedVideoFrame)
        {
            if (isCameraMoving)
            {
                //Console.WriteLine($"**************Moving...************");
                return;
            }
            Console.WriteLine($"[{DateTime.Now.ToString()}] Frame Received");
            _bitmapData = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, _bitmap.PixelFormat);
            decodedVideoFrame.TransformTo(_bitmapData.Scan0, _bitmapData.Stride, _transformParameters);
            var frame = (Bitmap)_bitmap.Clone();
            _bitmap.UnlockBits(_bitmapData);

            try
            {
                var tracks = tracker.Track(frame);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Tracking Error: {ex}]");
            }


            //_bitmap?.Save(Path.Combine("Image", "frambitmap.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);

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
