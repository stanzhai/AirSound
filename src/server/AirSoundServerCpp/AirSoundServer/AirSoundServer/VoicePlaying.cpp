// VoicePlaying.cpp: implementation of the CVoicePlaying class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "VoicePlaying.h"


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CVoicePlaying::CVoicePlaying()
{
	hWaveOut=NULL;
}

CVoicePlaying::~CVoicePlaying()
{
	if (IsOpen())
		Close();
}

BOOL CVoicePlaying::Play()
{
	res=waveOutPrepareHeader (hWaveOut,&WaveHeader,sizeof(WAVEHDR));
    GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;
		
	res=waveOutWrite(hWaveOut,&WaveHeader,sizeof(WAVEHDR));	
    GetMMResult(res);
	if (res!=MMSYSERR_NOERROR)
		return FALSE;
	else
		return TRUE;
}

BOOL CVoicePlaying::Open()
{
	if (IsOpen())
		return FALSE;
	
	res=waveOutOpen (&hWaveOut,WAVE_MAPPER,&PCMfmt,(DWORD) VoiceWaveOutProc,(DWORD) this, CALLBACK_FUNCTION);
	GetMMResult(res);
	
	if (res!=MMSYSERR_NOERROR)
	{
		hWaveOut=NULL;
		return FALSE;
	}
	else
		return TRUE;
}

BOOL CVoicePlaying::Close()
{
	//res=waveOutReset(hWaveOut);
	//GetMMResult(res);
	//if (res!=MMSYSERR_NOERROR)
	//	return FALSE;
	
	res=waveOutClose(hWaveOut);
	GetMMResult(res);

	if (res!=MMSYSERR_NOERROR)
		return FALSE;
	else
		return TRUE;
}

BOOL CVoicePlaying::IsOpen()
{
  if(hWaveOut!=NULL)
	 return TRUE;
  else
     return FALSE;
	

}

BOOL CALLBACK VoiceWaveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD dwInstance, DWORD dwParam1, DWORD dwParam2)
{
	
	CVoicePlaying* pVoice=(CVoicePlaying*) dwInstance;
 	if (uMsg==WOM_DONE)
	{
			
		pVoice->res=waveOutUnprepareHeader(pVoice->hWaveOut, &pVoice->WaveHeader, sizeof(WAVEHDR));
		pVoice->GetMMResult(pVoice->res);
		pVoice->PlayFinished();
		
		if (pVoice->res!=MMSYSERR_NOERROR)
			return FALSE;
		else
			return TRUE;
	}

	return TRUE;
}


void CVoicePlaying::PlayFinished()
{
	//write your own handler here

	//or simply create your own class and override this virtual function
}
