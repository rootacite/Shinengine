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
using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
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
using System.Diagnostics;
using SharpDX.Direct2D1.Effects;
using Device = SharpDX.Direct3D11.Device;

using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using System.Net.WebSockets;
using Blend = SharpDX.Direct2D1.Effects.Blend;
using System.Text;

namespace Shinengine.Surface
{
    sealed public class DynamicCharacter : Character
    {
        public DynamicCharacter(Theatre father, string name, string template , bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            : base(father, name, template, canshow, time, isAscy, vel_x, vel_y)
        {

        }
    }
    /// <summary>
    /// Character 类是抽象类，原则上不允许同时出现两个name相同的角色，这会在SaveLoad时引起bug
    /// </summary>
    sealed public class StaticCharacter : Character
    {
        [DllImport("Shinehelper.dll")]
        extern static public IntPtr GetDskWindow();
        public struct ChangeableAreaInfo
        {
            public string[] pics;
            public Rect area;
        }

        public struct ChangeableAreaDescription
        {
            public WICBitmap[] switches;
            public Rect area;
        }

        readonly List<ChangeableAreaDescription> ChAreas = null;
        private Direct2DImage dx_switch;

        public StaticCharacter(Theatre father,string name, string init_pic, bool canshow = true, ChangeableAreaInfo[] actions_souce = null, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            : base(father,name, init_pic, canshow, time, isAscy, vel_x, vel_y)
        {
            var whereIs = father.CharacterLayer;
            var load_init_array = new int[actions_souce.Length];
            for (int i = 0; i < actions_souce.Length; i++)
            {
                load_init_array[i] = -1;
            }
            m_father.m_des_init.Characters.Add(new Theatre.CharacterDescription()
            {
                areaDis = actions_souce,
                areasType = load_init_array,
                layer = whereIs,
                Showed = canshow,
                template = init_pic,
                name = name,
                vel_x = vel_x,
                vel_y = vel_y
            });

            if (m_father.sandboxMode) return;

            if (actions_souce == null) return;
            ChAreas = new List<ChangeableAreaDescription>();
            foreach (var i in actions_souce)
            {
                ChangeableAreaDescription pct_pos = new ChangeableAreaDescription
                {
                    area = i.area,//同步区域矩形

                    switches = new WICBitmap[i.pics.Length]//把pct_pos.switches的长度设置为i.pics的长度
                };//新建一个新的可更改区域描述
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

        public void SwitchTo(int area, int index, double? time = null, bool isAysn = false)
        {
            foreach (var i in m_father.m_des_init.Characters)
            {
                if (i.name == this._name)
                {
                    i.areasType[area] = index;
                }
            }

            if (m_father.sandboxMode) return;

            if (time == null) time = SharedSetting.TextSpeed;

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            Rect targetArea = ChAreas[area].area;

            WICBitmap rost_pitch = ChAreas[area].switches[index];

            shower.Dispatcher.Invoke(() => {
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
                return;
            };
            ManualResetEvent msbn = new ManualResetEvent(false);//////////////////AAA
            if (time == 0)
            {
                dx_switch.DrawProc += (e, v, w, h) =>
                {

                    D2DBitmap m_ipq = D2DBitmap.FromWicBitmap(e, Init_action);
                    D2DBitmap m_ipq2 = D2DBitmap.FromWicBitmap(e, rost_pitch);

                    e.BeginDraw();

                    e.DrawBitmap(m_ipq,
                new RawRectangleF(0, 0, w, h),
                 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                 new RawRectangleF(0, 0, Init_action.Size.Width, Init_action.Size.Height));

                    e.DrawBitmap(m_ipq2,
            new RawRectangleF(
                (float)(targetArea.Left * soul_rate),
                (float)(targetArea.Top * soul_rate),
                (float)(targetArea.Right * soul_rate),
                (float)(targetArea.Bottom * soul_rate)
                ),
             1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
             new RawRectangleF(0, 0, rost_pitch.Size.Width, rost_pitch.Size.Height));

                    e.EndDraw();

                    m_ipq.Dispose();
                    m_ipq2.Dispose();
                    if (!isAysn) msbn.Set();
                    return DrawProcResult.Death;
                };
                dx_switch.Disposed += (e, v) =>
                {
                    if (Last_Draw != null) if (!Last_Draw.IsDisposed) Last_Draw.Dispose(); 
                    Last_Draw = v;
                };
            }
            else
            {
                double interrase = 1 / ((double)time * 30);
                double varb = 0;

                D2DBitmap m_ipq = D2DBitmap.FromWicBitmap(dx_switch.View, Last_Draw);//////////////AAA
                D2DBitmap m_ipq2 = D2DBitmap.FromWicBitmap(dx_switch.View, rost_pitch);///////////////AAA
                D2DBitmap m_ipq3 = D2DBitmap.FromWicBitmap(dx_switch.View, Init_action);
                dx_switch.DrawProc += (e, v, w, h) =>
                {


                    e.BeginDraw();
                    e.Clear(null);

                    #region 两次绘图
                    e.DrawBitmap(m_ipq,
                new RawRectangleF(0, 0, w, h),
                 1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                 new RawRectangleF(0, 0, Last_Draw.Size.Width, Last_Draw.Size.Height));

                    e.DrawBitmap(m_ipq3,
                new RawRectangleF(0, 0, w, h),
                 (float)varb, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                 new RawRectangleF(0, 0, Init_action.Size.Width, Init_action.Size.Height));

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

                        if (!isAysn) msbn.Set();
                        return DrawProcResult.Death;//////////////////BB
                    }
                    varb += interrase;

                    return DrawProcResult.Normal;
                };

                dx_switch.Disposed += (e, v) =>
                {
                    if (Last_Draw != null) 
                        if (!Last_Draw.IsDisposed) 
                            Last_Draw.Dispose(); 
                    Last_Draw = v;
                    m_ipq.Dispose();//////////////////BB
                    m_ipq2.Dispose();//////////////////BB
                    m_ipq3.Dispose();
                };
            }
            // Last_Draw.Dispose();

            shower.Dispatcher.Invoke(() => { dx_switch.DrawStartup(shower); });
            if (!isAysn)
            {
                msbn.WaitOne();
                msbn.Dispose();//////////////////BB
            }
            msbn.Dispose();
        }

        public void Dispose()
        {
            foreach (var i in m_father.m_des_init.Characters)
            {
                if (i.name == this._name)
                {
                    m_father.m_des_init.Characters.Remove(i);
                    break;
                }
            }

            if (m_father.sandboxMode) return;
            foreach (var i in ChAreas)
            {
                foreach (var t in i.switches)
                {
                    t.Dispose();
                }
            }

            this.Remove(0, true);
        }
    }
    public abstract class Character
    {
        public Theatre m_father = null;
        AudioPlayer voice_player = null;
        public string _name = "";
        protected WICBitmap Init_action = null;

        protected WICBitmap Last_Draw = null;

        protected Image shower = null;
        protected Canvas whereIsShowed = null;

        public Character(Theatre father,string name, string template, bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
        {
            var layer = father.CharacterLayer;
            m_father = father;
            ManualResetEvent msbn = new ManualResetEvent(false);
            _name = name;

            if (m_father.sandboxMode) return;

            if (time == null) time = SharedSetting.SwitchSpeed;
            layer.Dispatcher.Invoke(new Action(() =>
            {

                whereIsShowed = layer;

                Init_action = Stage.LoadBitmap(template);

                shower = new Image
                {
                    Width = Init_action.Size.Width * (layer.Height / Init_action.Size.Height),
                    Height = layer.Height,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Stretch = Stretch.Fill,

                    Margin = new Thickness(vel_x, 0, 0, vel_y)
                };
                if (time != 0 || !canshow)
                {
                    shower.Opacity = 0;
                }
                whereIsShowed.Children.Add(shower);

                Direct2DImage direct2DImage = new Direct2DImage(new Size2((int)shower.Width, (int)shower.Height), 30);
                direct2DImage.DrawProc += (View, Souce, Width, Height) =>
                {
                    D2DBitmap m_bp = D2DBitmap.FromWicBitmap(View, Init_action,new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm,AlphaMode.Premultiplied)));
                    
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
                    msbn.Set();
                };

                direct2DImage.DrawStartup(shower);
            }));
            msbn.WaitOne();
            msbn.Dispose();
            if (!canshow)
                return;


            EasyAmal aml = new EasyAmal(shower, "(Opacity)", 0.0, 1.0, (double)time);
            aml.Start(isAscy);

        }

        public void Say( string lines, string voice = null)
        {
            m_father.m_des_init.Voice = voice;
            if (voice != null && !m_father.sandboxMode)
            {
                if (voice_player != null) voice_player.canplay = false;
                voice_player = new AudioPlayer(voice, false, SharedSetting.VoiceVolum);
            }
            m_father.Airplant.Say(lines, this._name);
        }

        protected void Remove(double? time = null, bool isAscy = true)
        {
            if (time == null) time = SharedSetting.SwitchSpeed;
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
                    EasyAmal out_pos = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time, (s, v) => { whereIsShowed.Dispatcher.Invoke(new Action(() => { whereIsShowed.Children.Remove(shower); })); });
                    out_pos.Start(true);

                }
            }
            if (Last_Draw != null) if (!Last_Draw.IsDisposed)
                    Last_Draw.Dispose();
            Init_action.Dispose();
        }

        public void Show(double? time = null, bool isAsyn = false)
        {
            for (int i = 0; i < m_father.m_des_init.Characters.Count; i++)
            {
                if (m_father.m_des_init.Characters[i].name == this._name)
                {
                    var am = m_father.m_des_init.Characters[i];
                    am.Showed = true;
                    m_father.m_des_init.Characters[i] = am;
                    break;
                }
            }
            if (m_father.sandboxMode) return;
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        public void Hide(double? time = null, bool isAsyn = false)
        {
            for (int i = 0; i < m_father.m_des_init.Characters.Count; i++)
            {
                if (m_father.m_des_init.Characters[i].name == this._name)
                {
                    var am = m_father.m_des_init.Characters[i];
                    am.Showed = false;
                    m_father.m_des_init.Characters[i] = am;
                    break;
                }
            }
            if (m_father.sandboxMode) return;
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }
    }


    public class EasyAmal
    {
        public Storyboard stbd;
        readonly UIElement uIElement;


        public EasyAmal(UIElement target, string attribute, double from, double to, double nSpeed, EventHandler completed = null)
        {
            if (nSpeed == 0)
                nSpeed = 0.001;
            uIElement = target;
            target.Dispatcher.Invoke(new Action(() =>
            {
                stbd = new Storyboard();
                DoubleAnimation dbam = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = TimeSpan.FromSeconds(nSpeed)
                };
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
        /// <summary>
        /// 从一个URL加载图片
        /// </summary>
        /// <param name="init_pic">路径</param>
        /// <returns>一个WIC图片</returns>
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
            File.Delete(rele_path);
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
        /// 将舞台的背景设置为一个图片，如果是第一次调用，time必须为0
        /// </summary>
        /// <param name="url">图片路径</param>
        /// <param name="time">动画时间</param>
        /// <param name="isAsyn">是否异步（true为异步）</param>
        public void SetAsImage(string url, double? time = null, bool isAsyn = false)
        {
            m_father.m_des_init.stageSouceType = true;
            m_father.m_des_init.stageSouce = url;
            if (m_father.sandboxMode) return;
            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
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
                WICBitmap mbp_ss = Stage.LoadBitmap(url);//第一次申请资源
                videoCtrl = new Direct2DImage(new Size2((int)mbp_ss.Size.Width, (int)mbp_ss.Size.Height), 30)
                {
                    Loadedsouce = null
                };


                D2DBitmap ral_picA = last_save == null ? null : D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, mbp_ss);




                videoCtrl.StartDrawing += (t, v, b, s) =>
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

            if (wait_sp != null)
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
        public void SetAsVideo(string url, double? time = null, bool isAsyn = false, bool loop = true)
        {
            m_father.m_des_init.stageSouceType = false;
            m_father.m_des_init.stageSouce = url;
            m_father.m_des_init.isLoop = loop;
            if (m_father.sandboxMode) return;
            if (time == null) time = SharedSetting.SwitchSpeed;
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
                videoCtrl = new Direct2DImage(new Size2(m_sourc.FrameSize.Width, m_sourc.FrameSize.Height), 30)
                {
                    Loadedsouce = m_sourc
                };

                if (last_save != null)
                    ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                videoCtrl.StartDrawing += (t, m, b, s) =>
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
            }));

            ManualResetEvent msc_evt = null;
            if (!isAsyn) msc_evt = new ManualResetEvent(false);

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            else if (time != 0)
            {
                double vara = 1.0, varb = 0.0;
                double increment = 1 / ((double)time * 30);
                videoCtrl.Disposed += (Loadedsouce, s) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s;
                    videoCtrl = null;

                    if (msc_evt != null)
                        msc_evt.Set();
                };
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
                videoCtrl.Disposed += (Loadedsouce, s) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                    last_save = s;
                    videoCtrl = null;
                    if (msc_evt != null)
                        msc_evt.Set();
                };
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

            Background.Dispatcher.Invoke(new Action(() => { videoCtrl.DrawStartup(Background); }));

            if (msc_evt != null)
            {
                msc_evt.WaitOne();
                msc_evt.Dispose();
                if (ral_pic != null)
                    ral_pic.Dispose();
            }
        }
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
            if (m_father.sandboxMode) return;
            if(last_save==null|| last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            #region 时间参数设置
            if (time == null) time = SharedSetting.SwitchSpeed;
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
                WICBitmap mbp_ss = Stage.LoadBitmap(url);//第一次申请资源
                videoCtrl = new Direct2DImage(new Size2((int)mbp_ss.Size.Width, (int)mbp_ss.Size.Height), 30)
                {
                    Loadedsouce = null
                };

                D2DBitmap ral_picA = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);
                D2DBitmap ral_picB = D2DBitmap.FromWicBitmap(videoCtrl.View, mbp_ss);

                videoCtrl.StartDrawing += (t, v, b, s) =>
                {
                    t.BeginDraw();
                    t.DrawBitmap(ral_picA, new RawRectangleF(0, 0, b, s), (float)vara, SharpDX.Direct2D1.InterpolationMode.Anisotropic, new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height), null);

                    t.EndDraw();
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
                        m_bn_effect.SetInput(1, m_op_effect.Output, new RawBool());

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
                videoCtrl.Disposed += (e, v) =>
                {
                    if (wait_sp != null)
                        wait_sp.Set();
                    ral_picA.Dispose();
                    ral_picB.Dispose();
                    if (ApplyintoSaver)
                    {
                        if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                        last_save = v;
                    }else
                    {
                        v.Dispose();
                    }
                    videoCtrl = null;
                    mbp_ss.Dispose();
                };
                videoCtrl.DrawStartup(Background);
            }));

            if (wait_sp != null)
            {
                wait_sp.WaitOne();
                wait_sp.Dispose();
                wait_sp = null;
            }
        }

        public void SuperimposeWithVideo(string url, double? time = null, bool isAsyn = false, bool ApplyintoSaver = false, BlendMode ?mode = null)
        {
           
            if (m_father.sandboxMode) return;
            if (last_save == null || last_save.IsDisposed)
            {
                throw new Exception("this api can only be called when you have called setAsImage Or setAsVideo with 0 time");
            }
            if (time == null) time = SharedSetting.SwitchSpeed;
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
                videoCtrl = new Direct2DImage(new Size2(m_sourc.FrameSize.Width, m_sourc.FrameSize.Height), 30)
                {
                    Loadedsouce = m_sourc
                };

                ral_pic = D2DBitmap.FromWicBitmap(videoCtrl.View, last_save);

                videoCtrl.StartDrawing += (t, m, b, s) =>
                {
                    t.BeginDraw();
                    t.DrawBitmap(ral_pic,
                       new RawRectangleF(0, 0, b, s),
                        1, SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
                        new RawRectangleF(0, 0, last_save.Size.Width, last_save.Size.Height));

                    t.EndDraw();
                    return;
                };
            }));

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
                videoCtrl.Disposed += (Loadedsouce, s) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (ApplyintoSaver)
                    {
                        if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                        last_save = s;
                    }
                    else
                    {
                        s.Dispose();
                    }
                    videoCtrl = null;

                    if (msc_evt != null)
                        msc_evt.Set();
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
                    if ( varb < 1)
                    {
         
                        if (mode != null) {
                            var m_op_effect = new SharpDX.Direct2D1.Effect(view, SharpDX.Direct2D1.Effect.Opacity);

                            m_op_effect.SetInput(0, BitSrc, new RawBool());
                            m_op_effect.SetValue(0, (float)varb);

                            var m_bn_effect = new SharpDX.Direct2D1.Effects.Blend(view)
                            {
                                Mode = mode.Value
                            };

                            m_bn_effect.SetInput(0, ral_pic, new RawBool());
                            m_bn_effect.SetInput(1, m_op_effect.Output, new RawBool());

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
                videoCtrl.Disposed += (Loadedsouce, s) => {
                    (Loadedsouce as VideoStreamDecoder).Dispose();
                    if (ApplyintoSaver)
                    {
                        if (last_save != null) if (!last_save.IsDisposed) last_save.Dispose();
                        last_save = s;
                    }
                    else
                    {
                        s.Dispose();
                    }
                    videoCtrl = null;
                    if (msc_evt != null)
                        msc_evt.Set();
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
        }
    }
    public class Usage
    {
        public static bool locked = false;
        public Grid UsageArea { get; private set; } = null;
        public Usage(Grid ua)
        {
            UsageArea = ua;
        }
        public void Show(double? time = null, bool isAsyn = false)
        {
            if (locked) return;
            if (time == null) time = SharedSetting.SwitchSpeed / 2.0;
            EasyAmal amsc = new EasyAmal(UsageArea, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }
        public void Hide(double? time = null, bool isAsyn = false)
        {
            if (locked) return;
            if (time == null) time = SharedSetting.SwitchSpeed / 2.0;
            EasyAmal amsc = new EasyAmal(UsageArea, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }
    }
    public class AirPlant
    {
        public Theatre m_father = null;
        private readonly Grid Vist = null;

        private readonly Grid chat_usage = null;

        readonly private TextBlock Lines_Usage = null;
        readonly private TextBlock Character_Usage = null;
        readonly private TextBlock _Contents = null;

        readonly private List<TextBlock> freedomLines = new List<TextBlock>();

        public AirPlant(Grid air, Grid names, TextBlock _Lines, TextBlock _Charecter, Grid _Vist, TextBlock Content)
        {
            chat_usage = names;
            Lines_Usage = _Lines;
            Character_Usage = _Charecter;
            Vist = _Vist;
            _Contents = Content;
            chat_usage.Opacity = 0;
        }

        public void Say(string line, string character = "", double? time = null)
        {
            m_father.m_des_init.line = line;
            m_father.m_des_init.name = character;
            if (m_father.sandboxMode) return;
            if (time == null) time = SharedSetting.TextSpeed;

            #region log call
            string load_printed = "";

            string ral_printf_str = "";
            if (character != "")
            {
                ral_printf_str += "[" + character + "] ";
            }
            ral_printf_str += line;

            foreach (var c in ral_printf_str)
            {

                Vist.Dispatcher.Invoke(new Action(() =>
                {
                    load_printed += c;
                    var ap_l = GamingBook.MeasureTextWidth(_Contents, _Contents.FontSize, load_printed);
                    if (ap_l.Width > _Contents.Width)
                    {
                        _Contents.Dispatcher.Invoke(new Action(() => { GamingTheatre.Preparation += '\n'; }));
                        load_printed = "";
                    }
                    GamingTheatre.Preparation += c;
                }));
            }
            GamingTheatre.Preparation += "\n";
            #endregion


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
                else
                if (character != "" && chat_usage.Opacity == 0)
                {
                    Character_Usage.Text = character;
                    EasyAmal _st = new EasyAmal(chat_usage, "(Opacity)", 0.0, 1.0, (double)time);
                    _st.Start(true);
                }
                else
                {
                    Character_Usage.Text = character;
                }
            }));
            esyn = new EasyAmal(Lines_Usage, "(Opacity)", 0.0, 1.0, (double)time);
            esyn.Start(false);


        }
        public void SayAt(string line, RectangleF location, double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.TextSpeed;
            EasyAmal m_txt = null;
            Vist.Dispatcher.Invoke(new Action(() =>
            {
                TextBlock n_mfLine = new TextBlock
                {
                    Text = line,
                    FontSize = Lines_Usage.FontSize,
                    FontFamily = Lines_Usage.FontFamily,
                    FontStyle = Lines_Usage.FontStyle,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
                    Width = location.Width,
                    Height = location.Height,
                    Margin = new Thickness(location.Left, location.Top, 0, 0)
                };

                freedomLines.Add(n_mfLine);
                if (time == 0)
                {
                    Vist.Children.Add(n_mfLine);
                    return;
                }
                n_mfLine.Opacity = 0;
                Vist.Children.Add(n_mfLine);
                m_txt = new EasyAmal(n_mfLine, "(Opacity)", 0.0, 1.0, (double)time);
            }));


            m_txt.Start(isAsyn);
        }

        public void CleanAllFreedom(double? time = null)
        {
            if (time == null) time = SharedSetting.TextSpeed;
            for (int i = 0; i < freedomLines.Count; i++)
            {
                EasyAmal mns = new EasyAmal(freedomLines[i], "(Opacity)", 1.0, 0.0, (double)time);
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
        public void SetEnvironmentMusic(string path = null)
        {
            if (path != null)
            {
                m_des_init.Environment = path;
            }
            if (sandboxMode)
                return;
            if (m_em_player != null)
            {
                m_em_player.canplay = false;
                m_em_player = null;
            }
            if (path != null)
            {
                m_em_player = new AudioPlayer(path, true, SharedSetting.EmVolum);
            }
        }
        public void SetBackgroundMusic(string path = null)
        {
            if (path != null)
            {
                m_des_init.BGM = path;
            }
            if (sandboxMode)
                return;
            if (m_player != null)
            {
                m_player.canplay = false;
                m_player = null;
            }
            if (path != null)
            {
                m_player = new AudioPlayer(path, true, SharedSetting.BGMVolum);
            }
        }

        public void SetLoppedEvireSound(string path = null)
        {
            if (path != null)
            {
                m_des_init.SE = path;
            }
            if (sandboxMode)
                return;
            if (m_se_player != null)
            {
                m_se_player.canplay = false;
                m_se_player = null;
            }
            if (path != null)
            {
                m_se_player = new AudioPlayer(path, true, SharedSetting.EmVolum);
            }
        }
        public List<StaticCharacter> cts = new List<StaticCharacter>();
        public struct CharacterDescription
        {
            public string name;
            public string template;
            public Canvas layer;
            public double vel_x;
            public double vel_y;

            public bool Showed;
            public int[] areasType;
            public StaticCharacter.ChangeableAreaInfo[] areaDis;
        }

        public struct FrameDescription
        {
            public string Voice;
            public string SE;
            public   string Environment;
            public   string BGM;
            public   string name;
            public   string line;

            public   bool stageSouceType;
            public   string stageSouce;

            public   bool isLoop;

            public List<CharacterDescription> Characters;
        }
        public FrameDescription m_des_init = new FrameDescription() { Characters = new List<CharacterDescription>() };

        #region rest
        public  bool sandboxMode = false;
        public int saved_frame = 0;
        public AudioPlayer m_player = null;
        public AudioPlayer m_em_player = null;
        public AudioPlayer m_se_player = null;


        public ManualResetEvent call_next = new ManualResetEvent(false);
        public Usage Usage { get; private set; }
        public Stage Stage { get; private set; }
        public AirPlant Airplant { get; private set; }
        public Grid bkSre = null;
        private bool onExit = false;
        public void Exit()
        {
            if (Stage.videoCtrl != null)
            {
                var dispoer = new Thread(() =>
                {
                    ManualResetEvent ficter = new ManualResetEvent(false);
                    Stage.videoCtrl.Disposed += (e, v) =>
                    {
                        ficter.Set();
                    };
                    Stage.videoCtrl.Dispose();

                    ficter.WaitOne();
                    ficter.Dispose();
                })
                {
                    IsBackground = true
                };
                dispoer.Start();
            }
            if (call_next != null)
                call_next.Set();
            call_next = null;
            onExit = true;
        }

        public Canvas CharacterLayer { get; } = null;

        public Theatre(Image _BackGround, Grid _UsageArea, Grid rbk, Grid air, Grid names, TextBlock _Lines, TextBlock _Charecter, Canvas charterLayer, TextBlock backlog)
        { 
            Usage = new Usage(_UsageArea);

            Stage = new Stage(_BackGround)
            {
                m_father = this
            };
            Airplant = new AirPlant(air, names, _Lines, _Charecter, rbk, backlog)
            {
                m_father = this
            };
            bkSre = rbk;
            CharacterLayer = charterLayer;
        }
        public void SetBackground(Color color)
        {
            bkSre.Dispatcher.Invoke(new Action(() => { bkSre.Background = new System.Windows.Media.SolidColorBrush(color); }));
        }
        #endregion
        public void WaitForClick(UIElement Home = null)
        {
            if (sandboxMode)
            {
                if (saved_frame < locatPlace)
                    return;
                sandboxMode = false;

                if (m_des_init.stageSouceType)
                    Stage.SetAsImage(m_des_init.stageSouce, null, false);
                else
                    Stage.SetAsVideo(m_des_init.stageSouce, null, true, m_des_init.isLoop);

                Airplant.Say(m_des_init.line, m_des_init.name);
                SetBackgroundMusic(m_des_init.BGM);
                SetEnvironmentMusic(m_des_init.Environment);

                for (int i = 0; i < m_des_init.Characters.Count; i++)
                {
                    var mp_ol = m_des_init.Characters[i];
                    cts[i] = new StaticCharacter(this, mp_ol.name, mp_ol.template, mp_ol.Showed, mp_ol.areaDis, null, true, mp_ol.vel_x, mp_ol.vel_y);
                    m_des_init.Characters.RemoveAt(m_des_init.Characters.Count - 1);
                    int iit = 0;
                    foreach (var t in mp_ol.areasType)
                    {
                        if (t != -1)
                            cts[i].SwitchTo(iit, t);
                        iit++;
                    }
                }
            }
            if (Home == null)
            {
                Home = bkSre;
            }
            if (GamingTheatre.isSkiping)
                return;
            if (GamingTheatre.AutoMode)
            {
                Thread.Sleep((int)(SharedSetting.AutoTime * 1000.0));
                return;
            }
            MouseButtonEventHandler localtion = new MouseButtonEventHandler((e, v) => { if (!Usage.locked) call_next.Set(); });
            MouseWheelEventHandler location2 = new MouseWheelEventHandler((e, v) => { if (!Usage.locked && v.Delta < 0 && !v.Handled) call_next.Set(); });

            Home.Dispatcher.Invoke(() => { Home.MouseLeftButtonUp += localtion; Home.MouseWheel += location2; });

            call_next.WaitOne();
            if (onExit)
            {
                throw new Exception("Exitted");
            }
            Home.Dispatcher.Invoke(() => { Home.MouseLeftButtonUp -= localtion; Home.MouseWheel -= location2; });
            call_next.Reset();
        }
        private int locatPlace = 0;
        public void SetNextLocatPosition(int place)
        {
            if (place == 0)
                return;
            sandboxMode = true;
            locatPlace = place;
        }
    }
}