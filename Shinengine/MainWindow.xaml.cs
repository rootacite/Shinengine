using SharpDX.Mathematics.Interop;
using System;
using System.Windows;
using System.Windows.Input;

using System.Runtime.InteropServices;


using SharpDX;
using System.Windows.Controls;
using SharpDX.Direct2D1;
using System.Windows.Media.Imaging;
using ImageBrush = System.Windows.Media.ImageBrush;
using System.Threading.Tasks;
using System.Windows.Interop;

using PInvoke;
using FFmpeg.AutoGen;
using System.Threading;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using SharpDX.WIC;
using System.Windows.Media.Animation;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool disFlag = false;
        VideoPlayer gameview = null;
        [DllImport("Shinehelper.dll")]
        unsafe extern static public byte* getPCM();
        [DllImport("Shinehelper.dll")]
        extern public static bool waveInit(IntPtr hWnd, int channels, int sample_rate, int bits_per_sample, int size);
        [DllImport("Shinehelper.dll")]
        unsafe extern public static void waveWrite(byte* in_buf, int in_buf_len);
        [DllImport("Shinehelper.dll")]
        extern public static void waveClose();
        IntPtr hWnd =(IntPtr) 0;
        Direct2DImage DxBkGround = null;
        private Task m_reseter;
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);

        unsafe public bool DrawCallback(WicRenderTarget view, object Loadedsouce, int Width, int Height)
        {
            var video = Loadedsouce as VideoStreamDecoder;
            if (disFlag)
                return false;
            if (video == null)
                return false;

            IntPtr dataPoint;
            int pitch;
            var res = video.TryDecodeNextFrame(out dataPoint, out pitch);
            if (!res) {
                new Thread(()=> { disFlag = true; DxBkGround.Dispose(); DxBkGround = null; }).Start();
                return false;
            }
            var ImGc = new ImagingFactory();
            var WICBIT = new WICBitmap(ImGc, video.FrameSize.Width, video.FrameSize.Height,SharpDX.WIC.PixelFormat.Format32bppBGR,new DataRectangle(dataPoint, pitch));
            var BitSrc = D2DBitmap.FromWicBitmap(view,WICBIT);

            view.BeginDraw();
            view.DrawBitmap(BitSrc,
               new RawRectangleF(0, 0, Width, Height),
               1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
               new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));
            view.EndDraw();

            ImGc.Dispose();
            WICBIT.Dispose();
            BitSrc.Dispose();
            return true;
        }

        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
           
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            timeBeginPeriod(1);

            this.Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/UI/loading.png")));
          
            return;
        }


        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
            gameview = new VideoPlayer();
            gameview.Start(hWnd, "assets\\09.mp4", ()=>{
                var c = gameview.Content;
                gameview.Content = this.Content;
                this.Content = c;
            });
            var c = this.Content;
            this.Content= gameview.Content;
            gameview.Content = c;

            return;
            Storyboard stb = new Storyboard();
            DoubleAnimation dmb = new DoubleAnimation();
            dmb.From = 1000;
            dmb.To = 100;
            dmb.Duration = TimeSpan.FromSeconds(1);
            stb.Children.Add(dmb);
            stb.FillBehavior = FillBehavior.HoldEnd;
            Storyboard.SetTarget(stb, this);
            Storyboard.SetTargetProperty(stb, new PropertyPath("(Width)"));

            stb.Begin();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
            if (m_reseter != null)
                m_reseter.Dispose();
            if (DxBkGround != null)
                DxBkGround.Dispose();
            if (gameview != null)
            {
                gameview.Close();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // _Position = e.GetPosition(this);
         //   Canvas.SetZIndex(test, Canvas.GetZIndex(test) - 1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hWnd = new WindowInteropHelper(this).Handle;
            DxBkGround = new Direct2DImage(new Size2((int)BackGround.Width, (int)BackGround.Height), 30)
            {
                Loadedsouce = new VideoStreamDecoder("assets\\title.wmv")
            };

            DxBkGround.Disposed += (Loadedsouce) => { (Loadedsouce as VideoStreamDecoder).Dispose(); };
            DxBkGround.DrawProc += DrawCallback;


            DxBkGround.DrawStartup(BackGround);
        }
    }
}
