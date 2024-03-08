using Microsoft.ML.OnnxRuntime;
using MOT.CORE.Matchers.Abstract;
using MOT.CORE.Matchers.Deep;
using MOT.CORE.Matchers.SORT;
using MOT.CORE.ReID.Models.Fast_Reid;
using MOT.CORE.ReID.Models.OSNet;
using MOT.CORE.ReID;
using MOT.CORE.YOLO.Models;
using MOT.CORE.YOLO;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CameraManager.Track
{
    internal class Tracker: IDisposable
    {
        private MatcherOption matcherOption;
        private float targetConfidence;
        private Matcher matcher;

        public Tracker(string configPath="config.json")
        {
            matcherOption = ReadMatcherOptionFromJson(configPath);
            
            matcher = ConstructMatcherFromOptions(matcherOption);
            float targetConfidence = float.Clamp(matcherOption.TargetConfidence, 0.0f, 1.0f);
        }
        

        public IReadOnlyList<ITrack> Track(Bitmap frame)
        {
            IReadOnlyList<ITrack> tracks = matcher.Run(frame, targetConfidence, DetectionObjectType.Person);
            return tracks;
        }



        private static Matcher ConstructMatcherFromOptions(MatcherOption options)
        {
            IPredictor predictor = ConstructPredictorFromOptions(options);

            Matcher matcher = options.MatcherType switch
            {
                MatcherType.DeepSort => new DeepSortMatcher(predictor,
                    ConstructAppearanceExtractorFromOptions(options),
                    options.AppearanceWeight ?? 0.775f,
                    options.Threshold ?? 0.5f,
                    options.MaxMisses ?? 50,
                    options.FramesToAppearanceSmooth ?? 40,
                    options.SmoothAppearanceWeight ?? 0.875f,
                    options.MinStreak ?? 8),

                MatcherType.Sort => new SortMatcher(predictor,
                    options.Threshold ?? 0.3f,
                    options.MaxMisses ?? 15,
                    options.MinStreak ?? 3),

                MatcherType.Deep => new DeepMatcher(predictor,
                    ConstructAppearanceExtractorFromOptions(options),
                    options.Threshold ?? 0.875f,
                    options.MaxMisses ?? 10,
                    options.MinStreak ?? 4),

                _ => throw new Exception("Matcher cannot be constructed.")
            };

            return matcher;
        }

        private static IPredictor ConstructPredictorFromOptions(MatcherOption options)
        {
            if (string.IsNullOrEmpty(options.DetectorFilePath))
                throw new ArgumentNullException($"{nameof(options.DetectorFilePath)} was undefined.");

            IPredictor predictor = options.YoloVersion switch
            {
                YoloVersion.Yolo640 => new YoloScorer<Yolo640v5>(File.ReadAllBytes(options.DetectorFilePath), SessionOptions.MakeSessionOptionWithCudaProvider()),
                YoloVersion.Yolo1280 => new YoloScorer<Yolo1280v5>(File.ReadAllBytes(options.DetectorFilePath), SessionOptions.MakeSessionOptionWithCudaProvider()),
                _ => throw new Exception("Yolo predictor cannot be constructed.")
            };

            return predictor;
        }

        private static IAppearanceExtractor ConstructAppearanceExtractorFromOptions(MatcherOption options)
        {
            if (string.IsNullOrEmpty(options.AppearanceExtractorFilePath))
                throw new ArgumentNullException($"{nameof(options.AppearanceExtractorFilePath)} was undefined.");

            if (options.AppearanceExtractorVersion == null)
                throw new ArgumentNullException($"{nameof(options.AppearanceExtractorVersion)} was undefined.");

            const int DefaultExtractorsCount = 4;

            IAppearanceExtractor appearanceExtractor = options.AppearanceExtractorVersion switch
            {
                AppearanceExtractorVersion.OSNet => new ReidScorer<OSNet_x1_0>(File.ReadAllBytes(options.AppearanceExtractorFilePath),
                    options.ExtractorsInMemoryCount ?? DefaultExtractorsCount, SessionOptions.MakeSessionOptionWithCudaProvider()),
                AppearanceExtractorVersion.FastReid => new ReidScorer<Fast_Reid_mobilenetv2>(File.ReadAllBytes(options.AppearanceExtractorFilePath),
                    options.ExtractorsInMemoryCount ?? DefaultExtractorsCount, SessionOptions.MakeSessionOptionWithCudaProvider()),
                _ => throw new Exception("Appearance extractor cannot be constructed.")
            };

            return appearanceExtractor;
        }

        public static MatcherOption ReadMatcherOptionFromJson(string json)
        {
            try
            {
                var option = JsonConvert.DeserializeObject<MatcherOption>(File.ReadAllText(json));
                if (option == null)
                {
                    option = new MatcherOption();
                }
                return option;
            }
            catch (Exception ex)
            {
                return new MatcherOption();
            }
        }

        public void Dispose()
        {
            matcher.Dispose();
        }

    }
}
