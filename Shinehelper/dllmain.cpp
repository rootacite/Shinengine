// dllmain.cpp : 定义 DLL 应用程序的入口点。

#include "pch.h"
#include <stdio.h>
#include <stdlib.h>
#include <windows.h>

#include <mmsystem.h>
#include <dsound.h>

#include "MicroFile.h"
#pragma comment (lib,"MicroFile.lib")

#pragma comment(lib, "dxguid.lib")
#pragma comment(lib, "dsound.lib")


#define MAX_AUDIO_BUF 4
int size;
int offset;

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



IDirectSoundBuffer8* m_pDSBuffer8 = NULL;    //used to manage sound buffers.
IDirectSound8* m_pDS = 0;

IDirectSoundBuffer* m_pDSBuffer = NULL;
IDirectSoundNotify8* m_pDSNotify = 0;

DSBPOSITIONNOTIFY m_pDSPosNotify[MAX_AUDIO_BUF];
HANDLE m_event[MAX_AUDIO_BUF];

LPVOID buf = NULL;
DWORD  buf_len = 0;
DWORD res = WAIT_OBJECT_0;

HANDLE hFile;
DWORD dwWrite;

extern "C" __declspec(dllexport) byte * getPCM(LPWSTR path) {
    HANDLE hF = CreateFile(path, GENERIC_READ, NULL, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
    int i= GetLastError();

    byte* result = (byte*)malloc(GetFileSize(hF, NULL));
    ReadFile(hF, result, GetFileSize(hF, NULL), &dwWrite, NULL);
    CloseHandle(hF);
   // delete &hPCM;

    return result;
}
MicroBinary* hhPCM = NULL;
extern "C" __declspec(dllexport) bool waveInit(HWND hWnd, int channels, int sample_rate, int bits_per_sample, int size)
{
    hhPCM = new MicroBinary(L"out.pcm");
    int i;
    ::size = size;
    offset = size;
    printf("waveinit\n");
    //Init DirectSound
    if (FAILED(DirectSoundCreate8(NULL, &m_pDS, NULL)))
        return FALSE;
    if (FAILED(m_pDS->SetCooperativeLevel(hWnd, DSSCL_NORMAL)))
        return FALSE;

    
    DSBUFFERDESC dsbd;
    memset(&dsbd, 0, sizeof(dsbd));
    dsbd.dwSize = sizeof(dsbd);
    dsbd.dwFlags = DSBCAPS_GLOBALFOCUS | DSBCAPS_CTRLPOSITIONNOTIFY | DSBCAPS_GETCURRENTPOSITION2;
    dsbd.dwBufferBytes = MAX_AUDIO_BUF * size;
    dsbd.lpwfxFormat = (WAVEFORMATEX*)malloc(sizeof(WAVEFORMATEX));
    dsbd.lpwfxFormat->wFormatTag = WAVE_FORMAT_PCM;
    /* format type */
    (dsbd.lpwfxFormat)->nChannels = channels;
    /* number of channels (i.e. mono, stereo...) */
    (dsbd.lpwfxFormat)->nSamplesPerSec = sample_rate;
    /* sample rate */
    (dsbd.lpwfxFormat)->nAvgBytesPerSec = sample_rate * (bits_per_sample / 8) * channels;
    /* for buffer estimation */
    (dsbd.lpwfxFormat)->nBlockAlign = (bits_per_sample / 8) * channels;
    /* block size of data */
    (dsbd.lpwfxFormat)->wBitsPerSample = bits_per_sample;
    /* number of bits per sample of mono data */
    (dsbd.lpwfxFormat)->cbSize = 0;

    //Creates a sound buffer object to manage audio samples.
    if (FAILED(m_pDS->CreateSoundBuffer(&dsbd, &m_pDSBuffer, NULL))) {
        
        return FALSE;
    }
    if (FAILED(m_pDSBuffer->QueryInterface(IID_IDirectSoundBuffer8, (LPVOID*)&m_pDSBuffer8))) {
        return FALSE;
    }
    //Get IDirectSoundNotify8
    if (FAILED(m_pDSBuffer8->QueryInterface(IID_IDirectSoundNotify, (LPVOID*)&m_pDSNotify))) {
        return FALSE;
    }
    for (i = 0; i < MAX_AUDIO_BUF; i++) {
        m_pDSPosNotify[i].dwOffset = i * size;
        m_event[i] = ::CreateEvent(NULL, false, false, NULL);
        m_pDSPosNotify[i].hEventNotify = m_event[i];
    }
    m_pDSNotify->SetNotificationPositions(MAX_AUDIO_BUF, m_pDSPosNotify);
    m_pDSNotify->Release();

    //Start Playing
  
    m_pDSBuffer8->SetCurrentPosition(0);
    m_pDSBuffer8->Play(0, 0, DSBPLAY_LOOPING);

    hFile = CreateFileA("out.pcm", GENERIC_WRITE, NULL, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    return 0;
}
DWORD dwSize;
extern "C" __declspec(dllexport) void waveWrite(byte* in_buf, int in_buf_len) {
    WriteFile(hFile, in_buf, in_buf_len, &dwSize, NULL);
  //  hhPCM->Push(in_buf, in_buf_len);
        if ((res >= WAIT_OBJECT_0) && (res <= WAIT_OBJECT_0 + 3)) {
            m_pDSBuffer8->Lock(offset, ::size, &buf, &buf_len, NULL, NULL, 0);

            // 如果是实时音频播放，那么下面的数据就可以把内存中buf_len大小的数据复制到buf指向的地址即可
            memcpy(buf, in_buf, in_buf_len);
            
            offset += buf_len;
            offset %= (::size * MAX_AUDIO_BUF);
            m_pDSBuffer8->Unlock(buf, buf_len, NULL, 0);
        }
        res = WaitForMultipleObjects(MAX_AUDIO_BUF, m_event, FALSE, INFINITE);
  
}

extern "C" __declspec(dllexport) void waveClose() 
{
    printf("OnClosed");
  //  hhPCM->Save();
  //  delete hhPCM;
    CloseHandle(hFile);
    m_pDSBuffer8->Lock(0, ::size, &buf, &buf_len, NULL, NULL, 0);

    // 如果是实时音频播放，那么下面的数据就可以把内存中buf_len大小的数据复制到buf指向的地址即可
    memset(buf, 0, buf_len);

    m_pDSBuffer8->Unlock(buf, buf_len, NULL, 0);

    //此处顺序也不能乱	
    if(m_pDSBuffer8){ m_pDSBuffer8->Release(); m_pDSBuffer8 =NULL;};
    if(m_pDS){ m_pDS->Release(); m_pDS =NULL;};
  
}

extern "C" __declspec(dllexport) HWND GetDskWindow() {
    return GetDesktopWindow();
}