using System.Collections.Generic;
using ChessLogic;
using ChessPieces;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Managers
{
    public class Chessboard : MonoBehaviour
    {
        // Managers
        public MovementManager MovementManager { get; set; }
        public TileManager TileManager { get; set; }
    
        //Generation Logic
        private Vector3 boardCenter;

        [Header("Prefabs & Materials")]
        public GameObject[] prefabs;
        public Material[] teamMaterials;

        [Header("Game Objects")] 
        public GameObject undoButton;
        public void StartGame()
        {
            MovementManager.WhitePieces = new List<GameObject>();
            MovementManager.BlackPieces = new List<GameObject>();
        
            boardCenter = new Vector3(transform.position.x * -1, 0, transform.position.z * -1);
            TileManager.Bounds = 
                new Vector3((TileManager.TileCountX / 2.0f) * TileManager.TileSize, 0, (TileManager.TileCountY / 2.0f) * TileManager.TileSize) + boardCenter;
        
            GenerateAllTiles();
            GenerateEliminationPositions();

            SpawnAllPieces();
            PositionAllPieces();

            undoButton.GetComponent<MeshRenderer>().enabled = true;
            undoButton.GetComponent<XRGrabInteractable>().enabled = true;
            undoButton.GetComponent<BoxCollider>().enabled = true;
        }
    
        private void GenerateEliminationPositions()
        {
            LinkedList<Vector3> blackEliminationPosition = new();
            LinkedList<Vector3> whiteEliminationPosition = new();
        
            var eliminationTiles = transform.Find(Shared.EliminationTileParentName);
            var whiteEliminationTiles = eliminationTiles.Find(Shared.WhiteEliminationTilesName);
            var blackEliminationTiles = eliminationTiles.Find(Shared.BlackEliminationTilesName);

            for (int i = 0; i < whiteEliminationTiles.childCount; i++)
            {
                blackEliminationPosition.AddLast(whiteEliminationTiles.GetChild(i).position);
            }

            for (int i = 0; i < blackEliminationTiles.childCount; i++)
            {
                whiteEliminationPosition.AddLast(blackEliminationTiles.GetChild(i).position);
            }

            MovementManager.FreeWhiteEliminationPosition = whiteEliminationPosition;
            MovementManager.FreeBlackEliminationPosition = blackEliminationPosition;
            
            MovementManager.UsedWhiteEliminationPosition = new LinkedList<Vector3>();
            MovementManager.UsedBlackEliminationPosition = new LinkedList<Vector3>();
            Destroy(eliminationTiles.gameObject);
        }
    
        private void GenerateAllTiles()
        {
            TileManager.yOffset += transform.position.y;
        

            var tiles = new GameObject[TileManager.TileCountX, TileManager.TileCountY];
            //To easily determine the type of the generated tile we add the bool isTileTypeWhite and set it to true.
            // For each x iteration we negate the type as each new row starts with what the previous row ended
            // For each y iteration we negate the type as the adjacent tile should be of a different type.
            var isTileTypeWhite = true;
            for (var x = 0; x < TileManager.TileCountX; x++)
            {
                isTileTypeWhite = !isTileTypeWhite;
                for (var y = 0; y < TileManager.TileCountY; y++)
                {
                    tiles[x, y] = GenerateSingleTile(x, y, isTileTypeWhite);
                    isTileTypeWhite = !isTileTypeWhite;
                }
            }

            TileManager.Tiles = tiles;
        }
    
        private GameObject GenerateSingleTile(int x, int y, bool isTileWhite)
        {
            var tileObject = new GameObject($"Tile: X:{x}, Y:{y}")
            {
                transform =
                {
                    parent = transform
                }
            };

            tileObject.AddComponent<Tile>();
            tileObject.GetComponent<Tile>().Position = new Vector2Int(x, y);
            tileObject.GetComponent<Tile>().IsWhiteTile = isTileWhite;
            tileObject.GetComponent<Tile>().TileManager = TileManager;

            var mesh = new Mesh();
            tileObject.AddComponent<MeshFilter>().mesh = mesh;
            tileObject.AddComponent<MeshRenderer>().material = TileManager.tilesMaterials[((int)Shared.TileType.Default)];

            var vertices = new Vector3[4];
            vertices[0] = new Vector3(x * TileManager.TileSize, TileManager.yOffset, y * TileManager.TileSize) - TileManager.Bounds;
            vertices[1] = new Vector3(x * TileManager.TileSize, TileManager.yOffset, (y + 1) * TileManager.TileSize) - TileManager.Bounds;
            vertices[2] = new Vector3((x + 1) * TileManager.TileSize, TileManager.yOffset, y * TileManager.TileSize) - TileManager.Bounds;
            vertices[3] = new Vector3((x + 1) * TileManager.TileSize, TileManager.yOffset, (y + 1) * TileManager.TileSize) - TileManager.Bounds;

            var tris = new int[] { 0, 1, 2, 1, 3, 2 };

            mesh.vertices = vertices;
            mesh.triangles = tris;

            mesh.RecalculateNormals();

            tileObject.layer = LayerMask.NameToLayer("Tile");
            tileObject.AddComponent<BoxCollider>();
            tileObject.GetComponent<BoxCollider>().isTrigger = true;

            return tileObject;
        }
    
        private void SpawnAllPieces()
        {
            var chessPieces = new ChessPiece[TileManager.TileCountX, TileManager.TileCountY];

            // white team
            chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
            chessPieces[0, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
            chessPieces[0, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
            chessPieces[0, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.White);
            MovementManager.SetKing(chessPieces[0, 3]);
            chessPieces[0, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.White);
            chessPieces[0, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
            chessPieces[0, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
            chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
            for (int i = 0; i < TileManager.TileCountX; i++)
            {
                chessPieces[1, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.White);
            }

            //black team
            chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
            chessPieces[7, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
            chessPieces[7, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
            chessPieces[7, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.Black);
            MovementManager.SetKing(chessPieces[7, 3]);
            chessPieces[7, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.Black);
            chessPieces[7, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
            chessPieces[7, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
            chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
            for (int i = 0; i < TileManager.TileCountX; i++)
            {
                chessPieces[6, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.Black);
            }

            MovementManager.ChessPieces = chessPieces;
        }
    
        public ChessPiece SpawnSinglePiece(ChessPieceType type, Shared.TeamType team)
        {
            var cpGameObject = Instantiate(prefabs[(int)type - 1], transform);
            AddTileDetector(cpGameObject);
        
            var cp = cpGameObject.GetComponent<ChessPiece>();
            cp.GetComponent<ChessPiece>().MovementManager = MovementManager;
            cp.GetComponent<ChessPiece>().Moves = new List<Move>();
        
            cp.type = type;
            cp.team = team;
            cp.GetComponent<MeshRenderer>().material = teamMaterials[(int)team];
            cp.Material = teamMaterials[(int)team];

            if (Shared.TeamType.Black.Equals(team))
            {
                MovementManager.BlackPieces.Add(cpGameObject);
            }
            else
            {
                MovementManager.WhitePieces.Add(cpGameObject);
            }
            
            return cp;
        }
    
        private static void AddTileDetector(GameObject cpGameObject)
        {
            var cpTileDetector = new GameObject(Shared.TileDetectorName)
            {
                transform =
                {
                    parent = cpGameObject.transform
                }
            };

            cpTileDetector.AddComponent<TileDetector>();
            cpTileDetector.GetComponent<TileDetector>().ChessPiece = cpGameObject.GetComponent<ChessPiece>();
            cpTileDetector.GetComponent<TileDetector>().ChessPiece = cpGameObject.GetComponent<ChessPiece>();

            cpTileDetector.AddComponent<BoxCollider>();
            BoxCollider tileDetectorBoxCollider = cpTileDetector.GetComponent<BoxCollider>();
            tileDetectorBoxCollider.isTrigger = true;
            tileDetectorBoxCollider.center = new Vector3(0.0f, -0.065f, 0.0f);
            tileDetectorBoxCollider.size = new Vector3(0.002f, 0.20f, 0.02f);
            tileDetectorBoxCollider.enabled = false;
        }
    
        private void PositionAllPieces()
        {
            for (int x = 0; x < TileManager.TileCountX; x++)
            for (int y = 0; y < TileManager.TileCountY; y++)
                if (MovementManager.ChessPieces[x, y] != null)
                    PositionSinglePiece(x, y);
        }
    
        public void PositionSinglePiece(int x, int y)
        {
            MovementManager.ChessPieces[x, y].currentX = x;
            MovementManager.ChessPieces[x, y].currentY = y;
            MovementManager.ChessPieces[x, y].startingPosition = (new Vector2Int(x, y));
            MovementManager.ChessPieces[x, y].transform.position = TileManager.GetTileCenter(x, y);
            MovementManager.ChessPieces[x, y].transform.Find(Shared.TileDetectorName).transform.position = TileManager.GetTileCenter(x, y);
            MovementManager.ChessPieces[x, y].SavePosition();
            MovementManager.ChessPieces[x, y].SaveOrientation();
        }
    }
}
