using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class GamePiece: MonoBehaviour
{
    public PieceNames pieceType;
    public Transform owner;
    public Rigidbody rb;
    public GamePieceState state;
    public GameObject colliderParent;
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public float startingDistance;

    void Start()
    {
        originalParent = transform.parent;
    }
}
