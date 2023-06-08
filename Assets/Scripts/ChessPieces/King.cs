using System.Collections.Generic;
using ChessLogic;
using UnityEngine;

namespace ChessPieces
{
    public class King : ChessPiece
    {
        public bool isChecked = false;
        public override void CalculateAvailablePositions()
        {
            Moves = new Moves();
            
            Vector2Int kingForwardMove = new(currentX, currentY + 1);
            Vector2Int kingBackwardMove = new(currentX, currentY - 1);
            Vector2Int kingLeftMove = new(currentX - 1, currentY);
            Vector2Int kingRightMove = new(currentX + 1, currentY);

            Vector2Int kingLeftForwardMove = new(currentX - 1, currentY + 1);
            Vector2Int kingLeftBackwardMove = new(currentX - 1, currentY - 1);
            Vector2Int kingRightBackwardMove = new(currentX + 1, currentY - 1);
            Vector2Int kingRightForwardMove = new(currentX + 1, currentY + 1);

            List<Vector2Int> possibleMoves = new()
            {
                kingForwardMove,
                kingBackwardMove,
                kingLeftMove,
                kingRightMove,
                kingLeftForwardMove,
                kingLeftBackwardMove,
                kingRightBackwardMove,
                kingRightForwardMove
            };

            possibleMoves.ForEach(move =>
            {
                var occupationType = MovementManager.CalculateSpaceOccupation(move, team);
                if(occupationType != Shared.TileOccupiedBy.EndOfTable) AddToTileAttackingPieces(move);
                
                switch (occupationType)
                {
                    case Shared.TileOccupiedBy.None:
                        Moves.AvailableMoves.Add(move);
                        break;
                    case Shared.TileOccupiedBy.EnemyPiece:
                        Moves.AttackMoves.Add(move);
                        break;
                }
            });

            if (isMoved) return;
            
            Vector2Int castleShort = new(currentX, currentY - 2);
            var castleShortRook = MovementManager.ChessPieces[castleShort.x, castleShort.y - 1];
            if (castleShortRook != null && castleShortRook is Rook shortRook && !shortRook.IsMoved)
                Moves.SpecialMoves.Add(new SpecialMove(castleShort, Shared.MoveType.ShortCastle));

            Vector2Int castleLong = new(currentX, currentY + 2);
            var castleLongRook = MovementManager.ChessPieces[castleLong.x, castleLong.y + 2];
            if (castleLongRook != null && castleLongRook is Rook longRook && !longRook.IsMoved)
                Moves.SpecialMoves.Add(new SpecialMove(castleLong, Shared.MoveType.LongCastle));
        }
    }
}
