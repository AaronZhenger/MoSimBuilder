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
    private readonly HashSet<PieceNames> scorePiecesSet = new HashSet<PieceNames>();
    [SerializeField] protected Collider[] occupyColliders;
    private readonly HashSet<GamePiece> uniquePieces = new HashSet<GamePiece>();
    private Vector3[] halfExtents;
    protected List<GamePiece> occupyObjects = new List<GamePiece>();
    private List<GamePiece> pieces = new List<GamePiece>();
    private LayerMask peiceMask;

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
        scorePiecesSet.Clear();
        foreach (var name in scorePieces)
        {
            scorePiecesSet.Add(name);
        }
        peiceMask = LayerMask.GetMask("Piece");
    }

    protected void ScorePoints(int multiplyer = 1)
    {
        bool auto = FMS.MatchState == MatchState.auto;
        bool matchOver = FMS.MatchState == MatchState.finished;

        if (matchOver) return;
        
        if (isBlue)
        {
            ScoreHolder.BlueScore -= lastAddedPoints;
            ScoreHolder.BlueScore += (auto? autoScoreToAdd : scoreToAdd * multiplyer);
        }
        else
        {
            ScoreHolder.RedScore -= lastAddedPoints;
            ScoreHolder.RedScore += (auto? autoScoreToAdd : scoreToAdd) * multiplyer;
        }

        lastAddedPoints = scoreToAdd * multiplyer;
    }
    
    public List<GamePiece> getOccupyPieces()
    {
        return occupyObjects;
    }
    
    protected List<GamePiece> occupyPieces()
    {
        uniquePieces.Clear();
        pieces.Clear();
        
        for (int i = 0; i < occupyColliders.Length; i++)
        {
            var size = Physics.OverlapBox(occupyColliders[i].transform.position, halfExtents[i], occupyColliders[i].transform.rotation, peiceMask);
            foreach (var collider in size)
            {
                var objectThing = collider.gameObject;
                var piece = Utils.FindParentObjectComponent<GamePiece>(objectThing);
                if (!piece) continue;
                if (uniquePieces.Contains(piece)) continue;
                if (piece.state != GamePieceState.World) continue;
                if (pieces.Contains(piece)) continue;
                uniquePieces.Add(piece);
            }
        }

        pieces.AddRange(uniquePieces);
        return pieces;
    }
}
