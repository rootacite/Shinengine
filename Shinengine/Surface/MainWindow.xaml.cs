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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 初始化成员
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
        #endregion
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
        public AudioPlayer m_BGkMusic = null;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            m_BGkMusic = new AudioPlayer("assets\\BGM\\10.wav", true);


            hWnd = new WindowInteropHelper(this).Handle;
            DxBkGround = new Direct2DImage(new Size2((int)BackGround.Width, (int)BackGround.Height), 30)
            {
                Loadedsouce = new VideoStreamDecoder("assets\\title.wmv")
            };

            DxBkGround.Disposed += (Loadedsouce,s) => { (Loadedsouce as VideoStreamDecoder).Dispose(); s.Dispose(); };
            DxBkGround.DrawProc += DrawCallback;

            DxBkGround.DrawStartup(BackGround);
            
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
           
            StaticCharacter.ChangeableAreaInfo[] m_Infos = new StaticCharacter.ChangeableAreaInfo[1];
            m_Infos[0].area = new Rect(0, 0, 1500, 1500);
            m_Infos[0].pics = new string[3];
            m_Infos[0].pics[0] = "assets\\Character\\BS_RE2x_face___000.png";
            m_Infos[0].pics[1] = "assets\\Character\\BS_RE2x_face___001.png";
            m_Infos[0].pics[2] = "assets\\Character\\BS_RE2x_face___002.png";

            if (m_BGkMusic != null)
            {
                 m_BGkMusic.canplay = false;
            }

            this.Background = new System.Windows.Media.SolidColorBrush(Colors.White);
            EasyAmal mnoi = null;
             mnoi = new EasyAmal(BkGrid, "(Opacity)", 1.0, 0.0, 1.6,(e,v)=>
            {
                m_BGkMusic = new AudioPlayer("assets\\BGM\\01.wav", true, 0.45f);
                GamingTheatre m_game = new GamingTheatre(this);
                this.Content = m_game.Content;
                mnoi.stbd.Stop();
                m_game.Start((s) =>
                {
                    s.usage.Hide(0);
                    s.setBackground(Color.FromRgb(0, 0, 0));
                    s.stage.setAsImage("assets\\CG\\10.png",0,false);
                    s.stage.Show(null, true);
                    s.usage.Show();

                    var character_1 = new StaticCharacter("墨小菊", "assets\\Character\\BS_RE20_01B.png", s.CharacterLayer, false, m_Infos, null, false);
                    character_1.SwitchTo(0,1);
                    character_1.Show();
                    while (true)
                    {
                     //   s.stage.setAsImage("assets\\CG\\10.png", 0.3, false);
                        character_1.SwitchTo(0, 1, null, false);
                       // character_1.Say(s.airplant, "好久不见!", "assets\\sound\\01.wma");

                        s.waitForClick(this);

                       // s.stage.setAsVideo("assets\\title.wmv", 0.3, false);
                        character_1.SwitchTo(0, 2, null, false);
                      //  character_1.Say(s.airplant, "你什么时候去死啊");
                        s.waitForClick(this);

                        GC.Collect();
                    }
                    return 0;
                });
                m_game.Close();
            });
            mnoi.Start(true);
         

        }
    }
}
