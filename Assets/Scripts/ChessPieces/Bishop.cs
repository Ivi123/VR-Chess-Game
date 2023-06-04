using ChessLogic;

namespace ChessPieces
{
    public class Bishop : ChessPiece
    {
        public override void CalculateAvailablePositions()
        {
            Moves = new Moves();
            
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 1);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 1);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, -1);
            Moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, -1);
        }
    }
}
