using System;
using System.Collections.Generic;
using System.Text;


using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using SharpDX.Mathematics.Interop;
using System.Drawing;
using System.Windows.Threading;
using Point = System.Windows.Point;
using System.Windows.Interop;
using System.Threading;
using System.Reflection;
using Image = System.Windows.Controls.Image;
using SolidColorBrush = SharpDX.Direct2D1.SolidColorBrush;

using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;
using System.Windows;
using System.Windows.Media;

using SypFormat = System.Drawing.Imaging.PixelFormat;
using PInvoke;

namespace Shinengine
{
    public class Direct2DImage
    {
        private int Times = 0;
        public int Dpis { get; private set; } = 0;

        public bool isReadyForDraw
        {
            get
            {
                return taskEvent == null;
            }
        }
        
        public int Width { get; }
        public int Height { get; }

        private int TargetDpi;
        private WriteableBitmap buffer = null;
        private Bitmap _realBack;
        private WICBitmap _bufferBack;
        private Graphics gPa = null;
        private double Speed = 0.01;
        private DispatcherTimer m_Dipter;

        public delegate void FarmeTask();

        private FarmeTask taskEvent = null;
        private Thread m_Dipter2;
        private bool isRunning = false;

        public void SetRenderTask(FarmeTask task)
        {
            taskEvent = task;
        }


        public WicRenderTarget View { get; } = null;
        public bool CannotDraw = false;
        unsafe public void Commit()
        {
            CannotDraw = true;
            try
            {
                if (!isRunning)
                    return;
                buffer.Lock();
                var m_lock = _bufferBack.Lock(BitmapLockFlags.Read);
                Kernel32.CopyMemory((void*)buffer.BackBuffer, (void*)m_lock.Data.DataPointer,(IntPtr)(buffer.PixelHeight * buffer.BackBufferStride));
                m_lock.Dispose();

                

                buffer.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                buffer.Unlock();
            }
            catch
            {
                // MessageBox.Show("");
            }
            CannotDraw = false;
        }
        public Direct2DImage(Image contorl, int Fps)
        {
            Width = (int)contorl.Width;
            Height = (int)contorl.Height;

            TargetDpi = Fps;

            buffer = new WriteableBitmap((int)contorl.Width, (int)contorl.Height, 72, 72, PixelFormats.Bgr32, null);
            _realBack = new Bitmap((int)contorl.Width, (int)contorl.Height, buffer.BackBufferStride, SypFormat.Format32bppRgb, buffer.BackBuffer);
            gPa = Graphics.FromImage(_realBack);

            var ImFactory = new ImagingFactory();
            _bufferBack =
            new WICBitmap(
                ImFactory,
                (int)contorl.Width,
                (int)contorl.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);

            var Factory = new D2DFactory();
            var renderTargetProperties =
            new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(Format.Unknown, AlphaMode.Unknown),
                0,
                0,
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);
            View = new WicRenderTarget(Factory, _bufferBack, renderTargetProperties);


            View.BeginDraw();
            View.Clear(new RawColor4(1, 1, 1, 1));
            View.EndDraw();


            Commit();
            contorl.Source = buffer;
            Speed = 1.0d / (double)TargetDpi;

            m_Dipter = new DispatcherTimer();
            m_Dipter.Interval = TimeSpan.FromSeconds(1);
            m_Dipter.Tick += (e, v) =>
            {
                int LastDpi = Times;
                Dpis = LastDpi;
                Times = 0;

                if (LastDpi < TargetDpi)
                {
                    try
                    {
                        //     double TimesOfWait = Speed * LastDpi;
                        double TimeOfDraw = (1.0d - (Speed * LastDpi)) / (double)LastDpi;
                        if (TimeOfDraw > 0.01)
                        {
                            Speed = 1.0d / (double)TargetDpi;
                            return;
                        }
                        Speed = (1.0f - (TimeOfDraw * TargetDpi)) / TargetDpi;
                    }
                    catch
                    {
                        Speed = 1.0d / (double)TargetDpi;

                    }
                }
                if (LastDpi > TargetDpi)
                {
                    try
                    {
                        Speed = (1.0f - (((1.0d - (Speed * LastDpi)) / (double)LastDpi) * TargetDpi)) / TargetDpi;
                    }
                    catch
                    {
                        Speed = 1.0d / (double)TargetDpi;
                    }
                }
            };

            m_Dipter2 = new Thread(
                   () => {//绘图代码
                       while (true)
                       {

                           if (taskEvent == null)
                           {
                               m_Dipter.IsEnabled = false;
                               while (taskEvent == null)
                               {
                                   Thread.Sleep(1);
                               }
                               m_Dipter.IsEnabled = true;
                           }

                           taskEvent();
                           taskEvent = null;


                           if (!isRunning)
                               break;



                           contorl.Dispatcher.Invoke(new Action(() =>
                           {

                               Commit();
                           }));


                           Thread.Sleep((int)(Speed * 1000.0d));
                           Times++;
                       }

                   });
            isRunning = true;
            m_Dipter.Start();
            m_Dipter2.Start();


        }

        public void Dispose()
        {
            m_Dipter.Stop();
            isRunning = false;
            //m_Dipter2.Abort();

            while (m_Dipter.IsEnabled || m_Dipter2.ThreadState == ThreadState.Running)
                Thread.Sleep(1);
            if (_realBack != null)
                _realBack.Dispose();
            if (_bufferBack != null)
                _bufferBack.Dispose();
            if (View != null)
                View.Dispose();
        }
        ~Direct2DImage()
        {

        }

    }


}
