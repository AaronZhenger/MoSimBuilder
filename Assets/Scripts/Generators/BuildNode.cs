using System;
using System.Collections;
using System.Collections.Generic;
using BuilderLib;
using MyBox;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

public class BuildNode: MonoBehaviour
{
    [SerializeField] private Collider _intakeCollider;
    public GamePiece currentGamePiece;
    [SerializeField] private bool Preload;
    [ConditionalField(nameof(Preload))]
    [SerializeField] private PieceNames pieceName;
    public NodeState currentState;
    public NodeAction[] Actions;
    private PlayerInput _playerInput;
    private InputActionMap _inputMap;
    private GameObject _robotParent;
    private Vector3 _halfExtents;
    
    private void Start()
    {
        foreach (var child in Utils.GetAllChildren(transform))
        {
            if (child.TryGetComponent(typeof(BoxCollider), out var col))
            {
                _intakeCollider = (Collider)col;
                
                _halfExtents = _intakeCollider.bounds.extents / 2;
            }
        }
        
        _robotParent = Utils.FindParentPlayerInput(gameObject);
        
        _playerInput = _robotParent.GetComponent<PlayerInput>();
        
        _inputMap = _playerInput.actions.FindActionMap("Robot");
        
        _inputMap.Enable();
        
        var Pieces = Resources.LoadAll<GameObject>("Pieces");
        switch (pieceName)
        {
            case PieceNames.Coral:
                spawnPiece("Coral", Pieces);
                break;
            case PieceNames.Algae:
                spawnPiece("Algae", Pieces);
                break;
        }
    }

    private void spawnPiece(string pieceName, GameObject[] pieces)
    {
        if (!Preload) return;
        foreach (var piece in pieces)
        {
            if (piece.name != pieceName) continue;
            currentGamePiece = Instantiate(piece, transform.position, transform.rotation, transform).GetComponent<GamePiece>();
            currentState = NodeState.Stowing;
            return;
        }
    }

    void Update()
    {
        var actionPerformed = false;
        var actionFinished = false;
        for (int i = 0; i < Actions.Length; i++)
        {

            var action = Actions[i];
            var controllerAction = _inputMap.FindAction(action.ControllerButton.ToString());
            var keyboardAction = _inputMap.FindAction(action.KeyboardButton.ToString());
            var buttonPressed = false;
            if (controllerAction.triggered)
            {
                if (controllerAction.activeControl?.device is Gamepad)
                {
                    buttonPressed = true;
                }
            }

            if (keyboardAction.triggered)
            {
                if (keyboardAction.activeControl?.device is Keyboard)
                {
                    buttonPressed = true;
                }
            }

            var controllerHeld = controllerAction.IsPressed() &&
                                 (controllerAction.activeControl?.device is Gamepad);
            var keyboardHeld = keyboardAction.IsPressed() &&
                               (keyboardAction.activeControl?.device is Keyboard);
            var buttonHeld = controllerHeld || keyboardHeld;

            if (buttonHeld || buttonPressed)
            {
                actionPerformed = true;
            }

            switch (action.Type)
            {
                case NodeType.Intake:
                    //intake null check
                    if (_intakeCollider)
                    {
                        //action type
                        switch (action.ControlType)
                        {
                            case NodeControlType.Hold:
                                actionPerformed = intakePiece(buttonHeld, action);
                                break;
                            case NodeControlType.Tap:
                                actionPerformed = intakePiece(buttonPressed, action);
                                break;
                            case NodeControlType.AlwaysPerform:
                                actionPerformed = intakePiece(true, action);
                                break;
                        }
                    }
                    break;
                case NodeType.Transfer:
                    //null check
                    if (action.MoveTo && currentGamePiece)
                    {
                        var finished = false;
                        switch (action.ControlType)
                        {
                            case NodeControlType.Hold:
                                 finished = transferPiece(buttonHeld, action);
                                break;
                            case NodeControlType.Tap:
                                finished = transferPiece(buttonPressed, action);
                                break;
                            case NodeControlType.AlwaysPerform:
                                finished = transferPiece(true, action);
                                actionPerformed = true;
                                break;
                        }
                        if (finished)
                        {
                            actionFinished = true;
                        }
                    }
                    break;
                case NodeType.Outake:
                    if (currentGamePiece)
                    {
                        var finished = false;
                        switch (action.ControlType)
                        {
                            case NodeControlType.Hold:
                                if (buttonHeld)
                                {
                                    currentState = NodeState.Outaking;
                                    finished = GamePieceManager.ReleaseToWorld(currentGamePiece, action);
                                    StartCoroutine(GamePieceManager.enableColliders(currentGamePiece));
                                }
                                break;
                            case NodeControlType.Tap:
                                if (buttonPressed)
                                {
                                    currentState = NodeState.Outaking;
                                    finished = GamePieceManager.ReleaseToWorld(currentGamePiece, action);
                                    StartCoroutine(GamePieceManager.enableColliders(currentGamePiece));
                                }
                                break;
                            case NodeControlType.AlwaysPerform:
                                currentState = NodeState.Outaking;
                                finished = GamePieceManager.ReleaseToWorld(currentGamePiece, action);
                                StartCoroutine(GamePieceManager.enableColliders(currentGamePiece));
                                break;
                                
                        }

                        if (finished)
                        {
                            actionFinished = true;
                        }
                    }
                    break;
            }
        }

        if (!actionPerformed && currentGamePiece && currentState != NodeState.Intakeing)
        {
            currentState = NodeState.Stowing;
            GamePieceManager.teleportTo(currentGamePiece, transform);
        } else if (actionFinished)
        {
            currentGamePiece = null;
        }
    }

    private bool transferPiece(bool button, NodeAction action)
    {
        var succeeded = false;
        if (button && currentGamePiece)
        {
            if (action.Animate)
            {
                currentState = NodeState.Transfering;
                succeeded = GamePieceManager.AnimateTo(currentGamePiece, action);
            }
            else
            {
                currentState = NodeState.Transfering;
                succeeded = GamePieceManager.teleportTo(currentGamePiece, action);
            }
        }
        
        return succeeded;
    }

    private bool intakePiece(bool button, NodeAction action)
    {
        //intake action
        if (button && !currentGamePiece)
        {
            var pieces = PoolObjects(action);
            currentGamePiece = closestPiece(pieces);
            if (!currentGamePiece) return false;
            currentGamePiece.startingDistance = distanceToPiece(currentGamePiece);
            currentState = NodeState.Intakeing;
        } else if (currentState == NodeState.Intakeing && currentGamePiece)
        {
            currentState = NodeState.Intakeing;
            if (action.Animate)
            {
                if (GamePieceManager.AnimateTo(currentGamePiece, action))
                {
                    currentState = NodeState.Stowing;
                }
                else
                {
                    if (currentGamePiece.startingDistance < distanceToPiece(currentGamePiece))
                    {
                        currentState = NodeState.Stowing;
                        currentGamePiece.colliderParent.SetActive(true);
                        currentGamePiece.state = GamePieceState.World;
                        currentGamePiece.transform.parent = currentGamePiece.originalParent;
                        currentGamePiece = null;
                    }
                }
            }
            else
            {
                if (GamePieceManager.teleportTo(currentGamePiece, transform))
                {
                    currentState = NodeState.Stowing;
                };
            }
        }
        else
        {
            return false;
        } 
        return true;
    }

    private List<GamePiece> PoolObjects(NodeAction action)
    {
        List<GamePiece> pieces = new List<GamePiece>();
        var mask = LayerMask.GetMask("Piece");
        var colliders = Physics.OverlapBox(_intakeCollider.transform.position, _halfExtents,
            _intakeCollider.transform.rotation, mask);
        foreach (Collider coll in colliders)
        {
            var objectThing = coll.gameObject;
            var piece = Utils.FindParentObjectComponent<GamePiece>(objectThing);
            if (!piece) continue;
            if (piece.pieceType != action.PieceType || piece.state != GamePieceState.World) continue;
            pieces.Add(piece);
        }
        
        return pieces;
    }

    private GamePiece closestPiece(List<GamePiece> pieces)
    {
        switch (pieces.Count)
        {
            case 0:
                return null;
            case 1:
                return pieces[0];
        }

        var closest = pieces[0];
        var distance = distanceToPiece(closest);

        foreach (var piece in pieces)
        {
            if (distanceToPiece(piece) < distance)
            {
                closest = piece;
            }
        }
        
        return closest;
    }

    private float distanceToPiece(GamePiece piece)
    {
        var pose = transform.InverseTransformPoint(piece.transform.position);
        return pose.magnitude;
    }
}

[Serializable]
public struct NodeAction
{
    public string name;
    public NodeType Type;
    public bool Animate;
    public float Speed;
    public PieceNames PieceType;
    public float AngularSpeed;
    public BuildNode MoveTo;
    public Direction Direction;
    public Vector3 Spin;
    public NodeControlType ControlType;
    public ControllerInputs ControllerButton;
    public KeyboardInputs KeyboardButton;
}


