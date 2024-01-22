using OpenCvSharp.Dnn;
using OpenCvSharp;


namespace CameraManager.Track
{
    internal class ImageSimilarityCalculator
    {
        private Net net1;
        private Net net2;

        private object lock1= new object();
        private object lock2 = new object();

        public ImageSimilarityCalculator(string modelPath = "Config/resnet18-v2-7.onnx")
        {
            // 从ONNX模型中读取网络
            this.net1 = CvDnn.ReadNetFromOnnx(modelPath);
            this.net2 = CvDnn.ReadNetFromOnnx(modelPath);

            net1.SetPreferableBackend(Backend.OPENCV);
            net1.SetPreferableTarget(Target.OPENCL);

            net2.SetPreferableBackend(Backend.OPENCV);
            net2.SetPreferableTarget(Target.OPENCL);
        }

        private Mat ExtractFeature(Mat img, Net net)
        {
            // 将图像转换为blob
            Mat blob = CvDnn.BlobFromImage(img, 1.0, new Size(224, 224), new Scalar(0.406, 0.456,0.485), true, false);

            // 设置网络的输入
            net.SetInput(blob);

            // 运行前向传播以获取特征
            Mat feature = net.Forward();

            return feature;
        }

        public double CalculateSimilarity(Mat img1, Mat img2)
        {
            Mat feature1;
            Mat feature2;
            lock (lock1)
            {
                feature1 = ExtractFeature(img1, net1);
            }
            lock (lock2)
            {
                feature2 = ExtractFeature(img2, net2);
            }

            // 计算两个特征向量的余弦相似度
            double similarity = feature1.Dot(feature2) / (Cv2.Norm(feature1) * Cv2.Norm(feature2));
            Console.WriteLine($"Cosine Similarity: {similarity}");

            return similarity;
        }
    }
}
