using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SharpDX.DirectWrite;
using System.Windows.Threading;

using PInvoke;
using System.Runtime.InteropServices;

using FFmpeg;
using FFmpeg.AutoGen;
using System.Drawing;


using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.WIC;

using WICBitmap = SharpDX.WIC.Bitmap;
using D2DFactory = SharpDX.Direct2D1.Factory;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using Image = System.Windows.Controls.Image;

using System.Windows.Media.Imaging;

using System.Windows.Media;
namespace Shinengine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Direct2DImage DxBkGround = null;
        bool enClose = false;
        private System.Windows.Point _Position;
        private int time=0;
        private bool draws=true;

        List<WICBitmap> bits = new List<WICBitmap>();

        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        public SharpDX.DirectWrite.Factory DwoR { get; private set; }
        public TextFormat TF { get; private set; }
        public bool CanRun { get; private set; } = true;
        int nFarm = 0;
        public void mth() { while (true) { time++; Thread.Sleep(1); } }
        public MainWindow()
        {
           
            InitializeComponent();

            timeBeginPeriod(1);
          //  Random aRm = new Random();
            Loaded += (s, e) =>
            {

                DxBkGround = new Direct2DImage(BackGround, 30, (view) =>
                {
                    SharpDX.Direct2D1.Bitmap farme = null;
                    if (bits.Count- nFarm < 60)
                        return false;
                    try
                    {
                        farme = SharpDX.Direct2D1.Bitmap.FromWicBitmap(view, bits[nFarm]);
                    }
                    catch
                    {
                        return false;
                    }
                    view.BeginDraw();


                    view.DrawBitmap(farme,
                        new RawRectangleF(0, 0, DxBkGround.Width, DxBkGround.Height),
                        1, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
                        new RawRectangleF(0, 0, bits[nFarm].Size.Width, bits[nFarm].Size.Height));
                    view.EndDraw();
                    farme.Dispose();
                    bits[nFarm].Dispose();
                    nFarm++;
                    return draws;
                });
                new Thread(() =>
                {
                    while (!enClose)
                    {

                        Dispatcher.Invoke(new Action(() => { this.Title = DxBkGround.Dpis.ToString(); }));
                        Thread.Sleep(1000);
                    }
                }).Start();
               
                new Thread(() =>
                {
                    while (true) {
                        Start("D:\\Restore\\希EDムービー.wmv");
                        while (bits.Count - nFarm > 60)
                            Thread.Sleep(1);
                    }
                }).Start();
                return;
                //  m_Dipter.Start();
            };

        }

        private void BackGround_MouseUp(object sender, MouseButtonEventArgs e)
        {
            draws = !draws;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timeEndPeriod(10);
            Kernel32.TerminateProcess(Kernel32.GetCurrentProcess().DangerousGetHandle(), -1); ;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
           // _Position = e.GetPosition(this);
        }

        public unsafe void Start(string url)
        {
            CanRun = true;
            ImagingFactory mFty = new ImagingFactory();

            Console.WriteLine(@"Current directory: " + Environment.CurrentDirectory);
            Console.WriteLine(@"Runnung in {0}-bit mode.", Environment.Is64BitProcess ? @"64" : @"32");
            //FFmpegDLL目录查找和设置
            FFmpegBinariesHelper.RegisterFFmpegBinaries();

            #region ffmpeg 初始化
            // 初始化注册ffmpeg相关的编码器
            ffmpeg.av_register_all();
            ffmpeg.avcodec_register_all();
            ffmpeg.avformat_network_init();

            Console.WriteLine($"FFmpeg version info: {ffmpeg.av_version_info()}");
            #endregion

            #region ffmpeg 日志
            // 设置记录ffmpeg日志级别
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_VERBOSE);
            av_log_set_callback_callback logCallback = (p0, level, format, vl) =>
            {
                if (level > ffmpeg.av_log_get_level()) return;

                var lineSize = 1024;
                var lineBuffer = stackalloc byte[lineSize];
                var printPrefix = 1;
                ffmpeg.av_log_format_line(p0, level, format, vl, lineBuffer, lineSize, &printPrefix);
                var line = Marshal.PtrToStringAnsi((IntPtr)lineBuffer);
                Console.Write(line);
            };
            ffmpeg.av_log_set_callback(logCallback);

            #endregion

            #region ffmpeg 转码


            // 分配音视频格式上下文
            var pFormatContext = ffmpeg.avformat_alloc_context();

            int error;

            //打开流
            error = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
            if (error != 0) throw new ApplicationException(GetErrorMessage(error));

            // 读取媒体流信息
            error = ffmpeg.avformat_find_stream_info(pFormatContext, null);
            if (error != 0) throw new ApplicationException(GetErrorMessage(error));

            // 这里只是为了打印些视频参数
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                Console.WriteLine($"{key} = {value}");
            }

            // 从格式化上下文获取流索引
            AVStream* pStream = null, aStream;
            for (var i = 0; i < pFormatContext->nb_streams; i++)
            {
                if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    pStream = pFormatContext->streams[i];

                }
                else if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    aStream = pFormatContext->streams[i];

                }
            }
            if (pStream == null) throw new ApplicationException(@"Could not found video stream.");

            // 获取流的编码器上下文
            var codecContext = *pStream->codec;

            Console.WriteLine($"codec name: {ffmpeg.avcodec_get_name(codecContext.codec_id)}");
            // 获取图像的宽、高及像素格式
            var width = codecContext.width;
            var height = codecContext.height;
            var sourcePixFmt = codecContext.pix_fmt;
         //  MessageBox.Show (codecContext.pts_correction_num_faulty_pts.ToString());

            // 得到编码器ID
            var codecId = codecContext.codec_id;
            // 目标像素格式
            var destinationPixFmt = AVPixelFormat.AV_PIX_FMT_BGRA;


            // 某些264格式codecContext.pix_fmt获取到的格式是AV_PIX_FMT_NONE 统一都认为是YUV420P
            if (sourcePixFmt == AVPixelFormat.AV_PIX_FMT_NONE && codecId == AVCodecID.AV_CODEC_ID_H264)
            {
                sourcePixFmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            }

            // 得到SwsContext对象：用于图像的缩放和转换操作
            var pConvertContext = ffmpeg.sws_getContext(width, height, sourcePixFmt,
                width, height, destinationPixFmt,
                ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (pConvertContext == null) throw new ApplicationException(@"Could not initialize the conversion context.");

            //分配一个默认的帧对象:AVFrame
            var pConvertedFrame = ffmpeg.av_frame_alloc();
            // 目标媒体格式需要的字节长度
            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixFmt, width, height, 1);
            // 分配目标媒体格式内存使用
            var convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            var dstData = new byte_ptrArray4();
            var dstLinesize = new int_array4();
            // 设置图像填充参数
            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, (byte*)convertedFrameBufferPtr, destinationPixFmt, width, height, 1);

            #endregion

            #region ffmpeg 解码
            // 根据编码器ID获取对应的解码器
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
            if (pCodec == null) throw new ApplicationException(@"Unsupported codec.");

            var pCodecContext = &codecContext;

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
                pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;

            // 通过解码器打开解码器上下文:AVCodecContext pCodecContext
            error = ffmpeg.avcodec_open2(pCodecContext, pCodec, null);
            if (error < 0) throw new ApplicationException(GetErrorMessage(error));

            // 分配解码帧对象：AVFrame pDecodedFrame
            var pDecodedFrame = ffmpeg.av_frame_alloc();

            // 初始化媒体数据包
            var packet = new AVPacket();
            var pPacket = &packet;
            ffmpeg.av_init_packet(pPacket);

            var frameNumber = 0;
            while (CanRun)
            {
                try
                {
                    do
                    {
                        // 读取一帧未解码数据
                        error = ffmpeg.av_read_frame(pFormatContext, pPacket);
                        Console.WriteLine(pPacket->dts);
                        if (error == ffmpeg.AVERROR_EOF) break;
                        if (error < 0) throw new ApplicationException(GetErrorMessage(error));

                        if (pPacket->stream_index != pStream->index) continue;

                        // 解码
                        error = ffmpeg.avcodec_send_packet(pCodecContext, pPacket);
                        if (error < 0) throw new ApplicationException(GetErrorMessage(error));
                        // 解码输出解码数据
                        error = ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame);
                    } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN) && CanRun);
                    if (error == ffmpeg.AVERROR_EOF) break;
                    if (error < 0) throw new ApplicationException(GetErrorMessage(error));

                    if (pPacket->stream_index != pStream->index) continue;

                    Console.WriteLine($@"frame: {frameNumber}");
                    // YUV->RGB
                    ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, height, dstData, dstLinesize);
                }
                finally
                {
                    ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                    ffmpeg.av_frame_unref(pDecodedFrame);//释放解码帧对象引用
                }

                // 封装Bitmap图片
                //   var bitmap = new System.Drawing.Bitmap(width, height, dstLinesize[0], System.Drawing.Imaging.PixelFormat.Format24bppRgb, convertedFrameBufferPtr);
                // 回调
                if (bits.Count == int.MaxValue)
                {
                    bits.Clear();
                    nFarm = 0;
                }
                bits.Add(new WICBitmap(mFty, width, height,SharpDX.WIC.PixelFormat.Format32bppBGR,new DataRectangle(convertedFrameBufferPtr, dstLinesize[0])));

                if (bits.Count- nFarm >= 150)
                {
                    while (bits.Count- nFarm > 90)
                        Thread.Sleep(1);
                };

                

                //bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + "\\264\\frame.buffer."+ frameNumber + ".jpg", ImageFormat.Jpeg);

                frameNumber++;
            }
            //播放完置空播放图片 
        //    MessageBox.Show("finish");

            #endregion

            #region 释放资源
            Marshal.FreeHGlobal(convertedFrameBufferPtr);
            ffmpeg.av_free(pConvertedFrame);
            ffmpeg.sws_freeContext(pConvertContext);

            ffmpeg.av_free(pDecodedFrame);
            ffmpeg.avcodec_close(pCodecContext);
            ffmpeg.avformat_close_input(&pFormatContext);


            #endregion
        }

        private static unsafe string GetErrorMessage(int error)
        {
            var bufferSize = 1024;
            var buffer = stackalloc byte[bufferSize];
            ffmpeg.av_strerror(error, buffer, (ulong)bufferSize);
            var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
            return message;
        }
    }
}
