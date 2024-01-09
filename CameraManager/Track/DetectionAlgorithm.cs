using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace CameraManager.Track
{
    internal class DetectionAlgorithm : IDetectionAlgorithm
    {
        private static DetectionAlgorithm _instance;
        private static readonly object _lock = new object();
        // 设置只检测人的类ID
        const int personClassId = 0; // 人的类ID为0

        private Net _net;
        //get output layer name
        string[] outNames;
        //create mats for output layer
        Mat[] outs;

        public DetectionAlgorithm()
        {
            // Load the model
            string cfg_path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "yolov3.cfg");
            string weight_path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "yolov3.weights");

            _net = CvDnn.ReadNetFromDarknet(cfg_path, weight_path);
            _net.SetPreferableBackend(Backend.OPENCV);
            _net.SetPreferableTarget(Target.OPENCL);

            //get output layer name
            outNames = _net.GetUnconnectedOutLayersNames();
            //create mats for output layer
            outs = outNames.Select(_ => new Mat()).ToArray();
        }

        public static DetectionAlgorithm Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DetectionAlgorithm();
                    }
                    return _instance;
                }
            }
        }

        public List<Rect2d> DetectObjects(Mat image)
        {
            // Preprocess the image
            using Mat blob = CvDnn.BlobFromImage(image, 1 / 255.0, new Size(416, 416), new Scalar(0, 0, 0), true, false);

            ////get output layer name
            //var outNames = _net.GetUnconnectedOutLayersNames();
            ////create mats for output layer
            //var outs = outNames.Select(_ => new Mat()).ToArray();
            lock (_lock)
            {
                // Set the input to the network
                _net.SetInput(blob);

                // Forward pass the network
                //using Mat output = _net.Forward();

                _net.Forward(outs, outNames);
                
            }

            //get result from all output
            var boxes = GetResult(outs, image, 0.5f, 0.3f);



            // Postprocess the output
            //List<Detection> detections = new List<Detection>();
            //for (int i = 0; i < output.Rows; i++)
            //{
            //    float confidence = output.At<float>(i, 2);
            //    // 只检测人
            //    int classId = (int)output.At<float>(i, 1);
            //    if (classId == personClassId)
            //    {
            //        //int classId = (int)output.At<float>(i, 1);
            //        float x = output.At<float>(i, 3);
            //        float y = output.At<float>(i, 4);
            //        float width = output.At<float>(i, 5);
            //        float height = output.At<float>(i, 6);

            //        int left = (int)((x - width / 2) * image.Width);
            //        int top = (int)((y - height / 2) * image.Height);
            //        int right = (int)((x + width / 2) * image.Width);
            //        int bottom = (int)((y + height / 2) * image.Height);

            //        detections.Add(new Detection(classId, confidence, left, top, right, bottom));
            //    }
            //}

            return boxes;
        }

        /// <summary>
        /// Get result form all output
        /// </summary>
        /// <param name="output"></param>
        /// <param name="image"></param>
        /// <param name="threshold"></param>
        /// <param name="nmsThreshold">threshold for nms</param>
        /// <param name="nms">Enable Non-maximum suppression or not</param>
        private static List<Rect2d> GetResult(IEnumerable<Mat> output, Mat image, float threshold, float nmsThreshold, bool nms = true)
        {
            //for nms
            var classIds = new List<int>();
            var confidences = new List<float>();
            var probabilities = new List<float>();
            var boxes = new List<Rect2d>();

            var w = image.Width;
            var h = image.Height;
            /*
             YOLO3 COCO trainval output
             0 1 : center                    2 3 : w/h
             4 : confidence                  5 ~ 84 : class probability 
            */
            const int prefix = 5;   //skip 0~4

            foreach (var prob in output)
            {
                for (var i = 0; i < prob.Rows; i++)
                {
                    var confidence = prob.At<float>(i, 4);
                    if (confidence > threshold)
                    {
                        //get classes probability
                        Cv2.MinMaxLoc(prob.Row(i)
                            .ColRange(prefix, prob.Cols), out _, out Point max);
                        var classes = max.X;

                        if (classes != personClassId)
                        {
                            continue;
                        }
                        var probability = prob.At<float>(i, classes + prefix);

                        if (probability > threshold) //more accuracy, you can cancel it
                        {
                            //get center and width/height
                            var centerX = prob.At<float>(i, 0) * w;
                            var centerY = prob.At<float>(i, 1) * h;
                            var width = prob.At<float>(i, 2) * w;
                            var height = prob.At<float>(i, 3) * h;

                            if (!nms)
                            {
                                // draw result (if don't use NMSBoxes)
                                Draw(image, classes, confidence, probability, centerX, centerY, width, height);
                                continue;
                            }

                            //put data to list for NMSBoxes
                            classIds.Add(classes);
                            confidences.Add(confidence);
                            probabilities.Add(probability);
                            boxes.Add(new Rect2d(centerX, centerY, width, height));
                        }
                    }
                }
            }

            if (!nms) return boxes;

            //using non-maximum suppression to reduce overlapping low confidence box
            CvDnn.NMSBoxes(boxes, confidences, threshold, nmsThreshold, out int[] indices);

            //Console.WriteLine($"NMSBoxes drop {confidences.Count - indices.Length} overlapping result.");

            foreach (var i in indices)
            {
                var box = boxes[i];
                Draw(image, classIds[i], confidences[i], probabilities[i], box.X, box.Y, box.Width, box.Height);
            }

            return boxes;
        }

        /// <summary>
        /// Draw result to image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="classes"></param>
        /// <param name="confidence"></param>
        /// <param name="probability"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void Draw(Mat image, int classes, float confidence, float probability, double centerX, double centerY, double width, double height)
        {
            //label formating
            var label = $"{classes} {probability * 100:0.00}%";
            //Console.WriteLine($"confidence {confidence * 100:0.00}% {label}");
            var x1 = (centerX - width / 2) < 0 ? 0 : centerX - width / 2; //avoid left side over edge
            //draw result
            image.Rectangle(new Point(x1, centerY - height / 2), new Point(centerX + width / 2, centerY + height / 2), new Scalar(0,255,0), 2);
            var textSize = Cv2.GetTextSize(label, HersheyFonts.HersheyTriplex, 0.5, 1, out var baseline);
            Cv2.Rectangle(image, new Rect(new Point(x1, centerY - height / 2 - textSize.Height - baseline),
                new Size(textSize.Width, textSize.Height + baseline)), new Scalar(0, 255, 0), Cv2.FILLED);
            var textColor = Cv2.Mean(classes).Val0 < 70 ? Scalar.White : Scalar.Black;
            Cv2.PutText(image, label, new Point(x1, centerY - height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.5, textColor);
        }
    }
}
    
