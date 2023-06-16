using System.Collections.Generic;
using ChessLogic;
using UnityEngine;

namespace ChessPieces
{
    public class Bishop : ChessPiece
    {
        public override void CalculateAvailablePositions()
        {
            Moves = new List<Move>();
            
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, 1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, 1, -1));
            Moves.AddRange(Shared.GeneratePossibleMovesBasedOnXAndYStep(this, -1, -1));
        }
    }
}
