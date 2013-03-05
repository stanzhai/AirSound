// VoiceBase.cpp: implementation of the CVoiceBase class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "VoiceBase.h"
#include  "Mmsystem.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CVoiceBase::CVoiceBase()
{
	//defualt settings
   	PCMfmt.cbSize=0;
	PCMfmt.wFormatTag=WAVE_FORMAT_PCM;
	PCMfmt.nChannels=1;
	PCMfmt.nSamplesPerSec=8000;
	PCMfmt.wBitsPerSample=8;
	PCMfmt.nBlockAlign=1;
	PCMfmt.nAvgBytesPerSec=8000;

	buffer=NULL;
}

CVoiceBase::~CVoiceBase()
{
	if (buffer!=NULL)
	{
		delete [] buffer;
		buffer=NULL;
	}
}


BOOL CVoiceBase::PrepareBuffer(DWORD ntime)
{
   if (buffer!=NULL)
   {
	   delete [] buffer;
	   buffer=NULL;
   }
   
   DWORD length=PCMfmt.nSamplesPerSec*PCMfmt.nChannels
	          *PCMfmt.wBitsPerSample*ntime/8;

   buffer = (char*)malloc(sizeof(char) * length);
   //memset(buffer, 0, length);

   if (buffer==NULL)
 	{
 		return FALSE;
 	}

   WaveHeader.lpData=buffer;
   WaveHeader.dwBufferLength=length;
   WaveHeader.dwBytesRecorded=0;
   WaveHeader.dwUser=0;
   WaveHeader.dwFlags=0;
   WaveHeader.reserved=0;
   WaveHeader.lpNext=0;

   return TRUE;
}

void CVoiceBase::DestroyBuffer()
{
	if (buffer!=NULL)
	{
		delete [] buffer;
		buffer=NULL;
	}
}

void  CVoiceBase::GetMMResult(MMRESULT res)
{
	switch (res)
	{
	case MMSYSERR_ALLOCATED: 
		m_result="Specified resource is already allocated.";
		break;
		
	case MMSYSERR_BADDEVICEID:
		m_result="Specified device identifier is out of range.";
		break;
		
	case MMSYSERR_NODRIVER:
		m_result="No device driver is present. ";
		break;
		
	case MMSYSERR_NOMEM:
		m_result="Unable to allocate or lock memory. ";
		break;
		
	case WAVERR_BADFORMAT:
		m_result="Attempted to open with an unsupported waveform-audio format.";
		break;
		
	case WAVERR_UNPREPARED:
		m_result="The buffer pointed to by the pwh parameter hasn't been prepared. ";
		break;
		
	case WAVERR_SYNC:
		m_result="The device is synchronous but waveOutOpen was called"
			"without using the WAVE_ALLOWSYNC flag. ";
		break;
		
	case WAVERR_STILLPLAYING:
		m_result="The buffer pointed to by the pwh parameter is still in the queue.";
		break;
		
	case MMSYSERR_NOTSUPPORTED:
		m_result="Specified device is synchronous and does not support pausing. ";
		break;
		
	case MMSYSERR_NOERROR:
		break;
		
	default:
		m_result="Unspecified error";
	}
}

string CVoiceBase::GetLastError()
{
	return m_result;
}

BOOL CVoiceBase::CopyBuffer(LPVOID lpBuffer,DWORD ntime)
{
	DWORD length=PCMfmt.nSamplesPerSec*PCMfmt.nChannels
		*PCMfmt.wBitsPerSample*ntime/8;
	memcpy(buffer, lpBuffer,length );
	return TRUE;
}

void CVoiceBase::SetFormat( DWORD nSamplesPerSec,  WORD  wBitsPerSample,WORD  nChannels)
{
	
   	PCMfmt.cbSize=0;
	PCMfmt.wFormatTag=WAVE_FORMAT_PCM;
	PCMfmt.nChannels=nChannels;
	PCMfmt.nSamplesPerSec=nSamplesPerSec;
	PCMfmt.wBitsPerSample=wBitsPerSample;
	PCMfmt.nBlockAlign=nChannels*wBitsPerSample/8;
	PCMfmt.nAvgBytesPerSec=nSamplesPerSec*nChannels*wBitsPerSample/8;
}
