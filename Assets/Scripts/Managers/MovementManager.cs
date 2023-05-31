using System.Collections.Generic;
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
        private Moves Moves { get; set; } 
        
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

            DisablePickUpOnOtherPieces(ChessPieces[x, y]);
        }
    
        public void PieceWasDropped(int currentX, int currentY, Tile newTile)
        {
            if (newTile != null)
            {
                Vector2Int newPosition = newTile.Position;

                if (newTile.IsAttackTile)
                {
                    ChessPiece enemyPiece = ChessPieces[newPosition.x, newPosition.y];

                    Vector3 eliminationPosition;
                    if(enemyPiece.team == Shared.TeamType.White)
                    {
                        eliminationPosition = WhiteEliminationPosition[0];
                        WhiteEliminationPosition.RemoveAt(0);
                    }
                    else
                    {
                        eliminationPosition = BlackEliminationPosition[0];
                        BlackEliminationPosition.RemoveAt(0);
                    }

                    enemyPiece.transform.position = eliminationPosition;
                    enemyPiece.GetComponent<XRGrabInteractable>().enabled = false;
                }

                var chessPiece = ChessPieces[currentX, currentY];
                chessPiece.transform.position = TileManager.GetTileCenter(newPosition.x, newPosition.y);
                ChessPieces[newPosition.x, newPosition.y] = chessPiece;
                ChessPieces[currentX, currentY] = null;
                chessPiece.currentX = newPosition.x;
                chessPiece.currentY = newPosition.y;

                chessPiece.SavePosition();
                chessPiece.IsMoved = true;
            }

            TileManager.Tiles[currentX, currentY].GetComponent<MeshRenderer>().material = TileManager.tilesMaterials[((int)Shared.TileType.Default)];
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
            EnablePickUpOnPieces();
        }

        private void DisablePickUpOnOtherPieces(ChessPiece pickedPiece)
        {
            foreach (var piece in ChessPieces)
            {
                if (piece != null && !piece.Equals(pickedPiece))
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = false;
                }
            }
        }
    
        private void EnablePickUpOnPieces()
        {
            foreach (var piece in ChessPieces)
            {
                if (piece != null)
                {
                    piece.GetComponent<XRGrabInteractable>().enabled = true;
                }
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
