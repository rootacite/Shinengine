
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace Shinengine.Surface
{
    public class EasyAmal
    {
        private Storyboard stbd;
        UIElement uIElement;

        public EasyAmal(UIElement target, string attribute, double from, double to, double nSpeed)
        {
            uIElement = target;
            target.Dispatcher.Invoke(new Action(()=> {
                stbd = new Storyboard();
                DoubleAnimation dbam = new DoubleAnimation();

                dbam.From = from;
                dbam.To = to;
                dbam.Duration = TimeSpan.FromSeconds(nSpeed);
                stbd.FillBehavior = FillBehavior.HoldEnd;
                stbd.Children.Add(dbam);
                Storyboard.SetTarget(stbd, target);
                Storyboard.SetTargetProperty(stbd, new PropertyPath(attribute));
            }));
           
        }

        public void Start(bool isAsyn)
        {
            if(isAsyn)
            {
                // uIElement.Dispatcher.Invoke(new Action(()=> { }));
                uIElement.Dispatcher.Invoke(new Action(() => { stbd.Begin(); }));
               
            }
            else
            {
                ManualResetEvent msbn = new ManualResetEvent(false);
               
                uIElement.Dispatcher.Invoke(new Action(() => { stbd.Completed += (e, v) => { msbn.Set(); }; stbd.Begin(); }));
                msbn.WaitOne();
            }
        }
    }
    public class Stage
    {
        public Image Background { get; private set; } = null;
        public Stage(Image bk)
        {
            Background = bk;
        }

        public void setASImage(string path)
        {
            var arep = new Uri("pack://siteoforigin:,,," + (path[0] == '/' ? "" : "/") + path);
            Background.Dispatcher.Invoke(() => { Background.Source = new BitmapImage(arep); });
        }
        public void Show(double time, bool isAsyn)
        {
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 0.0, 1.0, time);
            amsc.Start(isAsyn);
        }
        public void Hide(double time, bool isAsyn)
        {
            EasyAmal amsc = new EasyAmal(Background, "(Opacity)", 1.0, 0.0, time);
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
        public void Show(double time, bool isAsyn)
        {
            EasyAmal amsc = new EasyAmal(usageArea, "(Opacity)", 0.0, 1.0, time);
            amsc.Start(isAsyn);
        }
        public void Hide(double time, bool isAsyn)
        {
            EasyAmal amsc = new EasyAmal(usageArea, "(Opacity)", 1.0, 0.0, time);
            amsc.Start(isAsyn);
        }
    }
    public class Theatre
    {
        ManualResetEvent call_next = new ManualResetEvent(false);
        public Usage usage { get; private set; }
        public Stage stage { get; private set; }
        private Grid bkSre = null;

        public Theatre(Image bk, Grid ua, Grid rbk)
        {
            usage = new Usage(ua);
            stage = new Stage(bk);

            bkSre = rbk;
        }
        public void setBackground(Color color)
        {
            bkSre.Dispatcher.Invoke(new Action(() => { bkSre.Background = new  SolidColorBrush(color); }));
        }
        public void waitForClick(Window tart)
        {

            MouseButtonEventHandler localtion = new MouseButtonEventHandler((e, v) => { call_next.Set(); });
            tart.Dispatcher.Invoke(() => { tart.MouseUp += localtion; });

            call_next.WaitOne();

            tart.Dispatcher.Invoke(() => { tart.MouseUp -= localtion; });
            call_next.Reset();
        }

    }
}
