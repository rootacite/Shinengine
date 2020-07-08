using System;
using System.Windows;
using System.Windows.Input;

using System.Runtime.InteropServices;

using System.IO;
using SharpDX;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using SharpDX.WIC;
using System.Windows.Media;

using Shinengine.Media;
using System.Threading;
using Shinengine.Data;
using System.Windows.Controls;
using Image = System.Windows.Controls.Image;
using Shinengine.Scripts;
using MaterialDesignThemes.Wpf;
using Shinengine.Surface.Extra;
using Shinengine.Theatre;

namespace Shinengine.Surface
{
    static class ScriptList
    { 
        public struct ScriptDes
        {
            public int id;
            public GamingTheatre.ScriptHandle script;
            public Action scriptEnd;
        }
        static public ScriptDes[] Scripts = new ScriptDes[]
        {
            new ScriptDes(){id=0, script=Chapter1.Chapter1Script,scriptEnd=Chapter1.Chapter1ScriptEnd },
            new ScriptDes(){id=100, script=Chapter_H1.Chapter1Script,scriptEnd=Chapter_H1.Chapter1ScriptEnd }
        };
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Page top_page = null;

        #region sizing
        static public void ResizeToGrid(UIElementCollection grid, double width_rate, double height_rate)
        {

            foreach (var i in grid)
            {
                var _i = i as UIElement;
                if (_i.GetType() == typeof(System.Windows.Controls.Grid))
                {
                    var _i_r = _i as Grid;
                    if (_i_r.Name == "foreg" || _i_r.Name == "Page" || _i_r.Name == "character_usage" || _i_r.Name.Contains("save") || _i_r.Name== "ExtraGrid" || _i_r.Name== "_BkGrid")
                    {
                        _i_r.Width *= width_rate;
                        _i_r.Height *= height_rate;

                        var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                        _i_r.Margin = new_margin;
                    }
                    ResizeToGrid(_i_r.Children, width_rate, height_rate);
                    continue;
                }
                if (_i is Image)
                {
                    var _i_r = _i as Image;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;
                }
                else if (_i is Button)
                {
                    var _i_r = _i as Button;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;
                }
                else if (_i is Canvas)
                {
                    var _i_r = _i as Canvas;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;

                    ResizeToGrid(_i_r.Children, width_rate, height_rate);
                    continue;
                }
                else if (_i is TextBlock)
                {
                    var _i_r = _i as TextBlock;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    _i_r.FontSize *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;
                }
                else if (_i is Slider)
                {
                    var _i_r = _i as Slider;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;
                }
                else if (_i is StackPanel)
                {
                    var _i_r = _i as StackPanel;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;

                    ResizeToGrid(_i_r.Children, width_rate, height_rate);
                }else if(_i is PopupBox)
                {
                    var _i_r = _i as PopupBox;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;

                } else if(_i is ProgressBar)
                {
                    var _i_r = _i as ProgressBar;
                    _i_r.Width *= width_rate;
                    _i_r.Height *= height_rate;

                    var new_margin = new Thickness(_i_r.Margin.Left * width_rate, _i_r.Margin.Top * height_rate, _i_r.Margin.Right * width_rate, _i_r.Margin.Bottom * height_rate);
                    _i_r.Margin = new_margin;
                }
            }
        }
        static public void ResizeEvt(Grid page, Size2 oldSize, Size2 newSize)
        {
            double width_rate = (double)newSize.Width / (double)oldSize.Width;
            double height_rate = (double)newSize.Height / (double)oldSize.Height;

            ResizeToGrid(page.Children, width_rate, height_rate);
        }

        static Title _title = null;  
        static new public Title Title {
            get
            {
                return _title;
            }
            set
            {
                top_page = value;
                _title = value;
            }
        }
        static GamingBook _BookMode = null;
        static public GamingBook BookMode{
            get 
            {
                return _BookMode;
            }
            set
            {
                top_page = value;
                _BookMode = value;
            } 
        }

        static GamingTheatre _TheatreMode = null;
        static public GamingTheatre TheatreMode { 
            get 
            {
                return _TheatreMode;
            } 
            set 
            {
                _TheatreMode = value;
                top_page = value;
            }
        }

        static Setting _Settere = null;
        static public Setting Settere { 
            get 
            {
                return _Settere;
            } 
            set 
            {
                top_page = value;
                _Settere = value;
            }
        }
        static SaveLoad _Sldata = null;
        static public SaveLoad Sldata {
            get 
            { 
                return _Sldata;
            } 
            set 
            {
                _Sldata = value;
                top_page = value;
            }
        }
        static ExtraHome _ExtraPage = null;
        static public ContentControl sys_pite = null;
        static public ExtraHome ExtraPage { 
            get 
            {
                return _ExtraPage;
            } 
            set 
            {
                _ExtraPage = value;
                top_page = value;
            }
        }
        #endregion


        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        static public MainWindow m_window = null;

        public MainWindow()
        {
            timeBeginPeriod(1);
            InitializeComponent();
            sys_pite = sys_con_pite;

            m_window = this;
            if (SharedSetting.FullS)
            {

                this.WindowStyle = WindowStyle.None;
                this.WindowState = System.Windows.WindowState.Maximized;
            }

            FFmpegBinariesHelper.RegisterFFmpegBinaries();
            return;
        }
        static public GamingBook SwitchToBookmode()
        {
            GamingBook mbp = new GamingBook(m_window);
            mbp.Inint(new Data.DataStream("Book1.xml"));
            mbp.Start(0);

            BookMode = mbp;
            if (true)
            {
                ResizeEvt(BookMode.Book, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));

            }
            return mbp;
        }
        static public Title SwitchToTitle()
        {
            Title mlp = new Title();
            mlp.setting.Click += (e, v) =>
            {
                Settere = new Setting(new BitmapImage(new Uri("pack://application:,,,/UI/10.png")));
                if (true)
                {
                    ResizeEvt(Settere.mpOi, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));
                }
                Settere.foreg.Unloaded += (e, v) => { Settere = null; };
                Settere.foreg.MouseRightButtonUp += (e, v) =>
                {
                    SwitchToTitle();
                    MainWindow.sys_pite.Content = Title.Content;
                };
                Settere.exitlpg.Click += (e,v) =>
                {
                    SwitchToTitle();
                    MainWindow.sys_pite.Content = Title.Content;
                };
                Settere.fullandwindow.Click += (e, v) =>
                {
                    SharedSetting.FullS = !SharedSetting.FullS;
                    if (SharedSetting.FullS)
                    {
                        m_window.WindowStyle = WindowStyle.None;
                        m_window.WindowState = System.Windows.WindowState.Maximized;
                        SwitchToTitle();
                        MainWindow.sys_pite.Content = Title.Content;
                    }
                    else
                    {
                        m_window.WindowStyle = WindowStyle.SingleBorderWindow;
                        m_window.WindowState = System.Windows.WindowState.Normal;
                        SwitchToTitle();
                        MainWindow.sys_pite.Content = Title.Content;
                    }

                };
                MainWindow.sys_pite.Content = Settere.Content;
            };
            mlp.EnExit.Click += (e, v) =>
            {
                m_window.Close();
            };
           
            mlp.StartButton.Click += (e, v) =>
            {
                mlp.StartButton.IsEnabled = false;
                mlp.Background = new System.Windows.Media.SolidColorBrush(Colors.White);
                EasyAmal maa = null;

                maa = new EasyAmal(Title.BkGrid, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed / 2, (e, v) =>
                {
                    TheatreMode = SwitchToSignalTheatre(0, 0, null);

                    Title = null;
                    maa.stbd.Stop();
                });
                maa.Start(true);

            };
            mlp.SaveLoad.Click += (e, v) => {
                var m_thread_intp = new Thread(() =>
                {
                    EasyAmal mpos = new EasyAmal(mlp.BkGrid, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed);
                    mpos.Start(false);/// hide tview
                    mlp.Dispatcher.Invoke(new Action(() =>
                    {
                        SaveLoad msv = new SaveLoad(0, 0, null)
                        {
                            disableSave = true
                        };
                        if (true)
                        {
                            ResizeEvt(msv.Forgan, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));
                        }
                        MainWindow.sys_pite.Content = msv.Content;
                        Sldata = msv;
                        Title = null;
                        msv.Forgan.MouseRightButtonUp += (e, v) =>
                        {
                            Title = SwitchToTitle();
                            MainWindow.sys_pite.Content = Title.Content;
                            Sldata = null;
                        };

                        msv.exitlpg.Click += (e, v) =>
                        {
                            Title = SwitchToTitle();
                            MainWindow.sys_pite.Content = Title.Content;
                            Sldata = null;
                        };
                    }));

                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();
            };
            mlp.extra.Click += (e,v) => 
            {
                var m_thread_intp = new Thread(() =>
                {
                    EasyAmal mpos = new EasyAmal(mlp.BkGrid, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed);
                    mpos.Start(false);/// hide tview
                    mlp.Dispatcher.Invoke(new Action(() =>
                    {

                        ExtraHome msv = new ExtraHome();

                        MainWindow.sys_pite.Content = msv.Content;
                        ExtraPage = msv;
                        Title = null;
                        msv.ExtraGrid.MouseRightButtonUp += (e, v) =>
                        {
                            Title = SwitchToTitle();
                            MainWindow.sys_pite.Content = Title.Content;
                            ExtraPage = null;
                        };

                        msv.exitlpg.Click += (e, v) =>
                        {
                            Title = SwitchToTitle();
                            MainWindow.sys_pite.Content = Title.Content;
                            ExtraPage = null;
                        };
                    }));

                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();
            };
            Title = mlp;
            if (true)
            {
                ResizeEvt(Title.BkGrid, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));

            }

            return mlp;
        }

        static public GamingTheatre SwitchToSignalTheatre(int id,int start_place,Action end)
        {

            GamingTheatre m_game = new GamingTheatre();
            if (true)
            {
                ResizeEvt(m_game.SBK, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));
            }
            m_game.Init(m_window);
            m_game.toTitle.Click += (e, v) =>
            {
                Title = SwitchToTitle();
                MainWindow.sys_pite.Content = Title.Content;

                TheatreMode.Dispose();

                if (TheatreMode != null)
                    TheatreMode = null;
            };
            m_game.SaveLoad.Click += (e, v) =>
            {
                if (GamingTheatre.isSkiping)
                {
                    return;
                }
                GamingTheatre.AutoMode = false;
                if (m_game.ShowIn.Children.Contains(m_game.auto_icon))
                {
                    m_game.ShowIn.Children.Remove(m_game.auto_icon);
                }

                var m_thread_intp = new Thread(() =>
                {
                    EasyAmal mpos = new EasyAmal(m_game.SBK, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed);
                    mpos.Start(false);/// hide tview

                    m_game.Dispatcher.Invoke(new Action(() =>
                    {
                        SaveLoad mst = new SaveLoad(id, m_game.m_theatre.saved_frame, m_game.m_theatre.Stage.last_save);
                        if (true)
                        {
                            ResizeEvt(mst.Forgan, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));
                        }
                        Sldata = mst;

                        bool canFocue = false;
                        Sldata.exitlpg.Click += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(Sldata.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    MainWindow.sys_pite.Content = m_game.Content;
                                    EasyAmal mpos = new EasyAmal(m_game.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                                    mpos.Start(true);/// hide tview
                                    Sldata = null;
                                });
                                mpos2.Start(true);
                            }).Start();
                        };
                        Sldata.Forgan.MouseRightButtonUp += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(Sldata.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    MainWindow.sys_pite.Content = m_game.Content;
                                    EasyAmal mpos = new EasyAmal(m_game.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                                    mpos.Start(true);/// hide tview
                                    Sldata = null;
                                });
                                mpos2.Start(true);
                            }).Start();

                        };

                        EasyAmal mpos = new EasyAmal(mst.Forgan, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                        mpos.Start(true);

                        MainWindow.sys_pite.Content = mst.Content;
                    }));

                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();
            };
            m_game.setting.Click += (e, v) =>
            {
                if (GamingTheatre.isSkiping)
                {
                    return;
                }
                GamingTheatre.AutoMode = false;
                if (m_game.ShowIn.Children.Contains(m_game.auto_icon))
                {
                    m_game.ShowIn.Children.Remove(m_game.auto_icon);
                }
                #region 获取上一次绘图
                var lpic = m_game.m_theatre.Stage.last_save;

                var mbps = new WriteableBitmap((int)lpic.Size.Width, (int)lpic.Size.Height, 72, 72, System.Windows.Media.PixelFormats.Pbgra32, null);
                mbps.Lock();

                var mic_lock = lpic.Lock(BitmapLockFlags.Read);
                unsafe
                {
                    Direct2DImage.RtlMoveMemory((void*)mbps.BackBuffer, (void*)mic_lock.Data.DataPointer, lpic.Size.Height * mbps.BackBufferStride);
                }
                mic_lock.Dispose();
                mbps.AddDirtyRect(new Int32Rect(0, 0, (int)lpic.Size.Width, (int)lpic.Size.Height));
                mbps.Unlock();
                #endregion
                var m_thread_intp = new Thread(() =>
                {
                    m_game.m_theatre.Usage.Hide(null, false);
                    m_game.Dispatcher.Invoke(new Action(() =>
                    {
                        Setting mst = new Setting(mbps);
                        if (true)
                        {
                            ResizeEvt(mst.mpOi, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));
                        }
                        Settere = mst;
                        Settere.fullandwindow.IsEnabled = false;
                        EasyAmal mpos = new EasyAmal(Settere.foreg, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);

                        bool canFocue = false;
                        Settere.exitlpg.Click += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(Settere.foreg, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    MainWindow.sys_pite.Content = m_game.Content;
                                    m_game.m_theatre.Usage.Show(null, true);
                                    Settere = null;
                                });
                                mpos2.Start(true);
                            }).Start();
                        };
                        Settere.mpOi.MouseRightButtonUp += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(Settere.foreg, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    MainWindow.sys_pite.Content = m_game.Content;
                                    m_game.m_theatre.Usage.Show(null, true);
                                    Settere = null;
                                });
                                mpos2.Start(true);
                            }).Start();

                        };
                        mpos.Start(true);
                        MainWindow.sys_pite.Content = mst.Content;
                    }));

                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();

            };

            m_game.EnExit.Click += (e, v) =>
            {
                MessageBoxResult result = MessageBox.Show("是否要记住当前的进度?\n(下次进入游戏时不进入标题界面,而是直接从此处开始)", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.No)
                {
                    m_window.Close();
                }else if (result == MessageBoxResult.Yes)
                {
                    SharedSetting.Last = new SaveData.SaveInfo() { chapter = id, frames = m_game.m_theatre.saved_frame };
                    m_window.Close();
                }
            };
            
            if (end == null)
            {
                for (int i=0;i< ScriptList.Scripts.Length; i++)
                {
                    if (ScriptList.Scripts[i].id == id)
                    {
                        m_game.Start(ScriptList.Scripts[i].script, id, ScriptList.Scripts[i].scriptEnd);
                    }
                }
              
            }
            else
            {
                for (int i = 0; i < ScriptList.Scripts.Length; i++)
                {
                    if (ScriptList.Scripts[i].id == id)
                    {
                        m_game.Start(ScriptList.Scripts[i].script, id, end);
                    }
                }
            }
           
            m_game.BG.Opacity = 0;
            m_game.Usage.Opacity = 0;
            
            if (start_place != 0)
            {
                m_game.m_theatre.SetNextLocatPosition(start_place);
            }
            else
                m_game.m_theatre.saved_frame = 0;

            MainWindow.sys_pite.Content = m_game.Content;
            
            return m_game;


        }

        
        private void BkGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (SharedSetting.FullS)
            {
                ResizeEvt(_BkGrid, new Size2(1280, 720), new Size2((int)m_window.sys_con_pite.Width, (int)m_window.sys_con_pite.Height));

            }
            if (SharedSetting.Last != null)
            {
                TheatreMode = SwitchToSignalTheatre(SharedSetting.Last.Value.chapter, SharedSetting.Last.Value.frames, null);
                SharedSetting.Last = null;
                return;
            }
            _Shower.Opacity = 0;
            _Shower.Source = new BitmapImage(new Uri("pack://application:,,,/RegularUI/sys_warning_bg.png"));

            new Thread(()=> {
                /*
                EasyAmal _ms = new EasyAmal(_Shower, "(Opacity)", 0.0, 1.0, 1);
                _ms.Start(false);
                Thread.Sleep(3500);
                _ms = new EasyAmal(_Shower, "(Opacity)", 1.0, 0.0, 1);
                _ms.Start(false);
                _Shower.Dispatcher.Invoke(new Action(()=> { _Shower.Source = new BitmapImage(new Uri("pack://application:,,,/RegularUI/logo00.png")); }));
                _ms = new EasyAmal(_Shower, "(Opacity)", 0.0, 1.0, 1);
                _ms.Start(false);
                Thread.Sleep(3500);
                _ms = new EasyAmal(_Shower, "(Opacity)", 1.0, 0.0, 1);
                _ms.Start(false);
                */
                _Shower.Dispatcher.Invoke(new Action(() =>
                {
                    if (SharedSetting.Last == null)
                    {
                        SwitchToTitle();
                        MainWindow.sys_pite.Content = Title.Content;
                    }

              
                    return;
                }));

            }).Start();
           
            return;


           
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Directory.Delete(PackStream.TempPath[0..^1], true);
            }
            catch(Exception)
            {

            }
            PInvoke.Kernel32.ExitProcess(0);
        }

        private void MmKeyDown(object sender, KeyEventArgs e)
        {
            if (TheatreMode == null)
                return;
            if (Settere != null || TheatreMode.isBakcloging||TheatreMode.m_theatre.skipping_flag)
            {
                return;
            }
            if (e.Key == Key.LeftCtrl)
            {
                GamingTheatre.isSkiping = true;

                if (TheatreMode.m_theatre.call_next != null) TheatreMode.m_theatre.call_next.Set();
                TheatreMode.Menu.IsEnabled = false;
                if (!TheatreMode.ShowIn.Children.Contains(TheatreMode.skip_icon))
                {
                    TheatreMode.ShowIn.Children.Add(TheatreMode.skip_icon);
                }
            }

        }

        private void MmKeyUp(object sender, KeyEventArgs e)
       {
            if (e.Key == Key.Space)
                GC.Collect();
         
            if (TheatreMode == null)
                return;
            if (Settere != null || TheatreMode.isBakcloging || TheatreMode.m_theatre.skipping_flag)
            {
                return;
            }

            if (e.Key == Key.LeftCtrl)
            {
                GamingTheatre.isSkiping = false;
                if (TheatreMode != null)
                {
                    TheatreMode.Menu.IsEnabled = true;
                    if (TheatreMode.ShowIn.Children.Contains(TheatreMode.skip_icon))
                    {
                        TheatreMode.ShowIn.Children.Remove(TheatreMode.skip_icon);
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 100 || e.NewSize.Height < 100)
            {
                return;
            }
            int count_isntnull = 0;
            Page target = null;
            foreach (var i in new Page[]{ Title, BookMode ,TheatreMode, Settere, Sldata, ExtraPage })
            {
                if (i != null) { 
                    count_isntnull++;
                    target = i;
                }
            }
            if (target == null)
            {
                return;
            }
            if (count_isntnull > 1)
            {
                target = top_page;
            }

            Size2 new_size = new Size2();
            RECT m_rect=new RECT();
            GetClientRect(new WindowInteropHelper(this).Handle, out m_rect);
            if((double)m_rect.Right/ (double)m_rect.Bottom < (16 / 9.0d))
            {
                new_size.Width = (int)m_rect.Right;
                new_size.Height = (int)(m_rect.Right * (9 / 16.0d));

                sys_con_pite.Width = new_size.Width;
                sys_con_pite.Height = new_size.Height;

                sys_con_pite.Margin = new Thickness(0, m_rect.Bottom/2.0- sys_con_pite.Height  / 2.0, 0, 0);

                ResizeEvt(_BkGrid, new Size2((int)saveLoadingProcess.ActualWidth, (int)saveLoadingProcess.ActualHeight), new Size2(new_size.Width, new_size.Height));
                saveLoadingProcess.Margin = new Thickness(0,m_rect.Bottom / 2.0 - saveLoadingProcess.Height / 2.0, 0, 0);
            }
            else
            {
                new_size.Height = (int)m_rect.Bottom;
                new_size.Width = (int)(m_rect.Bottom * (16 / 9.0d));

                sys_con_pite.Width = new_size.Width;
                sys_con_pite.Height = new_size.Height;

                sys_con_pite.Margin = new Thickness( m_rect.Right  / 2.0 - sys_con_pite.Width/2.0, 0, 0, 0);

               ResizeEvt(_BkGrid, new Size2((int)saveLoadingProcess.ActualWidth, (int)saveLoadingProcess.ActualHeight), new Size2(new_size.Width, new_size.Height));
               saveLoadingProcess.Margin = new Thickness(m_rect.Right / 2.0 - saveLoadingProcess.Width / 2.0, 0, 0, 0);
            }

           
            if (target is Title)
            {
                var _target = target as Title;
                ResizeEvt((target as Title).BkGrid, new Size2((int)_target.BkGrid.ActualWidth, (int)_target.BkGrid.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
            else if(target is GamingBook)
            {
                var _target = target as GamingBook;
                ResizeEvt((target as GamingBook).Page, new Size2((int)_target.Page.ActualWidth, (int)_target.Page.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
            else if (target is GamingTheatre)
            {
                var _target = target as GamingTheatre;
                ResizeEvt((target as GamingTheatre).SBK, new Size2((int)_target.SBK.ActualWidth, (int)_target.SBK.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
            else if (target is Setting)
            {
                var _target = target as Setting;
                ResizeEvt((target as Setting).mpOi, new Size2((int)_target.mpOi.ActualWidth, (int)_target.mpOi.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
            else if (target is SaveLoad)
            {
                var _target = target as SaveLoad;
                ResizeEvt((target as SaveLoad).Forgan, new Size2((int)_target.Forgan.ActualWidth, (int)_target.Forgan.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
            else if (target is ExtraHome)
            {
                var _target = target as ExtraHome;
                ResizeEvt((target as ExtraHome).ExtraGrid, new Size2((int)_target.ExtraGrid.ActualWidth, (int)_target.ExtraGrid.ActualHeight), new Size2(new_size.Width, new_size.Height));
            }
        }

        [DllImport("user32")]
        public static extern bool GetClientRect(  IntPtr hwnd, out RECT lpRect );
        public struct RECT { public uint Left; public uint Top; public uint Right; public uint Bottom; }
    }
}