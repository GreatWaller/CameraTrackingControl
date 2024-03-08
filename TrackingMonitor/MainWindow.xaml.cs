using CameraManager;
using RTSP.RawFramesDecoding.DecodedFrames;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Interop;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using OpenCvSharp.Extensions;

namespace TrackingMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CameraController controller;
        private string baseUri = "https://192.168.1.40:44311/api/services/app/";
        //private string baseUri = "https://localhost:44311/api/services/app/";


        private WriteableBitmap _writeableBitmap;
        private Int32Rect _dirtyRect;

        public MainWindow()
        {
            InitializeComponent();

            controller = new CameraController(baseUri);
            controller.ImageChangeEvent += Controller_ImageChangeEvent;

            _writeableBitmap = new WriteableBitmap(1920, 1080, 96, 96, PixelFormats.Bgr24, null);
            _dirtyRect = new Int32Rect(0, 0, 1920, 1080);
            VideoImage.Source = _writeableBitmap;

            VideoImage.MouseDown += VideoImage_MouseDown;
        }

        private void VideoImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = Mouse.GetPosition(VideoImage);
            var x = mousePosition.X / VideoImage.ActualWidth * 1920;
            var y = mousePosition.Y / VideoImage.ActualHeight * 1080;
            Trace.TraceInformation($"[Mouse Position: {x},{y}]");
            controller.Click("Cam-7de4e6c1", x, y);
        }

        private unsafe void Controller_ImageChangeEvent(string deviceId, System.Drawing.Bitmap bitmap, OpenCvSharp.Rect2d detection)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                #region opencv
                var frame = (Bitmap)bitmap.Clone();
                var mat = BitmapConverter.ToMat(frame);
                if (detection.Width >=0)
                {
                    Draw(mat,1,detection.X, detection.Y, detection.Width, detection.Height);
                }
                //OpenCvSharp.Cv2.ImWrite("save.jpg", mat);
                _writeableBitmap.Lock();
                try
                {
                    _writeableBitmap.WritePixels(_dirtyRect, mat.Data, mat.Height * (int)mat.Step(), (int)mat.Step());
                }
                catch (Exception ex)
                {
                    Trace.TraceError(deviceId, ex);
                }
                finally
                {
                    _writeableBitmap.Unlock();
                }
                #endregion


                #region bitmap
                ////var frame = (Bitmap)bitmap.Clone();
                //BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                //    System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
                //_writeableBitmap.Lock();
                //try
                //{
                //    int rPixelBytes = data.Stride * data.Height;
                //    _writeableBitmap.WritePixels(_dirtyRect, mat.Data, data.Height * data.Stride, data.Stride);

                //    _writeableBitmap.AddDirtyRect(_dirtyRect);
                //}
                //catch(Exception ex)
                //{
                //    Trace.TraceError(deviceId, ex);
                //}
                //finally
                //{
                //    _writeableBitmap.Unlock();
                //    bitmap.UnlockBits(data);
                //}
                #endregion
            }));
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            try
            {
                controller.CreateVideoProcess("Cam-7de4e6c1");
            }
            catch (Exception)
            {

            }
        }


        private static void Draw(OpenCvSharp.Mat image, int classes, double centerX, double centerY, double width, double height)
        {
            //label formating
            var label = $"{classes}";
            //Console.WriteLine($"confidence {confidence * 100:0.00}% {label}");
            var x1 = (centerX - width / 2) < 0 ? 0 : centerX - width / 2; //avoid left side over edge
            //draw result
            image.Rectangle(new OpenCvSharp.Point(x1, centerY - height / 2), new OpenCvSharp.Point(centerX + width / 2, centerY + height / 2), new OpenCvSharp.Scalar(0, 255, 0), 2);
            //var textSize = OpenCvSharp.Cv2.GetTextSize(label, OpenCvSharp.HersheyFonts.HersheyTriplex, 0.5, 1, out var baseline);
            //OpenCvSharp.Cv2.Rectangle(image, new OpenCvSharp.Rect(new OpenCvSharp.Point(x1, centerY - height / 2 - textSize.Height - baseline),
            //    new OpenCvSharp.Size(textSize.Width, textSize.Height + baseline)), new OpenCvSharp.Scalar(0, 255, 0), OpenCvSharp.Cv2.FILLED);
            //var textColor = Cv2.Mean(classes).Val0 < 70 ? Scalar.White : Scalar.Black;
            //Cv2.PutText(image, label, new OpenCvSharp.Point(x1, centerY - height / 2 - baseline), HersheyFonts.HersheyTriplex, 0.5, textColor);
        }
    }
}