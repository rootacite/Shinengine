using FFmpeg.AutoGen;
using SharpDX;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WICBitmap = SharpDX.WIC.Bitmap;

namespace Shinengine
{
    class Video
    {
        long frameNumber = 0;
        public int nFarm = 0;
        public List<WICBitmap> bits = new List<WICBitmap>();
        [DllImport("Shinehelper.dll")]
        extern public static void waveInit(short nFormat, short nChannels, int nSamPer, int bitRate, int Bitsample);
        [DllImport("Shinehelper.dll")]
        unsafe extern public static void waveWriteBuffer(void* lpData, int size);
        public bool CanRun { get;  set; }

        [Obsolete]
        public unsafe void Start(string url)
        {
            Debug.WriteLine("Video startup");
            CanRun = true;
            ImagingFactory mFty = new ImagingFactory();
            //FFmpegDLL目录查找和设置
            #region ffmpeg 初始化
            // 初始化注册ffmpeg相关的编码器
            ffmpeg.av_register_all();
            ffmpeg.avcodec_register_all();
            ffmpeg.avformat_network_init();
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

            // 从格式化上下文获取流索引
            AVStream* pStream = null, aStream = null;
            for (var i = 0; i < pFormatContext->nb_streams; i++)
            {
                if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    pStream = pFormatContext->streams[i];

                }
             //   else if (pFormatContext->streams[i]->codec->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
              //  {
              //      aStream = pFormatContext->streams[i];

              //  }
            }
            if (pStream == null) throw new ApplicationException(@"Could not found video stream.");
          //  if (aStream == null) throw new ApplicationException(@"Could not found audio stream.");

            // 获取流的编码器上下文
            var codecContext = *pStream->codec;
       //     var codecContext_A = *aStream->codec;
            // 获取图像的宽、高及像素格式
            var width = codecContext.width;
            var height = codecContext.height;
            var sourcePixFmt = codecContext.pix_fmt;
            //  MessageBox.Show (codecContext.pts_correction_num_faulty_pts.ToString());

            // 得到编码器ID
            var codecId = codecContext.codec_id;
         //   var codecId_A = codecContext_A.codec_id;
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

            #region ffmpeg 解码初始化
            // 根据编码器ID获取对应的解码器
            var pCodec = ffmpeg.avcodec_find_decoder(codecId);
          //  var pCodec_A = ffmpeg.avcodec_find_decoder(codecId_A);

            if (pCodec == null ) throw new ApplicationException(@"Unsupported codec.");

            var pCodecContext = &codecContext;
          //  var pcodecContext_A = &codecContext_A;

            if ((pCodec->capabilities & ffmpeg.AV_CODEC_CAP_TRUNCATED) == ffmpeg.AV_CODEC_CAP_TRUNCATED)
                pCodecContext->flags |= ffmpeg.AV_CODEC_FLAG_TRUNCATED;

            // 通过解码器打开解码器上下文:AVCodecContext pCodecContext
            error = ffmpeg.avcodec_open2(pCodecContext, pCodec, null);
            if (error < 0) throw new ApplicationException(GetErrorMessage(error));

        //    error = ffmpeg.avcodec_open2(pcodecContext_A, pCodec_A, null);
            if (error < 0) throw new ApplicationException(GetErrorMessage(error));

            // 分配解码帧对象：AVFrame pDecodedFrame
            var pDecodedFrame = ffmpeg.av_frame_alloc();

            // 初始化媒体数据包
            var packet = new AVPacket();
            var pPacket = &packet;
            ffmpeg.av_init_packet(pPacket);

         //   var packetA = new AVPacket();
         //   var pPacketA = &packetA;
          //  ffmpeg.av_init_packet(pPacketA);


            frameNumber = 0;

            #endregion

            /*    #region 音频初始化

                ulong out_channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
                //AAC:1024  MP3:1152
                int out_nb_samples = pcodecContext_A->frame_size;
                AVSampleFormat out_sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;
                int out_sample_rate = 44100;
                int out_channels = ffmpeg.av_get_channel_layout_nb_channels(out_channel_layout);
                //Out Buffer Size
                int out_buffer_size = ffmpeg.av_samples_get_buffer_size((int*)0, out_channels, out_nb_samples, out_sample_fmt, 1);

                byte_ptrArray8* out_buffer = (byte_ptrArray8*)Marshal.AllocHGlobal(192000 * 2);

                var pFarme = ffmpeg.av_frame_alloc();

                int got_picture = 0;

                int index = 0;
                //FIX:Some Codec's Context Information is missing
                long in_channel_layout = ffmpeg.av_get_default_channel_layout(pcodecContext_A->channels);
                //Swr
                // struct SwrContext *au_convert_ctx;
                var au_convert_ctx = ffmpeg.swr_alloc();
                au_convert_ctx = ffmpeg.swr_alloc_set_opts(au_convert_ctx, (long)out_channel_layout, out_sample_fmt, out_sample_rate,
                in_channel_layout, pcodecContext_A->sample_fmt, pcodecContext_A->sample_rate, 0, (void*)0);
                ffmpeg.swr_init(au_convert_ctx);

                waveInit((short)AVSampleFormat.AV_SAMPLE_FMT_S16, (short)out_channels, out_sample_rate, (int)pcodecContext_A->bit_rate, pcodecContext_A->bits_per_coded_sample);


                #endregion */

            #region ffmpeg 解码
            while (CanRun)
            {
                // 读取一帧未解码数据
                error = ffmpeg.av_read_frame(pFormatContext, pPacket);
                if (error == ffmpeg.AVERROR_EOF) break;
                if (error < 0) throw new ApplicationException(GetErrorMessage(error));

                if (pPacket->stream_index == pStream->index)
                {
                    // 解码
                    error = ffmpeg.avcodec_send_packet(pCodecContext, pPacket);
                    if (error < 0) throw new ApplicationException(GetErrorMessage(error));
                    // 解码输出解码数据
                    error = ffmpeg.avcodec_receive_frame(pCodecContext, pDecodedFrame);

                    if (error == ffmpeg.AVERROR(ffmpeg.EAGAIN) && CanRun) continue;
                    if (error < 0) throw new ApplicationException(GetErrorMessage(error));
                    // YUV->RGB
                    ffmpeg.sws_scale(pConvertContext, pDecodedFrame->data, pDecodedFrame->linesize, 0, height, dstData, dstLinesize);

                    ffmpeg.av_packet_unref(pPacket);//释放数据包对象引用
                    ffmpeg.av_frame_unref(pDecodedFrame);//释放解码帧对象引用


                    // 封装Bitmap图片
                    //   var bitmap = new System.Drawing.Bitmap(width, height, dstLinesize[0], System.Drawing.Imaging.PixelFormat.Format24bppRgb, convertedFrameBufferPtr);
                    // 回调
                    if (bits.Count == int.MaxValue)
                    {
                        bits.Clear();
                        nFarm = 0;
                    }
                    bits.Add(new WICBitmap(mFty, width, height, SharpDX.WIC.PixelFormat.Format32bppBGR, new DataRectangle(convertedFrameBufferPtr, dstLinesize[0])));

                    if (bits.Count - nFarm >= 150)
                    {
                        while (bits.Count - nFarm > 90&&CanRun)
                            Thread.Sleep(1);
                    };



                    //bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + "\\264\\frame.buffer."+ frameNumber + ".jpg", ImageFormat.Jpeg);

                    frameNumber++;
                }

         //       if (pPacket->stream_index == aStream->index)
           //     {
                  //  error = ffmpeg.avcodec_decode_audio4(pcodecContext_A, pFarme, &got_picture, pPacketA);
                  //  if (error < 0)
                   // {
                  //      throw new ApplicationException(GetErrorMessage(error));
                  //  }
                  //
                  //  if (got_picture > 0)
                   // {
                   //     ffmpeg.swr_convert(au_convert_ctx, (byte**)&out_buffer, 192000, (byte**)&pFarme->data, pFarme->nb_samples);
//
                  //      index++;
                  //  }
//
                  //  for (int i = 0; i < out_buffer_size; i++)
                  ///  {
                  //     //Console.Write("123");
                   //    // waveWriteBuffer(out_buffer, out_buffer_size);
                   // }
                   // ffmpeg.av_free_packet(pPacketA);
                   // continue;
           //     }
            }
            //播放完置空播放图片 
            //    MessageBox.Show("finish");

            #endregion

            #region 释放资源

         //   ffmpeg.swr_free(&au_convert_ctx);
            Marshal.FreeHGlobal(convertedFrameBufferPtr);
            ffmpeg.av_free(pConvertedFrame);
            ffmpeg.sws_freeContext(pConvertContext);
            mFty.Dispose();
            ffmpeg.av_free(pDecodedFrame);
            ffmpeg.avcodec_close(pCodecContext);
            ffmpeg.avformat_close_input(&pFormatContext);

            for (int i = nFarm; i < bits.Count; i++)
            {
                if (!bits[i].IsDisposed)
                    bits[i].Dispose();
            }

            Debug.WriteLine("Video Disposed");
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
