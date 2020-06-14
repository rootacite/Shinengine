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

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Direct2DImage DxBkGround = null;

        [DllImport("Shinehelper.dll")]
        static extern public void Test1(bool yorn);
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);

        public bool DrawCallback(WicRenderTarget view, object Loadedsouce, int Width, int Height)
        {

            SharpDX.Direct2D1.Bitmap farme = null;
            if (Loadedsouce == null)
                return false;

            Video video = Loadedsouce as Video;

            if (video.nFarm == video.bits.Count)
                return false;

            if (video.bits[video.nFarm].IsDisposed)
                return false;
            Console.WriteLine(video.nFarm.ToString() + "：Using");
            farme = SharpDX.Direct2D1.Bitmap.FromWicBitmap(view, video.bits[video.nFarm]);


            view.BeginDraw();
            view.DrawBitmap(farme,
                new RawRectangleF(0, 0, Width, Height),
                1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                new RawRectangleF(0, 0, video.bits[video.nFarm].Size.Width, video.bits[video.nFarm].Size.Height));
            view.EndDraw();

            farme.Dispose();
            Console.WriteLine(video.nFarm.ToString() + "：Disposing");
         //   video.bits[video.nFarm].Dispose();

            video.nFarm++;
            if (video.bits.Count == video.nFarm)
            {
                new Task(() => { DxBkGround.Dispose(); }).Start();
            }
            return true;
        }

        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            timeBeginPeriod(1);

            Button m_btn = new Button()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(300, 0, 0, 0),
                Height = 31,
                Width = 103,
                Style = (Style)this.FindResource("BtnInfoStyle"),
                Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/UI/exit_0.png", false))),

            };

            m_btn.Click += (e, v) =>
            {
                GC.Collect();
            };
            
            BkGrid.Children.Add(m_btn);
            this.Background = new ImageBrush(new BitmapImage(new Uri("pack://siteoforigin:,,,/UI/loading.png")));

            return;
        }


        public void Reset()
        {
           

        }
        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
          
                //Loadedsouce.Dispose();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // _Position = e.GetPosition(this);
         //   Canvas.SetZIndex(test, Canvas.GetZIndex(test) - 1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DxBkGround = new Direct2DImage(new Size2((int)BackGround.Width, (int)BackGround.Height), 30)
            { Loadedsouce = new Video(Video.VideoMode.LoadonCatched) };

            DxBkGround.Disposed += (Loadedsouce) => { (Loadedsouce as Video).Dispose(); };
            DxBkGround.DrawProc += DrawCallback;

            BackGround.Visibility = Visibility.Hidden;
            new Task(() =>
            {
                (DxBkGround.Loadedsouce as Video).Start("assets\\title.wmv");

                this.Dispatcher.BeginInvoke(new Action(()=> {
                    DxBkGround.DrawStartup(BackGround);
                    BackGround.Visibility = Visibility.Visible;
                }));
              
            }).Start() ;
           
        }
    }
}
