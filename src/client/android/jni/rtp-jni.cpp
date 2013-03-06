#include <jni.h>

#ifdef __cplusplus

extern "C" {

void Java_com_rtptran_CMainRTP_rtptransport(JNIEnv *, jobject);

}

#endif

#include "rtpsession.h"
#include "rtpudpv4transmitter.h"
#include "rtpipv4address.h"
#include "rtpsessionparams.h"
#include "rtperrors.h"
#ifndef WIN32
	#include <netinet/in.h>
	#include <arpa/inet.h>
#else
	#include <winsock2.h>
#endif // WIN32
#include <stdlib.h>
#include <stdio.h>
#include <iostream>
#include <string>


#include <android/log.h>
const char* const LOG_TAG = "RTP_JNI";
//using namespace jrtplib

//
// This function checks if there was a RTP error. If so, it displays an error
// message and exists.
//

//void checkerror(jint rtperr)
//{
//	if (rtperr < 0)
//	{
//		std::cout << "ERROR: " << RTPGetErrorString(rtperr) << std::endl;
//		exit(-1);
//	}
//}

//
// The main routine
//

void Java_com_rtptran_CMainRTP_rtptransport(JNIEnv* env, jobject thiz)																 
{
 


	RTPSession sess;
	uint16_t portbase,destport;
	uint32_t destip;
	std::string ipstr;
	int status,i,num;

        // First, we'll ask for the necessary information
		
//	std::cout << "Enter local portbase:" << std::endl;
//	std::cin >> portbase;
//	std::cout << std::endl;
	portbase=1008;
	
//	std::cout << "Enter the destination IP address" << std::endl;
//	std::cin >> ipstr;
    ipstr="127.0.0.1";

	destip = inet_addr(ipstr.c_str());
	if (destip == INADDR_NONE)
	{
	//	std::cerr << "Bad IP address specified" << std::endl;
		return ;
	}
	
	// The inet_addr function returns a value in network byte order, but
	// we need the IP address in host byte order, so we use a call to
	// ntohl
	destip = ntohl(destip);
	
//	std::cout << "Enter the destination port" << std::endl;
//	std::cin >> destport;
	destport=31832;
	
	//std::cout << std::endl;
//	std::cout << "Number of packets you wish to be sent:" << std::endl;
//	std::cin >> num;
	num=500;
	
	// Now, we'll create a RTP session, set the destination, send some
	// packets and poll for incoming data.


//jboolean bl = (*env)->CallBooleanMethod(env, thiz, mid, js);
	
	RTPUDPv4TransmissionParams transparams;
	RTPSessionParams sessparams;
	
	// IMPORTANT: The local timestamp unit MUST be set, otherwise
	//            RTCP Sender Report info will be calculated wrong
	// In this case, we'll be sending 10 samples each second, so we'll
	// put the timestamp unit to (1.0/10.0)
	sessparams.SetOwnTimestampUnit(1.0/10.0);		
	
	sessparams.SetAcceptOwnPackets(true);
	transparams.SetPortbase(portbase);
	status = sess.Create(sessparams,&transparams);	
//	checkerror(status);
	
	RTPIPv4Address addr(destip,destport);
	
	status = sess.AddDestination(addr);
//	checkerror(status);

	__android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, "%s", "ready\n");


	for (i = 1 ; i <= num ; i++)
	{
		printf("\nSending packet %d/%d\n",i,num);
		
		// send the packet
		status = sess.SendPacket((void *)"1234567890",10,0,false,10);
//		checkerror(status);
	//	 jboolean bl = (*env)->CallBooleanMethod(env, thiz, regsec, jregsec);

		sess.BeginDataAccess();
		
		// check incoming packets
		if (sess.GotoFirstSourceWithData())
		{
			do
			{
				RTPPacket *pack;
				
				while ((pack = sess.GetNextPacket()) != NULL)
				{
					// You can examine the data here
				//	printf("Got packet !\n");
					
					// we don't longer need the packet, so
					// we'll delete it
					sess.DeletePacket(pack);
				}
			} while (sess.GotoNextSourceWithData());
		}
		
		sess.EndDataAccess();

#ifndef RTP_SUPPORT_THREAD
		status = sess.Poll();
//		checkerror(status);
#endif // RTP_SUPPORT_THREAD
		
		RTPTime::Wait(RTPTime(1,0));
	}
	
	sess.BYEDestroy(RTPTime(10,0),0,0);

 
	return;
}








