using CameraManager.OnvifCamera;
using MOT.CORE.Matchers.Abstract;
using OpenCvSharp;
using RTSP.RawFramesDecoding.DecodedFrames;
using RTSP.RawFramesDecoding;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    internal class VlcVideoProcessingService : IVideoProcessingService
    {
        public event DetectionEventHandler? DetectionEvent;
        public event ImageChangeEventHandler? ImageChangeEvent;

        private readonly CameraInfo cameraInfo;
        private bool isCameraMoving = false;
        private Tracker tracker;

        #region track a certain target
        private bool isLooking = false;
        private int trackId = 0;
        private Rect2d targetBox;
        private bool isClosing = false;
        #endregion

        private VlcVideoProvider videoProvider;

        public VlcVideoProcessingService(CameraInfo cameraInfo)
        {
            this.cameraInfo = cameraInfo;
            //cameraInfo.ServerStreamUri = "rtsp://192.168.1.210:554/ch3";

            videoProvider = new VlcVideoProvider(cameraInfo.ServerStreamUri, (uint)cameraInfo.VideoWidth, (uint)cameraInfo.VideoHeight);
            videoProvider.FrameReceived += VideoProvider_FrameReceived;
            tracker = new Tracker();
        }

        private void VideoProvider_FrameReceived(object? sender, System.Drawing.Bitmap e)
        {
            if (isClosing)
            {
                return;
            }
            Console.WriteLine($"[{DateTime.Now}] Frame Received");
            if (isCameraMoving)
            {
                //Console.WriteLine($"**************Moving...************");
                return;
            }
            var frame = (Bitmap)e.Clone();

            try
            {
                var tracks = tracker.Track(frame);

                foreach (var track in tracks)
                {
                    Console.WriteLine($"**********************************************************************************[Track Id: {track.Id}]");
                }


                if (tracks.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(work =>
                    {
                        ITrack target;
                        if (isLooking)
                        {
                            // find target closest to user click
                            target = tracks.OrderBy(p =>
                            {
                                var box = p.CurrentBoundingBox;
                                var dx = box.X + box.Width / 2 - targetBox.X;
                                var dy = box.Y + box.Height / 2 - targetBox.Y;
                                return dx * dx + dy * dy;

                            }).First();

                            trackId = target.Id;
                            isLooking = false;
                        }
                        else
                        {
                            target = tracks.OrderBy(p => MathF.Abs(p.Id - trackId)).First();
                            //target = tracks.OrderByDescending(p => p.Id).First();
                        }


                        var box = target.CurrentBoundingBox;
                        Rect2d detection = new Rect2d((double)(box.X + box.Width / 2), (double)(box.Y + box.Height / 2), box.Width, box.Height);
                        if (isCameraMoving)
                        {
                            Console.WriteLine($"***********************************Moving...*******************************************");
                            return;
                        }
                        isCameraMoving = true;
                        DetectionEvent?.Invoke(cameraInfo.DeviceId, detection);
                        ImageChangeEvent?.Invoke(cameraInfo.DeviceId, frame, detection);

                        isCameraMoving = false;
                    });
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Tracking Error: {ex}]");
            }

        }

        public void LookTo(Rect2d box)
        {
            targetBox = box;
            isLooking = true;
        }

        public void Start(string videoPath)
        {
            videoProvider?.Start();
        }

        public void Stop()
        {
            videoProvider?.Stop();
        }
    }
}
