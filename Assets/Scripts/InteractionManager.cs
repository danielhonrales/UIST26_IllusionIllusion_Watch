using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{

    [Header("Bruh")]
    public DialManager dialManager;
    public Image background;

    [Space(10)]
    [Header("Start")]
    public GameObject startUI;

    [Space(10)]
    [Header("Greetings")]
    public GameObject greetingsUI;

    [Space(10)]
    [Header("Weather")]
    public GameObject weatherUI;
    public List<GameObject> forecasts;
    public bool _weatherDialActive = false;

    [Space(10)]
    [Header("Breathing")]
    public GameObject breathingUI;
    

    void Start()
    {
        // Subscribe to bezel events
        dialManager.OnBezelValue.AddListener(OnIntensityChanged);
        dialManager.OnBezelDirection.AddListener(OnDirectionChanged);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Greetings()
    {
        StartCoroutine(GreetingsHelper());
    }

    public IEnumerator GreetingsHelper()
    {
        startUI.SetActive(false);
        greetingsUI.SetActive(true);
        yield return new WaitForSeconds(2);
        greetingsUI.transform.Find("GoodMorning").gameObject.SetActive(true);
        // haptic effect
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(GradualColorChange(background, new Color(38 / 255f, 99 / 255f, 125 / 255f), 3));
        yield return new WaitForSeconds(.5f);
        greetingsUI.transform.Find("Lets").gameObject.SetActive(true);
        // haptic effect
        yield return new WaitForSeconds(5);

        ResetGreetings();
    }

    public void ResetGreetings()
    {
        greetingsUI.transform.Find("GoodMorning").gameObject.SetActive(false);
        greetingsUI.transform.Find("Lets").gameObject.SetActive(false);
        greetingsUI.SetActive(false);
        StartCoroutine(GradualColorChange(background, Color.black, 1));

        Weather();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Weather()
    {
        StartCoroutine(WeatherHelper());
    }

    public IEnumerator WeatherHelper()
    {
        weatherUI.SetActive(true);
        yield return new WaitForSeconds(1);

        // Intro
        weatherUI.transform.Find("Intro").gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        weatherUI.transform.Find("Intro").Find("SpinArrow").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.75f);
        StartCoroutine(RotateFullCircle(weatherUI.transform.Find("Intro").Find("SpinArrow").GetComponent<RectTransform>(), 1.5f));

        // Forecasts
        float dialCooldown = 5f;
        float lastDialTime = -dialCooldown;
        _weatherDialActive = true;
        int currentForecastIndex = 0;

        while (_weatherDialActive)
        {
            float delta = dialManager.lastDelta;

            if (Mathf.Abs(delta) > 0.01f && Time.time - lastDialTime >= dialCooldown)
            {
                lastDialTime = Time.time;
                weatherUI.transform.Find("SpinControl").gameObject.SetActive(true);

                if (delta > 0)
                {
                    // ── Clockwise ──────────────────────────────
                    int nextIndex = Mathf.Min(currentForecastIndex + 1, forecasts.Count - 1);
                    SetForecast(currentForecastIndex, nextIndex);
                    currentForecastIndex = nextIndex;

                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    int nextIndex = Mathf.Max(currentForecastIndex - 1, 0);
                    SetForecast(currentForecastIndex, nextIndex);
                    currentForecastIndex = nextIndex;

                }
            }

            yield return null;
        }
    }

    void SetForecast(int currentIndex, int nextIndex)
    {
        if (currentIndex == nextIndex) return; // already at boundary
        forecasts[currentIndex].SetActive(false);
        forecasts[nextIndex].SetActive(true);

        // maybe fade in
        if (nextIndex == 0 )
        {
            StartCoroutine(GradualColorChange(background, Color.black, 1));
        }
        else if (nextIndex == 1)
        {   
            StartCoroutine(GradualColorChange(background, Color.red, 3));   // Sunny
            // haptic effect
        }
        else if (nextIndex == 2)
        {
            StartCoroutine(GradualColorChange(background, Color.gray, 3));   // Windy
            // haptic effect
        }
        else if (nextIndex == 3)
        {
            StartCoroutine(GradualColorChange(background, Color.blue, 3));   // Rainy
            // haptic effect
        } 
        else if (nextIndex == 4)
        {
            StartCoroutine(GradualColorChange(background, Color.black, 1));
        }

    }

    public void ResetWeather()
    {
        _weatherDialActive = false;
        weatherUI.transform.Find("Intro").gameObject.SetActive(false);
        weatherUI.transform.Find("Intro").Find("SpinArrow").gameObject.SetActive(false);
        weatherUI.transform.Find("SpinControl").gameObject.SetActive(false);
        weatherUI.SetActive(false);
        StartCoroutine(GradualColorChange(background, Color.black, 1));

        Breathing();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Breathing()
    {
        StartCoroutine(BreathingHelper());
    }

    public IEnumerator BreathingHelper()
    {
        breathingUI.SetActive(true);
        yield return new WaitForSeconds(1);

        // Intro
        breathingUI.transform.Find("Intro").gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        breathingUI.transform.Find("Intro").gameObject.SetActive(false);

        // Breathing exercise
        breathingUI.transform.Find("Exercise").gameObject.SetActive(true);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void OnIntensityChanged(float value)
    {
        int intensity = Mathf.RoundToInt(value * 255f);
        background.color = new Color(intensity, 0, 0, 1);
        Debug.Log($"Haptic intensity: {intensity}");

        // Feed into your VibrateRamp
        
    }

    void OnDirectionChanged(string direction)
    {
        if (direction == "cw")
            Debug.Log("Increase thermal → warm");
        else
            Debug.Log("Decrease thermal → cool");
    }

    IEnumerator GradualColorChange(Image image, Color targetColor, float duration)
    {
        Color startColor = image.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }

        image.color = targetColor; // ensure exact final color
    }

    IEnumerator RotateFullCircle(RectTransform rect, float duration = 1f)
    {
        float elapsed = 0f;
        Quaternion startRotation = rect.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(0f, 360f, elapsed / duration);
            rect.localRotation = startRotation * Quaternion.Euler(0f, 0f, -angle);
            yield return null;
        }

        rect.localRotation = startRotation; // restore exact original rotation
    }
}
