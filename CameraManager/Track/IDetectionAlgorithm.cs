using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    internal interface IDetectionAlgorithm
    {
        List<Rect2d> DetectObjects(Mat image);
    }
}
