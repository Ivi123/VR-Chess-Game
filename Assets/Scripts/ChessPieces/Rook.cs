using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override Moves CalculateAvailablePositions()
    {
        Moves moves = new();

        moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 0);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, 0, 1);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 0);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, 0, -1);

        return moves;
    }

}
