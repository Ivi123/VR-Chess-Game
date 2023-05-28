using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the <see cref="Pawn" />.
/// </summary>
public class Pawn : ChessPiece
{
    /// <summary>
    /// The CalculateAvailablePositions.
    /// </summary>
    /// <returns>The <see cref="List{Vector2Int}"/>.</returns>
    public override Moves CalculateAvailablePositions()
    {
        Moves moves = new();

        int direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

        Vector2Int possibleMove_OneTileAhead = new(currentX + (direction * 1), currentY);
        if (Shared.TileOccuppiedBy.None == Chessboard.CalculateSpaceOccupation(possibleMove_OneTileAhead, team)) moves.AvailableMoves.Add(possibleMove_OneTileAhead);
        if (!isMoved)
        {
            Vector2Int possibleMove_TwoTilesAhead = new(currentX + (direction * 2), currentY);
            if (Shared.TileOccuppiedBy.None == Chessboard.CalculateSpaceOccupation(possibleMove_TwoTilesAhead, team)) moves.AvailableMoves.Add(possibleMove_TwoTilesAhead);
        }

        Vector2Int attackMove_Left = new(currentX + (direction * 1), currentY + 1);
        if (Shared.TileOccuppiedBy.EnemyPiece == Chessboard.CalculateSpaceOccupation(attackMove_Left, team)) moves.AttackMoves.Add(attackMove_Left);

        Vector2Int attackMove_Right = new(currentX + (direction * 1), currentY - 1);
        if (Shared.TileOccuppiedBy.EnemyPiece == Chessboard.CalculateSpaceOccupation(attackMove_Right, team)) moves.AttackMoves.Add(attackMove_Right);

        return moves;
    }
}
