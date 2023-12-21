using CameraTrackingControl.Onvif;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTrackingControl
{
    internal class TrackingController
    {
        private CameraStatus currentCameraStatus = new CameraStatus();
        private ICamera cameraApi = new OnvifCamera();

        private FixedSizeQueue<Detection> detections = new FixedSizeQueue<Detection>(24);
        private bool isMoving = false;

        public TrackingController()
        {
            // cemreaStatus
            UpdateCameraStatus();
        }

        private void UpdateCameraStatus()
        {
            var status = cameraApi.GetCurrentStatus(currentCameraStatus);
            currentCameraStatus.Altitude = status.Altitude;
            currentCameraStatus.Azimuth = status.Azimuth;
            currentCameraStatus.Zoom = status.Zoom;
        }

        public void Track(ReceivedEventArgs e)
        {
            Console.WriteLine("[*****************************] Event Handling...");
            detections.Enqueue(e.Detection);

            var deltaAzi = MathF.Atan((e.Detection.CenterX - 0.5f)*currentCameraStatus.Width/currentCameraStatus.Fx);

            var deltaAlt = MathF.Atan((e.Detection.CenterY - 0.5f) * currentCameraStatus.Height/currentCameraStatus.Fy);

            UpdateCamera(deltaAlt, deltaAzi);
        }

        private void UpdateCamera(float deltaAlt, float deltaAzi)
        {
            // azi-> (0, 360)
            var azi = deltaAzi * 180 / MathF.PI;
            var alt = deltaAlt * 180 / MathF.PI;

            if (Math.Abs(azi) > 5.0f || Math.Abs(alt) > 5.0f)
            {
                Console.WriteLine($"[===========To Move============]");
                var aziDest = currentCameraStatus.Azimuth + azi;
                if (aziDest > 360)
                {
                    aziDest = -360 + currentCameraStatus.Azimuth + azi;
                }
                else if (aziDest < 0)
                {
                    aziDest = 360 + currentCameraStatus.Azimuth + azi;
                }

                // alt -> (-10, 80)
                var altDest = currentCameraStatus.Altitude + alt < -10.0f? -10.0f: currentCameraStatus.Altitude + alt;
                // change camera orientation
                cameraApi.Rotate(aziDest, altDest, currentCameraStatus.Zoom);
                // update camera status：GetCurrentStatus
                UpdateCameraStatus();
            }



        }


    }
}
