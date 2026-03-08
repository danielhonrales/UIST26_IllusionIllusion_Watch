using UnityEngine;

public class HapticManager : MonoBehaviour
{
    private AndroidJavaObject vibrator;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }
    }

    // Simple pulse
    public void Vibrate(long milliseconds)
    {
        vibrator?.Call("vibrate", milliseconds);
    }

    // Custom pattern: [delay, on, off, on, off...]
    public void VibratePattern(long[] pattern, int repeat = -1)
    {
        vibrator?.Call("vibrate", pattern, repeat);
    }

    // Waveform with amplitude (API 26+)
    public void VibrateWaveform(long[] timings, int[] amplitudes)
    {
        using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
        var effect = effectClass.CallStatic<AndroidJavaObject>(
            "createWaveform", timings, amplitudes, -1
        );
        vibrator?.Call("vibrate", effect);
    }

    // Predefined effects (click, tick, etc.)
    public void VibratePredefined(int effectId)
    {
        // effectId: 0=TICK, 1=CLICK, 2=HEAVY_CLICK, 3=DOUBLE_CLICK
        using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
        var effect = effectClass.CallStatic<AndroidJavaObject>(
            "createPredefined", effectId
        );
        vibrator?.Call("vibrate", effect);
    }

    public void Thing()
    {
        Debug.Log("bruh");
        Vibrate(1000);
    }
}