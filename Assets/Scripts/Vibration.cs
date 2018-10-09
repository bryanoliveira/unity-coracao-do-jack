using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Vibration {
	#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaObject vibrationObj = VibrationActivity.activityObj.Get<AndroidJavaObject>("vibration");
	#else
    private static AndroidJavaObject vibrationObj;
	#endif

    public static void Vibrate() {
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
            vibrationObj.Call("vibrate");
		#endif
    }

    public static void Vibrate(long milliseconds) {
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
            vibrationObj.Call("vibrate", milliseconds);
		#endif
    }

	// sleep, vibrate, sleep...
    public static void Vibrate(long[] pattern, int repeat) {
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
            vibrationObj.Call("vibrate", pattern, repeat);
		#endif
    }

    public static bool HasVibrator() {
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
            return vibrationObj.Call<bool>("hasVibrator");
        else
		#endif
            return false;

    }

    public static void Cancel() {
		#if UNITY_ANDROID && !UNITY_EDITOR
        if (Application.platform == RuntimePlatform.Android)
            vibrationObj.Call("cancel");
		#endif
    }
}