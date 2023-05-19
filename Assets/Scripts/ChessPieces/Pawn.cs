using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    // TODO: Add bound checks to make sure that a piece is not trying to move out of the board
    public override List<Vector2Int> CalculateAvailablePositions()
    {
        List<Vector2Int> possibleMoves = new();

        switch (team)
        {
            case Shared.TeamType.White:
                Vector2Int possibleMove_OneTileAhead_White = new Vector2Int(currentX + 1, currentY);
                if (!Chessboard.IsSpaceOccupied(possibleMove_OneTileAhead_White)) possibleMoves.Add(possibleMove_OneTileAhead_White);
                if (!isTouched)
                {
                    Vector2Int possibleMove_TwoTilesAhead_White = new Vector2Int(currentX + 2, currentY);
                    if (!Chessboard.IsSpaceOccupied(possibleMove_TwoTilesAhead_White)) possibleMoves.Add(possibleMove_TwoTilesAhead_White);
                }
                break;
            case Shared.TeamType.Black:
                Vector2Int possibleMove_OneTileAhead_Black = new Vector2Int(currentX - 1, currentY);
                if (!Chessboard.IsSpaceOccupied(possibleMove_OneTileAhead_Black)) possibleMoves.Add(possibleMove_OneTileAhead_Black);
                if (!isTouched)
                {
                    Vector2Int possibleMove_TwoTilesAhead_Black = new Vector2Int(currentX - 2, currentY);
                    if (!Chessboard.IsSpaceOccupied(possibleMove_TwoTilesAhead_Black)) possibleMoves.Add(possibleMove_TwoTilesAhead_Black);
                }
                break;
        }

        return possibleMoves;
    }
}
