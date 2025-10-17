using UnityEngine;

public class Timer : MonoBehaviour
{
    TMPro.TextMeshProUGUI timerText;
    float timer;
    string timerTextString;
    void Start()
    {
        timerText = GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 60f) 
        { 
            int minutes = (int)(timer / 60);
            int seconds = (int)(timer % 60);
            timerTextString = $"{minutes:0}:{seconds:00}";
            timerText.text = timerTextString;


        }
        else
        {
            timerTextString = $"{timer:0}";
            timerText.text = timerTextString;
        }


    }

    public string GetTime()
    {
        return timerTextString;
    }
}
