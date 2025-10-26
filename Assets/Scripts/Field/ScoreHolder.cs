using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreHolder : MonoBehaviour
{
    public static int BlueScore;
    public static int RedScore;
    [SerializeField] int blueScore;
    // Start is called before the first frame update
    void Start()
    {
        BlueScore = 0;
        RedScore = 0;
    }

    void Update()
    {
        blueScore = BlueScore;
    }
}
