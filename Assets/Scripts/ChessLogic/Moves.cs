using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChessLogic
{
    public class Moves
    {
        public List<Vector2Int> AttackMoves { get; set; }
        public List<Vector2Int> AvailableMoves { get; set; }
        public List<SpecialMove> SpecialMoves { get; set; }
        
        public Moves()
        {
            AvailableMoves = new();
            AttackMoves = new();
            SpecialMoves = new();
        }

        public Shared.MoveType FindSpecialMoveTypeFromCoords(Vector2Int coords)
        {
            return SpecialMoves.FirstOrDefault(move => move.Coords.Equals(coords))!.MoveType;
        }
        
        public void GeneratePossibleMovesBasedOnXAndYStep(ChessPiece chessPiece, int stepX, int stepY)
        {
            Shared.GeneratePossibleMovesBasedOnXAndYStep(this, chessPiece, stepX, stepY);
        }
    }

    public class SpecialMove
    {
        public Vector2Int Coords { get; set; }
        public Shared.MoveType MoveType { get; set; }

        public SpecialMove(Vector2Int moveCoords, Shared.MoveType moveType)
        {
            Coords = moveCoords;
            MoveType = moveType;
        }
    }
}
