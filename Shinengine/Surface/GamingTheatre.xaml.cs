using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using Shinengine.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using D2DBitmap = SharpDX.Direct2D1.Bitmap;

using WICBitmap = SharpDX.WIC.Bitmap;

namespace Shinengine.Surface
{
    /// <summary>
    /// GamingTheatre.xaml 的交互逻辑
    /// </summary>
    public partial class GamingTheatre : Window
    {
        public static bool isSkiping = false;
        public static bool AutoMode = false;

        bool onHidden = false;

        Window _main_window = null;
        public Theatre m_theatre = null;
        public Direct2DImage m_logo = null;

        private List<WICBitmap> logo_frames = new List<WICBitmap>();
        int ims = 0;
        public GamingTheatre(Window main_window)
        {
            InitializeComponent();
          
            _main_window = main_window;
            m_theatre = new Theatre(BG, Usage, SBK, AirPt, character_usage, Lines, character, ShowIn);

            VideoStreamDecoder vsd = new VideoStreamDecoder("UI\\LOGO_32.mov");
            IntPtr dataPoint;
            int pitch;

            while (true)
            {
                var res = vsd.TryDecodeNextFrame(out dataPoint, out pitch);
                if (!res)
                    break;
                var ImGc = new ImagingFactory();
                var WICBIT = new WICBitmap(ImGc, vsd.FrameSize.Width, vsd.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));

                logo_frames.Add(WICBIT);
            }
            vsd.Dispose();


            m_logo = new Direct2DImage(new SharpDX.Size2((int)Logo.Width, (int)Logo.Height), 30)
            {
                Loadedsouce = logo_frames
            };
   
            m_logo.DrawProc += (t, s, w, h) =>
            {
                var frames = s as List<WICBitmap>;

                if (ims == frames.Count)
                    ims = 0;
                t.BeginDraw();
                t.Clear(new RawColor4(0, 0, 0, 0));
                D2DBitmap parl_map = D2DBitmap.FromWicBitmap(t, frames[ims]); 
                t.DrawBitmap(parl_map,
             new RawRectangleF(0, 0, w, h),
              1, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
              new RawRectangleF(0, 0, frames[ims].Size.Width, frames[ims].Size.Height));
                t.EndDraw();

                ims++;
                parl_map.Dispose();
                return DrawProcResult.Commit;
            };
            m_logo.Disposed += (s,ss) =>
            {
                var frames = s as List<WICBitmap>;
                foreach (var f in frames)
                {
                    f.Dispose();
                }
                ss.Dispose();
           };
           m_logo.DrawStartup(Logo);

        }
        public delegate int ScriptHandle(Theatre theatre);
        Task scriptTask = null;

        public AudioPlayer m_player = null;

        public void Start(ScriptHandle scriptHandle)
        {
            scriptTask = new Task(()=> { scriptHandle(m_theatre); });
            scriptTask.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
     
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SBK_MouseUp(object sender, MouseButtonEventArgs e)
        {
        
        }

        private void SBK_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            isSkiping = false;
            AutoMode = false;
            if (!onHidden)
            {
                m_theatre.usage.Hide(null, true);
                onHidden = true;
                return;
            }
            else
            {
                m_theatre.usage.Show(null, true);
                onHidden = false;
                return;
            }
        }

        private void SBK_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isSkiping = false;
            AutoMode = false;
            if (onHidden)
            {
                m_theatre.usage.Show(null, true);
                onHidden = false;
                return;
            }
        }
        private void PopupBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)///Skip
        {
            isSkiping = !isSkiping;
            if (isSkiping)
            {
                if (this.m_theatre.call_next != null) this.m_theatre.call_next.Set();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)///Auto
        {

            AutoMode = !AutoMode;
            if (AutoMode)
            {
                if (this.m_theatre.call_next != null) this.m_theatre.call_next.Set();
            }
        }
    }
}
