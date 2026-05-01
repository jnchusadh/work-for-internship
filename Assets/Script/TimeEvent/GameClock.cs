using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private TextMeshProUGUI dayText = null;
    [SerializeField] private TextMeshProUGUI seasonText = null;
    [SerializeField] private TextMeshProUGUI yearText = null;
    private void OnEnable()
    {
        EventHandler.gameTimeMinuteEvent += UpdateGameTime;
    }
    private void OnDisable()
    {
        EventHandler.gameTimeMinuteEvent -= UpdateGameTime;
    }

    private void UpdateGameTime(int gameYear, Season season, int gameDay, string gameWeek, int gameHour, int gameMinute, int gameSecond)
    {
        gameMinute = gameMinute - (gameMinute % 10);
        string ampm = "";
        string minute;
        if (gameHour > 12)
        {
            ampm = "pm";
        }
        else
        {
            ampm = "am";
        }
        if (gameHour > 13)
        {
            gameHour -= 12;
        }
        if (gameMinute < 10)
        {
            minute = "0" + gameMinute.ToString();
        }
        else
        {
            minute = gameMinute.ToString();
        }
        string time = gameHour.ToString() + ":" + minute + ampm;
        timeText.SetText(time);
        dayText.SetText(gameWeek + "." + gameDay.ToString());
        seasonText.SetText(season.ToString());
        yearText.SetText("Year" + gameYear);
    }
}
