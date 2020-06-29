#pragma once
#include <Windows.h>

#include <string>
using namespace std;

#define DLLAPI __declspec(dllexport)

#ifndef ALGORITHM_EXPORTS
#define ALGORITHM_CLASS __declspec(dllimport)
#define ALGORITHM_TEMPLATE
#else //EXPORT
#define ALGORITHM_CLASS __declspec(dllexport)
#define ALGORITHM_TEMPLATE __declspec(dllexport)
#endif


#define ENCODE_BYTE 1
#define ENCODE_WORD 2
#define ENCODE_DWORD 4

 class DLLAPI MicroFile
{
public:
	//´ò¿ªÎÄ¼ş¾ä±ú£¬±£´æÎÄ¼şÃû£¬²»ÄÜ²Ù×EGÒÔÉÏÎÄ¼ş
	MicroFile(LPCWSTR filename);
	//¹Ø±ÕÎÄ¼ş¾ä±ú£¬ÇåÀúàÚ´E
	~MicroFile();
	//½«ÎÄ¼şÍEû×°ÈEÚ´E
	virtual BOOL Load();
	//Çå¿ÕÎÄ¼ş
	void Clear();
    //±£´æĞŞ¸Ä
	virtual BOOL Save();
	//»ñÈ¡ÎÄ¼ş´óĞ¡
	DWORD Size();
	///½«Ò»¶ÎÊı¾İÍÆÈEÄ¼şÄ©Î²
	void Push(LPCVOID sour, ULONG size);
	///½«Ò»¶ÎÊı¾İ´ÓÎÄ¼şÄ©Î²µ¯³E
	void Pop(LPVOID sour, ULONG size);
	//´Óµ±Ç°¶ÁÈ¡Î»ÖÃ½ØÈ¡Êı¾İ
	void Sub(LPVOID tart,int size);

	//¸´ÖÆÕû¸öÎÄ¼şÊı¾İ
	BOOL Gate(LPVOID tart);

	virtual BOOL Get(LPBYTE tart) = 0;
	virtual BOOL Set(BYTE sour) = 0;

	MicroFile& operator=(int sour);

	BYTE &operator*();
	BOOL operator++(int);
	BOOL operator--(int);
	BOOL operator-=(DWORD count);
	BOOL operator+=(DWORD count);



protected:
	HANDLE m_file = NULL;
	BYTE* fileData = NULL;
	DWORD size;
	wstring* name = new wstring;
	BYTE* nPoint = NULL;
};

class DLLAPI  MicroBinary :public MicroFile
{
public:
	MicroBinary(LPCWSTR filename);
	~MicroBinary();

	
	BOOL Get(LPWORD tart);
	BOOL Get(LPDWORD tart);
    BOOL Get(LPBYTE tart);
	BOOL Set(BYTE sour);
	BOOL Set(WORD sour);
	BOOL Set(DWORD sour);

	MicroBinary& operator=(int sour);
private:

};


class DLLAPI MicroText:public MicroFile
{
public:
	MicroText(LPCWSTR filename,UINT code);
	~MicroText();


	BOOL Get(LPWSTR tart);
	BOOL Get(LPSTR tart);

	BOOL Get(LPBYTE tart);
	BOOL Set(BYTE sour);

	BOOL Set(LPCWSTR tart);
	BOOL Set(LPCSTR tart);

	BOOL Save();
	BOOL Load();
	//MicroText& operator=(int sour);
	void Push(LPCWSTR sour);
	void Push(LPCSTR sour);


	void Pop(LPWSTR tart,int snbize);
	void Pop(LPSTR tart, int snbize);
	void Clear();

	DWORD Size();

	MicroText& operator=(int sour);

	char& operator*();
	WCHAR& operator&();

	BOOL operator++(int);
	BOOL operator--(int);

	//void Sub(LPBYTE tart, int size);
private:
	int m_code;
	wstring* wData = new wstring;
	string* Data = new string;
};


class DLLAPI MicroData :public MicroFile
{
public:
	 MicroData(LPCWSTR filename, DWORD nsize);
	 ~MicroData();


	 void operator=(int sour);

	 BOOL operator++(int);
	 BOOL operator--(int);
	 BOOL operator-=(DWORD count);
	 	BOOL operator+=(DWORD count);
	 void Push(LPCVOID sour);
	///½«Ò»¶ÎÊı¾İ´ÓÎÄ¼şÄ©Î²µ¯³E
	 void Pop(LPVOID tart);
	 DWORD Size();
	 BOOL Get(LPBYTE tart);
	 BOOL Set(BYTE sour);

	 BOOL Get(LPVOID tart);
	 BOOL Set(LPCVOID sour);
private:
	int structure;
};
