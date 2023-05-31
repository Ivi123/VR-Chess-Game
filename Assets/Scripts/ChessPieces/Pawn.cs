using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the <see cref="Pawn" />.
/// </summary>
public class Pawn : ChessPiece
{

    public override Moves CalculateAvailablePositions()
    {
        Moves moves = new();

        int direction = Shared.TeamType.White.Equals(team) ? 1 : -1;

        Vector2Int possibleMoveOneTileAhead = new(currentX + (direction * 1), currentY);
        if (Shared.TileOccupiedBy.None == MovementManager.CalculateSpaceOccupation(possibleMoveOneTileAhead, team)) moves.AvailableMoves.Add(possibleMoveOneTileAhead);
        if (!isMoved)
        {
            Vector2Int possibleMoveTwoTilesAhead = new(currentX + (direction * 2), currentY);
            if (Shared.TileOccupiedBy.None == MovementManager.CalculateSpaceOccupation(possibleMoveTwoTilesAhead, team)) moves.AvailableMoves.Add(possibleMoveTwoTilesAhead);
        }

        Vector2Int attackMoveLeft = new(currentX + (direction * 1), currentY + 1);
        if (Shared.TileOccupiedBy.EnemyPiece == MovementManager.CalculateSpaceOccupation(attackMoveLeft, team)) moves.AttackMoves.Add(attackMoveLeft);

        Vector2Int attackMoveRight = new(currentX + (direction * 1), currentY - 1);
        if (Shared.TileOccupiedBy.EnemyPiece == MovementManager.CalculateSpaceOccupation(attackMoveRight, team)) moves.AttackMoves.Add(attackMoveRight);

        return moves;
    }
}
