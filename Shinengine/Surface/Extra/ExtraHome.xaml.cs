using SharpDX;
using Shinengine.Data;
using Shinengine.Theatre;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shinengine.Surface.Extra
{
    /// <summary>
    /// ExtraHome.xaml 的交互逻辑
    /// </summary>
    public partial class ExtraHome : Page
    {
        Page ref_mode = null;
        public ExtraHome()
        {
            InitializeComponent();

            
        }
        private void Event_HS1(object sender, RoutedEventArgs e)
        {

            MainWindow.TheatreMode = MainWindow.SwitchToSignalTheatre(100, 0, () =>
            {
                var load_the = MainWindow.TheatreMode.SBK;
                var m_thread_intp = new Thread(() =>
                {
                    EasyAmal mpos = new EasyAmal(load_the, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed);
                    mpos.Start(false);/// hide tview
                    load_the.Dispatcher.Invoke(new Action(() =>
                    {

                        ExtraHome msv = new ExtraHome();

                        MainWindow.sys_pite.Content = msv.Content;
                        MainWindow.ExtraPage = msv;
                        MainWindow.Title = null;
                        msv.ExtraGrid.MouseRightButtonUp += (e, v) =>
                        {
                            MainWindow.Title = MainWindow.SwitchToTitle();
                            MainWindow.sys_pite.Content = MainWindow.Title.Content;
                            MainWindow.ExtraPage = null;
                        };

                        msv.exitlpg.Click += (e, v) =>
                        {
                            MainWindow.Title = MainWindow.SwitchToTitle();
                            MainWindow.sys_pite.Content = MainWindow.Title.Content;
                            MainWindow.ExtraPage = null;
                        };
                    }));
                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();
                MainWindow.TheatreMode.Dispose();

                if (MainWindow.TheatreMode != null)
                    MainWindow.TheatreMode = null;
            });

            MainWindow.ExtraPage = null;

        }

    }
}
