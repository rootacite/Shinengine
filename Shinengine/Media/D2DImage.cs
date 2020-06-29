using System;
using System.Collections.Generic;
using System.Text;

using Device = SharpDX.Direct3D11.Device1;

using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using System.Net.WebSockets;
using Blend = SharpDX.Direct2D1.Effects.Blend;
using System.Text;


using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory1;
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

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
using MapFlags = SharpDX.DXGI.MapFlags;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using System.Drawing;

namespace Shinengine.Media
{
    public enum DrawProcResult
    {
        Normal,
        Ignore,
        Death
    }
    public class Direct2DImage
    {

        public object Loadedsouce = null;
        [DllImport("Kernel32.dll")]
        unsafe extern public static void RtlMoveMemory(void* dst, void* sur, long size);

        [DllImport("Shinehelper.dll")]
        extern static public IntPtr GetDskWindow();
        private int Times = 0;//每画一帧，值自增1，计算帧率时归零
        public int Dpis { get; private set; } = 0;//表示当前的帧率
        public int Width { get; }//绘图区域的宽
        public int Height { get; }//绘图区域的长

        public int TargetDpi;//目标帧率

        private readonly WriteableBitmap buffer = null;//图片源
        public readonly ImagingFactory _ImagFc = null;
        private readonly D2DBitmap _bufferBack;//用于D2D绘图的WIC图片
        private readonly D2DFactory DxFac = null;

        public double Speed = 0;//画每帧后等待的时间
        private DispatcherTimer m_Dipter;//计算帧率的计时器
        private Task m_Dipter2;//绘图线程
        public delegate DrawProcResult FarmeTask(DeviceContext view, object Loadedsouce, int Width, int Height);

        public delegate void StartTask(DeviceContext view, WICBitmap last, int Width, int Height);
        public delegate void EndTask(object Loadedsouce, WICBitmap _buff);

        public event EndTask Disposed;
        public event FarmeTask DrawProc;
        public event StartTask StartDrawing;

        private bool isRunning = false;//指示是否正在运行
        private System.Drawing.Bitmap bufferCaller = null;
        private Graphics bufferSurface = null;

        public DeviceContext View { get; private set; } = null;//绘图目标


        public Direct2DImage(Size2 size, int Fps)
        {
            TargetDpi = Fps;



            Speed = 1.0d / (double)TargetDpi;
            Width = (int)size.Width;
            Height = (int)size.Height;
            isRunning = true;

            buffer = new WriteableBitmap((int)size.Width, (int)size.Height, 72, 72, PixelFormats.Pbgra32, null);
            bufferCaller = new System.Drawing.Bitmap(size.Width, size.Height, buffer.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, buffer.BackBuffer);
            bufferSurface = Graphics.FromImage(bufferCaller);

            SharpDX.Direct3D11.Device d3DDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);

            SharpDX.DXGI.Device dxgiDevice = d3DDevice.QueryInterface<Device>().QueryInterface<SharpDX.DXGI.Device>();

        
            SharpDX.Direct2D1.Device d2DDevice = new SharpDX.Direct2D1.Device(DxFac = new D2DFactory(), dxgiDevice);

            View = new DeviceContext(d2DDevice, DeviceContextOptions.EnableMultithreadedOptimizations);

            _ImagFc = new ImagingFactory();

            _bufferBack = new D2DBitmap(View, size,
                new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.Target | BitmapOptions.CannotDraw));
            View.Target = _bufferBack;


        }
        public void DrawStartup(Image contorl)
        {
            if (StartDrawing != null)
            {
                try
                {

                    //   Debug.WriteLine("Lock");

                    StartDrawing(View, null, Width, Height);


                    contorl.Dispatcher.Invoke(new Action(() => { Commit(); }));


                    //  Debug.WriteLine("UnLock");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
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

                    Debug.WriteLine("Speed:" + Speed.ToString());
                    DrawProcResult? UpData = null;
                    
                    UpData = DrawProc?.Invoke(View, Loadedsouce, Width, Height);
                    contorl.Dispatcher.Invoke(new Action(() => { Commit(); }));

                    if (UpData == null)
                    {
                        throw new Exception();
                    }
                    if (UpData == DrawProcResult.Normal)
                    {
                        Thread.Sleep((int)(Speed * 1000.0d));
                        Times++;
                        continue;
                    }
                    if(UpData == DrawProcResult.Ignore)
                    {
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

            contorl.Source = buffer;

            m_Dipter.Start();
            m_Dipter2.Start();
        }
        public void Dispose()
        {
            Console.WriteLine("dispose called");


            new Task(() =>
            {
                m_Dipter?.Stop();
                isRunning = false;

                m_Dipter2?.Wait();

                D2DBitmap m_local_buffer = new D2DBitmap(View, new Size2(Width, Height),
               new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.CannotDraw | BitmapOptions.CpuRead));

                m_local_buffer.CopyFromBitmap(_bufferBack);
                var m_end_map = m_local_buffer.Map(MapOptions.Read);
                var m_end_bitmap_wic = new WICBitmap(_ImagFc, Width, Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(m_end_map.DataPointer, m_end_map.Pitch));


                Disposed?.Invoke(Loadedsouce, m_end_bitmap_wic);

                m_local_buffer.Unmap();
                m_local_buffer.Dispose();

                if (View != null)
                    View.Dispose();
                if (_ImagFc != null)
                    _ImagFc.Dispose();
                if (DxFac != null)
                    DxFac.Dispose();
                m_Dipter2.Dispose();
                _bufferBack.Dispose();
                bufferSurface.Dispose();
                bufferCaller.Dispose();
            }).Start();
        }//ignore
        ~Direct2DImage()
        {

        }//ignore
        bool intp = false;
        Graphics mppp = Graphics.FromHwnd(GetDskWindow());
        unsafe public void Commit()
        {
            D2DBitmap m_local_buffer = new D2DBitmap(View, new Size2(Width, Height),
                new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.CannotDraw | BitmapOptions.CpuRead));//建立一个可以从CPU读取的bitrmap

            m_local_buffer.CopyFromBitmap(_bufferBack);//复制缓冲区

            buffer.Lock();

            var m_lock = m_local_buffer.Map(MapOptions.Read);

            var m_bp = new System.Drawing.Bitmap(m_local_buffer.PixelSize.Width, m_local_buffer.PixelSize.Height, m_lock.Pitch, System.Drawing.Imaging.PixelFormat.Format32bppArgb, m_lock.DataPointer);
           
            bufferSurface.Clear(System.Drawing.Color.Transparent);
            bufferSurface.DrawImage(m_bp, new System.Drawing.Point(0, 0));

            m_bp.Dispose();
            //
            m_local_buffer.Unmap();

            buffer.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            buffer.Unlock();

            m_local_buffer.Dispose();
           
        }//把后台数据呈现到前台
    }


}
