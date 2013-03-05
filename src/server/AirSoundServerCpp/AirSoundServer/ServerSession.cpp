#include "StdAfx.h"
#include "ServerSession.h"

ServerSession::ServerSession(void)
{
	WSADATA wsaData = {0};
	WSAStartup(MAKEWORD(2, 2), &wsaData);

	sessparams.SetOwnTimestampUnit(1.0/8000.0);		
	sessparams.SetAcceptOwnPackets(true);

	transparams.SetPortbase(31832);
	int result = this->Create(sessparams, &transparams);	
	CheckError(result);
}


ServerSession::~ServerSession(void)
{
	WSACleanup();
}

void ServerSession::OnNewSource( RTPSourceData *dat )
{
	if (dat->IsOwnSSRC())
		return;

	uint32_t ip;
	uint16_t port;

	if (dat->GetRTPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort();
	}
	else if (dat->GetRTCPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTCPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort()-1;
	}
	else
		return;

	RTPIPv4Address dest(ip,port);
	AddDestination(dest);

	struct in_addr inaddr;
	inaddr.s_addr = htonl(ip);

	// 将客户端IP信息添加到列表框
	char ipInfo[32] = {0};
	sprintf_s(ipInfo, sizeof(ipInfo), "%s:%d", inet_ntoa(inaddr), port);
	HWND hListWnd = GetDlgItem(g_hMainWnd, IDC_LIST_CLIENTS);
	SendMessage(hListWnd, LB_ADDSTRING, NULL, (LPARAM)ipInfo);
}

void ServerSession::OnRemoveSource( RTPSourceData *dat )
{
	if (dat->IsOwnSSRC())
		return;

	uint32_t ip;
	uint16_t port;

	if (dat->GetRTPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort();
	}
	else if (dat->GetRTCPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTCPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort()-1;
	}
	else
		return;

	RTPIPv4Address dest(ip,port);
	DeleteDestination(dest);

	struct in_addr inaddr;
	inaddr.s_addr = htonl(ip); 
	// 将客户端IP信息从列表框移除
	char ipInfo[32];
	sprintf_s(ipInfo, sizeof(ipInfo), "%s : %d", inet_ntoa(inaddr), port);
	HWND hListWnd = GetDlgItem(g_hMainWnd, IDC_LIST_CLIENTS);
	SendMessage(hListWnd, LB_DELETESTRING, NULL, (LPARAM)ipInfo);
}

void ServerSession::OnBYEPacket( RTPSourceData *dat )
{
	if (dat->IsOwnSSRC())
		return;
	if (dat->ReceivedBYE())
		return;

	uint32_t ip;
	uint16_t port;

	if (dat->GetRTPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort();
	}
	else if (dat->GetRTCPDataAddress() != 0)
	{
		const RTPIPv4Address *addr = (const RTPIPv4Address *)(dat->GetRTCPDataAddress());
		ip = addr->GetIP();
		port = addr->GetPort()-1;
	}
	else
		return;

	RTPIPv4Address dest(ip,port);
	DeleteDestination(dest);

	struct in_addr inaddr;
	inaddr.s_addr = htonl(ip);
	// 将客户端IP信息从列表框移除
	char ipInfo[32];
	sprintf_s(ipInfo, sizeof(ipInfo), "%s : %d", inet_ntoa(inaddr), port);
	HWND hListWnd = GetDlgItem(g_hMainWnd, IDC_LIST_CLIENTS);
	SendMessage(hListWnd, LB_DELETESTRING, NULL, (LPARAM)ipInfo);
}

void ServerSession::CheckError( int status )
{
	if (status < 0)
	{
		MessageBox(g_hMainWnd, RTPGetErrorString(status).c_str(), "RTP会话创建失败", MB_ICONERROR);
		exit(-1);
	}
}
