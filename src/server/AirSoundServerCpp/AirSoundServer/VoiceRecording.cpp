// VoiceRecording.cpp: implementation of the CVoiceRecording class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "VoiceRecording.h"


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif


//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CVoiceRecording::CVoiceRecording()
{
	hWaveIn=NULL;	
}

CVoiceRecording::~CVoiceRecording()
{
	if (IsOpen())
		Close();
}

BOOL CVoiceRecording::Record()
{
	res=waveInPrepareHeader(hWaveIn,&WaveHeader,sizeof(WAVEHDR));
	GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;


	res=waveInAddBuffer(hWaveIn,&WaveHeader,sizeof(WAVEHDR));
	GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;
		
	res=waveInStart(hWaveIn) ;
	GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;
	else
		return TRUE;
	
		
}

BOOL CVoiceRecording::Open()
{
	if (IsOpen())
		return FALSE;
	
	res=waveInOpen(&hWaveIn, 0 /*(UINT) WAVE_MAPPER*/, &PCMfmt, (DWORD) VoiceWaveInProc, (DWORD) this, CALLBACK_FUNCTION);
	GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
	{
		hWaveIn=NULL;
		return FALSE;
	}
	else
		return TRUE;
}

BOOL CVoiceRecording::Close()
{
	//res=waveInReset(hWaveIn);
	//GetMMResult(res);
	//if (res!=MMSYSERR_NOERROR)
	//	return FALSE;
		
	res=waveInClose (hWaveIn);
	GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;
	else
		return TRUE;
}

BOOL CVoiceRecording::IsOpen()
{
	if (hWaveIn!=NULL)
		return TRUE;
	else
		return FALSE;
}

BOOL CALLBACK VoiceWaveInProc(HWAVEIN hwi, UINT uMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2)
{
	if (uMsg==WIM_DATA)
	{
		CVoiceRecording* pVoice=(CVoiceRecording*) dwInstance;
		
		pVoice->res=waveInUnprepareHeader(pVoice->hWaveIn, &pVoice->WaveHeader, sizeof(WAVEHDR));
		pVoice->GetMMResult(pVoice->res);
		pVoice->RecordFinished();

		if (pVoice->res!=MMSYSERR_NOERROR)
			return FALSE;
		else
			return TRUE;
	}

	return TRUE;
}


void CVoiceRecording::RecordFinished()
{
	//write your handler here

	//or create your own classes that derived from this class
	//and override this virtual function
}
