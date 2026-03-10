using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject menuMain;
    public GameObject menuBreathing;
    public GameObject menuNotifications;

    public Coroutine currentTask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoToMain()
    {
        menuMain.SetActive(true);
        menuBreathing.SetActive(false);
        menuNotifications.SetActive(false);
    }

    public void GoToBreathing()
    {
        menuMain.SetActive(false);
        menuBreathing.SetActive(true);
    }

    public void GoToNotifications()
    {
        menuMain.SetActive(false);
        menuNotifications.SetActive(true);
    }
}
