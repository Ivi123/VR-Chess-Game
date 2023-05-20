using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class Chessboard : MonoBehaviour
{

    [Header("Art stuff")]
    private float tileSize = 0.120f;
    public float yOffset;
    public Vector3 boardCenter;

    [Header("Prefabs & Materials")]
    public GameObject[] prefabs;
    public Material[] teamMaterials;
    public Material[] tilesMaterials;

    //Logic
    protected ChessPiece[,] chessPieces;
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    public Vector3 bounds;

    private List<Vector2Int> availableMoves;

    private void Awake()
    {
        boardCenter = new Vector3(transform.position.x * -1, 0, transform.position.z * -1);
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);

        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {

    }

    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
            for (int y = 0; y < tileCountY; y++)
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {

        GameObject tileObject = new GameObject(string.Format("Tile: X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        tileObject.AddComponent<Tile>();
        tileObject.GetComponent<Tile>().Position = new Vector2Int(x, y);

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();
        tileObject.GetComponent<BoxCollider>().isTrigger = true;

        return tileObject;
    }

    //Spawning the pieces
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        // white team
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
        chessPieces[0, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
        chessPieces[0, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
        chessPieces[0, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.White);
        chessPieces[0, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.White);
        chessPieces[0, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.White);
        chessPieces[0, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.White);
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.White);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[1, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.White);
        }

        //black team
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
        chessPieces[7, 1] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
        chessPieces[7, 2] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
        chessPieces[7, 3] = SpawnSinglePiece(ChessPieceType.King, Shared.TeamType.Black);
        chessPieces[7, 4] = SpawnSinglePiece(ChessPieceType.Queen, Shared.TeamType.Black);
        chessPieces[7, 5] = SpawnSinglePiece(ChessPieceType.Bishop, Shared.TeamType.Black);
        chessPieces[7, 6] = SpawnSinglePiece(ChessPieceType.Knight, Shared.TeamType.Black);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, Shared.TeamType.Black);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[6, i] = SpawnSinglePiece(ChessPieceType.Pawn, Shared.TeamType.Black);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, Shared.TeamType team)
    {
        GameObject cpGameObject = Instantiate(prefabs[(int)type - 1], transform);
        AddTileDetector(cpGameObject);

        ChessPiece cp = cpGameObject.GetComponent<ChessPiece>();

        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[(int)team];
        cp.Material = teamMaterials[(int)team];

        return cp;
    }

    private void AddTileDetector(GameObject cpGameObject)
    {
        GameObject cpTileDetector = new GameObject(Shared.TileDetectorName);
        cpTileDetector.transform.parent = cpGameObject.transform;

        cpTileDetector.AddComponent<TileDetector>();
        cpTileDetector.GetComponent<TileDetector>().ChessPiece = cpGameObject.GetComponent<ChessPiece>();

        cpTileDetector.AddComponent<BoxCollider>();
        BoxCollider tileDetectorBoxColider = cpTileDetector.GetComponent<BoxCollider>();
        tileDetectorBoxColider.isTrigger = true;
        tileDetectorBoxColider.center = new Vector3(0.0f, -0.065f, 0.0f);
        tileDetectorBoxColider.size = new Vector3(0.002f, 0.20f, 0.02f);
        tileDetectorBoxColider.enabled = false;
    }

    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
            for (int y = 0; y < TILE_COUNT_Y; y++)
                if (chessPieces[x, y] != null)
                    PositionSinglePiece(x, y, true);

    }

    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].transform.position = GetTileCenter(x, y);
        chessPieces[x, y].transform.Find(Shared.TileDetectorName).transform.position = GetTileCenter(x, y);
        chessPieces[x, y].Chessboard = this;
        chessPieces[x, y].SavePosition();
        chessPieces[x, y].SaveOrientation();
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Operations
    public void PieceWasPickedUp(int x, int y)
    {
        tiles[x, y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Selected)];
        availableMoves = chessPieces[x, y].CalculateAvailablePositions();
        foreach (var move in availableMoves)
        {
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Available)];
            tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = true;
        }
        Task.Run(() => DisablePickUpOnOtherPieces(chessPieces[x, y]));
    }

    public void PieceWasDropped(int currentX, int currentY, Tile newTile)
    {
        tiles[currentX, currentY].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];
        foreach (var move in availableMoves)
        {
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Default)];
            tiles[move.x, move.y].GetComponent<Tile>().IsAvailableTile = false;
        }
        availableMoves = null;

        if (newTile != null)
        {
            Vector2Int newPosition = newTile.Position;
            ChessPiece chessPiece = chessPieces[currentX, currentY];

            chessPiece.transform.position = GetTileCenter(newPosition.x, newPosition.y);
            chessPieces[newPosition.x, newPosition.y] = chessPiece;
            chessPieces[currentX, currentY] = null;
            chessPiece.currentX = newPosition.x;
            chessPiece.currentY = newPosition.y;
            
            chessPiece.SavePosition();
            chessPiece.IsMoved = true;
        }

        Task.Run(() => EnablePickUpOnPieces());
    }

    public void DisablePickUpOnOtherPieces(ChessPiece pickedPiece)
    {
        foreach (var piece in chessPieces)
        {
            if (piece != null && !piece.Equals(pickedPiece))
            {
                piece.transform.parent.GetComponent<MeshCollider>().enabled = false;
            }
        }
    }

    public void EnablePickUpOnPieces()
    {
        foreach (var piece in chessPieces)
        {
            if (piece != null) 
            { 
                piece.transform.parent.GetComponent<MeshCollider>().enabled = true; 
            }
        }
    }

    public void HandleTileTrigger(Vector2Int possition, bool enterTrigger)
    {
        if (enterTrigger)
        {
            tiles[possition.x, possition.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.MoveTo)];
        }
        else
        {
            tiles[possition.x, possition.y].GetComponent<MeshRenderer>().material = tilesMaterials[((int)Shared.TileType.Available)];
        }
    }

    public bool IsSpaceOccupied(Vector2Int position)
    {
        ChessPiece chessPiece;
        try
        {
            chessPiece = chessPieces[position.x, position.y];
        }
        catch
        {
            return true;
        }

        return chessPiece != null;
    }

}
