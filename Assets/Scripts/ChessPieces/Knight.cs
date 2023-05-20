using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> CalculateAvailablePositions()
    {
        int direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

        Vector2Int horseForwardMove_1 = new(currentX + (direction * 2), currentY + 1);
        Vector2Int horseForwardMove_2 = new(currentX + (direction * 2), currentY - 1);

        Vector2Int horseBackwardMove_1 = new(currentX - (direction * 2), currentY + 1);
        Vector2Int horseBackwardMove_2 = new(currentX - (direction * 2), currentY - 1);

        Vector2Int horseLeftMove_1 = new(currentX + (direction * 1), currentY + 2);
        Vector2Int horseLeftMove_2 = new(currentX - (direction * 1), currentY + 2);

        Vector2Int horseRightMove_1 = new(currentX + (direction * 1), currentY - 2);
        Vector2Int horseRightMove_2 = new(currentX + (direction * 1), currentY - 2);

        List<Vector2Int> possibleMoves = new() { horseForwardMove_1, horseForwardMove_2, horseBackwardMove_1, horseBackwardMove_2, 
            horseLeftMove_1, horseLeftMove_2, horseRightMove_1, horseRightMove_2 };

        return possibleMoves.FindAll(move => !Chessboard.IsSpaceOccupied(move));
    }
}
