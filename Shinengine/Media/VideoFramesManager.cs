using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using Shinengine.Media;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

using WICBitmap = SharpDX.WIC.Bitmap;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.IO;

using Bitmap = System.Drawing.Bitmap;
using BmpBitmapEncoder = System.Windows.Media.Imaging.BmpBitmapEncoder;
using BitmapEncoder = System.Windows.Media.Imaging.BitmapEncoder;
using BitmapDecoder = SharpDX.WIC.BitmapDecoder;
using Shinengine.Data;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Blend = SharpDX.Direct2D1.Effects.Blend;
using Point = System.Drawing.Point;
namespace Shinengine.Media
{
    sealed public class VideoFramesManager : IDisposable
    {
        static private List<WICBitmap> GetAllFrames(string url)
        {
            ImagingFactory m_fc_av = new ImagingFactory();
            List<WICBitmap> result = new List<WICBitmap>();

            var m_sourc = new VideoStreamDecoder(url);
            while (true)
            {
                var res = m_sourc.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                if (!res)
                {
                    break;
                }
                var BitSrc = new WICBitmap(m_fc_av, m_sourc.FrameSize.Width, m_sourc.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));
                ; result.Add(BitSrc);
            }
            m_sourc.Dispose();
            return result;
        }
        private readonly WICBitmap[] _Frames = null;
        public WICBitmap[] Frames { 
            get 
            {
                return _Frames;
            } 
        }
        public VideoFramesManager(string assets_loaction)
        {

            List<WICBitmap> frames = GetAllFrames(assets_loaction);
            _Frames = frames.ToArray();
        }
        private int n_frame = 0;
        public int Loop_time { get; private set; } = 0;
        public WICBitmap GetFrame()
        {
            var m_result = _Frames[n_frame++];

            if (n_frame == _Frames.Length)
            {
                n_frame = 0;
                Loop_time++;
            }

            return m_result;
        }


        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    foreach (var i in _Frames)
                    {
                        i.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
         ~VideoFramesManager()
         {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
             Dispose(disposing: false);
         }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
