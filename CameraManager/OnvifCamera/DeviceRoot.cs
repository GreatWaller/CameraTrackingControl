using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.OnvifCamera
{
    internal class DeviceRoot
    {
        public DeviceResult Result { get; set; }
    }

    internal class DeviceResult
    {
        public int TotalCount { get; set; }
        public List<Device> Items { get; set;}
    }
}
