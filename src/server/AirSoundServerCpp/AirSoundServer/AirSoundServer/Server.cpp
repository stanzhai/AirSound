#include "stdafx.h"
#include "Server.h"


Server::Server(void):
	servSocket(INVALID_SOCKET)
{
	WSADATA wsaData = {0};
	int iResult = 0;

	sockaddr_in service;
	service.sin_family = AF_INET;
    service.sin_addr.s_addr = inet_addr("127.0.0.1");
    service.sin_port = htons(31832);

	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != 0) {
        (L"WSAStartup failed: %d\n", iResult);
		return;
    }

	servSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	if (servSocket == INVALID_SOCKET) {
        (L"socket function failed with error: %ld\n", WSAGetLastError());
        WSACleanup();
		return;
    }
	iResult = bind(servSocket, (SOCKADDR *) & service, sizeof (service));
    if (iResult == SOCKET_ERROR) {
        (L"bind function failed with error %d\n", WSAGetLastError());
		iResult = closesocket(servSocket);
        if (iResult == SOCKET_ERROR)
            (L"closesocket function failed with error %d\n", WSAGetLastError());
        WSACleanup();
        return;
    }
}


Server::~Server(void)
{
	WSACleanup();
}


void Server::Start()
{
	DWORD dwThreadId;
	CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)Server::WorkThread, this, NULL, &dwThreadId);
}

DWORD WINAPI Server::WorkThread(LPVOID lpParam)
{
	Server* server = (Server*)lpParam;
	while (true)
	{
		SOCKET client;
		if (listen(server->servSocket, SOMAXCONN) == SOCKET_ERROR)
		{
			continue;
		}
		client = accept(server->servSocket, NULL, NULL);

		char recvBuf[1024];
		recv(client, recvBuf, 1024, 0);
		int a;
		system("pause");
	}
	return 0;
}

void Server::Stop()
{

}