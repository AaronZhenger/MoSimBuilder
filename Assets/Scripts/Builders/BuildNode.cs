using System;
using System.Collections;
using System.Collections.Generic;
using BuilderLib;
using MyBox;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

[ExecuteAlways]
public class BuildNode: MonoBehaviour
{
    private BoxCollider _intakeCollider;
    public GamePiece currentGamePiece;
    [SerializeField] private bool Preload;

    [ConditionalField(nameof(hasIntake))] [SerializeField]
    private Vector3 intakeSize;
    [ConditionalField(nameof(Preload))]
    [SerializeField] private PieceNames pieceName;
    public NodeState currentState;
    public NodeAction[] Actions;
    private PlayerInput _playerInput;
    private InputActionMap _inputMap;
    private GameObject _robotParent;
    private Vector3 _halfExtents;
    private List<GamePiece> pieces = new List<GamePiece>();
    private static GameObject[] Pieces;
    private bool hasIntake = false;
    
    private Vector3 lastIntakePosition;
    private Quaternion lastIntakeRotation;
    
    private void Start()
    {
        foreach (var child in Utils.GetAllChildren(transform))
        {
            if (child.TryGetComponent(typeof(BoxCollider), out var col))
            {
                _intakeCollider = (BoxCollider)col;
                
                _halfExtents = _intakeCollider.bounds.extents / 2;
            }
        }
        
        _robotParent = Utils.FindParentPlayerInput(gameObject);
        
        _playerInput = _robotParent.GetComponent<PlayerInput>();
        
        _inputMap = _playerInput.actions.FindActionMap("Robot");
        
        _inputMap.Enable();
        
        Pieces ??= Resources.LoadAll<GameObject>("Pieces");
        SpawnPiece(pieceName.ToString(), Pieces);
    }

    private void SpawnPiece(string pieceName, GameObject[] pieces)
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
        
        if (!EditorApplication.isPlaying)
        {
            bool hasIntake = false;
            foreach (var action in Actions)
            {
                if (action.Type == NodeType.Intake)
                {
                    hasIntake = true;
                    break;
                }
            }

            this.hasIntake = hasIntake;

            if (hasIntake)
            {
                if (!_intakeCollider)
                {
                    var intakeParent = Utils.TryGetAddChild("IntakeBox", gameObject);
                    _intakeCollider = Utils.TryGetAddComponent<BoxCollider>(intakeParent);
                    _intakeCollider.size = intakeSize * 0.0254f;
                    _intakeCollider.transform.position = lastIntakePosition;
                    _intakeCollider.transform.rotation = lastIntakeRotation;
                }
                else
                {
                    _intakeCollider.size = intakeSize * 0.0254f;
                    _intakeCollider.isTrigger = true;
                    lastIntakePosition = _intakeCollider.transform.position;
                    lastIntakeRotation = _intakeCollider.transform.rotation;
                }
                
            }
            else
            {
                var box = Utils.FindChild("IntakeBox", gameObject);
                if (box)
                {
                    DestroyImmediate(box);
                }
            }
            
            return;
        };
        var actionFinished = false;
        var actionDone = false;
        for (int i = 0; i < Actions.Length; i++)
        {

            ref NodeAction action = ref Actions[i];
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

            if (buttonPressed || buttonHeld)
            {
                actionDone = true;
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
                                IntakePiece(buttonHeld, action);
                                break;
                            case NodeControlType.Tap:
                                IntakePiece(buttonPressed, action);
                                break;
                            case NodeControlType.AlwaysPerform:
                                IntakePiece(true, action);
                                break;
                        }
                    }
                    break;
                case NodeType.Transfer:
                    //null check
                    if (action.MoveTo && currentGamePiece)
                    {
                        if (!action.MoveTo.currentGamePiece)
                        {
                            var finished = false;
                            switch (action.ControlType)
                            {
                                case NodeControlType.Hold:
                                    finished = TransferPiece(buttonHeld, buttonPressed,  ref action);
                                    break;
                                case NodeControlType.Tap:
                                    StartCoroutine(TransferPieceCo(buttonPressed, action));
                                    break;
                                case NodeControlType.AlwaysPerform:
                                    finished = TransferPiece(true,  false, ref action);
                                    break;
                            }

                            if (finished)
                            {
                                actionFinished = true;
                            }
                        }
                    }
                    break;
                case NodeType.Outake:
                    if (currentGamePiece)
                    {
                        var finished = false;
                        if (!PerformTimerCheck(ref action, buttonPressed)) continue;
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

        if ((currentGamePiece && currentState == NodeState.Stowing) || (!actionDone && currentGamePiece))
        {
            currentState = NodeState.Stowing;
            GamePieceManager.teleportTo(currentGamePiece, transform);
        } else if (actionFinished)
        {
            currentGamePiece = null;
        }
    }

    private IEnumerator TransferPieceCo(bool buttonHeld, NodeAction action)
    {
        bool finished = false;
        while (!finished)
        {
            finished = TransferPiece(buttonHeld, buttonHeld, ref action);
            yield return null;
        }
    }
    
    private bool PerformTimerCheck(ref NodeAction action, bool onPressed = false, bool dontReset = false)
    {
        if (onPressed)
        {
            action.performTimer = 0;
        }
        
        action.performTimer += Time.deltaTime;

        //run timer
        if (action.performTimer > action.DelayTimer || (action.DelayTimer == 0))
        {
            if (dontReset) return true;
            action.performTimer = 0;
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TransferPiece(bool button, bool butonPressed, ref NodeAction action)
    {
        if (!PerformTimerCheck(ref action, butonPressed, true)) return false;
        var succeeded = false;
        if (currentGamePiece.pieceType != action.PieceType) return false;
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

            if (succeeded)
            {
                action.MoveTo.currentState = NodeState.Stowing;
                action.performTimer = 0;
            }
        }
        
        return succeeded;
    }

    private bool IntakePiece(bool button, NodeAction action)
    {
        //intake action
        if (button && !currentGamePiece)
        {
            var pieces = PoolObjects(action);
            currentGamePiece = ClosestPiece(pieces);
            if (!currentGamePiece) return false;
            currentGamePiece.startingDistance = DistanceToPiece(currentGamePiece);
            currentState = NodeState.Intakeing;
        } else if (currentState == NodeState.Intakeing && currentGamePiece)
        {
            currentState = NodeState.Intakeing;
            if (action.Animate)
            {
                if (!currentGamePiece) return false;
                if (GamePieceManager.AnimateTo(currentGamePiece, action, transform))
                {
                    currentState = NodeState.Stowing;
                }
                else
                {
                    if (currentGamePiece.startingDistance < DistanceToPiece(currentGamePiece))
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
        pieces.Clear();
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

    private GamePiece ClosestPiece(List<GamePiece> pieces)
    {
        switch (pieces.Count)
        {
            case 0:
                return null;
            case 1:
                return pieces[0];
        }

        var closest = pieces[0];
        var distance = DistanceToPiece(closest);

        foreach (var piece in pieces)
        {
            if (DistanceToPiece(piece) < distance)
            {
                closest = piece;
            }
        }
        
        return closest;
    }

    private float DistanceToPiece(GamePiece piece)
    {
        var pose = transform.InverseTransformPoint(piece.transform.position);
        return pose.magnitude;
    }
}

[Serializable]
public struct NodeAction
{
    public string Name;
    [Header("Node Behaviour on Action")]
    public NodeType Type;
    //interface stuff
    [ConditionalField(true, nameof(IsNotOuttake))]
    public bool Animate;
    [ConditionalField(true, nameof(SpeedVisible))]
    public float Speed;
    [ConditionalField(true, nameof(SpeedVisible))]
    public float AngularSpeed;
    [ConditionalField(true, nameof(IsTransfer))]
    public BuildNode MoveTo;
    [ConditionalField(true, nameof(IsOuttake))]
    public Direction Direction;
    [ConditionalField(true, nameof(IsOuttake))]
    public Vector3 Spin;
    [ConditionalField(true, nameof(IsNotIntake))]
    public float DelayTimer;
    [Header("General Settings")]
    public PieceNames PieceType;
    public NodeControlType ControlType;
    [HideInInspector]
    public float performTimer;
    public ControllerInputs ControllerButton;
    public KeyboardInputs KeyboardButton;
    
    //Conditional values
    private bool IsTransfer() => Type is NodeType.Transfer;
    private bool IsOuttake() => Type is NodeType.Outake;
    private bool IsNotOuttake() => Type is not NodeType.Outake;
    private bool IsNotIntake() => Type is not NodeType.Intake;
    private bool SpeedVisible() => (IsNotOuttake() && Animate) || IsOuttake();
}


