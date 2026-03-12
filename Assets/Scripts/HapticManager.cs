using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Sockets;
using System.Text;

public class HapticManager : MonoBehaviour
{

    public ToolTCP toolTCP;
    private AndroidJavaObject motor;

    int loops = 3;
    float preheat_time = 1.25f;
    int hop_count = 3;
    float saltation_duration = 0.08f;
    float saltation_interval = 0.13f;
    float motion_duration = 1.2f;
    float motion_interval = 0.5f;
    float funneling_duration = 0.12f;
    float funneling_interval = 0.5f;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            motor = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }
    }

    // Simple pulse
    public void Vibrate(long milliseconds)
    {
        motor?.Call("vibrate", milliseconds);
    }

    // Custom pattern: [delay, on, off, on, off...]
    public void VibratePattern(long[] pattern, int repeat = -1)
    {
        motor?.Call("vibrate", pattern, repeat);
    }

    public void VibrateWaveform(long[] timings, int[] amplitudes)
    {
        using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
        var effect = effectClass.CallStatic<AndroidJavaObject>(
            "createWaveform", timings, amplitudes, -1
        );
        motor?.Call("vibrate", effect);
    }

    // Waveform with amplitude (API 26+)
    public void VibrateRamp(float duration, int intensity, int rampUp, int rampDown)
    {
        duration *= 1000;

        List<long> timings = new List<long>();
        List<int> amplitudes = new List<int>();

        long thirdDuration = (long)duration / 3;
        int step = intensity / 3;

        if (rampUp == 1 && rampDown == 1)
        {
            // Ramp up, hold, ramp down
            timings.AddRange(new long[] { thirdDuration / 3, thirdDuration / 3, thirdDuration / 3 });
            amplitudes.AddRange(new int[] { step, step * 2, intensity });

            timings.Add(thirdDuration);
            amplitudes.Add(intensity);

            timings.AddRange(new long[] { thirdDuration / 3, thirdDuration / 3, thirdDuration / 3 });
            amplitudes.AddRange(new int[] { step * 2, step, 0 });
        }
        else if (rampUp == 1 && rampDown == 0)
        {
            // Ramp up, then hold at full intensity
            timings.AddRange(new long[] { thirdDuration / 3, thirdDuration / 3, thirdDuration / 3 });
            amplitudes.AddRange(new int[] { step, step * 2, intensity });

            timings.Add(thirdDuration * 2);
            amplitudes.Add(intensity);
        }
        else if (rampUp == 0 && rampDown == 1)
        {
            // Hold at full intensity, then ramp down
            timings.Add(thirdDuration * 2);
            amplitudes.Add(intensity);

            timings.AddRange(new long[] { thirdDuration / 3, thirdDuration / 3, thirdDuration / 3 });
            amplitudes.AddRange(new int[] { step * 2, step, 0 });
        }
        else
        {
            // No ramp - constant intensity
            timings.AddRange(new long[] { (long)duration });
            amplitudes.AddRange(new int[] { intensity });
        }

        VibrateWaveform(timings.ToArray(), amplitudes.ToArray());
    }

    public void Pulse()
    {
        Vibrate(1000);
    }

    public void Breathing()
    {
        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        // Thermal - Hot
        pattern.thermalPulses.Add(new(0, time, 4, 2.3f, -1));
        //time += preheat_time;

        // In - Motion
        pattern.tactilePulses.Add(new(0, time, motion_duration / 2, 30));
        time += .15f;
        pattern.tactilePulses.Add(new(1, time, motion_duration, 50, 1, 1));
        time += .15f;
        watchCommands.Add(new(time, motion_duration, 65, 1, 1));
        time += .15f;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 80, 1, 1));
        time += .15f;
        pattern.tactilePulses.Add(new(1, time, motion_duration, 70, 1, 1));
        time += .15f;
        watchCommands.Add(new(time, motion_duration, 65, 1, 1));
        time += .15f;

        // In - Saltation
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(1, time, saltation_duration, 100));
            time += saltation_interval * 1.2f;
        }
        for (int i = 0; i < hop_count; i++)
        {
            watchCommands.Add(new(time, saltation_duration, 100, 0, 0));
            time += saltation_interval * 1.4f;
        }
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(0, time, saltation_duration, 100));
            time += saltation_interval * 1.6f;
        }
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(1, time, saltation_duration, 100));
            time += saltation_interval * 1.8f;
        }
        for (int i = 0; i < hop_count; i++)
        {
            watchCommands.Add(new(time, saltation_duration, 100, 0, 0));
            time += saltation_interval * 2.0f;
        }

        // Hold - Funneling
        /* pattern.tactilePulses.Add(new(0, time, funneling_duration, 95));
        pattern.tactilePulses.Add(new(1, time, funneling_duration, 95));
        time += funneling_interval * 1.2f;
        pattern.tactilePulses.Add(new(1, time, funneling_duration, 95));
        watchCommands.Add(new(time, funneling_duration, 95, 0, 0));
        time += funneling_interval * 1.2f; */
        
        pattern.thermalPulses.Add(new(0, time, 5, 1.8f, 1));   // Thermal - Cold

        watchCommands.Add(new(time, funneling_duration, 95, 0, 0));
        pattern.tactilePulses.Add(new(0, time, funneling_duration, 95));
        time += funneling_interval * 1.2f;

        // Out - Motion
        watchCommands.Add(new(time, motion_duration * 1.2f, 50, 0, 0));
        time += motion_interval * 1.1f;
        pattern.tactilePulses.Add(new(1, time, motion_duration * 1.2f, 65, 1, 1));
        time += motion_interval * 1.1f;
        pattern.tactilePulses.Add(new(0, time, motion_duration * 1.4f, 80, 1, 1));
        time += motion_interval * 1.2f;
        watchCommands.Add(new(time, motion_duration * 1.4f, 65, 1, 1));
        time += motion_interval * 1.2f;
        pattern.tactilePulses.Add(new(1, time, motion_duration * 1.4f, 65, 1, 1));
        time += motion_interval * 1.1f;
        pattern.tactilePulses.Add(new(0, time, motion_duration * 1.2f, 65, 1, 1));
        time += motion_interval / 2;
        watchCommands.Add(new(time, motion_duration / 2, 40, 1, 1));

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;
        Debug.Log(GetMasterClockTime());
        Debug.Log(pattern.startTime);

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void Incoming()
    {
        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        watchCommands.Add(new(time, 0.2f, 100));
        time += 0.1f;
        watchCommands.Add(new(time, 0.2f, 100));

        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void Mad()
    {
        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        // Thermal - Hot
        pattern.thermalPulses.Add(new(0, time, 4, 1.8f, 1));
        time += preheat_time;

        // Inward - Motion
        watchCommands.Add(new(time, motion_duration, 100));
        time += motion_interval;
        watchCommands.Add(new(time, motion_duration, 60, 0, 1));
        pattern.tactilePulses.Add(new(0, time, motion_duration, 60, 1, 0));
        pattern.tactilePulses.Add(new(1, time, motion_duration, 60, 1, 0));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 100, 0, 0));
        pattern.tactilePulses.Add(new(1, time, motion_duration, 100, 0, 0));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 60, 0, 1));
        pattern.tactilePulses.Add(new(1, time, motion_duration, 60, 0, 1));

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void Happy()
    {
        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        // Thermal - Cold
        pattern.thermalPulses.Add(new(0, time, 4, 2.3f, -1));
        //time += preheat_time;

        // Inward - Motion
        watchCommands.Add(new(time, motion_duration, 100, 1, 1));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 100, 1, 0));
        pattern.tactilePulses.Add(new(1, time, motion_duration, 100, 1, 0));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 60, 0, 1));
        pattern.tactilePulses.Add(new(1, time, motion_duration, 60, 0, 1));

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void Rain()
    {
        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        // Thermal - Cold
        pattern.thermalPulses.Add(new(0, time, 4, 2.3f, -1));
        time += preheat_time;

        // Saltation
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(1, time, saltation_duration, 100));
            time += saltation_interval;
        }
        for (int i = 0; i < hop_count; i++)
        {
            watchCommands.Add(new(time, saltation_duration, 100, 0, 0));
            time += saltation_interval;
        }
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(0, time, saltation_duration, 100));
            time += saltation_interval;
        }
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(1, time, saltation_duration, 100));
            time += saltation_interval;
        }
        for (int i = 0; i < hop_count; i++)
        {
            watchCommands.Add(new(time, saltation_duration, 100, 0, 0));
            time += saltation_interval;
        }
        for (int i = 0; i < hop_count; i++)
        {
            pattern.tactilePulses.Add(new(0, time, saltation_duration, 100));
            time += saltation_interval;
        }

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void Hot()
    {
        PatternInfo pattern = new();
        float time = 0;

        // Thermal - Hot
        pattern.thermalPulses.Add(new(0, time, 2, 1.8f, 1));
        time += preheat_time;

        pattern.tactilePulses.Add(new(0, time, funneling_duration, 95));
        pattern.tactilePulses.Add(new(1, time, funneling_duration, 95));
        time += 0.3f;
        pattern.tactilePulses.Add(new(0, time, funneling_duration, 95));
        pattern.tactilePulses.Add(new(1, time, funneling_duration, 95));
        time += 0.3f;
        pattern.tactilePulses.Add(new(0, time, funneling_duration, 95));
        pattern.tactilePulses.Add(new(1, time, funneling_duration, 95));
        time += 0.3f;

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
    }

    public void Alarm()
    {
        float motion_duration = 0.6f;
        float motion_interval = 0.25f;

        PatternInfo pattern = new();
        List<WatchCommand> watchCommands = new();
        float time = 0;

        // Thermal - Hot
        pattern.thermalPulses.Add(new(0, time, 4 + preheat_time, 1.8f, 1));
        //time += preheat_time;

        // In - Motion
        pattern.tactilePulses.Add(new(0, time, motion_duration / 2, 30));
        time += motion_interval / 2;
        pattern.tactilePulses.Add(new(1, time, motion_duration, 50, 1, 1));
        time += motion_interval;
        watchCommands.Add(new(time, motion_duration, 80, 1, 1));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration, 70, 1, 1));
        time += motion_interval / 2;
        pattern.tactilePulses.Add(new(1, time, motion_duration, 70, 1, 1));
        time += motion_interval;
        watchCommands.Add(new(time, motion_duration, 80, 1, 1));
        time += motion_interval;
        pattern.tactilePulses.Add(new(0, time, motion_duration / 2, 30));
        time += motion_interval / 2;
        pattern.tactilePulses.Add(new(1, time, motion_duration, 50, 1, 1));
        time += motion_interval;
        watchCommands.Add(new(time, motion_duration, 80, 1, 1));
        time += motion_interval;

        // Determine start time
        pattern.startTime = GetMasterClockTime() + 2000;

        // Send info
        pattern.sendTime = GetMasterClockTime();
        string json = JsonConvert.SerializeObject(pattern, Formatting.Indented,
            new JsonSerializerSettings
            {
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double
            });
        Debug.Log(json);
        toolTCP.SendMessageToPi(json);
        SendWatchCommands(pattern.startTime, watchCommands);
    }

    public void SendWatchCommands(long startTime, List<WatchCommand> commands)
    {
        foreach (var cmd in commands)
        {
            // Option 1: Schedule with coroutine
            StartCoroutine(DelayedVibrate(startTime, cmd.waitTime, cmd.duration, cmd.intensity / 2, cmd.rampUp, cmd.rampDown));

            // Option 2: Send to watch over network (if watch is networked)
            // watchTCP.SendVibrationCommand(cmd);
        }
    }

    private IEnumerator DelayedVibrate(long startTime, float waitTime, float duration, int intensity, int rampUp = 0, int rampDown = 0)
    {
        while (GetMasterClockTime() < startTime)
        {
            yield return new WaitForSeconds(0.001f);
        }
        yield return new WaitForSeconds(waitTime);
        VibrateRamp(duration, intensity, rampUp, rampDown);
    }

    private long GetMasterClockTime()
    {
        return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

[System.Serializable]
public class PatternInfo
{
    public long sendTime;
    public long startTime;
    public List<TactilePulse> tactilePulses;
    public List<ThermalPulse> thermalPulses;

    public PatternInfo()
    {
        tactilePulses = new List<TactilePulse>();
        thermalPulses = new List<ThermalPulse>();
    }
}

[System.Serializable]
public class WatchCommand
{
    public float waitTime;
    public float duration; 
    public int intensity;
    public int rampUp;
    public int rampDown;

    public WatchCommand(float wait, float dur, int intens, int up = 0, int down = 0)
    {
        waitTime = wait;
        duration = dur;
        intensity = intens;
        rampUp = up;
        rampDown = down;
    }
}


[System.Serializable]
public class TactilePulse
{
    public int index;
    public float waitTime;
    public float duration;
    public int intensity;
    public int rampUp;
    public int rampDown;

    public TactilePulse(int ind, float wait, float dur, int intens, int up = 0, int down = 0)
    {
        index = ind;
        waitTime = wait;
        duration = dur;
        intensity = intens;
        rampUp = up;
        rampDown = down;
    }
}

[System.Serializable]
public class ThermalPulse
{
    public int index;
    public float waitTime;
    public float duration;
    public float voltage;
    public int polarity;

    public ThermalPulse(int ind, float wait, float dur, float volt, int pol)
    {
        index = ind;
        waitTime = wait;
        duration = dur;
        voltage = volt;
        polarity = pol;
    }
}