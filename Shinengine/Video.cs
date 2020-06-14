using FFmpeg.AutoGen;
using SharpDX;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using WICBitmap = SharpDX.WIC.Bitmap;

namespace Shinengine
{
    class Video
    {
        public int nFarm = 0;
        public List<WICBitmap> bits = new List<WICBitmap>();
        [DllImport("Shinehelper.dll")]
        extern public static void waveInit(short nFormat, short nChannels, int nSamPer, int bitRate, int Bitsample);
        [DllImport("Shinehelper.dll")]
        unsafe extern public static void waveWriteBuffer(void* lpData, int size);
        bool CanRun { get; set; }

        [Obsolete]
        public unsafe void Start(string url)
        {
            int index = 0;
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

            #region 转码共通
            // 分配音视频格式上下文
            var pFormatContext = ffmpeg.avformat_alloc_context();

            int error;
            //打开流
            error = ffmpeg.avformat_open_input(&pFormatContext, url, null, null);
            if (error != 0) throw new ApplicationException(GetErrorMessage(error));
            // 读取媒体流信息
            error = ffmpeg.avformat_find_stream_info(pFormatContext, null);
            if (error != 0) throw new ApplicationException(GetErrorMessage(error));
            #endregion


            // 从格式化上下文获取流索引
            AVStream* pStream = null, aStream = null;
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
            if (aStream == null) throw new ApplicationException(@"Could not found audio stream.");

            // 获取流的编码器上下文
            var codecContext = *pStream->codec;
            var cedecparA = *aStream->codecpar;

            var codecId = codecContext.codec_id;

            #region 转码（与音频无关）
            // 获取图像的宽、高及像素格式
            var width = codecContext.width;
            var height = codecContext.height;
            var sourcePixFmt = codecContext.pix_fmt;
            //  MessageBox.Show (codecContext.pts_correction_num_faulty_pts.ToString());

            // 得到编码器ID

            // 目标像素格式
            var destinationPixFmt = AVPixelFormat.AV_PIX_FMT_BGRA;

            // 某些264格式codecContext.pix_fmt获取到的格式是AV_PIX_FMT_NONE 统一都认为是YUV420P
            if (sourcePixFmt == AVPixelFormat.AV_PIX_FMT_NONE && codecId == AVCodecID.AV_CODEC_ID_H264)
            {
                sourcePixFmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            }
            #region 视频swr
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
            #endregion

            #endregion

            #region ffmpeg 解码初始化

            // 根据编码器ID获取对应的解码器
            var pCodec_A = ffmpeg.avcodec_find_decoder(cedecparA.codec_id);

            if (pCodec_A == null) throw new ApplicationException(@"Unsupported codec.");

            var pcodecContext_A = ffmpeg.avcodec_alloc_context3(pCodec_A);
            ffmpeg.avcodec_parameters_to_context(pcodecContext_A, &cedecparA);

            error = ffmpeg.avcodec_open2(pcodecContext_A, pCodec_A, null);
            if (error < 0) throw new ApplicationException(GetErrorMessage(error));


          
            #region 解码（视频）
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


            var frameNumber = 0;
            #endregion
            // 初始化媒体数据包
            var packet = new AVPacket();
            var pPacket = &packet;
            ffmpeg.av_init_packet(pPacket);
            #endregion

            #region 音频数据初始化

            long out_ch_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
            AVSampleFormat out_sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;
            int out_sample_rate = 44100;
            long in_ch_layout = (long)cedecparA.channel_layout;
            AVSampleFormat in_sample_fmt = pcodecContext_A->sample_fmt;
            int in_sample_rate = pcodecContext_A->sample_rate;

            var pSwrContext = ffmpeg.swr_alloc_set_opts((SwrContext*)0, out_ch_layout, out_sample_fmt,
                                               out_sample_rate, in_ch_layout,
                                               in_sample_fmt, in_sample_rate, 0, (void*)0);

            error = ffmpeg.swr_init(pSwrContext);
            if (error < 0) throw new ApplicationException(GetErrorMessage(error));

            int outChannels = ffmpeg.av_get_channel_layout_nb_channels((ulong)out_ch_layout);


            int dataSize = ffmpeg.av_samples_get_buffer_size((int*)0, outChannels,
                                                      cedecparA.frame_size,
                                                      out_sample_fmt, 0);

            int out_sample_fmt_track;
            if (out_sample_fmt == AVSampleFormat.AV_SAMPLE_FMT_U8)
            {
                out_sample_fmt_track = 3;
            }
            else
            {
                out_sample_fmt_track = 2;
            }

            byte* resampleOutBuffer = (byte*)Marshal.AllocHGlobal(dataSize);
            AVFrame* pFrame = ffmpeg.av_frame_alloc();
            waveInit((short)out_sample_fmt_track, (short)outChannels, out_sample_rate, (int)pcodecContext_A->bit_rate, pcodecContext_A->bits_per_coded_sample);
            #endregion

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
                        while (bits.Count - nFarm > 90)
                            Thread.Sleep(1);
                    };



                    //bitmap.Save(AppDomain.CurrentDomain.BaseDirectory + "\\264\\frame.buffer."+ frameNumber + ".jpg", ImageFormat.Jpeg);

                    frameNumber++;
                }

                if (pPacket->stream_index == aStream->index)
                {
                    if (ffmpeg.avcodec_send_packet(pcodecContext_A, &packet) == 0)
                    {

                        if (ffmpeg.avcodec_receive_frame(pcodecContext_A, pFrame) == 0)
                        {
                            //解码数据
                            index++;
                            byte* []ArrData = pFrame->data.ToArray();
                            byte** byteCont = (byte**)Marshal.AllocHGlobal(sizeof(byte*)*ArrData.Length);

                            int t = 0;
                            foreach (byte* i in ArrData)
                            {
                                *(byteCont + t) = i;
                                t++;
                            }
                            ffmpeg.swr_convert(pSwrContext, &resampleOutBuffer, pFrame->nb_samples,
                                      byteCont,
                                pFrame->nb_samples);

                            waveWriteBuffer(resampleOutBuffer, dataSize);

                            Marshal.FreeHGlobal((IntPtr)byteCont);
                        }

                    }
                    ffmpeg.av_packet_unref(&packet);//释放数据包对象引用
                    ffmpeg.av_frame_unref(pFrame);//释放解码帧对象引用
                    continue;
                }
            }
            //播放完置空播放图片 
            //    MessageBox.Show("finish");

            #endregion

            #region 释放资源

            Marshal.FreeHGlobal(convertedFrameBufferPtr);
            ffmpeg.av_free(pConvertedFrame);
            ffmpeg.sws_freeContext(pConvertContext);
            mFty.Dispose();
            ffmpeg.av_free(pDecodedFrame);
            ffmpeg.avcodec_close(pCodecContext);
            ffmpeg.avformat_close_input(&pFormatContext);

            ffmpeg.av_frame_free(&pFrame);

            ffmpeg.av_frame_free(&pFrame);
            ffmpeg.swr_close(pSwrContext);
            ffmpeg.swr_free(&pSwrContext);
            ffmpeg.avcodec_free_context(&pcodecContext_A);
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
