using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using Shinengine.Data;
using Shinengine.Media;
using Shinengine.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
using Image = System.Windows.Controls.Image;
using WICBitmap = SharpDX.WIC.Bitmap;
using Shinengine.Theatre;

namespace Shinengine.Surface
{
    /// <summary>
    /// GamingTheatre.xaml 的交互逻辑
    /// </summary>
    public partial class GamingTheatre : Page, IDisposable
    {

        public static bool isSkiping = false;
        public static bool AutoMode = false;

        bool onHidden = false;
        AudioPlayer m_replayer = null;
        Window _main_window = null;
        public Shinengine.Theatre.Theatre m_theatre = null;//unmanged
        private Direct2DImage m_logo = null;//unmanged

        ~GamingTheatre()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            m_theatre.Dispose();
            m_logo.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
        public bool IsDisposed { get; private set; } = false;
        static private readonly List<WICBitmap> logo_frames = new List<WICBitmap>();

        int ims = 0;
        static GamingTheatre()
        {
            if (Directory.Exists("Temp"))
                Directory.Delete("Temp", true);
            VideoStreamDecoder vsd = new VideoStreamDecoder(@"assets.shine:movie/LOGO_32.mov");

            while (true)
            {
                var res = vsd.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                if (!res)
                    break;
                var ImGc = new ImagingFactory();
                var WICBIT = new WICBitmap(ImGc, vsd.FrameSize.Width, vsd.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));
                //  var mp = new System.Drawing.Bitmap(vsd.FrameSize.Width, vsd.FrameSize.Height, pitch, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, dataPoint);
                //   mp.Save("test/" + logo_frames.Count + ".png");
                //   mp.Dispose();
                ImGc.Dispose();
                logo_frames.Add(WICBIT);
            }
            vsd.Dispose();
        }
        public GamingTheatre()
        {
            InitializeComponent();
            Preparation = "";
            isSkiping = false;
            AutoMode = false;


        }
        public void Init(Window main_window)
        {
            _main_window = main_window;
            m_theatre = new Shinengine.Theatre.Theatre(BG, Usage, SBK, AirPt, character_usage, Lines, character, ShowIn, _Contents);

            #region logo
            m_logo = new Direct2DImage(new SharpDX.Size2((int)logo_frames[0].Size.Width, (int)logo_frames[0].Size.Height), 30)
            {
                Loadedsouce = logo_frames
            };
            m_logo.DrawProc += (t, s, w, h) =>
            {
                var frames = s as List<WICBitmap>;

                if (ims == frames.Count)
                    ims = 0;
                t.View.BeginDraw();
                t.View.Clear(null);
                D2DBitmap parl_map = D2DBitmap.FromWicBitmap(t.View, frames[ims]);
                t.View.DrawBitmap(parl_map, 1, InterpolationMode.Anisotropic);
                t.View.EndDraw();

                ims++;
                parl_map.Dispose();
                return DrawProcResult.Normal;
            };
            m_logo.DrawStartup(Logo);


            #endregion

        }
        public delegate int ScriptHandle(Shinengine.Theatre.Theatre theatre, GamingTheatre self);
        Task scriptTask = null;
        public int ResultOfTheatre = 0;
        public void Start(ScriptHandle scriptHandle, int chapter, Action endEvent = null)
        {
            scriptTask = new Task(() =>
            {
                int result = scriptHandle(m_theatre, this);
                if (result != -1)
                {
                    ResultOfTheatre = result;
                    this.Dispatcher.Invoke(endEvent);
                }

            });
            scriptTask.Start();
            ReturnLast.Click += (e, v) =>
            {
                MainWindow.TheatreMode.Dispose();

                MainWindow.TheatreMode = MainWindow.SwitchToSignalTheatre(chapter, 0, null);
            };
        }


        private void SBK_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SBK_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            isSkiping = false;
            AutoMode = false;
            if (isBakcloging)
            {
                text_count_picker = 0;
                Shinengine.Theatre.Usage.locked = false;
                m_theatre.Usage.Show(null, true);
                EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 1.0, 0.0, Data.SharedSetting.SwitchSpeed);
                m_pip.Start(true);
                isBakcloging = false;
                Menu.IsEnabled = true;
                e.Handled = true;
                return;
            }
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
            if (!onHidden && m_theatre.canCtrl)
            {
                m_theatre.Usage.Hide(null, true);
                onHidden = true;
                return;
            }
            else
            {
                m_theatre.Usage.Show(null, true);
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
            if (onHidden && m_theatre.canCtrl)
            {
                m_theatre.Usage.Show(null, true);
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
            Source = new BitmapImage(new Uri(@"pack://application:,,,/UI/AutoMini.png"))
        };

        public Image skip_icon = new Image()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = 45,
            Height = 45,
            Margin = new Thickness(68, 18, 0, 0),
            Source = new BitmapImage(new Uri(@"pack://application:,,,/UI/SkipMini.png"))
        };

        public bool isBakcloging = false;
        long text_count_picker = 0;
        private void SBK_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Preparation.Length == 0)
                return;
            List<string> lines = Preparation[0..^1].Split('\n').ToList();
            if (e.Delta > 0)
            {
                if (text_count_picker == 0)
                {
                    m_theatre.Usage.Hide(null, true);
                    Shinengine.Theatre.Usage.locked = true;
                    EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 0.0, 1.0, Data.SharedSetting.SwitchSpeed);
                    m_pip.Start(true);

                    text_count_picker -= 1;
                    CommitText();
                    restIlt.Value = 0;
                    isBakcloging = true;
                    Menu.IsEnabled = false;

                    e.Handled = true;
                    return;
                }
                if ((-text_count_picker - 1) < lines.Count - 27)
                    text_count_picker -= 1;
            }
            else
            {
                if (text_count_picker < 0) text_count_picker += 1;
                else return;
                if (text_count_picker == 0)
                {

                    Shinengine.Theatre.Usage.locked = false;
                    m_theatre.Usage.Show(null, true);
                    EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 1.0, 0.0, Data.SharedSetting.SwitchSpeed);
                    m_pip.Start(true);
                    isBakcloging = false;
                    Menu.IsEnabled = true;

                    e.Handled = true;
                    return;
                }
            }
            restIlt.Value = (-text_count_picker - 1);

        }
        private void RestIlt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CommitText((int)(e.NewValue + 27));
        }
        public static string Preparation = "";  //显示最后18行
        private void CommitText(int start_line = 27)
        {
            if (Preparation.Length == 0)
                return;
            List<string> lines = Preparation[0..^1].Split('\n').ToList();
            restIlt.Maximum = lines.Count - 27 < 0 ? 0 : lines.Count - 27;
            if (lines.Count <= start_line)
            {
                _Contents.Text = string.Join("\n", lines);
                return;
            }
            List<string> _lines = new List<string>();
            for (int i = lines.Count - start_line; i - (lines.Count - start_line) < 27; i++)
            {
                if (i >= lines.Count)
                    break;
                _lines.Add(lines[i]);
            }

            _Contents.Text = string.Join("\n", _lines);
            return;
        }

        private void EnLog_Click(object sender, RoutedEventArgs e)
        {
            if (Preparation.Length == 0)
                return;
            List<string> lines = Preparation[0..^1].Split('\n').ToList();

            if (text_count_picker == 0)
            {
                m_theatre.Usage.Hide(null, true);
                Shinengine.Theatre.Usage.locked = true;
                EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 0.0, 1.0, Data.SharedSetting.SwitchSpeed);
                m_pip.Start(true);

                text_count_picker -= 1;
                CommitText();
                restIlt.Value = 0;
                isBakcloging = true;
                Menu.IsEnabled = false;

                e.Handled = true;
                return;
            }
        }

        private void Exitlpg_Click(object sender, RoutedEventArgs e)
        {
            if (isBakcloging)
            {
                text_count_picker = 0;
                Shinengine.Theatre.Usage.locked = false;
                m_theatre.Usage.Show(null, true);
                EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 1.0, 0.0, Data.SharedSetting.SwitchSpeed);
                m_pip.Start(true);
                isBakcloging = false;
                Menu.IsEnabled = true;
                e.Handled = true;
                return;
            }
        }

        private void Ll_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Ll_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void EnVoice_Click(object sender, RoutedEventArgs e)
        {
            if (m_replayer != null)
                m_replayer.canplay = false;
            m_replayer = new AudioPlayer(m_theatre.Voice, false, SharedSetting.VoiceVolum);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isSkiping = false;
            AutoMode = false;
            if (isBakcloging)
            {
                text_count_picker = 0;
                Shinengine.Theatre.Usage.locked = false;
                m_theatre.Usage.Show(null, true);
                EasyAmal m_pip = new EasyAmal(BackLogLayer, "(Opacity)", 1.0, 0.0, Data.SharedSetting.SwitchSpeed);
                m_pip.Start(true);
                isBakcloging = false;
                Menu.IsEnabled = true;
                e.Handled = true;
                return;
            }
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
                m_theatre.Usage.Hide(null, true);
                onHidden = true;
                return;
            }
            else
            {
                m_theatre.Usage.Show(null, true);
                onHidden = false;
                return;
            }
        }

        public int current_frame 
        {
            get
            {
                return m_theatre.saved_frame;
            }
        }
    }
}
