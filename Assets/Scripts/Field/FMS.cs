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
    public MatchState state;
    
    private MatchState previousMatchState;

    private LoadMatch matchLoader;

    public RobotState robotState;
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
        MatchTimer -= Time.deltaTime;

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
                    StartCoroutine(wait(autoDisableTime));
                    break;
                case MatchState.finished:
                    StartCoroutine(wait(matchDisabledTime));
                    break;
            }
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
        matchLoader.setFMS(this);
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