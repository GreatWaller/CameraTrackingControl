using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager
{
    internal class Matrix3x3
    {
        public double M11 { get; set; }
        public double M12 { get; set; }
        public double M13 { get; set; }
        public double M21 { get; set; }
        public double M22 { get; set; }
        public double M23 { get; set; }
        public double M31 { get; set; }
        public double M32 { get; set; }
        public double M33 { get; set; }

        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3
            {
                M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            };
        }

        public static Point3F operator *(Matrix3x3 matrix, Point3F point)
        {
            return new Point3F(
                matrix.M11 * point.X + matrix.M12 * point.Y + matrix.M13 * point.Z,
                matrix.M21 * point.X + matrix.M22 * point.Y + matrix.M23 * point.Z,
                matrix.M31 * point.X + matrix.M32 * point.Y + matrix.M33 * point.Z
            );
        }

        // 计算相机的旋转矩阵
        public static Matrix3x3 CalculateCameraRotationMatrix(double pitch, double yaw, double roll)
        {
            // 计算相机的旋转矩阵
            var cameraRotationMatrix = new Matrix3x3
            {
                M11 = Math.Cos(yaw) * Math.Cos(roll),
                M12 = Math.Cos(yaw) * Math.Sin(roll) * Math.Sin(pitch) - Math.Sin(yaw) * Math.Cos(pitch),
                M13 = Math.Cos(yaw) * Math.Sin(roll) * Math.Cos(pitch) + Math.Sin(yaw) * Math.Sin(pitch),
                M21 = Math.Sin(yaw) * Math.Cos(roll),
                M22 = Math.Sin(yaw) * Math.Sin(roll) * Math.Sin(pitch) + Math.Cos(yaw) * Math.Cos(pitch),
                M23 = Math.Sin(yaw) * Math.Sin(roll) * Math.Cos(pitch) - Math.Cos(yaw) * Math.Sin(pitch),
                M31 = -Math.Sin(roll),
                M32 = Math.Cos(roll) * Math.Sin(pitch),
                M33 = Math.Cos(roll) * Math.Cos(pitch)
            };

            return cameraRotationMatrix;
        }
    }
}
