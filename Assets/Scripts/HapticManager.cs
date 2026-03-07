using UnityEngine;

public class HapticManager : MonoBehaviour
{
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject currentActivity;
    private AndroidJavaObject vibratorService;

    void Start()
    {
        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
        AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
        string vibratorServiceName = contextClass.GetStatic<string>("VIBRATOR_SERVICE");

        vibratorService = context.Call<AndroidJavaObject>("getSystemService", vibratorServiceName);
    }

    public void Vibrate(long milliseconds)
    {
        if (vibratorService != null)
        {
            vibratorService.Call("vibrate", milliseconds);
        }
    }

    public void VibratePattern(long[] pattern)
    {
        if (vibratorService != null)
        {
            vibratorService.Call("vibrate", pattern, -1);
        }
    }

    public void Thing()
    {
        Vibrate(1000);
    }
}