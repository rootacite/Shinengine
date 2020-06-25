using SharpDX.Mathematics.Interop;
using System;
using System.Windows;
using System.Windows.Input;

using System.Runtime.InteropServices;


using SharpDX;
using SharpDX.Direct2D1;
using System.Windows.Media.Imaging;
using ImageBrush = System.Windows.Media.ImageBrush;
using System.Windows.Interop;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using SharpDX.WIC;
using System.Windows.Media;
using System.Media;

using Shinengine.Media;
using Color = System.Windows.Media.Color;

using NAudio.Wave;
using System.Threading;
using System.Drawing;
using SharpDX.DXGI;
using Shinengine.Data;

namespace Shinengine.Surface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Title title = null;
        GamingBook bookMode = null;
        GamingTheatre theatreMode = null;
        Setting settere = null;

        StaticCharacter.ChangeableAreaInfo[] m_Infos = null;
        private StaticCharacter character_1;

        public MainWindow()
        {
            InitializeComponent();
            m_Infos = new StaticCharacter.ChangeableAreaInfo[1];
            m_Infos[0].area = new Rect(0, 0, 1500, 1500);
            m_Infos[0].pics = new string[3];
            m_Infos[0].pics[0] = "assets\\Character\\BS_RE2x_face___000.png";
            m_Infos[0].pics[1] = "assets\\Character\\BS_RE2x_face___001.png";
            m_Infos[0].pics[2] = "assets\\Character\\BS_RE2x_face___002.png";

            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            return;
        }
        private GamingBook switchToBookmode()
        {
            GamingBook mbp = new GamingBook(this);
            mbp.Inint(new Data.DataStream("Book1.xml"));
            mbp.Start(0);

            bookMode = mbp;
            return mbp; 
        }
        private Title switchToTitle()
        {
            Title mlp = new Title();
            mlp.StartButton.Click += (e, v) =>
            {

                mlp.StartButton.IsEnabled = false;
                mlp.Background = new System.Windows.Media.SolidColorBrush(Colors.White);
                EasyAmal maa = null;

                 maa = new EasyAmal(title.BkGrid, "(Opacity)",1.0,0.0, SharedSetting.switchSpeed/2,(e,v)=>
                {
                    theatreMode = switchToTheatremode();
                    this.Content = theatreMode.Content;
                    maa.stbd.Stop();
                    
                    title.Close();
                  

                });
                maa.Start(true);
            };
            title = mlp;
            return mlp;
        }

        private GamingTheatre switchToTheatremode()
        {


            GamingTheatre m_game = new GamingTheatre(this);

            m_game.toTitle.Click += (e, v) =>
            {
                  title = switchToTitle();
                  this.Content = title.Content;
                theatreMode.Close();

                theatreMode.m_logo.Dispose();
                if (theatreMode.m_player != null)
                    theatreMode.m_player.canplay = false;
                theatreMode.m_theatre.Exit();

                if (theatreMode != null)
                    theatreMode = null;
            };

            m_game.setting.Click += (e, v) =>
            {
                if (GamingTheatre.isSkiping)
                {
                    return;
                }
                var lpic = m_game.m_theatre.stage.last_save;

                var mbps = new WriteableBitmap((int)m_game.BG.Width, (int)m_game.BG.Height, 72, 72, System.Windows.Media.PixelFormats.Pbgra32, null);
                mbps.Lock();

                var mic_lock = lpic.Lock(BitmapLockFlags.Read);
                unsafe
                {
                    Direct2DImage.RtlMoveMemory((void*)mbps.BackBuffer, (void*)mic_lock.Data.DataPointer, mbps.PixelHeight * mbps.BackBufferStride);
                }
                mic_lock.Dispose();
                mbps.AddDirtyRect(new Int32Rect(0, 0, (int)m_game.BG.Width, (int)m_game.BG.Height));
                mbps.Unlock();
                var m_thread_intp = new Thread(()=> {
                    m_game.m_theatre.usage.Hide(null, false);
                    m_game.Dispatcher.Invoke(new Action(()=> {
                        Setting mst = new Setting(mbps, this, this.Content, m_game.m_player);
                        settere = mst;
                        EasyAmal mpos = new EasyAmal(settere.foreg, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);

                        bool canFocue = false;
                        settere.mpOi.MouseRightButtonUp += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(()=> {
                               
                                EasyAmal mpos2 = new EasyAmal(settere.foreg, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) => {
                                    settere.Close();
                                    this.Content = m_game.Content;
                                    m_game.m_theatre.usage.Show(null, true);
                                    settere = null;
                                });
                                mpos2.Start(true);
                            }).Start();
                           
                        };
                        mpos.Start(true);
                        this.Content = mst.Content;
                    }));
                   
                });
                m_thread_intp.IsBackground = true;
                m_thread_intp.Start();
               
            };

            m_game.BG.Opacity = 0;
            m_game.Usage.Opacity = 0;
            m_game.Start((s) =>
            {
                try
                {
                    s.setBackground(Colors.White);
                    s.stage.setAsImage("assets\\CG\\10.png", 0, false);
                    s.stage.Show(null, true);
                    s.usage.Show();

                    m_game.m_player = new AudioPlayer("assets\\BGM\\01.wav", true);

                    character_1 = new StaticCharacter("墨小菊", "assets\\Character\\BS_RE20_01B.png", s.CharacterLayer, false, m_Infos, null, false);
                    character_1.SwitchTo(0, 1, 0, false);
                    character_1.Show();

                    s.waitForClick(s.bkSre);


                    character_1.Hide();
                    s.stage.setAsVideo("assets\\movie\\H005a.mpg", null, true);
                    s.waitForClick(s.bkSre);
                    s.stage.setAsVideo("assets\\movie\\H005b.mpg", null, true);
                    s.waitForClick(s.bkSre);
                    s.stage.setAsVideo("assets\\movie\\H005c.mpg", null, true);
                    s.waitForClick(s.bkSre);
                    s.stage.setAsVideo("assets\\movie\\H005d.mpg", null, true);
                    s.waitForClick(s.bkSre);
                    s.stage.setAsVideo("assets\\movie\\H005e.mpg", null, true);
                    s.waitForClick(s.bkSre);
                    s.stage.setAsVideo("assets\\movie\\H005f.mpg", null, true);

                    s.waitForClick(s.bkSre);

                }
                catch
                {
                    character_1.Dispose();
                    return 0;
                }
                return 0;
            });
            return m_game;
           

        }
        private void BkGrid_Loaded(object sender, RoutedEventArgs e)
        {
            switchToTitle();
            this.Content = title.Content;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (title != null)
                title.Close();

            if (theatreMode != null)
                theatreMode.Close();

            if (settere != null)
                settere.Close();

            if (bookMode != null)
                bookMode.Close();

            PInvoke.Kernel32.ExitProcess(0);
        }

        private void mmKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                GamingTheatre.isSkiping = true;
            if (theatreMode != null) if (theatreMode.m_theatre.call_next != null) theatreMode.m_theatre.call_next.Set();
        }

        private void mmKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                GamingTheatre.isSkiping = false;
        }
    }
}
