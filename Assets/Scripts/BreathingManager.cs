using System.Collections;
using UnityEngine;

public class BreathingManager : MonoBehaviour
{

    public ParticleSystem psIn;
    public ParticleSystem psOut;
    public HapticManager hapticManager;
    private Coroutine breathingTask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        breathingTask = StartCoroutine(BreathingTask());
    }

    void OnDisable()
    {
        StopCoroutine(BreathingTask());
    }

    public IEnumerator BreathingTask()
    {
        hapticManager.Breathing();
        yield return new WaitForSeconds(2f);
        psIn.Play();
        yield return new WaitForSeconds(6f);
        psOut.Play();
        yield return new WaitForSeconds(4f);

        //yield return new WaitForSeconds(1);
        breathingTask = StartCoroutine(BreathingTask());
    }
}
