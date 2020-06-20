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
using System.Windows.Media;
using System.Media;

using Shinengine.Media;
using Shinengine.Data;

namespace Shinengine.Surface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            if (!res) {
                DxBkGround = null;
                return DrawProcResult.Death;
            }
            var ImGc = new ImagingFactory();
            var WICBIT = new WICBitmap(ImGc, video.FrameSize.Width, video.FrameSize.Height,SharpDX.WIC.PixelFormat.Format32bppPBGRA,new DataRectangle(dataPoint, pitch));
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
            return DrawProcResult.Commit;
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

        [Obsolete]
        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {


            
            return;
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
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
        public MediaPlayer m_BGkMusic = new MediaPlayer();
        private SoundPlayer player;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            player = new SoundPlayer();
            player.SoundLocation = "assets\\bgm04s.wav";
            player.Load();
           
            hWnd = new WindowInteropHelper(this).Handle;
            DxBkGround = new Direct2DImage(new Size2((int)BackGround.Width, (int)BackGround.Height), 30)
            {
                Loadedsouce = new VideoStreamDecoder("assets\\title.wmv")
            };

            DxBkGround.Disposed += (Loadedsouce) => { (Loadedsouce as VideoStreamDecoder).Dispose(); };
            DxBkGround.DrawProc += DrawCallback;

            DxBkGround.DrawStartup(BackGround);
            player.Play();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            player.Stop();
            player.Dispose();


            VideoPlayer vpm = new VideoPlayer();
            vpm.Start(hWnd,"assets\\VIDEO\\video_01.mp4",()=>
            {
                vpm.Close();
                GamingTheatre m_game = new GamingTheatre();
                this.Content = m_game.Content;
                m_game.Start((s) =>
                {
                    s.setBackground(Color.FromRgb(0, 0, 0));
                    s.stage.setASImage("/assets/CG/02.png");
                    s.stage.Show(2, false);
                    s.usage.Show(2, false);
                    //    s.usage.Show(0.5, false);

                    s.waitForClick(this);
                    s.usage.Hide(1, false);
                    s.waitForClick(this);
                    s.usage.Show(1, false);
                    return 0;
                });
                m_game.Close();
            });

            this.Content = vpm.Content;
        }
    }
}
