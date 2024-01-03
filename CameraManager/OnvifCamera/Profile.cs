using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.OnvifCamera
{
    internal class DeviceProfile
    {
        public List<Profile> Result { get; set; }
    }
    internal class Profile
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public bool Fixed { get; set; }
        public bool FixedFieldSpecified { get; set; }
        public VideoSourceConfiguration VideoSourceConfiguration { get; set; }
        public AudioSourceConfiguration AudioSourceConfiguration { get; set; }
        public VideoEncoderConfiguration VideoEncoderConfiguration { get; set; }
        public AudioEncoderConfiguration AudioEncoderConfiguration { get; set; }
        public VideoAnalyticsConfiguration VideoAnalyticsConfiguration { get; set; }
        public PtzConfiguration PtzConfiguration { get; set; }
        public MetadataConfiguration MetadataConfiguration { get; set; }

    }

    internal class VideoSourceConfiguration
    {
        public string Token { get; set; }
        public string ViewMode { get; set; }
    }

    internal class AudioSourceConfiguration
    {
        public string Token { get; set; }
    }

    internal class VideoEncoderConfiguration
    {
        public string VideoEncoding { get; set; }
        public int VideoWidth { get; set; }
        public int VideoHeight { get; set; }
        public double Quality { get; set; }
        public int FrameRateLimit { get; set; }
        public int EncodingInterval { get; set; }
        public int BitrateLimit { get; set; }
        public string Mpeg4Profile { get; set; }
        public string H264Profile { get; set; }
        public string MulticastIpv4Address { get; set; }
        public string MulticastIpv6Address { get; set; }
        public int MulticastPort { get; set; }
        public int Ttl { get; set; }
        public bool AutoStart { get; set; }
        public string SesstionTimeout { get; set; }
    }
    internal class AudioEncoderConfiguration
    {
        public string AudioEncoding { get; set; }
        public int Bitrate { get; set; }
        public int SampleRate { get; set; }
        public string MulticastIpv4Address { get; set; }
        public string MulticastIpv6Address { get; set; }
        public int MulticastPort { get; set; }
        public int Ttl { get; set; }
        public bool AutoStart { get; set; }
        public string SesstionTimeout { get; set; }
    }

    internal class VideoAnalyticsConfiguration
    {
    }

    internal class PtzConfiguration
    {
        public string NodeToken { get; set; }
        public string DefaultAbsolutePantTiltPositionSpace { get; set; }
        public string DefaultAbsoluteZoomPositionSpace { get; set; }
        public string DefaultRelativePanTiltTranslationSpace { get; set; }
        public string DefaultRelativeZoomTranslationSpace { get; set; }
        public string DefaultContinuousPanTiltVelocitySpace { get; set; }
        public string DefaultContinuousZoomVelocitySpace { get; set; }
        public double DefaultPanSpeed { get; set; }
        public double DefaultTiltSpeed { get; set; }
        public string DefaultPanTiltSpace { get; set; }
        public double DefaultZoomSpeed { get; set; }
        public string DefaultZoomSpace { get; set; }
        public string DefaultPTZTimeout { get; set; }
        public double PanMinLimit { get; set; }
        public double PanMaxLimit { get; set; }
        public double TiltMinLimit { get; set; }
        public double TiltMaxLimit { get; set; }
        public double ZoomMinLimit { get; set; }
        public double ZoomMaxLimit { get; set; }
        public bool MoveRampSpecified { get; set; }
        public bool PresetRampSpecified { get; set; }
        public bool PresetTourRampSpecified { get; set; }
    }

    internal class MetadataConfiguration
    {
    }
}
