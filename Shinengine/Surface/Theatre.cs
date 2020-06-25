
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using Shinengine.Media;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using BitmapSource = System.Windows.Media.Imaging.BitmapSource;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using System.IO;

using Bitmap = System.Drawing.Bitmap;
using BmpBitmapEncoder = System.Windows.Media.Imaging.BmpBitmapEncoder;
using BitmapEncoder = System.Windows.Media.Imaging.BitmapEncoder;
using System.Drawing;
using System.Windows.Navigation;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows.Interop;
using BitmapDecoder = SharpDX.WIC.BitmapDecoder;
using Shinengine.Data;

namespace Shinengine.Surface
{
    sealed public class DynamicCharacter : Character
    {
        public DynamicCharacter(string name, string template, Grid layer, bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            :base(name, template, layer, canshow, time, isAscy, vel_x, vel_y)
        {

        }
    }
    sealed public class StaticCharacter : Character
    {
        [DllImport("Shinehelper.dll")]
        extern static public IntPtr GetDskWindow();
        public struct ChangeableAreaInfo{
            public string[] pics;
            public Rect area;
        }

        public struct ChangeableAreaDescription
        {
            public WICBitmap[] switches;
            public Rect area;
        }

        List<ChangeableAreaDescription> ChAreas = null;
        private Direct2DImage dx_switch;

        public StaticCharacter(string name, string init_pic, Grid whereIs, bool canshow = true, ChangeableAreaInfo[] actions_souce = null, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            : base(name, init_pic, whereIs, canshow, time, isAscy, vel_x, vel_y)
        {
            if (actions_souce == null) return;
            ChAreas = new List<ChangeableAreaDescription>();
            foreach (var i in actions_souce)
            {
                ChangeableAreaDescription pct_pos = new ChangeableAreaDescription();//新建一个新的可更改区域描述
                pct_pos.area = i.area;//同步区域矩形

                pct_pos.switches = new WICBitmap[i.pics.Length];//把pct_pos.switches的长度设置为i.pics的长度
                for (int t = 0; t < pct_pos.switches.Length; t++)//遍历pct_pos.switches，设置为对应的素材
                {
                    shower.Dispatcher.Invoke(new Action(() =>
                    {
                        pct_pos.switches[t] = Stage.LoadBitmap(i.pics[t]);
                    }));
                }

                ChAreas.Add(pct_pos);
            } 
        }

        public void SwitchTo(int area, int index, double ?time = null, bool isAysn = false)
        {
            if (time == null) time = SharedSetting.textSpeed;

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            Rect targetArea = ChAreas[area].area;

            WICBitmap rost_pitch = ChAreas[area].switches[index];
            
            shower.Dispatcher.Invoke(()=> {
                dx_switch = new Direct2DImage(new Size2((int)shower.Width, (int)shower.Height), 30)//////////////AAA
                {
                    
                };
            });
            double soul_rate = 0;
            shower.Dispatcher.Invoke(() => { soul_rate = whereIsShowed.Height / Init_action.Size.Height; });
            dx_switch.StartDrawing += (e, v, w, h) =>
             {
                 D2DBitmap m_ipq = D2DBitmap.FromWicBitmap(e, Last_Draw);/////////////////AAA
                 e.BeginDraw();
                 e.Clear(null);

                 e.DrawBitmap(m_ipq,
             new RawRectangleF(0, 0, w, h),
              1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
              new RawRectangleF(0, 0, Last_Draw.Size.Width, Last_Draw.Size.Height));

                 e.EndDraw();

                 m_ipq.Dispose();//////////////////BB
                 return true;
             };
            ManualResetEvent msbn = new ManualResetEvent(false);//////////////////AAA
            if (time == 0)
            {
                dx_switch.DrawProc += (e, v, w, h) =>
                {
                   
                    D2DBitmap m_ipq = D2DBitmap.FromWicBitmap(e, Last_Draw);
                    D2DBitmap m_ipq2 = D2DBitmap.FromWicBitmap(e, rost_pitch);

                    e.BeginDraw();

                    e.DrawBitmap(m_ipq,
                new RawRectangleF(0, 0, w, h),
                 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                 new RawRectangleF(0, 0, Last_Draw.Size.Width, Last_Draw.Size.Height));

                    e.DrawBitmap(m_ipq2,
            new RawRectangleF(
                (float)(targetArea.Left* soul_rate), 
                (float)(targetArea.Top * soul_rate), 
                (float)(targetArea.Right * soul_rate), 
                (float)(targetArea.Bottom * soul_rate)
                ),
             1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
             new RawRectangleF(0, 0, rost_pitch.Size.Width, rost_pitch.Size.Height));

                    e.EndDraw();

                    m_ipq.Dispose();
                    m_ipq2.Dispose();
                    if(!isAysn)  msbn.Set();
                    return DrawProcResult.Death;
                };
            }
            else
            {
                double interrase = 1 / ((double)time * 30);
                double varb = 0;
              

                dx_switch.DrawProc += (e, v, w, h) =>
                {
                    D2DBitmap m_ipq = D2DBitmap.FromWicBitmap(e, Last_Draw);//////////////AAA
                    D2DBitmap m_ipq2 = D2DBitmap.FromWicBitmap(e, rost_pitch);///////////////AAA

                    e.BeginDraw();
                    e.Clear(null);
                    
                    #region 两次绘图
                    e.DrawBitmap(m_ipq,
                new RawRectangleF(0, 0, w, h),
                 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                 new RawRectangleF(0, 0, Last_Draw.Size.Width, Last_Draw.Size.Height));

                    e.DrawBitmap(m_ipq2,
            new RawRectangleF(
                (float)(targetArea.Left * soul_rate),
                (float)(targetArea.Top * soul_rate),
                (float)(targetArea.Right * soul_rate),
                (float)(targetArea.Bottom * soul_rate)
                ),
             (float)varb, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
             new RawRectangleF(0, 0, rost_pitch.Size.Width, rost_pitch.Size.Height));
                    #endregion
                    
                    e.EndDraw();
                    if (varb > 1)
                    {

                        m_ipq.Dispose();
                        m_ipq2.Dispose();

                        if(!isAysn) msbn.Set();
                        return DrawProcResult.Death;//////////////////BB
                    }
                    varb += interrase;

                    m_ipq.Dispose();//////////////////BB
                    m_ipq2.Dispose();//////////////////BB
                    return DrawProcResult.Commit;
                };
            }
           // Last_Draw.Dispose();

            dx_switch.Disposed += (e, v) => {if(Last_Draw!=null)if(!Last_Draw.IsDisposed) Last_Draw.Dispose(); Last_Draw = v; };
            shower.Dispatcher.Invoke(() => { dx_switch.DrawStartup(shower); });
            if (!isAysn) {
                msbn.WaitOne();
                msbn.Dispose();//////////////////BB
            }
            msbn.Dispose();
        }

        public void Dispose()
        {
            foreach(var i in ChAreas)
            {
                foreach (var t in i.switches)
                {
                    t.Dispose();
                }
            }

            this.Remove(0, true);
        }
    }
    public class Character
    {
        AudioPlayer voice_player = null;
        public string _name = "";
        protected WICBitmap Init_action = null;

        protected WICBitmap Last_Draw = null;

        protected Image shower = null;
        protected Grid whereIsShowed = null;

        public Character(string name, string template, Grid layer, bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            _name = name;
            layer.Dispatcher.Invoke(new Action(() =>
            {

                whereIsShowed = layer;

                Init_action = Stage.LoadBitmap(template);

                shower = new Image();

                shower.Width = Init_action.Size.Width * (layer.Height / Init_action.Size.Height);
                shower.Height = layer.Height;
                shower.HorizontalAlignment = HorizontalAlignment.Center;
                shower.VerticalAlignment = VerticalAlignment.Bottom;
                shower.Stretch = Stretch.Fill;

                shower.Margin = new Thickness(vel_x, 0, 0, vel_y);
                if (time != 0 || !canshow)
                {
                    shower.Opacity = 0;
                }
                whereIsShowed.Children.Add(shower);

                Direct2DImage direct2DImage = new Direct2DImage(new Size2((int)shower.Width, (int)shower.Height), 30);
                direct2DImage.DrawProc += (View, Souce, Width, Height) =>
                {
                    D2DBitmap m_bp = D2DBitmap.FromWicBitmap(View, Init_action);

                    View.BeginDraw();
                    View.Clear(null);

                    View.DrawBitmap(m_bp,
             new RawRectangleF(0, 0, Width, Height),
              1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
              new RawRectangleF(0, 0, Init_action.Size.Width, Init_action.Size.Height));

                    View.EndDraw();

                    m_bp.Dispose();
                    return DrawProcResult.Death;
                };

                direct2DImage.Disposed += (e, c) =>
                {
                    Last_Draw = c;
                };

                direct2DImage.DrawStartup(shower);
            }));
            if (!canshow)
                return;


            EasyAmal aml = new EasyAmal(shower, "(Opacity)", 0.0, 1.0, (double)time);
            aml.Start(isAscy);

        }

        public void Say(AirPlant target, string lines, string voice = null)
        {
            if (voice != null)
            {
                if (voice_player != null) voice_player.canplay = false;
                voice_player = new AudioPlayer(voice, false, SharedSetting.VoiceVolum);
            }
            target.Say(lines, this._name);
        }

        public void Remove(double ?time = null, bool isAscy = true)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            ManualResetEvent msbn = new ManualResetEvent(false);
            if (time == 0)
                whereIsShowed.Dispatcher.Invoke(new Action(() => { whereIsShowed.Children.Remove(shower); }));
            else
            {
                if (!isAscy)
                {
                    EasyAmal out_pos = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time);
                    out_pos.Start(false);
                    whereIsShowed.Dispatcher.Invoke(new Action(() => { whereIsShowed.Children.Remove(shower); }));
                }
                else
                {
                    EasyAmal out_pos = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time,(s,v)=> { whereIsShowed.Dispatcher.Invoke(new Action(() => { whereIsShowed.Children.Remove(shower); })); });
                    out_pos.Start(true);
                   
                }
            }
            if(Last_Draw!=null)if(!Last_Draw.IsDisposed)
            Last_Draw.Dispose();
            Init_action.Dispose();
        }

        public void Show(double? time = null,bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        public void Hide(double ?time = null,bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }
    }


    public class EasyAmal
    {
        public Storyboard stbd;
        UIElement uIElement;


        public EasyAmal(UIElement target, string attribute, double from, double to, double nSpeed, EventHandler completed = null)
        {
            uIElement = target;
            target.Dispatcher.Invoke(new Action(() =>
            {
                stbd = new Storyboard();
                DoubleAnimation dbam = new DoubleAnimation();

                dbam.From = from;
                dbam.To = to;
                dbam.Duration = TimeSpan.FromSeconds(nSpeed);
                stbd.FillBehavior = FillBehavior.HoldEnd;
                stbd.Children.Add(dbam);
                Storyboard.SetTarget(stbd, target);
                Storyboard.SetTargetProperty(stbd, new PropertyPath(attribute));

                if (completed != null) stbd.Completed += completed;
            }));

        }

        public void Start(bool isAsyn)
        {
            if (isAsyn)
            {
                // uIElement.Dispatcher.Invoke(new Action(()=> { }));
                uIElement.Dispatcher.Invoke(new Action(() =>
                {
                   
                    stbd.Begin();
                }));

            }
            else
            {
                ManualResetEvent msbn = new ManualResetEvent(false);

                uIElement.Dispatcher.Invoke(new Action(() =>
                {
                    stbd.Completed += (e, v) =>
                    {
                        msbn.Set();
                    };
                    stbd.Begin();
                }));
                msbn.WaitOne();
                msbn.Dispose();
            }
        }
    }
    public class Stage
    {
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }
        public static D2DBitmap ConvertFromSystemBitmap(System.Drawing.Bitmap bmp, RenderTarget renderTarget)
        {
            System.Drawing.Bitmap desBitmap;//预定义要是使用的bitmap
            //如果原始的图像像素格式不是32位带alpha通道
            //需要转换为32位带alpha通道的格式
            //否则无法和Direct2D的格式对应
            if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            {
                desBitmap = new System.Drawing.Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(desBitmap))
                {
                    g.DrawImage(bmp, 0, 0);
                }
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



            BitmapProperties bp;
            PixelFormat pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);

            bp = new BitmapProperties(
                      pixelFormat,
                      desBitmap.HorizontalResolution,
                      desBitmap.VerticalResolution
                    );
            D2DBitmap tempBitmap = new D2DBitmap(renderTarget, new Size2(desBitmap.Width, desBitmap.Height), bp);
            tempBitmap.CopyFromMemory(byteData, bmpData.Stride);

            return tempBitmap;
        }
        /// <summary>
        /// 从一个URL加载图片
        /// </summary>
        /// <param name="init_pic">路径</param>
        /// <returns>一个WIC图片</returns>
        public static WICBitmap LoadBitmap(string init_pic)
        {
            var Imgc = new ImagingFactory();
            var Demcoder = new BitmapDecoder(Imgc, init_pic, SharpDX.IO.NativeFileAccess.Read, DecodeOptions.CacheOnLoad);

            BitmapFrameDecode nm_opb = Demcoder.GetFrame(0);
            var convert = new FormatConverter(Imgc);
            convert.Initialize(nm_opb, SharpDX.WIC.PixelFormat.Format32bppPBGRA);

            var Init_action = new WICBitmap(Imgc, convert, BitmapCreateCacheOption.CacheOnLoad);

            Imgc.Dispose();
            Demcoder.Dispose();
            nm_opb.Dispose();
            convert.Dispose();

            return Init_action;
        }
        public WICBitmap last_save = null;
        public Direct2DImage videoCtrl = null;
        public Image Background { get; private set; } = null;
        public Stage(Image bk)
        {
            Background = bk;
        }

        /// <summary>
        /// 将舞台的背景设置为一个图片，如果是第一次调用，time必须为0
        /// </summary>
        /// <param name="url">图片路径</param>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void setAsImage(string url, double? time = null, bool isAsyn = false)
        {
            #region 时间参数设置
            if (time == null) time = SharedSetting.switchSpeed;
            if (videoCtrl != null)
            {
                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += (e, v) =>
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

                videoCtrl = new Direct2DImage(new Size2((int)Background.Width, (int)Background.Height), 30)
                {
                    Loadedsouce = null
                };
                WICBitmap mbp_ss = Stage.LoadBitmap(url);//第一次申请资源
               
                D2DBitmap ral_picA = last_save == null ? null : D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                 D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View,mbp_ss);

                  


                videoCtrl.StartDrawing += (t, v, b, s) =>
                {
                t.BeginDraw();
                if (ral_picA != null)
                    t.DrawBitmap(ral_picA,
                  new RawRectangleF(0, 0, b, s),
                   1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                   new RawRectangleF(0, 0, mbp_ss.Size.Width, mbp_ss.Size.Height));

                t.EndDraw();
                return true;
                 };

                videoCtrl.DrawProc += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    if (ral_picA != null)
                        t.DrawBitmap(ral_picA,
             new RawRectangleF(0, 0, b, s),
              (float)vara, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
              new RawRectangleF(0, 0, mbp_ss.Size.Width, mbp_ss.Size.Height));
                    t.DrawBitmap(ral_picB,
            new RawRectangleF(0, 0, b, s),
             (float)varb, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
             new RawRectangleF(0, 0, mbp_ss.Size.Width, mbp_ss.Size.Height));

                    t.EndDraw();
                    if (vara < 0 || varb > 1)
                    {
                        return DrawProcResult.Death;
                    }
                    vara -= increment;
                    varb += increment;
                    return DrawProcResult.Commit;
                };
                videoCtrl.Disposed += (e, v) =>
                  {
                      if (wait_sp != null)
                          wait_sp.Set();
                      if (ral_picA != null)
                          ral_picA.Dispose();
                      ral_picB.Dispose();
                      if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                      last_save = v;
                      videoCtrl = null;
                      mbp_ss.Dispose();
                  };
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp!=null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }
        /// <summary>
        /// 将舞台的背景设置为一个视频，如果是第一次调用，time必须为0
        /// </summary>
        /// <param name="url">图片路径</param>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void setAsVideo(string url, double? time = null, bool isAsyn = false, bool loop = true)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            if (videoCtrl != null)
            {

                ManualResetEvent ficter = new ManualResetEvent(false);
                videoCtrl.Disposed += (e, v) => 
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
                videoCtrl = new Direct2DImage(new Size2((int)Background.Width, (int)Background.Height), 30)
                {
                    Loadedsouce = m_sourc
                };
               
                if (last_save != null)
                    ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                videoCtrl.StartDrawing += (t, m, b, s) =>
                {
                    t.BeginDraw();

                    t.DrawBitmap(ral_pic,
             new RawRectangleF(0, 0, b, s),
              1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
              new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height));

                    t.EndDraw();
                    return true;
                };
            }));

            ManualResetEvent msc_evt = null;
            if(!isAsyn) msc_evt = new ManualResetEvent(false);

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            else if (time != 0)
            {
                double vara = 1.0, varb = 0.0;
                double increment = 1 / ((double)time * 30);
                videoCtrl.Disposed += (Loadedsouce,s) => { (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s;
                    videoCtrl = null;

                    if (msc_evt != null)
                        msc_evt.Set();
                };
                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    var video = Loadedsouce as VideoStreamDecoder;

                    if (video == null)
                        return DrawProcResult.Ignore;

                    IntPtr dataPoint;
                    int pitch;
                    var res = video.TryDecodeNextFrame(out dataPoint, out pitch);
                    if (!res)
                    {
                        if (loop)
                        {
                            video.Position(0);

                            return DrawProcResult.Ignore;
                        }
                        return DrawProcResult.Death;
                    }
                    var ImGc = new ImagingFactory();
                    var WICBIT = new WICBitmap(ImGc, video.FrameSize.Width, video.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));
                    var BitSrc = D2DBitmap.FromWicBitmap(view, WICBIT);

                    view.BeginDraw();
                    if (vara > 0 && varb < 1)
                    {
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

                    ImGc.Dispose();
                    WICBIT.Dispose();
                    BitSrc.Dispose();

                    vara -= increment;
                    varb += increment;
                    return DrawProcResult.Commit;
                };
            }
            else if (time == 0)
            {
                videoCtrl.Disposed += (Loadedsouce, s) => { (Loadedsouce as VideoStreamDecoder).Dispose(); 
                    if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s;
                    videoCtrl = null;
                    if (msc_evt != null)
                        msc_evt.Set();
                };
                videoCtrl.DrawProc += (view, Loadedsouce, Width, Height) =>
                {
                    var video = Loadedsouce as VideoStreamDecoder;

                    if (video == null)
                        return DrawProcResult.Ignore;

                    IntPtr dataPoint;
                    int pitch;
                    var res = video.TryDecodeNextFrame(out dataPoint, out pitch);
                    if (!res)
                    {
                        if (loop)
                        {
                            video.Position(0);
                            return DrawProcResult.Ignore;
                        }
                        return DrawProcResult.Death;
                    }
                    var ImGc = new ImagingFactory();
                    var WICBIT = new WICBitmap(ImGc, video.FrameSize.Width, video.FrameSize.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, new DataRectangle(dataPoint, pitch));
                    var BitSrc = D2DBitmap.FromWicBitmap(view, WICBIT);

                    view.BeginDraw();

                    view.DrawBitmap(BitSrc,
                  new RawRectangleF(0, 0, Width, Height),
                   1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                   new RawRectangleF(0, 0, video.FrameSize.Width, video.FrameSize.Height));

                    view.EndDraw();

                    ImGc.Dispose();
                    WICBIT.Dispose();
                    BitSrc.Dispose();
                    return DrawProcResult.Commit;
                };
            }

            Background.Dispatcher.Invoke(new Action(() => { videoCtrl.DrawStartup(Background); }));

            if (msc_evt != null)
            {
                msc_evt.WaitOne();
                msc_evt.Dispose();
                ral_pic.Dispose();
            }
        }
        /// <summary>
        /// 显示舞台
        /// </summary>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void Show(double ?time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        /// <summary>
        /// 隐藏舞台
        /// </summary>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void Hide(double ?time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.switchSpeed;
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 1.0, 0.0,(double) time);
            amsc.Start(isAsyn);
        }
    }
    public class Usage
    {
        public Grid usageArea { get; private set; } = null;
        public Usage(Grid ua)
        {
            usageArea = ua;
        }
        public void Show( double? time = null,bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.switchSpeed / 2.0;
            EasyAmal amsc = new EasyAmal(usageArea, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        public void Hide(double? time = null, bool isAsyn = false)
        {
           if(time==null) time = SharedSetting.switchSpeed/2.0;
            EasyAmal amsc = new EasyAmal(usageArea, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }
    }
    public class AirPlant
    {
        private Grid Vist = null;

        private Grid underView = null;
        private Grid chat_usage = null;

        private TextBlock Lines_Usage = null;
        private TextBlock Character_Usage = null;

        private List<TextBlock> freedomLines = new List<TextBlock>();

        public AirPlant(Grid air, Grid names, TextBlock _Lines, TextBlock _Charecter, Grid _Vist)
        {
            underView = air;
            chat_usage = names;
            Lines_Usage = _Lines;
            Character_Usage = _Charecter;
            Vist = _Vist;

            chat_usage.Opacity = 0;
        }

        public void Say(string line, string character = "", double ?time = null)
        {
            if (time == null) time = SharedSetting.textSpeed;
            EasyAmal esyn = new EasyAmal(Lines_Usage, "(Opacity)", 1.0, 0.0, (double)time);
            esyn.Start(false);
            Lines_Usage.Dispatcher.Invoke(new Action(() =>
            {
                Lines_Usage.Text = line;

                if (character == "" && chat_usage.Opacity == 1)
                {
                    EasyAmal _st = new EasyAmal(chat_usage, "(Opacity)", 1.0, 0.0, (double)time);
                    _st.Start(true);
                    Character_Usage.Text = character;
                }
                if (character != "" && chat_usage.Opacity == 0)
                {
                    Character_Usage.Text = character;
                    EasyAmal _st = new EasyAmal(chat_usage, "(Opacity)", 0.0, 1.0, (double)time);
                    _st.Start(true);
                }
            }));

            esyn = new EasyAmal(Lines_Usage, "(Opacity)", 0.0, 1.0, (double)time);
            esyn.Start(false);


        }
        public void SayAt(string line, RectangleF location, double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.textSpeed;
            EasyAmal m_txt = null;
            Vist.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock n_mfLine = new TextBlock();
                n_mfLine.Text = line;
                n_mfLine.FontSize = Lines_Usage.FontSize;
                n_mfLine.FontFamily = Lines_Usage.FontFamily;
                n_mfLine.FontStyle = Lines_Usage.FontStyle;
                n_mfLine.HorizontalAlignment = HorizontalAlignment.Left;
                n_mfLine.VerticalAlignment = VerticalAlignment.Top;
                n_mfLine.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray);
                n_mfLine.Width = location.Width;
                n_mfLine.Height = location.Height;
                n_mfLine.Margin = new Thickness(location.Left, location.Top, 0, 0);

                freedomLines.Add(n_mfLine);
                if (time == 0)
                {
                    Vist.Children.Add(n_mfLine);
                    return;
                }
                n_mfLine.Opacity = 0;
                Vist.Children.Add(n_mfLine);
                m_txt = new EasyAmal(n_mfLine, "(Opacity)", 0.0, 1.0, (double) time);
            }));


            m_txt.Start(isAsyn);
        }

        public void CleanAllFreedom(double? time = null)
        {
          if(time==null)  time = SharedSetting.textSpeed;
            for (int i = 0; i < freedomLines.Count; i++)
            {
                EasyAmal mns = new EasyAmal(freedomLines[i], "(Opacity)", 1.0, 0.0,(double)time);
                if (i != freedomLines.Count - 1) mns.Start(true); else mns.Start(false);

            }
            for (int i = 0; i < freedomLines.Count; i++)
            {
                Vist.Children.Remove(freedomLines[i]);
            }
            freedomLines.Clear();
        }
    }
    public class Theatre
    {
        public ManualResetEvent call_next = new ManualResetEvent(false);
        public Usage usage { get; private set; }
        public Stage stage { get; private set; }
        public AirPlant airplant { get; private set; }
        public Grid bkSre = null;
        private bool onExit = false;
        public void Exit()
        {
            if (stage.videoCtrl != null)
            {
                var dispoer=new Thread(()=> {
                    ManualResetEvent ficter = new ManualResetEvent(false);
                    stage.videoCtrl.Disposed += (e, v) =>
                    {
                        ficter.Set();
                    };
                    stage.videoCtrl.Dispose();

                    ficter.WaitOne();
                    ficter.Dispose();
                });
                dispoer.IsBackground = true;
                dispoer.Start();
            }
            if (call_next != null)
                call_next.Set();
            call_next = null;
            onExit = true;
        }
        private Grid _charterLayer = null;

        public Grid CharacterLayer { get { return _charterLayer; } }

        public Theatre(Image _BackGround, Grid _UsageArea, Grid rbk, Grid air, Grid names, TextBlock _Lines, TextBlock _Charecter, Grid charterLayer)
        {
            usage = new Usage(_UsageArea);
            stage = new Stage(_BackGround);
            airplant = new AirPlant(air, names, _Lines, _Charecter, rbk);
            bkSre = rbk;
            _charterLayer = charterLayer;
        }
        public void setBackground(Color color)
        {
            bkSre.Dispatcher.Invoke(new Action(() => { bkSre.Background = new System.Windows.Media.SolidColorBrush(color); }));
        }
        public void waitForClick(UIElement Home)
        {
            if (GamingTheatre.isSkiping)
                return;
            if(GamingTheatre.AutoMode)
            {
                Thread.Sleep((int)(SharedSetting.AutoTime * 1000.0));
                return;
            }    
            MouseButtonEventHandler localtion = new MouseButtonEventHandler((e, v) => { call_next.Set(); });
            Home.Dispatcher.Invoke(() => { Home.MouseLeftButtonUp += localtion; });

            call_next.WaitOne();
            if (onExit)
            {
                throw new Exception();
            }
            Home.Dispatcher.Invoke(() => { Home.MouseLeftButtonUp -= localtion; });
            call_next.Reset();
        }

    }
}
