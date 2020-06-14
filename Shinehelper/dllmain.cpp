// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include <stdio.h>
#include<mmsystem.h>
#include<mmreg.h>
#pragma comment(lib, "winmm.lib")


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        break;
    case DLL_THREAD_ATTACH:
        break;
    case DLL_THREAD_DETACH:
        break;
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}
HWAVEOUT        hwo;
WAVEHDR         wh;
WAVEFORMATEX    wfx;
HANDLE          wait;
extern "C" __declspec(dllexport) void waveInit(short nFormat,short nChannels,int nSamPer,int bitRate,int Bitsample) {

	wfx.wFormatTag = WAVE_FORMAT_PCM;//设置波形声音的格式
	wfx.nChannels = nChannels;//设置音频文件的通道数量
	wfx.nSamplesPerSec = nSamPer;//设置每个声道播放和记录时的样本频率
	wfx.nAvgBytesPerSec = bitRate;//设置请求的平均数据传输率,单位byte/s。这个值对于创建缓冲大小是很有用的
	wfx.nBlockAlign = 2;//以字节为单位设置块对齐
	wfx.wBitsPerSample = Bitsample;
	wfx.cbSize = 0;//额外信息的大小
	wait = CreateEvent(NULL, 0, 0, NULL);
	waveOutOpen(&hwo, WAVE_MAPPER, &wfx, (DWORD_PTR)wait, 0L, CALLBACK_EVENT);//打开一个给定的波形音频输出装置来进行回放
//	fopen_s(&thbgm, "paomo.pcm", "rb");
//	cnt = fread(buf, sizeof(char), 1024 * 1024 * 4, thbgm);//读取文件4M的数据到内存来进行播放，通过这个部分的修改，增加线程可变成网络音频数据的实时传输。当然如果希望播放完整的音频文件，也是要在这里稍微改一改

	printf("Wave Init");
//	fclose(thbgm);
	
}

extern "C" __declspec(dllexport) void waveWriteBuffer(void* lpData, int size) {

	printf("Wave Prei");

	wh.lpData = (LPSTR)lpData;
	wh.dwBufferLength = size;
	wh.dwFlags = 0L;
	wh.dwLoops = 1L;
	waveOutPrepareHeader(hwo, &wh, sizeof(WAVEHDR));//准备一个波形数据块用于播放
	waveOutWrite(hwo, &wh, sizeof(WAVEHDR));//在音频媒体中播放第二个函数wh指定的数据
	//WaitForSingleObject(wait, INFINITE);//用来检测hHandle事件的信号状态，在某一线程中调用该函数时，线程暂时挂起，如果在挂起的INFINITE毫秒内，线程所等待的对象变为有信号状态，则该函数立即返回
}

extern "C" __declspec(dllexport) void waveClear() {
	waveOutClose(hwo);
}