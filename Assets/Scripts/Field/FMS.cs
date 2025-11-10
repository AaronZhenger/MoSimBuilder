using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class FMS : MonoBehaviour
{
    public int matchTime = 150;
    public int autoTime = 15;
    public float autoDisableTime = 0.5f;
    public int endgameTime = 20;
    public float matchDisabledTime = 3;
    public static float MatchTimer;
    public static RobotState RobotState;
    public static MatchState MatchState;
    
    private MatchState previousMatchState;

    private LoadMatch matchLoader;
    // Start is called before the first frame update
    void OnEnable()
    {
        matchLoader.setFMS(this);
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        MatchTimer -= Time.deltaTime;

        if (MatchState != previousMatchState && MatchState != MatchState.endgame)
        {
            if (MatchState == MatchState.teleop)
            {
                StartCoroutine(wait(autoDisableTime));
            } else if (MatchState == MatchState.finished)
            {
                StartCoroutine(wait(matchDisabledTime));
            }
            
            previousMatchState = MatchState;
        }

        if (MatchTimer >= matchTime - autoTime)
        {
            MatchState = MatchState.auto;
        } else if (MatchTimer <= endgameTime)
        {
            MatchState = MatchState.endgame;
        } else if (MatchTimer >= 0)
        {
            MatchState = MatchState.teleop;
        }
        else
        {
            MatchState = MatchState.finished;
        }
        
        previousMatchState = MatchState;
    }

    private IEnumerator wait(float time)
    {
        RobotState = RobotState.disabled;
        yield return new WaitForSeconds(time);
        RobotState = RobotState.enabled;
    }

    public void Restart()
    {
        matchLoader = Utils.FindParentObjectComponent<LoadMatch>(gameObject);
        MatchTimer = matchTime;
        previousMatchState = MatchState.auto;
        MatchState = MatchState.auto;
        RobotState = RobotState.enabled;
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