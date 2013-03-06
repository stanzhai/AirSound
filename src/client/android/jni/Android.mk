LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE    := rtp-jni
LOCAL_SRC_FILES := rtp-jni.cpp

LOCAL_CFLAGS +=   -DENABLE_TRACE

LOCAL_C_INCLUDES:= $(LOCAL_PATH)/jthread $(LOCAL_PATH)/jrtplib  
LOCAL_EXPORT_C_INCLUDES:=$(LOCAL_C_INCLUDES)


 
LOCAL_STATIC_LIBRARIES :=\
           jthread jrtplib 

LOCAL_LDLIBS += -llog

include $(BUILD_SHARED_LIBRARY)
$(call import-module,jthread)
$(call import-module,jrtplib)
