using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> CalculateAvailablePositions()
    {
        List<Vector2Int> possibleMoves = new();
        int direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

        Vector2Int possibleMove_OneTileAhead = new(currentX + (direction * 1), currentY);
        if (!Chessboard.IsSpaceOccupied(possibleMove_OneTileAhead)) possibleMoves.Add(possibleMove_OneTileAhead);
        if (!isMoved)
        {
            Vector2Int possibleMove_TwoTilesAhead = new(currentX + (direction * 2), currentY);
            if (!Chessboard.IsSpaceOccupied(possibleMove_TwoTilesAhead)) possibleMoves.Add(possibleMove_TwoTilesAhead);
        }

        return possibleMoves;
    }
}
