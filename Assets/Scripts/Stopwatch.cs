using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stopwatch : MonoBehaviour
{
    float currentTime = 0;
    bool timeStarted;
    public TMP_Text displayTime;

    void Start()
    {
        currentTime = 0;
        timeStarted = false;
    }

    void Update()
    {
        if (timeStarted)

        {
            currentTime += Time.deltaTime;
        }

        displayTime.text = $"{Mathf.FloorToInt(currentTime / 60)}:{Mathf.FloorToInt(currentTime % 60)}";
    }

    public void StartTime()
    {
        timeStarted = true;
    }

    public void StopTime()
    {
        timeStarted = false;
    }

    public void ResetTime()
    {
        timeStarted = false;
        currentTime = 0;
    }
}
