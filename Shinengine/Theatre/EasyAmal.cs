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
using System.Drawing;
using System.Runtime.InteropServices;
using BitmapDecoder = SharpDX.WIC.BitmapDecoder;
using Shinengine.Data;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using Blend = SharpDX.Direct2D1.Effects.Blend;
using Point = System.Drawing.Point;
using System.Security.Cryptography.Xml;
using System.Linq.Expressions;
using Shinengine.Surface;

namespace Shinengine.Theatre
{
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
    }//这个类没有使用过非托管资源
}