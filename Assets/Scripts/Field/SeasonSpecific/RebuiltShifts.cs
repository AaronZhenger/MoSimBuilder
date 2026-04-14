using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Random = System.Random;

public class RebuiltShifts : ScoreOnlyOnce
{
    [SerializeField] private CurrentShift currentShift;
    private bool blueWonAuto;
    private float shiftTimer;
    private MatchState previousMatchState;

    [SerializeField] private GameObject shiftOnLight;
    // Update is called once per frame
    private void Start()
    {
        blueWonAuto = false;
        shiftTimer = 3;
        currentShift = CurrentShift.Auto;
    }

    private new void FixedUpdate()
    {
        shiftOnLight.SetActive(isOnShift());

        poolOccupyObjects();

        handleShiftState();
        // Create a set of current objects for comparison
        compareObjects(isOnShift());

        ScorePoints(totalScore); // Pass the total accumulated score

        FMS.ShiftTimer = shiftTimer;
        switch (currentShift)
        {
            case CurrentShift.Auto:       FMS.ShiftName = "Auto"; break;
            case CurrentShift.Transition:  FMS.ShiftName = "1/6"; break;
            case CurrentShift.Shift1:      FMS.ShiftName = "2/6"; break;
            case CurrentShift.Shift2:      FMS.ShiftName = "3/6"; break;
            case CurrentShift.Shift3:      FMS.ShiftName = "4/6"; break;
            case CurrentShift.Shift4:      FMS.ShiftName = "5/6"; break;
            case CurrentShift.EndGame:     FMS.ShiftName = "6/6"; break;
            default:                       FMS.ShiftName = ""; break;
        }
    }

    private void handleShiftState()
    {
        if (FMS.MatchState != MatchState.auto && previousMatchState == MatchState.auto)
        {
            if (ScoreHolder.BlueScore > ScoreHolder.RedScore)
            {
                blueWonAuto = true;
            }else if (ScoreHolder.BlueScore == ScoreHolder.RedScore)
            {
                var rng = new Random();
                blueWonAuto = rng.Next(0, 1) == 1;
            }

            currentShift = CurrentShift.Auto;
        }

        if (FMS.MatchState is not MatchState.auto && currentShift != CurrentShift.EndGame)
        {
            shiftTimer -= Time.deltaTime;

            if (shiftTimer <= 0)
            {
                if (currentShift == CurrentShift.Auto)
                    shiftTimer = 10;
                else if (currentShift == CurrentShift.Shift4)
                    shiftTimer = 30;
                else
                    shiftTimer = 25;
                currentShift += 1;
            }
        }

        previousMatchState = FMS.MatchState;
    }

    private bool isOnShift()
    {
        var isBlue = GetIsBlue();

        if (blueWonAuto)
        {
            if (isBlue)
            {
                return currentShift is CurrentShift.Auto or CurrentShift.Transition or CurrentShift.Shift2 or CurrentShift.Shift4 or CurrentShift.EndGame;
            }
            else
            {
                return currentShift is CurrentShift.Auto or CurrentShift.Transition or CurrentShift.Shift1 or CurrentShift.Shift3 or CurrentShift.EndGame;
            }

        }
        else
        {
            if (isBlue)
            {
                return currentShift is CurrentShift.Auto or CurrentShift.Transition or CurrentShift.Shift1 or CurrentShift.Shift3 or CurrentShift.EndGame;
            }
            else
            {
                return currentShift is CurrentShift.Auto or CurrentShift.Transition or CurrentShift.Shift2 or CurrentShift.Shift4 or CurrentShift.EndGame;
            }
        }
    }

    [Serializable]
    public enum CurrentShift
    {
        Auto,
        Transition,
        Shift1,
        Shift2,
        Shift3,
        Shift4,
        EndGame,
    }
}