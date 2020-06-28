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

namespace Shinengine.Surface
{
    /// <summary>
    /// SaveLoad.xaml 的交互逻辑
    /// </summary>
    public partial class SaveLoad : Page
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
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

                imp_save001 = SaveData.save1;
                save_1_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save001.imp));
                save_1_content.Text = imp_save001.comment;

                imp_save003 = SaveData.save3;
                save_3_event.Background = new ImageBrush(BitmapToBitmapSource(imp_save003.imp));
                save_3_content.Text = imp_save003.comment;

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
            bool en_saved = true;
            SaveData.SaveInfo? imp = null;
            try
            {
                imp = SaveData.save1;
            }
            catch
            {
                en_saved = false;
            }

            if (en_saved)
            {
                if (alreadyKeep) return;
                alreadyKeep = true;
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                switch (imp.Value.chapter)
                {
                    case 0:
                        MainWindow.theatreMode = MainWindow.switchToSignalTheatre();
                        MainWindow.theatreMode.m_theatre.SetNextLocatPosition(imp.Value.frames);
                        break;
                    default:
                        throw new Exception("Error: Data Out Of Index");
                }
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                        EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                       mpos.Start(true);/// hide tview
                    MainWindow.theatreMode.Start(Chapter1.Chapter1Script, () =>
                    {
                        MainWindow.title = MainWindow.switchToTitle();
                        MainWindow.m_window.Content = MainWindow.title.Content;

                        MainWindow.theatreMode.m_logo.Dispose();
                        MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                        MainWindow.theatreMode.m_theatre.Exit();

                        if (MainWindow.theatreMode != null)
                            MainWindow.theatreMode = null;
                    });
                    MainWindow.sldata = null;
                });
                mpos2.Start(true);
            }
            else
            {
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_1_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_1_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save1 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_1_content.Text };
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
                if (alreadyKeep) return;
                alreadyKeep = true;
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                switch (imp.Value.chapter)
                {
                    case 0:
                        MainWindow.theatreMode = MainWindow.switchToSignalTheatre();
                        MainWindow.theatreMode.m_theatre.SetNextLocatPosition(imp.Value.frames);
                        break;
                    default:
                        throw new Exception("Error: Data Out Of Index");
                }
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview
                    MainWindow.theatreMode.Start(Chapter1.Chapter1Script, () =>
                    {
                        MainWindow.title = MainWindow.switchToTitle();
                        MainWindow.m_window.Content = MainWindow.title.Content;

                        MainWindow.theatreMode.m_logo.Dispose();
                        MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                        MainWindow.theatreMode.m_theatre.Exit();

                        if (MainWindow.theatreMode != null)
                            MainWindow.theatreMode = null;
                    });
                    MainWindow.sldata = null;
                });
                mpos2.Start(true);
            }
            else
            {
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_2_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_2_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save2 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_2_content.Text };
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
                if (alreadyKeep) return;
                alreadyKeep = true;
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                switch (imp.Value.chapter)
                {
                    case 0:
                        MainWindow.theatreMode = MainWindow.switchToSignalTheatre();
                        MainWindow.theatreMode.m_theatre.SetNextLocatPosition(imp.Value.frames);
                        break;
                    default:
                        throw new Exception("Error: Data Out Of Index");
                }
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview
                    MainWindow.theatreMode.Start(Chapter1.Chapter1Script, () =>
                    {
                        MainWindow.title = MainWindow.switchToTitle();
                        MainWindow.m_window.Content = MainWindow.title.Content;

                        MainWindow.theatreMode.m_logo.Dispose();
                        MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                        MainWindow.theatreMode.m_theatre.Exit();

                        if (MainWindow.theatreMode != null)
                            MainWindow.theatreMode = null;
                    });
                    MainWindow.sldata = null;
                });
                mpos2.Start(true);
            }
            else
            {
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_3_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_3_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save3 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_3_content.Text };
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
                if (alreadyKeep) return;
                alreadyKeep = true;
                MainWindow.theatreMode.m_logo.Dispose();
                MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                MainWindow.theatreMode.m_theatre.Exit();

                switch (imp.Value.chapter)
                {
                    case 0:
                        MainWindow.theatreMode = MainWindow.switchToSignalTheatre();
                        MainWindow.theatreMode.m_theatre.SetNextLocatPosition(imp.Value.frames);
                        break;
                    default:
                        throw new Exception("Error: Data Out Of Index");
                }
                EasyAmal mpos2 = new EasyAmal(this.Forgan, "(Opacity)", 1.0, 0.0, SharedSetting.switchSpeed, (e, c) =>
                {
                    MainWindow.m_window.Content = MainWindow.theatreMode.Content;
                    EasyAmal mpos = new EasyAmal(MainWindow.theatreMode.SBK, "(Opacity)", 0.0, 1.0, SharedSetting.switchSpeed);
                    mpos.Start(true);/// hide tview
                    MainWindow.theatreMode.Start(Chapter1.Chapter1Script, () =>
                    {
                        MainWindow.title = MainWindow.switchToTitle();
                        MainWindow.m_window.Content = MainWindow.title.Content;

                        MainWindow.theatreMode.m_logo.Dispose();
                        MainWindow.theatreMode.m_theatre.SetBackgroundMusic();
                        MainWindow.theatreMode.m_theatre.Exit();

                        if (MainWindow.theatreMode != null)
                            MainWindow.theatreMode = null;
                    });
                    MainWindow.sldata = null;
                });
                mpos2.Start(true);
            }
            else
            {
                var data_sp = _last_draw.Lock(BitmapLockFlags.Read);
                var m_img = new Bitmap(_last_draw.Size.Width, _last_draw.Size.Height, data_sp.Stride, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, data_sp.Data.DataPointer);
                save_4_event.Background = new ImageBrush(BitmapToBitmapSource(m_img));
                save_4_content.Text = "第" + chapter.ToString() + "章 " + "第" + _frame.ToString() + "节";

                SaveData.save4 = new SaveData.SaveInfo() { chapter = this.chapter, frames = _frame, imp = m_img, comment = save_4_content.Text };
            }
        }
        bool alreadyKeep = false;
    }
}
