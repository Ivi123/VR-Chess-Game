using System.Collections.Generic;
using ChessLogic;

namespace ChessPieces
{
    public class Rook : ChessPiece
    {
        public override void CalculateAvailablePositions()
        {
            Moves = new List<Move>();

            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 0, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 0, -1));
        }
    }
}
