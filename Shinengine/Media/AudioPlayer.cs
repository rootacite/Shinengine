﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Shinengine.Media
{
    public class AudioPlayer
    {
        Thread PlayThread = null;
        public bool canplay = true;
        public AudioPlayer(string url, bool loop = false, float volum = 1)
        {
            PlayThread = new Thread(() =>
             {
                 var audioFile = new AudioFileReader(url);
                 var outputDevice = new WaveOutEvent();
                 outputDevice.Init(audioFile);
                 outputDevice.Volume = volum;
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
             });
            PlayThread.IsBackground = true;
            PlayThread.Start();
        }
    }
}
