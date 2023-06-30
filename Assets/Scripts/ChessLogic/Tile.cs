using System.Collections.Generic;
using ChessPieces;
using Managers;
using UnityEngine;

namespace ChessLogic
{
    public class Tile : MonoBehaviour
    {
    
        public Vector2Int Position { get; set; }
        public TileManager TileManager { get; set; }

        // Tile Attack Status

        public Shared.AttackedBy AttackedBy { get; set; }
        public List<ChessPiece> WhiteAttackingPieces { get; set; }
        public List<ChessPiece> BlackAttackingPieces { get; set; }

        // Tile Type Properties
        public bool IsWhiteTile { get; set; }
        public bool IsAvailableTile { get; set; }
        public bool IsAttackTile { get; set; }
        public bool IsSpecialTile { get; set; }
    
        public Tile(int x, int y)
        {
            Position = new Vector2Int(x, y);
            WhiteAttackingPieces = new List<ChessPiece>();
            BlackAttackingPieces = new List<ChessPiece>();
            AttackedBy = Shared.AttackedBy.None;
        }

        public void DetermineTileTypeFromMove(Move move)
        {
            IsAvailableTile = move.Type is Shared.MoveType.Normal or Shared.MoveType.Promotion or Shared.MoveType.LongCastle
                or Shared.MoveType.ShortCastle;
            IsAttackTile = move.Type is Shared.MoveType.Attack or Shared.MoveType.AttackPromotion
                or Shared.MoveType.EnPassant;
            IsSpecialTile = move.Type is Shared.MoveType.Promotion or Shared.MoveType.AttackPromotion
                or Shared.MoveType.ShortCastle or Shared.MoveType.LongCastle or Shared.MoveType.EnPassant;
        }

        public void ResetTileType()
        {
            IsAttackTile = false;
            IsSpecialTile = false;
            IsAvailableTile = false;
        }
        
        public void TriggeredAvailableMove(Shared.MovementType movementType)
        {
            TileManager.HandleTileTrigger(Position, true, movementType);
        }
    
        public void ExitedTriggeredAvailableMove(Shared.MovementType movementType)
        {
            TileManager.HandleTileTrigger(Position, false, movementType);
        }

        public void ResetAttackStatus()
        {
            AttackedBy = Shared.AttackedBy.None;
            WhiteAttackingPieces = new List<ChessPiece>();
            BlackAttackingPieces = new List<ChessPiece>();

            IsSpecialTile = false;
            IsAttackTile = false;
            IsSpecialTile = false;
        }

        public void DetermineAttackStatus()
        {
            var noWhitePieces = WhiteAttackingPieces.Count;
            var noBlackPieces = BlackAttackingPieces.Count;

            if (noWhitePieces == 0 && noBlackPieces == 0) AttackedBy = Shared.AttackedBy.None;
            else if (noWhitePieces != 0 && noBlackPieces == 0) AttackedBy = Shared.AttackedBy.White;
            else if (noWhitePieces == 0 && noBlackPieces != 0) AttackedBy = Shared.AttackedBy.Black;
            else AttackedBy = Shared.AttackedBy.Both;
        }
    }
}
