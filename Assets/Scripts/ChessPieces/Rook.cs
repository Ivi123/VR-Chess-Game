using ChessLogic;

namespace ChessPieces
{
    public class Rook : ChessPiece
    {
        public override void CalculateAvailablePositions()
        {
            Moves = new Moves();
            
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 0);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, 0, 1);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 0);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, 0, -1);
        }

    }
}
