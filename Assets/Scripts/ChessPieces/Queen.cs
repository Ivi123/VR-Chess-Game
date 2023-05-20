using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> CalculateAvailablePositions()
    {
        List<Vector2Int> possibleMoves = new();
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 0));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 0, 1));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 0));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 0, -1));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 1));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 1));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, -1));
        possibleMoves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, -1));

        return possibleMoves;
    }
}
