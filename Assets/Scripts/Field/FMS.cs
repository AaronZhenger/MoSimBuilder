using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Util;

public class FMS : MonoBehaviour
{
    public int matchTime = 150;
    public int autoTime = 15;
    public float autoDisableTime = 0.5f;
    public int endgameTime = 20;
    public float matchDisabledTime = 3;
    public GameObject[] blueStationCams;
    public GameObject[] redStationCams;
    public static float MatchTimer;
    public static RobotState RobotState;
    public static MatchState MatchState;
    public MatchState state;

    private MatchState previousMatchState;

    private LoadMatch matchLoader;
    private TextMeshProUGUI timer;

    public RobotState robotState;

    public static float ShiftTimer;
    public static string ShiftName = "";

    private TextMeshProUGUI _matchStateLabel;
    private float _autoEndTime;

    // Start is called before the first frame update
    void OnEnable()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        state = MatchState;
        robotState = RobotState;
        if (robotState == RobotState.enabled) MatchTimer -= Time.deltaTime;

        if (MatchTimer >= matchTime - autoTime)
        {
            MatchState = MatchState.auto;
        }  else if (MatchTimer >= endgameTime)
        {
            MatchState = MatchState.teleop;
        }
        else if (MatchTimer < 0)
        {
            MatchState = MatchState.finished;
        } else if (MatchTimer <= endgameTime)
        {
            MatchState = MatchState.endgame;
        }

        if (MatchState != previousMatchState && MatchState != MatchState.endgame)
        {
            switch (MatchState)
            {
                case MatchState.teleop:
                    MatchState = MatchState.auto;
                    StartCoroutine(wait(autoDisableTime));
                    MatchState = MatchState.teleop;
                    break;
                case MatchState.finished:
                    MatchState = MatchState.endgame;
                    StartCoroutine(wait(matchDisabledTime));
                    MatchState = MatchState.finished;
                    break;
            }
        }

        previousMatchState = MatchState;

        float minutes = Mathf.FloorToInt(MatchTimer / 60);

        // The remainder after dividing by 60 gives the remaining seconds
        float seconds = Mathf.FloorToInt(MatchTimer % 60);

        if (minutes < 0) minutes = 0;
        if (seconds < 0) seconds = 0;

        if (timer != null)
        {
            timer.text = $"{minutes:00}:{seconds:00}";
        }

        UpdateOverlay();
    }

    private IEnumerator wait(float time)
    {
        RobotState = RobotState.disabled;
        yield return new WaitForSeconds(time);
        RobotState = RobotState.enabled;
    }

    public void Restart()
    {
        var dispT = GameObject.Find("TimerDisplay");
        if (dispT != null)
        {
            timer = dispT.GetComponent<TextMeshProUGUI>();
        }

        matchLoader = Utils.FindParentObjectComponent<LoadMatch>(gameObject);
        matchLoader.setFMS(this);
        MatchTimer = matchTime;
        _autoEndTime = matchTime - autoTime;
        previousMatchState = MatchState.auto;
        MatchState = MatchState.auto;
        RobotState = RobotState.enabled;
        ShiftTimer = 0;
        ShiftName = "";
    }

    private void UpdateOverlay()
    {
        if (_matchStateLabel == null)
        {
            var gameUi = GameObject.Find("GameUi");
            if (gameUi == null) return;
            foreach (var t in gameUi.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "MatchStateLabel")
                {
                    _matchStateLabel = t.GetComponent<TextMeshProUGUI>();
                    break;
                }
            }
            if (_matchStateLabel == null) return;
        }

        string stateText;
        switch (MatchState)
        {
            case MatchState.auto:
                float autoRemaining = MatchTimer - _autoEndTime;
                int autoSec = Mathf.CeilToInt(Mathf.Max(autoRemaining, 0f));
                stateText = $"Auto :{autoSec:D2}";
                break;
            case MatchState.teleop:
            case MatchState.endgame:
                if (ShiftTimer > 0 && ShiftName.Length > 0)
                {
                    int shiftSec = Mathf.CeilToInt(ShiftTimer);
                    stateText = $"{ShiftName} :{shiftSec:D2}";
                }
                else
                {
                    stateText = "";
                }
                break;
            case MatchState.finished:
                stateText = "Match Over";
                break;
            default:
                stateText = "";
                break;
        }
        _matchStateLabel.text = stateText;
    }
}

[Serializable]
public enum RobotState
{
    enabled,
    disabled,
}

[Serializable]
public enum MatchState
{
    auto,
    teleop,
    endgame,
    finished
}