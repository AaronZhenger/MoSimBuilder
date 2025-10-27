using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class SpawnPieceTarget : MonoBehaviour
{
    public SpawnType spawnType;

    public float SpawnDistance;

    public float Velocity;
    // Start is called before the first frame update
    void Start()
    {
        SpawnGamePiece.Targets.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
