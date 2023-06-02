using UnityEngine;
using UnityEngine.Serialization;

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
                var tileType = Shared.MovementType.Normal == movementType ? Shared.TileType.MoveTo : Shared.TileType.HighlightAttack;
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
    }
}
