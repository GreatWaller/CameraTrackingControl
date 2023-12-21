using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraTrackingControl
{
    public class ReceivedEventArgs : EventArgs
    {
        public Detection Detection { get; set; }
        public ReceivedEventArgs(Detection detection)
        {
            Detection = detection;
        }

    }
}
