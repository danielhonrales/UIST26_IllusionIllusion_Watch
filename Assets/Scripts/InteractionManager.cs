using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{

    [Header("Bruh")]
    public DialManager dialManager;
    public HapticManager hapticManager;
    public Image background;
    public Image clockFace;
    public bool dialActive = false;

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

    [Space(10)]
    [Header("Breathing")]
    public GameObject breathingUI;

    [Space(10)]
    [Header("Phone")]
    public GameObject phoneUI;
    public List<GameObject> emotes;
    public bool readyToSend = false;

    

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
        yield return new WaitForSeconds(1.5f);
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
        bool dialReady = true;
        dialActive = true;
        int currentForecastIndex = 0;

        while (dialActive)
        {
            float delta = dialManager.lastDelta;
            dialManager.lastDelta = 0f;

            if (dialReady && Mathf.Abs(delta) > 0.5f)
            {
                dialReady = false;
                weatherUI.transform.Find("SpinControl").gameObject.SetActive(true);

                if (delta < 0)
                {
                    // ── Clockwise ──────────────────────────────
                    int nextIndex = Mathf.Min(currentForecastIndex + 1, forecasts.Count - 1);
                    StartCoroutine(SetForecast(currentForecastIndex, nextIndex));
                    currentForecastIndex = nextIndex;
                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    int nextIndex = Mathf.Max(currentForecastIndex - 1, 0);
                    StartCoroutine(SetForecast(currentForecastIndex, nextIndex));
                    currentForecastIndex = nextIndex;
                }

                // Cooldown: flush delta then re-enable after delay
                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f; // discard any input during cooldown
                    dialReady = true;
                }));
            } else if (Input.GetKey(KeyCode.R))
            {
                int nextIndex = Mathf.Min(currentForecastIndex + 1, forecasts.Count - 1);
                StartCoroutine(SetForecast(currentForecastIndex, nextIndex));
                currentForecastIndex = nextIndex;

                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f; // discard any input during cooldown
                    dialReady = true;
                }));
            } else if (Input.GetKey(KeyCode.T)){
                int nextIndex = Mathf.Max(currentForecastIndex - 1, 0);
                StartCoroutine(SetForecast(currentForecastIndex, nextIndex));
                currentForecastIndex = nextIndex;

                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f; // discard any input during cooldown
                    dialReady = true;
                }));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ReenableDial(float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    IEnumerator SetForecast(int currentIndex, int nextIndex)
    {
        if (currentIndex == nextIndex) {
            // haptic effect
            StartCoroutine(GradualColorChange(background, Color.white, 0.3f));
            StartCoroutine(GradualColorChange(background, Color.black, 0.3f));
            yield return null; // already at boundary
        }
        forecasts[currentIndex].SetActive(false);

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

        yield return new WaitForSeconds(1);
        forecasts[nextIndex].SetActive(true);
    }

    public void ResetWeather()
    {
        dialActive = false;
        foreach (GameObject forecast in forecasts)
        {
            forecast.SetActive(true);
        }
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
        yield return new WaitForSeconds(1);
        breathingUI.SetActive(true);
        yield return new WaitForSeconds(1);

        // Intro
        breathingUI.transform.Find("Intro").gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        breathingUI.transform.Find("Intro").gameObject.SetActive(false);

        // Breathing exercise
        ParticleSystem psIn = breathingUI.transform.Find("Exercise").Find("BreatheIn").GetComponent<ParticleSystem>();
        ParticleSystem psOut = breathingUI.transform.Find("Exercise").Find("BreatheOut").GetComponent<ParticleSystem>();
        for (int i = 0; i < 3; i++)
        {
            hapticManager.Breathing();
            yield return new WaitForSeconds(2f);
            psIn.Play();
            yield return new WaitForSeconds(4.5f);

            if (i != 2) {
                psOut.Play();
                yield return new WaitForSeconds(4f);

                yield return new WaitForSeconds(1);
            }
        }

        ResetBreathing();
    }

    public void ResetBreathing()
    {
        breathingUI.SetActive(false);
        Phone();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void Phone()
    {
        StartCoroutine(PhoneHelper());
    }

    public IEnumerator PhoneHelper()
    {
        // Call
        clockFace.gameObject.SetActive(false);
        phoneUI.SetActive(true);
        phoneUI.transform.Find("Call").gameObject.SetActive(true);
        StartCoroutine(GradualColorChange(background, Color.white, 0.3f));
        StartCoroutine(Wiggle(phoneUI.transform.Find("Call").Find("Image").gameObject.GetComponent<RectTransform>(), 5f, 20f, 4f));
        // haptic effect
        yield return new WaitForSeconds(4);

        // Response
        StartCoroutine(GradualColorChange(background, Color.black, 0.3f));
        phoneUI.transform.Find("Call").gameObject.SetActive(false);
        phoneUI.transform.Find("Response").gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        phoneUI.transform.Find("Response").Find("Intro").gameObject.SetActive(false);
        phoneUI.transform.Find("Controls").gameObject.SetActive(true);

        int currentEmoteIndex = 0;
        SetEmote(currentEmoteIndex);
        int thermalVal = 0;

        Vector2 touchStartPos = Vector2.zero;
        bool isSwiping = false;
        float swipeThreshold = 5f;

        dialActive = true;
        float dialCooldown = 3f;
        bool dialReady = true;

        while (dialActive)
        {
            // ── Touch/Swipe Detection ──────────────────────────
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    isSwiping = true;
                }
                else if (touch.phase == TouchPhase.Ended && isSwiping)
                {
                    Vector2 swipeDelta = touch.position - touchStartPos;
                    isSwiping = false;

                    if (Mathf.Abs(swipeDelta.y) > swipeThreshold)
                    {
                        if (swipeDelta.y > 0)
                        {
                            currentEmoteIndex = (currentEmoteIndex + 1) % emotes.Count;
                            SetEmote(currentEmoteIndex);
                            readyToSend = true;
                        }
                        else
                        {
                            currentEmoteIndex = (currentEmoteIndex - 1 + emotes.Count) % emotes.Count;
                            SetEmote(currentEmoteIndex);
                            readyToSend = true;
                        }
                    }
                }else if (touch.phase == TouchPhase.Canceled)
                {
                    isSwiping = false;
                }
            }

            // ── Dial Detection ────────────────────────────────
            float delta = dialManager.lastDelta;
            dialManager.lastDelta = 0f;

            if (dialReady && (Mathf.Abs(delta) > 0.5f || Input.GetKeyDown(KeyCode.T)))
            {
                dialReady = false;

                if (delta < 0 || Input.GetKeyDown(KeyCode.T))
                {
                    // ── Clockwise ──────────────────────────────
                    thermalVal = Mathf.Min(thermalVal + 1, 3);
                    Debug.Log($"Thermal: {thermalVal}");
                    readyToSend = true;
                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    thermalVal = Mathf.Max(thermalVal - 1, -3);
                    Debug.Log($"Thermal: {thermalVal}");
                    readyToSend = true;
                }

                UpdateThermalBackground(thermalVal);
                // haptic effect

                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f;
                    dialReady = true;
                }));
            }

            yield return null;
        }
    }

    IEnumerator Wiggle(RectTransform rect, float angle = 15f, float speed = 20f, float duration = 1f)
    {
        float elapsed = 0f;
        Quaternion startRotation = rect.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float z = Mathf.Sin(elapsed * speed) * angle;
            rect.localRotation = startRotation * Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        rect.localRotation = startRotation;
    }

    void SetEmote(int index)
    {
        for (int i = 0; i < emotes.Count; i++)
            emotes[i].SetActive(i == index);
    }

    void UpdateThermalBackground(int thermalVal)
    {
        Color targetColor;

        if (thermalVal >= 0)
        {
            // neutral → hot (white → red)
            targetColor = Color.Lerp(Color.black, Color.red, thermalVal / 3f);
        }
        else
        {
            // cold → neutral (blue → white)
            targetColor = Color.Lerp(Color.blue, Color.black, (thermalVal + 3f) / 3f);
        }

        StartCoroutine(GradualColorChange(background, targetColor, 0.5f));
    }

    public void OnBackButton(string state)
    {
        if (state == "down" && readyToSend)
        {
            readyToSend = false;
            StartCoroutine(Send());
        }
    }

    public void OnHomeButton(string state)
    {
        if (state == "down")
            Debug.Log("Home button pressed");
    }

    public IEnumerator Send()
    {
        dialActive = false;
        phoneUI.transform.Find("Response").Find("Sent").gameObject.SetActive(true);
        // haptic effect
        yield return new WaitForSeconds(5);
        ResetPhone();
    }

    public void ResetPhone()
    {
        phoneUI.SetActive(false);
        SetEmote(-1);
        phoneUI.transform.Find("Response").Find("Sent").gameObject.SetActive(false);
        phoneUI.transform.Find("Controls").gameObject.SetActive(false);
        clockFace.gameObject.SetActive(true);
        StartCoroutine(GradualColorChange(background, Color.black, 0.5f));
        startUI.SetActive(true);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void OnIntensityChanged(float value)
    {
        int intensity = Mathf.RoundToInt(value * 255f);
        Debug.Log($"Haptic intensity: {intensity}");
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
