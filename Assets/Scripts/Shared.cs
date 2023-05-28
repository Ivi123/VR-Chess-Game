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
        AvailableBlack,
        AvailableWhite,
        MoveTo,
        AttackTileBlack,
        AttackTileWhite
    }

    public enum TileOccuppiedBy
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

    public static void GeneratePossibleMovesBasedOnXAndYStep(Moves moves, ChessPiece chessPiece, int stepX, int stepY)
    {
        List<Vector2Int> possibleMoves = new();

        while (true)
        {
            Vector2Int lastAddedMove =
                possibleMoves.Count == 0
                    ? new(chessPiece.currentX, chessPiece.currentY)
                    : possibleMoves[possibleMoves.Count - 1];

            Vector2Int possibleMove = new(lastAddedMove.x + stepX, lastAddedMove.y + stepY);
            var occupationType = chessPiece.Chessboard.CalculateSpaceOccupation(possibleMove, chessPiece.team);

            if (TileOccuppiedBy.FriendlyPiece == occupationType || TileOccuppiedBy.EndOfTable == occupationType)
            {
                break;
            }

            if(TileOccuppiedBy.EnemyPiece == occupationType)
            {
                moves.AttackMoves.Add(possibleMove);
                break;
            }

            moves.AvailableMoves.Add(possibleMove);
            possibleMoves.Add(possibleMove);
        }

    }

}
