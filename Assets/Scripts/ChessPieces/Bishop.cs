public class Bishop : ChessPiece
{
    public override Moves CalculateAvailablePositions()
    {
        Moves moves = new();

        moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 1);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 1);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, 1, -1);
        moves.GeneratePossibleMovesBasedOnXAndYStep(this, -1, -1);

        return moves;
    }
}
