using NAudio.Wave;
using Shinengine.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Shinengine.Media
{
    public class AudioPlayer
    {
        readonly Thread PlayThread = null;
        public bool canplay = true;
        public WaveOutEvent outputDevice;

        public AudioPlayer(string url, bool loop = false, float? volum = null)
        {
            if (volum == null)
                volum = SharedSetting.BGMVolum;
            PlayThread = new Thread(() =>
          {
              string path_rele = PackStream.Locate(url);
              var audioFile = new AudioFileReader(path_rele);
              outputDevice = new WaveOutEvent();
              outputDevice.Init(audioFile);
              outputDevice.Volume = (float)volum;
              do
              {
                  outputDevice.Play(); // 异步执行
                     while (outputDevice.PlaybackState == PlaybackState.Playing)
                  {
                      Thread.Sleep(100);
                      if (!canplay)
                      {

                          break;
                      }

                  }
                  outputDevice.Stop();
                  audioFile.Seek(0, SeekOrigin.Begin);
              } while (loop && canplay);

              outputDevice.Dispose();
              audioFile.Close();
              audioFile.Dispose();
              try
              {
                  File.Delete(path_rele);
              }
              catch
              {

              }
          })
            {
                IsBackground = true
            };
            PlayThread.Start();
        }


    }
}
