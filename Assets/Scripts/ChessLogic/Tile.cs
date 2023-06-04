using System.Collections.Generic;
using ChessLogic;
using ChessPieces;
using Managers;
using UnityEngine;

/// <summary>
/// Defines the <see cref="Tile" />.
/// </summary>
public class Tile : MonoBehaviour
{
    
    public Vector2Int Position { get; set; }
    public TileManager TileManager { get; set; }

    // Tile Attack Status

    public Shared.AttackedBy AttackedBy { get; set; }
    public List<ChessPiece> WhiteAttackingPieces { get; set; }
    public List<ChessPiece> BlackAttackingPieces { get; set; }

    // Tile Type Properties
    public bool IsWhiteTile { get; set; }
    public bool IsAvailableTile { get; set; }
    public bool IsAttackTile { get; set; }
    public bool IsSpecialTile { get; set; }
    
    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
        WhiteAttackingPieces = new List<ChessPiece>();
        BlackAttackingPieces = new List<ChessPiece>();
        AttackedBy = Shared.AttackedBy.None;
    }
    
    public void TriggeredAvailableMove(Shared.MovementType movementType)
    {
        TileManager.HandleTileTrigger(Position, true, movementType);
    }
    
    public void ExitedTriggeredAvailableMove(Shared.MovementType movementType)
    {
        TileManager.HandleTileTrigger(Position, false, movementType);
    }

    public void ResetAttackStatus()
    {
        AttackedBy = Shared.AttackedBy.None;
        WhiteAttackingPieces = new List<ChessPiece>();
        BlackAttackingPieces = new List<ChessPiece>();

        IsSpecialTile = false;
        IsAttackTile = false;
        IsSpecialTile = false;
    }
}
