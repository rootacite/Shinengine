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

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;

namespace Shinengine.Media
{
    public enum DrawProcResult
    {
        Normal,
        Ignore,
        Death
    }

    public delegate void StartTask(DeviceContext view, WICBitmap last, int Width, int Height);
    public delegate void EndedTask();
    public delegate void EndingTask(object Loadedsouce, Direct2DImage self);
    public delegate DrawProcResult DrawProcTask(DeviceContext view, object Loadedsouce, int Width, int Height);
    sealed public class Direct2DImage : IDisposable
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

        private WriteableBitmap buffer = null;//图片源
        public readonly ImagingFactory _ImagFc = null;
        public readonly D2DBitmap _bufferBack;//用于D2D绘图的WIC图片
        private readonly D2DFactory DxFac = null;

        public double Speed = 0;//画每帧后等待的时间
        private DispatcherTimer m_Dipter;//计算帧率的计时器
        private Thread m_Dipter2;//绘图线程


        public event EndedTask Disposed;
        public event EndingTask Disposing;

        public event DrawProcTask DrawProc;
        public event StartTask FirstDraw;

        private bool isRunning = false;//指示是否正在运行
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


            View = new DeviceContext(d2DDevice, DeviceContextOptions.EnableMultithreadedOptimizations);
            _ImagFc = new ImagingFactory();
            _bufferBack = new D2DBitmap(View, size,
                new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.Target | BitmapOptions.CannotDraw));
            View.Target = _bufferBack;


            buffer = new WriteableBitmap((int)size.Width, (int)size.Height, 72, 72, PixelFormats.Pbgra32, null);

        }
        public void DrawStartup(Image contorl)
        {
            if (FirstDraw != null)
            {
                try
                {
                    FirstDraw(View, null, Width, Height);
                    contorl.Dispatcher.Invoke(new Action(() => { Commit(); }));
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
            m_Dipter2 = new Thread(() =>
            {//绘图代码

                while (isRunning)
                {


                    DrawProcResult? UpData = null;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    UpData = DrawProc?.Invoke(View, Loadedsouce, Width, Height);
                    if (!(UpData == DrawProcResult.Ignore || UpData == null)) contorl.Dispatcher.Invoke(new Action(() => { Commit(); }));

                    sw.Stop();

                    decimal time = sw.ElapsedTicks / (decimal)Stopwatch.Frequency * 1000;
                    decimal wait_time = 1000.0M / (decimal)TargetDpi - time;

                    if (wait_time < 0)
                    {
                        wait_time = 0;
                    }

                    if (UpData == DrawProcResult.Normal || UpData == null)
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

                Disposing?.Invoke(Loadedsouce, this);

                buffer = null;

                _bufferBack?.Dispose();
                View?.Dispose();

                _ImagFc?.Dispose();


                d2DDevice?.Dispose();
                DxFac?.Dispose();
                dxgiDevice?.Dispose();
                d3DDevice?.Dispose();

                Disposed?.Invoke();
            })
            { IsBackground = true };

            contorl.Source = buffer;

            m_Dipter.Start();
            m_Dipter2.Start();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);

        }
        private void Dispose(bool isdisposing)
        {
            if (IsDisposed) return;
            IsDisposed = true;
            m_Dipter?.Stop();
           

            if (isdisposing)
            {
               
            }

            isRunning = false;
        }
        public bool IsDisposed { get; private set; } = false;
        public WICBitmap LastDraw
        {
            get
            {
                D2DBitmap m_local_buffer = new D2DBitmap(View, new Size2(Width, Height),
            new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.CannotDraw | BitmapOptions.CpuRead));

                m_local_buffer.CopyFromBitmap(_bufferBack);
                var m_end_map = m_local_buffer.Map(MapOptions.Read);
                var m_end_bitmap_wic = new WICBitmap(_ImagFc, Width, Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(m_end_map.DataPointer, m_end_map.Pitch));

                m_local_buffer.Unmap();
                m_local_buffer.Dispose();

                return m_end_bitmap_wic;
            }
        }
        ~Direct2DImage()
        {
            Dispose(false);
        }//ignore
        unsafe public void Commit()
        {
            D2DBitmap m_copied_buffer = new D2DBitmap(View, new Size2(Width, Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied), 72, 72, BitmapOptions.CannotDraw | BitmapOptions.CpuRead));
            m_copied_buffer.CopyFromBitmap(_bufferBack);

            var m_copied_map = m_copied_buffer.Map(MapOptions.Read);

            buffer.Lock();

            for (int i = 0; i < m_copied_buffer.PixelSize.Height; i++)
            {
                int* source_base = (int*)(m_copied_map.DataPointer + i * m_copied_map.Pitch);
                int* target_base = (int*)(buffer.BackBuffer + i * buffer.BackBufferStride);

                RtlMoveMemory((void*)target_base, (void*)source_base, 4 * m_copied_buffer.PixelSize.Width);
            }

            buffer.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            buffer.Unlock();

            m_copied_buffer.Unmap();
            m_copied_buffer.Dispose();
        }//把后台数据呈现到前台

    }


}
