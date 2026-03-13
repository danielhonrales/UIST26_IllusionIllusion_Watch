using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationsManager : MonoBehaviour
{

    public HapticManager hapticManager;

    [Header("Menus")]
    public GameObject menuApps;
    public GameObject menuMessaging;
    public GameObject menuWeather;
    public GameObject menuAlarm;
    [Space(10)]

    [Header("Messaging")]
    public GameObject neutralBackground;
    public GameObject madBackground;
    public GameObject happyBackground;
    public GameObject incoming;
    public GameObject areYouComing;
    public TMP_Text dots;
    public GameObject mad;
    public GameObject happy;
    public GameObject reactions;
    [Space(10)]

    [Header("Weather")]
    public TMP_Text looksLike;
    public GameObject hot;
    public GameObject rain;
    [Space(10)]
    
    [Header("Alarm")]
    public TMP_Text countdown;
    public Button alarmButton;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMessaging()
    {
        menuApps.SetActive(false);
        menuMessaging.SetActive(true);
    }

    public void OpenWeather()
    {
        menuApps.SetActive(false);
        menuWeather.SetActive(true);

        StartCoroutine(Weather());
    }

    public void Messaging()
    {
        StartCoroutine(MessagingHelper());
    }

    public IEnumerator MessagingHelper()
    {
        yield return new WaitForSeconds(1);
        incoming.SetActive(true);
        areYouComing.SetActive(true);
        //hapticManager.Incoming();

        yield return new WaitForSeconds(1);
        reactions.SetActive(true);
    }

    public void Mad()
    {
        reactions.SetActive(false);
        areYouComing.SetActive(false);
        StartCoroutine(MadHelper());
    }

    public IEnumerator MadHelper()
    {

        for (int i = 0; i < 3; i++)
        {
            dots.text = ".";
            yield return new WaitForSeconds(0.4f);
            dots.text = "..";
            yield return new WaitForSeconds(0.4f);
            dots.text = "...";
            yield return new WaitForSeconds(0.4f);
            if (i == 0)
            {
                //hapticManager.Mad();
            }
        }
        dots.text = "";

        mad.SetActive(true);
        neutralBackground.SetActive(false);
        madBackground.SetActive(true);
        ;
        yield return new WaitForSeconds(3);

        mad.SetActive(false);
        neutralBackground.SetActive(true);
        madBackground.SetActive(false);
        OpenAppMenu();
    }

    public void Happy()
    {
        reactions.SetActive(false);
        areYouComing.SetActive(false);
        StartCoroutine(HappyHelper());
    }

    public IEnumerator HappyHelper()
    {
        
        for (int i = 0; i < 3; i++)
        {
            dots.text = ".";
            yield return new WaitForSeconds(0.4f);
            dots.text = "..";
            yield return new WaitForSeconds(0.4f);
            dots.text = "...";
            yield return new WaitForSeconds(0.4f);
            if (i == 0)
            {
                //hapticManager.Happy();
            }
        }
        dots.text = "";

        happy.SetActive(true);
        
        neutralBackground.SetActive(false);
        happyBackground.SetActive(true);
        
        yield return new WaitForSeconds(3);
        happy.SetActive(false);
        neutralBackground.SetActive(true);
        happyBackground.SetActive(false);
        OpenAppMenu();
    }

    public IEnumerator Weather()
    {
        yield return new WaitForSeconds(0.5f);
        Coroutine helper = StartCoroutine(WeatherHelper());

        hapticManager.Hot();
        yield return new WaitForSeconds(2);

        hot.SetActive(true);
        
        yield return new WaitForSeconds(2);
        hapticManager.Rainy();
        yield return new WaitForSeconds(1);
        hot.SetActive(false);

        yield return new WaitForSeconds(0.75f);

        rain.SetActive(true);
        //hapticManager.Rain();
        yield return new WaitForSeconds(3);
        yield return new WaitForSeconds(2);
        rain.SetActive(false);

        yield return new WaitForSeconds(2f);
        StopCoroutine(helper);
        OpenAppMenu();
    }

    public IEnumerator WeatherHelper()
    {
        while (true)
        {
            looksLike.text = "Looks like.";
            yield return new WaitForSeconds(0.5f);
            looksLike.text = "Looks like..";
            yield return new WaitForSeconds(0.5f);
            looksLike.text = "Looks like...";
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OpenAlarm()
    {
        menuApps.SetActive(false);
        menuAlarm.SetActive(true);
    }

    public void Alarm()
    {
        alarmButton.enabled = false;
        Coroutine helper = StartCoroutine(AlarmHelper());
    }

    public IEnumerator AlarmHelper()
    {
        for (int i = 5; i > -1; i--)
        {
            countdown.text = string.Format("00:0{0}", i);
            if (i == 2) {
                hapticManager.Alarm();
            }
            if (i != 0) {
                yield return new WaitForSeconds(1);
            }
        }

        countdown.color = Color.red;
        
        yield return new WaitForSeconds(4);
        countdown.color = Color.white;
        alarmButton.enabled = true;
        OpenAppMenu();
    }

    public void OpenAppMenu()
    {
        menuApps.SetActive(true);
        menuMessaging.SetActive(false);
        menuWeather.SetActive(false);
        menuAlarm.SetActive(false);
    }
}
