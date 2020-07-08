using Shinengine.Media;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

using WICBitmap = SharpDX.WIC.Bitmap;
using System.Collections.Generic;
using Shinengine.Data;
using Shinengine.Surface;

namespace Shinengine.Theatre
{
  
    sealed public class Theatre : IDisposable
    {
        public void SetEnvironmentMusic(string path = null)
        { 
            if (m_em_player != null)
            {
                m_em_player.canplay = false;
                m_em_player = null;
            }
            if (path != null)
            {
                m_em_player = new AudioPlayer(path, true, SharedSetting.EmVolum);
            }
        }//很难简化,或简化收益太小的代码
        public void SetBackgroundMusic(string path = null)
        { 
            if (m_player != null)
            {
                m_player.canplay = false;
                m_player = null;
            }
            if (path != null)
            {
                m_player = new AudioPlayer(path, true, SharedSetting.BGMVolum);
            }
        }//很难简化,或简化收益太小的代码
        public void SetLoppedEvireSound(string path = null)
        { 
            if (m_se_player != null)
            {
                m_se_player.canplay = false;
                m_se_player = null;
            }
            if (path != null)
            {
                m_se_player = new AudioPlayer(path, true, SharedSetting.EmVolum);
            }
        }//很难简化,或简化收益太小的代码


        public List<StaticCharacter> cts = new List<StaticCharacter>();
      
        #region rest 
        public int saved_frame = 0;
        public AudioPlayer m_player = null;
        public AudioPlayer m_em_player = null;
        public AudioPlayer m_se_player = null;


        public ManualResetEvent call_next = new ManualResetEvent(false);
        public Usage Usage { get; private set; }
        public Stage Stage { get; private set; }
        public AirPlant Airplant { get; private set; }
        public Grid bkSre = null;
        private bool IsDisposed { get; set; } = false;
        public void Dispose()
        {
            if (IsDisposed) return;

            
            if (call_next != null)
                call_next.Set();
            call_next = null;

            Stage.Dispose();

            Stage = null;
            Usage = null;
            Airplant = null;

            SetEnvironmentMusic();
            SetBackgroundMusic();
            SetLoppedEvireSound();
            StaticCharacter[] am_pos = new StaticCharacter[cts.Count];
            int im = 0;
            foreach (var i in cts)
            {
                am_pos[im] = i;
                im++;
            }
            foreach(var i in am_pos)
            {
                if (i?.Disposed == false) i.Dispose();
            }
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }//很难简化,或简化收益太小的代码

        ~Theatre()
        {
            Dispose();
        }
        public string Voice;
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
        }//很难简化,或简化收益太小的代码
        public void SetBackground(Color color)
        {
            bkSre.Dispatcher.Invoke(new Action(() => { bkSre.Background = new System.Windows.Media.SolidColorBrush(color); }));
        }//很难简化,或简化收益太小的代码
        #endregion
        public void WaitForClick(UIElement Home = null)
        {
            
            if (Home == null)
            {
                Home = bkSre;
            }
            if (skipping_flag)
            {
                MainWindow.m_window.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow.m_window.sl_process.Value = 100.0 * ((double)saved_frame / (double)locatPlace);
                    MainWindow.m_window.sl_tepro.Text = ((int)(100.0 * ((double)saved_frame / (double)locatPlace))).ToString() + "%";
                }));
            }
            if (locatPlace == saved_frame)
            {
                GamingTheatre.isSkiping = false;
                skipping_flag = false;
                MainWindow.m_window.Dispatcher.Invoke(new Action(()=> {
                    MainWindow.m_window.sys_con_pite.Visibility = Visibility.Visible;
                    MainWindow.m_window.saveLoadingProcess.Visibility = Visibility.Hidden;
                }));
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
            canCtrl = true;
            call_next.WaitOne();
            canCtrl = false;
            if (IsDisposed)
            {
                throw new Exception("Exitted");
            }
            Home.Dispatcher.Invoke(() => { Home.MouseLeftButtonUp -= localtion; Home.MouseWheel -= location2; });
            call_next.Reset();
        }
        private int locatPlace = 0;
        public bool canCtrl = true;
        public void SetNextLocatPosition(int place)
        {
            if (place == 0)
                return;

            GamingTheatre.isSkiping = true;
            skipping_flag = true;
            MainWindow.m_window.sl_process.Value =0;
            MainWindow.m_window.sl_tepro.Text = "0%";
            MainWindow.m_window.sys_con_pite.Visibility = Visibility.Hidden;
            MainWindow.m_window.saveLoadingProcess.Visibility = Visibility.Visible;
            /////////////////new instead

            locatPlace = place;
        }
        public bool skipping_flag = false;
    }//已经确认过安全的类

}