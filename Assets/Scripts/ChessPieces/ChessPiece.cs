using System.Collections.Generic;
using System.Linq;
using ChessLogic;
using Managers;
using UnityEngine;

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
        public Moves Moves { get; set; }

        public bool IsMoved { get => isMoved; set => isMoved = value; }
        public Tile HoveringTile { get; set; }
        public MovementManager MovementManager { get; set; }

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

            if (!MovementManager.GameManager.IsPlayerTurn && MovementManager.GameManager.GameStatus == Shared.GameStatus.Continue)
                MovementManager.GameManager.MakeBotTurn();
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

        public List<Vector2Int> GetAllPossibleMoves()
        {
            var allMoves = Moves.AvailableMoves.Select(move => new Vector2Int(move.x, move.y)).ToList();
            allMoves.AddRange(Moves.AttackMoves.Select(move => new Vector2Int(move.x, move.y)).ToList());

            return allMoves;
        }
        
        public abstract void CalculateAvailablePositions();

        public Moves CalculateAvailablePositionsWithoutUpdating()
        {
            if (currentX == -1 && currentY == -1)
            {
                return new Moves();
            } 
            
            // Save Old Moves
            var oldMoves = new Moves
            {
                AvailableMoves = new List<Vector2Int>(Moves.AvailableMoves),
                AttackMoves = new List<Vector2Int>(Moves.AttackMoves),
                SpecialMoves = new List<SpecialMove>(Moves.SpecialMoves)
            };

            //Calculate new moves
            CalculateAvailablePositions();

            // Save New Moves in a separate field 
            var newMoves = new Moves
            {
                AvailableMoves = new List<Vector2Int>(Moves.AvailableMoves),
                AttackMoves = new List<Vector2Int>(Moves.AttackMoves),
                SpecialMoves = new List<SpecialMove>(Moves.SpecialMoves)
            };

            //Revert piece moves back to the old set
            Moves = oldMoves;
            
            return newMoves;
        }

        public void AddToTileAttackingPieces(Vector2Int coords)
        {
            var attackTile = MovementManager.TileManager.GetTile(coords);
            var attackingPiecesList = team == Shared.TeamType.White
                ? attackTile.WhiteAttackingPieces
                : attackTile.BlackAttackingPieces;
            attackingPiecesList.Add(this);
        }
    }
}