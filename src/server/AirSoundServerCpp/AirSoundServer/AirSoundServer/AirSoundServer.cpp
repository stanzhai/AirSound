// AirSoundServer.cpp : 定义应用程序的入口点。
//

#include "stdafx.h"
#include "AirSoundServer.h"
#include "Recorder.h"
#include "Server.h"
#include "VoiceRecording.h"
#include "VoicePlaying.h"

// 全局变量:
HINSTANCE hInst;								// 当前实例
HICON hIcon;
HWND hMainWnd;
Recorder recorder;
Server server;
CVoiceRecording m_Record;						// 录音对象
CVoicePlaying m_Play;

LRESULT CALLBACK DialogProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam);
void Init(HWND hDlgWnd);
void AddRecordDev(int id, string devName);

int APIENTRY _tWinMain(_In_ HINSTANCE hInstance,
                     _In_opt_ HINSTANCE hPrevInstance,
                     _In_ LPTSTR    lpCmdLine,
                     _In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);

	hInst = hInstance;
	hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_AIRSOUNDSERVER));

	INT_PTR result = DialogBox(hInstance, MAKEINTRESOURCE(IDD_DIALOG_MAIN), NULL, (DLGPROC)DialogProc);
	return result;
}

LRESULT CALLBACK DialogProc(HWND hDlg, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_INITDIALOG:
		Init(hDlg);
		return TRUE;

	case WM_COMMAND:
		int id = LOWORD(wParam);
		if (id == IDC_BUTTON_START)
		{
			server.Start();
		}
		if (id == IDCANCEL)
		{
			EndDialog(hDlg, 0);
		}
		break;
	}
	return FALSE;
}

void Init(HWND hDlgWnd)
{
	hMainWnd = hDlgWnd;
	// 设置窗口图标
	SendMessage(hDlgWnd, WM_SETICON, ICON_BIG, (LPARAM)hIcon);
	// 添加所有的录音设备到ComboBox
	recorder.EnumDevs((EnumDevsProc)AddRecordDev);
	HWND hComboWnd = GetDlgItem(hMainWnd, IDC_COMBO_DEVS);
	// 如果有1个以上的录音设备，则选中第一个录音设备
	int count = SendMessage(hComboWnd, CB_GETCOUNT, NULL, NULL);
	if (count !=  0)
	{
		SendMessage(hComboWnd, CB_SETCURSEL, NULL, 0);
	}
}

void AddRecordDev(int id, string devName)
{
	HWND hComboWnd = GetDlgItem(hMainWnd, IDC_COMBO_DEVS);
	LRESULT index = SendMessage(hComboWnd, CB_ADDSTRING, NULL, (LPARAM)devName.c_str());
	SendMessage(hComboWnd, CB_SETITEMDATA, index, id);
}

void AddStringToList(HWND hDlg)
{
	HWND hListWnd = GetDlgItem(hDlg, ID_LISTBOX);
	SendMessage(hListWnd, LB_ADDSTRING, NULL, (LPARAM)TEXT("翟士丹"));
}
