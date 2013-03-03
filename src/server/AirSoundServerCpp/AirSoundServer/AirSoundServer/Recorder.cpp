#include "stdafx.h"
#include "Recorder.h"


Recorder::Recorder(void)
{
}


Recorder::~Recorder(void)
{
}

void Recorder::EnumDevs(EnumDevsProc lpProc)
{
	WAVEINCAPS waveInCap;
	UINT count = waveInGetNumDevs();
	for (int i = 0; i < count; i++)
	{
		UINT result = waveInGetDevCaps(i, &waveInCap, sizeof(waveInCap));
		if (result == 0)
		{
			lpProc(i, waveInCap.szPname);
		}
	}
}