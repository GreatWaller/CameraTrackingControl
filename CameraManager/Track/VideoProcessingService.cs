using OpenCvSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.Tracking;
using CameraManager.OnvifCamera;

namespace CameraManager.Track
{
    internal delegate void DetectionEventHandler(string deviceId, Rect2d detection);
    internal class VideoProcessingService
    {
        private string deviceId;
        private readonly IDetectionAlgorithm _detectionAlgorithm;
        private readonly Queue<Mat> _frameQueue;

        private bool isDetceting=false;
        private bool isCameraMoving=false;
        private const int step = 4;

        private bool shouldStop=false;

        public event DetectionEventHandler DetectionEvent;
        public VideoProcessingService(string _deviceId, IDetectionAlgorithm detectionAlgorithm)
        {
            deviceId = _deviceId;
            _detectionAlgorithm = detectionAlgorithm;
            _frameQueue = new Queue<Mat>();
        }

        public void ProcessVideo(string videoPath)
        {
            // Load the video
            VideoCapture capture = new VideoCapture(videoPath);
            capture.Set(VideoCaptureProperties.BufferSize, 24);
            // Detect objects every 5 frames
            int frameCount = 0;
            using var frame = new Mat();
            // Loop over the frames
            while (true)
            {
                if (shouldStop)
                {
                    break;
                }
                // Read the frame
                capture.Read(frame);
                frameCount++;

                // If the frame is empty, break out of the loop
                if (frame.Empty())
                {
                    break;
                }

                if (isCameraMoving)
                {
                    continue;
                }
                // Detect objects every 5 frames
                if (frameCount % step != 0)
                {
                    continue;
                }

                // Detect objects in the frame
                List<Rect2d> detections = _detectionAlgorithm.DetectObjects(frame);
                // move camera
                if (detections.Count > 0)
                {
                    //var detection = detections[0];
                    var detection = detections.OrderBy(p => Math.Abs(p.X - frame.Width / 2)).First();

                    Console.WriteLine($"Detection: {detection.X}, {detection.Y}");
                    isCameraMoving = true;
                    Task task = Task.Run(() =>
                    {
                        DetectionEvent.Invoke(deviceId, detection);
                        isCameraMoving = false;
                    });
                    
                }

                // Display the frame
                Cv2.ImShow(deviceId, frame);

                // Press Esc to exit
                if (Cv2.WaitKey(1) == 27)
                {
                    break;
                }
            }

            // Release the capture object
            capture.Release();
            Cv2.DestroyWindow(deviceId);
        }

        internal void Stop()
        {
            shouldStop=true;
        }
    }
}
