using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moves
{
    public List<Vector2Int> AttackMoves { get; set; }
    public List<Vector2Int> AvailableMoves { get; set; }

    public Moves()
    {
        AvailableMoves = new();
        AttackMoves = new();
    }

    public void GeneratePossibleMovesBasedOnXAndYStep(ChessPiece chessPiece, int stepX, int stepY)
    {
        Shared.GeneratePossibleMovesBasedOnXAndYStep(this, chessPiece, stepX, stepY);
    }
}
