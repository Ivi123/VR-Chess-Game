using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using Managers;
using Players;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ChessPieces
{
    public enum ChessPieceType
    {
        None = 0,
        Pawn = 1,
        Rook = 2,
        Knight = 3,
        Bishop = 4,
        Queen = 5,
        King = 6
    }

    public struct Move
    {
        public Vector2Int Coords { get; set; }
        public Shared.MoveType Type { get; set; }
        
        public Move(Vector2Int moveCoords, Shared.MoveType moveType)
        {
            Coords = moveCoords;
            Type = moveType;
        }

        private Move(Move move)
        {
            Coords = move.Coords;
            Type = move.Type;
        }

        public static List<Move> DeepCopy(List<Move> moves)
        {
            return moves.Select(move => new Move(move)).ToList();
        }
        
    }
    
    public abstract class ChessPiece : MonoBehaviour
    {
        public Shared.TeamType team;
        public Vector2Int startingPosition;
        public int currentX;
        public int currentY;
        public ChessPieceType type;

        // Art
        public Material Material { get; set; }

        // Logic
        public Quaternion DesiredRotation { get; set; }
        private Vector3 position;
        protected bool isMoved = false;
        public int pieceScore;
        public List<Move> Moves { get; set; }

        public bool IsMoved { get => isMoved; set => isMoved = value; }
        public Tile HoveringTile { get; set; }
        public MovementManager MovementManager { get; set; }

        public Player MyPlayer { get; set; }
        
        //---------------------------------------------------- Methods ------------------------------------------------------

        public void Start()
        {
            transform.rotation = Quaternion.Euler(team == Shared.TeamType.White ? new Vector3(0, -90, 0) : new Vector3(0, 90, 0));
            SaveOrientation();
        }

        public void PickPiece()
        {
            transform.Find(Shared.TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = true;
            MovementManager.PieceWasPickedUp(currentX, currentY);
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }

        public void PlacePiece()
        {
            transform.Find(Shared.TileDetectorName).gameObject.GetComponent<BoxCollider>().enabled = false;
            MovementManager.PieceWasDropped(currentX, currentY, HoveringTile);

            transform.SetPositionAndRotation(position, DesiredRotation);
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            HoveringTile = null;
        }

        public void EnablePickUpOnPiece()
        {
            gameObject.GetComponent<XRGrabInteractable>().enabled = true;
        }

        public void DisablePickUpOnPiece()
        {
            gameObject.GetComponent<XRGrabInteractable>().enabled = false;
        }
        
        public void SavePosition()
        {
            var transformPosition = transform.position;
            position = new Vector3(transformPosition.x, transformPosition.y, transformPosition.z);
        }

        public void SaveOrientation()
        {
            var rotation = GetComponent<Transform>().transform.rotation;
            DesiredRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        public void RevertToOriginalMaterial()
        {
            GetComponent<MeshRenderer>().material = Material;
        }
        
        public abstract void CalculateAvailablePositions(ChessPiece[,] board, Tile[,] tiles);

        public List<Move> CalculateAvailablePositionsWithoutUpdating(ChessPiece[,] board, Tile[,] tiles)
        {
            if (currentX == -1 && currentY == -1)
            {
                return new List<Move>();
            } 
            
            // Save Old Moves
            var oldMoves = new List<Move>(Moves);

            //Calculate new moves
            CalculateAvailablePositions(board, tiles);

            // Save New Moves in a separate field 
            var newMoves = new List<Move>(Moves);

            //Revert piece moves back to the old set
            Moves = oldMoves;
            
            return newMoves;
        }

        public void AddToTileAttackingPieces(Tile[,] tiles, Vector2Int coords)
        {
            var attackTile = tiles[coords.x, coords.y];
            var attackingPiecesList = team == Shared.TeamType.White
                ? attackTile.WhiteAttackingPieces
                : attackTile.BlackAttackingPieces;
            attackingPiecesList.Add(this);
        }
    }
}