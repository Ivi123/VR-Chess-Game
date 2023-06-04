using System.Collections.Generic;
using ChessPieces;
using UnityEngine;

namespace ChessLogic
{
    public static class Shared
    {
        public const string TileDetectorName = "TileDetector";

        public const string EliminationTileParentName = "EliminationTiles - DestroyOnLoad";
        public const string WhiteEliminationTilesName = "White EliminationTiles";
        public const string BlackEliminationTilesName = "Black EliminationTiles";

        public enum TileType
        {
            Default,
            Selected,
            AvailableBlack,
            AvailableWhite,
            MoveTo,
            AttackTileBlack,
            AttackTileWhite,
            HighlightAttack
        }

        public enum TileOccupiedBy
        {
            EndOfTable,
            FriendlyPiece,
            EnemyPiece,
            None
        }

        public enum TeamType
        {
            White,
            Black
        }

        public enum MovementType
        {
            Normal,
            Attack,
            None
        }

        public enum MoveType
        {
            EnPassant,
            Castling,
            Promotion,
            Normal
        }

        public enum AttackedBy
        {
            None,
            White,
            Black,
            Both
        }
    
        public static void GeneratePossibleMovesBasedOnXAndYStep(Moves moves, ChessPiece chessPiece, int stepX, int stepY)
        {
            List<Vector2Int> possibleMoves = new();

            while (true)
            {
                var lastAddedMove =
                    possibleMoves.Count == 0
                        ? new Vector2Int(chessPiece.currentX, chessPiece.currentY)
                        : possibleMoves[^1];

                Vector2Int possibleMove = new(lastAddedMove.x + stepX, lastAddedMove.y + stepY);
                var occupationType = chessPiece.MovementManager.CalculateSpaceOccupation(possibleMove, chessPiece.team);

                if (occupationType is TileOccupiedBy.FriendlyPiece or TileOccupiedBy.EndOfTable)
                {
                    break;
                }

                if (TileOccupiedBy.EnemyPiece == occupationType)
                {
                    moves.AttackMoves.Add(possibleMove);
                    break;
                }

                moves.AvailableMoves.Add(possibleMove);
                possibleMoves.Add(possibleMove);
            }

        }

    }
}