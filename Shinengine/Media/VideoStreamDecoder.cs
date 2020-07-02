using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using WICBitmap = SharpDX.WIC.Bitmap;
using FFmpeg;
using FFmpeg.AutoGen;
using SharpDX;
using Shinengine.Data;

namespace Shinengine.Media
{
    public sealed unsafe class VideoStreamDecoder : IDisposable
    {
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly int _streamIndex;
        private readonly AVFrame* _pFrame;

        public SwsContext* PConvertContext { get; }

        private readonly AVFrame* _receivedFrame;
        private readonly AVPacket* _pPacket;
        private string source = null;
        public VideoStreamDecoder(string url, AVHWDeviceType HWDeviceType = AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
        {
            source = PackStream.Locate(url);
            _pFormatContext = ffmpeg.avformat_alloc_context();
            _receivedFrame = ffmpeg.av_frame_alloc();
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_open_input(&pFormatContext, source, null, null).ThrowExceptionIfError();
            ffmpeg.avformat_find_stream_info(_pFormatContext, null).ThrowExceptionIfError();
            AVCodec* codec = null;
            _streamIndex = ffmpeg.av_find_best_stream(_pFormatContext, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, &codec, 0).ThrowExceptionIfError();
            _pCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (HWDeviceType != AVHWDeviceType.AV_HWDEVICE_TYPE_NONE)
            {
                ffmpeg.av_hwdevice_ctx_create(&_pCodecContext->hw_device_ctx, HWDeviceType, null, null, 0).ThrowExceptionIfError();
            }
            ffmpeg.avcodec_parameters_to_context(_pCodecContext, _pFormatContext->streams[_streamIndex]->codecpar).ThrowExceptionIfError();
            ffmpeg.avcodec_open2(_pCodecContext, codec, null).ThrowExceptionIfError();

            CodecName = ffmpeg.avcodec_get_name(codec->id);
            FrameSize = new Size(_pCodecContext->width, _pCodecContext->height);
            PixelFormat = _pCodecContext->pix_fmt;

            _pPacket = ffmpeg.av_packet_alloc();
            _pFrame = ffmpeg.av_frame_alloc();

            PConvertContext = ffmpeg.sws_getContext(_pCodecContext->width, _pCodecContext->height, _pCodecContext->pix_fmt,
              _pCodecContext->width, _pCodecContext->height, AVPixelFormat.AV_PIX_FMT_BGRA,
              ffmpeg.SWS_FAST_BILINEAR, null, null, null);

            if (PConvertContext == null) throw new ApplicationException(@"Could not initialize the conversion context.");

            dstData = new byte_ptrArray4();
            dstLinesize = new int_array4();
            convertedFrameBufferPtr = Marshal.AllocHGlobal(ffmpeg.av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_BGRA, _pCodecContext->width, _pCodecContext->height, 1));
            ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize,
                (byte*)convertedFrameBufferPtr,
                AVPixelFormat.AV_PIX_FMT_BGRA, _pCodecContext->width, _pCodecContext->height, 1);
        }

        public string CodecName { get; }
        public Size FrameSize { get; }
        public AVPixelFormat PixelFormat { get; }
        public byte_ptrArray4 dstData;
        public int_array4 dstLinesize;
        readonly private IntPtr convertedFrameBufferPtr;

        public void Dispose()
        {
            Marshal.FreeHGlobal(convertedFrameBufferPtr);

            ffmpeg.sws_freeContext(PConvertContext);
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_free(_pFrame);

            ffmpeg.av_packet_unref(_pPacket);
            ffmpeg.av_free(_pPacket);

            ffmpeg.avcodec_close(_pCodecContext);
            var pFormatContext = _pFormatContext;
            ffmpeg.avformat_close_input(&pFormatContext);
            try
            {
                File.Delete(source);
            }
            catch
            {

            }
        }

        public bool TryDecodeNextFrame(out IntPtr data, out int pitch)
        {
            ffmpeg.av_frame_unref(_pFrame);
            ffmpeg.av_frame_unref(_receivedFrame);
            int error;
            do
            {
                try
                {
                    do
                    {
                        error = ffmpeg.av_read_frame(_pFormatContext, _pPacket);
                        if (error == ffmpeg.AVERROR_EOF)
                        {
                            data = (IntPtr)0;
                            pitch = 0;
                            return false;
                        }

                        error.ThrowExceptionIfError();
                    } while (_pPacket->stream_index != _streamIndex);

                    ffmpeg.avcodec_send_packet(_pCodecContext, _pPacket).ThrowExceptionIfError();
                }
                finally
                {
                    ffmpeg.av_packet_unref(_pPacket);
                }

                error = ffmpeg.avcodec_receive_frame(_pCodecContext, _pFrame);
            } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            error.ThrowExceptionIfError();




            if (_pCodecContext->hw_device_ctx != null)
            {
                ffmpeg.av_hwframe_transfer_data(_receivedFrame, _pFrame, 0).ThrowExceptionIfError();
                ffmpeg.sws_scale(PConvertContext, _receivedFrame->data, _receivedFrame->linesize, 0, _pCodecContext->height, dstData, dstLinesize);
                data = convertedFrameBufferPtr;
                pitch = dstLinesize[0];
            }
            else
            {
                ffmpeg.sws_scale(PConvertContext, _pFrame->data, _pFrame->linesize, 0, _pCodecContext->height, dstData, dstLinesize);
                data = convertedFrameBufferPtr;
                pitch = dstLinesize[0];
            }
            return true;
        }

        public IReadOnlyDictionary<string, string> GetContextInfo()
        {
            AVDictionaryEntry* tag = null;
            var result = new Dictionary<string, string>();
            while ((tag = ffmpeg.av_dict_get(_pFormatContext->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                var key = Marshal.PtrToStringAnsi((IntPtr)tag->key);
                var value = Marshal.PtrToStringAnsi((IntPtr)tag->value);
                result.Add(key, value);
            }

            return result;
        }

        public void Position(long pos)
        {
            ffmpeg.av_seek_frame(_pFormatContext, _streamIndex, pos, ffmpeg.AVSEEK_FLAG_BACKWARD);
        }
    }
    public static class Extension   //  必须是一个静态类
    {
        public static int ThrowExceptionIfError(this int value)    //必须为public static 类型，且参数使用this关键字
        {
            if (value < 0) throw new Exception("应用发生错误");
            else
            {
                return value;
            }
        }
    }
}

