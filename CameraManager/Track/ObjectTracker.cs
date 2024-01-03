using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Extensions;
using OpenCvSharp.Tracking;
using OpenCvSharp.Dnn;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

namespace CameraManager.Track
{
    internal class ObjectTracker
    {
        private VideoCapture capture;
        private Window window;
        private Net net;
        private TrackerKCF tracker;
        private bool shouldRun = true;
        private Rect selectedObject;
        private List<Rect> objectLocations = new List<Rect>(); // 存储对象位置信息

        private int consecutiveTrackingFailures = 0;
        private int maxTrackingFailures = 5; // 举例：设置最大追踪失败次数

        // 设置只检测人的类ID
        int personClassId = 0; // 人的类ID为0

        public ObjectTracker(int cameraIndex)
        {
            capture = new VideoCapture(cameraIndex);
            //window = new Window($"Object Tracking - Camera {cameraIndex}");

            // 加载 YOLO 网络
            net = CvDnn.ReadNetFromDarknet("yolov3.weights", "yolov3.cfg");
            //tracker = new TrackerKCF();
        }

        public void Run()
        {
            Task.Run(() =>
            {
                Mat frame = new Mat();
                capture.Read(frame);

                while (!window.IsDisposed && shouldRun)
                {
                    capture.Read(frame);

                    if (frame.Empty())
                    {
                        break;
                    }

                    DetectAndTrackObjects(frame);

                    window.ShowImage(frame);

                    // 添加适当的延时，以便观察图像并避免过度占用CPU
                    Cv2.WaitKey(1);
                }
            });
        }

        public void SetSelectedObject(int mouseX, int mouseY)
        {
            selectedObject = GetClickedObject(mouseX, mouseY);
            shouldRun = false; // 结束追踪任务
        }

        public void StopTracking()
        {
            shouldRun = false;
        }

        private void DetectAndTrackObjects(Mat image)
        {
            //var grayFrame = new Mat();
            //Cv2.CvtColor(image, grayFrame, ColorConversionCodes.BGR2GRAY);

            // 创建一个 4D blob 从图像
            Mat blob = CvDnn.BlobFromImage(image, 1.0 / 255.0, new OpenCvSharp.Size(416, 416), new Scalar(0, 0, 0), true, false);

            // 设置网络的输入
            net.SetInput(blob);

            // 进行前向传播以获取输出层
            //get output layer name
            //var outNames = net.GetUnconnectedOutLayersNames();
            ////create mats for output layer
            //var output = outNames.Select(_ => new Mat()).ToArray();
            //net.Forward(output, outNames);
            Mat output = net.Forward();

            // 对输出层进行后处理以获取检测结果
            // ...
            // 后处理输出
            List<Detection> detections = new List<Detection>();
            for (int i = 0; i < output.Rows; i++)
            {
                float confidence = output.At<float>(i, 2);

                if (confidence > 0.5)
                {
                    // 只检测人
                    int classId = (int)output.At<float>(i, 1);
                    if (classId == personClassId)
                    {
                        float x = output.At<float>(i, 3);
                        float y = output.At<float>(i, 4);
                        float width = output.At<float>(i, 5);
                        float height = output.At<float>(i, 6);

                        int left = (int)((x - width / 2) * image.Width);
                        int top = (int)((y - height / 2) * image.Height);
                        int right = (int)((x + width / 2) * image.Width);
                        int bottom = (int)((y + height / 2) * image.Height);

                        detections.Add(new Detection(classId, confidence, left, top, right, bottom));
                    }
                }
            }

            // 在原图像上画出检测到的目标
            // ...


            if (detections.Count == 0)
            {
                // 当检测不到对象时，重置追踪失败次数
                consecutiveTrackingFailures = 0;
                selectedObject = Rect.Empty;
                return;
            }

            foreach (var obj in detections)
            {
                var rect = Rect.FromLTRB(obj.Left, obj.Top, obj.Right, obj.Bottom);

                if (!tracker.Update(image, ref rect))
                {
                    // 追踪失败的处理逻辑
                    consecutiveTrackingFailures++;

                    if (consecutiveTrackingFailures >= maxTrackingFailures)
                    {
                        shouldRun = false;
                        return;
                    }
                }
                else
                {
                    consecutiveTrackingFailures = 0;
                    // 其他追踪和显示逻辑
                }
            }
        }

        public void RunDetection(Mat frame)
        {
            var grayFrame = new Mat();
            Cv2.CvtColor(frame, grayFrame, ColorConversionCodes.BGR2GRAY);

            // 替换为适用于对象的检测方法
            //var detectedObjects = objectCascade.DetectMultiScale(grayFrame, 1.1, 3, HaarDetectionType.ScaleImage);


            // 将检测到的对象位置添加到列表中
            objectLocations.Clear();
            //objectLocations.AddRange(detectedObjects);
        }

        private Rect GetClickedObject(int mouseX, int mouseY)
        {
            // 适应实际情况，根据点击位置判断对象
            foreach (var obj in objectLocations)
            {
                var rect = new Rect(obj.X, obj.Y, obj.Width, obj.Height);
                if (rect.Contains(new OpenCvSharp.Point(mouseX, mouseY)))
                {
                    return rect;
                }
            }

            return Rect.Empty;
        }

        // 其他可能需要的方法和成员变量

        public void Dispose()
        {
            capture?.Dispose();
            window?.Dispose();
            net?.Dispose();
        }
    }
}
