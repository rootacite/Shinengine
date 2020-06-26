using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using Shinengine.Media;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class GamingTheatre : Page
    {

        public static bool isSkiping = false;
        public static bool AutoMode = false;

        bool onHidden = false;

        Window _main_window = null;
        public Theatre m_theatre = null;
        public Direct2DImage m_logo = null;

        private List<WICBitmap> logo_frames = new List<WICBitmap>();

        int ims = 0;
        public GamingTheatre()
        {
            InitializeComponent();
            Preparation = "";
        }
        public void Init(Window main_window)
        {
            _main_window = main_window;
            m_theatre = new Theatre(BG, Usage, SBK, AirPt, character_usage, Lines, character, ShowIn,_Contents);
            #region logo
            VideoStreamDecoder vsd = new VideoStreamDecoder("assets\\movie\\LOGO_32.mov");
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
            m_logo.Disposed += (s, ss) =>
            {
                var frames = s as List<WICBitmap>;
                foreach (var f in frames)
                {
                    f.Dispose();
                }
                ss.Dispose();
            };
            m_logo.DrawStartup(Logo);
            #endregion
        }
        public delegate int ScriptHandle(Theatre theatre);
        Task scriptTask = null;

        public void Start(ScriptHandle scriptHandle)
        {
            scriptTask = new Task(() => { scriptHandle(m_theatre); });
            scriptTask.Start();
        }


        private void SBK_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SBK_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            isSkiping = false;
            AutoMode = false;
            if (isSkiping)
            {
                if (!this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Add(skip_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Remove(skip_icon);
                }
            }
            if (AutoMode)
            {
                if (!this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Add(auto_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Remove(auto_icon);
                }
            }
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
            if (isSkiping)
            {
                if (!this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Add(skip_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Remove(skip_icon);
                }
            }
            if (AutoMode)
            {
                if (!this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Add(auto_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Remove(auto_icon);
                }
            }
            if (onHidden)
            {
                m_theatre.usage.Show(null, true);
                onHidden = false;
                return;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)///Skip
        {
            isSkiping = !isSkiping;
            if (isSkiping)
            {
                if (!this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Add(skip_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(skip_icon))
                {
                    this.ShowIn.Children.Remove(skip_icon);
                }
            }
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
                if (!this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Add(auto_icon);
                }
            }
            else
            {
                if (this.ShowIn.Children.Contains(auto_icon))
                {
                    this.ShowIn.Children.Remove(auto_icon);
                }
            }
            if (AutoMode)
            {
                if (this.m_theatre.call_next != null) this.m_theatre.call_next.Set();
            }
        }

        public Image auto_icon = new Image()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 45,
            Height = 45,
            Margin = new Thickness(18, 18, 0, 0),
            Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/assets/local/AutoMini.png"))
        };

        public Image skip_icon = new Image()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 45,
            Height = 45,
            Margin = new Thickness(68, 18, 0, 0),
            Source = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/assets/local/SkipMini.png"))
        };

        long text_count_picker = 0;
        private void SBK_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Preparation.Length == 0)
                return;
            List<string> lines = Preparation.Substring(0, Preparation.Length - 1).Split('\n').ToList();
            if (e.Delta > 0)
            {
                if (text_count_picker == 0)
                {
                    m_theatre.usage.Hide(null, true);
                    Surface.Usage.locked = true;
                    EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 0.0, 1.0, Data.SharedSetting.switchSpeed);
                    m_pip.Start(true);

                    text_count_picker -= 1;
                    CommitText();
                    return;
                }
                if(-text_count_picker < lines.Count-18)
                text_count_picker -= 1;
            }
            else
            {
                if (text_count_picker < 0) text_count_picker += 1;
                else return;
                if (text_count_picker == 0)
                {

                    Surface.Usage.locked = false;
                    m_theatre.usage.Show(null, true);
                    EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 1.0, 0.0, Data.SharedSetting.switchSpeed);
                    m_pip.Start(true);

                    
                    return;
                }
            }

            restIlt.Value = -text_count_picker;
        }
        private void restIlt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CommitText((int)(restIlt.Value + 18));
        }
        public static string Preparation = "";  //显示最后18行
        private void CommitText(int start_line = 18)
        {
            List<string> lines = Preparation.Substring(0, Preparation.Length - 1).Split('\n').ToList();
            restIlt.Maximum = lines.Count - 18 < 0 ? 0 : lines.Count - 18;
            if (lines.Count <= start_line)
            {
                _Contents.Text = string.Join("\n", lines);
                return;
            }
            List<string> _lines = new List<string>();
            for (int i = lines.Count - start_line; i - (lines.Count - start_line) < 18; i++)
            {
                if (i >= lines.Count)
                    break;
                _lines.Add(lines[i]);
            }

            _Contents.Text = string.Join("\n", _lines);
            return;
        }
    }
}
