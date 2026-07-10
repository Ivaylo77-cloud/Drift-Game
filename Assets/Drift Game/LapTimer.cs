using UnityEngine;
using TMPro;

public class LapTimer : MonoBehaviour
{
    public static LapTimer Instance;

    public TMP_Text bestLapText;
    public TMP_Text previousLapText;

    private float lapStartTime;
    private float bestLap = Mathf.Infinity;

    private bool firstLap = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        lapStartTime = Time.time;
    }

    public void FinishLap()
    {

        Debug.Log("Lap Finished!");

        float lapTime = Time.time - lapStartTime;

        if (!firstLap)
        {
            previousLapText.text = "Previous: " + lapTime.ToString("F2") + " s";

            if (lapTime < bestLap)
            {
                bestLap = lapTime;
                bestLapText.text = "Best: " + bestLap.ToString("F2") + " s";
            }
        }

        firstLap = false;
        lapStartTime = Time.time;
    }
}