// VoiceRecording.h: interface for the CVoiceRecording class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0Aaa__INCLUDED_)
#define AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0Aaa__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "VoiceBase.h"
BOOL CALLBACK VoiceWaveInProc(
						 HWAVEIN hwi,       
						 UINT uMsg,         
						 DWORD dwInstance,  
						 DWORD dwParam1,    
						 DWORD dwParam2     
						 );


class CVoiceRecording : public CVoiceBase  
{
public:
	virtual void RecordFinished();
	BOOL IsOpen();
	BOOL Close();
	BOOL Open();
    BOOL Record();
    HWAVEIN hWaveIn;
	CVoiceRecording();
	virtual ~CVoiceRecording();
   
	
};

#endif // !defined(AFX_VOICEPLAYING_H__D3C0CFEA_0015_4A9A_BEF9_C5A132CB0Aaa__INCLUDED_)
