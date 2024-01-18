using OpenCvSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.Tracking;
using CameraManager.OnvifCamera;
using System.Text.RegularExpressions;
using OpenCvSharp.ImgHash;
using System.Runtime.InteropServices;
using OpenCvSharp.XFeatures2D;

namespace CameraManager.Track
{
    internal delegate void DetectionEventHandler(string deviceId, Rect2d detection);
    internal class VideoProcessingService
    {
        private string deviceId;
        private readonly IDetectionAlgorithm _detectionAlgorithm;
        private readonly Queue<Mat> _frameQueue;

        private VideoCapture capture;
        private bool isDetceting=false;
        private bool isCameraMoving=false;
        private const int step = 4;

        private bool shouldStop=false;
        private object lockObject = new object();
        private Mat currentFrame = new Mat();
        const int MaxErrorFrameCount = 24;
        private int errorFrameCount = 0;

        public event DetectionEventHandler DetectionEvent;

        private Mat? lastTarget = null;

        public VideoProcessingService(string _deviceId, IDetectionAlgorithm detectionAlgorithm)
        {
            deviceId = _deviceId;
            _detectionAlgorithm = detectionAlgorithm;
            _frameQueue = new Queue<Mat>();

            
        }
        private void CaptureFrames(string videoPath)
        {
            // Load the video
            VideoCapture capture = new VideoCapture(videoPath);
            capture.Set(VideoCaptureProperties.BufferSize, 3);

            while (!shouldStop)
            {
                lock (lockObject)
                {
                    capture.Read(currentFrame);
                }

                // 在这里添加任何你需要的处理，例如通知处理线程有新的帧可用等
            }
            capture.Release();
        }

        public void ProcessVideo(string videoPath)
        {
            // 启动捕获线程
            //Thread captureThread = new Thread(() => CaptureFrames(videoPath));
            //captureThread.Start();

            // Load the video
            capture = new VideoCapture(videoPath);
            capture.Set(VideoCaptureProperties.BufferSize, 3);
            // Detect objects every 5 frames
            int frameCount = 0;
            using Mat frame= new Mat();
            // Loop over the frames
            while (true)
            {
                if (shouldStop)
                {
                    break;
                }
                // Read the frame
                //frame = currentFrame.Clone();
                try
                {
                    capture.Read(frame);
                }
                catch (Exception)
                {
                    Console.WriteLine($"[Frame Capture Failure.]");
                    continue;
                }
                frameCount++;

                // If the frame is empty, break out of the loop
                if (frame.Empty())
                {
                    if(errorFrameCount++ > MaxErrorFrameCount)
                    {
                        capture = new VideoCapture(videoPath);
                        errorFrameCount = 0;
                    }
                    
                    continue;
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
                ThreadPool.QueueUserWorkItem(work =>
                {
                    //var f = frame.Clone();
                    // move camera
                    if (detections.Count > 0)
                    {
                        if(lastTarget == null)
                        {
                            var d = detections.OrderBy(p => Math.Abs(p.X - frame.Width / 2)).First();
                            lastTarget= SubImage(frame, d);
                            Cv2.ImWrite("frame.jpg", frame);
                            Cv2.ImWrite("snapshot.jpg", lastTarget);

                        }

                        var detection = detections.Count > 1 ? FindSimilarTarget(frame, detections) : detections[0];

                        Console.WriteLine($"[Detection: {detection.X}, {detection.Y}]");
                        isCameraMoving = true;
                        DetectionEvent.Invoke(deviceId, detection);
                        isCameraMoving = false;

                    }
                });
                

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
            //captureThread.Join();
        }

        private Rect2d FindSimilarTarget(Mat image, List<Rect2d> detections)
        {
            double l = 0;
            int i = 0;
            Console.WriteLine("=================");
            for (int j = 0; j < detections.Count; j++)
            {
                var item = detections[j];
                using var detectionMat = SubImage(image, item);

                //Cv2.ImWrite("subimage.jpg", detectionMat);
                using var target = lastTarget.Clone();
                // Resize and crop the images
                (Mat resizedCroppedImage1, Mat resizedImage2) = ResizeAndCropImages(detectionMat, target);
                var similarity = CompareMSSIM(resizedCroppedImage1, resizedImage2);

                //var similarity = CompareMSSIM(detectionMat, target);
                Console.WriteLine(similarity);
                if (similarity > l)
                {
                    l = similarity;
                    i = j;
                }
            }
            Console.WriteLine("=================");

            return detections[i];
        }

        internal void Stop()
        {
            shouldStop=true;
        }

        static double PSNR(Mat image1, Mat image2)
        {
            // Check if the images have the same dimensions
            if (image1.Width != image2.Width || image1.Height != image2.Height)
            {
                // Resize the images to the same size
                Mat resizedImage1 = new Mat();
                Mat resizedImage2 = new Mat();
                Cv2.Resize(image1, resizedImage1, new Size(Math.Min(image1.Width, image2.Width), Math.Min(image1.Height, image2.Height)));
                Cv2.Resize(image2, resizedImage2, new Size(Math.Min(image1.Width, image2.Width), Math.Min(image1.Height, image2.Height)));

                // Calculate the PSNR using the resized images
                return PSNR(resizedImage1, resizedImage2);
            }
            else
            {
                // Convert the images to grayscale
                Mat grayscaleImage1 = image1.CvtColor(ColorConversionCodes.BGR2GRAY);
                Mat grayscaleImage2 = image2.CvtColor(ColorConversionCodes.BGR2GRAY);

                // Calculate the mean squared error (MSE)
                Mat diff = grayscaleImage1 - grayscaleImage2;
                double mse = Cv2.Mean(diff.Mul(diff)).Val0;

                // Calculate the peak signal-to-noise ratio (PSNR)
                double psnr = 10 * Math.Log10(255 * 255 / mse);

                return psnr;
            }
        }

        public double CalculateMSE(Mat image1, Mat image2)
        {
            if (image1.Size() != image2.Size())
            {
                // Resize images to the same size
                Cv2.Resize(image1, image1, image2.Size());
            }

            Mat diff = new Mat();
            Cv2.Absdiff(image1, image2, diff);
            diff = diff.Mul(diff);

            Scalar mse = Cv2.Mean(diff);
            return mse.Val0;
        }

        private Mat SubImage(Mat image, Rect2d bbox)
        {
            var topLeftX = bbox.X - bbox.Width / 2;
            if (topLeftX < 0)
                topLeftX = 0;
            var topLeftY = bbox.Y - bbox.Height / 2;
            if (topLeftY < 0)
                topLeftY = 0;
            var bottomRightX = bbox.X + bbox.Width / 2;
            if (bottomRightX > image.Width)
                bottomRightX = image.Width;
            var bottomRightY = bbox.Y + bbox.Height / 2;
            if (bottomRightY > image.Height)
                bottomRightY = image.Height;

            var submat = image.SubMat((int)topLeftY, (int)bottomRightY, (int)topLeftX, (int)bottomRightX);
            return submat;
        }

        public static string CalculatePerceptualHash(Mat image)
        {
            using var pHash = PHash.Create();
            using var hash = new Mat();
            pHash.Compute(image, hash);

            var data = new byte[hash.Width * hash.Height];
            Marshal.Copy(hash.Data, data, 0, data.Length);

            var hashHex = BitConverter.ToString(data).Replace("-", string.Empty);
            return hashHex;
        }

        public static int CalculateHammingDistance(string hash1, string hash2)
        {
            if (hash1.Length != hash2.Length)
                throw new ArgumentException("两个哈希值的长度必须相等。");

            int distance = 0;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    distance++;
            }

            return distance;
        }

        public double ComparePerceptualHash(Mat image1, Mat image2)
        {
            string hash1 = CalculatePerceptualHash(image1);
            string hash2 = CalculatePerceptualHash(image2);

            int distance = CalculateHammingDistance(hash1, hash2);
            Console.WriteLine($"汉明距离：{distance}");
            return (double)distance;
        }

        public static double CompareHistograms(Mat image1, Mat image2)
        {
            // 将图像转换为灰度图像
            Cv2.CvtColor(image1, image1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(image2, image2, ColorConversionCodes.BGR2GRAY);

            // 计算图像的直方图
            Mat hist1 = new Mat();
            Mat hist2 = new Mat();
            int[] hdims = { 256 }; // 直方图大小，即直方图中柱子的数量
            Rangef[] ranges = { new Rangef(0, 256) }; // 像素值范围
            Cv2.CalcHist(new Mat[] { image1 }, new int[] { 0 }, null, hist1, 1, hdims, ranges);
            Cv2.CalcHist(new Mat[] { image2 }, new int[] { 0 }, null, hist2, 1, hdims, ranges);

            // 归一化直方图
            Cv2.Normalize(hist1, hist1, alpha: 0, beta: 1, normType: NormTypes.MinMax);
            Cv2.Normalize(hist2, hist2, alpha: 0, beta: 1, normType: NormTypes.MinMax);

            // 比较直方图
            double correlation = Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);

            return correlation;
        }
        public double CompareHistAllChannel(Mat matA, Mat matB)
        {
            // 拆分通道
            Cv2.Split(matA, out Mat[] matA_S);
            Cv2.Split(matB, out Mat[] matB_S);

            //直方图的像素范围   
            Rangef[] histRange = { new Rangef(0, 256) };

            //直方图数组大小
            int[] histSize = { 256 };

            //直方图输出数组
            Mat hist_A = new Mat();
            Mat hist_B = new Mat();

            bool uniform = true, accumulate = false;
            Cv2.CalcHist(matA_S, new int[] { 0, 1, 2 }, null, hist_A, 1, histSize, histRange, uniform, accumulate);
            Cv2.CalcHist(matB_S, new int[] { 0, 1, 2 }, null, hist_B, 1, histSize, histRange, uniform, accumulate);

            //归一化，排除图像分辨率不一致的影响
            Cv2.Normalize(hist_A, hist_A, 0, 1, NormTypes.MinMax, -1, null);
            Cv2.Normalize(hist_B, hist_B, 0, 1, NormTypes.MinMax, -1, null);

            //相关性比较
            var res = Cv2.CompareHist(hist_A, hist_B, HistCompMethods.Correl);
            return res;
        }

        public static double CompareMSSIM(Mat i1, Mat i2)
        {
            using var image1 = new Mat();
            Cv2.Resize(i1, image1, new OpenCvSharp.Size(i2.Size().Width, i2.Size().Height));

            const double C1 = 6.5025, C2 = 58.5225;
            MatType d = MatType.CV_32F;

            using Mat I1 = new Mat(), I2 = new Mat();
            image1.ConvertTo(I1, d);
            i2.ConvertTo(I2, d);

            using Mat I2_2 = I2.Mul(I2);
            using Mat I1_2 = I1.Mul(I1);
            using Mat I1_I2 = I1.Mul(I2);

            using Mat mu1 = new Mat(), mu2 = new Mat();
            Cv2.GaussianBlur(I1, mu1, new OpenCvSharp.Size(11, 11), 1.5);
            Cv2.GaussianBlur(I2, mu2, new OpenCvSharp.Size(11, 11), 1.5);

            using Mat mu1_2 = mu1.Mul(mu1);
            using Mat mu2_2 = mu2.Mul(mu2);
            using Mat mu1_mu2 = mu1.Mul(mu2);

            Mat sigma1_2 = new Mat(), sigma2_2 = new Mat(), sigma12 = new Mat();

            Cv2.GaussianBlur(I1_2, sigma1_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma1_2 -= mu1_2;

            Cv2.GaussianBlur(I2_2, sigma2_2, new OpenCvSharp.Size(11, 11), 1.5);
            sigma2_2 -= mu2_2;

            Cv2.GaussianBlur(I1_I2, sigma12, new OpenCvSharp.Size(11, 11), 1.5);
            sigma12 -= mu1_mu2;

            Mat t1, t2, t3;

            t1 = 2 * mu1_mu2 + C1;
            t2 = 2 * sigma12 + C2;
            t3 = t1.Mul(t2);

            t1 = mu1_2 + mu2_2 + C1;
            t2 = sigma1_2 + sigma2_2 + C2;
            t1 = t1.Mul(t2);

            using Mat ssim_map = new Mat();
            Cv2.Divide(t3, t1, ssim_map);

            Scalar mssim = Cv2.Mean(ssim_map);

            var result = (mssim.Val0 + mssim.Val1 + mssim.Val2) / 3;

            return result;
        }

        

        public static double ComputeSimilarity(Mat img1, Mat img2)
        {
            double hessianThresh = 400.0;
            using (var surf = SURF.Create(hessianThresh))
            {
                // Detect the keypoints and compute descriptors for each image
                KeyPoint[] keypoints1, keypoints2;
                Mat descriptors1 = new Mat(), descriptors2 = new Mat();
                surf.DetectAndCompute(img1, null, out keypoints1, descriptors1);
                surf.DetectAndCompute(img2, null, out keypoints2, descriptors2);

                if(descriptors1.Rows ==0 ||  descriptors2.Rows == 0)
                {
                    return 0;
                }
                // Match descriptors between the two images
                using (var matcher = new FlannBasedMatcher())
                {
                    var matches = matcher.Match(descriptors1, descriptors2);

                    // Filter matches using the Lowe's ratio test
                    const double ratioThresh = 0.5;
                    var goodMatches = new List<DMatch>();
                    foreach (var m in matches)
                    {
                        if (m.Distance < ratioThresh)
                        {
                            goodMatches.Add(m);
                        }
                    }

                    // Compute the similarity as the ratio of good matches to total keypoints
                    double similarity = (double)goodMatches.Count / (double)(keypoints1.Length + keypoints2.Length);
                    return similarity;
                }
            }
        }
        /// <summary>
        /// Resizes and crops two images to match the aspect ratio and size of the smaller image.
        /// </summary>
        /// <param name="image1">The first image.</param>
        /// <param name="image2">The second image.</param>
        /// <returns>A tuple containing the resized and cropped version of the first image and the resized version of the second image.</returns>
        public static (Mat, Mat) ResizeAndCropImages(Mat image1, Mat image2)
        {
            // Get the aspect ratio of the smaller image
            double aspectRatio = Math.Min(image1.Width / (double)image2.Width, image1.Height / (double)image2.Height);

            // Calculate the new size of the smaller image
            Size newSize = new Size((int)(image2.Width * aspectRatio), (int)(image2.Height * aspectRatio));

            // Resize the smaller image to match the aspect ratio of the larger image
            Mat resizedImage2 = new Mat();
            Cv2.Resize(image2, resizedImage2, newSize);

            // Crop the larger image to match the size of the resized smaller image
            Rect cropRect = new Rect(0, 0, resizedImage2.Width, resizedImage2.Height);
            Mat croppedImage1 = image1[cropRect];

            // Resize the cropped image to match the size of the resized smaller image
            Mat resizedCroppedImage1 = new Mat();
            Cv2.Resize(croppedImage1, resizedCroppedImage1, newSize);

            // Return the resized and cropped images
            return (resizedCroppedImage1, resizedImage2);
        }
    }
}

