using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Shinengine
{
    /// <summary>
    /// GameView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayer : Window
    {
        int i = 0;

        double video_time = 0;
        double audio_time = 0;
        [DllImport("Shinehelper.dll")]
        unsafe extern static public byte* getPCM([MarshalAs(UnmanagedType.LPWStr)] string path);
        [DllImport("Shinehelper.dll")]
        extern public static bool waveInit(IntPtr hWnd, int channels, int sample_rate, int bits_per_sample, int size);
        [DllImport("Shinehelper.dll")]
        unsafe extern public static void waveWrite(byte* in_buf, int in_buf_len);
        [DllImport("Shinehelper.dll")]
        extern public static void waveClose();
        Direct2DImage dxVideo = null;
        public VideoPlayer()
        {
            InitializeComponent();

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private bool DrawCallback(WicRenderTarget view, object Loadedsouce, int Width, int Height)
        {
            SharpDX.Direct2D1.Bitmap farme = null;
            if (Loadedsouce == null)
                return false;
            Video video = Loadedsouce as Video;

            if (video.nFarm == video.bits.Count)
                return false;

            if (video.bits[video.nFarm]?.frame.IsDisposed == true)
                return false;
            video_time = (double)(video.bits[video.nFarm]?.time_base);
            if(audio_time-video_time > 0.1)
            {
                video.bits[video.nFarm]?.frame.Dispose();

                video.nFarm++;
                return false;
            }
            if (audio_time - video_time < -0.1)
            {
                return true;
            }
            // Console.WriteLine(video.nFarm.ToString() + "：Using");
            farme = SharpDX.Direct2D1.Bitmap.FromWicBitmap(view, video.bits[video.nFarm]?.frame);
            
            view.BeginDraw();
            view.DrawBitmap(farme,
                new RawRectangleF(0, 0, Width, Height),
                1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                new RawRectangleF(0, 0, (float)video.bits[video.nFarm]?.frame.Size.Width, (float)video.bits[video.nFarm]?.frame.Size.Height));
            view.EndDraw();

            farme.Dispose();
         //   Console.WriteLine(video.nFarm.ToString() + "：Disposing");
            video.bits[video.nFarm]?.frame.Dispose();

            video.nFarm++;
            return true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (dxVideo != null)
                dxVideo.Dispose();
        }

        public void Stop()
        {
            dxVideo.Dispose();
        }

        public void Start(IntPtr hWnd, string path, Action endplay)
        {
            var vest = new Video(Video.VideoMode.LoadWithPlaying, path);
            dxVideo = new Direct2DImage(new SharpDX.Size2((int)BackGround.Width, (int)BackGround.Height), vest.Fps)
            {
                Loadedsouce = vest
            };
            dxVideo.Disposed += (Loadedsouce) => { (Loadedsouce as Video).Dispose(); };
            dxVideo.DrawProc += DrawCallback;


            var video = dxVideo.Loadedsouce as Video;
            new Task(() =>
            {

                while (!video.CanRun)
                    Thread.Sleep(1);
                unsafe
                {
                        //       var intp = getPCM("assets\\09.pcm");
                        waveInit(hWnd, video.out_channels, video.out_sample_rate, video.bit_per_sample, video.out_buffer_size);

                   
                    while (true)
                    {
                        
                        if (!video.CanRun)
                            break;
                        if (video.entiryPlayed && i == video.abits.Count && video.nFarm == video.bits.Count) 
                            break;
                        if (i == video.abits.Count)
                        {
                            Thread.Sleep(1);
                            continue;
                        }
                        audio_time = (double)(video.abits[i]?.time_base);

                        waveWrite((byte*)video.abits[i]?.data, video.out_buffer_size);
                        Marshal.FreeHGlobal((IntPtr)video.abits[i]?.data);
                        video.abits[i] = null;
                        i++;
                    }

                    waveClose();
                    this.Dispatcher.Invoke(endplay);
                }

            }).Start();


            video.Start();
            dxVideo.DrawStartup(BackGround);
        }
    }
}
