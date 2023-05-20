using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shared
{
    public static readonly string TileDetectorName = "TileDetector";

    public enum TileType
    {
        Default,
        Selected,
        Available,
        MoveTo
    }

    public enum TeamType
    {
        White,
        Black
    }

    public static List<Vector2Int> GeneratePossibleMovesBasedOnXAndYStep(ChessPiece chessPiece, int stepX, int stepY)
    {
        List<Vector2Int> possibleMoves = new();

        while (true)
        {
            Vector2Int lastAddedMove = 
                possibleMoves.Count == 0 
                    ? new(chessPiece.currentX, chessPiece.currentY) 
                    : possibleMoves[possibleMoves.Count - 1];
            
            Vector2Int PossibleMove = new(lastAddedMove.x + stepX, lastAddedMove.y + stepY) ;

            if (chessPiece.Chessboard.IsSpaceOccupied(PossibleMove))
            {
                break;
            }

            possibleMoves.Add(PossibleMove);
        }

        return possibleMoves;
    }

}
