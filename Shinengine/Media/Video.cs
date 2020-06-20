using FFmpeg.AutoGen;
using SharpDX;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WICBitmap = SharpDX.WIC.Bitmap;

namespace Shinengine.Media
{
    public struct VideoFrame
    {
        public WICBitmap frame;
        public double time_base;
    }

    public struct AudioFrame
    {
        public IntPtr data;
        public double time_base;
    }
    unsafe public class Video
    {

    

        private readonly VideoMode mode;
        public enum VideoMode
        {
            LoadonCatched,
            LoadWithPlaying
        }
        public int nFarm = 0;
        public List<VideoFrame?> bits = new List<VideoFrame?>();
        public List<AudioFrame?> abits = new List<AudioFrame?>();

        public int video_frame_max { get; private set; }
        public int audio_frame_max { get; private set; }
        public bool CanRun { get; set; } = false;
        public int Fps = 0;
        public int out_channels { get; private set; }
        public int out_buffer_size { get; private set; }
        public int out_sample_rate { get; private set; }
        public int out_nb_samples { get; private set; }
        public int bit_per_sample { get; private set; }

        public delegate void CleanUp();
        public event CleanUp EndPlayed;

        public event CleanUp Disposed;
        [DllImport("Kernel32.dll")]
        unsafe extern public static void RtlMoveMemory(void* dst, void* sur, long size);

        unsafe public Video(VideoMode mod,string url)
        {
            mode = mod;
            #region ffmpeg 转码

            #region 转码共通
            // 分配音视频格式上下文
            pFormatContext = ffmpeg.avformat_alloc_context();

            var _pFormatContext = pFormatContext;
            //打开流
            ffmpeg.avformat_open_input(&_pFormatContext, url, null, null).ThrowExceptionIfError();
            // 读取媒体流信息
            ffmpeg.avformat_find_stream_info(pFormatContext, null).ThrowExceptionIfError();
            #endregion


            // 从格式化上下文获取流索引
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

   //编码器ID
            #endregion
       
            if (aStream != null)
            {
                #region 解码（音频）
                var cedecparA = *aStream->codecpar;
                // 根据编码器ID获取对应的解码器
                var pCodec_A = ffmpeg.avcodec_find_decoder(cedecparA.codec_id);

                if (pCodec_A == null) throw new ApplicationException(@"Unsupported codec.");

                pcodecContext_A = ffmpeg.avcodec_alloc_context3(pCodec_A);

                ffmpeg.avcodec_parameters_to_context(pcodecContext_A, &cedecparA);

                ffmpeg.avcodec_open2(pcodecContext_A, pCodec_A, null).ThrowExceptionIfError();

                #endregion
                #region 转码音频
                ulong out_channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
                //nb_samples: AAC-1024 MP3-1152
                out_nb_samples = pcodecContext_A->frame_size;
                bit_per_sample = pcodecContext_A->bits_per_coded_sample;
                AVSampleFormat out_sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;
                out_sample_rate = pcodecContext_A->sample_rate;
                out_channels = ffmpeg.av_get_channel_layout_nb_channels(out_channel_layout);
                //Out Buffer Size
                out_buffer_size = ffmpeg.av_samples_get_buffer_size((int*)0, out_channels, out_nb_samples, out_sample_fmt, 1);

  
                //////////////////////////////////
                long in_channel_layout = ffmpeg.av_get_default_channel_layout(pcodecContext_A->channels);
                //Swr
                au_convert_ctx = ffmpeg.swr_alloc();
                au_convert_ctx = ffmpeg.swr_alloc_set_opts(au_convert_ctx, (long)out_channel_layout, out_sample_fmt, out_sample_rate,
                    in_channel_layout, pcodecContext_A->sample_fmt, pcodecContext_A->sample_rate, 0, (void*)0);
                ffmpeg.swr_init(au_convert_ctx);
                #endregion

            }

            #region 准备解码视频
            var pCodec = ffmpeg.avcodec_find_decoder(pStream->codecpar->codec_id);
            pCodecContext = ffmpeg.avcodec_alloc_context3(pCodec);

            if (pCodec == null) throw new ApplicationException(@"Unsupported codec.");

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
                pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;
            ffmpeg.avcodec_parameters_to_context(pCodecContext, pStream->codecpar);
            // 通过解码器打开解码器上下文:AVCodecContext pCodecContext
            ffmpeg.avcodec_open2(pCodecContext, pCodec, null).ThrowExceptionIfError();
            #endregion
            #region 转码视频
            var width = pStream->codecpar->width;
            var height = pStream->codecpar->height;
            var sourcePixFmt = pCodecContext->pix_fmt;

            var destinationPixFmt = AVPixelFormat.AV_PIX_FMT_BGRA;//目标像素格式

            if (sourcePixFmt == AVPixelFormat.AV_PIX_FMT_NONE && pStream->codecpar->codec_id == AVCodecID.AV_CODEC_ID_H264)
            {
                sourcePixFmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            }
            #region 视频swr
      
            pConvertContext = ffmpeg.sws_getContext(width, height, sourcePixFmt,
                width, height, destinationPixFmt,
                ffmpeg.SWS_FAST_BILINEAR, null, null, null);      // 得到SwsContext对象：用于图像的缩放和转换操作
            if (pConvertContext == null) throw new ApplicationException(@"Could not initialize the conversion context.");

            //分配一个默认的帧对象:AVFrame
            pConvertedFrame = ffmpeg.av_frame_alloc();
            // 目标媒体格式需要的字节长度
            var convertedFrameBufferSize = ffmpeg.av_image_get_buffer_size(destinationPixFmt, width, height, 1);
            // 分配目标媒体格式内存使用
            convertedFrameBufferPtr = Marshal.AllocHGlobal(convertedFrameBufferSize);
            dstData = new byte_ptrArray4();
            dstLinesize = new int_array4();
            // 设置图像填充参数
            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, (byte*)convertedFrameBufferPtr, destinationPixFmt, width, height, 1);
            #endregion
            #endregion

            Fps = (int)Math.Round(((double)pStream->avg_frame_rate.num / (double)pStream->avg_frame_rate.den), 0);
            pPacket = ffmpeg.av_packet_alloc();
        }

        #region 对象定义
        private AVFormatContext* pFormatContext = null;
        private AVCodecContext* pCodecContext = null;
        private AVCodecContext* pcodecContext_A = null;
        private AVStream* pStream = null;
        private AVStream* aStream = null;
        private SwsContext *pConvertContext = null;
        private SwrContext* au_convert_ctx = null;
        private IntPtr convertedFrameBufferPtr;
        private byte_ptrArray4 dstData;
        private int_array4 dstLinesize;

        #endregion
        private AVFrame* pConvertedFrame;

        private AVPacket* pPacket = null;

        [Obsolete]
        public unsafe void Start()
        {
            Debug.WriteLine("Video startup");

            var _pFormatContext = pFormatContext;
            var _au_convert_ctx = au_convert_ctx;
            var _pcodecContext_A = pcodecContext_A;
      


            var frameNumber = 0;
            ImagingFactory mFty = new ImagingFactory();
            var pAudioFrame = ffmpeg.av_frame_alloc();
            var pDecodedFrame = ffmpeg.av_frame_alloc();

            CanRun = true;


            #region ffmpeg 解码

            if (mode == VideoMode.LoadWithPlaying)
            {
                if (aStream == null)
                    throw new Exception("No Audio");
                var Tsk_Video = new Task(() =>
                  {
                      int got_picture = 0;
                      byte* out_buffer = (byte*)Marshal.AllocHGlobal(19200 * 2);
                      while (CanRun)
                      {
                        // 读取一帧未解码数据

                        int error = ffmpeg.av_read_frame(pFormatContext, pPacket);
                          if (error == ffmpeg.AVERROR_EOF) break;
                          error.ThrowExceptionIfError();

                          if (pPacket->stream_index == pStream->index)
                          {
                            // 解码
                            ffmpeg.avcodec_send_packet(pCodecContext, pPacket).ThrowExceptionIfError();
                            // 解码输出解码数据
                            error = ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame);
                              if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN) && CanRun) continue;
                              error.ThrowExceptionIfError();
                              double timeset = ffmpeg.av_frame_get_best_effort_timestamp(pDecodedFrame) * ffmpeg.av_q2d(pStream->time_base);
                            // YUV->RGB
                            ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, pCodecContext->height, dstData, dstLinesize);

                              ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                            ffmpeg.av_frame_unref(pDecodedFrame);//释放解码帧对象引用


                            if (bits.Count == int.MaxValue)
                              {
                                  bits.Clear();
                                  nFarm = 0;
                              }
                              var m_bitLoads = new WICBitmap(mFty, pCodecContext->width, pCodecContext->height, SharpDX.WIC.PixelFormat.Format32bppBGR, new DataRectangle(convertedFrameBufferPtr, dstLinesize[0]));

                              if (m_bitLoads.Size == null)
                                  throw new Exception();

                              bits.Add(new VideoFrame() { frame = m_bitLoads, time_base = timeset });

                              if (bits.Count - nFarm >= 120)
                              {
                                  while (bits.Count - nFarm > 60 && CanRun)
                                      Thread.Sleep(1);
                              };



                              frameNumber++;
                          }

                          if (aStream != null) if (pPacket->stream_index == aStream->index)
                              {
                                  int ret = ffmpeg.avcodec_decode_audio4(pcodecContext_A, pAudioFrame, &got_picture, pPacket);
                                  if (ret < 0)
                                  {

                                      return;
                                  }

                                  double timeset = ffmpeg.av_frame_get_best_effort_timestamp(pAudioFrame) * ffmpeg.av_q2d(aStream->time_base);
                                  if (got_picture > 0)
                                  {
                                      ffmpeg.swr_convert(au_convert_ctx, &out_buffer, 19200, (byte**)&pAudioFrame->data, pAudioFrame->nb_samples);

                                      var mbuf = Marshal.AllocHGlobal(out_buffer_size);

                                      RtlMoveMemory((void*)mbuf, out_buffer, out_buffer_size);
                                      abits.Add(new AudioFrame() { data = mbuf, time_base = timeset });

                                      index++;

                                  }
                                  ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                                ffmpeg.av_frame_unref(pAudioFrame);//释放解码帧对象引用
                                continue;
                              }
                      }
                    #region 资源释放

                    Marshal.FreeHGlobal(convertedFrameBufferPtr);
                      ffmpeg.av_free(pConvertedFrame);
                      ffmpeg.sws_freeContext(pConvertContext);
                      mFty.Dispose();
                      ffmpeg.av_free(pDecodedFrame);
                      ffmpeg.avcodec_close(pCodecContext);
                      var __pFormatContext = pFormatContext;
                      ffmpeg.avformat_close_input(&__pFormatContext);



                      if (aStream != null)
                      {
                          ffmpeg.swr_close(au_convert_ctx);
                          var __au_convert_ctx = au_convert_ctx;
                          ffmpeg.swr_free(&__au_convert_ctx);
                          var __pcodecContext_A = pcodecContext_A;
                          ffmpeg.avcodec_free_context(&__pcodecContext_A);
                      }
                      if (!CanRun)
                          for (int i = nFarm; i < bits.Count; i++)
                          {
                              if ((bool)!bits[i]?.frame.IsDisposed)
                                  bits[i]?.frame.Dispose();
                          }
                      else { entiryPlayed = true; EndPlayed?.Invoke(); }

                      Debug.WriteLine("Video Disposed");

                    #endregion

                });

                Tsk_Video.Start();
                Disposed += () => { Tsk_Video.Wait(); Tsk_Video.Dispose(); };
            }
            else
            {

                while (CanRun)
                {
                    // 读取一帧未解码数据
                    int error = ffmpeg.av_read_frame(pFormatContext, pPacket);
                    if (error == ffmpeg.AVERROR_EOF) break;
                    error.ThrowExceptionIfError();
                    if (pPacket->stream_index == pStream->index)
                    {
                        // 解码
                        error = ffmpeg.avcodec_send_packet(pCodecContext, pPacket);
                        if (error < 0) throw new ApplicationException(GetErrorMessage(error));
                        // 解码输出解码数据
                        error = ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame);

                        if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN) && CanRun) continue;
                        if (error < 0) throw new ApplicationException(GetErrorMessage(error));
                        double timeset = ffmpeg.av_frame_get_best_effort_timestamp(pDecodedFrame) * ffmpeg.av_q2d(pStream->time_base);
                        // YUV->RGB
                        ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, pCodecContext->height, dstData, dstLinesize);

                        ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                        ffmpeg.av_frame_unref(pDecodedFrame);//释放解码帧对象引用

                        var m_bitLoads = new WICBitmap(mFty, pCodecContext->width, pCodecContext->height, SharpDX.WIC.PixelFormat.Format32bppBGR, new DataRectangle(convertedFrameBufferPtr, dstLinesize[0]));

                        if (m_bitLoads.Size == null)
                            throw new Exception();

                        bits.Add(new VideoFrame() { frame = m_bitLoads, time_base = timeset });
                        Console.WriteLine("Using Video");
                        frameNumber++;
                    }
                    if (aStream != null) if (pPacket->stream_index == aStream->index)
                        {
                            ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                            continue;
                        }
                }



                Marshal.FreeHGlobal(convertedFrameBufferPtr);
                ffmpeg.av_free(pConvertedFrame);
                ffmpeg.sws_freeContext(pConvertContext);
                mFty.Dispose();
                ffmpeg.av_free(pDecodedFrame);
                ffmpeg.avcodec_close(pCodecContext);
                ffmpeg.avformat_close_input(&_pFormatContext);

                if (aStream != null)
                {
                    ffmpeg.swr_close(au_convert_ctx);
                    ffmpeg.swr_free(&_au_convert_ctx);
                    ffmpeg.avcodec_free_context(&_pcodecContext_A);
                }
                Debug.WriteLine("Porpare over");
            }
            #endregion
        }
        public bool entiryPlayed { get; private set; } = false;
        private int index = 0;


        public void Dispose()
        {
            if (mode == VideoMode.LoadWithPlaying)
            {
                if (!entiryPlayed)
                    CanRun = false;
                else for (int i = nFarm; i < bits.Count; i++)
                    {
                        if ((bool)!bits[i]?.frame.IsDisposed)
                            bits[i]?.frame.Dispose();
                    }
            }
            else
            {
                for (int i = 0; i < bits.Count; i++)
                {
                    if ((bool)!bits[i]?.frame.IsDisposed)
                        bits[i]?.frame.Dispose();
                }

                Debug.WriteLine("Video Disposed");
            }

            Disposed?.Invoke();
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
