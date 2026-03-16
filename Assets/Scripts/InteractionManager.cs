using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public int lastIndex;

    [Space(10)]
    [Header("AC")]
    public GameObject acUI;
    public int currentTemp;
    public float tempLevels = 16f;

    [Space(10)]
    [Header("Breathing")]
    public GameObject breathingUI;

    [Space(10)]
    [Header("Phone")]
    public GameObject phoneUI;
    public List<GameObject> emotes;
    public int emoteIndex;
    public int emoteLevels = 3;
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
        hapticManager.GoodMorning();

        
        yield return new WaitForSeconds(1.5f);
        hapticManager.LetsGetReady();
        StartCoroutine(GradualColorChange(background, new Color(38 / 255f, 99 / 255f, 125 / 255f), 3));
        yield return new WaitForSeconds(.5f);
        greetingsUI.transform.Find("Lets").gameObject.SetActive(true);
        greetingsUI.transform.Find("Effect").GetComponent<ParticleSystem>().Play();
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
        yield return new WaitForSeconds(2f);
        weatherUI.transform.Find("SpinControl").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(RotateFullCircle(weatherUI.transform.Find("SpinControl").gameObject.GetComponent<RectTransform>(), 1.5f));
        

        // Forecasts
        float dialCooldown = .1f;
        bool dialReady = true;
        dialActive = true;
        int currentForecastIndex = 0;
        weatherUI.transform.Find("SpinControl").Find("Text").GetComponent<TMP_Text>().text = "turn";

        lastIndex = -1;

        while (dialActive)
        {
            float delta = dialManager.lastDelta;
            dialManager.lastDelta = 0f;

            if ((dialReady && Mathf.Abs(delta) > 0.5f) || Input.GetKey(KeyCode.R))
            {
                dialReady = false;
                weatherUI.transform.Find("SpinControl").gameObject.SetActive(true);

                RectTransform rtSpin = weatherUI.transform.Find("SpinControl").GetComponent<RectTransform>();

                if (delta < 0)
                {
                    // ── Clockwise ──────────────────────────────
                    rtSpin.rotation = Quaternion.Euler(0f, 0f, rtSpin.rotation.eulerAngles.z - 30f);
                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    rtSpin.rotation = Quaternion.Euler(0f, 0f, rtSpin.rotation.eulerAngles.z + 30f);
                }

                float z = Mathf.Round(rtSpin.rotation.eulerAngles.z);

                // 12 zones of 30 degrees each, starting at 0
                // 0=blank, 1=9am, 2=10am, 3=11am, 4=12pm, 5=1pm, 6=2pm, 7=3pm, 8=4pm, 9=5pm, 10=next, 11=next
                if (z >= 330f && z <= 359f)
                    currentForecastIndex = 5;
                else if (z >= 0f && z <= 29f)
                    currentForecastIndex = 4;
                else if (z >= 30f && z <= 59f)
                    currentForecastIndex = 3;
                else if (z >= 60f && z <= 89f)
                    currentForecastIndex = 2;
                else if (z >= 90f && z <= 119f)
                    currentForecastIndex = 1;
                else if (z >= 120f && z <= 149f)
                    currentForecastIndex = 0;
                else if (z >= 150f && z <= 179f)
                    currentForecastIndex = 11;
                else if (z >= 180f && z <= 209f)
                    currentForecastIndex = 10;
                else if (z >= 210f && z <= 239f)
                    currentForecastIndex = 9;
                else if (z >= 240f && z <= 269f)
                    currentForecastIndex = 8;
                else if (z >= 270f && z <= 299f)
                    currentForecastIndex = 7;
                else if (z >= 300f && z <= 329f)
                    currentForecastIndex = 6;
                    

                currentForecastIndex = Mathf.Clamp(currentForecastIndex, 0, 10);
                Debug.Log($"z={z} index={currentForecastIndex}");

                TMP_Text spinLabel = weatherUI.transform.Find("SpinControl").Find("Text").GetComponent<TMP_Text>();
                string[] forecastLabels = { "turn", "9am", "10am", "11am", "12pm", "1pm", "2pm", "3pm", "4pm", "5pm", "next", "next" };
                spinLabel.text = forecastLabels[currentForecastIndex];

                if (z == 0f || z == 180f)
                {
                    weatherUI.transform.Find("SpinControl").Find("Text").GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else
                {
                    weatherUI.transform.Find("SpinControl").Find("Text").GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                if (z % 30f == 0f)
                {
                    StartCoroutine(SetForecast(currentForecastIndex));
                }
                // Cooldown: flush delta then re-enable after delay
                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f; // discard any input during cooldown
                    dialReady = true;
                }));
            }

            yield return null;
        }
    }

    IEnumerator ReenableDial(float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    IEnumerator SetForecast(int currentIndex)
    {
        foreach (GameObject forecast in forecasts)
        {
            forecast.SetActive(false);
        }

        forecasts[(currentIndex + 2) / 3].SetActive(true);

        if (currentIndex == 0 || currentIndex == 10 || currentIndex == 11 && !(lastIndex == 0 || lastIndex == 10 || lastIndex == 11)) {
            hapticManager.Interrupt();
            //StartCoroutine(GradualColorChange(background, Color.white, 0.3f));
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(GradualColorChange(background, Color.black, 1f));
            hapticManager.GoodMorning();
        } else if (currentIndex >= 1 && currentIndex <= 3 && !(lastIndex >= 1 && lastIndex <= 3))
        {   
            StartCoroutine(GradualColorChange(background, new Color32(204, 194, 86, 255), 1));   // Sunny
            hapticManager.Interrupt();
            yield return new WaitForSeconds(0.1f);
            hapticManager.Sunny();
        }
        else if (currentIndex >= 4 && currentIndex <= 6 && !(lastIndex >= 4 && lastIndex <= 6))
        {
            StartCoroutine(GradualColorChange(background, new Color32(98, 134, 156, 255), 1));   // Windy
            hapticManager.Interrupt();
            yield return new WaitForSeconds(0.1f);
            hapticManager.Windy();
        }
        else if (currentIndex >= 7 && currentIndex <= 9 && !(lastIndex >= 7 && lastIndex <= 9))
        {
            StartCoroutine(GradualColorChange(background, new Color32(57, 63, 115, 255), 1));   // Rainy
            hapticManager.Interrupt();
            yield return new WaitForSeconds(0.1f);
            hapticManager.Rainy();
        }

        lastIndex = currentIndex;
    }

    public void ResetWeather()
    {
        dialActive = false;
        foreach (GameObject forecast in forecasts)
        {
            forecast.SetActive(false);
        }
        weatherUI.transform.Find("Intro").gameObject.SetActive(false);
        weatherUI.transform.Find("Intro").Find("SpinArrow").gameObject.SetActive(false);
        weatherUI.transform.Find("SpinControl").gameObject.SetActive(false);
        weatherUI.SetActive(false);
        StartCoroutine(GradualColorChange(background, Color.black, 1));

        AC();
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void AC()
    {
        StartCoroutine(ACHelper());
    }

    public IEnumerator ACHelper()
    {
        acUI.SetActive(true);
        yield return new WaitForSeconds(1);

        // Intro
        acUI.transform.Find("Intro").gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        acUI.transform.Find("Intro").gameObject.SetActive(false);
        acUI.transform.Find("SpinControl").gameObject.SetActive(true);
        acUI.transform.Find("Temp").gameObject.SetActive(true);
        acUI.transform.Find("Degree").gameObject.SetActive(true);
        acUI.transform.Find("Exit").gameObject.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(RotateFullCircle(weatherUI.transform.Find("SpinControl").gameObject.GetComponent<RectTransform>(), 1.5f));

        float dialCooldown = .1f;
        bool dialReady = true;
        dialActive = true;
        currentTemp = 32;
        acUI.transform.Find("Temp").gameObject.GetComponent<TMP_Text>().text = currentTemp.ToString();

        while (dialActive)
        {
            float delta = dialManager.lastDelta;
            dialManager.lastDelta = 0f;

            if ((dialReady && Mathf.Abs(delta) > 0.5f) || Input.GetKey(KeyCode.R))
            {
                if (delta < 0)
                {
                    // ── Clockwise ──────────────────────────────
                    if (currentTemp < 48) {
                        currentTemp++;
                    }
                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    if (currentTemp > 16)
                    {
                        currentTemp--;
                    }
                }

                acUI.transform.Find("Temp").gameObject.GetComponent<TMP_Text>().text = currentTemp.ToString();
                UpdateACBackground();
                hapticManager.Interrupt();
                hapticManager.AC(currentTemp);
                // Cooldown: flush delta then re-enable after delay
                StartCoroutine(ReenableDial(dialCooldown, () =>
                {
                    dialManager.lastDelta = 0f; // discard any input during cooldown
                    dialReady = true;
                }));
            }

            yield return null;
        }

        void UpdateACBackground()
        {
            Color targetColor;

            if (currentTemp >= 32)
            {
                // neutral → hot (white → red)
                targetColor = Color.Lerp(Color.black, Color.red, (float)(currentTemp - 32f) / (48f - 32f));
            }
            else
            {
                // cold → neutral (blue → white)
                targetColor = Color.Lerp(Color.blue, Color.black, (float)(currentTemp - 16f) / (32f - 16f));
            }

            StartCoroutine(GradualColorChange(background, targetColor, 0.1f));
        }
    }

    public void ResetAC()
    {
        dialActive = false;
        acUI.transform.Find("SpinControl").gameObject.SetActive(false);
        acUI.transform.Find("Temp").gameObject.SetActive(false);
        acUI.transform.Find("Degree").gameObject.SetActive(false);
        acUI.transform.Find("Exit").gameObject.SetActive(false);
        acUI.SetActive(false);
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
            if (i != 2) {
                yield return new WaitForSeconds(2f);
                psIn.Play();
                hapticManager.Breathing();
                yield return new WaitForSeconds(6f);
                psOut.Play();
                yield return new WaitForSeconds(3f);
            } else
            {
                yield return new WaitForSeconds(2f);
                psIn.Play();
                hapticManager.Breathing(true);
                yield return new WaitForSeconds(4f);
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
        hapticManager.Call();
        yield return new WaitForSeconds(4);

        // Response
        StartCoroutine(GradualColorChange(background, Color.black, 0.3f));
        phoneUI.transform.Find("Call").gameObject.SetActive(false);
        phoneUI.transform.Find("Response").gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        phoneUI.transform.Find("Response").Find("Intro").gameObject.SetActive(false);
        phoneUI.transform.Find("Controls").gameObject.SetActive(true);

        emoteIndex = 3;
        SetEmote();

        dialActive = true;
        float dialCooldown = 0.1f;
        bool dialReady = true;

        while (dialActive)
        {

            // ── Dial Detection ────────────────────────────────
            float delta = dialManager.lastDelta;
            dialManager.lastDelta = 0f;

            if (dialReady && (Mathf.Abs(delta) > 0.5f || Input.GetKeyDown(KeyCode.T)))
            {
                dialReady = false;

                if (delta < 0 || Input.GetKey(KeyCode.R))
                {
                    // ── Clockwise ──────────────────────────────
                    emoteIndex = Mathf.Min(emoteIndex + 1, emoteLevels * 2);
                }
                else
                {
                    // ── Counter-clockwise ──────────────────────
                    emoteIndex = Mathf.Max(emoteIndex - 1, 0);
                }

                readyToSend = true;
                SetEmote();

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

    void SetEmote()
    {
        hapticManager.Interrupt();
        UpdateThermalBackground();
        for (int i = 0; i < emotes.Count; i++)
            emotes[i].SetActive(i == emoteIndex);
        if (emoteIndex == 0)
        {
            hapticManager.Angry3();
        } 
        else if (emoteIndex == 1)
        {
            hapticManager.Angry2();
        }
        else if (emoteIndex == 2) 
        {
            hapticManager.Angry1();
        }
        else if (emoteIndex == 3) 
        {
            hapticManager.Neutral();
        }
        else if (emoteIndex == 4) 
        {
            hapticManager.Happy1();
        }
        else if (emoteIndex == 5) 
        {
            hapticManager.Happy2();
        }
        else if (emoteIndex == 6) 
        {
            hapticManager.Happy3();
        } 
        else
        {
            StartCoroutine(GradualColorChange(background, Color.black, 1f));
        }
    }

    void UpdateThermalBackground()
    {
        Color targetColor;

        if (emoteIndex >= 3)
        {
            // neutral → hot (white → red)
            targetColor = Color.Lerp(Color.black, new Color32(31, 99, 76, 255), (float)(emoteIndex - 3) / emoteLevels);
        }
        else
        {
            // cold → neutral (blue → white)
            targetColor = Color.Lerp(new Color32(99, 35, 31, 255), Color.black, ((float)(emoteIndex - 3) + emoteLevels) / emoteLevels);
        }

        StartCoroutine(GradualColorChange(background, targetColor, 0.25f));
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

    public void SendEmote()
    {
        StartCoroutine(Send());
    }

    public IEnumerator Send()
    {
        dialActive = false;
        phoneUI.transform.Find("Controls").gameObject.SetActive(false);
        phoneUI.transform.Find("Response").Find("Sent").gameObject.SetActive(true);
        StartCoroutine(Wiggle(emotes[emoteIndex].GetComponent<RectTransform>()));
        SetEmote();
        yield return new WaitForSeconds(4);
        ResetPhone();
    }

    public void ResetPhone()
    {
        phoneUI.SetActive(false);
        emoteIndex = -1;
        SetEmote();
        phoneUI.transform.Find("Response").Find("Sent").gameObject.SetActive(false);
        phoneUI.transform.Find("Response").Find("Intro").gameObject.SetActive(true);
        clockFace.gameObject.SetActive(true);
        StartCoroutine(GradualColorChange(background, Color.black, 0.5f));
        StartCoroutine(EndSession());
    }

    public IEnumerator EndSession()
    {
        phoneUI.transform.Find("Thanks").gameObject.SetActive(true);
        yield return new WaitForSeconds(3);
        phoneUI.transform.Find("Thanks").gameObject.SetActive(false);
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
