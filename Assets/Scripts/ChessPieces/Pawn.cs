using System.Collections.Generic;
using ChessLogic;
using UnityEngine;

namespace ChessPieces
{
    public class Pawn : ChessPiece
    {
        public bool IsEnPassantTarget { get; set; }
        
        public override Moves CalculateAvailablePositions()
        {
            Moves moves = new();

            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

            Vector2Int possibleMoveOneTileAhead = new(currentX + (direction * 1), currentY);
            if (Shared.TileOccupiedBy.None == MovementManager.CalculateSpaceOccupation(possibleMoveOneTileAhead, team))
            {
                moves.AvailableMoves.Add(possibleMoveOneTileAhead);
                if (!isMoved)
                {
                    Vector2Int possibleMoveTwoTilesAhead = new(currentX + (direction * 2), currentY);
                    if (Shared.TileOccupiedBy.None == MovementManager.CalculateSpaceOccupation(possibleMoveTwoTilesAhead, team)) moves.AvailableMoves.Add(possibleMoveTwoTilesAhead);
                }
            }

            Vector2Int attackMoveLeft = new(currentX + (direction * 1), currentY + 1);
            if (Shared.TileOccupiedBy.EnemyPiece == MovementManager.CalculateSpaceOccupation(attackMoveLeft, team)) moves.AttackMoves.Add(attackMoveLeft);

            Vector2Int attackMoveRight = new(currentX + (direction * 1), currentY - 1);
            if (Shared.TileOccupiedBy.EnemyPiece == MovementManager.CalculateSpaceOccupation(attackMoveRight, team)) moves.AttackMoves.Add(attackMoveRight);
            
            moves.SpecialMoves = CalculateSpecialMoves();

            return moves;
        }

        private List<SpecialMove> CalculateSpecialMoves()
        {
            List<SpecialMove> moves = new();
            
            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;
            List<Vector2Int> possibleEnPassantTarget = new()
            {
                new Vector2Int(currentX, currentY + 1),
                new Vector2Int(currentX, currentY - 1)
            };
            
            List<Vector2Int> possibleEnPassantAttack = new()
            {
                new Vector2Int(currentX + (direction * 1), currentY + 1),
                new Vector2Int(currentX + (direction * 1), currentY - 1)
            };

            for (var i = 0; i < possibleEnPassantTarget.Count; i++)
            {
                var enPassantOccupation = MovementManager.CalculateSpaceOccupation(possibleEnPassantTarget[i], team);
                if (enPassantOccupation != Shared.TileOccupiedBy.EnemyPiece) continue;
                
                var enemyPiece = MovementManager.GetChessPiece(possibleEnPassantTarget[i]);
                if (enemyPiece is not Pawn pawn || !pawn.IsEnPassantTarget) continue;
                
                var specialMove = new SpecialMove(possibleEnPassantAttack[i], Shared.MoveType.EnPassant);
                moves.Add(specialMove);
            }
            
            return moves;
        }
    }
}
