using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace BuilderLib
{
    public static class GamePieceManager
    {
        public static void disableColliders(GamePiece piece)
        {
            if (piece.colliderParent.activeSelf)
            {
                piece.colliderParent.SetActive(false);
            }
        }

        public static IEnumerator enableColliders(GamePiece piece)
        {
            if (piece.colliderParent.activeSelf)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        
            piece.colliderParent.SetActive(true);
        }
        public static bool AnimateTo(GamePiece piece, NodeAction action)
        {
            var speed = action.Speed * 0.0254f;

            var transform = piece.rb.transform;
            var target = action.MoveTo.transform;
            if (piece.state != GamePieceState.Moving)
            {
                piece.state = GamePieceState.Moving;
                piece.startPosition = transform.localPosition;
                disableColliders(piece);
            }

            var distance = transform.parent.InverseTransformPoint(target.position) - piece.startPosition;
            var parentPosition = transform.parent.position;
            var step = distance.normalized * ((speed) * Time.deltaTime);
            var finalPosition = piece.startPosition + step;

            piece.startPosition = finalPosition;
            transform.position = parentPosition + transform.parent.TransformDirection(finalPosition);
            piece.rb.position = parentPosition + transform.parent.TransformDirection(finalPosition);
            piece.rb.velocity = Vector3.zero;

            var distanceMagnitude = distance.magnitude;


            // Calculate target rotation based on movement direction
            Quaternion targetRotation = target.rotation;
            Quaternion shortestTargetRotation;
            if (piece.pieceType == PieceNames.Coral)
            {
                shortestTargetRotation = FindShortestSymmetricRotation(
                    transform.rotation,
                    targetRotation
                );
            }
            else
            {
                shortestTargetRotation = targetRotation;
            }

            // Smoothly rotate towards target rotation
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                shortestTargetRotation,
                action.AngularSpeed * Time.fixedDeltaTime
            );

            if (action.AngularSpeed == 0)
            {
                transform.localRotation = Quaternion.identity;
            }

            if (distanceMagnitude <= 0.75f * 0.0254f)
            {
                changeParent(piece, action);
                return true; // Reached target
            }
            else
            {
                return false; // Moving towards target
            }
        }

        public static bool teleportTo(GamePiece piece, NodeAction action)
        {
            disableColliders(piece);
            var transform = piece.rb.transform;
            var target = action.MoveTo.transform;
            transform.position = target.position;
            transform.rotation = target.rotation;
            changeParent(piece, action);
            return true;
        }
    
        public static bool teleportTo(GamePiece piece, Transform target)
        {
            disableColliders(piece);
            var transform = piece.rb.transform;
            transform.position = target.position;
            transform.rotation = target.rotation;
            changeParent(piece, target);
            return true;
        }
    
        public static bool ReleaseToWorld(GamePiece piece, NodeAction action)
        {
            if (piece.pieceType != action.PieceType) return false;
            var speed = action.Speed * 0.0254f;
            var rb = piece.rb;
            var transform = rb.transform;

            rb.velocity = Vector3.zero;
            piece.transform.localPosition = Vector3.zero;
            piece.transform.localEulerAngles = Vector3.zero;
            Vector3 velocity;
            switch (action.Direction)
            {
                case Direction.forward:
                    velocity = piece.owner.transform.forward.normalized * speed;
                    break;
                case Direction.up:
                    velocity = piece.owner.transform.up.normalized * speed;
                    break;
                case Direction.sideways:
                    velocity = piece.owner.transform.right * speed;
                    break;
                default:
                    velocity = piece.owner.transform.forward.normalized * speed;
                    break;
            }
            rb.velocity = velocity;
            rb.angularVelocity = transform.TransformDirection(action.Spin);
        
            piece.state = GamePieceState.World;

            piece.transform.parent = piece.originalParent;
        
            return true;
        }


        public static bool changeParent(GamePiece piece, NodeAction action)
        {
            piece.owner = action.MoveTo.transform;
            piece.transform.parent = action.MoveTo.transform;
            action.MoveTo.currentGamePiece = piece;
            piece.state = GamePieceState.Stationary;
            return true;
        }
    
        public static bool changeParent(GamePiece piece, Transform target)
        {
            piece.owner = target.transform;
            piece.transform.parent = target.transform;
            piece.state = GamePieceState.Stationary;
            return true;
        }
    
        private static Quaternion FindShortestSymmetricRotation(Quaternion current, Quaternion target)
        {
            // For objects with symmetry on X and Y axes, we need to check multiple equivalent rotations
            List<Quaternion> symmetricRotations = new List<Quaternion>();
    
            // Original target
            symmetricRotations.Add(target);
    
            // X-axis symmetry (180° rotation around X)
            symmetricRotations.Add(target * Quaternion.Euler(180f, 0f, 0f));
    
            // Y-axis symmetry (180° rotation around Y)
            symmetricRotations.Add(target * Quaternion.Euler(0f, 180f, 0f));
    
            // Both X and Y symmetry
            symmetricRotations.Add(target * Quaternion.Euler(180f, 180f, 0f));
    
            // Find the rotation with the smallest angular distance
            Quaternion bestRotation = target;
            float smallestAngle = float.MaxValue;
    
            foreach (Quaternion symRotation in symmetricRotations)
            {
                float angle = Quaternion.Angle(current, symRotation);
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    bestRotation = symRotation;
                }
            }
    
            return bestRotation;
        }
    }
}



