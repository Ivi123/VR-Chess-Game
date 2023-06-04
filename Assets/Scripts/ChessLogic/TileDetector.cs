using ChessPieces;
using UnityEngine;

namespace ChessLogic
{
    public class TileDetector : MonoBehaviour
    {
        //--------------------------------------------------------- VARIABLES ----------------------------------------------------------

        public ChessPiece ChessPiece { get; set; }

        //---------------------------------------------------------- METHODS ----------------------------------------------------------

        public void OnTriggerEnter(Collider other)
        {
            var hoveredTile = other.gameObject.GetComponent<Tile>();
            if (hoveredTile == null) return;

            var movementType =
                hoveredTile.IsAvailableTile ? Shared.MovementType.Normal
                : hoveredTile.IsAttackTile ? Shared.MovementType.Attack
                : Shared.MovementType.None;
            if (movementType == Shared.MovementType.None) return;
        
            ChessPiece.HoveringTile = hoveredTile;
            hoveredTile.TriggeredAvailableMove(movementType);
        }

        public void OnTriggerExit(Collider other)
        {
            var hoveredTile = other.gameObject.GetComponent<Tile>();
            if (hoveredTile == null) return;

            if (hoveredTile.IsAvailableTile)
            {
                hoveredTile.ExitedTriggeredAvailableMove(Shared.MovementType.Normal);
            } else if (hoveredTile.IsAttackTile)
            {
                hoveredTile.ExitedTriggeredAvailableMove(Shared.MovementType.Attack);
            }

            if (hoveredTile.Equals(ChessPiece.HoveringTile))
            {
                ChessPiece.HoveringTile = null;
            }
        }
    }
}
