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
using System.Windows.Controls;
using System.Collections.Generic;
using FFmpeg.AutoGen;
using Image = System.Windows.Controls.Image;
using Shinengine.Scripts;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.CompilerServices;
using MaterialDesignThemes.Wpf;
using Shinengine.Surface.Extra;

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
        static public void ResizeToGrid(UIElementCollection grid, double width_rate, double height_rate)
        {

            foreach (var i in grid)
            {
                var _i = i as UIElement;
                if (_i.GetType() == typeof(System.Windows.Controls.Grid))
                {
                    var _i_r = _i as Grid;
                    if (_i_r.Name == "foreg" || _i_r.Name == "Page" || _i_r.Name == "character_usage" || _i_r.Name.Contains("save") || _i_r.Name== "ExtraGrid")
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

                }

            }
        }
        static private void ResizeEvt(Grid page, Size2 oldSize, Size2 newSize)
        {
            double width_rate = (double)newSize.Width / (double)oldSize.Width;
            double height_rate = (double)newSize.Height / (double)oldSize.Height;

            ResizeToGrid(page.Children, width_rate, height_rate);
        }
        static public Title title = null;
        static public GamingBook bookMode = null;
        static public GamingTheatre theatreMode = null;
        static public Setting settere = null;
        static public SaveLoad sldata = null;
        static public ExtraHome extraPage = null;
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        static public MainWindow m_window = null;

        public MainWindow()
        {
            timeBeginPeriod(1);
            InitializeComponent();
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

            bookMode = mbp;
            if (SharedSetting.FullS)
            {
                ResizeEvt(bookMode.Book, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));

            }
            return mbp;
        }
        static public Title SwitchToTitle()
        {
            Title mlp = new Title();
            mlp.setting.Click += (e, v) =>
            {
                settere = new Setting(new BitmapImage(new Uri("pack://application:,,,/UI/10.png")), null, null);
                if (SharedSetting.FullS)
                {
                    ResizeEvt(settere.mpOi, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                }
                settere.foreg.Unloaded += (e, v) => { settere = null; };
                settere.foreg.MouseRightButtonUp += (e, v) =>
                {
                    SwitchToTitle();
                    m_window.Content = title.Content;
                };
                settere.exitlpg.Click += (e,v) =>
                {
                    SwitchToTitle();
                    m_window.Content = title.Content;
                };
                settere.fullandwindow.Click += (e, v) =>
                {
                    SharedSetting.FullS = !SharedSetting.FullS;
                    if (SharedSetting.FullS)
                    {
                        m_window.WindowStyle = WindowStyle.None;
                        m_window.WindowState = System.Windows.WindowState.Maximized;
                        ResizeEvt(settere.mpOi, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                    }
                    else
                    {
                        m_window.WindowStyle = WindowStyle.SingleBorderWindow;
                        m_window.WindowState = System.Windows.WindowState.Normal;
                        ResizeEvt(settere.mpOi, new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight), new Size2(1280, 720));
                    }

                };
                m_window.Content = settere.Content;
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

                maa = new EasyAmal(title.BkGrid, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed / 2, (e, v) =>
                {
                    theatreMode = SwitchToSignalTheatre(0, 0, null);


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

                        m_window.Content = msv.Content;
                        sldata = msv;
                        title = null;
                        msv.Forgan.MouseRightButtonUp += (e, v) =>
                        {
                            title = SwitchToTitle();
                            m_window.Content = title.Content;
                            sldata = null;
                        };

                        msv.exitlpg.Click += (e, v) =>
                        {
                            title = SwitchToTitle();
                            m_window.Content = title.Content;
                            sldata = null;
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

                        m_window.Content = msv.Content;
                        extraPage = msv;
                        title = null;
                        msv.ExtraGrid.MouseRightButtonUp += (e, v) =>
                        {
                            title = SwitchToTitle();
                            m_window.Content = title.Content;
                            extraPage = null;
                        };

                        msv.exitlpg.Click += (e, v) =>
                        {
                            title = SwitchToTitle();
                            m_window.Content = title.Content;
                            extraPage = null;
                        };
                    }));

                })
                {
                    IsBackground = true
                };
                m_thread_intp.Start();
            };
            title = mlp;
            if (SharedSetting.FullS)
            {
                ResizeEvt(title.BkGrid, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));

            }

            return mlp;
        }

        static public GamingTheatre SwitchToSignalTheatre(int id,int start_place,Action end)
        {


            GamingTheatre m_game = new GamingTheatre();
            if (SharedSetting.FullS)
            {
                ResizeEvt(m_game.SBK, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
            }
            m_game.Init(m_window);
            m_game.toTitle.Click += (e, v) =>
            {
                title = SwitchToTitle();
                m_window.Content = title.Content;

                theatreMode.m_logo.Dispose();
                theatreMode.m_theatre.SetBackgroundMusic();
                theatreMode.m_theatre.Exit();

                if (theatreMode != null)
                    theatreMode = null;
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
                        if (SharedSetting.FullS)
                        {
                            ResizeEvt(mst.Forgan, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                        }
                        sldata = mst;

                        bool canFocue = false;
                        sldata.exitlpg.Click += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(sldata.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    m_window.Content = m_game.Content;
                                    EasyAmal mpos = new EasyAmal(m_game.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                                    mpos.Start(true);/// hide tview
                                    sldata = null;
                                });
                                mpos2.Start(true);
                            }).Start();

                        };
                        sldata.Forgan.MouseRightButtonUp += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(sldata.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    m_window.Content = m_game.Content;
                                    EasyAmal mpos = new EasyAmal(m_game.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                                    mpos.Start(true);/// hide tview
                                    sldata = null;
                                });
                                mpos2.Start(true);
                            }).Start();

                        };

                        EasyAmal mpos = new EasyAmal(mst.Forgan, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);
                        mpos.Start(true);

                        m_window.Content = mst.Content;
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
                        Setting mst = new Setting(mbps, m_game.m_theatre.m_player, m_game.m_theatre.m_em_player);
                        if (SharedSetting.FullS)
                        {
                            ResizeEvt(mst.mpOi, new Size2(1280, 720), new Size2((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight));
                        }
                        settere = mst;
                        settere.fullandwindow.IsEnabled = false;
                        EasyAmal mpos = new EasyAmal(settere.foreg, "(Opacity)", 0.0, 1.0, SharedSetting.SwitchSpeed);

                        bool canFocue = false;
                        settere.exitlpg.Click += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(settere.foreg, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    m_window.Content = m_game.Content;
                                    m_game.m_theatre.Usage.Show(null, true);
                                    settere = null;
                                });
                                mpos2.Start(true);
                            }).Start();
                        };
                        settere.mpOi.MouseRightButtonUp += (e, v) =>
                        {
                            if (canFocue) return;
                            canFocue = true;
                            new Thread(() =>
                            {

                                EasyAmal mpos2 = new EasyAmal(settere.foreg, "(Opacity)", 1.0, 0.0, SharedSetting.SwitchSpeed, (e, c) =>
                                {
                                    m_window.Content = m_game.Content;
                                    m_game.m_theatre.Usage.Show(null, true);
                                    settere = null;
                                });
                                mpos2.Start(true);
                            }).Start();

                        };
                        mpos.Start(true);
                        m_window.Content = mst.Content;
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

            m_window.Content = m_game.Content;

            return m_game;


        }

        
        private void BkGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (SharedSetting.Last == null)
            {
                SwitchToTitle();
                this.Content = title.Content;
            }
            else
            {
                theatreMode = SwitchToSignalTheatre(SharedSetting.Last.Value.chapter, SharedSetting.Last.Value.frames, null);
                SharedSetting.Last = null;
            }
            return;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PInvoke.Kernel32.ExitProcess(0);
        }

        private void MmKeyDown(object sender, KeyEventArgs e)
        {
            if (theatreMode == null)
                return;
            if (settere != null || theatreMode.isBakcloging)
            {
                return;
            }
            if (e.Key == Key.LeftCtrl)
                GamingTheatre.isSkiping = true;

            if (theatreMode.m_theatre.call_next != null) theatreMode.m_theatre.call_next.Set();
            theatreMode.Menu.IsEnabled = false;
            if (!theatreMode.ShowIn.Children.Contains(theatreMode.skip_icon))
            {
                theatreMode.ShowIn.Children.Add(theatreMode.skip_icon);
            }


        }

        private void MmKeyUp(object sender, KeyEventArgs e)
       {
            if (e.Key == Key.LeftCtrl)
                GamingTheatre.isSkiping = false;
            if (theatreMode == null)
                return;
            if (settere != null || theatreMode.isBakcloging)
            {
                return;
            }

          
            if (theatreMode != null)
            {
                theatreMode.Menu.IsEnabled = true;
                if (theatreMode.ShowIn.Children.Contains(theatreMode.skip_icon))
                {
                    theatreMode.ShowIn.Children.Remove(theatreMode.skip_icon);
                }
            }
        }
    }
}