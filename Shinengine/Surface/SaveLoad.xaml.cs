using Shinengine.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SharpDX.WIC;
using SharpDX.Direct2D1;

using BitmapSource = System.Windows.Media.Imaging.BitmapSource;
using ImageBrush = System.Windows.Media.ImageBrush;

using WICBitmap = SharpDX.WIC.Bitmap;
using SharpDX;

using Bitmap = System.Drawing.Bitmap;
using System.Threading;
using Shinengine.Scripts;
using System.IO;

namespace Shinengine.Surface
{
    /// <summary>
    /// SaveLoad.xaml 的交互逻辑
    /// </summary>
    public partial class SaveLoad : Page
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public bool disableSave = false;
        public BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource source;
            try
            {
                source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return source;
        }
        int chapter = 0;
        int _frame = 0;
        private WICBitmap _last_draw;

        public SaveLoad(int _chapter, int frame, WICBitmap last_draw)
        {
            InitializeComponent();
            chapter = _chapter;
            _last_draw = last_draw;
            _frame = frame;
            SaveData.SaveInfo imp_save001 = new SaveData.SaveInfo();
            SaveData.SaveInfo imp_save002 = new SaveData.SaveInfo();
            SaveData.SaveInfo imp_save003 = new SaveData.SaveInfo();
            SaveData.SaveInfo imp_save004 = new SaveData.SaveInfo();
            try
            {
                imp_save002 = SaveData.save2;
                save_2_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save002.imp));
                save_2_content.Text = imp_save002.comment;
            }
            catch
            {

            }
            try
            {
                imp_save003 = SaveData.save3;
                save_3_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save003.imp));
                save_3_content.Text = imp_save003.comment;
            }
            catch
            {

            }
            try
            {
                imp_save001 = SaveData.save1;
                save_1_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save001.imp));
                save_1_content.Text = imp_save001.comment;
            }
            catch
            {

            }
            try
            {
                imp_save004 = SaveData.save4;
                save_4_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save004.imp));
                save_4_content.Text = imp_save004.comment;
            }
            catch
            {

            }
        }

        static public void SaveSignalPlaceToData()
        {

        }
        static public void SwitchToSignalPlace()
        {

        }

        private void save_1_event_Click(object sender, RoutedEventArgs e)
        {
             
            bool en_saved = true;//是否已经存档？
            SaveData.SaveInfo? imp = null;//如果有存档，这是此次存档的信息
            try
            {
                imp = SaveData.save1;
            }
            catch
            {
                en_saved = false;
            }

            if (en_saved)//如果有存档
            {

                var pos = Mouse.GetPosition(save_1_event);
                if (pos.X < (save_1_event.Width / 2.0))
                {
                    imp.Value.imp.Dispose();

                    if (disableSave) return;
                    var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                    var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                    save_1_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                    save_1_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                    while (true)
                    {
                        try
                        {
                            File.Delete("data\\save001.png");
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                   

                    SaveData.save1 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
                    data_sp.Dispose();
                    m_img.Dispose();
                    return;
                }//如果点击按钮的左边，则覆盖存档

                if (alreadyKeep) return;
                alreadyKeep = true;//禁止快速重复点击

                if (MainWindow.theatreMode == null)//如果是从标题进入读档
                {
                    MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter,imp.Value.frames, null);
                    EasyAmal _mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                    {
                        MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                        EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                        mpos.Start(true);/// hide tview
 
                        MainWindow.sldata = null;
                    });
                   _mpos2.Start(true);

                    return;
                }
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview

                    MainWindow.sldata = null;
                });
                mpos2.Start(true);

            }
            else
            {

                if (disableSave) return;
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_1_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_1_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save1 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
                data_sp.Dispose();
                m_img.Dispose();
            }
        }
        private void save_2_event_Click(object sender, RoutedEventArgs e)
        {
            bool en_saved = true;
            SaveData.SaveInfo? imp = null;
            try
            {
                imp = SaveData.save2;
            }
            catch
            {
                en_saved = false;
            }

            if (en_saved)
            {
                var pos = Mouse.GetPosition(save_2_event);
                if (pos.X < (save_2_event.Width / 2.0))
                {
                    imp.Value.imp.Dispose();
                    if (disableSave) return;
                    var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                    var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                    save_2_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                    save_2_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";
               
                    while (true)
                    {
                        try
                        {
                            File.Delete("data\\save002.png");
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }

                    SaveData.save2 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
                    data_sp.Dispose();
                    m_img.Dispose();
                    return;
                }
                if (alreadyKeep) return;
                alreadyKeep = true;
                if (MainWindow.theatreMode == null)
                {
                    MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                    EasyAmal _mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                    {
                        MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                        EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                        mpos.Start(true);/// hide tview

                        MainWindow.sldata = null;
                    });
                    _mpos2.Start(true);
                    return;
                }
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview

                    MainWindow.sldata = null;
                });
                mpos2.Start(true);

            }
            else
            {

                if (disableSave) return;
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_2_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_2_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save2 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_2_content.Text };
                data_sp.Dispose();
                m_img.Dispose();
            }
        }
        private void save_3_event_Click(object sender, RoutedEventArgs e)
        {
            bool en_saved = true;
            SaveData.SaveInfo? imp = null;
            try
            {
                imp = SaveData.save3;
            }
            catch
            {
                en_saved = false;
            }

            if (en_saved)
            {
                var pos = Mouse.GetPosition(save_3_event);
                if (pos.X < (save_1_event.Width / 2.0))
                {
                    imp.Value.imp.Dispose();
                    if (disableSave) return;
                    var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                    var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                    save_3_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                    save_3_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";
                  
                    while (true)
                    {
                        try
                        {
                            File.Delete("data\\save003.png");
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                    SaveData.save3 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
                    data_sp.Dispose();
                    m_img.Dispose();
                    return;
                }
                if (alreadyKeep) return;
                alreadyKeep = true;
                if (MainWindow.theatreMode == null)
                {
                    MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                    EasyAmal _mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                    {
                        MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                        EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                        mpos.Start(true);/// hide tview

                        MainWindow.sldata = null;
                    });
                    _mpos2.Start(true);
                    return;
                }
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview

                    MainWindow.sldata = null;
                });
                mpos2.Start(true);

            }
            else
            {
                if (disableSave) return;
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_3_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_3_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save3 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_3_content.Text };
                data_sp.Dispose();
                m_img.Dispose();
            }
        }
        private void save_4_event_Click(object sender, RoutedEventArgs e)
        {
            bool en_saved = true;
            SaveData.SaveInfo? imp = null;
            try
            {
                imp = SaveData.save4;
            }
            catch
            {
                en_saved = false;
            }

            if (en_saved)
            {
                var pos = Mouse.GetPosition(save_4_event);
                if (pos.X < (save_1_event.Width / 2.0))
                {
                    imp.Value.imp.Dispose();
                    if (disableSave) return;
                    var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                    var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                    save_4_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                    save_4_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";
                    
                    while (true)
                    {
                        try
                        {
                            File.Delete("data\\save004.png");
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(100);
                            continue;
                        }
                    }
                    SaveData.save4 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
                    data_sp.Dispose();
                    m_img.Dispose();
                    return;
                }
                if (alreadyKeep) return;
                alreadyKeep = true;
                if (MainWindow.theatreMode == null)
                {
                    MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                    EasyAmal _mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                    {
                        MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                        EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                        mpos.Start(true);/// hide tview

                        MainWindow.sldata = null;
                    });
                    _mpos2.Start(true);
                    return;
                }
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                MainWindow.theatreMode = MainWindow.switchToSignalTheatre(imp.Value.chapter, imp.Value.frames, null);
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview

                    MainWindow.sldata = null;
                });
                mpos2.Start(true);

            }
            else
            {
                if (disableSave) return;
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_4_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_4_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save4 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_4_content.Text };
                data_sp.Dispose();
                m_img.Dispose();
            }
        }
        bool alreadyKeep = false;

      

    }
}