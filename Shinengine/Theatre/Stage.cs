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


namespace Shinengine.Theatre
{


    public enum ImageEffects
    {
        BW_Effect
    }
    sealed public class Stage : IDisposable
    {
        public bool IsDisposed { get; private set; } = false;
        public void Dispose()
        {
            if (IsDisposed) return;

            if (this.videoCtrl != null)
            {
                var dispoer = new Thread(() =>
                {
                    ManualResetEvent ficter = new ManualResetEvent(false);
                    this.videoCtrl.Disposed += () =>
                    {
                        ficter.Set();
                    };
                    this.videoCtrl.Dispose();

                    ficter.WaitOne();
                    ficter.Dispose();
                })
                {
                    IsBackground = true
                };
                dispoer.Start();
            }
            if (buffer?.IsDisposed == false)
                buffer.Dispose();
            if (!last_save.IsDisposed)
                last_save.Dispose();

            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
        ~Stage()
        {
            Dispose();
        }



        public Theatre m_father = null;
        #region system tools
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));
            using MemoryStream outStream = new MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapImage));
            enc.Save(outStream);
            Bitmap bitmap = new System.Drawing.Bitmap(outStream);
            return new Bitmap(bitmap);
        }
        public static D2DBitmap ConvertFromSystemBitmap(System.Drawing.Bitmap bmp, DeviceContext renderTarget)
        {
            System.Drawing.Bitmap desBitmap;//预定义要是使用的bitmap
            //如果原始的图像像素格式不是32位带alpha通道
            //需要转换为32位带alpha通道的格式
            //否则无法和Direct2D的格式对应
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                desBitmap = new System.Drawing.Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(desBitmap);
                g.DrawImage(bmp, 0, 0);
            }
            else
            {
                desBitmap = bmp;
            }


            //直接内存copy会非常快
            //如果使用循环逐点转换会非常慢
            System.Drawing.Imaging.BitmapData bmpData = desBitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, desBitmap.Width, desBitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,
                        desBitmap.PixelFormat
                    );
            int numBytes = bmpData.Stride * desBitmap.Height;
            byte[] byteData = new byte[numBytes];
            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptr, byteData, 0, numBytes);
            desBitmap.UnlockBits(bmpData);



            BitmapProperties1 bp;
            PixelFormat pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);

            bp = new BitmapProperties1(
                      pixelFormat,
                      desBitmap.HorizontalResolution,
                      desBitmap.VerticalResolution
                    );
            D2DBitmap tempBitmap = new D2DBitmap(renderTarget, new Size2(desBitmap.Width, desBitmap.Height), bp);
            tempBitmap.CopyFromMemory(byteData, bmpData.Stride);

            return tempBitmap;
        }
        public static WICBitmap LoadBitmap(string init_pic)
        {
            string rele_path = PackStream.Locate(init_pic);
            var Imgc = new ImagingFactory();
            var Demcoder = new BitmapDecoder(Imgc, rele_path, SharpDX.IO.NativeFileAccess.Read, DecodeOptions.CacheOnLoad);

            BitmapFrameDecode nm_opb = Demcoder.GetFrame(0);
            var convert = new FormatConverter(Imgc);
            convert.Initialize(nm_opb, SharpDX.WIC.PixelFormat.Format32bppPBGRA);

            var Init_action = new WICBitmap(Imgc, convert, BitmapCreateCacheOption.CacheOnLoad);

            Imgc.Dispose();
            Demcoder.Dispose();
            nm_opb.Dispose();
            convert.Dispose();
            try
            {
                File.Delete(rele_path);
            }
            catch
            {

            }
            return Init_action;
        }

        #endregion
        public WICBitmap last_save = null;
        public Direct2DImage videoCtrl = null;
        public Image Background { get; private set; } = null;
        public Stage(Image bk)
        {
            Background = bk;
        }

        /// <summary>
        /// 将舞台的背景设置为一个图片，如果是第一次调用,并且没有调用过SetLastDraw，time必须为0
        /// </summary>
        /// <param name="url">图片路径</param>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void SetAsImage(string url, double? time = null, bool isAsyn = false)
        {
            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {
                ManualResetEvent ficter = new ManualResetEvent(false);

 

                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            double vara = 1.0, varb = 0.0;

            double increment = time != 0 ? 1 / ((double)time * 30) : 1.0;

            #endregion

            ManualResetEvent wait_sp = null;
            if (!isAsyn) wait_sp = new ManualResetEvent(false);
            Background.Dispatcher.Invoke(new Action(() =>
            {
                WICBitmap mbp_ss = Stage.LoadBitmap(url);//第一次申请资源
                videoCtrl = new Direct2DImage(new Size2((int)mbp_ss.Size.Width, (int)mbp_ss.Size.Height), 30)
                {
                    Loadedsouce = null
                };


                D2DBitmap ral_picA = last_save == null ? null : D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, mbp_ss);




                videoCtrl.FirstDraw += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    if (ral_picA != null)
                        t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);

                    t.EndDraw();
                    return;
                };

                videoCtrl.DrawProc += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    if (ral_picA != null)
                        t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawBitmap(ral_picB, new RawRectangleF(0, 0, b, s), (float)varb, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, mbp_ss.Size.Width, mbp_ss.Size.Height), null);

                    t.EndDraw();
                    if (vara <= 0 || varb >= 1)
                    {
                        return DrawProcResult.Death;
                    }
                    vara -= increment;
                    varb += increment;
                    return DrawProcResult.Normal;
                };
                videoCtrl.SouceDisposing += (e,s ) =>
                {
                    if (ral_picA != null)
                        ral_picA.Dispose();
                    ral_picB.Dispose();
                    if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s.LastDraw;
                    mbp_ss.Dispose();

                    if (wait_sp != null)
                        wait_sp.Set();
                };
                videoCtrl.Disposed += () => { videoCtrl = null; };
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp != null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }//安全
        /// <summary>
        /// 将舞台的背景设置为一个视频，如果是第一次调用,并且没有调用过SetLastDraw，time必须为0 
        /// </summary>
        /// <param name="url">图片路径</param>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void SetAsVideo(string url, double? time = null, bool isAsyn = false, bool loop = true)
        {

            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {

                ManualResetEvent ficter = new ManualResetEvent(false);
 
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            //////////////////////////////////
            D2DBitmap ral_pic = null;
            var m_sourc = new VideoStreamDecoder(url);

            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2(m_sourc.FrameSize.Width, m_sourc.FrameSize.Height), 30)
                {
                    Loadedsouce = m_sourc
                };


            }));
            if (last_save != null)
                ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

            videoCtrl.FirstDraw += (t, m, b, s) =>
            {
                t.BeginDraw();
                if (ral_pic != null)
                    t.DrawBitmap(ral_pic,
                       new RawRectangleF(0, 0, b, s),
                        1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                        new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height));

                t.EndDraw();
                return;
            };
            ManualResetEvent msc_evt = null;
            if (!isAsyn) msc_evt = new ManualResetEvent(false);

            videoCtrl.SouceDisposing += (Loadedsouce,s ) => {
                (Loadedsouce as VideoStreamDecoder).Dispose();
                if (!last_save.IsDisposed) last_save.Dispose();
                last_save = s.LastDraw;

                if (msc_evt != null)
                    msc_evt.Set();
                if (ral_pic != null)
                    ral_pic.Dispose();
            };
            videoCtrl.Disposed += () => { videoCtrl = null; };
            /////////////////安全的部分//////////////////////
            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            else if (time != 0)
            {
                double vara = 1.0, varb = 0.0;
                double increment = 1 / ((double)time * 30);

                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    if (!(Loadedsouce is VideoStreamDecoder video))
                        return DrawProcResult.Ignore;

                    var res = video.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                    if (!res)
                    {
                        if (loop)
                        {
                            video.Position(0);

                            return DrawProcResult.Ignore;
                        }
                        return DrawProcResult.Death;
                    }
                    var BitSrc = new D2DBitmap(view, new Size2(video.FrameSize.Width, video.FrameSize.Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                    BitSrc.CopyFromMemory(dataPoint, pitch);


                    view.BeginDraw();
                    if (vara > 0 && varb < 1)
                    {
                        if (ral_pic != null)
                            view.DrawBitmap(ral_pic,
                          new RawRectangleF(0, 0, Width, Height),
                           (float)vara, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                           new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));

                        view.DrawBitmap(BitSrc,
                      new RawRectangleF(0, 0, Width, Height),
                       (float)varb, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                       new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));
                    }
                    else
                    {
                        view.DrawBitmap(BitSrc,
                      new RawRectangleF(0, 0, Width, Height),
                       1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                       new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));
                    }

                    view.EndDraw();

                    BitSrc.Dispose();

                    vara -= increment;
                    varb += increment;
                    return DrawProcResult.Normal;
                };
            }
            else if (time == 0)
            {

                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    if (!(Loadedsouce is VideoStreamDecoder video))
                        return DrawProcResult.Ignore;

                    var res = video.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                    if (!res)
                    {
                        if (loop)
                        {
                            video.Position(0);
                            return DrawProcResult.Ignore;
                        }
                        return DrawProcResult.Death;
                    }
                    var BitSrc = new D2DBitmap(view, new Size2(video.FrameSize.Width, video.FrameSize.Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                    BitSrc.CopyFromMemory(dataPoint, pitch);
                    view.BeginDraw();

                    view.DrawBitmap(BitSrc,
                  new RawRectangleF(0, 0, Width, Height),
                   1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                   new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));

                    view.EndDraw();

                    BitSrc.Dispose();
                    return DrawProcResult.Normal;
                };
            }
            ////////////////////////////////////////////////

            Background.Dispatcher.Invoke(new Action(() => { videoCtrl.DrawStartup(Background); }));

            if (msc_evt != null)
            {
                msc_evt.WaitOne();
                msc_evt.Dispose();
                if (ral_pic != null)
                    ral_pic.Dispose();
            }
        }//已修复内存泄漏
        /// <summary>
        /// 显示舞台
        /// </summary>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void Show(double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        /// <summary>
        /// 隐藏舞台
        /// </summary>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void Hide(double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }

        public void SuperimposeWithImage(string url, double? time = null, bool isAsyn = false, bool ApplyintoSaver = false, BlendMode? mode = null)
        {
            if (last_save == null || last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {
                ManualResetEvent ficter = new ManualResetEvent(false);
 
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            double vara = 1.0, varb = 0.0;

            double increment = time != 0 ? 1 / ((double)time * 30) : 1.0;

            #endregion

            ManualResetEvent wait_sp = null;
            if (!isAsyn) wait_sp = new ManualResetEvent(false);
            WICBitmap mbp_ss = Stage.LoadBitmap(url);//第一次申请资源
            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2((int)mbp_ss.Size.Width, (int)mbp_ss.Size.Height), 30)
                {
                    Loadedsouce = null
                };
            }));
            D2DBitmap ral_picA = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);
            D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, mbp_ss);

            videoCtrl.FirstDraw += (t, v, b, s) =>
            {
                t.BeginDraw();
                if (mode != null)
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    var m_bn_effect = new Blend(t)
                    {
                        Mode = mode.Value
                    };

                    m_bn_effect.SetInput(0, ral_picA, new RawBool());
                    var buffer_opt = m_op_effect.Output;
                    m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                    buffer_opt.Dispose();
                    t.DrawImage(m_bn_effect);

                    m_op_effect.Dispose();
                    m_bn_effect.Dispose();
                }
                else
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawImage(m_op_effect);

                    m_op_effect.Dispose();
                }
                t.EndDraw();
                if (vara <= 0 || varb >= 1)
                {
                    return;
                }
                vara -= increment;
                varb += increment;
                return;
            };



            videoCtrl.DrawProc += (t, v, b, s) =>
            {
                t.BeginDraw();
                if (mode != null)
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    var m_bn_effect = new Blend(t)
                    {
                        Mode = mode.Value
                    };

                    m_bn_effect.SetInput(0, ral_picA, new RawBool());
                    var buffer_opt = m_op_effect.Output;
                    m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                    buffer_opt.Dispose();
                    t.DrawImage(m_bn_effect);

                    m_op_effect.Dispose();
                    m_bn_effect.Dispose();
                }
                else
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawImage(m_op_effect);

                    m_op_effect.Dispose();
                }
                t.EndDraw();
                if (vara <= 0 || varb >= 1)
                {
                    return DrawProcResult.Death;
                }
                vara -= increment;
                varb += increment;
                return DrawProcResult.Normal;
            };

            videoCtrl.SouceDisposing += (e,s) =>
            {
                if (wait_sp != null)
                    wait_sp.Set();
                ral_picA.Dispose();
                ral_picB.Dispose();
                if (ApplyintoSaver)
                {
                    if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s.LastDraw;
                } 
                mbp_ss.Dispose();
            };
            videoCtrl.Disposed += () => {
                videoCtrl = null;
            };
            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp != null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }//安全でしょう？多分。
        public void SuperimposeWithVideo(string url, double? time = null, bool isAsyn = false, bool ApplyintoSaver = false, BlendMode? mode = null)
        {
            if (last_save == null || last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {

                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            D2DBitmap ral_pic = null;
            var m_sourc = new VideoStreamDecoder(url);

            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2(m_sourc.FrameSize.Width, m_sourc.FrameSize.Height), 30)
                {
                    Loadedsouce = m_sourc
                };
            }));
            ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

            videoCtrl.FirstDraw += (t, m, b, s) =>
            {
                t.BeginDraw();
                t.DrawBitmap(ral_pic,
                   new RawRectangleF(0, 0, b, s),
                    1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                    new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height));


                var res = m_sourc.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                if (!res)
                {
                    throw new Exception();
                }

                var BitSrc = new D2DBitmap(t, new Size2(m_sourc.FrameSize.Width, m_sourc.FrameSize.Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                BitSrc.CopyFromMemory(dataPoint, pitch);

                if (mode != null)
                {

                    var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(t)
                    {
                        Mode = mode.Value
                    };

                    m_bn_effect.SetInput(0, ral_pic, new RawBool());
                    m_bn_effect.SetInput(1, BitSrc, new RawBool());

                    t.DrawImage(m_bn_effect);

                    m_bn_effect.Dispose();
                }
                else
                {
                    t.DrawBitmap(ral_pic, new RawRectangleF(0, 0, b, s), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawBitmap(BitSrc,
                  new RawRectangleF(0, 0, b, s),
                   1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                   new RawRectangleF(0, 0, m_sourc.FrameSize.Width, m_sourc.FrameSize.Height));
                }

                BitSrc.Dispose();
                t.EndDraw();
                return;
            };
            ManualResetEvent msc_evt = null;
            if (!isAsyn) msc_evt = new ManualResetEvent(false);

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            else if (time != 0)
            {
                double varb = 0.0;
                double increment = 1 / ((double)time * 30);
                videoCtrl.SouceDisposing += (Loadedsouce,s ) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (ApplyintoSaver)
                    {
                        if (!last_save.IsDisposed) last_save.Dispose();
                        last_save = s.LastDraw;
                    } 
                    if (msc_evt != null)
                        msc_evt.Set();
                };
                videoCtrl.Disposed += () => {
                    videoCtrl = null;
                };
                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    if (!(Loadedsouce is VideoStreamDecoder video))
                        return DrawProcResult.Ignore;

                    var res = video.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                    if (!res)
                    {
                        return DrawProcResult.Death;
                    }
                    var BitSrc = new D2DBitmap(view, new Size2(video.FrameSize.Width, video.FrameSize.Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                    BitSrc.CopyFromMemory(dataPoint, pitch);


                    view.BeginDraw();
                    if (varb < 1)
                    {

                        if (mode != null)
                        {
                            var m_op_effect = new SharpDX.Direct2D1.Effect(view, SharpDX.Direct2D1.Effect.Opacity);

                            m_op_effect.SetInput(0, BitSrc, new RawBool());
                            m_op_effect.SetValue(0, (float)varb);

                            var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                            {
                                Mode = mode.Value
                            };

                            m_bn_effect.SetInput(0, ral_pic, new RawBool());
                            var buffer_opt = m_op_effect.Output;
                            m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                            buffer_opt.Dispose();
                            view.DrawImage(m_bn_effect);

                            m_op_effect.Dispose();
                            m_bn_effect.Dispose();
                        }
                        else
                        {
                            var m_op_effect = new SharpDX.Direct2D1.Effect(view, SharpDX.Direct2D1.Effect.Opacity);

                            m_op_effect.SetInput(0, BitSrc, new RawBool());
                            m_op_effect.SetValue(0, (float)varb);

                            view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                            view.DrawImage(m_op_effect);

                            m_op_effect.Dispose();
                        }
                    }
                    else
                    {
                        if (mode != null)
                        {

                            var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                            {
                                Mode = mode.Value
                            };

                            m_bn_effect.SetInput(0, ral_pic, new RawBool());
                            m_bn_effect.SetInput(1, BitSrc, new RawBool());

                            view.DrawImage(m_bn_effect);

                            m_bn_effect.Dispose();
                        }
                        else
                        {
                            view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                            view.DrawBitmap(BitSrc,
                          new RawRectangleF(0, 0, Width, Height),
                           1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                           new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));
                        }
                    }

                    view.EndDraw();

                    BitSrc.Dispose();

                    varb += increment;
                    return DrawProcResult.Normal;
                };
            }
            else if (time == 0)
            {
                videoCtrl.SouceDisposing += (Loadedsouce,s ) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (ApplyintoSaver)
                    {
                        if (!last_save.IsDisposed) last_save.Dispose();
                        last_save = s.LastDraw;
                    } 
                    if (msc_evt != null)
                        msc_evt.Set();
                };
                videoCtrl.Disposed += () => {
                    videoCtrl = null;
                };
                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    if (!(Loadedsouce is VideoStreamDecoder video))
                        return DrawProcResult.Ignore;

                    var res = video.TryDecodeNextFrame(out IntPtr dataPoint, out int pitch);
                    if (!res)
                    {
                        return DrawProcResult.Death;
                    }
                    var BitSrc = new D2DBitmap(view, new Size2(video.FrameSize.Width, video.FrameSize.Height), new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));
                    BitSrc.CopyFromMemory(dataPoint, pitch);
                    view.BeginDraw();

                    if (mode != null)
                    {

                        var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                        {
                            Mode = mode.Value
                        };

                        m_bn_effect.SetInput(0, ral_pic, new RawBool());
                        m_bn_effect.SetInput(1, BitSrc, new RawBool());

                        view.DrawImage(m_bn_effect);

                        m_bn_effect.Dispose();
                    }
                    else
                    {
                        view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                        view.DrawBitmap(BitSrc,
                      new RawRectangleF(0, 0, Width, Height),
                       1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                       new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));
                    }
                    view.EndDraw();

                    BitSrc.Dispose();
                    return DrawProcResult.Normal;
                };
            }

            Background.Dispatcher.Invoke(new Action(() => { videoCtrl.DrawStartup(Background); }));

            if (msc_evt != null)
            {
                msc_evt.WaitOne();
                msc_evt.Dispose();
                if (ral_pic != null)
                    ral_pic.Dispose();
            }
        }//安全でしょう？多分。

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
        public void SuperimposeWithVideo_CatchOnLoad(string url, double? time = null, bool isAsyn = false, bool ApplyintoSaver = false, BlendMode? mode = null)
        {

            List<WICBitmap> frames = GetAllFrames(url);
            if (frames.Count == 0)
            {
                throw new Exception();
            }

            if (last_save == null || last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {

                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }

            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2(frames[0].Size.Width, frames[0].Size.Height), 30)
                {
                    Loadedsouce = frames
                };
            }));
            D2DBitmap ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

            int nFrame = 0;

            videoCtrl.FirstDraw += (t, m, b, s) =>
            {
                t.BeginDraw();
                t.DrawBitmap(ral_pic,
                   new RawRectangleF(0, 0, b, s),
                    1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                    new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height));

                if (mode != null)
                {

                    var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(t)
                    {
                        Mode = mode.Value
                    };
                    var av_basic = D2DBitmap.FromWicBitmap(t, frames[nFrame]);
                    m_bn_effect.SetInput(0, ral_pic, new RawBool());
                    m_bn_effect.SetInput(1, av_basic, new RawBool());

                    t.DrawImage(m_bn_effect);
                    av_basic.Dispose();
                    m_bn_effect.Dispose();
                    frames[nFrame].Dispose();
                    nFrame++;
                }
                else
                {
                    t.DrawBitmap(ral_pic, new RawRectangleF(0, 0, b, s), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    var av_basic = D2DBitmap.FromWicBitmap(t, frames[nFrame]);
                    t.DrawBitmap(av_basic,
                  new RawRectangleF(0, 0, b, s),
                   1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                   new RawRectangleF(0, 0, frames[0].Size.Width, frames[0].Size.Height));
                    frames[nFrame].Dispose();
                    av_basic.Dispose();
                    nFrame++;
                }

                t.EndDraw();
                return;
            };
            ManualResetEvent msc_evt = null;
            if (!isAsyn) msc_evt = new ManualResetEvent(false);

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            else if (time != 0)
            {
                double varb = 0.0;
                double increment = 1 / ((double)time * 30);
                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    view.BeginDraw();
                    if (varb < 1)
                    {

                        if (mode != null)
                        {
                            var m_op_effect = new SharpDX.Direct2D1.Effect(view, SharpDX.Direct2D1.Effect.Opacity);
                            var sv_basic = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                            m_op_effect.SetInput(0, sv_basic, new RawBool());
                            m_op_effect.SetValue(0, (float)varb);

                            var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                            {
                                Mode = mode.Value
                            };

                            m_bn_effect.SetInput(0, ral_pic, new RawBool());
                            var buffer_opt = m_op_effect.Output;
                            m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                            buffer_opt.Dispose();
                            view.DrawImage(m_bn_effect);

                            m_op_effect.Dispose();
                            m_bn_effect.Dispose();
                            sv_basic.Dispose();
                            nFrame++;
                        }
                        else
                        {
                            var m_op_effect = new SharpDX.Direct2D1.Effect(view, SharpDX.Direct2D1.Effect.Opacity);
                            var sv_basic = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                            m_op_effect.SetInput(0, sv_basic, new RawBool());
                            m_op_effect.SetValue(0, (float)varb);

                            view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                            view.DrawImage(m_op_effect);

                            sv_basic.Dispose();
                            m_op_effect.Dispose();
                            nFrame++;
                        }
                    }
                    else
                    {
                        if (mode != null)
                        {

                            var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                            {
                                Mode = mode.Value
                            };
                            var av_basic = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                            m_bn_effect.SetInput(0, ral_pic, new RawBool());
                            m_bn_effect.SetInput(1, av_basic, new RawBool());

                            view.DrawImage(m_bn_effect);

                            av_basic.Dispose();
                            nFrame++;
                            m_bn_effect.Dispose();
                        }
                        else
                        {
                            view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                            var av_basic = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                            view.DrawBitmap(av_basic,
                          new RawRectangleF(0, 0, Width, Height),
                           1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                           new RawRectangleF(0, 0, frames[nFrame].Size.Width, frames[nFrame].Size.Height));
                            av_basic.Dispose();
                            nFrame++;
                        }
                    }

                    view.EndDraw();
                    if (nFrame == frames.Count)
                        return DrawProcResult.Death;
                    varb += increment;
                    return DrawProcResult.Normal;
                };
            }
            else if (time == 0)
            {

                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    view.BeginDraw();

                    if (mode != null)
                    {

                        var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                        {
                            Mode = mode.Value
                        };
                        var m_n_frame = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                        m_bn_effect.SetInput(0, ral_pic, new RawBool());
                        m_bn_effect.SetInput(1, m_n_frame, new RawBool());
                        m_n_frame.Dispose();
                        view.DrawImage(m_bn_effect);

                        m_bn_effect.Dispose();
                        nFrame++;
                    }
                    else
                    {
                        view.DrawBitmap(ral_pic, new RawRectangleF(0, 0, Width, Height), 1, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                        var m_n_frame = D2DBitmap.FromWicBitmap(view, frames[nFrame]);
                        view.DrawBitmap(m_n_frame,
                      new RawRectangleF(0, 0, Width, Height),
                       1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                       new RawRectangleF(0, 0, frames[nFrame].Size.Width, frames[nFrame].Size.Height));
                        m_n_frame.Dispose();
                        nFrame++;
                    }
                    view.EndDraw();

                    if (nFrame == frames.Count)
                        return DrawProcResult.Death;
                    return DrawProcResult.Normal;
                };
            }
            videoCtrl.SouceDisposing += (Loadedsouce,s ) => {

                foreach (var i in Loadedsouce as List<WICBitmap>)
                {
                    i.Dispose();
                }

                if (ApplyintoSaver)
                {
                    if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s.LastDraw;
                } 

                if (msc_evt != null)
                    msc_evt.Set();
                if (ral_pic != null && !ral_pic.IsDisposed)
                {
                    ral_pic.Dispose();
                }
            };
            videoCtrl.Disposed += () => {
                videoCtrl = null;
            };
            Background.Dispatcher.Invoke(new Action(() => { videoCtrl.DrawStartup(Background); }));

            if (msc_evt != null)
            {
                msc_evt.WaitOne();
                msc_evt.Dispose();
                if (ral_pic != null)
                    ral_pic.Dispose();
            }
        }
        public void SetLastDraw(string url)
        {
            if (last_save != null)
                if (!last_save.IsDisposed)
                    last_save.Dispose();
            last_save = LoadBitmap(url);
        }
        public enum StageEffect
        {
            Quiver
        }
        public void DoEffect(StageEffect effect)
        {
            switch (effect)
            {
                case StageEffect.Quiver:
                    Effect_Quiver();
                    break;
                default:
                    throw new Exception("Undefine Error");
            }
        }
        private void Effect_Quiver()
        {
            Point[] af_shake_point = new Point[50];
            for (int i = 0; i < 50; i++)
            {
                af_shake_point[i] = GetRandomPoint(10);
            }
            Thickness old_pos = new Thickness();
            Background.Dispatcher.Invoke(new Action(() => { old_pos = Background.Margin; }));

            new Thread(() => {
                foreach (var i in af_shake_point)
                {
                    Background.Dispatcher.Invoke(new Action(() =>
                    {
                        Background.Margin = new Thickness(old_pos.Left + i.X, old_pos.Top + i.Y, 0, 0);
                    }));
                    Thread.Sleep(7);
                }
                Background.Dispatcher.Invoke(new Action(() =>
                {
                    Background.Margin = old_pos;
                }));
            })
            { IsBackground = true }.Start();

        }
        Point GetRandomPoint(int r)
        {
            double x2y(double r, double x)
            {
                return Math.Sqrt((r * r) - (x * x));
            };
            Point result = new Point();
            Random m_rand = new Random();

            result.X = m_rand.Next(-r, r);
            int range_y = (int)x2y(r, result.X);
            result.Y = m_rand.Next(-range_y, range_y);
            return result;
        }
        public void SuperimposeWithImage(WICBitmap visp, double? time = null, bool isAsyn = false, bool ApplyintoSaver = false, BlendMode? mode = null)
        {
            if (last_save == null || last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {
                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            double vara = 1.0, varb = 0.0;

            double increment = time != 0 ? 1 / ((double)time * 30) : 1.0;

            #endregion

            ManualResetEvent wait_sp = null;
            if (!isAsyn) wait_sp = new ManualResetEvent(false);
            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2((int)visp.Size.Width, (int)visp.Size.Height), 30)
                {
                    Loadedsouce = null
                };
            }));
            D2DBitmap ral_picA = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);
            D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, visp);

            videoCtrl.FirstDraw += (t, v, b, s) =>
            {
                t.BeginDraw();
                if (mode != null)
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    var m_bn_effect = new Blend(t)
                    {
                        Mode = mode.Value
                    };

                    m_bn_effect.SetInput(0, ral_picA, new RawBool());
                    var buffer_opt = m_op_effect.Output;
                    m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                    buffer_opt.Dispose();
                    t.DrawImage(m_bn_effect);

                    m_op_effect.Dispose();
                    m_bn_effect.Dispose();
                }
                else
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawImage(m_op_effect);

                    m_op_effect.Dispose();
                }
                t.EndDraw();
                if (vara <= 0 || varb >= 1)
                {
                    return;
                }
                vara -= increment;
                varb += increment;
                return;
            };



            videoCtrl.DrawProc += (t, v, b, s) =>
            {
                t.BeginDraw();
                if (mode != null)
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    var m_bn_effect = new Blend(t)
                    {
                        Mode = mode.Value
                    };

                    m_bn_effect.SetInput(0, ral_picA, new RawBool());
                    var buffer_opt = m_op_effect.Output;
                    m_bn_effect.SetInput(1, buffer_opt, new RawBool());
                    buffer_opt.Dispose();
                    t.DrawImage(m_bn_effect);

                    m_op_effect.Dispose();
                    m_bn_effect.Dispose();
                }
                else
                {
                    var m_op_effect = new SharpDX.Direct2D1.Effect(t, SharpDX.Direct2D1.Effect.Opacity);

                    m_op_effect.SetInput(0, ral_picB, new RawBool());
                    m_op_effect.SetValue(0, (float)varb);

                    t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawImage(m_op_effect);

                    m_op_effect.Dispose();
                }
                t.EndDraw();
                if (vara <= 0 || varb >= 1)
                {
                    return DrawProcResult.Death;
                }
                vara -= increment;
                varb += increment;
                return DrawProcResult.Normal;
            };
            videoCtrl.SouceDisposing += (e ,s) =>
            {
                if (wait_sp != null)
                    wait_sp.Set();
                ral_picA.Dispose();
                ral_picB.Dispose();
                if (ApplyintoSaver)
                {
                    if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s.LastDraw;
                } 
            };
            videoCtrl.Disposed += () => {
                videoCtrl = null;
            };
            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp != null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }//安全でしょう？多分。
        public void SetAsImage(WICBitmap visp, double? time = null, bool isAsyn = false)
        {

            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
            if (videoCtrl != null)
            {
                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += () =>
                {
                    ficter.Set();
                };
                videoCtrl.Dispose();

                ficter.WaitOne();
                ficter.Dispose();
            }
            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            double vara = 1.0, varb = 0.0;

            double increment = time != 0 ? 1 / ((double)time * 30) : 1.0;

            #endregion
            int this_width = visp.Size.Width;
            int this_height = visp.Size.Height;
            ManualResetEvent wait_sp = null;
            if (!isAsyn) wait_sp = new ManualResetEvent(false);
            Background.Dispatcher.Invoke(new Action(() =>
            {
                videoCtrl = new Direct2DImage(new Size2(this_width, this_height), 30)
                {
                    Loadedsouce = null
                };


                D2DBitmap ral_picA = last_save == null ? null : D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, visp);




                videoCtrl.FirstDraw += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    if (ral_picA != null)
                        t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);

                    t.EndDraw();
                    return;
                };

                videoCtrl.DrawProc += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    if (ral_picA != null)
                        t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);
                    t.DrawBitmap(ral_picB, new RawRectangleF(0, 0, b, s), (float)varb, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, this_width, this_height), null);

                    t.EndDraw();
                    if (vara <= 0 || varb >= 1)
                    {
                        return DrawProcResult.Death;
                    }
                    vara -= increment;
                    varb += increment;
                    return DrawProcResult.Normal;
                };
                videoCtrl.SouceDisposing += (e ,s) =>
                {
                    if (ral_picA != null)
                        ral_picA.Dispose();
                    ral_picB.Dispose();
                    if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s.LastDraw;
         
                    if (wait_sp != null)
                        wait_sp.Set();
                };
                videoCtrl.Disposed += () => {
                    videoCtrl = null;
                };
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp != null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }//安全
        private WICBitmap buffer = null;
        public void SetBufferAsUrl(string url = null)
        {
            if (buffer != null) if (!buffer.IsDisposed) buffer.Dispose();
            if (url != null) buffer = LoadBitmap(url);
        }//把舞台缓冲区设置为一个图片,必须当桢调用当桢释放.
        public WICBitmap GetBuffer()
        {
            return buffer;
        }
        public void ApplyEffectToBuffer(ImageEffects effect)
        {
            if (buffer == null) throw new Exception("buffer is empty");

            var m_dev_loader = new Direct2DImage(new Size2(buffer.Size.Width, buffer.Size.Height), 1);/////

            switch (effect)
            {
                case ImageEffects.BW_Effect:

                    var this_pix = D2DBitmap.FromWicBitmap(m_dev_loader.View, buffer);
                    m_dev_loader.Loadedsouce = new SharpDX.Direct2D1.Effects.Saturation(m_dev_loader.View);

                    SharpDX.Direct2D1.Effects.Saturation save_effect_intp = m_dev_loader.Loadedsouce as SharpDX.Direct2D1.Effects.Saturation;
                    save_effect_intp.SetInput(0, this_pix, new RawBool());
                    save_effect_intp.Value = 0;
                    this_pix.Dispose();

                    m_dev_loader.SouceDisposing += (e ,s) =>
                    {
                        (e as SharpDX.Direct2D1.Effects.Saturation)?.GetInput(0).Dispose();
                        (e as SharpDX.Direct2D1.Effects.Saturation)?.Dispose();
                    };
                    break;

                default:
                    throw new Exception("undefined effect");
            }

            m_dev_loader.View.BeginDraw();
            m_dev_loader.View.DrawImage(m_dev_loader.Loadedsouce as Effect);
            m_dev_loader.View.EndDraw();
            m_dev_loader.Commit();

            ManualResetEvent eve = new ManualResetEvent(false);

            m_dev_loader.SouceDisposing += (e,s) => {
                buffer.Dispose();
                buffer = s.LastDraw;
                eve.Set();
            };
            m_dev_loader.Disposed += () => { m_dev_loader = null; };
            m_dev_loader.Dispose();

            eve.WaitOne();
            eve.Dispose();
        }
    }
}
