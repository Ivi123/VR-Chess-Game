using UnityEngine;

/// <summary>
/// Defines the <see cref="TileDetector" />.
/// </summary>
public class TileDetector : MonoBehaviour
{
    //--------------------------------------------------------- VARIABLES ----------------------------------------------------------

    public ChessPiece ChessPiece { get; set; }

    //---------------------------------------------------------- METHODS ----------------------------------------------------------

    public void OnTriggerEnter(Collider other)
    {
        Tile hoveredTile = other.gameObject.GetComponent<Tile>();
        if (hoveredTile == null) return;

        Shared.MoveType moveType =
            hoveredTile.IsAvailableTile ? Shared.MoveType.Normal
            : hoveredTile.IsAttackTile ? Shared.MoveType.Attack
            : Shared.MoveType.None;
        if (moveType == Shared.MoveType.None) return;
        
        ChessPiece.HoveringTile = hoveredTile;
        hoveredTile.TriggeredAvailableMove(moveType);
    }

    public void OnTriggerExit(Collider other)
    {
        Tile hoveredTile = other.gameObject.GetComponent<Tile>();
        if (hoveredTile == null) return;

        if (hoveredTile.IsAvailableTile)
        {
            hoveredTile.ExitedTriggeredAvailableMove(Shared.MoveType.Normal);
        } else if (hoveredTile.IsAttackTile)
        {
            hoveredTile.ExitedTriggeredAvailableMove(Shared.MoveType.Attack);
        }

        if (hoveredTile.Equals(ChessPiece.HoveringTile))
        {
            ChessPiece.HoveringTile = null;
        }
    }
}
