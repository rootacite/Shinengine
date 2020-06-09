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

namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Direct2DImage DxBkGround = null;
        bool enClose = false;
        private bool draws=true;
        
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        public SharpDX.DirectWrite.Factory DwoR { get; private set; }
        public TextFormat TF { get; private set; }
        public bool CanRun { get; private set; } = true;
        Video video = null;

        [DllImport("Shinehelper.dll")]
        extern static public void waveInit();
        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();
            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            timeBeginPeriod(1);
            waveInit();


            DwoR = new SharpDX.DirectWrite.Factory();
            TF = new TextFormat(DwoR, "宋体", 24);
            // return;
            Loaded += (s, e) =>
            {

                DxBkGround = new Direct2DImage(BackGround, 30, (view) =>
                {
                    SharpDX.Direct2D1.Bitmap farme = null;
                    if (video == null)
                        return false;
                    if (video.bits.Count - video.nFarm < 60)
                        return false;
                    try
                    {
                        farme = SharpDX.Direct2D1.Bitmap.FromWicBitmap(view, video.bits[video.nFarm]);
                    }
                    catch
                    {
                        return false;
                    }

                    view.BeginDraw();

                    view.DrawBitmap(farme,
                        new RawRectangleF(0, 0, DxBkGround.Width, DxBkGround.Height),
                        1, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
                        new RawRectangleF(0, 0, video.bits[video.nFarm].Size.Width, video.bits[video.nFarm].Size.Height));
                    view.DrawText("我草你妈", TF, new RawRectangleF(0, 0, 500, 100), new SolidColorBrush(view, new RawColor4(1, 1, 0, 1)));
                    view.EndDraw();
                    farme.Dispose();
                    video.bits[video.nFarm].Dispose();
                    video.nFarm++;
                    return draws;
                });
                new Thread(() =>
                {
                    while (!enClose)
                    {

                        Dispatcher.Invoke(new Action(() => { this.Title = (video.bits.Count- video.nFarm).ToString(); }));
                        Thread.Sleep(50);
                    }
                }).Start();
               
                new Thread(() =>
                {
                    while (true) {
                        video = new Video();
                        video.Start("D:\\OPmovie.wmv");
                        while (video.bits.Count - video.nFarm > 60)
                            Thread.Sleep(1);
                    }
                }).Start();
                return;
                //  m_Dipter.Start();
            };


            
        }

        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
            draws = !draws;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
            Kernel32.ExitProcess(0);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
           // _Position = e.GetPosition(this);
        }

      

       
    }
}
