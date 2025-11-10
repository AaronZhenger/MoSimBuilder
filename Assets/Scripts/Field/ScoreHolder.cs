using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHolder : MonoBehaviour
{
    public static int BlueScore;
    public static int RedScore;
    [SerializeField] int blueScore;
    
    private TextMeshProUGUI redScoreDisplay;
    private TextMeshProUGUI blueScoreDisplay;
    // Start is called before the first frame update
    void Start()
    {
        BlueScore = 0;
        RedScore = 0;

        blueScoreDisplay = GameObject.Find("BlueScoreDisplay").GetComponent<TextMeshProUGUI>();
        
        redScoreDisplay = GameObject.Find("RedScoreDisplay").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        blueScore = BlueScore;
        
        blueScoreDisplay.text = BlueScore.ToString();
        redScoreDisplay.text = RedScore.ToString();
    }
}
