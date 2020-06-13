using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using SharpDX.DirectWrite;
using System.Windows.Threading;

using PInvoke;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;


using SharpDX;
using SharpDX.WIC;

using SharpDX.DirectSound;

using WICBitmap = SharpDX.WIC.Bitmap;
using System.Windows.Interop;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using SharpDX.Direct2D1;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ImageBrush = System.Windows.Media.ImageBrush;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Direct2DImage DxBkGround = null;
        private bool draws=true;

        
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        public SharpDX.DirectWrite.Factory DwoR { get; private set; }
        public TextFormat TF { get; private set; }
        public bool CanRun { get; private set; } = true;

        /// <summary>
        /// 操作控件的Z顺序
        /// </summary>
        /// <param name="sender">菜单</param>
        /// <param name="moveToFront">True 前移 ，false 置后</param>
        /// <param name="toBottom">true 移动至底端，false 移动一层</param>


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
                if (DxBkGround != null)
                {
                    (DxBkGround.Loadedsouce as Video).CanRun = false;
                    DxBkGround.Dispose();
                }
                //  m_sce = new Video();
                //  new Thread(() =>
                //  {
                //     m_sce.Start("D:\\OPmovie.wmv");
                // }).Start();

                DxBkGround = new Direct2DImage(BackGround, 30, (view, Loadedsouce, Width, Height) =>
                {
                    //   Debug.WriteLine("1234567");
                    var m_sce = (Loadedsouce as Video);
                    SharpDX.Direct2D1.Bitmap farme = null;
                    if (m_sce == null)
                        return false;
                    if (m_sce.bits.Count - m_sce.nFarm < 60)
                        return false;
                    try
                    {
                        farme = SharpDX.Direct2D1.Bitmap.FromWicBitmap(view, m_sce.bits[m_sce.nFarm]);
                    }
                    catch
                    {
                        return false;
                    }

                    view.BeginDraw();

                    view.DrawBitmap(farme,
                        new RawRectangleF(0, 0, Width, Height),
                        1, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
                        new RawRectangleF(0, 0, m_sce.bits[m_sce.nFarm].Size.Width, m_sce.bits[m_sce.nFarm].Size.Height));
                    view.EndDraw();
                    farme.Dispose();
                    m_sce.bits[m_sce.nFarm].Dispose();
                    m_sce.nFarm++;

                    return draws;
                })
                { Loadedsouce = new Video() };
                new Thread(() =>
                {
                    (DxBkGround.Loadedsouce as Video).Start("D:\\OPmovie.wmv");
                }).Start();
                DxBkGround.DrawStartup();
            };
            
            BkGrid.Children.Add(m_btn);
            return;
        }
        public void Reset()
        {
           

        }
        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
          
                //m_sce.Dispose();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
            Kernel32.ExitProcess(0);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            // _Position = e.GetPosition(this);
         //   Canvas.SetZIndex(test, Canvas.GetZIndex(test) - 1);
        }

      

       
    }
}
