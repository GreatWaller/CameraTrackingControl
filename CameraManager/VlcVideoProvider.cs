using LibVLCSharp.Shared;
using RTSP.RawFramesDecoding.DecodedFrames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CameraManager
{
    internal class VlcVideoProvider : IDisposable
    {
        private LibVLC libvlc;
        /// <summary>
        /// The media player instance. You must call <see cref="CreatePlayer"/> before using this.
        /// </summary>
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// RGBA is used, so 4 byte per pixel, or 32 bits.
        /// </summary>
        private const uint BytePerPixel = 4;

        /// <summary>
        /// the number of bytes per "line"
        /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
        /// </summary>
        private uint Pitch;

        /// <summary>
        /// The number of lines in the buffer.
        /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
        /// </summary>
        private uint Lines;

        public string Uri { get; private set; } = "demo uri";
        public uint Width { get; private set; }
        public uint Height { get; private set; }


        private MemoryMappedFile CurrentMappedFile;
        private MemoryMappedViewAccessor CurrentMappedViewAccessor;
        private ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)> FilesToProcess = new ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)>();
        private long FrameCounter = 0;

        public event EventHandler<Bitmap> FrameReceived;

        private bool isRunning = false;

        public VlcVideoProvider(string uri, uint width, uint height)
        {
            // this will load the native libvlc library (if needed, depending on the platform).
            Core.Initialize();
            CreatePlayer(uri, width, height);
        }


        /// <summary>
        /// Creates the player. This method must be called before using <see cref="MediaPlayer"/>
        /// </summary>
        /// <param name="vlcLibDirectory">The directory where to find the vlc library</param>
        /// <param name="vlcMediaPlayerOptions">The initialization options to be given to libvlc</param>
        private void CreatePlayer(string uri, uint width, uint height)
        {
            Uri = uri;
            Width = width;
            Height = height;

            // Create a new LibVLC instance
            libvlc = new LibVLC();

            // Create a new media player
            mediaPlayer = new MediaPlayer(libvlc);
            //mediaPlayer.SetVideoCallbacks(LockVideo, null, null);


            // Set the media player's media to the RTSP stream URL
            //mediaPlayer.Media = new Media(libvlc, new Uri("rtsp://admin:CS%40202304@192.168.1.151:554/Streaming/Channels/101?transportmode=unicast&profile=Profile_1"));
            mediaPlayer.Media = new Media(libvlc, new Uri(uri));

            mediaPlayer.Media.AddOption(":network-caching=333");
            mediaPlayer.Media.AddOption(":clock-jitter=0");
            mediaPlayer.Media.AddOption(":clock-syncro=0");
            mediaPlayer.Media.AddOption(":no-audio");


            Pitch = Align(width * BytePerPixel);
            Lines = Align(height);

            // Set the size and format of the video here.
            mediaPlayer.SetVideoFormat("RV32", width, height, Pitch);
            mediaPlayer.SetVideoCallbacks(LockCallback, null, DisplayCallback);


            uint Align(uint size)
            {
                if (size % 32 == 0)
                {
                    return size;
                }

                return ((size / 32) + 1) * 32;// Align on the next multiple of 32
            }

        }

        public async Task Start()
        {
            mediaPlayer.Play();
            isRunning = true;
            await ProcessAsync().ConfigureAwait(false);
        }

        public void Stop()
        {
            isRunning = false;
            mediaPlayer.Stop();
        }

        private async Task ProcessAsync()
        {
            var frameNumber = 0;
            await Task.Factory.StartNew(async () =>
            {
                while (isRunning)
                {
                    if (!mediaPlayer.IsPlaying)
                    {
                        mediaPlayer.Stop();
                        var res = mediaPlayer.Play();
                        Trace.TraceInformation($"Connecting...");
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                    if (FilesToProcess.TryDequeue(out var file))
                    {
                        using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Bgra32>((int)(Pitch / BytePerPixel), (int)Lines))
                        using (var sourceStream = file.file.CreateViewStream())
                        {
                            var mg = image.GetPixelMemoryGroup();

                            for (int i = 0; i < mg.Count; i++)
                            {
                                sourceStream.Read(MemoryMarshal.AsBytes(mg[i].Span));
                            }

                            image.Mutate(ctx => ctx.Crop((int)Width, (int)Height));
                            var memoryStream = new MemoryStream();
                            image.SaveAsBmp(memoryStream);
                            var bitmap = new Bitmap(memoryStream);
                            FrameReceived?.Invoke(this, bitmap);

                        }
                        file.accessor.Dispose();
                        file.file.Dispose();
                        frameNumber++;
                    }
                    else
                    {
                    }
                }
            });
        }

        private IntPtr LockCallback(IntPtr opaque, IntPtr planes)
        {
            CurrentMappedFile = MemoryMappedFile.CreateNew(null, Pitch * Lines);
            CurrentMappedViewAccessor = CurrentMappedFile.CreateViewAccessor();
            Marshal.WriteIntPtr(planes, CurrentMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle());
            return IntPtr.Zero;
        }

        private void DisplayCallback(IntPtr opaque, IntPtr picture)
        {
            if (FrameCounter++ % 4 ==0)
            {
                FilesToProcess.Enqueue((CurrentMappedFile, CurrentMappedViewAccessor));
                CurrentMappedFile = null;
                CurrentMappedViewAccessor = null;
            }
        }



        public void Dispose()
        {
            mediaPlayer.Dispose();
            libvlc.Dispose();
        }
    }
}
