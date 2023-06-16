using ChessLogic;
using ChessPieces;
using UnityEngine;

namespace Managers
{
    public class TileManager : MonoBehaviour
    {
        [Header("Prefabs & Materials")]
        public Material[] tilesMaterials;
        
        [Header("Tile Logic")]
        public const float TileSize = 0.120f;
        public float yOffset = 0.005f;
        public const int TileCountX = 8;
        public const int TileCountY = 8;
        public Vector3 Bounds { get; set; }
        public GameObject[,] Tiles { get; set; }

        public Vector3 GetTileCenter(int x, int y)
        {
            return new Vector3(x * TileSize, yOffset, y * TileSize) - Bounds + new Vector3(TileSize / 2, 0, TileSize / 2);
        }

        public void UpdateTileMaterialAfterMove(ChessPiece chessPiece)
        {
            foreach (var move in chessPiece.Moves)
            {
                UpdateTileMaterial(new Vector2Int(move.Coords.x, move.Coords.y), Shared.TileType.Default);
                var tile = GetTile(move.Coords);
                
                tile.IsSpecialTile = false;                
                tile.IsAttackTile = false;
                tile.IsAvailableTile = false;
            }
        }
        
        public void UpdateTileMaterial(Vector2Int tileCoord, Shared.TileType materialType)
        {
            Tiles[tileCoord.x, tileCoord.y].GetComponent<MeshRenderer>().material = tilesMaterials[(int)materialType];
        }

        public bool IsTileWhite(Vector2Int tileCoord)
        {
            return Tiles[tileCoord.x, tileCoord.y].GetComponent<Tile>().IsWhiteTile;
        }
        
        public void HandleTileTrigger(Vector2Int position, bool enterTrigger, Shared.MovementType movementType)
        {
            if (enterTrigger)
            {
                var tileType = Shared.MovementType.Normal == movementType ? Shared.TileType.HighlightMoveTo : Shared.TileType.HighlightAttack;
                UpdateTileMaterial(position, tileType);
            }
            else
            {
                var tile = Tiles[position.x, position.y];
                if (tile.GetComponent<Tile>().IsWhiteTile)
                {
                    var tileType = Shared.MovementType.Normal == movementType ? Shared.TileType.AvailableWhite : Shared.TileType.AttackTileWhite;
                    UpdateTileMaterial(position, tileType);
                }
                else
                {
                    var tileType = Shared.MovementType.Normal == movementType ? Shared.TileType.AvailableBlack : Shared.TileType.AttackTileBlack;
                    UpdateTileMaterial(position, tileType);
                }
            }
        }

        public void ResetTileAttackedStatus()
        {
            foreach (var tile in Tiles)
            {
                tile.GetComponent<Tile>().ResetAttackStatus();
            }
        }

        public void DetermineAttackStatus()
        {
            foreach (var tile in Tiles)
            {
                tile.GetComponent<Tile>().DetermineAttackStatus();
            }
        }
        
        public Tile GetTile(Vector2Int tileCoord)
        {
            return GetTile(tileCoord.x, tileCoord.y);
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y].GetComponent<Tile>();
        }
    }
}
