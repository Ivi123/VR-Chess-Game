using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Managers
{
    public class MovementManager : MonoBehaviour
    {
        public ChessPiece[,] ChessPieces { get; set; }
        public List<Vector3> WhiteEliminationPosition { get; set; }
        public List<Vector3> BlackEliminationPosition { get; set; }
        public TileManager TileManager { get; set; }
        public GameManager GameManager { get; set; }
        private Moves Moves { get; set; } 
        public List<GameObject> WhitePieces { get; set; }
        public List<GameObject> BlackPieces { get; set; }
        
        public void PieceWasPickedUp(int x, int y)
        {
            TileManager.UpdateTileMaterial(new Vector2Int(x, y), Shared.TileType.Selected);
            Moves = ChessPieces[x, y].CalculateAvailablePositions();
            foreach (var move in Moves.AvailableMoves)
            {
                var moveCoord = new Vector2Int(move.x, move.y);
                TileManager.UpdateTileMaterial(moveCoord, 
                    TileManager.IsTileWhite(moveCoord) 
                        ? Shared.TileType.AvailableWhite 
                        : Shared.TileType.AvailableBlack);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = true;
            }

            foreach (var move in Moves.AttackMoves)
            {
                var moveCoord = new Vector2Int(move.x, move.y);
                TileManager.UpdateTileMaterial(moveCoord, 
                    TileManager.IsTileWhite(moveCoord) 
                        ? Shared.TileType.AttackTileWhite 
                        : Shared.TileType.AttackTileBlack);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAttackTile = true;
            }

            var pickedPiece = ChessPieces[x, y];
            DisablePickUpOnOtherPieces(pickedPiece.gameObject, pickedPiece.team);
        }
    
        public void PieceWasDropped(int currentX, int currentY, Tile newTile)
        {
            var chessPiece = ChessPieces[currentX, currentY];
            EnablePickUpOnPieces(chessPiece.team);

            if (newTile != null)
            {
                Vector2Int newPosition = newTile.Position;

                if (newTile.IsAttackTile)
                {
                    ChessPiece enemyPiece = ChessPieces[newPosition.x, newPosition.y];

                    Vector3 eliminationPosition;
                    if(enemyPiece.team == Shared.TeamType.White)
                    {
                        WhitePieces.Remove(enemyPiece.gameObject);
                        eliminationPosition = WhiteEliminationPosition[0];
                        WhiteEliminationPosition.RemoveAt(0);
                    }
                    else
                    {
                        BlackPieces.Remove(enemyPiece.gameObject);
                        eliminationPosition = BlackEliminationPosition[0];
                        BlackEliminationPosition.RemoveAt(0);
                    }

                    enemyPiece.transform.position = eliminationPosition;
                    enemyPiece.GetComponent<XRGrabInteractable>().enabled = false;
                    
                }

                chessPiece.transform.position = TileManager.GetTileCenter(newPosition.x, newPosition.y);
                ChessPieces[newPosition.x, newPosition.y] = chessPiece;
                ChessPieces[currentX, currentY] = null;
                chessPiece.currentX = newPosition.x;
                chessPiece.currentY = newPosition.y;

                chessPiece.SavePosition();
                chessPiece.IsMoved = true;

                GameManager.IsWhiteTurn = !GameManager.IsWhiteTurn;
                DisableOrEnablePickUpOnPieces(WhitePieces);
                DisableOrEnablePickUpOnPieces(BlackPieces);
            }

            TileManager.UpdateTileMaterial(new Vector2Int(currentX, currentY), Shared.TileType.Default);
            foreach (var move in Moves.AvailableMoves)
            {
                TileManager.UpdateTileMaterial(new Vector2Int(move.x, move.y), Shared.TileType.Default);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = false;
            }

            foreach (var move in Moves.AttackMoves)
            {
                TileManager.UpdateTileMaterial(new Vector2Int(move.x, move.y), Shared.TileType.Default);
                TileManager.Tiles[move.x, move.y].GetComponent<Tile>().IsAttackTile = false;
            }

            Moves = null;
        }

        private void DisablePickUpOnOtherPieces(GameObject pickedPiece, Shared.TeamType team)
        {
            if (Shared.TeamType.White.Equals(team))
            {
                foreach (var piece in WhitePieces.Where(piece => !piece.Equals(pickedPiece)))
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = false;
                }
            }
            else
            {
                foreach (var piece in BlackPieces.Where(piece => !piece.Equals(pickedPiece)))
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = false;
                }
            }
            
        }

        public void DisableOrEnablePickUpOnPieces(List<GameObject> pieces)
        {
            foreach (var piece in pieces)
            {
                piece.GetComponent<XRGrabInteractable>().enabled 
                    = !piece.GetComponent<XRGrabInteractable>().enabled;
            }
        }
    
        private void EnablePickUpOnPieces(Shared.TeamType team)
        {
            var teamToBeEnabled = Shared.TeamType.White.Equals(team) ? WhitePieces : BlackPieces;
            foreach (var piece in teamToBeEnabled)
            { 
                piece.GetComponent<XRGrabInteractable>().enabled = true;
            }
        }
        
        public Shared.TileOccupiedBy CalculateSpaceOccupation(Vector2Int position, Shared.TeamType selectedPieceTeam)
        {
            ChessPiece chessPiece;
            try
            {
                chessPiece = ChessPieces[position.x, position.y];
            }
            catch
            {
                return Shared.TileOccupiedBy.EndOfTable;
            }

            if (chessPiece == null)
            {
                return Shared.TileOccupiedBy.None;
            }

            return selectedPieceTeam.Equals(chessPiece.team) ? Shared.TileOccupiedBy.FriendlyPiece : Shared.TileOccupiedBy.EnemyPiece;
        }
    }
}
