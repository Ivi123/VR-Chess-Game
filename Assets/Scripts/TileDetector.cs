using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDetector : MonoBehaviour
{
    //--------------------------------------------------------- VARIABLES ----------------------------------------------------------

    private ChessPiece chessPiece;
    public ChessPiece ChessPiece { get => chessPiece; set => chessPiece = value; }

    //---------------------------------------------------------- METHODS ----------------------------------------------------------

    public void OnTriggerEnter(Collider other)
    {
        Tile hoveredTile = other.gameObject.GetComponent<Tile>();
        if (hoveredTile != null && hoveredTile.IsAvailableTile)
        {
            chessPiece.HoveringTile = hoveredTile;
            hoveredTile.TriggeredAvailableMove();
        }
        else if (hoveredTile != null)
        {
            chessPiece.HoveringTile = null;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Tile hoveredTile = other.gameObject.GetComponent<Tile>();
        if (hoveredTile != null && hoveredTile.IsAvailableTile)
        {
            hoveredTile.ExitedTriggeredAvailableMove();
            if (hoveredTile.Equals(chessPiece.HoveringTile))
            {
                chessPiece.HoveringTile = null;

            }
        }
    }
}
