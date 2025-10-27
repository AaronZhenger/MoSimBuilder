using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using Util;

public class SpawnGamePiece : MonoBehaviour
{
    [Header("Piece Settings")]
    [SerializeField] private PieceNames peiceType;
    [SerializeField] private Direction velocityDirection;
    [SerializeField] private float velocity;
    
    [Header("Sliding Settings")]
    [SerializeField] private bool axisSlides;
    [ConditionalField(nameof(axisSlides))]
    [SerializeField] private Direction direction;

    [Header("Spawn Control")]
    [SerializeField] private float delayTimer;
    
    [Header("Threshold Spawn Settings")]
    [SerializeField] private BoxCollider detectionVolume; 
    [SerializeField] private int thresholdCount = 1;

    private static Dictionary<PieceNames, GameObject> _piecesMap;
    public static List<SpawnPieceTarget> Targets = new List<SpawnPieceTarget>();
    
    private bool _pieceSpawned;
    private float _timer;
    private int _pieceMask;
    
    private Collider[] _overlapResults;
    private readonly List<GamePiece> _currentGamePieces = new List<GamePiece>();
    
    private Vector3 _cachedForwardVelocity;
    private Vector3 _cachedSidewaysVelocity;
    private Vector3 _cachedUpVelocity;
    
    void Awake()
    {
        if (_piecesMap == null)
        {
            _piecesMap = new Dictionary<PieceNames, GameObject>();
            var pieces = Resources.LoadAll<GameObject>("Pieces");
            
            foreach (var piece in pieces)
            {
                var gamePieceComponent = piece.GetComponent<GamePiece>();
                if (gamePieceComponent)
                {
                    _piecesMap[gamePieceComponent.pieceType] = piece;
                }
            }
        }
        
        _overlapResults = new Collider[20 * thresholdCount];
        _pieceMask = LayerMask.GetMask("Piece");
        
        _cachedForwardVelocity = Vector3.forward * velocity;
        _cachedSidewaysVelocity = Vector3.right * velocity;
        _cachedUpVelocity = Vector3.up * velocity;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= delayTimer)
        {
            _timer = 0f;
            _pieceSpawned = false;
        }
    }

    private void SpawnPiece(PieceNames pieceTypeEnum, float velocityValue)
    {
        if (_pieceSpawned || !_piecesMap.TryGetValue(pieceTypeEnum, out GameObject piecePrefab)) 
            return;

        var item = Instantiate(piecePrefab, transform.position, transform.rotation, transform)
            .GetComponent<GamePiece>();
        
        if (Mathf.Approximately(velocityValue, velocity))
        {
            item.rb.velocity = velocityDirection switch
            {
                Direction.forward => _cachedForwardVelocity,
                Direction.sideways => _cachedSidewaysVelocity,
                Direction.up => _cachedUpVelocity,
                _ => _cachedForwardVelocity
            };
        }
        else
        {
            item.rb.velocity = velocityDirection switch
            {
                Direction.forward => Vector3.forward * velocityValue,
                Direction.sideways => Vector3.right * velocityValue,
                Direction.up => Vector3.up * velocityValue,
                _ => Vector3.forward * velocityValue
            };
        }
        
        _pieceSpawned = true;
    }

    private Vector3 GetClosestPointOnAxis(Vector3 targetPosition, Direction slideDirection)
    {
        Vector3 spawnerPos = transform.position;
        
        return slideDirection switch
        {
            Direction.sideways => new Vector3(targetPosition.x, spawnerPos.y, spawnerPos.z),
            Direction.up => new Vector3(spawnerPos.x, targetPosition.y, spawnerPos.z),
            Direction.forward => new Vector3(spawnerPos.x, spawnerPos.y, targetPosition.z),
            _ => spawnerPos
        };
    }

    private bool CheckInternalThreshold()
    {
        if (!detectionVolume) return false;

        _currentGamePieces.Clear();
        
        int numColliders = Physics.OverlapBoxNonAlloc(
            detectionVolume.transform.position, 
            detectionVolume.size * 0.5f,
            _overlapResults,
            detectionVolume.transform.rotation, 
            _pieceMask
        );

        for (int i = 0; i < numColliders; i++)
        {
            var piece = Utils.FindParentObjectComponent<GamePiece>(_overlapResults[i].gameObject);
            
            if (!piece || piece.pieceType != peiceType || piece.state != GamePieceState.World) 
                continue;
            
            if (!_currentGamePieces.Contains(piece))
            {
                _currentGamePieces.Add(piece);
            }
        }

        return _currentGamePieces.Count >= thresholdCount;
    }

    void FixedUpdate()
    {
        if (_pieceSpawned) return;

        bool shouldSpawn = false;
        float targetVelocity = velocity;

        for (int i = 0; i < Targets.Count; i++)
        {
            var target = Targets[i];
            if (!target) continue;

            if (target.spawnType == SpawnType.Distance)
            {
                Vector3 effectiveSpawnPosition = axisSlides 
                    ? GetClosestPointOnAxis(target.transform.position, direction)
                    : transform.position;
                
                float distanceSq = (effectiveSpawnPosition - target.transform.position).sqrMagnitude;
                
                if (distanceSq <= target.SpawnDistance * target.SpawnDistance)
                {
                    shouldSpawn = true;
                    targetVelocity = target.Velocity;
                    break;
                }
            }
            else
            {
                if (!CheckInternalThreshold())
                {
                    shouldSpawn = true;
                    targetVelocity = target.Velocity;
                    break;
                }
            }
        }
        
        if (shouldSpawn)
        {
            SpawnPiece(peiceType, targetVelocity);
            return;
        }

        if (!CheckInternalThreshold())
        {
            SpawnPiece(peiceType, velocity);
        }
    }
}