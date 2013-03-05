#pragma once
#include "jrtplib3/rtpsession.h"
#include "jrtplib3/rtpsourcedata.h"
#include "jrtplib3/rtpipv4address.h"
#include "jrtplib3/rtpudpv4transmitter.h"
#include "jrtplib3/rtpsessionparams.h"

using namespace jrtplib;
extern HWND g_hMainWnd;

/*
*	基于RTPSession的Server端会话服务
*/
class ServerSession :
	public RTPSession
{
private:
	RTPUDPv4TransmissionParams transparams;
	RTPSessionParams sessparams;

	void CheckError(int status);
protected:
	void OnNewSource(RTPSourceData *srcdat);
	void OnRemoveSource(RTPSourceData *srcdat);
	void OnBYEPacket(RTPSourceData *srcdat);
public:
	ServerSession(void);
	~ServerSession(void);
};