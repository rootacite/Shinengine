using SharpDX;
using SharpDX.Direct2D1;
using Shinengine.Media;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;

using WICBitmap = SharpDX.WIC.Bitmap;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Shinengine.Data;
using System.Security.Authentication.ExtendedProtection;

namespace Shinengine.Theatre
{

    sealed public class DynamicCharacter : Character, IDisposable
    {
        public DynamicCharacter(Theatre father, string name, string template, bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            : base(father, name, template, canshow, time, isAscy, vel_x, vel_y)
        {

        }

        public bool Disposed { get; private set; } = false;
        ~DynamicCharacter()
        {
            this.Dispose();
        }
        public new void Dispose()
        {
            if (Disposed) return;



            base.Remove();
            base.Dispose();
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// Character 类是抽象类，原则上不允许同时出现两个name相同的角色，这会在SaveLoad时引起bug
    /// </summary>
    sealed public class StaticCharacter : Character, IDisposable
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

        public StaticCharacter(Theatre father, string name, string init_pic, bool canshow = true, ChangeableAreaInfo[] actions_souce = null, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
            : base(father, name, init_pic, canshow, time, isAscy, vel_x, vel_y)
        {
            var whereIs = father.CharacterLayer;

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
        }//已经确认过安全的代码,再次修改需要小心

        public void SwitchTo(int area, int index, double? time = null, bool isAysn = false)
        {
            if (time == null) time = SharedSetting.TextSpeed;

            if (time < 1.0 / 30.0 && time != 0)
            {
                throw new Exception("time can not be less than 1/30s");
            }
            Rect targetArea = ChAreas[area].area;

            WICBitmap rost_pitch = ChAreas[area].switches[index];

            shower.Dispatcher.Invoke(() =>
            {
                dx_switch = new Direct2DImage(new Size2((int)shower.Width, (int)shower.Height), 30)//////////////AAA
                {

                };
            });
            double soul_rate = 0;
            shower.Dispatcher.Invoke(() => { soul_rate = whereIsShowed.Height / Init_action.Size.Height; });
            dx_switch.FirstDraw += (e, v, w, h) =>
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
                dx_switch.Disposing += (e, s) =>
                {
                    if (Last_Draw != null) if (!Last_Draw.IsDisposed) Last_Draw.Dispose();
                    Last_Draw = s.LastDraw;
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

                dx_switch.Disposing += (e, s) =>
                {
                    if (Last_Draw != null)
                        if (!Last_Draw.IsDisposed)
                            Last_Draw.Dispose();
                    Last_Draw = s.LastDraw;
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
        }//已经确认过安全的代码,再次修改需要小心
        ~StaticCharacter()
        {
            Dispose();
        }
        public bool Disposed { get; private set; } = false;
        public new void Dispose()
        {
            if (Disposed) return;
            foreach (var i in ChAreas)
            {
                foreach (var t in i.switches)
                {
                    t.Dispose();
                }
            }
            for (int i = 0; i < m_father.cts.Count; i++)
            {
                if (m_father.cts[i] == this)
                {
                    m_father.cts.RemoveAt(i);
                    break;
                }
            }
            base.Remove();
            base.Dispose();
            Disposed = true;
            GC.SuppressFinalize(this);
        }//已经确认可以正确释放资源
    }
    public abstract class Character : IDisposable
    {
        public Theatre m_father = null;
        AudioPlayer voice_player = null;
        public string _name = "";
        protected WICBitmap Init_action = null;

        protected WICBitmap Last_Draw = null;

        protected Image shower = null;
        protected Canvas whereIsShowed = null;

        public Character(Theatre father, string name, string template, bool canshow = true, double? time = null, bool isAscy = true, double vel_x = 0, double vel_y = 0)
        {
            var layer = father.CharacterLayer;
            m_father = father;
            ManualResetEvent msbn = new ManualResetEvent(false);
            _name = name;

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
                    D2DBitmap m_bp = D2DBitmap.FromWicBitmap(View, Init_action, new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)));

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

                direct2DImage.Disposing += (e, s) =>
                {
                    Last_Draw = s.LastDraw;
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

        }//已经确认过安全的代码,再次修改需要小心

        public void Say(string lines, string voice = null)
        {
            m_father.Voice = voice;
            if (voice != null)
            {
                if (voice_player != null) voice_player.canplay = false;
                voice_player = new AudioPlayer(voice, false, SharedSetting.VoiceVolum);
            }
            m_father.Airplant.Say(lines, this._name);
        }//安全的代码

        public bool IsDisposed { get; private set; } = false;
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            if (disposing)
            {
                shower = null;
                whereIsShowed = null;
            }
            if (Last_Draw != null) if (!Last_Draw.IsDisposed)
                    Last_Draw.Dispose();
            Init_action.Dispose();
            IsDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~Character()
        {
            Dispose(false);
        }
        protected void Remove()
        {
            whereIsShowed.Dispatcher.Invoke(new Action(() => { whereIsShowed.Children.Remove(shower); }));

        }//已经确认过安全的代码,再次修改需要小心
        public void Show(double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 0.0, 1.0, (double)time);
            amsc.Start(isAsyn);
        }//安全的代码
        public void Hide(double? time = null, bool isAsyn = false)
        {
            if (time == null) time = SharedSetting.SwitchSpeed;
            EasyAmal amsc = new EasyAmal(shower, "(Opacity)", 1.0, 0.0, (double)time);
            amsc.Start(isAsyn);
        }//安全的代码
    }
}
