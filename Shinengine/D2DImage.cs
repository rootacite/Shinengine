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

using System.Windows.Threading;
using System.Threading;
using Image = System.Windows.Controls.Image;

using System.Windows.Media.Imaging;

using System.Windows.Media;

using System.Windows;
using PInvoke;
using FFmpeg.AutoGen;
using System.Runtime.InteropServices;

namespace Shinengine
{
    public class Direct2DImage
    {
        [DllImport("Kernel32.dll")]
        unsafe extern public static void RtlMoveMemory(void* dst, void* sur, long size);
        private int Times = 0;//每画一帧，值自增1，计算帧率时归零
        public int Dpis { get; private set; } = 0;//表示当前的帧率
        public int Width { get; }//绘图区域的宽
        public int Height { get; }//绘图区域的长

        private readonly int TargetDpi;//目标帧率
        private readonly WriteableBitmap buffer = null;//图片源
        private readonly WICBitmap _bufferBack;//用于D2D绘图的WIC图片

        public double Speed = 0;//画每帧后等待的时间
        private readonly DispatcherTimer m_Dipter;//计算帧率的计时器
        private readonly Thread m_Dipter2;//绘图线程
        public delegate bool FarmeTask(WicRenderTarget view);


        private bool isRunning = false;//指示是否正在运行

        private WicRenderTarget View { get; } = null;//绘图目标
        unsafe public void Commit()
        {
            try
            {
                if (!isRunning)
                    return;
                buffer.Lock();
                var m_lock = _bufferBack.Lock(BitmapLockFlags.Read);
                RtlMoveMemory((void*)buffer.BackBuffer, (void*)m_lock.Data.DataPointer, buffer.PixelHeight * buffer.BackBufferStride);
             //   Kernel32.CopyMemory(Kernel32.GetCurrentProcess().DangerousGetHandle(), buffer.BackBuffer, m_lock.Data.DataPointer, (IntPtr)(buffer.PixelHeight * buffer.BackBufferStride), (IntPtr)0);
                m_lock.Dispose();

                buffer.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                buffer.Unlock();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }//把后台数据呈现到前台
        public Direct2DImage(Image contorl, int Fps, FarmeTask taskCallBack)
        {
            TargetDpi = Fps;
            m_Dipter = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            m_Dipter.Tick += (e, v) =>
            {
                Console.WriteLine("Reper");
                Dpis = Times;
                double TimesOfWait = Speed * Dpis;
                if (TimesOfWait > 1)
                {
                    Speed = 1.0d / TargetDpi;
                    Times = 0;
                    return;
                    //  MessageBox.Show(Speed.ToString()+","+Dpis.ToString());
                }
              //  Console.WriteLine(TimesOfWait.ToString());
                double TimeOfDraw = 1.0d - TimesOfWait;
                if (Dpis < TargetDpi)
                {
                    try
                    {
                        if (TimeOfDraw / (double)Dpis > 1.0d / TargetDpi) 
                        {
                            Speed = 0;
                        }
                        else
                        Speed = (1.0f - ((TimeOfDraw / (double)Dpis) * TargetDpi)) / TargetDpi;
                    }
                    catch
                    {
                        Speed = 1.0d / TargetDpi;
                    }
                }
                if (Dpis > TargetDpi)
                {
                    try
                    {
                        Speed = (1.0f - ((TimeOfDraw / (double)Dpis) * TargetDpi)) / TargetDpi;
                    }
                    catch
                    {
                        Speed = 1.0d / TargetDpi;
                    }
                }

                Times = 0;
            };
            m_Dipter2 = new Thread(() =>
            {//绘图代码


                while (isRunning)
                {
                    var UpData = taskCallBack(View);
                    if (UpData)
                        contorl.Dispatcher.Invoke(new Action(() =>
                        {

                            Commit();
                        }));


                    Thread.Sleep((int)(Speed * 1000.0d));
                    Times++;
                }
            });


            Speed = 1.0d / (double)TargetDpi;
            Width = (int)contorl.Width;
            Height = (int)contorl.Height;
            isRunning = true;

            
            buffer = new WriteableBitmap((int)contorl.Width, (int)contorl.Height, 72, 72, PixelFormats.Bgr32, null);
            contorl.Source = buffer;
            _bufferBack =
                new WICBitmap(
                new ImagingFactory(),
                (int)contorl.Width,
                (int)contorl.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);


            View = new WicRenderTarget(new D2DFactory(), _bufferBack, new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(Format.Unknown, AlphaMode.Unknown),
                0,
                0,
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT));


            View.BeginDraw();
            View.Clear(new RawColor4(1, 1, 1, 1));
            View.EndDraw();

            Commit();
           
            m_Dipter.Start();
            m_Dipter2.Start();
        }

        public void Dispose()
        {
            m_Dipter.Stop();
            isRunning = false;
            m_Dipter2.Abort();

            while (m_Dipter.IsEnabled || m_Dipter2.ThreadState != ThreadState.Aborted)
                Thread.Sleep(1);
            if (_bufferBack != null)
                _bufferBack.Dispose();
            if (View != null)
                View.Dispose();
        }//ignore
        ~Direct2DImage()
        {

        }//ignore

    }


}
