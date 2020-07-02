using System;

using Device = SharpDX.Direct3D11.Device1;

using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;


using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using System.Windows.Threading;
using System.Threading;
using Image = System.Windows.Controls.Image;

using System.Windows.Media.Imaging;

using System.Windows.Media;

using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
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
        public event StartTask FirstDraw;

        private bool isRunning = false;//指示是否正在运行
        private readonly System.Drawing.Bitmap bufferCaller = null;
        private readonly Graphics bufferSurface = null;
        private readonly SharpDX.Direct3D11.Device d3DDevice;// = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
        private readonly SharpDX.DXGI.Device dxgiDevice;// = d3DDevice.QueryInterface<Device>().QueryInterface<SharpDX.DXGI.Device>();

        private readonly SharpDX.Direct2D1.Device d2DDevice = null;

        public DeviceContext View { get; private set; } = null;//绘图目标


        public Direct2DImage(Size2 size, int Fps)
        {
            TargetDpi = Fps;
            d3DDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            var __dxgiDevice = d3DDevice.QueryInterface<Device>();
           dxgiDevice = __dxgiDevice.QueryInterface<SharpDX.DXGI.Device>();
            __dxgiDevice.Dispose();
            Speed = 1.0d / (double)TargetDpi;
            Width = (int)size.Width;
            Height = (int)size.Height;
            isRunning = true;

            DxFac = new D2DFactory();
            d2DDevice = new SharpDX.Direct2D1.Device(DxFac, dxgiDevice);
            buffer = new WriteableBitmap((int)size.Width, (int)size.Height, 72, 72, PixelFormats.Pbgra32, null);

            bufferCaller = new System.Drawing.Bitmap(size.Width, size.Height, buffer.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, buffer.BackBuffer);
            bufferSurface = Graphics.FromImage(bufferCaller);


            View = new DeviceContext(d2DDevice, DeviceContextOptions.EnableMultithreadedOptimizations);

            _ImagFc = new ImagingFactory();

            _bufferBack = new D2DBitmap(View, size,
                new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.Target | BitmapOptions.CannotDraw));
            View.Target = _bufferBack;

        }
        public void DrawStartup(Image contorl)
        {
            if (FirstDraw != null)
            {
                try
                {

                    //   Debug.WriteLine("Lock");

                    FirstDraw(View, null, Width, Height);


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
                Debug.WriteLine("Speed:" + Dpis.ToString());

                Times = 0;
            };
            m_Dipter2 = new Task(() =>
            {//绘图代码

                while (isRunning)
                {


                    DrawProcResult? UpData = null;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    UpData = DrawProc?.Invoke(View, Loadedsouce, Width, Height);
                    contorl.Dispatcher.Invoke(new Action(() => { Commit(); }));
                    sw.Stop();

                    decimal time = sw.ElapsedTicks / (decimal)Stopwatch.Frequency * 1000;
                    decimal wait_time = 1000.0M / (decimal)TargetDpi - time;

                    if (wait_time < 0)
                    {
                        wait_time = 0;
                    }
                    Debug.WriteLine("Time:" + time.ToString());
                    if (UpData == null)
                    {
                        throw new Exception();
                    }
                    if (UpData == DrawProcResult.Normal)
                    {
                        Thread.Sleep((int)wait_time);
                        Times++;
                        continue;
                    }
                    if (UpData == DrawProcResult.Ignore)
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
 
                m_Dipter2.Dispose();
                _bufferBack.Dispose();
                bufferSurface.Dispose();
                bufferCaller.Dispose();
                d2DDevice.Dispose();
                DxFac.Dispose();
                dxgiDevice.Dispose();
                d3DDevice.Dispose();

                GC.Collect();
            }).Start();
        }//ignore
        ~Direct2DImage()
        {

        }//ignore
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
