using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CameraManager.Track
{
    internal class MatcherOption
    {
        public string? SourceFilePath { get; set; }

        public string? TargetFilePath { get; set; }

        public string? DetectorFilePath { get; set; } = "Models/Yolo/yolo640v5.onnx";

        public MatcherType MatcherType { get; set; } = MatcherType.DeepSort;

        public YoloVersion YoloVersion { get; set; } = YoloVersion.Yolo640;

        public AppearanceExtractorVersion AppearanceExtractorVersion { get; set; } = AppearanceExtractorVersion.OSNet;

        public string? AppearanceExtractorFilePath { get; set; } = "Models/Reid/osnet_x1_0_msmt17.onnx";

        public float? FPS { get; set; }

        public float? Threshold { get; set; }

        public float? AppearanceWeight { get; set; }

        public float? SmoothAppearanceWeight { get; set; }

        public int? MinStreak { get; set; }

        public int? MaxMisses { get; set; }

        public int? FramesToAppearanceSmooth { get; set; }

        public int? ExtractorsInMemoryCount { get; set; }

        public float TargetConfidence { get; set; } = 0.4f;
    }

    public enum MatcherType : byte
    {
        DeepSort = 0,
        Sort,
        Deep
    }

    public enum YoloVersion : byte
    {
        Yolo1280 = 0,
        Yolo640
    }

    public enum AppearanceExtractorVersion : byte
    {
        OSNet = 0,
        FastReid
    }
}
