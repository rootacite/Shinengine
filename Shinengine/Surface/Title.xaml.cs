using SharpDX.Mathematics.Interop;
using System;
using System.Windows;
using System.Windows.Input;

using System.Runtime.InteropServices;


using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Media.Imaging;
using ImageBrush = System.Windows.Media.ImageBrush;
using System.Windows.Interop;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using SharpDX.WIC;
using System.Windows.Media;
using System.Media;

using Shinengine.Media;
using Color = System.Windows.Media.Color;

using NAudio.Wave;
using System.Threading;
using System.Drawing;

namespace Shinengine.Surface
{
    
    /// <summary>
    /// Title.xaml 的交互逻辑
    /// </summary>
    public partial class Title : Window
    {
        bool nCanrun = true;


        [DllImport("Shinehelper.dll")]
        unsafe extern static public byte* getPCM();
        [DllImport("Shinehelper.dll")]
        extern public static bool waveInit(IntPtr hWnd, int channels, int sample_rate, int bits_per_sample, int size);
        [DllImport("Shinehelper.dll")]
        unsafe extern public static void waveWrite(byte* in_buf, int in_buf_len);
        [DllImport("Shinehelper.dll")]
        extern public static void waveClose();
        IntPtr hWnd = (IntPtr)0;
        Direct2DImage DxBkGround = null;
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);

        unsafe public DrawProcResult DrawCallback(WicRenderTarget view, object Loadedsouce, int Width, int Height)
        {
            var video = Loadedsouce as VideoStreamDecoder;
            if (video == null)
                return DrawProcResult.Ignore;

            IntPtr dataPoint;
            int pitch;
            var res = video.TryDecodeNextFrame(out dataPoint, out pitch);
            if (!res)
            {
                DxBkGround = null;
                return DrawProcResult.Death;
            }
            var ImGc = new ImagingFactory();
            var WICBIT = new WICBitmap(ImGc, video.FrameSize.Width, video.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));
            var BitSrc = D2DBitmap.FromWicBitmap(view, WICBIT);

            view.BeginDraw();
            view.DrawBitmap(BitSrc,
              new RawRectangleF(0, 0, Width, Height),
               1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
               new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));


            view.EndDraw();

            ImGc.Dispose();
            WICBIT.Dispose();

            BitSrc.Dispose();
            if (!nCanrun) return DrawProcResult.Death;
            return DrawProcResult.Commit;
        }
        public Title()
        {
            InitializeComponent();
            timeBeginPeriod(1);

            this.Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/UI/loading.png")));

            m_BGkMusic = new AudioPlayer("assets\\BGM\\10.wav", true);


            hWnd = new WindowInteropHelper(this).Handle;
            
            DxBkGround = new Direct2DImage(new Size2((int)BackGround.Width, (int)BackGround.Height), 30)
            {
                Loadedsouce = new VideoStreamDecoder("assets\\title.wmv")
            };

            DxBkGround.Disposed += (Loadedsouce, s) => { (Loadedsouce as VideoStreamDecoder).Dispose(); s.Dispose(); };
            DxBkGround.DrawProc += DrawCallback;

            DxBkGround.DrawStartup(BackGround);

            BkGrid.Unloaded += (e, v) =>
            {
                m_BGkMusic.canplay = false;
                nCanrun = false;
            };
            
        }

        [Obsolete]
        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {



            return;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // _Position = e.GetPosition(this);
            //   Canvas.SetZIndex(test, Canvas.GetZIndex(test) - 1);
        }
        public AudioPlayer m_BGkMusic = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

         

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

           

        }
    } 
}
