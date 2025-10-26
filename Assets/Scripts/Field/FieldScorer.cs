using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class FieldScorer : MonoBehaviour
{
    [SerializeField] private bool isBlue;
    [SerializeField] private int scoreToAdd;
    [SerializeField] private int autoScoreToAdd;
    [SerializeField] protected PieceNames[] scorePieces;
    [SerializeField] protected Collider[] occupyColliders;
    protected Vector3[] halfExtents;
    protected List<GamePiece> occupyObjects = new List<GamePiece>();

    private int lastAddedPoints;

    private void OnEnable()
    {
        occupyObjects = new List<GamePiece>();
        occupyObjects = new List<GamePiece>();
        halfExtents = new Vector3[occupyColliders.Length];
        for (int i = 0; i < occupyColliders.Length; i++)
        {
            halfExtents[i] = occupyColliders[i].bounds.extents / 2;
        }
    }

    protected void ScorePoints(int multiplyer = 1)
    {
        if (isBlue)
        {
            ScoreHolder.BlueScore -= lastAddedPoints;
            ScoreHolder.BlueScore += scoreToAdd * multiplyer;
        }
        else
        {
            ScoreHolder.RedScore -= lastAddedPoints;
            ScoreHolder.RedScore += scoreToAdd * multiplyer;
        }

        lastAddedPoints = scoreToAdd * multiplyer;
    }
    
    public List<GamePiece> getOccupyPieces()
    {
        return occupyObjects;
    }
    
    protected List<GamePiece> occupyPieces()
    {
        List<GamePiece> pieces = new List<GamePiece>();
        var mask = LayerMask.GetMask("Piece");
        for (int i = 0; i < occupyColliders.Length; i++)
        {
            var colliders = Physics.OverlapBox(occupyColliders[i].transform.position, halfExtents[i],
                occupyColliders[i].transform.rotation, mask);
            foreach (Collider coll in colliders)
            {
                var objectThing = coll.gameObject;
                var piece = Utils.FindParentObjectComponent<GamePiece>(objectThing);
                if (!piece) continue;
                bool isPiece = false;
                foreach (PieceNames name in scorePieces)
                {
                    if (piece.pieceType == name) isPiece = true;
                }
                if (!isPiece || piece.state != GamePieceState.World) continue;
                if (pieces.Contains(piece)) continue;
                pieces.Add(piece);
            }
        }

        return pieces;
    }
}
