using Managers;
using UnityEngine;

/// <summary>
/// Defines the <see cref="Tile" />.
/// </summary>
public class Tile : MonoBehaviour
{
    public bool IsWhiteTile { get; set; }
    public bool IsAvailableTile { get; set; }
    public bool IsAttackTile { get; set; }
    public Vector2Int Position { get; set; }
    public TileManager TileManager { get; set; }
    
    public Tile(int x, int y)
    {
        Position = new Vector2Int(x, y);
    }
    
    public void TriggeredAvailableMove(Shared.MoveType moveType)
    {
        TileManager.HandleTileTrigger(Position, true, moveType);
    }
    
    public void ExitedTriggeredAvailableMove(Shared.MoveType moveType)
    {
        TileManager.HandleTileTrigger(Position, false, moveType);
    }
}
