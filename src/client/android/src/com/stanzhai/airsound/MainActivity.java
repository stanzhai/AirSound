package com.stanzhai.airsound;

import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;
import android.media.AudioTrack;

public class MainActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		//AudioTrack at = new AudioTrack()
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	public native void unimplementedStringFromJNI();
	static {
        System.loadLibrary("rtp-jni");
    }
}
