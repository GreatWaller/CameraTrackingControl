using CameraManager.OnvifCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager
{
    internal class MoveStatus
    {
        public MoveStatus(CameraStatus status)
        {
            CameraStatus = status;
        }

        public CameraStatus CameraStatus { get; set; }
        public bool IsRunning { get; set; } = false;


    }
}
