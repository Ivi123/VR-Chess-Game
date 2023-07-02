using System.Collections.Generic;
using ChessLogic;
using Managers;
using UnityEngine;

namespace ChessPieces
{
    public class Knight : ChessPiece
    {
        public void Awake()
        {
            pieceScore = 3;
        }

        public override void CalculateAvailablePositions(ChessPiece[,] board, Tile[,] tiles)
        {
            protectsKing = false;
            Moves = new List<Move>();
            var direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

            Vector2Int horseForwardMove1 = new(currentX + (direction * 2), currentY + 1);
            Vector2Int horseForwardMove2 = new(currentX + (direction * 2), currentY - 1);

            Vector2Int horseBackwardMove1 = new(currentX - (direction * 2), currentY + 1);
            Vector2Int horseBackwardMove2 = new(currentX - (direction * 2), currentY - 1);

            Vector2Int horseLeftMove1 = new(currentX + (direction * 1), currentY + 2);
            Vector2Int horseLeftMove2 = new(currentX - (direction * 1), currentY + 2);

            Vector2Int horseRightMove1 = new(currentX + (direction * 1), currentY - 2);
            Vector2Int horseRightMove2 = new(currentX - (direction * 1), currentY - 2);

            List<Vector2Int> possibleMoves = new()
            {
                horseForwardMove1,
                horseForwardMove2,
                horseBackwardMove1,
                horseBackwardMove2,
                horseLeftMove1,
                horseLeftMove2,
                horseRightMove1,
                horseRightMove2
            };

            possibleMoves.ForEach(move =>
            {
                var occupationType = MovementManager.CalculateSpaceOccupation(board, move, team);
                if(occupationType != Shared.TileOccupiedBy.EndOfTable) AddToTileAttackingPieces(tiles, move);
                switch (occupationType)
                {
                    case Shared.TileOccupiedBy.None:
                        Moves.Add(new Move(move, Shared.MoveType.Normal));
                        break;
                    case Shared.TileOccupiedBy.EnemyPiece:
                        Moves.Add(new Move(move, Shared.MoveType.Attack));
                        break;
                }
            });
        }

        public override void MarkAttackedTiles(ChessPiece[,] board, Tile[,] tiles)
        {
            foreach (var move in Moves)
                AddToTileAttackingPieces(tiles, move.Coords);
        }
    }
}
