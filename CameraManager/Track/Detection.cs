using System.Runtime.InteropServices;

namespace CameraManager.Track
{
    internal class Detection
    {
        public int ClassId { get; set; }
        public float Confidence { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Detection(int classId, float confidence, int left, int top, int right, int bottom)
        {
            ClassId = classId;
            Confidence = confidence;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;

            // 计算边界框中心点x,y的像素坐标
            X = (left + right) / 2;
            Y = (top + bottom) / 2;

            // 计算边界框的像素宽度和高度
            Width = right - left;
            Height = bottom - top;
        }
    }
}