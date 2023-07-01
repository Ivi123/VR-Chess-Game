using System;
using System.Collections.Generic;
using ChessLogic;
using ChessPieces;
using UnityEngine;

public class Turn
{
    public MovedPieces PiecesMovedInThisTurn { get; set; }
    public Shared.MoveType MoveType { get; set; }

    public Shared.TeamType Team { get; set; }
    
    public Turn(MovedPieces piecesMovedInThisTurn, Shared.MoveType moveType, Shared.TeamType team)
    {
        PiecesMovedInThisTurn = piecesMovedInThisTurn;
        MoveType = moveType;
        Team = team;
    }
}

public class MovedPieces
{
    public static readonly Vector2Int EliminationPosition = new Vector2Int(-1, -1);
    
    public List<ChessPiece> Pieces { get; set; }
    public List<Tuple<Vector2Int, Vector2Int>> PositionChanges { get; set; }

    public MovedPieces()
    {
        Pieces = new List<ChessPiece>();
        PositionChanges = new List<Tuple<Vector2Int, Vector2Int>>();
    }

    public void AddNewPieceAndPosition(ChessPiece piece, Vector2Int newPosition)
    {
        var oldPosition = new Vector2Int(piece.currentX, piece.currentY);
        var positionChange = new Tuple<Vector2Int, Vector2Int>(oldPosition, newPosition);

        Pieces.Add(piece);
        PositionChanges.Add(positionChange);
    }
}
