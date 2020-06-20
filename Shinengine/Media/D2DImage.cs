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
using FFmpeg.AutoGen;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shinengine.Media
{
    public enum DrawProcResult
    {
        Ignore,
        Commit,
        Death
    }
    public class Direct2DImage
    {

        public object Loadedsouce = null;
        [DllImport("Kernel32.dll")]
        unsafe extern public static void RtlMoveMemory(void* dst, void* sur, long size);
        private int Times = 0;//每画一帧，值自增1，计算帧率时归零
        public int Dpis { get; private set; } = 0;//表示当前的帧率
        public int Width { get; }//绘图区域的宽
        public int Height { get; }//绘图区域的长

        public int TargetDpi;//目标帧率

        private readonly WriteableBitmap buffer = null;//图片源
        private readonly ImagingFactory _ImagFc = null;
        private readonly WICBitmap _bufferBack;//用于D2D绘图的WIC图片
        private readonly D2DFactory DxFac = null;

        public double Speed = 0;//画每帧后等待的时间
        private  DispatcherTimer m_Dipter;//计算帧率的计时器
        private  Task m_Dipter2;//绘图线程
        public delegate DrawProcResult FarmeTask(WicRenderTarget view, object Loadedsouce, int Width, int Height);

        public delegate bool StartTask(WicRenderTarget view, WICBitmap last, int Width, int Height);
        public delegate void EndTask(object Loadedsouce);

        public event EndTask Disposed;
        public event FarmeTask DrawProc;
        public event StartTask StartDrawing;

        private bool isRunning = false;//指示是否正在运行

        private WicRenderTarget View { get; } = null;//绘图目标

        unsafe public void Commit()
        {
          try
            {
                
             //   Debug.WriteLine("Lock");
                buffer.Lock();
                var m_lock = _bufferBack.Lock(BitmapLockFlags.Read);
                RtlMoveMemory((void*)buffer.BackBuffer, (void*)m_lock.Data.DataPointer, buffer.PixelHeight * buffer.BackBufferStride);
             //   Kernel32.CopyMemory(Kernel32.GetCurrentProcess().DangerousGetHandle(), buffer.BackBuffer, m_lock.Data.DataPointer, (IntPtr)(buffer.PixelHeight * buffer.BackBufferStride), (IntPtr)0);
                m_lock.Dispose();

                buffer.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
                buffer.Unlock();

              //  Debug.WriteLine("UnLock");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }//把后台数据呈现到前台


        public Direct2DImage(Size2 size, int Fps)
        {
            Debug.WriteLine("Dx Startup");
            TargetDpi = Fps;
        


            Speed = 1.0d / (double)TargetDpi;
            Width = (int)size.Width;
            Height = (int)size.Height;
            isRunning = true;

            
            buffer = new WriteableBitmap((int)size.Width, (int)size.Height, 72, 72, PixelFormats.Bgr32, null);
 

            _ImagFc = new ImagingFactory();
            _bufferBack =
                new WICBitmap(
                _ImagFc,
                (int)size.Width,
                (int)size.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);

            DxFac = new D2DFactory();
            View = new WicRenderTarget(DxFac, _bufferBack, new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(Format.Unknown, AlphaMode.Unknown),
                0,
                0,
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT));
          
            
        }
        public void DrawStartup(Image contorl)
        {
            if (contorl.Source != null)
            {
                WriteableBitmap rectBuf = contorl.Source as WriteableBitmap;


                if (StartDrawing != null)
                {
                    var UpData = StartDrawing(View, new WICBitmap(_ImagFc,Width,Height, SharpDX.WIC.PixelFormat.Format32bppBGR,new DataRectangle(rectBuf.BackBuffer, rectBuf.BackBufferStride)), Width, Height);
                    //  var litmit = true;
                    if (UpData)
                        contorl.Dispatcher.Invoke(new Action(() =>
                        {
                            Commit();
                            //     litmit = false;
                        }));
                }

                
            }

            m_Dipter = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            m_Dipter.Tick += (e, v) =>
            {

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
            m_Dipter2 = new Task(() =>
            {//绘图代码

                while (isRunning)
                {
                    //  Debug.WriteLine("Start Draw");
                    var UpData = DrawProc?.Invoke(View, Loadedsouce, Width, Height);
                    //  var litmit = true;
                    if (UpData == null)
                    {
                        throw new Exception();
                    }
                    if (UpData == DrawProcResult.Commit)
                    {
                        contorl.Dispatcher.Invoke(new Action(() =>
                        {
                            Commit();
                            //     litmit = false;
                        }));
                        Thread.Sleep((int)(Speed * 1000.0d));
                        Times++;
                        continue;
                    }

                    // while (litmit&&isRunning)
                    //      Thread.Sleep(1);
                    if (UpData == DrawProcResult.Ignore)
                    {
                        Thread.Sleep((int)(Speed * 1000.0d));
                        continue;
                    }
                    if (UpData == DrawProcResult.Death)
                    {
                        this.Dispose();
                        break;
                    }
                }

                Debug.WriteLine("Dispod");


            });

            m_Dipter.Start();
            m_Dipter2.Start();
            contorl.Source = buffer;
        }
        public void Dispose()
        {
            Console.WriteLine("dispose called");


            new Thread(() =>
            {
                m_Dipter?.Stop();
                isRunning = false;

                m_Dipter2?.Wait();



                if (_bufferBack != null)
                    _bufferBack.Dispose();
                if (View != null)
                    View.Dispose();
                if (_ImagFc != null)
                    _ImagFc.Dispose();
                if (DxFac != null)
                    DxFac.Dispose();
                m_Dipter2.Dispose();



                Disposed?.Invoke(Loadedsouce);
            }).Start();
        }//ignore
        ~Direct2DImage()
        {

        }//ignore

    }


}
