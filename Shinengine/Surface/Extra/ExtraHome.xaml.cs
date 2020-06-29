using Shinengine.Data;
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
        public ExtraHome()
        {
            InitializeComponent();
        }

        private void Event_HS1(object sender, RoutedEventArgs e)
        {
            
            EasyAmal _mpos2 = new EasyAmal(this.ExtraGrid, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
            {
                MainWindow.theatreMode = MainWindow.SwitchToSignalTheatre(100, 0, ()=>
                {
                    var load_the = MainWindow.theatreMode.SBK;
                    var m_thread_intp = new Thread(() =>
                    {
                        EasyAmal mpos = new EasyAmal(load_the, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed);
                        mpos.Start(false);/// hide tview
                        load_the.Dispatcher.Invoke(new Action(() =>
                        {

                            ExtraHome msv = new ExtraHome();

                            MainWindow.m_window.Content = msv.Content;
                            MainWindow.extraPage = msv;
                            MainWindow.title = null;
                            msv.ExtraGrid.MouseRightButtonUp += (e, v) =>
                            {
                                MainWindow.title = MainWindow.SwitchToTitle();
                                MainWindow.m_window.Content = MainWindow.title.Content;
                                MainWindow.extraPage = null;
                            };

                            msv.exitlpg.Click += (e, v) =>
                            {
                                MainWindow.title = MainWindow.SwitchToTitle();
                                MainWindow.m_window.Content = MainWindow.title.Content;
                                MainWindow.extraPage = null;
                            };
                        }));
                    })
                    {
                        IsBackground = true
                    };
                    m_thread_intp.Start();
                    MainWindow.theatreMode.m_logo.Dispose();
                    MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                    MainWindow.theatreMode.m_theatre.Exit();

                    if (MainWindow.theatreMode != null)
                        MainWindow.theatreMode = null;
                });
              
                MainWindow.extraPage = null;
            });
            _mpos2.Start(true);
        }
    }
}
