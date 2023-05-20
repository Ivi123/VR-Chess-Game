using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> CalculateAvailablePositions()
    {
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

        return possibleMoves.FindAll(move => !Chessboard.IsSpaceOccupied(move));
    }
}
