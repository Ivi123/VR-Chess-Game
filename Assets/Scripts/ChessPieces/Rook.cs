using System.Collections.Generic;
using ChessLogic;

namespace ChessPieces
{
    public class Rook : ChessPiece
    {
        public void Awake()
        {
            pieceScore = 5;
        }

        public override void CalculateAvailablePositions(ChessPiece[,] board, Tile[,] tiles)
        {
            Moves = new List<Move>();

            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 0, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, -1, 0));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(board, tiles, this, 0, -1));
        }
        
    }
}
