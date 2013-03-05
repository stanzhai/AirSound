// VoicePlaying.h: interface for the CVoicePlaying class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0A5D__INCLUDED_)
#define AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0A5D__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "VoiceBase.h"
BOOL CALLBACK VoiceWaveOutProc(
						  HWAVEOUT hwi,       
						  UINT uMsg,         
						  DWORD dwInstance,  
						  DWORD dwParam1,    
						  DWORD dwParam2     
						  );

class CVoicePlaying : public CVoiceBase  
{
public:
	void PlayFinished();
	BOOL IsOpen();
	BOOL Close();
	BOOL Open();
    BOOL Play();
	HWAVEOUT hWaveOut;
	CVoicePlaying();
	virtual ~CVoicePlaying();
   


};

#endif // !defined(AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0A5D__INCLUDED_)
