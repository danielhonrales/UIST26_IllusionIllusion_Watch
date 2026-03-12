using UnityEngine;
using UnityEngine.Events;

public class DialManager : MonoBehaviour
{
    [Header("Settings")]
    [Range(0.01f, 1f)]
    public float sensitivity = 0.1f;
    public bool invertDirection = false;

    [Header("Events")]
    public UnityEvent<float> OnBezelDelta;     // raw delta each frame
    public UnityEvent<float> OnBezelValue;     // normalized 0–1 accumulated
    public UnityEvent<string> OnBezelDirection; // "cw" or "ccw"

    [Header("Debug")]
    public float currentValue = 0.5f;          // starts at midpoint
    public float lastDelta = 0f;

    private AndroidJavaObject _activity;
    private bool _isAndroid;

    void Start()
    {
        _isAndroid = Application.platform == RuntimePlatform.Android;

        if (_isAndroid)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    void Update()
    {
        if (!_isAndroid) return;

        // Poll native delta
        float delta = _activity.Call<float>("getRotaryDelta");
        if (Mathf.Abs(delta) < 0.001f) return;

        if (invertDirection) delta = -delta;

        lastDelta = delta;
        currentValue = Mathf.Clamp01(currentValue + delta * sensitivity);

        // Fire events
        OnBezelDelta?.Invoke(delta);
        OnBezelValue?.Invoke(currentValue);
        OnBezelDirection?.Invoke(delta > 0 ? "cw" : "ccw");
    }

    // Called by UnityPlayer.UnitySendMessage from Kotlin (event-driven)
    public void OnBezelEvent(string message)
    {
        string[] parts = message.Split(':');
        string direction = parts[0];
        float delta = float.Parse(parts[1]);

        Debug.Log($"Bezel event: {direction} delta={delta}");
    }

    // ── Public API ─────────────────────────────────────────────

    public void ResetValue()
    {
        currentValue = 0.5f;
        _activity?.Call("resetAccumulated");
    }

    // Map 0–1 value to any range
    public float GetMappedValue(float min, float max)
    {
        return Mathf.Lerp(min, max, currentValue);
    }

    // Map to haptic intensity 0–255
    public int GetHapticIntensity() => Mathf.RoundToInt(currentValue * 255f);
}
