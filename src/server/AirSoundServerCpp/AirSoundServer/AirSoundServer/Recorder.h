#pragma once
#include <iostream>
#include <Windows.h>
#include <MMSystem.h>
using namespace std;

// 
typedef void (*EnumDevsProc)(int, string);

class Recorder
{
public:
	Recorder(void);
	~Recorder(void);

	// √∂æŸ¬º“Ù…Ë±∏
	void EnumDevs(EnumDevsProc lpProc);
};

