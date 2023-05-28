using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override Moves CalculateAvailablePositions()
    {
        Moves moves = new();
        int direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

        Vector2Int KingForwardMove = new(currentX, currentY + 1);
        Vector2Int KingBackwardMove = new(currentX, currentY - 1);
        Vector2Int KingLeftMove = new(currentX - 1, currentY);
        Vector2Int KingRightMove = new(currentX + 1, currentY);

        Vector2Int KingLeftForwardMove = new(currentX - 1, currentY + 1);
        Vector2Int KingLeftBackwardMove = new(currentX - 1, currentY - 1);
        Vector2Int KingRightBackwardMove = new(currentX + 1, currentY - 1);
        Vector2Int KingRightForwardMove = new(currentX + 1, currentY + 1);

        List<Vector2Int> possibleMoves = new()
        {
            KingForwardMove,
            KingBackwardMove,
            KingLeftMove,
            KingRightMove,
            KingLeftForwardMove,
            KingLeftBackwardMove,
            KingRightBackwardMove,
            KingRightForwardMove
        };

        possibleMoves.ForEach(move =>
        {
            var occupationType = Chessboard.CalculateSpaceOccupation(move, team);
            switch (occupationType)
            {
                case Shared.TileOccuppiedBy.None:
                    moves.AvailableMoves.Add(move);
                    break;
                case Shared.TileOccuppiedBy.EnemyPiece:
                    moves.AttackMoves.Add(move);
                    break;
            }
        });

        return moves;
    }
}
